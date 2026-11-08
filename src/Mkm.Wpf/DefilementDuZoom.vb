Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media

Namespace Mkm.Wpf

    ''' <summary>
    ''' La zone qui défile d'une fenêtre zoomée, posée autour de la présentation de son contenu (Window.Content reste le
    ''' même) : le contenu y garde au moins <see cref="MkmResources.ZoomMinimumSize"/> (jamais plus que la fenêtre), sinon
    ''' la taille de la zone ; au-delà, des barres de défilement. À 100 % ou moins, elle s'efface : le contenu a la taille
    ''' de la fenêtre, comme sans elle. Seule une fenêtre de taille fixe (SizeToContent="Manual") en a une : une fenêtre à
    ''' la taille de son contenu (un dialogue) grandit avec le zoom.
    ''' </summary>
    Friend NotInheritable Class DefilementDuZoom
        Inherits ScrollViewer

        Private Shared ReadOnly ZoneProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Zone", GetType(DefilementDuZoom), GetType(DefilementDuZoom), New FrameworkPropertyMetadata(Nothing))

        Private ReadOnly _fenetre As Window
        Private ReadOnly _contenu As FrameworkElement

        Private Sub New(fenetre As Window, contenu As FrameworkElement)
            _fenetre = fenetre
            _contenu = contenu
            Focusable = False
            IsTabStop = False
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            Automation.AutomationProperties.SetAutomationId(Me, "DefilementDuZoom")
            AddHandler ScrollChanged, Sub(s, e)
                                          If e.ViewportWidthChange <> 0 OrElse e.ViewportHeightChange <> 0 Then Dimensionner()
                                      End Sub
            AddHandler fenetre.SizeChanged, Sub() Dimensionner()
        End Sub

        ''' <summary>La zone de la fenêtre ; créée autour de la présentation de son contenu si <paramref name="creer"/>.</summary>
        Friend Shared Function De(fenetre As Window, creer As Boolean) As DefilementDuZoom
            Dim zone = TryCast(fenetre.GetValue(ZoneProperty), DefilementDuZoom)
            If zone IsNot Nothing OrElse Not creer OrElse fenetre.SizeToContent <> SizeToContent.Manual OrElse fenetre.Content Is Nothing Then Return zone
            ' La présentation du contenu de la fenêtre, dans son gabarit, et son parent.
            Dim presentation = Descendants(fenetre).OfType(Of ContentPresenter)().
                FirstOrDefault(Function(p) p.TemplatedParent Is fenetre AndAlso p.Content Is fenetre.Content)
            If presentation Is Nothing Then Return Nothing
            Dim parent = VisualTreeHelper.GetParent(presentation)
            Dim decorateur = TryCast(parent, Decorator)
            Dim panneau = TryCast(parent, System.Windows.Controls.Panel)
            If decorateur Is Nothing AndAlso panneau Is Nothing Then Return Nothing
            zone = New DefilementDuZoom(fenetre, presentation)
            If decorateur IsNot Nothing Then
                decorateur.Child = Nothing
                zone.Content = presentation
                decorateur.Child = zone
            Else
                Dim place = panneau.Children.IndexOf(presentation)
                panneau.Children.RemoveAt(place)
                zone.Content = presentation
                panneau.Children.Insert(place, zone)
            End If
            fenetre.SetValue(ZoneProperty, zone)
            ' Les notifications déjà là restent dans la partie visible.
            For Each notifications In Descendants(presentation).OfType(Of Controls.Toast)()
                notifications.SuivreLaZone(zone)
            Next
            Return zone
        End Function

        Private Shared Iterator Function Descendants(racine As DependencyObject) As IEnumerable(Of DependencyObject)
            For i = 0 To VisualTreeHelper.GetChildrenCount(racine) - 1
                Dim enfant = VisualTreeHelper.GetChild(racine, i)
                Yield enfant
                For Each petit In Descendants(enfant)
                    Yield petit
                Next
            Next
        End Function

        ''' <summary>La taille du contenu : au moins la mise en page minimale, sinon la place visible.</summary>
        Friend Sub Dimensionner()
            Dim reglages = MkmResources.Current
            Dim facteur = If(reglages Is Nothing, 1.0, reglages.Zoom)
            If facteur <= 1.001 Then
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
                _contenu.ClearValue(WidthProperty)
                _contenu.ClearValue(HeightProperty)
                Return
            End If
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            ' La mise en page minimale, jamais plus grande que la fenêtre elle-même (en pixels à 100 %, sans son cadre).
            Dim minimum = reglages.ZoomMinimumSize
            Dim largeurMin = Math.Min(minimum.Width, Math.Max(0, _fenetre.ActualWidth - 16))
            Dim hauteurMin = Math.Min(minimum.Height, Math.Max(0, _fenetre.ActualHeight - 39))
            Dim largeur = If(ViewportWidth > 0, ViewportWidth, ActualWidth)
            Dim hauteur = If(ViewportHeight > 0, ViewportHeight, ActualHeight)
            _contenu.Width = Math.Max(largeurMin, largeur)
            _contenu.Height = Math.Max(hauteurMin, hauteur)
        End Sub

        ' Les flèches et Page haut/bas restent aux éléments de la fenêtre (la navigation au clavier) : la zone ne les prend pas.
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        End Sub

        ' La molette ne défile la zone que si elle a de quoi défiler ; Maj+molette, de côté.
        Protected Overrides Sub OnMouseWheel(e As MouseWheelEventArgs)
            If e.Handled Then Return
            If Keyboard.Modifiers = ModifierKeys.Shift AndAlso ScrollableWidth > 0 Then
                If e.Delta > 0 Then LineLeft() Else LineRight()
                e.Handled = True
            ElseIf ScrollableHeight > 0 Then
                MyBase.OnMouseWheel(e)
            End If
        End Sub

    End Class

End Namespace
