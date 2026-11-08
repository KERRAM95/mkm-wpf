Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Interop
Imports System.Windows.Media
Imports System.Windows.Threading
Imports Microsoft.Win32
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf

    Partial Public Class MkmResources

        ''' <summary>Le plus petit zoom : 80 %.</summary>
        Public Const MinZoom As Double = 0.8

        ''' <summary>Le plus grand zoom : 200 %.</summary>
        Public Const MaxZoom As Double = 2.0

        ''' <summary>Les paliers de <see cref="ZoomIn"/> et <see cref="ZoomOut"/> : 80, 90, 100, 110, 125, 150, 175 et 200 %.</summary>
        Public Shared ReadOnly Property ZoomLevels As IReadOnlyList(Of Double) = New Double() {0.8, 0.9, 1.0, 1.1, 1.25, 1.5, 1.75, 2.0}

        ''' <summary>Vrai sur un élément qui garde Ctrl+molette pour lui (une visionneuse qui zoome son dessin).</summary>
        Public Shared ReadOnly HandlesZoomWheelProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "HandlesZoomWheel", GetType(Boolean), GetType(MkmResources), New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits))

        Private _zoom As Double = 1.0
        Private _zoomShortcuts As Boolean = True
        Private _zoomIndicator As Boolean = True

        ''' <summary>
        ''' Le zoom de toute l'interface : 1 (100 %) par défaut, de <see cref="MinZoom"/> à <see cref="MaxZoom"/>. Texte,
        ''' icônes, bordures et marges grandissent ensemble, dans chaque fenêtre de l'application, ses dialogues et ses
        ''' fenêtres flottantes (listes, menus, info-bulles) ; il se change pendant que l'application tourne.
        ''' </summary>
        Public Property Zoom As Double
            Get
                Return _zoom
            End Get
            Set(value As Double)
                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Throw New ArgumentOutOfRangeException(NameOf(value))
                Dim borne = Math.Round(Math.Max(MinZoom, Math.Min(MaxZoom, value)), 2)
                If borne = _zoom Then Return
                _zoom = borne
                Agrandissement.Appliquer(_zoom)
                RaiseEvent ZoomChanged(Me, EventArgs.Empty)
            End Set
        End Property

        ''' <summary>
        ''' Vrai (par défaut) pour les raccourcis du zoom dans chaque fenêtre : Ctrl + et Ctrl − (le palier suivant ou
        ''' précédent), Ctrl + 0 (100 %), Ctrl+molette. Un élément qui garde Ctrl+molette pour lui pose
        ''' <c>MkmResources.HandlesZoomWheel="True"</c>.
        ''' </summary>
        Public Property ZoomShortcuts As Boolean
            Get
                Return _zoomShortcuts
            End Get
            Set(value As Boolean)
                _zoomShortcuts = value
            End Set
        End Property

        ''' <summary>Vrai (par défaut) pour une bulle « 125 % » au milieu de la fenêtre, une seconde, quand un raccourci change le zoom.</summary>
        Public Property ZoomIndicator As Boolean
            Get
                Return _zoomIndicator
            End Get
            Set(value As Boolean)
                _zoomIndicator = value
            End Set
        End Property

        ''' <summary>Quand le zoom change.</summary>
        Public Event ZoomChanged As EventHandler

        ''' <summary>Le palier au-dessus du zoom actuel (jusqu'à 200 %).</summary>
        Public Sub ZoomIn()
            Zoom = ZoomLevels.Where(Function(p) p > _zoom + 0.001).DefaultIfEmpty(MaxZoom).First()
        End Sub

        ''' <summary>Le palier au-dessous du zoom actuel (jusqu'à 80 %).</summary>
        Public Sub ZoomOut()
            Zoom = ZoomLevels.Where(Function(p) p < _zoom - 0.001).DefaultIfEmpty(MinZoom).Last()
        End Sub

        ''' <summary>Revient à 100 %.</summary>
        Public Sub ResetZoom()
            Zoom = 1.0
        End Sub

        ''' <summary>
        ''' La taille du texte réglée dans Windows (Paramètres › Accessibilité › Taille du texte), de 1 à 2,25 ; 1 sans
        ''' réglage. Une application en part au premier lancement, quand l'utilisateur n'a pas encore choisi son zoom :
        ''' <c>MkmResources.Current.Zoom = MkmResources.WindowsTextScale</c>.
        ''' </summary>
        Public Shared ReadOnly Property WindowsTextScale As Double
            Get
                Try
                    Using cle = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Accessibility")
                        Dim valeur = cle?.GetValue("TextScaleFactor")
                        If TypeOf valeur Is Integer Then
                            Dim taille = CInt(valeur)
                            If taille >= 100 AndAlso taille <= 225 Then Return taille / 100.0
                        End If
                    End Using
                Catch ex As Exception When TypeOf ex Is Security.SecurityException OrElse TypeOf ex Is UnauthorizedAccessException
                    ' Le registre fermé : le réglage par défaut.
                End Try
                Return 1.0
            End Get
        End Property

        ''' <summary>Vrai si l'élément garde Ctrl+molette pour lui.</summary>
        Public Shared Function GetHandlesZoomWheel(element As DependencyObject) As Boolean
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return CBool(element.GetValue(HandlesZoomWheelProperty))
        End Function

        ''' <summary>Pose sur un élément (et ses enfants) qu'il garde Ctrl+molette pour lui.</summary>
        Public Shared Sub SetHandlesZoomWheel(element As DependencyObject, value As Boolean)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(HandlesZoomWheelProperty, value)
        End Sub

    End Class

    ''' <summary>
    ''' Le zoom appliqué aux fenêtres : une mise à l'échelle de leur racine (LayoutTransform), qui met en page dans la
    ''' place qui reste, comme le zoom d'un navigateur ; les fenêtres flottantes (listes, menus, info-bulles) suivent
    ''' l'échelle de leur élément. Au-dessus ou au-dessous de 100 %, le texte se dessine en mode « Ideal » : le mode
    ''' « Display » de la bibliothèque, le plus net à 100 %, devient flou agrandi.
    ''' </summary>
    Friend NotInheritable Class Agrandissement

        Private Shared _enregistre As Boolean
        Private Shared ReadOnly _fenetres As New List(Of WeakReference(Of Window))()
        Private Shared _bulle As Popup
        Private Shared _minuterie As DispatcherTimer

        ' Marque la racine que le zoom a mise à l'échelle : il ne retire que sa propre échelle.
        Private Shared ReadOnly ZoomeeProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Zoomee", GetType(Boolean), GetType(Agrandissement), New FrameworkPropertyMetadata(False))

        Private Sub New()
        End Sub

        Private Shared ReadOnly Property Zoom As Double
            Get
                Return If(MkmResources.Current Is Nothing, 1.0, MkmResources.Current.Zoom)
            End Get
        End Property

        ' Une fois par application (MkmResources) : les fenêtres qui s'ouvrent, leurs raccourcis, les menus et info-bulles.
        Friend Shared Sub Enregistrer()
            If _enregistre Then Return
            _enregistre = True
            EventManager.RegisterClassHandler(GetType(Window), FrameworkElement.LoadedEvent, New RoutedEventHandler(AddressOf FenetreChargee))
            EventManager.RegisterClassHandler(GetType(Window), Keyboard.PreviewKeyDownEvent, New KeyEventHandler(AddressOf Touche))
            EventManager.RegisterClassHandler(GetType(Window), Mouse.PreviewMouseWheelEvent, New MouseWheelEventHandler(AddressOf Molette))
            EventManager.RegisterClassHandler(GetType(ContextMenu), ContextMenu.OpenedEvent, New RoutedEventHandler(AddressOf FlottanteOuverte))
            EventManager.RegisterClassHandler(GetType(ToolTip), ToolTip.OpenedEvent, New RoutedEventHandler(AddressOf FlottanteOuverte))
        End Sub

        ' Le zoom changé : chaque fenêtre ouverte le prend.
        Friend Shared Sub Appliquer(facteur As Double)
            For Each fenetre In Fenetres()
                Agrandir(RacineDe(fenetre), facteur)
            Next
        End Sub

        Private Shared Function Fenetres() As List(Of Window)
            Dim vivantes As New List(Of Window)()
            _fenetres.RemoveAll(Function(r)
                                    Dim w As Window = Nothing
                                    If Not r.TryGetTarget(w) Then Return True
                                    vivantes.Add(w)
                                    Return False
                                End Function)
            Return vivantes
        End Function

        ' La racine du gabarit de la fenêtre : la barre de titre d'un dialogue de mkm grandit avec son contenu.
        Friend Shared Function RacineDe(fenetre As Window) As FrameworkElement
            If fenetre Is Nothing OrElse VisualTreeHelper.GetChildrenCount(fenetre) = 0 Then Return Nothing
            Return TryCast(VisualTreeHelper.GetChild(fenetre, 0), FrameworkElement)
        End Function

        ''' <summary>Met un élément à l'échelle du zoom, son texte dans le mode qui reste net ; à 1, le rend tel quel.</summary>
        Friend Shared Sub Agrandir(element As FrameworkElement, facteur As Double)
            If element Is Nothing Then Return
            If Math.Abs(facteur - 1.0) < 0.001 Then
                If CBool(element.GetValue(ZoomeeProperty)) Then
                    element.ClearValue(FrameworkElement.LayoutTransformProperty)
                    element.ClearValue(ZoomeeProperty)
                End If
                element.ClearValue(TextOptions.TextFormattingModeProperty)
                Return
            End If
            Dim echelle As New ScaleTransform(facteur, facteur)
            echelle.Freeze()
            element.LayoutTransform = echelle
            element.SetValue(ZoomeeProperty, True)
            element.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Ideal)
        End Sub

        Private Shared Sub FenetreChargee(sender As Object, e As RoutedEventArgs)
            Dim fenetre = DirectCast(sender, Window)
            If Not Fenetres().Contains(fenetre) Then _fenetres.Add(New WeakReference(Of Window)(fenetre))
            If Math.Abs(Zoom - 1.0) >= 0.001 Then Agrandir(RacineDe(fenetre), Zoom)
        End Sub

        ' Un menu ou une info-bulle sans parent n'hérite pas du mode du texte de la fenêtre : il le prend à l'ouverture.
        Private Shared Sub FlottanteOuverte(sender As Object, e As RoutedEventArgs)
            Dim flottante = TryCast(sender, FrameworkElement)
            If flottante Is Nothing Then Return
            If Math.Abs(Zoom - 1.0) < 0.001 Then
                flottante.ClearValue(TextOptions.TextFormattingModeProperty)
            Else
                flottante.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Ideal)
            End If
        End Sub

        ''' <summary>
        ''' Ce que demande une touche avec Ctrl : +1 (agrandir), -1 (réduire), 0 (100 %), Nothing (rien). <paramref name="caractere"/>
        ''' est le caractère de la touche dans la disposition du clavier : « - » est sur la touche 6 d'un clavier AZERTY.
        ''' </summary>
        Friend Shared Function ActionDuClavier(touche As Key, caractere As Char) As Integer?
            Select Case touche
                Case Key.Add : Return 1
                Case Key.Subtract : Return -1
                Case Key.D0, Key.NumPad0 : Return 0
            End Select
            Select Case caractere
                Case "+"c, "="c : Return 1
                Case "-"c : Return -1
            End Select
            Return Nothing
        End Function

        <DllImport("user32.dll")>
        Private Shared Function MapVirtualKey(code As UInteger, type As UInteger) As UInteger
        End Function

        Private Shared Function CaractereDe(touche As Key) As Char
            Try
                Dim code = MapVirtualKey(CUInt(KeyInterop.VirtualKeyFromKey(touche)), 2UI) And &H7FFFUI
                Return If(code = 0, ControlChars.NullChar, ChrW(CInt(code)))
            Catch ex As DllNotFoundException
                Return ControlChars.NullChar
            End Try
        End Function

        Private Shared Sub Touche(sender As Object, e As KeyEventArgs)
            Dim reglages = MkmResources.Current
            If e.Handled OrElse reglages Is Nothing OrElse Not reglages.ZoomShortcuts Then Return
            If (Keyboard.Modifiers And ModifierKeys.Control) = 0 OrElse (Keyboard.Modifiers And ModifierKeys.Alt) <> 0 Then Return
            Dim action = ActionDuClavier(e.Key, CaractereDe(e.Key))
            If Not action.HasValue Then Return
            Changer(DirectCast(sender, Window), action.Value)
            e.Handled = True
        End Sub

        Private Shared Sub Molette(sender As Object, e As MouseWheelEventArgs)
            Dim reglages = MkmResources.Current
            If e.Handled OrElse reglages Is Nothing OrElse Not reglages.ZoomShortcuts Then Return
            If Keyboard.Modifiers <> ModifierKeys.Control OrElse e.Delta = 0 Then Return
            Dim source = TryCast(e.OriginalSource, DependencyObject)
            If source IsNot Nothing AndAlso MkmResources.GetHandlesZoomWheel(source) Then Return
            Changer(DirectCast(sender, Window), If(e.Delta > 0, 1, -1))
            e.Handled = True
        End Sub

        Private Shared Sub Changer(fenetre As Window, action As Integer)
            Dim reglages = MkmResources.Current
            Select Case action
                Case 1 : reglages.ZoomIn()
                Case -1 : reglages.ZoomOut()
                Case Else : reglages.ResetZoom()
            End Select
            If reglages.ZoomIndicator Then Montrer(fenetre, reglages.Zoom)
        End Sub

        ' La bulle « 125 % », au milieu de la fenêtre, une seconde ; elle ne prend ni le clavier ni la souris.
        Private Shared Sub Montrer(fenetre As Window, facteur As Double)
            Dim racine = RacineDe(fenetre)
            If racine Is Nothing Then Return
            If _bulle Is Nothing Then
                Dim texte As New TextBlock()
                Dim cadre As New Border With {.Child = texte}
                cadre.SetResourceReference(Border.BackgroundProperty, "mkm.tooltip.background")
                cadre.SetResourceReference(TextBlock.ForegroundProperty, "mkm.tooltip.color")
                cadre.SetResourceReference(Border.PaddingProperty, "mkm.tooltip.padding")
                cadre.SetResourceReference(Border.CornerRadiusProperty, "mkm.tooltip.border.radius")
                cadre.SetResourceReference(TextBlock.FontFamilyProperty, "mkm.font.family")
                cadre.SetResourceReference(TextBlock.FontSizeProperty, "mkm.font.size")
                _bulle = New Popup With {.Child = cadre, .Placement = PlacementMode.Center, .AllowsTransparency = True,
                                         .IsHitTestVisible = False, .Focusable = False, .StaysOpen = True}
                Automation.AutomationProperties.SetLiveSetting(texte, Automation.AutomationLiveSetting.Polite)
                _minuterie = New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(1.2)}
                AddHandler _minuterie.Tick, Sub()
                                                _minuterie.Stop()
                                                _bulle.IsOpen = False
                                            End Sub
            End If
            Dim libelle = facteur.ToString("P0", Localizer.Current.Language)
            Dim pourcentage = DirectCast(DirectCast(_bulle.Child, Border).Child, TextBlock)
            pourcentage.Text = libelle
            _bulle.PlacementTarget = racine
            _bulle.IsOpen = True
            ' Narrateur annonce le nouveau zoom (une région « vivante »).
            Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(pourcentage)?.RaiseAutomationEvent(Automation.Peers.AutomationEvents.LiveRegionChanged)
            _minuterie.Stop()
            _minuterie.Start()
        End Sub

    End Class

End Namespace
