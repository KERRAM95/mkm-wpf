Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Markup
Imports System.Windows.Threading

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le bouton partagé de mkm (SplitButton) : l'action principale, et à sa droite un chevron qui ouvre un menu des
    ''' autres. Ses éléments sont ceux du menu, des <c>MenuItem</c> de WPF.
    ''' <code>
    ''' &lt;mkm:SplitButton Content="Enregistrer" Icon="{x:Static mkm:Icons.Download}" Click="Enregistrer_Click"&gt;
    '''     &lt;MenuItem Header="Enregistrer sous…" /&gt;
    '''     &lt;MenuItem Header="Enregistrer une révision" /&gt;
    ''' &lt;/mkm:SplitButton&gt;
    ''' </code>
    ''' </summary>
    <ContentProperty("Items")>
    <TemplatePart(Name:="PART_Button", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_DropDown", Type:=GetType(ButtonBase))>
    Public Class SplitButton
        Inherits ContentControl

        ''' <summary>L'icône du bouton principal, avant son libellé.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(SplitButton), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La gravité des deux boutons.</summary>
        Public Shared ReadOnly SeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Severity), GetType(ButtonSeverity), GetType(SplitButton), New FrameworkPropertyMetadata(ButtonSeverity.Primary))

        ''' <summary>La forme des deux boutons.</summary>
        Public Shared ReadOnly VariantProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([Variant]), GetType(ButtonVariant), GetType(SplitButton), New FrameworkPropertyMetadata(ButtonVariant.Filled))

        ''' <summary>Vrai quand le menu est ouvert. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsDropDownOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsDropDownOpen), GetType(Boolean), GetType(SplitButton),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsDropDownOpenChanged))

        ''' <summary>Le clic sur le bouton principal (non sur le chevron).</summary>
        Public Shared ReadOnly ClickEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Click), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(SplitButton))

        Private ReadOnly _menu As New ContextMenu With {.Placement = PlacementMode.Bottom}
        Private _bouton As ButtonBase
        Private _chevron As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(SplitButton), New FrameworkPropertyMetadata(GetType(SplitButton)))
            FocusableProperty.OverrideMetadata(GetType(SplitButton), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un bouton partagé, son menu vide.</summary>
        Public Sub New()
            _menu.PlacementTarget = Me
            AddHandler _menu.Opened, AddressOf OnMenuOuvert
            AddHandler _menu.Closed, AddressOf OnMenuFerme
            _menu.AddHandler(MenuItem.ClickEvent, New RoutedEventHandler(AddressOf Menu_Click))
            ' Le menu hérite du contexte de données du bouton.
            _menu.SetBinding(DataContextProperty, New Data.Binding(NameOf(DataContext)) With {.Source = Me})
        End Sub

        ''' <summary>Les éléments du menu : des <c>MenuItem</c>, des <c>Separator</c>.</summary>
        <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)>
        Public ReadOnly Property Items As ItemCollection
            Get
                Return _menu.Items
            End Get
        End Property

        ''' <summary>Le menu du chevron (un ContextMenu de WPF, au style de mkm).</summary>
        Public ReadOnly Property DropDownMenu As ContextMenu
            Get
                Return _menu
            End Get
        End Property

        ''' <summary>L'icône du bouton principal.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>La gravité.</summary>
        Public Property Severity As ButtonSeverity
            Get
                Return DirectCast(GetValue(SeverityProperty), ButtonSeverity)
            End Get
            Set(value As ButtonSeverity)
                SetValue(SeverityProperty, value)
            End Set
        End Property

        ''' <summary>La forme.</summary>
        Public Property [Variant] As ButtonVariant
            Get
                Return DirectCast(GetValue(VariantProperty), ButtonVariant)
            End Get
            Set(value As ButtonVariant)
                SetValue(VariantProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le menu est ouvert.</summary>
        Public Property IsDropDownOpen As Boolean
            Get
                Return CBool(GetValue(IsDropDownOpenProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsDropDownOpenProperty, value)
            End Set
        End Property

        ''' <summary>Le clic sur le bouton principal.</summary>
        Public Custom Event Click As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                [AddHandler](ClickEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                [RemoveHandler](ClickEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _bouton IsNot Nothing Then RemoveHandler _bouton.Click, AddressOf Bouton_Click
            If _chevron IsNot Nothing Then
                RemoveHandler _chevron.Click, AddressOf Chevron_Click
                RemoveHandler _chevron.PreviewKeyDown, AddressOf Chevron_PreviewKeyDown
            End If
            MyBase.OnApplyTemplate()
            _bouton = TryCast(GetTemplateChild("PART_Button"), ButtonBase)
            _chevron = TryCast(GetTemplateChild("PART_DropDown"), ButtonBase)
            If _bouton IsNot Nothing Then AddHandler _bouton.Click, AddressOf Bouton_Click
            If _chevron IsNot Nothing Then
                AddHandler _chevron.Click, AddressOf Chevron_Click
                AddHandler _chevron.PreviewKeyDown, AddressOf Chevron_PreviewKeyDown
            End If
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par le libellé : ses deux boutons.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group, Function() TryCast(Content, String))
        End Function

        ' Le bouton principal : le Click du bouton partagé, et non celui du bouton du gabarit.
        Private Sub Bouton_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            Me.RaiseEvent(New RoutedEventArgs(ClickEvent, Me))
        End Sub

        ' Le clic d'une action du menu remonte au bouton partagé et à la page, comme celui d'un menu posé dans la page :
        ' le menu, une fenêtre à part, ne le leur transmet pas.
        Private Sub Menu_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            Me.RaiseEvent(New RoutedEventArgs(MenuItem.ClickEvent, e.OriginalSource))
        End Sub

        Private Sub Chevron_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            SetCurrentValue(IsDropDownOpenProperty, Not IsDropDownOpen)
        End Sub

        ' ↓ et ↑ sur le chevron ouvrent le menu, comme mkm.
        Private Sub Chevron_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            If e.Key <> Key.Down AndAlso e.Key <> Key.Up Then Return
            SetCurrentValue(IsDropDownOpenProperty, True)
            e.Handled = True
        End Sub

        Private Shared Sub OnIsDropDownOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim bouton = DirectCast(d, SplitButton)
            Dim ouvert = CBool(e.NewValue)
            If ouvert AndAlso bouton._menu.Items.Count = 0 Then
                bouton.SetCurrentValue(IsDropDownOpenProperty, False)
                Return
            End If
            ' Le menu aussi large que le bouton partagé, au moins (min-width: 100% chez mkm).
            If ouvert Then bouton._menu.MinWidth = bouton.ActualWidth
            bouton._menu.IsOpen = ouvert
            TryCast(UIElementAutomationPeer.FromElement(If(CType(bouton._chevron, UIElement), bouton)), PairDuChevron)?.Annoncer(Not ouvert, ouvert)
        End Sub

        ' Ouvert au clavier, le menu prend le focus sur son premier élément, comme dans mkm.
        Private Sub OnMenuOuvert(sender As Object, e As RoutedEventArgs)
            SetCurrentValue(IsDropDownOpenProperty, True)
            If Not (InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice) Then Return
            Dispatcher.BeginInvoke(DispatcherPriority.Input, New System.Action(
                Sub()
                    Dim premier = _menu.Items.OfType(Of MenuItem)().FirstOrDefault(Function(m) m.IsEnabled)
                    premier?.Focus()
                End Sub))
        End Sub

        ' Fermé, le menu rend le focus au chevron, comme dans mkm ; à la souris sans son anneau, que mkm ne montre qu'au
        ' clavier.
        Private Sub OnMenuFerme(sender As Object, e As RoutedEventArgs)
            SetCurrentValue(IsDropDownOpenProperty, False)
            If _chevron IsNot Nothing AndAlso _chevron.IsVisible Then _chevron.Focus()
        End Sub

    End Class

    ''' <summary>
    ''' Le chevron d'un <see cref="SplitButton"/>, partie de son gabarit : un bouton d'icône seule qui se dit
    ''' « développé » ou « réduit » selon son menu (ExpandCollapse), comme <c>aria-expanded</c> dans mkm.
    ''' </summary>
    Friend NotInheritable Class ChevronDeSplitButton
        Inherits Button

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDuChevron(Me)
        End Function

    End Class

    Friend NotInheritable Class PairDuChevron
        Inherits ButtonAutomationPeer
        Implements IExpandCollapseProvider

        Friend Sub New(chevron As ChevronDeSplitButton)
            MyBase.New(chevron)
        End Sub

        Private ReadOnly Property Partage As SplitButton
            Get
                Return TryCast(DirectCast(Owner, FrameworkElement).TemplatedParent, SplitButton)
            End Get
        End Property

        Protected Overrides Function GetClassNameCore() As String
            Return NameOf(SplitButton) & "DropDown"
        End Function

        Protected Overrides Function GetNameCore() As String
            Dim nom = MyBase.GetNameCore()
            Return If(String.IsNullOrEmpty(nom), InfoBulle(Owner), nom)
        End Function

        Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
            If patternInterface = PatternInterface.ExpandCollapse Then Return Me
            Return MyBase.GetPattern(patternInterface)
        End Function

        Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
            Get
                Return If(Partage IsNot Nothing AndAlso Partage.IsDropDownOpen, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
            End Get
        End Property

        Public Sub Expand() Implements IExpandCollapseProvider.Expand
            If Not IsEnabled() Then Throw New ElementNotEnabledException()
            Partage?.SetCurrentValue(SplitButton.IsDropDownOpenProperty, True)
        End Sub

        Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
            If Not IsEnabled() Then Throw New ElementNotEnabledException()
            Partage?.SetCurrentValue(SplitButton.IsDropDownOpenProperty, False)
        End Sub

        Friend Sub Annoncer(avant As Boolean, apres As Boolean)
            If Not ListenerExists(AutomationEvents.PropertyChanged) Then Return
            RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                                      If(avant, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed),
                                      If(apres, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed))
        End Sub

    End Class

End Namespace
