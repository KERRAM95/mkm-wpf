Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>Où s'ouvrent les actions d'un <see cref="SpeedDial"/>.</summary>
    Public Enum SpeedDialDirection
        Up
        Down
        Left
        Right
        UpLeft
        UpRight
        DownLeft
        DownRight
    End Enum

    ''' <summary>Comment se rangent les actions d'un <see cref="SpeedDial"/>.</summary>
    Public Enum SpeedDialType
        ''' <summary>En ligne, dans la direction (par défaut).</summary>
        Linear
        ''' <summary>En cercle autour du bouton.</summary>
        Circle
        ''' <summary>En demi-cercle, du côté de la direction (Up, Down, Left, Right).</summary>
        SemiCircle
        ''' <summary>En quart de cercle, dans la direction (UpLeft, UpRight, DownLeft, DownRight).</summary>
        QuarterCircle
    End Enum

    ''' <summary>
    ''' Le bouton d'actions de mkm (<c>SpeedDial</c>) : un bouton rond qui, cliqué, déploie ses actions (des boutons
    ''' ronds) en ligne, en cercle ou en arc ; une action choisie les replie.
    ''' <code>
    ''' &lt;mkm:SpeedDial Direction="Up"&gt;
    '''     &lt;mkm:SpeedDialItem Label="Copier" Icon="{x:Static mkm:Icons.Copy}" /&gt;
    ''' &lt;/mkm:SpeedDial&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Button", Type:=GetType(ButtonBase))>
    Public Class SpeedDial
        Inherits ItemsControl

        ''' <summary>Vrai quand les actions sont déployées. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsOpen), GetType(Boolean), GetType(SpeedDial),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsOpenChanged))

        ''' <summary>Où s'ouvrent les actions : vers le haut par défaut.</summary>
        Public Shared ReadOnly DirectionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Direction), GetType(SpeedDialDirection), GetType(SpeedDial), New FrameworkPropertyMetadata(SpeedDialDirection.Up))

        ''' <summary>Comment se rangent les actions : en ligne par défaut.</summary>
        Public Shared ReadOnly TypeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Type), GetType(SpeedDialType), GetType(SpeedDial), New FrameworkPropertyMetadata(SpeedDialType.Linear))

        ''' <summary>Le rayon d'un cercle ou d'un arc, en pixels ; 0 (par défaut) : 20 par action.</summary>
        Public Shared ReadOnly RadiusProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Radius), GetType(Double), GetType(SpeedDial), New FrameworkPropertyMetadata(0.0))

        ''' <summary>L'icône du bouton fermé : le plus par défaut.</summary>
        Public Shared ReadOnly ShowIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowIcon), GetType(Object), GetType(SpeedDial), New FrameworkPropertyMetadata(InternalIcons.Plus))

        ''' <summary>L'icône du bouton ouvert ; sans elle, le plus tourne d'un huitième de tour (une croix).</summary>
        Public Shared ReadOnly HideIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(HideIcon), GetType(Object), GetType(SpeedDial), New FrameworkPropertyMetadata(Nothing, AddressOf OnHideIconChanged))

        Private Shared ReadOnly HasHideIconPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasHideIcon), GetType(Boolean), GetType(SpeedDial), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand une icône de bouton ouvert est donnée.</summary>
        Public Shared ReadOnly HasHideIconProperty As DependencyProperty = HasHideIconPropertyKey.DependencyProperty

        Private _bouton As ButtonBase
        Private _fenetre As Window

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(SpeedDial), New FrameworkPropertyMetadata(GetType(SpeedDial)))
            FocusableProperty.OverrideMetadata(GetType(SpeedDial), New FrameworkPropertyMetadata(False))
        End Sub

        Public Sub New()
            [AddHandler](ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Action_Click))
            ' Ouvert dès le XAML (sans fenêtre encore), le clic dehors est écouté au chargement ; déchargé, il ne
            ' l'est plus (la fenêtre retiendrait le contrôle).
            AddHandler Loaded, Sub()
                                   If IsOpen Then Ecouter()
                               End Sub
            AddHandler Unloaded, Sub() Cesser()
        End Sub

        ''' <summary>Vrai quand les actions sont déployées.</summary>
        Public Property IsOpen As Boolean
            Get
                Return CBool(GetValue(IsOpenProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsOpenProperty, value)
            End Set
        End Property

        ''' <summary>Où s'ouvrent les actions.</summary>
        Public Property Direction As SpeedDialDirection
            Get
                Return DirectCast(GetValue(DirectionProperty), SpeedDialDirection)
            End Get
            Set(value As SpeedDialDirection)
                SetValue(DirectionProperty, value)
            End Set
        End Property

        ''' <summary>Comment se rangent les actions.</summary>
        Public Property Type As SpeedDialType
            Get
                Return DirectCast(GetValue(TypeProperty), SpeedDialType)
            End Get
            Set(value As SpeedDialType)
                SetValue(TypeProperty, value)
            End Set
        End Property

        ''' <summary>Le rayon d'un cercle ou d'un arc.</summary>
        Public Property Radius As Double
            Get
                Return CDbl(GetValue(RadiusProperty))
            End Get
            Set(value As Double)
                SetValue(RadiusProperty, value)
            End Set
        End Property

        ''' <summary>L'icône du bouton fermé.</summary>
        Public Property ShowIcon As Object
            Get
                Return GetValue(ShowIconProperty)
            End Get
            Set(value As Object)
                SetValue(ShowIconProperty, value)
            End Set
        End Property

        ''' <summary>L'icône du bouton ouvert.</summary>
        Public Property HideIcon As Object
            Get
                Return GetValue(HideIconProperty)
            End Get
            Set(value As Object)
                SetValue(HideIconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand une icône de bouton ouvert est donnée.</summary>
        Public ReadOnly Property HasHideIcon As Boolean
            Get
                Return CBool(GetValue(HasHideIconProperty))
            End Get
        End Property

        Private Shared Sub OnHideIconChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            d.SetValue(HasHideIconPropertyKey, e.NewValue IsNot Nothing)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is SpeedDialItem
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New SpeedDialItem()
        End Function

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _bouton = TryCast(GetTemplateChild("PART_Button"), ButtonBase)
        End Sub

        ' Le bouton ouvre ou ferme ; une action choisie (ou sa commande) replie les actions, comme mkm.
        Private Sub Action_Click(sender As Object, e As RoutedEventArgs)
            If e.OriginalSource Is _bouton Then
                SetCurrentValue(IsOpenProperty, Not IsOpen)
                e.Handled = True
            ElseIf TypeOf e.OriginalSource Is SpeedDialItem Then
                SetCurrentValue(IsOpenProperty, False)
                _bouton?.Focus()
            End If
        End Sub

        Private Shared Sub OnIsOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim menu = DirectCast(d, SpeedDial)
            Dim ouvert = CBool(e.NewValue)
            If ouvert Then
                menu.Ecouter()
                If InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice Then
                    menu.Dispatcher.BeginInvoke(Threading.DispatcherPriority.Loaded, New System.Action(
                        Sub() TryCast(menu.ItemContainerGenerator.ContainerFromIndex(0), UIElement)?.Focus()))
                End If
            Else
                menu.Cesser()
                If menu.IsKeyboardFocusWithin AndAlso menu._bouton IsNot Nothing AndAlso Not menu._bouton.IsKeyboardFocused Then menu._bouton.Focus()
            End If
            TryCast(UIElementAutomationPeer.FromElement(If(CType(menu._bouton, UIElement), menu)), PairDuBoutonDeSpeedDial)?.Annoncer(Not ouvert, ouvert)
        End Sub

        ' Ouvert, un clic hors du bouton et de ses actions le replie, comme mkm.
        Private Sub Ecouter()
            Cesser()
            _fenetre = Window.GetWindow(Me)
            _fenetre?.AddHandler(PreviewMouseDownEvent, New MouseButtonEventHandler(AddressOf Fenetre_PreviewMouseDown), True)
        End Sub

        Private Sub Cesser()
            _fenetre?.RemoveHandler(PreviewMouseDownEvent, New MouseButtonEventHandler(AddressOf Fenetre_PreviewMouseDown))
            _fenetre = Nothing
        End Sub

        Private Sub Fenetre_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
            Dim cible = TryCast(e.OriginalSource, DependencyObject)
            While cible IsNot Nothing
                If cible Is Me Then Return
                cible = If(TypeOf cible Is Media.Visual, Media.VisualTreeHelper.GetParent(cible), LogicalTreeHelper.GetParent(cible))
            End While
            SetCurrentValue(IsOpenProperty, False)
        End Sub

        ''' <summary>Échap replie les actions.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse e.Key <> Key.Escape OrElse Not IsOpen Then Return
            SetCurrentValue(IsOpenProperty, False)
            _bouton?.Focus()
            e.Handled = True
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran : son bouton, puis ses actions.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

    ''' <summary>Une action d'un <see cref="SpeedDial"/> : un bouton rond d'icône, son nom (<see cref="Label"/>) en info-bulle.</summary>
    Public Class SpeedDialItem
        Inherits Button

        ''' <summary>Le nom de l'action : son info-bulle et son nom pour les lecteurs d'écran.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Label), GetType(String), GetType(SpeedDialItem), New FrameworkPropertyMetadata(Nothing, AddressOf OnLabelChanged))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(SpeedDialItem), New FrameworkPropertyMetadata(GetType(SpeedDialItem)))
        End Sub

        ''' <summary>Le nom de l'action.</summary>
        Public Property Label As String
            Get
                Return CStr(GetValue(LabelProperty))
            End Get
            Set(value As String)
                SetValue(LabelProperty, value)
            End Set
        End Property

        Private Shared Sub OnLabelChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim action = DirectCast(d, SpeedDialItem)
            If action.ReadLocalValue(ToolTipProperty) Is DependencyProperty.UnsetValue OrElse TypeOf action.ToolTip Is String Then
                action.ToolTip = If(String.IsNullOrEmpty(CStr(e.NewValue)), Nothing, e.NewValue)
            End If
        End Sub

    End Class

    ''' <summary>Le bouton rond d'un <see cref="SpeedDial"/>, qui se dit développé ou réduit.</summary>
    Friend NotInheritable Class BoutonDeSpeedDial
        Inherits Button

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDuBoutonDeSpeedDial(Me)
        End Function

    End Class

    Friend NotInheritable Class PairDuBoutonDeSpeedDial
        Inherits ButtonAutomationPeer
        Implements IExpandCollapseProvider

        Friend Sub New(bouton As BoutonDeSpeedDial)
            MyBase.New(bouton)
        End Sub

        Private ReadOnly Property Menu As SpeedDial
            Get
                Return TryCast(DirectCast(Owner, FrameworkElement).TemplatedParent, SpeedDial)
            End Get
        End Property

        Protected Overrides Function GetClassNameCore() As String
            Return NameOf(SpeedDial)
        End Function

        Protected Overrides Function GetNameCore() As String
            Dim nom = If(Menu IsNot Nothing, AutomationProperties.GetName(Menu), Nothing)
            Return If(String.IsNullOrEmpty(nom), Localization.Localizer.Current.T("aria.actions"), nom)
        End Function

        Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
            If patternInterface = PatternInterface.ExpandCollapse Then Return Me
            Return MyBase.GetPattern(patternInterface)
        End Function

        Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
            Get
                Return If(Menu IsNot Nothing AndAlso Menu.IsOpen, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
            End Get
        End Property

        Public Sub Expand() Implements IExpandCollapseProvider.Expand
            Menu?.SetCurrentValue(SpeedDial.IsOpenProperty, True)
        End Sub

        Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
            Menu?.SetCurrentValue(SpeedDial.IsOpenProperty, False)
        End Sub

        Friend Sub Annoncer(avant As Boolean, apres As Boolean)
            If Not ListenerExists(AutomationEvents.PropertyChanged) Then Return
            RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                                      If(avant, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed),
                                      If(apres, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed))
        End Sub

    End Class

    ''' <summary>
    ''' La rangée des actions d'un <see cref="SpeedDial"/> : sans taille à elle, elle pose chaque action autour du centre
    ''' du bouton, en ligne ou en arc (calculatePointStyle de mkm).
    ''' </summary>
    Friend NotInheritable Class RangeeDeSpeedDial
        Inherits System.Windows.Controls.Panel

        Public Shared ReadOnly DirectionProperty As DependencyProperty = SpeedDial.DirectionProperty.AddOwner(
            GetType(RangeeDeSpeedDial), New FrameworkPropertyMetadata(SpeedDialDirection.Up, FrameworkPropertyMetadataOptions.AffectsArrange))

        Public Shared ReadOnly TypeProperty As DependencyProperty = SpeedDial.TypeProperty.AddOwner(
            GetType(RangeeDeSpeedDial), New FrameworkPropertyMetadata(SpeedDialType.Linear, FrameworkPropertyMetadataOptions.AffectsArrange))

        Public Shared ReadOnly RadiusProperty As DependencyProperty = SpeedDial.RadiusProperty.AddOwner(
            GetType(RangeeDeSpeedDial), New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange))

        Public Shared ReadOnly GapProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Gap), GetType(Double), GetType(RangeeDeSpeedDial), New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange))

        Public Property Direction As SpeedDialDirection
            Get
                Return DirectCast(GetValue(DirectionProperty), SpeedDialDirection)
            End Get
            Set(value As SpeedDialDirection)
                SetValue(DirectionProperty, value)
            End Set
        End Property

        Public Property Type As SpeedDialType
            Get
                Return DirectCast(GetValue(TypeProperty), SpeedDialType)
            End Get
            Set(value As SpeedDialType)
                SetValue(TypeProperty, value)
            End Set
        End Property

        Public Property Radius As Double
            Get
                Return CDbl(GetValue(RadiusProperty))
            End Get
            Set(value As Double)
                SetValue(RadiusProperty, value)
            End Set
        End Property

        Public Property Gap As Double
            Get
                Return CDbl(GetValue(GapProperty))
            End Get
            Set(value As Double)
                SetValue(GapProperty, value)
            End Set
        End Property

        ' Sans taille : les actions débordent autour du bouton, sans pousser la page.
        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            For Each enfant As UIElement In InternalChildren
                enfant.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
            Next
            Return New Size(0, 0)
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim enfants = InternalChildren.Cast(Of UIElement)().ToList()
            Dim centre As New Point(finale.Width / 2, finale.Height / 2)
            For i = 0 To enfants.Count - 1
                Dim ecart = Decalage(i, enfants, finale)
                Dim taille = enfants(i).DesiredSize
                enfants(i).Arrange(New Rect(centre.X + ecart.X - taille.Width / 2, centre.Y + ecart.Y - taille.Height / 2, taille.Width, taille.Height))
            Next
            Return finale
        End Function

        ''' <summary>Le décalage du centre de l'action <paramref name="i"/> depuis le centre du bouton.</summary>
        Friend Function Decalage(i As Integer, enfants As IList(Of UIElement), bouton As Size) As Vector
            Dim nombre = enfants.Count
            If Type <> SpeedDialType.Linear Then
                Dim r = If(Radius > 0, Radius, nombre * 20.0)
                Select Case Type
                    Case SpeedDialType.Circle
                        Dim pas = 2 * Math.PI / nombre
                        Return New Vector(r * Math.Cos(pas * i), r * Math.Sin(pas * i))
                    Case SpeedDialType.SemiCircle
                        Dim pas = If(nombre > 1, Math.PI / (nombre - 1), 0)
                        Dim x = r * Math.Cos(pas * i), y = r * Math.Sin(pas * i)
                        Select Case Direction
                            Case SpeedDialDirection.Down : Return New Vector(x, y)
                            Case SpeedDialDirection.Left : Return New Vector(-y, x)
                            Case SpeedDialDirection.Right : Return New Vector(y, x)
                            Case Else : Return New Vector(x, -y)
                        End Select
                    Case Else
                        Dim pas = If(nombre > 1, Math.PI / (2 * (nombre - 1)), 0)
                        Dim x = r * Math.Cos(pas * i), y = r * Math.Sin(pas * i)
                        Select Case Direction
                            Case SpeedDialDirection.UpLeft : Return New Vector(-x, -y)
                            Case SpeedDialDirection.DownLeft : Return New Vector(-y, x)
                            Case SpeedDialDirection.DownRight : Return New Vector(y, x)
                            Case Else : Return New Vector(x, -y)
                        End Select
                End Select
            End If
            ' En ligne : l'une après l'autre, à Gap du bouton et entre elles.
            Dim horizontal = Direction = SpeedDialDirection.Left OrElse Direction = SpeedDialDirection.Right
            Dim distance = If(horizontal, bouton.Width, bouton.Height) / 2 + Gap
            For j = 0 To i - 1
                distance += If(horizontal, enfants(j).DesiredSize.Width, enfants(j).DesiredSize.Height) + Gap
            Next
            distance += If(horizontal, enfants(i).DesiredSize.Width, enfants(i).DesiredSize.Height) / 2
            Select Case Direction
                Case SpeedDialDirection.Down : Return New Vector(0, distance)
                Case SpeedDialDirection.Left : Return New Vector(-distance, 0)
                Case SpeedDialDirection.Right : Return New Vector(distance, 0)
                Case Else : Return New Vector(0, -distance)
            End Select
        End Function

    End Class

End Namespace
