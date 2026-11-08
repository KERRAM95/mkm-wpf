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
            If champ._liste IsNot Nothing Then
                champ._liste.ItemsSource = champ.Suggestions
                champ._liste.SelectedIndex = -1
            End If
            Dim nombre = If(champ.Suggestions Is Nothing, 0, champ.Suggestions.Cast(Of Object)().Count())
            champ.SetValue(HasSuggestionsPropertyKey, nombre > 0)
            ' La réponse à une demande ouvre la liste, s'il y a quoi montrer.
            If champ._demande AndAlso champ.IsKeyboardFocusWithin Then
                champ._demande = False
                champ.SetCurrentValue(IsDropDownOpenProperty, nombre > 0 OrElse champ.ShowEmptyMessage)
            End If
        End Sub

        Private Shared Sub OnIsDropDownOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, AutoComplete)
            If CBool(e.NewValue) Then champ._fermeture.Ecouter() Else champ._fermeture.Cesser()
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
            If Not IsDropDownOpen OrElse _liste Is Nothing Then
                If e.Key = Key.Down AndAlso Keyboard.Modifiers = ModifierKeys.Alt AndAlso Dropdown Then
                    Demander(String.Empty)
                    e.Handled = True
                End If
                MyBase.OnPreviewKeyDown(e)
                Return
            End If
            Dim nombre = _liste.Items.Count
            Select Case e.Key
                Case Key.Down
                    If nombre > 0 Then Surligner(Math.Min(nombre - 1, _liste.SelectedIndex + 1))
                    e.Handled = True
                Case Key.Up
                    If nombre > 0 Then Surligner(Math.Max(0, _liste.SelectedIndex - 1))
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
        End Sub

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
