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
    <TemplatePart(Name:="PART_ToggleAll", Type:=GetType(System.Windows.Controls.Primitives.ToggleButton))>
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

        ''' <summary>
        ''' Vrai sur l'option sous le clavier, quand la liste est ouverte : grisée comme au survol. Le clavier reste au
        ''' champ ; ↓ ↑ Début Fin déplacent cette option, Espace et Entrée l'ajoutent ou la retirent.
        ''' </summary>
        Public Shared ReadOnly IsOptionFocusedProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "IsOptionFocused", GetType(Boolean), GetType(MultiSelect), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai sur l'option sous le clavier.</summary>
        Public Shared Function GetIsOptionFocused(element As DependencyObject) As Boolean
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return CBool(element.GetValue(IsOptionFocusedProperty))
        End Function

        ''' <summary>Réservé à la bibliothèque, qui déplace l'option sous le clavier.</summary>
        Public Shared Sub SetIsOptionFocused(element As DependencyObject, value As Boolean)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(IsOptionFocusedProperty, value)
        End Sub

        Private Shared ReadOnly HasSelectionPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasSelection), GetType(Boolean), GetType(MultiSelect), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand au moins une option est choisie.</summary>
        Public Shared ReadOnly HasSelectionProperty As DependencyProperty = HasSelectionPropertyKey.DependencyProperty

        Private ReadOnly _puces As New ObservableCollection(Of String)
        Private ReadOnly _pucesLues As ReadOnlyObservableCollection(Of String)
        Private _rangeeDePuces As ItemsControl
        Private _popup As Popup
        Private _toutCocher As System.Windows.Controls.Primitives.ToggleButton
        ' L'option sous le clavier (focusedOptionIndex de mkm) : le clavier, lui, reste au champ.
        Private _indice As Integer = -1
        Private _champ As ButtonBase
        Private _filtreDuGabarit As TextBox
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
            _filtreDuGabarit = TryCast(GetTemplateChild("PART_Filter"), TextBox)
            If _champ IsNot Nothing Then AddHandler _champ.Click, AddressOf Champ_Click
            _popup = TryCast(GetTemplateChild("PART_Popup"), Popup)
            _toutCocher = TryCast(GetTemplateChild("PART_ToggleAll"), System.Windows.Controls.Primitives.ToggleButton)
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
            ' La case de tête : cochée quand toutes les options montrées (filtrées) sont choisies, la règle de son clic.
            If _toutCocher IsNot Nothing Then _toutCocher.IsChecked = Items.Count > 0 AndAlso Items.Cast(Of Object)().All(Function(o) SelectedItems.Contains(o))
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
                ' L'option sous le clavier : la première choisie, sinon la première, comme mkm.
                liste._indice = If(liste.SelectedItems.Count > 0, liste.Items.IndexOf(liste.SelectedItems(0)), 0)
                liste.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, New System.Action(Sub() liste.Surligner(liste._indice)))
            Else
                liste._fermeture.Cesser()
                liste.Surligner(-1)
            End If
            TryCast(UIElementAutomationPeer.FromElement(liste), PairDeMultiSelect)?.Annoncer(Not CBool(e.NewValue), CBool(e.NewValue))
        End Sub

        ' Déplace l'option sous le clavier, et la montre ; -1 : aucune.
        Private Sub Surligner(indice As Integer)
            For i = 0 To Items.Count - 1
                Dim conteneur = ItemContainerGenerator.ContainerFromIndex(i)
                If conteneur IsNot Nothing Then conteneur.ClearValue(IsOptionFocusedProperty)
            Next
            _indice = If(Items.Count = 0, -1, Math.Min(indice, Items.Count - 1))
            If _indice < 0 OrElse Not IsDropDownOpen Then Return
            ScrollIntoView(Items(_indice))
            ItemContainerGenerator.ContainerFromIndex(_indice)?.SetValue(IsOptionFocusedProperty, True)
        End Sub

        ' Ajoute ou retire l'option.
        Private Sub Basculer(choix As Object)
            If SelectedItems.Contains(choix) Then SelectedItems.Remove(choix) Else SelectedItems.Add(choix)
        End Sub

        ' Un clic sur une option l'ajoute ou la retire ; le clavier reste au champ.
        Protected Overrides Sub OnPreviewMouseLeftButtonUp(e As MouseButtonEventArgs)
            MyBase.OnPreviewMouseLeftButtonUp(e)
            Dim conteneur = TryCast(ContainerFromElement(Me, TryCast(e.OriginalSource, DependencyObject)), ListBoxItem)
            If conteneur Is Nothing OrElse Not conteneur.IsEnabled Then Return
            Dim choix = ItemContainerGenerator.ItemFromContainer(conteneur)
            Basculer(choix)
            Surligner(Items.IndexOf(choix))
            e.Handled = True
        End Sub

        ' Une option ne prend pas le clic à la souris : le MultiSelect la bascule au relâché.
        Protected Overrides Sub OnPreviewMouseLeftButtonDown(e As MouseButtonEventArgs)
            MyBase.OnPreviewMouseLeftButtonDown(e)
            If TypeOf ContainerFromElement(Me, TryCast(e.OriginalSource, DependencyObject)) Is ListBoxItem Then e.Handled = True
        End Sub

        ' Dans le filtre : ↓ ↑ déplacent l'option sous le clavier, Entrée la bascule ; le texte garde le reste.
        Friend Overrides Sub ToucheDuFiltre(e As KeyEventArgs)
            Select Case e.Key
                Case Key.Down
                    Surligner(Math.Min(Items.Count - 1, _indice + 1))
                    e.Handled = True
                Case Key.Up
                    Surligner(Math.Max(0, _indice - 1))
                    e.Handled = True
                Case Key.Enter
                    If _indice >= 0 AndAlso _indice < Items.Count Then Basculer(Items(_indice))
                    e.Handled = True
            End Select
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
        ''' Fermée, ↓, Alt+↓, Entrée et Espace ouvrent la liste ; ouverte, ↓ ↑ Début Fin déplacent l'option sous le
        ''' clavier, Espace et Entrée l'ajoutent ou la retirent, Ctrl+A choisit tout ou retire tout, Échap et Tab la
        ''' ferment (comme mkm : le clavier reste au champ).
        ''' </summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            Dim touche = If(e.Key = Key.System, e.SystemKey, e.Key)
            If Not IsDropDownOpen Then
                ' ↑ ouvre aussi, comme mkm.
                If touche = Key.Down OrElse touche = Key.Up OrElse touche = Key.Enter OrElse touche = Key.Space OrElse touche = Key.F4 Then
                    SetCurrentValue(IsDropDownOpenProperty, True)
                    e.Handled = True
                End If
                Return
            End If
            ' Dans le filtre, les touches écrivent : seules Échap et Tab y ferment la liste (↓ ↑ Entrée : ToucheDuFiltre).
            Dim nombre = Items.Count
            If e.OriginalSource Is _filtreDuGabarit Then touche = If(touche = Key.Escape OrElse touche = Key.Tab, touche, Key.None)
            Select Case touche
                Case Key.Down
                    If Keyboard.Modifiers <> ModifierKeys.Alt Then
                        Surligner(Math.Min(nombre - 1, _indice + 1))
                        e.Handled = True
                        Return
                    End If
                Case Key.Up
                    If Keyboard.Modifiers <> ModifierKeys.Alt Then
                        Surligner(Math.Max(0, _indice - 1))
                        e.Handled = True
                        Return
                    End If
                Case Key.Home
                    Surligner(0)
                    e.Handled = True
                    Return
                Case Key.End
                    Surligner(nombre - 1)
                    e.Handled = True
                    Return
                Case Key.Space, Key.Enter
                    If _indice >= 0 AndAlso _indice < nombre Then Basculer(Items(_indice))
                    e.Handled = True
                    Return
                Case Key.A
                    If Keyboard.Modifiers = ModifierKeys.Control Then
                        ToutCocher_Click(Me, e)
                        e.Handled = True
                        Return
                    End If
            End Select
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
