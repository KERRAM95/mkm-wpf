Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Input
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La barre de commandes de mkm, à la manière de 3DEXPERIENCE : des sections choisies par des
    ''' onglets, des commandes en icônes, des commandes fixes en tête de chaque section ; elle se replie,
    ''' montre ou cache ses libellés (clic droit), et défile quand elle manque de place.
    ''' <code>
    ''' &lt;mkm:ActionBar ActiveSection="edition"&gt;
    '''     &lt;mkm:ActionBar.FixedItems&gt;
    '''         &lt;mkm:ActionBarButton Label="Enregistrer" Icon="{x:Static mkm:Icons.Download}" Command="{Binding Enregistrer}" /&gt;
    '''     &lt;/mkm:ActionBar.FixedItems&gt;
    '''     &lt;mkm:ActionBarSection Key="edition" Header="Édition"&gt;
    '''         &lt;mkm:ActionBarButton … /&gt;
    '''     &lt;/mkm:ActionBarSection&gt;
    ''' &lt;/mkm:ActionBar&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Elle se pose en bas de son conteneur, centrée, comme dans 3DEXPERIENCE ; ses listes s'ouvrent
    ''' au-dessus quand la place le permet. Au clavier, toute la rangée n'est qu'un arrêt de tabulation
    ''' (la dernière commande qui a eu le focus) ; les flèches, Début et Fin s'y déplacent.
    ''' </remarks>
    <TemplatePart(Name:="PART_Commands", Type:=GetType(ItemsControl))>
    <TemplatePart(Name:="PART_Scroll", Type:=GetType(ScrollViewer))>
    <TemplatePart(Name:="PART_Previous", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Next", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Toggle", Type:=GetType(ButtonBase))>
    Public Class ActionBar
        Inherits Selector

        ' Une flèche avance des trois quarts de la largeur visible : le dernier quart reste en vue (mkm).
        Private Const RatioDeDefilement As Double = 0.75

        ''' <summary>La clé du style des info-bulles des commandes.</summary>
        Public Shared ReadOnly Property ToolTipStyleKey As ComponentResourceKey = New ComponentResourceKey(GetType(ActionBar), "ToolTip")

        ''' <summary>La clé du style du menu du clic droit.</summary>
        Public Shared ReadOnly Property ContextMenuStyleKey As ComponentResourceKey = New ComponentResourceKey(GetType(ActionBar), "ContextMenu")

        ''' <summary>La clé du style des choix du menu du clic droit.</summary>
        Public Shared ReadOnly Property MenuItemStyleKey As ComponentResourceKey = New ComponentResourceKey(GetType(ActionBar), "MenuItem")

        ''' <summary>La clé de la section montrée ; sans elle, ou si elle n'en désigne aucune, la première.</summary>
        Public Shared ReadOnly ActiveSectionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ActiveSection), GetType(String), GetType(ActionBar),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnActiveSectionChanged))

        ''' <summary>Vrai quand la barre est repliée : seul son en-tête reste.</summary>
        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsCollapsed), GetType(Boolean), GetType(ActionBar),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>
        ''' Vrai quand les libellés sont montrés sous les icônes ; le clic droit sur la barre le change.
        ''' Hérité par les commandes de la barre.
        ''' </summary>
        Public Shared ReadOnly ShowLabelsProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "ShowLabels", GetType(Boolean), GetType(ActionBar),
            New FrameworkPropertyMetadata(False,
                                          FrameworkPropertyMetadataOptions.Inherits Or
                                          FrameworkPropertyMetadataOptions.BindsTwoWayByDefault Or
                                          FrameworkPropertyMetadataOptions.AffectsMeasure,
                                          AddressOf ActionBarItem.Rafraichir))

        Private Shared ReadOnly HasTabsPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasTabs), GetType(Boolean), GetType(ActionBar), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai quand la barre a plusieurs sections visibles : elle montre alors leurs onglets.</summary>
        Public Shared ReadOnly HasTabsProperty As DependencyProperty = HasTabsPropertyKey.DependencyProperty

        Private Shared ReadOnly HasFixedItemsPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasFixedItems), GetType(Boolean), GetType(ActionBar), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai quand la barre a des commandes fixes.</summary>
        Public Shared ReadOnly HasFixedItemsProperty As DependencyProperty = HasFixedItemsPropertyKey.DependencyProperty

        Private Shared ReadOnly HasSectionItemsPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasSectionItems), GetType(Boolean), GetType(ActionBar), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai quand la section montrée a des commandes.</summary>
        Public Shared ReadOnly HasSectionItemsProperty As DependencyProperty = HasSectionItemsPropertyKey.DependencyProperty

        Private Shared ReadOnly IsScrollablePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsScrollable), GetType(Boolean), GetType(ActionBar), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai quand la rangée ne tient pas dans la largeur de la barre : ses flèches paraissent.</summary>
        Public Shared ReadOnly IsScrollableProperty As DependencyProperty = IsScrollablePropertyKey.DependencyProperty

        Private _commandes As ItemsControl
        Private _defilement As ScrollViewer
        Private _parRepli As Boolean
        Private _precedent As ButtonBase
        Private _suivant As ButtonBase
        Private _repli As ButtonBase
        Private ReadOnly _montrer As MenuItem
        Private ReadOnly _cacher As MenuItem

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ActionBar), New FrameworkPropertyMetadata(GetType(ActionBar)))
            FocusableProperty.OverrideMetadata(GetType(ActionBar), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une barre vide ; ses sections s'ajoutent en XAML, comme ses enfants.</summary>
        Public Sub New()
            FixedItems = New ObservableCollection(Of UIElement)()
            AddHandler FixedItems.CollectionChanged, Sub(s, e) MettreAJourEtat()

            ' Le clic droit : montrer ou cacher les libellés, le choix actuel coché.
            Dim menu As New ContextMenu()
            menu.SetResourceReference(StyleProperty, ContextMenuStyleKey)
            _montrer = ChoixDesLibelles("actionBar.showLabels", True)
            _cacher = ChoixDesLibelles("actionBar.hideLabels", False)
            menu.Items.Add(_montrer)
            menu.Items.Add(_cacher)
            ContextMenu = menu
        End Sub

        ''' <summary>Les commandes montrées en tête de chaque section.</summary>
        Public ReadOnly Property FixedItems As ObservableCollection(Of UIElement)

        ''' <summary>La clé de la section montrée.</summary>
        Public Property ActiveSection As String
            Get
                Return CStr(GetValue(ActiveSectionProperty))
            End Get
            Set(value As String)
                SetValue(ActiveSectionProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la barre est repliée.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand les libellés sont montrés sous les icônes.</summary>
        Public Property ShowLabels As Boolean
            Get
                Return CBool(GetValue(ShowLabelsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowLabelsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la barre montre les onglets de ses sections.</summary>
        Public ReadOnly Property HasTabs As Boolean
            Get
                Return CBool(GetValue(HasTabsProperty))
            End Get
        End Property

        ''' <summary>Vrai quand la barre a des commandes fixes.</summary>
        Public ReadOnly Property HasFixedItems As Boolean
            Get
                Return CBool(GetValue(HasFixedItemsProperty))
            End Get
        End Property

        ''' <summary>Vrai quand la section montrée a des commandes.</summary>
        Public ReadOnly Property HasSectionItems As Boolean
            Get
                Return CBool(GetValue(HasSectionItemsProperty))
            End Get
        End Property

        ''' <summary>Vrai quand la rangée défile.</summary>
        Public ReadOnly Property IsScrollable As Boolean
            Get
                Return CBool(GetValue(IsScrollableProperty))
            End Get
        End Property

        ''' <summary>Les libellés sont-ils montrés pour cet élément (hérité de sa barre) ?</summary>
        Public Shared Function GetShowLabels(element As DependencyObject) As Boolean
            Return CBool(element.GetValue(ShowLabelsProperty))
        End Function

        ''' <summary>Montre ou cache les libellés d'une barre.</summary>
        Public Shared Sub SetShowLabels(element As DependencyObject, value As Boolean)
            element.SetValue(ShowLabelsProperty, value)
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _defilement IsNot Nothing Then
                RemoveHandler _defilement.ScrollChanged, AddressOf OnDefilement
                RemoveHandler _defilement.SizeChanged, AddressOf OnDefilement
            End If
            If _precedent IsNot Nothing Then RemoveHandler _precedent.Click, AddressOf OnPrecedent
            If _suivant IsNot Nothing Then RemoveHandler _suivant.Click, AddressOf OnSuivant
            If _repli IsNot Nothing Then RemoveHandler _repli.Click, AddressOf OnRepli

            MyBase.OnApplyTemplate()
            _commandes = TryCast(GetTemplateChild("PART_Commands"), ItemsControl)
            _defilement = TryCast(GetTemplateChild("PART_Scroll"), ScrollViewer)
            _precedent = TryCast(GetTemplateChild("PART_Previous"), ButtonBase)
            _suivant = TryCast(GetTemplateChild("PART_Next"), ButtonBase)
            _repli = TryCast(GetTemplateChild("PART_Toggle"), ButtonBase)

            If _defilement IsNot Nothing Then
                AddHandler _defilement.ScrollChanged, AddressOf OnDefilement
                AddHandler _defilement.SizeChanged, AddressOf OnDefilement
            End If
            If _precedent IsNot Nothing Then AddHandler _precedent.Click, AddressOf OnPrecedent
            If _suivant IsNot Nothing Then AddHandler _suivant.Click, AddressOf OnSuivant
            If _repli IsNot Nothing Then AddHandler _repli.Click, AddressOf OnRepli

            MontrerLaSection()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is ActionBarSection
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New ActionBarSection()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnInitialized(e As EventArgs)
            MyBase.OnInitialized(e)
            ChoisirLaSection()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            ChoisirLaSection()
            MettreAJourEtat()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnSelectionChanged(e As SelectionChangedEventArgs)
            MyBase.OnSelectionChanged(e)
            Dim section = TryCast(SelectedItem, ActionBarSection)
            ' Un repli (la section voulue absente ou cachée) ne réécrit pas la clé voulue : elle reviendra avec sa section.
            If section IsNot Nothing AndAlso Not _parRepli AndAlso Not String.Equals(section.Key, ActiveSection, StringComparison.Ordinal) Then
                SetCurrentValue(ActiveSectionProperty, section.Key)
            End If
            MontrerLaSection()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnContextMenuOpening(e As ContextMenuEventArgs)
            ' Repliée, la barre n'a pas de libellés à montrer : pas de menu.
            If IsCollapsed Then
                e.Handled = True
                Return
            End If
            _montrer.IsChecked = ShowLabels
            _cacher.IsChecked = Not ShowLabels
            MyBase.OnContextMenuOpening(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            MyBase.OnPreviewKeyDown(e)
            ' Avant le défilement de la rangée, qui prendrait Début et Fin pour lui (ScrollViewer.OnKeyDown).
            If e.Handled OrElse (e.Key <> Key.Home AndAlso e.Key <> Key.End) Then Return

            ' Début et Fin : la première et la dernière commande de la rangée.
            Dim cibles = Commandes().Where(Function(c) c.IsVisible AndAlso c.IsEnabled AndAlso c.Focusable).ToList()
            If cibles.Count = 0 OrElse Not cibles.Any(Function(c) c.IsKeyboardFocusWithin) Then Return
            If e.Key = Key.Home Then cibles.First().Focus() Else cibles.Last().Focus()
            e.Handled = True
        End Sub

        ''' <summary>Les commandes de la rangée, dans l'ordre : les fixes, puis celles de la section montrée.</summary>
        Friend Function Commandes() As IEnumerable(Of UIElement)
            Dim section = TryCast(SelectedItem, ActionBarSection)
            Dim rangee = FixedItems.Concat(If(section Is Nothing, Enumerable.Empty(Of UIElement)(), section.Items.OfType(Of UIElement)()))
            Return rangee.Select(Function(c) If(TryCast(c, ActionBarDropDown)?.FocusTarget, c)).Where(Function(c) Not (TypeOf c Is ActionBarSeparator))
        End Function

        ''' <summary>Montre une section : elle devient la section choisie.</summary>
        Friend Sub Montrer(section As ActionBarSection)
            If section IsNot Nothing AndAlso Items.Contains(section) AndAlso Not Equals(SelectedItem, section) Then
                SetCurrentValue(SelectedItemProperty, section)
            End If
        End Sub

        ''' <summary>Ce qui dépend des sections et des commandes : les onglets, la zone fixe.</summary>
        Friend Sub MettreAJourEtat()
            Dim visibles = Items.OfType(Of ActionBarSection)().Count(Function(s) s.Visibility <> Visibility.Collapsed)
            SetValue(HasTabsPropertyKey, visibles > 1)
            SetValue(HasFixedItemsPropertyKey, FixedItems.Count > 0)
            Dim section = TryCast(SelectedItem, ActionBarSection)
            SetValue(HasSectionItemsPropertyKey, section IsNot Nothing AndAlso section.Items.Count > 0)
        End Sub

        Private Shared Sub OnActiveSectionChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, ActionBar).ChoisirLaSection()
        End Sub

        ' La section d'ActiveSection, sinon celle déjà choisie, sinon la première visible.
        Private Sub ChoisirLaSection()
            Dim sections = Items.OfType(Of ActionBarSection)().ToList()
            Dim voulue = sections.FirstOrDefault(Function(s) s.Key IsNot Nothing AndAlso
                                                             String.Equals(s.Key, ActiveSection, StringComparison.Ordinal) AndAlso
                                                             s.Visibility <> Visibility.Collapsed)
            Dim repli = voulue Is Nothing
            If repli Then
                Dim actuelle = TryCast(SelectedItem, ActionBarSection)
                If actuelle IsNot Nothing AndAlso sections.Contains(actuelle) AndAlso actuelle.Visibility <> Visibility.Collapsed Then Return
                voulue = sections.FirstOrDefault(Function(s) s.Visibility <> Visibility.Collapsed)
            End If
            If voulue Is Nothing OrElse Equals(SelectedItem, voulue) Then Return
            _parRepli = repli
            Try
                SetCurrentValue(SelectedItemProperty, voulue)
            Finally
                _parRepli = False
            End Try
        End Sub

        ''' <summary>Une section cachée ou montrée : la section montrée peut changer (la voulue revient, ou une autre la remplace).</summary>
        Friend Sub SectionVisibiliteChangee()
            ChoisirLaSection()
            MettreAJourEtat()
        End Sub

        ' Les commandes de la section choisie passent dans la rangée.
        Private Sub MontrerLaSection()
            Dim section = TryCast(SelectedItem, ActionBarSection)
            If _commandes IsNot Nothing Then _commandes.ItemsSource = If(section Is Nothing, Nothing, section.Items)
            MettreAJourEtat()
        End Sub

        Private Function ChoixDesLibelles(cle As String, montrer As Boolean) As MenuItem
            Dim choix As New MenuItem With {.IsCheckable = False}
            choix.SetResourceReference(StyleProperty, MenuItemStyleKey)
            choix.SetBinding(HeaderedItemsControl.HeaderProperty, New Binding("[" & cle & "]") With {.Source = Localizer.Current, .Mode = BindingMode.OneWay})
            AddHandler choix.Click, Sub(s, e) SetCurrentValue(ShowLabelsProperty, montrer)
            Return choix
        End Function

        Private Sub OnRepli(sender As Object, e As RoutedEventArgs)
            SetCurrentValue(IsCollapsedProperty, Not IsCollapsed)
        End Sub

        Private Sub OnPrecedent(sender As Object, e As RoutedEventArgs)
            Defiler(-1)
        End Sub

        Private Sub OnSuivant(sender As Object, e As RoutedEventArgs)
            Defiler(1)
        End Sub

        Private Sub Defiler(sens As Integer)
            If _defilement Is Nothing Then Return
            _defilement.ScrollToHorizontalOffset(_defilement.HorizontalOffset + sens * _defilement.ViewportWidth * RatioDeDefilement)
        End Sub

        ' Le défilement, dès que la rangée ne tient plus dans la largeur de la barre ; revu à chaque
        ' changement de taille. Ses flèches prennent alors place aux deux bords de la rangée.
        Private Sub OnDefilement(sender As Object, e As EventArgs)
            If _defilement Is Nothing Then Return
            Dim defilable = _defilement.ScrollableWidth > 0.5
            If defilable = IsScrollable Then Return

            SetValue(IsScrollablePropertyKey, defilable)
            Dim largeur = TryFindResource("mkm.actionbar.nav.button.width")
            Dim bord = If(Not defilable, 0.0, If(TypeOf largeur Is Double, CDbl(largeur), 32.0))
            _defilement.Margin = New Thickness(bord, 0, bord, 0)
        End Sub

    End Class

End Namespace
