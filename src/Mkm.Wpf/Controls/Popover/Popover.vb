Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Threading

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La fenêtre flottante de mkm (Popover) : un cadre ombré, une flèche vers son élément, qui s'ouvre dessous (dessus
    ''' s'il n'y a pas la place) et se ferme d'un clic ailleurs ou d'Échap. Posé n'importe où dans la page, il n'y prend
    ''' pas de place ; l'application l'ouvre sur un élément.
    ''' <code>
    ''' &lt;mkm:Button x:Name="Partager" Content="Partager" Click="Partager_Click" /&gt;
    ''' &lt;mkm:Popover x:Name="Partage" Target="{Binding ElementName=Partager}"&gt;…&lt;/mkm:Popover&gt;
    ''' </code>
    ''' <code>Private Sub Partager_Click(sender As Object, e As RoutedEventArgs) : Partage.Toggle() : End Sub</code>
    ''' </summary>
    <TemplatePart(Name:="PART_Popup", Type:=GetType(Popup))>
    <TemplatePart(Name:="PART_Arrow", Type:=GetType(FrameworkElement))>
    Public Class Popover
        Inherits ContentControl

        ''' <summary>L'élément sous lequel il s'ouvre, et vers lequel pointe sa flèche.</summary>
        Public Shared ReadOnly TargetProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Target), GetType(UIElement), GetType(Popover), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai quand il est ouvert. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsOpen), GetType(Boolean), GetType(Popover),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsOpenChanged))

        Private Shared ReadOnly IsFlippedPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFlipped), GetType(Boolean), GetType(Popover), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand il s'est ouvert au-dessus de son élément, faute de place dessous : sa flèche en bas.</summary>
        Public Shared ReadOnly IsFlippedProperty As DependencyProperty = IsFlippedPropertyKey.DependencyProperty

        ''' <summary>Quand il s'ouvre.</summary>
        Public Shared ReadOnly OpenedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Opened), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(Popover))

        ''' <summary>Quand il se ferme.</summary>
        Public Shared ReadOnly ClosedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Closed), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(Popover))

        Private _popup As Popup
        Private _fleche As FrameworkElement
        Private _fenetre As Window
        ' Le moment où un clic sur l'élément l'a fermé : le Click de ce même clic ne doit pas le rouvrir.
        Private _fermeSurLaCible As Date = Date.MinValue

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Popover), New FrameworkPropertyMetadata(GetType(Popover)))
            FocusableProperty.OverrideMetadata(GetType(Popover), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>L'élément sous lequel il s'ouvre.</summary>
        Public Property Target As UIElement
            Get
                Return DirectCast(GetValue(TargetProperty), UIElement)
            End Get
            Set(value As UIElement)
                SetValue(TargetProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand il est ouvert.</summary>
        Public Property IsOpen As Boolean
            Get
                Return CBool(GetValue(IsOpenProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsOpenProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand il s'est ouvert au-dessus de son élément.</summary>
        Public ReadOnly Property IsFlipped As Boolean
            Get
                Return CBool(GetValue(IsFlippedProperty))
            End Get
        End Property

        ''' <summary>Quand il s'ouvre.</summary>
        Public Custom Event Opened As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                [AddHandler](OpenedEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                [RemoveHandler](OpenedEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Quand il se ferme.</summary>
        Public Custom Event Closed As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                [AddHandler](ClosedEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                [RemoveHandler](ClosedEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>L'ouvre sous son élément, ou sous <paramref name="cible"/>, qui devient son élément.</summary>
        Public Sub Show(Optional cible As UIElement = Nothing)
            If cible IsNot Nothing Then Target = cible
            SetCurrentValue(IsOpenProperty, True)
        End Sub

        ''' <summary>Le ferme.</summary>
        Public Sub Hide()
            SetCurrentValue(IsOpenProperty, False)
        End Sub

        ''' <summary>
        ''' L'ouvre s'il est fermé, le ferme s'il est ouvert : ce que fait le clic sur son élément. Le clic qui le ferme en
        ''' tombant sur l'élément ne le rouvre pas.
        ''' </summary>
        Public Sub Toggle(Optional cible As UIElement = Nothing)
            If IsOpen Then
                Hide()
            ElseIf (Date.UtcNow - _fermeSurLaCible).TotalMilliseconds > 400 Then
                Show(cible)
            End If
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _popup IsNot Nothing Then
                RemoveHandler _popup.Opened, AddressOf Popup_Opened
                RemoveHandler _popup.Closed, AddressOf Popup_Closed
                _popup.CustomPopupPlacementCallback = Nothing
            End If
            MyBase.OnApplyTemplate()
            _popup = TryCast(GetTemplateChild("PART_Popup"), Popup)
            _fleche = TryCast(GetTemplateChild("PART_Arrow"), FrameworkElement)
            If _popup IsNot Nothing Then
                AddHandler _popup.Opened, AddressOf Popup_Opened
                AddHandler _popup.Closed, AddressOf Popup_Closed
                _popup.CustomPopupPlacementCallback = AddressOf Placer
            End If
        End Sub

        ' Dessous, son cadre à gauche de l'élément ; sinon dessus. La marge de l'ombre (popover.window.padding) est hors du
        ' cadre : la fenêtre recule d'autant.
        Private Function Placer(taille As Size, cible As Size, decalage As Point) As CustomPopupPlacement()
            Dim marge = MargeDeLOmbre()
            Return {
                New CustomPopupPlacement(New Point(-marge.Left, cible.Height - marge.Top), PopupPrimaryAxis.Horizontal),
                New CustomPopupPlacement(New Point(-marge.Left, -taille.Height + marge.Bottom), PopupPrimaryAxis.Horizontal)}
        End Function

        Private Function MargeDeLOmbre() As Thickness
            Dim marge = TryFindResource("mkm.popover.window.padding")
            Return If(TypeOf marge Is Thickness, DirectCast(marge, Thickness), New Thickness())
        End Function

        Private Shared Sub OnIsOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim fenetre = DirectCast(d, Popover)
            If CBool(e.NewValue) Then Return
            ' Fermé alors qu'il avait le clavier : le clavier revient à son élément, comme mkm.
            If fenetre._popup?.Child IsNot Nothing AndAlso fenetre._popup.Child.IsKeyboardFocusWithin Then fenetre.Target?.Focus()
        End Sub

        Private Sub Popup_Opened(sender As Object, e As EventArgs)
            ' Échap le ferme où qu'ait été le clavier, comme mkm (closeOnEscape écoute le document).
            _fenetre = Window.GetWindow(Me)
            If _fenetre IsNot Nothing Then AddHandler _fenetre.PreviewKeyDown, AddressOf Fenetre_PreviewKeyDown
            ' Après la mise en place : dessus ou dessous, et la flèche face à l'élément.
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, New System.Action(AddressOf Orienter))
            ' Ouvert au clavier, le clavier passe dans le cadre : Tab y tourne, Échap le ferme.
            If InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice Then
                Dispatcher.BeginInvoke(DispatcherPriority.Input, New System.Action(
                    Sub() _popup?.Child?.MoveFocus(New TraversalRequest(FocusNavigationDirection.First))))
            End If
            Me.RaiseEvent(New RoutedEventArgs(OpenedEvent, Me))
        End Sub

        Private Sub Popup_Closed(sender As Object, e As EventArgs)
            If _fenetre IsNot Nothing Then RemoveHandler _fenetre.PreviewKeyDown, AddressOf Fenetre_PreviewKeyDown
            _fenetre = Nothing
            If Target IsNot Nothing AndAlso Target.IsMouseOver AndAlso Mouse.LeftButton = MouseButtonState.Pressed Then
                _fermeSurLaCible = Date.UtcNow
            End If
            SetCurrentValue(IsOpenProperty, False)
            Me.RaiseEvent(New RoutedEventArgs(ClosedEvent, Me))
        End Sub

        ' Retourné quand le cadre est au-dessus de l'élément ; la flèche suit l'élément quand le cadre a dû se décaler
        ' vers la gauche pour tenir dans l'écran (popover.arrow.left chez mkm).
        Private Sub Orienter()
            Dim cadre = _popup?.Child
            If cadre Is Nothing OrElse Target Is Nothing OrElse PresentationSource.FromVisual(cadre) Is Nothing OrElse
               PresentationSource.FromVisual(Target) Is Nothing Then Return
            Dim haut = cadre.PointToScreen(New Point(0, 0))
            Dim cible = Target.PointToScreen(New Point(0, 0))
            SetValue(IsFlippedPropertyKey, haut.Y < cible.Y)
            If _fleche IsNot Nothing Then
                Dim echelle = PresentationSource.FromVisual(cadre).CompositionTarget.TransformFromDevice.M11
                Dim ecart = (cible.X - haut.X) * echelle
                _fleche.RenderTransform = New TranslateTransform(Math.Max(0, ecart), 0)
            End If
        End Sub

        ' Échap le ferme ; le clavier revient à son élément s'il était dans le cadre.
        Private Sub Fenetre_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            If e.Key <> Key.Escape OrElse Not IsOpen Then Return
            Hide()
            e.Handled = True
        End Sub

    End Class

    ''' <summary>
    ''' Le cadre d'un <see cref="Popover"/>, partie de son gabarit : un panneau pour les lecteurs d'écran, nommé par le
    ''' nom d'automatisation du Popover (comme <c>role="dialog"</c> et <c>aria-label</c> dans mkm).
    ''' </summary>
    Friend NotInheritable Class CadreDePopover
        Inherits Border

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Pane,
                                       Function() If(TemplatedParent Is Nothing, String.Empty, AutomationProperties.GetName(TemplatedParent)))
        End Function

    End Class

End Namespace
