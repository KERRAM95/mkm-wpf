Imports System.ComponentModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Threading

Namespace Mkm.Wpf.Controls

    ''' <summary>Les données de <see cref="AutoComplete.Complete"/> : le texte saisi.</summary>
    Public Class AutoCompleteEventArgs
        Inherits RoutedEventArgs

        ''' <summary>Les données d'une demande de suggestions.</summary>
        Public Sub New(evenement As RoutedEvent, source As Object, query As String)
            MyBase.New(evenement, source)
            Me.Query = query
        End Sub

        ''' <summary>Le texte saisi : l'application cherche les suggestions qui lui répondent.</summary>
        Public ReadOnly Property Query As String

    End Class

    ''' <summary>
    ''' Le champ à suggestions de mkm (AutoComplete) : pendant la frappe, il demande ses suggestions à l'application
    ''' (<c>Complete</c>, après <c>Delay</c> ms et <c>MinLength</c> caractères) et les montre dessous ; ↓ ↑ les parcourent,
    ''' Entrée ou un clic en choisit une (<c>SelectedItem</c>, <c>ItemSelect</c>). <c>Dropdown</c> ajoute un bouton qui
    ''' les demande toutes.
    ''' <code>
    ''' &lt;mkm:AutoComplete Placeholder="Référence" Complete="Reference_Complete" /&gt;
    ''' </code>
    ''' <code>
    ''' Private Sub Reference_Complete(sender As Object, e As AutoCompleteEventArgs)
    '''     DirectCast(sender, AutoComplete).Suggestions = References.Where(Function(r) r.StartsWith(e.Query)).ToList()
    ''' End Sub
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Popup", Type:=GetType(Popup))>
    <TemplatePart(Name:="PART_List", Type:=GetType(System.Windows.Controls.ListBox))>
    <TemplatePart(Name:="PART_DropDown", Type:=GetType(ButtonBase))>
    Public Class AutoComplete
        Inherits InputText

        ''' <summary>Les suggestions à montrer, que l'application pose en réponse à <c>Complete</c>.</summary>
        Public Shared ReadOnly SuggestionsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Suggestions), GetType(Collections.IEnumerable), GetType(AutoComplete),
            New FrameworkPropertyMetadata(Nothing, AddressOf OnSuggestionsChanged))

        ''' <summary>Le nombre de caractères à saisir avant de demander des suggestions : 1 par défaut.</summary>
        Public Shared ReadOnly MinLengthProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MinLength), GetType(Integer), GetType(AutoComplete), New FrameworkPropertyMetadata(1))

        ''' <summary>L'attente après la dernière frappe avant de demander des suggestions, en millisecondes : 300 par défaut.</summary>
        Public Shared ReadOnly DelayProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Delay), GetType(Integer), GetType(AutoComplete), New FrameworkPropertyMetadata(300))

        ''' <summary>La propriété d'une suggestion qui la nomme ; sinon son texte.</summary>
        Public Shared ReadOnly DisplayMemberPathProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(DisplayMemberPath), GetType(String), GetType(AutoComplete), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai pour un bouton à droite du champ qui demande toutes les suggestions.</summary>
        Public Shared ReadOnly DropdownProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Dropdown), GetType(Boolean), GetType(AutoComplete), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai pour dire « Aucun résultat » quand aucune suggestion ne répond.</summary>
        Public Shared ReadOnly ShowEmptyMessageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowEmptyMessage), GetType(Boolean), GetType(AutoComplete), New FrameworkPropertyMetadata(True))

        ''' <summary>La suggestion choisie. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly SelectedItemProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(SelectedItem), GetType(Object), GetType(AutoComplete),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>Vrai quand la liste des suggestions est ouverte.</summary>
        Public Shared ReadOnly IsDropDownOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsDropDownOpen), GetType(Boolean), GetType(AutoComplete),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsDropDownOpenChanged))

        Private Shared ReadOnly HasSuggestionsPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasSuggestions), GetType(Boolean), GetType(AutoComplete), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand au moins une suggestion est montrée.</summary>
        Public Shared ReadOnly HasSuggestionsProperty As DependencyProperty = HasSuggestionsPropertyKey.DependencyProperty

        ''' <summary>Quand il faut des suggestions : <see cref="AutoCompleteEventArgs.Query"/> est le texte saisi.</summary>
        Public Shared ReadOnly CompleteEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Complete), RoutingStrategy.Bubble, GetType(EventHandler(Of AutoCompleteEventArgs)), GetType(AutoComplete))

        ''' <summary>Quand une suggestion est choisie : <see cref="SelectedItem"/>.</summary>
        Public Shared ReadOnly ItemSelectEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(ItemSelect), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(AutoComplete))

        Private ReadOnly _attente As New DispatcherTimer()
        Private ReadOnly _fermeture As FermetureAuDefilement
        Private _liste As System.Windows.Controls.ListBox
        Private _popup As Popup
        Private _bouton As ButtonBase
        Private _texteInterne As Boolean
        Private _demande As Boolean

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(AutoComplete), New FrameworkPropertyMetadata(GetType(AutoComplete)))
        End Sub

        ''' <summary>Un champ à suggestions vide.</summary>
        Public Sub New()
            AddHandler _attente.Tick, AddressOf Attente_Tick
            _fermeture = New FermetureAuDefilement(Me, Sub() SetCurrentValue(IsDropDownOpenProperty, False))
        End Sub

        ''' <summary>Les suggestions à montrer.</summary>
        Public Property Suggestions As Collections.IEnumerable
            Get
                Return DirectCast(GetValue(SuggestionsProperty), Collections.IEnumerable)
            End Get
            Set(value As Collections.IEnumerable)
                SetValue(SuggestionsProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre de caractères avant de demander des suggestions.</summary>
        Public Property MinLength As Integer
            Get
                Return CInt(GetValue(MinLengthProperty))
            End Get
            Set(value As Integer)
                SetValue(MinLengthProperty, value)
            End Set
        End Property

        ''' <summary>L'attente avant de demander des suggestions, en millisecondes.</summary>
        Public Property Delay As Integer
            Get
                Return CInt(GetValue(DelayProperty))
            End Get
            Set(value As Integer)
                SetValue(DelayProperty, value)
            End Set
        End Property

        ''' <summary>La propriété qui nomme une suggestion.</summary>
        Public Property DisplayMemberPath As String
            Get
                Return CStr(GetValue(DisplayMemberPathProperty))
            End Get
            Set(value As String)
                SetValue(DisplayMemberPathProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour le bouton qui demande toutes les suggestions.</summary>
        Public Property Dropdown As Boolean
            Get
                Return CBool(GetValue(DropdownProperty))
            End Get
            Set(value As Boolean)
                SetValue(DropdownProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour dire « Aucun résultat ».</summary>
        Public Property ShowEmptyMessage As Boolean
            Get
                Return CBool(GetValue(ShowEmptyMessageProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowEmptyMessageProperty, value)
            End Set
        End Property

        ''' <summary>La suggestion choisie.</summary>
        Public Property SelectedItem As Object
            Get
                Return GetValue(SelectedItemProperty)
            End Get
            Set(value As Object)
                SetValue(SelectedItemProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la liste des suggestions est ouverte.</summary>
        Public Property IsDropDownOpen As Boolean
            Get
                Return CBool(GetValue(IsDropDownOpenProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsDropDownOpenProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand au moins une suggestion est montrée.</summary>
        Public ReadOnly Property HasSuggestions As Boolean
            Get
                Return CBool(GetValue(HasSuggestionsProperty))
            End Get
        End Property

        ''' <summary>Quand il faut des suggestions.</summary>
        Public Custom Event Complete As EventHandler(Of AutoCompleteEventArgs)
            AddHandler(value As EventHandler(Of AutoCompleteEventArgs))
                [AddHandler](CompleteEvent, value)
            End AddHandler
            RemoveHandler(value As EventHandler(Of AutoCompleteEventArgs))
                [RemoveHandler](CompleteEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As AutoCompleteEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Quand une suggestion est choisie.</summary>
        Public Custom Event ItemSelect As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                [AddHandler](ItemSelectEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                [RemoveHandler](ItemSelectEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _liste IsNot Nothing Then RemoveHandler _liste.PreviewMouseLeftButtonUp, AddressOf Liste_Clic
            If _popup IsNot Nothing Then RemoveHandler _popup.Closed, AddressOf Popup_Closed
            If _bouton IsNot Nothing Then RemoveHandler _bouton.Click, AddressOf Bouton_Click
            MyBase.OnApplyTemplate()
            _liste = TryCast(GetTemplateChild("PART_List"), System.Windows.Controls.ListBox)
            _popup = TryCast(GetTemplateChild("PART_Popup"), Popup)
            _bouton = TryCast(GetTemplateChild("PART_DropDown"), ButtonBase)
            If _liste IsNot Nothing Then
                AddHandler _liste.PreviewMouseLeftButtonUp, AddressOf Liste_Clic
                _liste.ItemsSource = Suggestions
            End If
            If _popup IsNot Nothing Then AddHandler _popup.Closed, AddressOf Popup_Closed
            If _bouton IsNot Nothing Then AddHandler _bouton.Click, AddressOf Bouton_Click
        End Sub

        ''' <summary>La frappe relance l'attente ; après <c>Delay</c> ms, les suggestions sont demandées.</summary>
        Protected Overrides Sub OnTextChanged(e As TextChangedEventArgs)
            MyBase.OnTextChanged(e)
            If _texteInterne Then Return
            ' Retaper après un choix le défait : la valeur redevient le texte, comme mkm.
            If SelectedItem IsNot Nothing Then SetCurrentValue(SelectedItemProperty, Nothing)
            _attente.Stop()
            If Text.Length < MinLength Then
                SetCurrentValue(IsDropDownOpenProperty, False)
                Return
            End If
            _attente.Interval = TimeSpan.FromMilliseconds(Math.Max(0, Delay))
            _attente.Start()
        End Sub

        Private Sub Attente_Tick(sender As Object, e As EventArgs)
            _attente.Stop()
            Demander(Text)
        End Sub

        ''' <summary>Demande les suggestions de <paramref name="texte"/> : <c>Complete</c>, comme après la frappe.</summary>
        Public Sub Search(texte As String)
            Demander(If(texte, String.Empty))
        End Sub

        Private Sub Demander(texte As String)
            _demande = True
            Me.RaiseEvent(New AutoCompleteEventArgs(CompleteEvent, Me, texte))
        End Sub

        ' Le bouton : toutes les suggestions (dropdownMode « blank » de mkm), ou la liste refermée.
        Private Sub Bouton_Click(sender As Object, e As RoutedEventArgs)
            If IsDropDownOpen Then
                SetCurrentValue(IsDropDownOpenProperty, False)
            Else
                Focus()
                Demander(String.Empty)
            End If
        End Sub

        Private Shared Sub OnSuggestionsChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, AutoComplete)
            ' Une liste observable remplie sur place compte aussi (abonnement faible : elle ne retient pas le champ).
            Dim ancienne = TryCast(e.OldValue, System.Collections.Specialized.INotifyCollectionChanged)
            If ancienne IsNot Nothing Then System.Collections.Specialized.CollectionChangedEventManager.RemoveHandler(ancienne, AddressOf champ.Suggestions_CollectionChanged)
            Dim nouvelle = TryCast(e.NewValue, System.Collections.Specialized.INotifyCollectionChanged)
            If nouvelle IsNot Nothing Then System.Collections.Specialized.CollectionChangedEventManager.AddHandler(nouvelle, AddressOf champ.Suggestions_CollectionChanged)
            If champ._liste IsNot Nothing Then
                champ._liste.ItemsSource = champ.Suggestions
                champ._liste.SelectedIndex = -1
            End If
            champ.Relire()
        End Sub

        Private Sub Suggestions_CollectionChanged(sender As Object, e As System.Collections.Specialized.NotifyCollectionChangedEventArgs)
            Relire()
        End Sub

        ' Le nombre de suggestions ; la réponse à une demande ouvre la liste, s'il y a quoi montrer.
        Private Sub Relire()
            Dim nombre = If(Suggestions Is Nothing, 0, Suggestions.Cast(Of Object)().Count())
            SetValue(HasSuggestionsPropertyKey, nombre > 0)
            If _demande AndAlso IsKeyboardFocusWithin Then
                _demande = False
                SetCurrentValue(IsDropDownOpenProperty, nombre > 0 OrElse ShowEmptyMessage)
            End If
        End Sub

        Private Shared Sub OnIsDropDownOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, AutoComplete)
            If CBool(e.NewValue) Then champ._fermeture.Ecouter() Else champ._fermeture.Cesser()
            ' Déplié ou replié, dit aux lecteurs d'écran (aria-expanded de mkm).
            If AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged) Then
                UIElementAutomationPeer.FromElement(champ)?.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                    If(CBool(e.OldValue), ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed),
                    If(CBool(e.NewValue), ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed))
            End If
        End Sub

        Private Sub Popup_Closed(sender As Object, e As EventArgs)
            SetCurrentValue(IsDropDownOpenProperty, False)
        End Sub

        ''' <summary>Le texte d'une suggestion : sa propriété <c>DisplayMemberPath</c>, sinon son texte.</summary>
        Friend Function TexteDe(suggestion As Object) As String
            If suggestion IsNot Nothing AndAlso Not String.IsNullOrEmpty(DisplayMemberPath) Then
                Dim propriete = TypeDescriptor.GetProperties(suggestion).Find(DisplayMemberPath, True)
                If propriete IsNot Nothing Then Return If(propriete.GetValue(suggestion)?.ToString(), String.Empty)
            End If
            Return If(suggestion?.ToString(), String.Empty)
        End Function

        ''' <summary>Choisit une suggestion : son texte dans le champ, <c>SelectedItem</c>, <c>ItemSelect</c>, la liste refermée.</summary>
        Public Sub SelectItem(suggestion As Object)
            _attente.Stop()
            _texteInterne = True
            Try
                Text = TexteDe(suggestion)
                CaretIndex = Text.Length
            Finally
                _texteInterne = False
            End Try
            SetCurrentValue(SelectedItemProperty, suggestion)
            SetCurrentValue(IsDropDownOpenProperty, False)
            Me.RaiseEvent(New RoutedEventArgs(ItemSelectEvent, Me))
        End Sub

        Private Sub Liste_Clic(sender As Object, e As MouseButtonEventArgs)
            Dim element = ItemsControl.ContainerFromElement(_liste, TryCast(e.OriginalSource, DependencyObject))
            If element Is Nothing Then Return
            SelectItem(_liste.ItemContainerGenerator.ItemFromContainer(element))
            e.Handled = True
        End Sub

        ''' <summary>↓ ↑ parcourent les suggestions, Entrée et Tab choisissent, Échap referme la liste.</summary>
        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            ' Avec Alt, WPF donne Key.System : la vraie touche est dans SystemKey.
            Dim touche = If(e.Key = Key.System, e.SystemKey, e.Key)
            Dim alt = (Keyboard.Modifiers And ModifierKeys.Alt) = ModifierKeys.Alt
            If Not IsDropDownOpen OrElse _liste Is Nothing Then
                If touche = Key.Down AndAlso alt AndAlso Dropdown Then
                    Demander(String.Empty)
                    e.Handled = True
                End If
                MyBase.OnPreviewKeyDown(e)
                Return
            End If
            Dim nombre = _liste.Items.Count
            If touche = Key.Up AndAlso alt Then
                ' Alt+↑ : la suggestion surlignée choisie, la liste refermée.
                If _liste.SelectedIndex >= 0 Then SelectItem(_liste.SelectedItem) Else SetCurrentValue(IsDropDownOpenProperty, False)
                e.Handled = True
                MyBase.OnPreviewKeyDown(e)
                Return
            End If
            Select Case touche
                Case Key.Down
                    If nombre > 0 Then Surligner(Math.Min(nombre - 1, _liste.SelectedIndex + 1))
                    e.Handled = True
                Case Key.Up
                    ' Sans suggestion surlignée, ↑ va à la dernière, comme mkm.
                    If nombre > 0 Then Surligner(If(_liste.SelectedIndex < 0, nombre - 1, Math.Max(0, _liste.SelectedIndex - 1)))
                    e.Handled = True
                Case Key.Home, Key.End
                    ' Début et Fin restent ceux du texte.
                Case Key.Enter, Key.Tab
                    If _liste.SelectedIndex >= 0 Then
                        SelectItem(_liste.SelectedItem)
                        e.Handled = e.Key = Key.Enter
                    Else
                        SetCurrentValue(IsDropDownOpenProperty, False)
                    End If
                Case Key.Escape
                    SetCurrentValue(IsDropDownOpenProperty, False)
                    e.Handled = True
            End Select
            MyBase.OnPreviewKeyDown(e)
        End Sub

        Private Sub Surligner(indice As Integer)
            _liste.SelectedIndex = indice
            _liste.ScrollIntoView(_liste.SelectedItem)
            ' La suggestion surlignée est annoncée (aria-activedescendant de mkm) : le clavier reste au champ.
            Dim conteneur = TryCast(_liste.ItemContainerGenerator.ContainerFromIndex(indice), UIElement)
            If conteneur IsNot Nothing AndAlso AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) Then
                UIElementAutomationPeer.CreatePeerForElement(conteneur)?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected)
            End If
        End Sub

        ''' <summary>Une liste déroulante pour les lecteurs d'écran (role="combobox" de mkm) : sa valeur, et elle se déplie.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDAutoComplete(Me)
        End Function

        Friend Sub Deplier()
            If IsDropDownOpen Then Return
            Focus()
            Demander(If(Dropdown, String.Empty, Text))
        End Sub

        Private NotInheritable Class PairDAutoComplete
            Inherits TextBoxAutomationPeer
            Implements IExpandCollapseProvider

            Public Sub New(champ As AutoComplete)
                MyBase.New(champ)
            End Sub

            Private ReadOnly Property Champ As AutoComplete
                Get
                    Return DirectCast(Owner, AutoComplete)
                End Get
            End Property

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.ComboBox
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(AutoComplete)
            End Function

            Protected Overrides Function GetNameCore() As String
                Return Nommer(MyBase.GetNameCore(), Champ.Placeholder)
            End Function

            Protected Overrides Function GetItemStatusCore() As String
                Return EtatDuChamp(Owner, MyBase.GetItemStatusCore())
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.ExpandCollapse Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
                Get
                    Return If(Champ.IsDropDownOpen, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
                End Get
            End Property

            Public Sub Expand() Implements IExpandCollapseProvider.Expand
                Champ.Deplier()
            End Sub

            Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
                Champ.SetCurrentValue(IsDropDownOpenProperty, False)
            End Sub

        End Class

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnLostKeyboardFocus(e)
            If Not IsKeyboardFocusWithin Then
                _attente.Stop()
                SetCurrentValue(IsDropDownOpenProperty, False)
            End If
        End Sub

    End Class

End Namespace
