Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Threading
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>Comment un <see cref="MultiSelect"/> montre ses choix dans son champ.</summary>
    Public Enum MultiSelectDisplay
        ''' <summary>Leurs textes, séparés de virgules.</summary>
        Comma
        ''' <summary>Une puce par choix, qui se retire d'un clic sur sa croix.</summary>
        Chip
    End Enum

    ''' <summary>
    ''' La liste déroulante à choix multiples de mkm (MultiSelect) : un champ qui montre les choix, et une liste qui
    ''' s'ouvre dessous, une case devant chaque option, « tout sélectionner » et un filtre en tête. Ses choix sont ses
    ''' <c>SelectedItems</c> ; c'est un <see cref="Listbox"/> dont la liste s'ouvre sous un champ.
    ''' <code>
    ''' &lt;mkm:MultiSelect Placeholder="Maturités" ItemsSource="{Binding Maturites}" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Popup", Type:=GetType(Popup))>
    <TemplatePart(Name:="PART_Field", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_ToggleAll", Type:=GetType(ToggleButton))>
    Public Class MultiSelect
        Inherits Listbox

        ''' <summary>Le texte gris du champ, tant que rien n'est choisi.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Placeholder), GetType(String), GetType(MultiSelect), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Comment le champ montre les choix : leurs textes (par défaut) ou des puces.</summary>
        Public Shared ReadOnly DisplayProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Display), GetType(MultiSelectDisplay), GetType(MultiSelect),
            New FrameworkPropertyMetadata(MultiSelectDisplay.Comma, AddressOf OnAffichageChanged))

        ''' <summary>Au-delà de ce nombre de choix, le champ dit « n éléments sélectionnés » ; 0 : tous les textes.</summary>
        Public Shared ReadOnly MaxSelectedLabelsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MaxSelectedLabels), GetType(Integer), GetType(MultiSelect), New FrameworkPropertyMetadata(0, AddressOf OnAffichageChanged))

        ''' <summary>Vrai pour la case « tout sélectionner » en tête de la liste.</summary>
        Public Shared ReadOnly ShowToggleAllProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowToggleAll), GetType(Boolean), GetType(MultiSelect), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai quand la liste est ouverte. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsDropDownOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsDropDownOpen), GetType(Boolean), GetType(MultiSelect),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsDropDownOpenChanged))

        Private Shared ReadOnly SelectionTextPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(SelectionText), GetType(String), GetType(MultiSelect), New FrameworkPropertyMetadata(String.Empty))
        ''' <summary>Ce que le champ montre : les textes des choix, ou « n éléments sélectionnés » ; vide sans choix.</summary>
        Public Shared ReadOnly SelectionTextProperty As DependencyProperty = SelectionTextPropertyKey.DependencyProperty

        Private Shared ReadOnly HasSelectionPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasSelection), GetType(Boolean), GetType(MultiSelect), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand au moins une option est choisie.</summary>
        Public Shared ReadOnly HasSelectionProperty As DependencyProperty = HasSelectionPropertyKey.DependencyProperty

        Private ReadOnly _puces As New ObservableCollection(Of String)
        Private ReadOnly _pucesLues As ReadOnlyObservableCollection(Of String)
        Private _rangeeDePuces As ItemsControl
        Private _popup As Popup
        Private _toutCocher As ToggleButton
        Private _champ As ButtonBase
        ' Le moment où un clic sur le champ a fermé la liste : le Click de ce même clic ne doit pas la rouvrir.
        Private _fermeeSurLeChamp As Date = Date.MinValue
        Private ReadOnly _fermeture As FermetureAuDefilement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(MultiSelect), New FrameworkPropertyMetadata(GetType(MultiSelect)))
            SelectionModeProperty.OverrideMetadata(GetType(MultiSelect), New FrameworkPropertyMetadata(SelectionMode.Multiple))
        End Sub

        ''' <summary>Une liste à choix multiples vide.</summary>
        Public Sub New()
            _pucesLues = New ReadOnlyObservableCollection(Of String)(_puces)
            _fermeture = New FermetureAuDefilement(Me, Sub() SetCurrentValue(IsDropDownOpenProperty, False))
            AddHandler Loaded, Sub() AddHandler Localizer.Current.LanguageChanged, AddressOf Langue_Changee
            AddHandler Unloaded, Sub() RemoveHandler Localizer.Current.LanguageChanged, AddressOf Langue_Changee
            [AddHandler](Chip.RemoveEvent, New RoutedEventHandler(AddressOf Puce_Remove))
        End Sub

        ''' <summary>Le texte gris du champ.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Comment le champ montre les choix.</summary>
        Public Property Display As MultiSelectDisplay
            Get
                Return DirectCast(GetValue(DisplayProperty), MultiSelectDisplay)
            End Get
            Set(value As MultiSelectDisplay)
                SetValue(DisplayProperty, value)
            End Set
        End Property

        ''' <summary>Au-delà de ce nombre de choix, le champ les compte.</summary>
        Public Property MaxSelectedLabels As Integer
            Get
                Return CInt(GetValue(MaxSelectedLabelsProperty))
            End Get
            Set(value As Integer)
                SetValue(MaxSelectedLabelsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour la case « tout sélectionner ».</summary>
        Public Property ShowToggleAll As Boolean
            Get
                Return CBool(GetValue(ShowToggleAllProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowToggleAllProperty, value)
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

        ''' <summary>Ce que le champ montre.</summary>
        Public ReadOnly Property SelectionText As String
            Get
                Return CStr(GetValue(SelectionTextProperty))
            End Get
        End Property

        ''' <summary>Vrai quand au moins une option est choisie.</summary>
        Public ReadOnly Property HasSelection As Boolean
            Get
                Return CBool(GetValue(HasSelectionProperty))
            End Get
        End Property

        ''' <summary>Les textes des choix montrés en puces (<c>Display="Chip"</c>), dans l'ordre où ils ont été faits.</summary>
        Public ReadOnly Property SelectedChips As ReadOnlyObservableCollection(Of String)
            Get
                Return _pucesLues
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _toutCocher IsNot Nothing Then RemoveHandler _toutCocher.Click, AddressOf ToutCocher_Click
            If _popup IsNot Nothing Then RemoveHandler _popup.Closed, AddressOf Popup_Closed
            If _champ IsNot Nothing Then RemoveHandler _champ.Click, AddressOf Champ_Click
            MyBase.OnApplyTemplate()
            _champ = TryCast(GetTemplateChild("PART_Field"), ButtonBase)
            If _champ IsNot Nothing Then AddHandler _champ.Click, AddressOf Champ_Click
            _popup = TryCast(GetTemplateChild("PART_Popup"), Popup)
            _toutCocher = TryCast(GetTemplateChild("PART_ToggleAll"), ToggleButton)
            _rangeeDePuces = TryCast(GetTemplateChild("Puces"), ItemsControl)
            If _toutCocher IsNot Nothing Then AddHandler _toutCocher.Click, AddressOf ToutCocher_Click
            If _popup IsNot Nothing Then AddHandler _popup.Closed, AddressOf Popup_Closed
            Decrire()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnSelectionChanged(e As SelectionChangedEventArgs)
            MyBase.OnSelectionChanged(e)
            Decrire()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Decrire()
        End Sub

        Private Shared Sub OnAffichageChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, MultiSelect).Decrire()
        End Sub

        Private Sub Langue_Changee(sender As Object, e As EventArgs)
            Decrire()
        End Sub

        ' Le texte du champ, ses puces, et la case « tout sélectionner ».
        Private Sub Decrire()
            Dim choix = SelectedItems.Cast(Of Object)().ToList()
            SetValue(HasSelectionPropertyKey, choix.Count > 0)
            Dim texte = If(MaxSelectedLabels > 0 AndAlso choix.Count > MaxSelectedLabels,
                           Localizer.Current.T("locale.selectionMessage", choix.Count),
                           String.Join(", ", choix.Select(Function(c) TexteDe(c))))
            SetValue(SelectionTextPropertyKey, texte)
            _puces.Clear()
            For Each c In choix
                _puces.Add(TexteDe(c))
            Next
            If _toutCocher IsNot Nothing Then _toutCocher.IsChecked = Items.Count > 0 AndAlso choix.Count >= Items.Count
        End Sub

        ' La croix d'une puce retire son choix.
        Private Sub Puce_Remove(sender As Object, e As RoutedEventArgs)
            Dim puce = TryCast(e.OriginalSource, DependencyObject)
            If puce Is Nothing OrElse _rangeeDePuces Is Nothing Then Return
            Dim conteneur = ItemsControl.ContainerFromElement(_rangeeDePuces, puce)
            Dim indice = If(conteneur Is Nothing, -1, _rangeeDePuces.ItemContainerGenerator.IndexFromContainer(conteneur))
            If indice >= 0 AndAlso indice < SelectedItems.Count Then SelectedItems.RemoveAt(indice)
            e.Handled = True
        End Sub

        ' Tout choisir, ou tout retirer quand tout est choisi ; avec un filtre, les options montrées.
        Private Sub ToutCocher_Click(sender As Object, e As RoutedEventArgs)
            If Items.Cast(Of Object)().All(Function(o) SelectedItems.Contains(o)) Then
                For Each o In Items.Cast(Of Object)().ToList()
                    SelectedItems.Remove(o)
                Next
            Else
                SelectAll()
            End If
            Decrire()
        End Sub

        Private Shared Sub OnIsDropDownOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim liste = DirectCast(d, MultiSelect)
            If CBool(e.NewValue) Then
                liste._fermeture.Ecouter()
                ' Le clavier passe à l'option choisie, sinon à la première, comme mkm.
                liste.Dispatcher.BeginInvoke(DispatcherPriority.Input, New System.Action(AddressOf liste.EntrerDansLaListe))
            Else
                liste._fermeture.Cesser()
            End If
            TryCast(UIElementAutomationPeer.FromElement(liste), PairDeMultiSelect)?.Annoncer(Not CBool(e.NewValue), CBool(e.NewValue))
        End Sub

        Private Sub EntrerDansLaListe()
            If Not IsDropDownOpen OrElse Items.Count = 0 Then Return
            Dim indice = If(SelectedIndex >= 0, Items.IndexOf(SelectedItems(0)), 0)
            Dim option0 = TryCast(ItemContainerGenerator.ContainerFromIndex(Math.Max(0, indice)), ListBoxItem)
            If option0 Is Nothing Then
                UpdateLayout()
                option0 = TryCast(ItemContainerGenerator.ContainerFromIndex(Math.Max(0, indice)), ListBoxItem)
            End If
            option0?.Focus()
        End Sub

        ' Un clic sur le champ ouvre ou ferme la liste.
        Private Sub Champ_Click(sender As Object, e As RoutedEventArgs)
            If Not IsKeyboardFocusWithin Then Focus()
            If Not IsDropDownOpen AndAlso (Date.UtcNow - _fermeeSurLeChamp).TotalMilliseconds < 400 Then Return
            SetCurrentValue(IsDropDownOpenProperty, Not IsDropDownOpen)
        End Sub

        Private Sub Popup_Closed(sender As Object, e As EventArgs)
            If _champ IsNot Nothing AndAlso _champ.IsMouseOver AndAlso Mouse.LeftButton = MouseButtonState.Pressed Then
                _fermeeSurLeChamp = Date.UtcNow
            End If
            SetCurrentValue(IsDropDownOpenProperty, False)
        End Sub

        ''' <summary>
        ''' Fermée, ↓, Alt+↓, Entrée et Espace ouvrent la liste ; ouverte, Échap et Tab la ferment. Les flèches et Espace
        ''' dans la liste sont ceux d'un ListBox à choix multiples : Espace ajoute ou retire l'option.
        ''' </summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            Dim touche = If(e.Key = Key.System, e.SystemKey, e.Key)
            If Not IsDropDownOpen Then
                If touche = Key.Down OrElse touche = Key.Enter OrElse touche = Key.Space OrElse touche = Key.F4 Then
                    SetCurrentValue(IsDropDownOpenProperty, True)
                    e.Handled = True
                ElseIf touche = Key.Up Then
                    e.Handled = True
                End If
                Return
            End If
            If touche = Key.Escape OrElse touche = Key.Tab OrElse (touche = Key.Up AndAlso Keyboard.Modifiers = ModifierKeys.Alt) Then
                SetCurrentValue(IsDropDownOpenProperty, False)
                Focus()
                ' Tab ferme la liste et passe à l'élément suivant (ou précédent), comme mkm.
                If touche = Key.Tab Then
                    MoveFocus(New TraversalRequest(If(Keyboard.Modifiers = ModifierKeys.Shift, FocusNavigationDirection.Previous, FocusNavigationDirection.Next)))
                End If
                e.Handled = True
                Return
            End If
            MyBase.OnKeyDown(e)
        End Sub

        ''' <summary>Une liste déroulante pour les lecteurs d'écran : son texte, développée ou réduite, ses options cochées.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeMultiSelect(Me)
        End Function

        ''' <summary>Le pair d'un MultiSelect : une liste déroulante (ComboBox) à choix multiples.</summary>
        Private NotInheritable Class PairDeMultiSelect
            Inherits ListBoxAutomationPeer
            Implements IExpandCollapseProvider, IValueProvider

            Public Sub New(liste As MultiSelect)
                MyBase.New(liste)
            End Sub

            Private ReadOnly Property Liste As MultiSelect
                Get
                    Return DirectCast(Owner, MultiSelect)
                End Get
            End Property

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.ComboBox
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(MultiSelect)
            End Function

            Protected Overrides Function GetNameCore() As String
                Dim nom = MyBase.GetNameCore()
                Return If(String.IsNullOrEmpty(nom), If(Liste.Placeholder, String.Empty), nom)
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.ExpandCollapse OrElse patternInterface = PatternInterface.Value Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
                Get
                    Return If(Liste.IsDropDownOpen, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
                End Get
            End Property

            Public Sub Expand() Implements IExpandCollapseProvider.Expand
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                Liste.SetCurrentValue(IsDropDownOpenProperty, True)
            End Sub

            Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                Liste.SetCurrentValue(IsDropDownOpenProperty, False)
            End Sub

            Public ReadOnly Property Value As String Implements IValueProvider.Value
                Get
                    Return Liste.SelectionText
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

        End Class

    End Class

End Namespace
