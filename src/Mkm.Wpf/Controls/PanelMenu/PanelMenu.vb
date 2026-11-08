Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le menu en panneaux de mkm (<c>PanelMenu</c>) : des panneaux l'un sous l'autre ; chacun, cliqué, déplie ses
    ''' éléments, qui déplient les leurs (une arborescence de navigation).
    ''' <code>
    ''' &lt;mkm:PanelMenu&gt;
    '''     &lt;mkm:PanelMenuItem Header="Documents" Icon="{x:Static mkm:Icons.Documents}"&gt;
    '''         &lt;mkm:PanelMenuItem Header="Plans" /&gt;
    '''     &lt;/mkm:PanelMenuItem&gt;
    ''' &lt;/mkm:PanelMenu&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Sans <see cref="Multiple"/>, un seul panneau est déplié à la fois, comme mkm. Au clavier : ↑ ↓ d'un élément à
    ''' l'autre, → déplie, ← replie ou remonte au parent, Entrée et Espace déplient ou lancent.
    ''' </remarks>
    Public Class PanelMenu
        Inherits ItemsControl

        ''' <summary>Vrai pour garder plusieurs panneaux dépliés à la fois.</summary>
        Public Shared ReadOnly MultipleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Multiple), GetType(Boolean), GetType(PanelMenu), New FrameworkPropertyMetadata(False))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(PanelMenu), New FrameworkPropertyMetadata(GetType(PanelMenu)))
            FocusableProperty.OverrideMetadata(GetType(PanelMenu), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai pour garder plusieurs panneaux dépliés.</summary>
        Public Property Multiple As Boolean
            Get
                Return CBool(GetValue(MultipleProperty))
            End Get
            Set(value As Boolean)
                SetValue(MultipleProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is PanelMenuItem
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New PanelMenuItem()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Dim panneau = DirectCast(element, PanelMenuItem)
            panneau.PoserLeNiveau(0)
            Ranger()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Ranger()
        End Sub

        ' Le premier et le dernier panneau : leurs bords et leurs coins.
        Private Sub Ranger()
            Dim panneaux = Enumerable.Range(0, Items.Count).
                Select(Function(i) TryCast(ItemContainerGenerator.ContainerFromIndex(i), PanelMenuItem)).
                Where(Function(p) p IsNot Nothing).ToList()
            For i = 0 To panneaux.Count - 1
                panneaux(i).PoserLaPlace(i = 0, i = panneaux.Count - 1)
            Next
        End Sub

        ''' <summary>Un panneau s'ouvre : sans <see cref="Multiple"/>, les autres se replient.</summary>
        Friend Sub PanneauDeplie(panneau As PanelMenuItem)
            If Multiple Then Return
            For i = 0 To Items.Count - 1
                Dim autre = TryCast(ItemContainerGenerator.ContainerFromIndex(i), PanelMenuItem)
                If autre IsNot Nothing AndAlso autre IsNot panneau Then autre.IsExpanded = False
            Next
        End Sub

        ''' <summary>Un arbre pour les lecteurs d'écran : ses panneaux et leurs éléments.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Tree)
        End Function

    End Class

    ''' <summary>
    ''' Un élément de <see cref="PanelMenu"/> : au premier niveau, un panneau ; plus bas, un lien. Avec des éléments, un
    ''' clic le déplie ; sans, un clic le lance (<see cref="Click"/>, <see cref="Command"/>).
    ''' </summary>
    <TemplatePart(Name:="PART_Header", Type:=GetType(ButtonBase))>
    Public Class PanelMenuItem
        Inherits HeaderedItemsControl

        ''' <summary>L'icône devant le libellé (une géométrie).</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(PanelMenuItem), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai quand ses éléments se montrent. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsExpandedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsExpanded), GetType(Boolean), GetType(PanelMenuItem),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsExpandedChanged))

        ''' <summary>La commande d'un élément sans suite.</summary>
        Public Shared ReadOnly CommandProperty As DependencyProperty = ButtonBase.CommandProperty.AddOwner(GetType(PanelMenuItem))

        ''' <summary>Le paramètre de la commande.</summary>
        Public Shared ReadOnly CommandParameterProperty As DependencyProperty = ButtonBase.CommandParameterProperty.AddOwner(GetType(PanelMenuItem))

        Private Shared ReadOnly LevelPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Level), GetType(Integer), GetType(PanelMenuItem), New FrameworkPropertyMetadata(0))
        ''' <summary>Le niveau : 0 pour un panneau, 1 pour ses éléments…</summary>
        Public Shared ReadOnly LevelProperty As DependencyProperty = LevelPropertyKey.DependencyProperty

        Private Shared ReadOnly IsFirstPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFirst), GetType(Boolean), GetType(PanelMenuItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le premier panneau.</summary>
        Public Shared ReadOnly IsFirstProperty As DependencyProperty = IsFirstPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLastPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLast), GetType(Boolean), GetType(PanelMenuItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le dernier panneau.</summary>
        Public Shared ReadOnly IsLastProperty As DependencyProperty = IsLastPropertyKey.DependencyProperty

        ''' <summary>Un élément sans suite a été lancé.</summary>
        Public Shared ReadOnly ClickEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Click), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(PanelMenuItem))

        Private _entete As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(PanelMenuItem), New FrameworkPropertyMetadata(GetType(PanelMenuItem)))
            FocusableProperty.OverrideMetadata(GetType(PanelMenuItem), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>L'icône devant le libellé.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand ses éléments se montrent.</summary>
        Public Property IsExpanded As Boolean
            Get
                Return CBool(GetValue(IsExpandedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsExpandedProperty, value)
            End Set
        End Property

        ''' <summary>La commande d'un élément sans suite.</summary>
        Public Property Command As ICommand
            Get
                Return DirectCast(GetValue(CommandProperty), ICommand)
            End Get
            Set(value As ICommand)
                SetValue(CommandProperty, value)
            End Set
        End Property

        ''' <summary>Le paramètre de la commande.</summary>
        Public Property CommandParameter As Object
            Get
                Return GetValue(CommandParameterProperty)
            End Get
            Set(value As Object)
                SetValue(CommandParameterProperty, value)
            End Set
        End Property

        ''' <summary>Le niveau.</summary>
        Public ReadOnly Property Level As Integer
            Get
                Return CInt(GetValue(LevelProperty))
            End Get
        End Property

        ''' <summary>Vrai pour le premier panneau.</summary>
        Public ReadOnly Property IsFirst As Boolean
            Get
                Return CBool(GetValue(IsFirstProperty))
            End Get
        End Property

        ''' <summary>Vrai pour le dernier panneau.</summary>
        Public ReadOnly Property IsLast As Boolean
            Get
                Return CBool(GetValue(IsLastProperty))
            End Get
        End Property

        ''' <summary>Un élément sans suite a été lancé.</summary>
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

        Friend Sub PoserLeNiveau(niveau As Integer)
            SetValue(LevelPropertyKey, niveau)
        End Sub

        Friend Sub PoserLaPlace(premier As Boolean, dernier As Boolean)
            SetValue(IsFirstPropertyKey, premier)
            SetValue(IsLastPropertyKey, dernier)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is PanelMenuItem
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New PanelMenuItem()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            DirectCast(element, PanelMenuItem).PoserLeNiveau(Level + 1)
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _entete IsNot Nothing Then
                RemoveHandler _entete.Click, AddressOf Entete_Click
                RemoveHandler _entete.KeyDown, AddressOf Entete_KeyDown
            End If
            MyBase.OnApplyTemplate()
            _entete = TryCast(GetTemplateChild("PART_Header"), ButtonBase)
            If _entete IsNot Nothing Then
                AddHandler _entete.Click, AddressOf Entete_Click
                AddHandler _entete.KeyDown, AddressOf Entete_KeyDown
            End If
        End Sub

        ''' <summary>Avec des éléments, déplie ou replie ; sans, lance l'élément.</summary>
        Friend Sub Activer()
            If HasItems Then
                IsExpanded = Not IsExpanded
                Return
            End If
            Me.RaiseEvent(New RoutedEventArgs(ClickEvent, Me))
            Dim commande = Command
            If commande IsNot Nothing AndAlso commande.CanExecute(CommandParameter) Then commande.Execute(CommandParameter)
        End Sub

        Private Sub Entete_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            Activer()
        End Sub

        Private Shared Sub OnIsExpandedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim element = DirectCast(d, PanelMenuItem)
            If CBool(e.NewValue) Then TryCast(ItemsControl.ItemsControlFromItemContainer(element), PanelMenu)?.PanneauDeplie(element)
            TryCast(UIElementAutomationPeer.FromElement(If(CType(element._entete, UIElement), element)), PairDEnTeteDePanelMenu)?.Annoncer(Not CBool(e.NewValue), CBool(e.NewValue))
        End Sub

        ' → déplie, puis passe au premier élément ; ← replie, puis remonte au parent ; ↑ ↓ passent d'un élément à l'autre.
        Private Sub Entete_KeyDown(sender As Object, e As KeyEventArgs)
            Select Case e.Key
                Case Key.Right
                    If HasItems AndAlso Not IsExpanded Then
                        IsExpanded = True
                    ElseIf HasItems Then
                        TryCast(ItemContainerGenerator.ContainerFromIndex(0), PanelMenuItem)?.Prendre()
                    End If
                Case Key.Left
                    If HasItems AndAlso IsExpanded Then
                        IsExpanded = False
                    Else
                        TryCast(ItemsControl.ItemsControlFromItemContainer(Me), PanelMenuItem)?.Prendre()
                    End If
                Case Key.Down
                    _entete.MoveFocus(New TraversalRequest(FocusNavigationDirection.Down))
                Case Key.Up
                    _entete.MoveFocus(New TraversalRequest(FocusNavigationDirection.Up))
                Case Else
                    Return
            End Select
            e.Handled = True
        End Sub

        ''' <summary>Le clavier, sur l'en-tête de l'élément.</summary>
        Friend Sub Prendre()
            _entete?.Focus()
        End Sub

        ''' <summary>L'en-tête porte le pair de l'élément ; l'élément lui-même ne fait que le grouper.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group, Function() TryCast(Header, String))
        End Function

    End Class

    ''' <summary>L'en-tête d'un <see cref="PanelMenuItem"/> : un bouton, qui se déplie s'il a des éléments.</summary>
    Friend NotInheritable Class EnTeteDePanelMenu
        Inherits ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(EnTeteDePanelMenu), New FrameworkPropertyMetadata(GetType(EnTeteDePanelMenu)))
        End Sub

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDEnTeteDePanelMenu(Me)
        End Function

    End Class

    Friend NotInheritable Class PairDEnTeteDePanelMenu
        Inherits ButtonBaseAutomationPeer
        Implements IExpandCollapseProvider, IInvokeProvider

        Friend Sub New(entete As EnTeteDePanelMenu)
            MyBase.New(entete)
        End Sub

        Private ReadOnly Property Element As PanelMenuItem
            Get
                Return TryCast(DirectCast(Owner, FrameworkElement).TemplatedParent, PanelMenuItem)
            End Get
        End Property

        Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
            Return AutomationControlType.Button
        End Function

        Protected Overrides Function GetClassNameCore() As String
            Return NameOf(PanelMenuItem)
        End Function

        Protected Overrides Function GetNameCore() As String
            Dim nom = MyBase.GetNameCore()
            If Not String.IsNullOrEmpty(nom) Then Return nom
            Return If(TryCast(Element?.Header, String), String.Empty)
        End Function

        Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
            If patternInterface = PatternInterface.ExpandCollapse Then Return If(Element IsNot Nothing AndAlso Element.HasItems, Me, Nothing)
            If patternInterface = PatternInterface.Invoke Then Return Me
            Return MyBase.GetPattern(patternInterface)
        End Function

        Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
            Get
                If Element Is Nothing OrElse Not Element.HasItems Then Return ExpandCollapseState.LeafNode
                Return If(Element.IsExpanded, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
            End Get
        End Property

        Public Sub Expand() Implements IExpandCollapseProvider.Expand
            If Element IsNot Nothing Then Element.IsExpanded = True
        End Sub

        Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
            If Element IsNot Nothing Then Element.IsExpanded = False
        End Sub

        Public Sub Invoke() Implements IInvokeProvider.Invoke
            If Not IsEnabled() Then Throw New ElementNotEnabledException()
            Dispatcher.BeginInvoke(Threading.DispatcherPriority.Input, New System.Action(Sub() Element?.Activer()))
        End Sub

        Friend Sub Annoncer(avant As Boolean, apres As Boolean)
            If Not ListenerExists(AutomationEvents.PropertyChanged) Then Return
            RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                                      If(avant, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed),
                                      If(apres, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed))
        End Sub

    End Class

End Namespace
