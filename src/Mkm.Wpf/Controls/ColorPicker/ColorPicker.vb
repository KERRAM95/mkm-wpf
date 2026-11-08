Imports System.Globalization
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le choix de couleur de mkm (<c>ColorPicker</c>) : un carré de la couleur qui, cliqué, ouvre un panneau sombre, le
    ''' carré des nuances (saturation à l'horizontale, luminosité à la verticale) et la bande des teintes.
    ''' <code>
    ''' &lt;mkm:ColorPicker Value="3b82f6" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Value"/> est la couleur en hexadécimal, sans dièse (« ff0000 »), comme mkm ; <see cref="Color"/> la même,
    ''' en <see cref="Media.Color"/>. <see cref="Inline"/> montre le panneau à la place du carré.
    ''' </remarks>
    <TemplatePart(Name:="PART_Preview", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Popup", Type:=GetType(Popup))>
    <TemplatePart(Name:="PART_PanelHost", Type:=GetType(ContentPresenter))>
    <TemplatePart(Name:="PART_InlineHost", Type:=GetType(ContentPresenter))>
    Public Class ColorPicker
        Inherits Control

        ''' <summary>La couleur, en hexadécimal sans dièse : « ff0000 » par défaut. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(String), GetType(ColorPicker),
            New FrameworkPropertyMetadata("ff0000", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnValueChanged))

        ''' <summary>La couleur. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly ColorProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Color), GetType(Color), GetType(ColorPicker),
            New FrameworkPropertyMetadata(Colors.Red, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnColorChanged))

        ''' <summary>Vrai pour le panneau montré en place, sans le carré.</summary>
        Public Shared ReadOnly InlineProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Inline), GetType(Boolean), GetType(ColorPicker), New FrameworkPropertyMetadata(False, AddressOf OnInlineChanged))

        ''' <summary>Vrai quand le panneau est ouvert. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsDropDownOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsDropDownOpen), GetType(Boolean), GetType(ColorPicker),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsDropDownOpenChanged))

        Private Shared ReadOnly ColorBrushPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(ColorBrush), GetType(Brush), GetType(ColorPicker), New FrameworkPropertyMetadata(Brushes.Red))
        ''' <summary>Le pinceau de la couleur, celui du carré.</summary>
        Public Shared ReadOnly ColorBrushProperty As DependencyProperty = ColorBrushPropertyKey.DependencyProperty

        ''' <summary>La couleur a changé.</summary>
        Public Shared ReadOnly ColorChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(ColorChanged), RoutingStrategy.Bubble, GetType(RoutedPropertyChangedEventHandler(Of Color)), GetType(ColorPicker))

        Private ReadOnly _panneau As PanneauDeCouleur
        Private _apercu As ButtonBase
        Private _popup As Popup
        Private _hotePopup As ContentPresenter
        Private _hoteEnPlace As ContentPresenter
        Private _synchro As Boolean
        Private _fermeSurLApercu As Date

        ' La couleur en teinte (0 à 360), saturation et luminosité (0 à 1) : la teinte se garde quand la couleur est grise.
        Friend Teinte As Double
        Friend Saturation As Double = 1
        Friend Luminosite As Double = 1

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ColorPicker), New FrameworkPropertyMetadata(GetType(ColorPicker)))
            FocusableProperty.OverrideMetadata(GetType(ColorPicker), New FrameworkPropertyMetadata(False))
        End Sub

        Public Sub New()
            _panneau = New PanneauDeCouleur(Me)
            AddLogicalChild(_panneau)
            AddHandler Localizer.Current.LanguageChanged, Sub() Nommer()
        End Sub

        ''' <summary>La couleur, en hexadécimal sans dièse.</summary>
        Public Property Value As String
            Get
                Return CStr(GetValue(ValueProperty))
            End Get
            Set(value As String)
                SetValue(ValueProperty, value)
            End Set
        End Property

        ''' <summary>La couleur.</summary>
        Public Property Color As Color
            Get
                Return DirectCast(GetValue(ColorProperty), Color)
            End Get
            Set(value As Color)
                SetValue(ColorProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour le panneau montré en place.</summary>
        Public Property Inline As Boolean
            Get
                Return CBool(GetValue(InlineProperty))
            End Get
            Set(value As Boolean)
                SetValue(InlineProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le panneau est ouvert.</summary>
        Public Property IsDropDownOpen As Boolean
            Get
                Return CBool(GetValue(IsDropDownOpenProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsDropDownOpenProperty, value)
            End Set
        End Property

        ''' <summary>Le pinceau de la couleur.</summary>
        Public ReadOnly Property ColorBrush As Brush
            Get
                Return DirectCast(GetValue(ColorBrushProperty), Brush)
            End Get
        End Property

        ''' <summary>La couleur a changé.</summary>
        Public Custom Event ColorChanged As RoutedPropertyChangedEventHandler(Of Color)
            AddHandler(value As RoutedPropertyChangedEventHandler(Of Color))
                [AddHandler](ColorChangedEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedPropertyChangedEventHandler(Of Color))
                [RemoveHandler](ColorChangedEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedPropertyChangedEventArgs(Of Color))
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Le panneau, enfant logique du choix.</summary>
        Protected Overrides ReadOnly Property LogicalChildren As IEnumerator
            Get
                Return New Object() {_panneau}.GetEnumerator()
            End Get
        End Property

        ''' <summary>Le panneau, pour les tests.</summary>
        Friend ReadOnly Property Panneau As PanneauDeCouleur
            Get
                Return _panneau
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _apercu IsNot Nothing Then RemoveHandler _apercu.Click, AddressOf Apercu_Click
            If _popup IsNot Nothing Then RemoveHandler _popup.Closed, AddressOf Popup_Closed
            If _hotePopup IsNot Nothing Then _hotePopup.Content = Nothing
            If _hoteEnPlace IsNot Nothing Then _hoteEnPlace.Content = Nothing
            MyBase.OnApplyTemplate()
            _apercu = TryCast(GetTemplateChild("PART_Preview"), ButtonBase)
            _popup = TryCast(GetTemplateChild("PART_Popup"), Popup)
            _hotePopup = TryCast(GetTemplateChild("PART_PanelHost"), ContentPresenter)
            _hoteEnPlace = TryCast(GetTemplateChild("PART_InlineHost"), ContentPresenter)
            If _apercu IsNot Nothing Then AddHandler _apercu.Click, AddressOf Apercu_Click
            If _popup IsNot Nothing Then AddHandler _popup.Closed, AddressOf Popup_Closed
            Placer()
            Nommer()
        End Sub

        ' Le panneau, dans la fenêtre flottante ou en place.
        Private Sub Placer()
            If _hotePopup IsNot Nothing Then _hotePopup.Content = Nothing
            If _hoteEnPlace IsNot Nothing Then _hoteEnPlace.Content = Nothing
            Dim hote = If(Inline, _hoteEnPlace, _hotePopup)
            If hote IsNot Nothing Then hote.Content = _panneau
        End Sub

        Private Shared Sub OnInlineChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim choix = DirectCast(d, ColorPicker)
            choix.SetCurrentValue(IsDropDownOpenProperty, False)
            choix.Placer()
        End Sub

        Private Shared Sub OnValueChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim choix = DirectCast(d, ColorPicker)
            If choix._synchro Then Return
            Dim couleur As Color
            If Not EssayerLeHexa(CStr(e.NewValue), couleur) Then Return
            choix.SetCurrentValue(ColorProperty, couleur)
        End Sub

        Private Shared Sub OnColorChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim choix = DirectCast(d, ColorPicker)
            Dim couleur = DirectCast(e.NewValue, Color)
            choix._synchro = True
            Try
                choix.SetCurrentValue(ValueProperty, VersHexa(couleur))
            Finally
                choix._synchro = False
            End Try
            Dim pinceau As New SolidColorBrush(couleur)
            pinceau.Freeze()
            choix.SetValue(ColorBrushPropertyKey, pinceau)
            ' La teinte, la saturation et la luminosité suivent, sauf quand c'est le panneau qui a donné la couleur.
            If Not choix._depuisLePanneau Then
                Dim t, s, l As Double
                VersTsl(couleur, t, s, l)
                If s > 0 AndAlso l > 0 Then choix.Teinte = t
                choix.Saturation = s
                choix.Luminosite = l
            End If
            choix._panneau.Montrer()
            choix.Nommer()
            choix.RaiseEvent(New RoutedPropertyChangedEventArgs(Of Color)(DirectCast(e.OldValue, Color), couleur, ColorChangedEvent))
        End Sub

        Private _depuisLePanneau As Boolean

        ''' <summary>Pose la couleur depuis la teinte, la saturation et la luminosité du panneau.</summary>
        Friend Sub Choisir(teinte As Double, saturation As Double, luminosite As Double)
            Me.Teinte = Math.Max(0, Math.Min(360, teinte))
            Me.Saturation = Math.Max(0, Math.Min(1, saturation))
            Me.Luminosite = Math.Max(0, Math.Min(1, luminosite))
            _depuisLePanneau = True
            Try
                SetCurrentValue(ColorProperty, VersCouleur(Me.Teinte, Me.Saturation, Me.Luminosite))
            Finally
                _depuisLePanneau = False
            End Try
            _panneau.Montrer()
        End Sub

        ' ---- Les conversions ----

        ''' <summary>« rrggbb », en minuscules, sans dièse.</summary>
        Friend Shared Function VersHexa(couleur As Color) As String
            Return String.Format(CultureInfo.InvariantCulture, "{0:x2}{1:x2}{2:x2}", couleur.R, couleur.G, couleur.B)
        End Function

        ''' <summary>Lit « rrggbb », « #rrggbb » ou « rgb » ; faux pour autre chose.</summary>
        Friend Shared Function EssayerLeHexa(texte As String, ByRef couleur As Color) As Boolean
            Dim hexa = If(texte, String.Empty).Trim().TrimStart("#"c)
            If hexa.Length = 3 Then hexa = String.Concat(hexa.Select(Function(c) New String(c, 2)))
            Dim nombre As Integer
            If hexa.Length <> 6 OrElse Not Integer.TryParse(hexa, NumberStyles.HexNumber, CultureInfo.InvariantCulture, nombre) Then Return False
            couleur = Color.FromRgb(CByte((nombre >> 16) And &HFF), CByte((nombre >> 8) And &HFF), CByte(nombre And &HFF))
            Return True
        End Function

        ''' <summary>La couleur d'une teinte (0 à 360), saturation et luminosité (0 à 1).</summary>
        Friend Shared Function VersCouleur(teinte As Double, saturation As Double, luminosite As Double) As Color
            Dim h = (teinte Mod 360) / 60
            Dim i = CInt(Math.Floor(h)) Mod 6
            Dim f = h - Math.Floor(h)
            Dim v = luminosite * 255
            Dim p = v * (1 - saturation)
            Dim q = v * (1 - saturation * f)
            Dim t = v * (1 - saturation * (1 - f))
            Dim r, g, b As Double
            Select Case i
                Case 0 : r = v : g = t : b = p
                Case 1 : r = q : g = v : b = p
                Case 2 : r = p : g = v : b = t
                Case 3 : r = p : g = q : b = v
                Case 4 : r = t : g = p : b = v
                Case Else : r = v : g = p : b = q
            End Select
            Return Color.FromRgb(CByte(Math.Round(r)), CByte(Math.Round(g)), CByte(Math.Round(b)))
        End Function

        ''' <summary>La teinte, la saturation et la luminosité d'une couleur.</summary>
        Friend Shared Sub VersTsl(couleur As Color, ByRef teinte As Double, ByRef saturation As Double, ByRef luminosite As Double)
            Dim r = couleur.R / 255.0, g = couleur.G / 255.0, b = couleur.B / 255.0
            Dim plus = Math.Max(r, Math.Max(g, b))
            Dim moins = Math.Min(r, Math.Min(g, b))
            Dim ecart = plus - moins
            luminosite = plus
            saturation = If(plus = 0, 0, ecart / plus)
            If ecart = 0 Then
                teinte = 0
            ElseIf plus = r Then
                teinte = 60 * (((g - b) / ecart) Mod 6)
            ElseIf plus = g Then
                teinte = 60 * ((b - r) / ecart + 2)
            Else
                teinte = 60 * ((r - g) / ecart + 4)
            End If
            If teinte < 0 Then teinte += 360
        End Sub

        ' ---- Ouvrir, fermer ----

        Private Sub Apercu_Click(sender As Object, e As RoutedEventArgs)
            If Not IsDropDownOpen AndAlso (Date.UtcNow - _fermeSurLApercu).TotalMilliseconds < 400 Then Return
            SetCurrentValue(IsDropDownOpenProperty, Not IsDropDownOpen)
        End Sub

        Private Sub Popup_Closed(sender As Object, e As EventArgs)
            If _apercu IsNot Nothing AndAlso _apercu.IsMouseOver AndAlso Mouse.LeftButton = MouseButtonState.Pressed Then _fermeSurLApercu = Date.UtcNow
            SetCurrentValue(IsDropDownOpenProperty, False)
        End Sub

        Private Shared Sub OnIsDropDownOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim choix = DirectCast(d, ColorPicker)
            If CBool(e.NewValue) Then
                ' Ouvert, le clavier passe au carré des nuances.
                choix.Dispatcher.BeginInvoke(Threading.DispatcherPriority.Loaded, New System.Action(Sub() choix._panneau.Prendre()))
            ElseIf choix._panneau.IsKeyboardFocusWithin Then
                choix._apercu?.Focus()
            End If
        End Sub

        ''' <summary>Échap referme le panneau ; Entrée aussi, dans le panneau.</summary>
        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            MyBase.OnPreviewKeyDown(e)
            If Not IsDropDownOpen Then Return
            If e.Key = Key.Escape OrElse (e.Key = Key.Enter AndAlso _panneau.IsKeyboardFocusWithin) Then
                SetCurrentValue(IsDropDownOpenProperty, False)
                _apercu?.Focus()
                e.Handled = True
            End If
        End Sub

        ' Le carré se nomme « Choisir une couleur » (ou par AutomationProperties.Name), et dit la couleur.
        Private Sub Nommer()
            If _apercu Is Nothing Then Return
            Dim nom = AutomationProperties.GetName(Me)
            AutomationProperties.SetName(_apercu, If(String.IsNullOrEmpty(nom), Localizer.Current.T("aria.chooseColor"), nom))
            AutomationProperties.SetHelpText(_apercu, "#" & Value)
            AutomationProperties.SetItemStatus(_apercu, "#" & Value)
        End Sub

    End Class

    ''' <summary>
    ''' Le panneau d'un <see cref="ColorPicker"/> : le carré des nuances et la bande des teintes, leurs poignées ; la
    ''' souris les tire, les flèches les déplacent.
    ''' </summary>
    <TemplatePart(Name:="PART_Selector", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_Hue", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_ColorHandle", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_HueHandle", Type:=GetType(FrameworkElement))>
    Friend NotInheritable Class PanneauDeCouleur
        Inherits Control

        Private Shared ReadOnly HueBrushPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HueBrush), GetType(Brush), GetType(PanneauDeCouleur), New FrameworkPropertyMetadata(Brushes.Red))
        Public Shared ReadOnly HueBrushProperty As DependencyProperty = HueBrushPropertyKey.DependencyProperty

        Private Const Cote As Double = 150

        Private ReadOnly _choix As ColorPicker
        Private _carre As FrameworkElement
        Private _bande As FrameworkElement
        Private _poignee As FrameworkElement
        Private _poigneeDeTeinte As FrameworkElement
        Private _tire As FrameworkElement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(PanneauDeCouleur), New FrameworkPropertyMetadata(GetType(PanneauDeCouleur)))
            FocusableProperty.OverrideMetadata(GetType(PanneauDeCouleur), New FrameworkPropertyMetadata(False))
        End Sub

        Friend Sub New(choix As ColorPicker)
            _choix = choix
        End Sub

        Public ReadOnly Property HueBrush As Brush
            Get
                Return DirectCast(GetValue(HueBrushProperty), Brush)
            End Get
        End Property

        Public Overrides Sub OnApplyTemplate()
            For Each partie In {_carre, _bande}
                If partie Is Nothing Then Continue For
                RemoveHandler partie.MouseLeftButtonDown, AddressOf Partie_MouseLeftButtonDown
                RemoveHandler partie.MouseMove, AddressOf Partie_MouseMove
                RemoveHandler partie.MouseLeftButtonUp, AddressOf Partie_MouseLeftButtonUp
                RemoveHandler partie.PreviewKeyDown, AddressOf Partie_PreviewKeyDown
            Next
            MyBase.OnApplyTemplate()
            _carre = TryCast(GetTemplateChild("PART_Selector"), FrameworkElement)
            _bande = TryCast(GetTemplateChild("PART_Hue"), FrameworkElement)
            _poignee = TryCast(GetTemplateChild("PART_ColorHandle"), FrameworkElement)
            _poigneeDeTeinte = TryCast(GetTemplateChild("PART_HueHandle"), FrameworkElement)
            For Each partie In {_carre, _bande}
                If partie Is Nothing Then Continue For
                AddHandler partie.MouseLeftButtonDown, AddressOf Partie_MouseLeftButtonDown
                AddHandler partie.MouseMove, AddressOf Partie_MouseMove
                AddHandler partie.MouseLeftButtonUp, AddressOf Partie_MouseLeftButtonUp
                AddHandler partie.PreviewKeyDown, AddressOf Partie_PreviewKeyDown
            Next
            Nommer()
            Montrer()
        End Sub

        Private Sub Nommer()
            If _carre IsNot Nothing Then AutomationProperties.SetName(_carre, Localizer.Current.T("aria.colorSelector"))
            If _bande IsNot Nothing Then AutomationProperties.SetName(_bande, Localizer.Current.T("aria.hue"))
        End Sub

        ''' <summary>Le clavier, au carré des nuances.</summary>
        Friend Sub Prendre()
            _carre?.Focus()
        End Sub

        ''' <summary>Les poignées à la couleur, le carré à la teinte.</summary>
        Friend Sub Montrer()
            Dim teinte As New SolidColorBrush(ColorPicker.VersCouleur(_choix.Teinte, 1, 1))
            teinte.Freeze()
            SetValue(HueBrushPropertyKey, teinte)
            If _poignee IsNot Nothing Then
                Canvas.SetLeft(_poignee, _choix.Saturation * Cote)
                Canvas.SetTop(_poignee, (1 - _choix.Luminosite) * Cote)
            End If
            If _poigneeDeTeinte IsNot Nothing Then Canvas.SetTop(_poigneeDeTeinte, Cote - _choix.Teinte * Cote / 360)
        End Sub

        Private Sub Partie_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            _tire = DirectCast(sender, FrameworkElement)
            _tire.Focus()
            _tire.CaptureMouse()
            Tirer(e.GetPosition(_tire))
            e.Handled = True
        End Sub

        Private Sub Partie_MouseMove(sender As Object, e As MouseEventArgs)
            If _tire IsNot Nothing AndAlso _tire.IsMouseCaptured Then Tirer(e.GetPosition(_tire))
        End Sub

        Private Sub Partie_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
            _tire?.ReleaseMouseCapture()
            _tire = Nothing
        End Sub

        ''' <summary>Un point du carré (saturation, luminosité) ou de la bande (teinte), en pixels de la partie.</summary>
        Friend Sub Tirer(point As Point, Optional bande As Boolean? = Nothing)
            Dim surLaBande = If(bande, _tire Is _bande AndAlso _tire IsNot Nothing)
            Dim x = Math.Max(0, Math.Min(Cote, point.X))
            Dim y = Math.Max(0, Math.Min(Cote, point.Y))
            If surLaBande Then
                _choix.Choisir(360 * (Cote - y) / Cote, _choix.Saturation, _choix.Luminosite)
            Else
                _choix.Choisir(_choix.Teinte, x / Cote, (Cote - y) / Cote)
            End If
        End Sub

        ' Les flèches déplacent la poignée d'un pixel (Maj : de dix).
        Private Sub Partie_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            Dim pas = If(Keyboard.Modifiers = ModifierKeys.Shift, 10.0, 1.0) / Cote
            If sender Is _bande Then
                Select Case e.Key
                    Case Key.Up, Key.Right : _choix.Choisir(_choix.Teinte + pas * 360, _choix.Saturation, _choix.Luminosite)
                    Case Key.Down, Key.Left : _choix.Choisir(_choix.Teinte - pas * 360, _choix.Saturation, _choix.Luminosite)
                    Case Else : Return
                End Select
            Else
                Select Case e.Key
                    Case Key.Right : _choix.Choisir(_choix.Teinte, _choix.Saturation + pas, _choix.Luminosite)
                    Case Key.Left : _choix.Choisir(_choix.Teinte, _choix.Saturation - pas, _choix.Luminosite)
                    Case Key.Up : _choix.Choisir(_choix.Teinte, _choix.Saturation, _choix.Luminosite + pas)
                    Case Key.Down : _choix.Choisir(_choix.Teinte, _choix.Saturation, _choix.Luminosite - pas)
                    Case Else : Return
                End Select
            End If
            e.Handled = True
        End Sub

    End Class

    ''' <summary>
    ''' Une zone du panneau d'un <see cref="ColorPicker"/> (les nuances, les teintes) : elle prend le clavier, et se
    ''' présente aux lecteurs d'écran, nommée.
    ''' </summary>
    Friend NotInheritable Class ZoneDeCouleur
        Inherits ContentControl

        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Custom)
        End Function

    End Class

End Namespace
