Imports System.Collections
Imports System.ComponentModel
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
    ''' La liste déroulante en cascade de mkm (<c>CascadeSelect</c>) : un champ qui, ouvert, montre une liste ; une option
    ''' qui en a d'autres ouvre la suivante à sa droite ; une option sans suite se choisit et s'écrit dans le champ.
    ''' <code>
    ''' &lt;mkm:CascadeSelect Placeholder="Choisir une ville"&gt;
    '''     &lt;mkm:CascadeSelectItem Header="France"&gt;
    '''         &lt;mkm:CascadeSelectItem Header="Paris" /&gt;
    '''     &lt;/mkm:CascadeSelectItem&gt;
    ''' &lt;/mkm:CascadeSelect&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Les listes sont des menus de WPF : le survol et → ouvrent la suivante, ← la referme, Entrée choisit, Échap
    ''' referme. Liées à des données : <c>ItemsSource</c> et un <c>HierarchicalDataTemplate</c> (<c>ItemTemplate</c>) ;
    ''' <c>DisplayMemberPath</c> dit la propriété du texte du champ.
    ''' </remarks>
    <ContentProperty("Items")>
    <TemplatePart(Name:="PART_Field", Type:=GetType(ButtonBase))>
    Public Class CascadeSelect
        Inherits Control

        ''' <summary>L'option choisie : un <see cref="CascadeSelectItem"/>, ou sa donnée. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly SelectedItemProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(SelectedItem), GetType(Object), GetType(CascadeSelect),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnSelectedItemChanged))

        ''' <summary>Le texte gris du champ tant que rien n'est choisi.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Placeholder), GetType(String), GetType(CascadeSelect), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Pour des options liées à des données : la propriété dont la valeur s'écrit dans le champ.</summary>
        Public Shared ReadOnly DisplayMemberPathProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(DisplayMemberPath), GetType(String), GetType(CascadeSelect), New FrameworkPropertyMetadata(Nothing, AddressOf OnAffichageChanged))

        ''' <summary>Vrai quand la liste est ouverte. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsDropDownOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsDropDownOpen), GetType(Boolean), GetType(CascadeSelect),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsDropDownOpenChanged))

        ''' <summary>Vrai pour la bordure rouge d'une saisie refusée.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType(CascadeSelect), New FrameworkPropertyMetadata(False))

        ''' <summary>Les données des options, à la place de <see cref="Items"/>.</summary>
        Public Shared ReadOnly ItemsSourceProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ItemsSource), GetType(IEnumerable), GetType(CascadeSelect), New FrameworkPropertyMetadata(Nothing, AddressOf OnItemsSourceChanged))

        ''' <summary>Le gabarit d'une option liée à des données : un <see cref="HierarchicalDataTemplate"/>.</summary>
        Public Shared ReadOnly ItemTemplateProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ItemTemplate), GetType(DataTemplate), GetType(CascadeSelect), New FrameworkPropertyMetadata(Nothing, AddressOf OnItemTemplateChanged))

        Private Shared ReadOnly SelectionTextPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(SelectionText), GetType(String), GetType(CascadeSelect), New FrameworkPropertyMetadata(String.Empty))
        ''' <summary>Le texte du champ : celui de l'option choisie.</summary>
        Public Shared ReadOnly SelectionTextProperty As DependencyProperty = SelectionTextPropertyKey.DependencyProperty

        Private Shared ReadOnly HasSelectionPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasSelection), GetType(Boolean), GetType(CascadeSelect), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand une option est choisie.</summary>
        Public Shared ReadOnly HasSelectionProperty As DependencyProperty = HasSelectionPropertyKey.DependencyProperty

        ''' <summary>Le choix a changé.</summary>
        Public Shared ReadOnly SelectedItemChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(SelectedItemChanged), RoutingStrategy.Bubble, GetType(RoutedPropertyChangedEventHandler(Of Object)), GetType(CascadeSelect))

        Private ReadOnly _liste As ListeEnCascade
        Private ReadOnly _fermeture As FermetureAuDefilement
        Private _champ As ButtonBase
        Private _fermeeSurLeChamp As Date

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(CascadeSelect), New FrameworkPropertyMetadata(GetType(CascadeSelect)))
            FocusableProperty.OverrideMetadata(GetType(CascadeSelect), New FrameworkPropertyMetadata(True))
        End Sub

        Public Sub New()
            _liste = New ListeEnCascade(Me) With {.Placement = PlacementMode.Bottom, .PlacementTarget = Me}
            _fermeture = New FermetureAuDefilement(Me, Sub() SetCurrentValue(IsDropDownOpenProperty, False))
            AddHandler _liste.Opened, AddressOf Liste_Opened
            AddHandler _liste.Closed, AddressOf Liste_Closed
            _liste.AddHandler(MenuItem.ClickEvent, New RoutedEventHandler(AddressOf Option_Click))
            ' La liste hérite du contexte de données du champ.
            _liste.SetBinding(DataContextProperty, New Data.Binding(NameOf(DataContext)) With {.Source = Me})
        End Sub

        ''' <summary>Les options (des <see cref="CascadeSelectItem"/>), écrites dans le XAML.</summary>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
        Public ReadOnly Property Items As ItemCollection
            Get
                Return _liste.Items
            End Get
        End Property

        ''' <summary>L'option choisie, ou sa donnée.</summary>
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

        ''' <summary>La propriété dont la valeur s'écrit dans le champ, pour des options liées à des données.</summary>
        Public Property DisplayMemberPath As String
            Get
                Return CStr(GetValue(DisplayMemberPathProperty))
            End Get
            Set(value As String)
                SetValue(DisplayMemberPathProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la liste est ouverte.</summary>
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

        ''' <summary>Les données des options.</summary>
        Public Property ItemsSource As IEnumerable
            Get
                Return DirectCast(GetValue(ItemsSourceProperty), IEnumerable)
            End Get
            Set(value As IEnumerable)
                SetValue(ItemsSourceProperty, value)
            End Set
        End Property

        ''' <summary>Le gabarit d'une option liée à des données.</summary>
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

        ''' <summary>Vrai quand une option est choisie.</summary>
        Public ReadOnly Property HasSelection As Boolean
            Get
                Return CBool(GetValue(HasSelectionProperty))
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

        ''' <summary>La liste du champ, pour les tests.</summary>
        Friend ReadOnly Property Liste As ContextMenu
            Get
                Return _liste
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _champ IsNot Nothing Then RemoveHandler _champ.Click, AddressOf Champ_Click
            MyBase.OnApplyTemplate()
            _champ = TryCast(GetTemplateChild("PART_Field"), ButtonBase)
            If _champ IsNot Nothing Then AddHandler _champ.Click, AddressOf Champ_Click
            Decrire()
        End Sub

        ''' <summary>Le texte d'une option : son titre, ou la propriété <see cref="DisplayMemberPath"/> de sa donnée.</summary>
        Friend Function TexteDe(choix As Object) As String
            Dim element = TryCast(choix, HeaderedItemsControl)
            If element IsNot Nothing Then Return If(element.Header?.ToString(), String.Empty)
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

        Private Shared Sub OnSelectedItemChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, CascadeSelect)
            champ.Decrire()
            TryCast(UIElementAutomationPeer.FromElement(champ), PairDeCascadeSelect)?.AnnoncerLaValeur()
            champ.RaiseEvent(New RoutedPropertyChangedEventArgs(Of Object)(e.OldValue, e.NewValue, SelectedItemChangedEvent))
        End Sub

        Private Shared Sub OnAffichageChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, CascadeSelect).Decrire()
        End Sub

        Private Shared Sub OnItemsSourceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, CascadeSelect)._liste.ItemsSource = DirectCast(e.NewValue, IEnumerable)
        End Sub

        Private Shared Sub OnItemTemplateChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, CascadeSelect)._liste.ItemTemplate = DirectCast(e.NewValue, DataTemplate)
        End Sub

        ' ---- Ouvrir, fermer, choisir ----

        Private Shared Sub OnIsDropDownOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, CascadeSelect)
            Dim ouvert = CBool(e.NewValue)
            If ouvert AndAlso champ._liste.Items.Count = 0 Then
                champ.SetCurrentValue(IsDropDownOpenProperty, False)
                Return
            End If
            ' La liste aussi large que le champ, au moins (min-width: 100% chez mkm).
            If ouvert Then champ._liste.MinWidth = champ.ActualWidth
            champ._liste.IsOpen = ouvert
            If ouvert Then champ._fermeture.Ecouter() Else champ._fermeture.Cesser()
            TryCast(UIElementAutomationPeer.FromElement(champ), PairDeCascadeSelect)?.Annoncer(Not ouvert, ouvert)
        End Sub

        ' Ouverte au clavier, la liste le prend sur l'option choisie si elle est de la première liste, sinon sur la
        ' première, comme mkm.
        Private Sub Liste_Opened(sender As Object, e As RoutedEventArgs)
            SetCurrentValue(IsDropDownOpenProperty, True)
            If Not (InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice) Then Return
            Dispatcher.BeginInvoke(DispatcherPriority.Input, New System.Action(
                Sub()
                    Dim choisie = TryCast(_liste.ItemContainerGenerator.ContainerFromItem(SelectedItem), MenuItem)
                    Dim premiere = _liste.Items.Cast(Of Object)().
                        Select(Function(o) TryCast(_liste.ItemContainerGenerator.ContainerFromItem(o), MenuItem)).
                        FirstOrDefault(Function(m) m IsNot Nothing AndAlso m.IsEnabled)
                    Dim cible = If(choisie, premiere)
                    cible?.Focus()
                End Sub))
        End Sub

        ' Fermée, la liste rend le clavier au champ.
        Private Sub Liste_Closed(sender As Object, e As RoutedEventArgs)
            If _champ IsNot Nothing AndAlso _champ.IsMouseOver AndAlso Mouse.LeftButton = MouseButtonState.Pressed Then
                _fermeeSurLeChamp = Date.UtcNow
            End If
            SetCurrentValue(IsDropDownOpenProperty, False)
            If IsVisible Then Focus()
        End Sub

        ' Une option sans suite se choisit ; la liste se referme.
        Private Sub Option_Click(sender As Object, e As RoutedEventArgs)
            Dim conteneur = TryCast(e.OriginalSource, MenuItem)
            If conteneur Is Nothing OrElse conteneur.HasItems Then Return
            e.Handled = True
            Choisir(DonneeDe(conteneur))
        End Sub

        ''' <summary>La donnée d'une option : l'option elle-même, écrite dans le XAML, ou ce qu'elle montre.</summary>
        Friend Shared Function DonneeDe(conteneur As MenuItem) As Object
            Dim parent = ItemsControl.ItemsControlFromItemContainer(conteneur)
            Dim donnee = parent?.ItemContainerGenerator.ItemFromContainer(conteneur)
            Return If(donnee Is Nothing OrElse donnee Is DependencyProperty.UnsetValue, conteneur, donnee)
        End Function

        ''' <summary>Choisit l'option et referme la liste ; le clavier revient au champ.</summary>
        Friend Sub Choisir(choix As Object)
            SetCurrentValue(SelectedItemProperty, choix)
            SetCurrentValue(IsDropDownOpenProperty, False)
            Focus()
        End Sub

        ' Un clic sur le champ ouvre ou ferme la liste (un clic qui vient de la fermer ne la rouvre pas).
        Private Sub Champ_Click(sender As Object, e As RoutedEventArgs)
            If Not IsKeyboardFocused Then Focus()
            If Not IsDropDownOpen AndAlso (Date.UtcNow - _fermeeSurLeChamp).TotalMilliseconds < 400 Then Return
            SetCurrentValue(IsDropDownOpenProperty, Not IsDropDownOpen)
        End Sub

        ''' <summary>Sur le champ : ↓, Entrée, Espace, F4 ou Alt+↓ ouvrent la liste.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse IsDropDownOpen Then Return
            Dim touche = If(e.Key = Key.System, e.SystemKey, e.Key)
            If touche = Key.Down OrElse touche = Key.Enter OrElse touche = Key.Space OrElse touche = Key.F4 Then
                SetCurrentValue(IsDropDownOpenProperty, True)
                e.Handled = True
            End If
        End Sub

        ''' <summary>Une liste déroulante pour les lecteurs d'écran : sa valeur, le texte de l'option choisie.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeCascadeSelect(Me)
        End Function

        Private NotInheritable Class PairDeCascadeSelect
            Inherits FrameworkElementAutomationPeer
            Implements IExpandCollapseProvider, IValueProvider

            Public Sub New(champ As CascadeSelect)
                MyBase.New(champ)
            End Sub

            Private ReadOnly Property Champ As CascadeSelect
                Get
                    Return DirectCast(Owner, CascadeSelect)
                End Get
            End Property

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.ComboBox
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(CascadeSelect)
            End Function

            Protected Overrides Function IsKeyboardFocusableCore() As Boolean
                Return Champ.Focusable AndAlso Champ.IsEnabled
            End Function

            Protected Overrides Function GetNameCore() As String
                Dim nom = MyBase.GetNameCore()
                Return If(String.IsNullOrEmpty(nom), If(Champ.Placeholder, String.Empty), nom)
            End Function

            ' La zone qui ouvre la liste n'est que la surface du champ.
            Protected Overrides Function GetChildrenCore() As Collections.Generic.List(Of AutomationPeer)
                Return Nothing
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

    ''' <summary>
    ''' Une option de <see cref="CascadeSelect"/> : son texte (<c>Header</c>) et, si elle en a, les options de la liste
    ''' suivante. <c>IsChosen</c> marque l'option choisie.
    ''' </summary>
    Public Class CascadeSelectItem
        Inherits MenuItem

        Private Shared ReadOnly IsChosenPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsChosen), GetType(Boolean), GetType(CascadeSelectItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour l'option choisie : elle prend le fond de la surbrillance.</summary>
        Public Shared ReadOnly IsChosenProperty As DependencyProperty = IsChosenPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(CascadeSelectItem), New FrameworkPropertyMetadata(GetType(CascadeSelectItem)))
        End Sub

        Public Sub New()
            ' Montrée (sa liste ouverte), l'option se dit choisie ou non.
            AddHandler Loaded, Sub() Marquer()
        End Sub

        ''' <summary>Vrai pour l'option choisie.</summary>
        Public ReadOnly Property IsChosen As Boolean
            Get
                Return CBool(GetValue(IsChosenProperty))
            End Get
        End Property

        Private Sub Marquer()
            Dim parent As ItemsControl = Me
            While parent IsNot Nothing AndAlso TypeOf parent IsNot ListeEnCascade
                parent = ItemsControl.ItemsControlFromItemContainer(parent)
            End While
            Dim champ = TryCast(parent, ListeEnCascade)?.Champ
            If champ Is Nothing Then Return
            Dim choix = champ.SelectedItem
            SetValue(IsChosenPropertyKey, choix IsNot Nothing AndAlso Equals(CascadeSelect.DonneeDe(Me), choix))
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is CascadeSelectItem OrElse TypeOf item Is Separator
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New CascadeSelectItem()
        End Function

    End Class

    ''' <summary>La première liste d'un <see cref="CascadeSelect"/> : un menu, ses options des <see cref="CascadeSelectItem"/>.</summary>
    Friend NotInheritable Class ListeEnCascade
        Inherits ContextMenu

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ListeEnCascade), New FrameworkPropertyMetadata(GetType(ListeEnCascade)))
        End Sub

        Friend Sub New(champ As CascadeSelect)
            Me.Champ = champ
        End Sub

        Friend ReadOnly Property Champ As CascadeSelect

        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is CascadeSelectItem OrElse TypeOf item Is Separator
        End Function

        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New CascadeSelectItem()
        End Function

    End Class

End Namespace
