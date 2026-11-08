Imports System.Collections
Imports System.Collections.Specialized
Imports System.ComponentModel
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
    ''' La liste déroulante en arbre de mkm (<c>TreeSelect</c>) : un champ qui, ouvert, montre un arbre (<see cref="Tree"/>)
    ''' où choisir un nœud ; le titre du nœud choisi s'écrit dans le champ.
    ''' <code>
    ''' &lt;mkm:TreeSelect Placeholder="Choisir un dossier"&gt;
    '''     &lt;mkm:TreeNode Header="Documents" IsExpanded="True"&gt;
    '''         &lt;mkm:TreeNode Header="Plans" /&gt;
    '''     &lt;/mkm:TreeNode&gt;
    ''' &lt;/mkm:TreeSelect&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' L'arbre est celui de mkm-wpf, avec ses touches : flèches, + et -, Entrée ou Espace choisissent le nœud sous le
    ''' clavier, Échap referme sans choisir. Lié à des données : <c>ItemsSource</c> et un <c>HierarchicalDataTemplate</c>
    ''' (<c>ItemTemplate</c>) ; <c>DisplayMemberPath</c> dit alors la propriété du texte du champ.
    ''' </remarks>
    <ContentProperty("Items")>
    <TemplatePart(Name:="PART_Field", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Popup", Type:=GetType(Popup))>
    <TemplatePart(Name:="PART_TreeHost", Type:=GetType(ContentPresenter))>
    <TemplatePart(Name:="PART_Clear", Type:=GetType(ButtonBase))>
    Public Class TreeSelect
        Inherits Control

        ''' <summary>Le nœud choisi : un <see cref="TreeNode"/>, ou la donnée de son nœud. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly SelectedItemProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(SelectedItem), GetType(Object), GetType(TreeSelect),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnSelectedItemChanged))

        ''' <summary>Le texte gris du champ tant que rien n'est choisi.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Placeholder), GetType(String), GetType(TreeSelect), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Pour des nœuds liés à des données : la propriété dont la valeur s'écrit dans le champ.</summary>
        Public Shared ReadOnly DisplayMemberPathProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(DisplayMemberPath), GetType(String), GetType(TreeSelect), New FrameworkPropertyMetadata(Nothing, AddressOf OnAffichageChanged))

        ''' <summary>Vrai pour une croix qui retire le choix, quand il y en a un.</summary>
        Public Shared ReadOnly ShowClearProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowClear), GetType(Boolean), GetType(TreeSelect), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai quand l'arbre est ouvert. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsDropDownOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsDropDownOpen), GetType(Boolean), GetType(TreeSelect),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsDropDownOpenChanged))

        ''' <summary>Vrai pour la bordure rouge d'une saisie refusée.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType(TreeSelect), New FrameworkPropertyMetadata(False))

        ''' <summary>Les données des nœuds, à la place de <see cref="Items"/>.</summary>
        Public Shared ReadOnly ItemsSourceProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ItemsSource), GetType(IEnumerable), GetType(TreeSelect), New FrameworkPropertyMetadata(Nothing, AddressOf OnItemsSourceChanged))

        ''' <summary>Le gabarit d'un nœud lié à des données : un <see cref="HierarchicalDataTemplate"/>.</summary>
        Public Shared ReadOnly ItemTemplateProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ItemTemplate), GetType(DataTemplate), GetType(TreeSelect), New FrameworkPropertyMetadata(Nothing, AddressOf OnItemTemplateChanged))

        Private Shared ReadOnly SelectionTextPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(SelectionText), GetType(String), GetType(TreeSelect), New FrameworkPropertyMetadata(String.Empty))
        ''' <summary>Le texte du champ : le titre du nœud choisi.</summary>
        Public Shared ReadOnly SelectionTextProperty As DependencyProperty = SelectionTextPropertyKey.DependencyProperty

        Private Shared ReadOnly HasSelectionPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasSelection), GetType(Boolean), GetType(TreeSelect), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand un nœud est choisi.</summary>
        Public Shared ReadOnly HasSelectionProperty As DependencyProperty = HasSelectionPropertyKey.DependencyProperty

        Private Shared ReadOnly IsEmptyPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsEmpty), GetType(Boolean), GetType(TreeSelect), New FrameworkPropertyMetadata(True))
        ''' <summary>Vrai quand l'arbre n'a aucun nœud : il dit « Aucun résultat ».</summary>
        Public Shared ReadOnly IsEmptyProperty As DependencyProperty = IsEmptyPropertyKey.DependencyProperty

        ''' <summary>Le choix a changé.</summary>
        Public Shared ReadOnly SelectedItemChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(SelectedItemChanged), RoutingStrategy.Bubble, GetType(RoutedPropertyChangedEventHandler(Of Object)), GetType(TreeSelect))

        Private ReadOnly _arbre As New Tree()
        Private ReadOnly _fermeture As FermetureAuDefilement
        Private _champ As ButtonBase
        Private _popup As Popup
        Private _hote As ContentPresenter
        Private _croix As ButtonBase
        Private _fermeeSurLeChamp As Date

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(TreeSelect), New FrameworkPropertyMetadata(GetType(TreeSelect)))
            FocusableProperty.OverrideMetadata(GetType(TreeSelect), New FrameworkPropertyMetadata(True))
        End Sub

        Public Sub New()
            _fermeture = New FermetureAuDefilement(Me, Sub() SetCurrentValue(IsDropDownOpenProperty, False))
            _arbre.Background = Media.Brushes.Transparent
            _arbre.SetResourceReference(PaddingProperty, "mkm.treeselect.tree.padding")
            _arbre.SetResourceReference(MaxHeightProperty, "mkm.treeselect.tree.max.height")
            ' Le clavier tourne dans l'arbre ouvert, sans en sortir par Tab : Tab le referme.
            KeyboardNavigation.SetTabNavigation(_arbre, KeyboardNavigationMode.None)
            AddHandler _arbre.PreviewKeyDown, AddressOf Arbre_PreviewKeyDown
            _arbre.AddHandler(MouseLeftButtonUpEvent, New MouseButtonEventHandler(AddressOf Arbre_MouseLeftButtonUp))
            AddHandler CType(_arbre.Items, INotifyCollectionChanged).CollectionChanged, Sub() MettreAJourLeVide()
            AddLogicalChild(_arbre)
        End Sub

        ''' <summary>Les nœuds de l'arbre (des <see cref="TreeNode"/>), écrits dans le XAML.</summary>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
        Public ReadOnly Property Items As ItemCollection
            Get
                Return _arbre.Items
            End Get
        End Property

        ''' <summary>Le nœud choisi, ou sa donnée.</summary>
        Public Property SelectedItem As Object
            Get
                Return GetValue(SelectedItemProperty)
            End Get
            Set(value As Object)
                SetValue(SelectedItemProperty, value)
            End Set
        End Property

        ''' <summary>Le texte gris du champ tant que rien n'est choisi.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>La propriété dont la valeur s'écrit dans le champ, pour des nœuds liés à des données.</summary>
        Public Property DisplayMemberPath As String
            Get
                Return CStr(GetValue(DisplayMemberPathProperty))
            End Get
            Set(value As String)
                SetValue(DisplayMemberPathProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour une croix qui retire le choix.</summary>
        Public Property ShowClear As Boolean
            Get
                Return CBool(GetValue(ShowClearProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowClearProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand l'arbre est ouvert.</summary>
        Public Property IsDropDownOpen As Boolean
            Get
                Return CBool(GetValue(IsDropDownOpenProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsDropDownOpenProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour la bordure rouge d'une saisie refusée.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <summary>Les données des nœuds.</summary>
        Public Property ItemsSource As IEnumerable
            Get
                Return DirectCast(GetValue(ItemsSourceProperty), IEnumerable)
            End Get
            Set(value As IEnumerable)
                SetValue(ItemsSourceProperty, value)
            End Set
        End Property

        ''' <summary>Le gabarit d'un nœud lié à des données.</summary>
        Public Property ItemTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(ItemTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(ItemTemplateProperty, value)
            End Set
        End Property

        ''' <summary>Le texte du champ.</summary>
        Public ReadOnly Property SelectionText As String
            Get
                Return CStr(GetValue(SelectionTextProperty))
            End Get
        End Property

        ''' <summary>Vrai quand un nœud est choisi.</summary>
        Public ReadOnly Property HasSelection As Boolean
            Get
                Return CBool(GetValue(HasSelectionProperty))
            End Get
        End Property

        ''' <summary>Vrai quand l'arbre n'a aucun nœud.</summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return CBool(GetValue(IsEmptyProperty))
            End Get
        End Property

        ''' <summary>Le choix a changé.</summary>
        Public Custom Event SelectedItemChanged As RoutedPropertyChangedEventHandler(Of Object)
            AddHandler(value As RoutedPropertyChangedEventHandler(Of Object))
                [AddHandler](SelectedItemChangedEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedPropertyChangedEventHandler(Of Object))
                [RemoveHandler](SelectedItemChangedEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object))
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>L'arbre, enfant logique du champ : il en hérite le contexte de données et les ressources.</summary>
        Protected Overrides ReadOnly Property LogicalChildren As IEnumerator
            Get
                Return New Object() {_arbre}.GetEnumerator()
            End Get
        End Property

        ''' <summary>L'arbre du champ, pour les tests.</summary>
        Friend ReadOnly Property Arbre As Tree
            Get
                Return _arbre
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _champ IsNot Nothing Then RemoveHandler _champ.Click, AddressOf Champ_Click
            If _popup IsNot Nothing Then RemoveHandler _popup.Closed, AddressOf Popup_Closed
            If _croix IsNot Nothing Then RemoveHandler _croix.Click, AddressOf Croix_Click
            If _hote IsNot Nothing Then _hote.Content = Nothing
            MyBase.OnApplyTemplate()
            _champ = TryCast(GetTemplateChild("PART_Field"), ButtonBase)
            _popup = TryCast(GetTemplateChild("PART_Popup"), Popup)
            _hote = TryCast(GetTemplateChild("PART_TreeHost"), ContentPresenter)
            _croix = TryCast(GetTemplateChild("PART_Clear"), ButtonBase)
            If _champ IsNot Nothing Then AddHandler _champ.Click, AddressOf Champ_Click
            If _popup IsNot Nothing Then AddHandler _popup.Closed, AddressOf Popup_Closed
            If _croix IsNot Nothing Then AddHandler _croix.Click, AddressOf Croix_Click
            If _hote IsNot Nothing Then _hote.Content = _arbre
            Decrire()
            MettreAJourLeVide()
        End Sub

        ''' <summary>Le texte d'un nœud : son titre, ou la propriété <see cref="DisplayMemberPath"/> de sa donnée.</summary>
        Friend Function TexteDe(choix As Object) As String
            Dim noeud = TryCast(choix, HeaderedItemsControl)
            If noeud IsNot Nothing Then Return If(noeud.Header?.ToString(), String.Empty)
            If choix IsNot Nothing AndAlso Not String.IsNullOrEmpty(DisplayMemberPath) Then
                Dim propriete = TypeDescriptor.GetProperties(choix).Find(DisplayMemberPath, True)
                If propriete IsNot Nothing Then Return If(propriete.GetValue(choix)?.ToString(), String.Empty)
            End If
            Return If(choix?.ToString(), String.Empty)
        End Function

        Private Sub Decrire()
            SetValue(HasSelectionPropertyKey, SelectedItem IsNot Nothing)
            SetValue(SelectionTextPropertyKey, If(SelectedItem Is Nothing, String.Empty, TexteDe(SelectedItem)))
        End Sub

        Private Sub MettreAJourLeVide()
            SetValue(IsEmptyPropertyKey, _arbre.Items.Count = 0)
        End Sub

        Private Shared Sub OnSelectedItemChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, TreeSelect)
            champ.Decrire()
            TryCast(UIElementAutomationPeer.FromElement(champ), PairDeTreeSelect)?.AnnoncerLaValeur()
            champ.RaiseEvent(New RoutedPropertyChangedEventArgs(Of Object)(e.OldValue, e.NewValue, SelectedItemChangedEvent))
        End Sub

        Private Shared Sub OnAffichageChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, TreeSelect).Decrire()
        End Sub

        Private Shared Sub OnItemsSourceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, TreeSelect)
            champ._arbre.ItemsSource = DirectCast(e.NewValue, IEnumerable)
            champ.MettreAJourLeVide()
        End Sub

        Private Shared Sub OnItemTemplateChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, TreeSelect)._arbre.ItemTemplate = DirectCast(e.NewValue, DataTemplate)
        End Sub

        ' ---- Ouvrir, fermer ----

        Private Shared Sub OnIsDropDownOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, TreeSelect)
            Dim ouvert = CBool(e.NewValue)
            If ouvert Then
                champ._fermeture.Ecouter()
                Dim auClavier = InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice
                champ.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, New System.Action(Sub() champ.MontrerLeChoix(auClavier)))
            Else
                champ._fermeture.Cesser()
                ' Le clavier, s'il était dans l'arbre, revient au champ.
                If champ._arbre.IsKeyboardFocusWithin Then champ.Focus()
            End If
            TryCast(UIElementAutomationPeer.FromElement(champ), PairDeTreeSelect)?.Annoncer(Not ouvert, ouvert)
        End Sub

        ' Ouvert : le nœud choisi, ses parents dépliés, sous le clavier ; sans choix, le premier au clavier, l'arbre à la
        ' souris (rien ne s'y surligne avant un geste).
        Private Sub MontrerLeChoix(auClavier As Boolean)
            If Not IsDropDownOpen Then Return
            Dim noeud = Conteneur(SelectedItem)
            If noeud Is Nothing AndAlso _arbre.SelectedItem IsNot Nothing Then
                Dim surligne = Conteneur(_arbre.SelectedItem)
                If surligne IsNot Nothing Then surligne.IsSelected = False
            End If
            If noeud Is Nothing AndAlso auClavier Then noeud = TryCast(_arbre.ItemContainerGenerator.ContainerFromIndex(0), TreeViewItem)
            If noeud Is Nothing Then
                _arbre.Focus()
                Return
            End If
            Dim parent = TryCast(ItemsControl.ItemsControlFromItemContainer(noeud), TreeViewItem)
            While parent IsNot Nothing
                parent.IsExpanded = True
                parent = TryCast(ItemsControl.ItemsControlFromItemContainer(parent), TreeViewItem)
            End While
            noeud.IsSelected = True
            noeud.BringIntoView()
            noeud.Focus()
        End Sub

        ' Le nœud (le conteneur) d'un choix : le choix lui-même, ou celui, réalisé, de sa donnée.
        Friend Function Conteneur(choix As Object) As TreeViewItem
            If choix Is Nothing Then Return Nothing
            Dim noeud = TryCast(choix, TreeViewItem)
            If noeud IsNot Nothing Then Return noeud
            Return Chercher(_arbre, choix)
        End Function

        Private Shared Function Chercher(parent As ItemsControl, choix As Object) As TreeViewItem
            Dim direct = TryCast(parent.ItemContainerGenerator.ContainerFromItem(choix), TreeViewItem)
            If direct IsNot Nothing Then Return direct
            For Each enfant In parent.Items
                Dim conteneur = TryCast(parent.ItemContainerGenerator.ContainerFromItem(enfant), TreeViewItem)
                If conteneur Is Nothing Then Continue For
                Dim trouve = Chercher(conteneur, choix)
                If trouve IsNot Nothing Then Return trouve
            Next
            Return Nothing
        End Function

        ''' <summary>Choisit le nœud et referme l'arbre ; le clavier revient au champ.</summary>
        Friend Sub Choisir(choix As Object)
            SetCurrentValue(SelectedItemProperty, choix)
            SetCurrentValue(IsDropDownOpenProperty, False)
            Focus()
        End Sub

        ' Un clic sur un nœud le choisit (sa flèche, un bouton, garde son clic : elle déplie).
        Private Sub Arbre_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
            Dim noeud = Ancetre(Of TreeViewItem)(TryCast(e.OriginalSource, DependencyObject))
            If noeud Is Nothing OrElse Not noeud.IsSelected OrElse Not noeud.IsEnabled Then Return
            Choisir(_arbre.SelectedItem)
            e.Handled = True
        End Sub

        Private Shared Function Ancetre(Of T As DependencyObject)(depart As DependencyObject) As T
            While depart IsNot Nothing AndAlso TypeOf depart IsNot T
                depart = If(TypeOf depart Is Media.Visual OrElse TypeOf depart Is Media.Media3D.Visual3D,
                            Media.VisualTreeHelper.GetParent(depart), LogicalTreeHelper.GetParent(depart))
            End While
            Return DirectCast(depart, T)
        End Function

        ' Dans l'arbre : Entrée et Espace choisissent le nœud sous le clavier ; Échap referme sans choisir ; Tab referme
        ' et passe à l'élément suivant.
        Private Sub Arbre_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            Select Case e.Key
                Case Key.Enter, Key.Space
                    If _arbre.SelectedItem IsNot Nothing Then Choisir(_arbre.SelectedItem)
                    e.Handled = True
                Case Key.Escape
                    SetCurrentValue(IsDropDownOpenProperty, False)
                    Focus()
                    e.Handled = True
                Case Key.Tab
                    SetCurrentValue(IsDropDownOpenProperty, False)
                    Focus()
                    MoveFocus(New TraversalRequest(If(Keyboard.Modifiers = ModifierKeys.Shift, FocusNavigationDirection.Previous, FocusNavigationDirection.Next)))
                    e.Handled = True
            End Select
        End Sub

        ' Un clic sur le champ ouvre ou ferme l'arbre (un clic qui vient de le fermer ne le rouvre pas).
        Private Sub Champ_Click(sender As Object, e As RoutedEventArgs)
            If Not IsKeyboardFocused Then Focus()
            If Not IsDropDownOpen AndAlso (Date.UtcNow - _fermeeSurLeChamp).TotalMilliseconds < 400 Then Return
            SetCurrentValue(IsDropDownOpenProperty, Not IsDropDownOpen)
        End Sub

        Private Sub Popup_Closed(sender As Object, e As EventArgs)
            If _champ IsNot Nothing AndAlso _champ.IsMouseOver AndAlso Mouse.LeftButton = MouseButtonState.Pressed Then
                _fermeeSurLeChamp = Date.UtcNow
            End If
            SetCurrentValue(IsDropDownOpenProperty, False)
        End Sub

        ' La croix retire le choix.
        Private Sub Croix_Click(sender As Object, e As RoutedEventArgs)
            SetCurrentValue(SelectedItemProperty, Nothing)
            Focus()
            e.Handled = True
        End Sub

        ''' <summary>Sur le champ : ↓, Entrée, Espace, F4 ou Alt+↓ ouvrent l'arbre ; Échap le referme.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse _arbre.IsKeyboardFocusWithin Then Return
            Dim touche = If(e.Key = Key.System, e.SystemKey, e.Key)
            Select Case touche
                Case Key.Down, Key.Enter, Key.Space, Key.F4
                    If Not IsDropDownOpen Then
                        SetCurrentValue(IsDropDownOpenProperty, True)
                        e.Handled = True
                    End If
                Case Key.Escape, Key.Up
                    If IsDropDownOpen AndAlso (touche = Key.Escape OrElse Keyboard.Modifiers = ModifierKeys.Alt) Then
                        SetCurrentValue(IsDropDownOpenProperty, False)
                        e.Handled = True
                    End If
            End Select
        End Sub

        ''' <summary>Une liste déroulante pour les lecteurs d'écran : sa valeur, le titre du nœud choisi ; ouverte ou fermée.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeTreeSelect(Me)
        End Function

        Private NotInheritable Class PairDeTreeSelect
            Inherits FrameworkElementAutomationPeer
            Implements IExpandCollapseProvider, IValueProvider

            Public Sub New(champ As TreeSelect)
                MyBase.New(champ)
            End Sub

            Private ReadOnly Property Champ As TreeSelect
                Get
                    Return DirectCast(Owner, TreeSelect)
                End Get
            End Property

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.ComboBox
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(TreeSelect)
            End Function

            Protected Overrides Function IsKeyboardFocusableCore() As Boolean
                Return Champ.Focusable AndAlso Champ.IsEnabled
            End Function

            Protected Overrides Function GetNameCore() As String
                Dim nom = MyBase.GetNameCore()
                Return If(String.IsNullOrEmpty(nom), If(Champ.Placeholder, String.Empty), nom)
            End Function

            ' La zone qui ouvre l'arbre n'est que la surface du champ : seule la croix (« Effacer ») est un enfant.
            Protected Overrides Function GetChildrenCore() As Collections.Generic.List(Of AutomationPeer)
                Dim enfants = MyBase.GetChildrenCore()
                If enfants Is Nothing Then Return Nothing
                Dim zone = Champ._champ
                enfants.RemoveAll(Function(p) TypeOf p Is UIElementAutomationPeer AndAlso DirectCast(p, UIElementAutomationPeer).Owner Is zone)
                Return If(enfants.Count = 0, Nothing, enfants)
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.ExpandCollapse OrElse patternInterface = PatternInterface.Value Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
                Get
                    Return If(Champ.IsDropDownOpen, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
                End Get
            End Property

            Public Sub Expand() Implements IExpandCollapseProvider.Expand
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                Champ.SetCurrentValue(IsDropDownOpenProperty, True)
            End Sub

            Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                Champ.SetCurrentValue(IsDropDownOpenProperty, False)
            End Sub

            Public ReadOnly Property Value As String Implements IValueProvider.Value
                Get
                    Return Champ.SelectionText
                End Get
            End Property

            Public ReadOnly Property IsReadOnly As Boolean Implements IValueProvider.IsReadOnly
                Get
                    Return True
                End Get
            End Property

            Public Sub SetValue(valeur As String) Implements IValueProvider.SetValue
                Throw New InvalidOperationException()
            End Sub

            Friend Sub Annoncer(avant As Boolean, apres As Boolean)
                If Not ListenerExists(AutomationEvents.PropertyChanged) Then Return
                RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                                          If(avant, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed),
                                          If(apres, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed))
            End Sub

            Friend Sub AnnoncerLaValeur()
                If Not ListenerExists(AutomationEvents.PropertyChanged) Then Return
                RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, String.Empty, Champ.SelectionText)
            End Sub

        End Class

    End Class

End Namespace
