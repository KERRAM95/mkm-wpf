Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' L'organigramme de mkm (<c>OrganizationChart</c>) : des nœuds en arbre, chacun au-dessus de ses enfants, reliés par
    ''' des traits ; un nœud peut se replier, se choisir.
    ''' <code>
    ''' &lt;mkm:OrganizationChart&gt;
    '''     &lt;mkm:OrganizationChartNode Header="Direction"&gt;
    '''         &lt;mkm:OrganizationChartNode Header="Bureau d'études" /&gt;
    '''     &lt;/mkm:OrganizationChartNode&gt;
    ''' &lt;/mkm:OrganizationChart&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>Lié à des données : <c>ItemsSource</c> et un <c>HierarchicalDataTemplate</c>.</remarks>
    Public Class OrganizationChart
        Inherits ItemsControl

        ''' <summary>Vrai pour un bouton sous chaque nœud qui a des enfants : il les replie.</summary>
        Public Shared ReadOnly CollapsibleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Collapsible), GetType(Boolean), GetType(OrganizationChart), New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits))

        ''' <summary>Vrai pour des nœuds qu'un clic choisit.</summary>
        Public Shared ReadOnly SelectableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Selectable), GetType(Boolean), GetType(OrganizationChart), New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits))

        ''' <summary>Le nœud choisi (sa donnée, liée à des données). Se lie dans les deux sens.</summary>
        Public Shared ReadOnly SelectedItemProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(SelectedItem), GetType(Object), GetType(OrganizationChart),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnSelectedItemChanged))

        ''' <summary>Le choix a changé.</summary>
        Public Shared ReadOnly SelectedItemChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(SelectedItemChanged), RoutingStrategy.Bubble, GetType(RoutedPropertyChangedEventHandler(Of Object)), GetType(OrganizationChart))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(OrganizationChart), New FrameworkPropertyMetadata(GetType(OrganizationChart)))
            FocusableProperty.OverrideMetadata(GetType(OrganizationChart), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai pour des nœuds qui se replient.</summary>
        Public Property Collapsible As Boolean
            Get
                Return CBool(GetValue(CollapsibleProperty))
            End Get
            Set(value As Boolean)
                SetValue(CollapsibleProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour des nœuds qu'un clic choisit.</summary>
        Public Property Selectable As Boolean
            Get
                Return CBool(GetValue(SelectableProperty))
            End Get
            Set(value As Boolean)
                SetValue(SelectableProperty, value)
            End Set
        End Property

        ''' <summary>Le nœud choisi.</summary>
        Public Property SelectedItem As Object
            Get
                Return GetValue(SelectedItemProperty)
            End Get
            Set(value As Object)
                SetValue(SelectedItemProperty, value)
            End Set
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

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is OrganizationChartNode
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New OrganizationChartNode()
        End Function

        ' Le nœud choisi se marque ; les autres non.
        Private Shared Sub OnSelectedItemChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim organigramme = DirectCast(d, OrganizationChart)
            For Each noeud In organigramme.Noeuds(organigramme)
                noeud.SetValue(OrganizationChartNode.IsSelectedPropertyKey, e.NewValue IsNot Nothing AndAlso Equals(OrganizationChartNode.DonneeDe(noeud), e.NewValue))
            Next
            organigramme.RaiseEvent(New RoutedPropertyChangedEventArgs(Of Object)(e.OldValue, e.NewValue, SelectedItemChangedEvent))
        End Sub

        ' Les nœuds réalisés, à tout niveau.
        Friend Iterator Function Noeuds(parent As ItemsControl) As IEnumerable(Of OrganizationChartNode)
            For i = 0 To parent.Items.Count - 1
                Dim noeud = TryCast(parent.ItemContainerGenerator.ContainerFromIndex(i), OrganizationChartNode)
                If noeud Is Nothing Then Continue For
                Yield noeud
                For Each enfant In Noeuds(noeud)
                    Yield enfant
                Next
            Next
        End Function

        ''' <summary>Un arbre pour les lecteurs d'écran : ses nœuds.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Tree)
        End Function

    End Class

    ''' <summary>Un nœud d'<see cref="OrganizationChart"/> : son contenu (<c>Header</c>) dans un cadre, ses enfants dessous.</summary>
    <TemplatePart(Name:="PART_Node", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_Toggle", Type:=GetType(ButtonBase))>
    Public Class OrganizationChartNode
        Inherits HeaderedItemsControl

        ''' <summary>Vrai quand ses enfants se montrent (par défaut). Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsExpandedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsExpanded), GetType(Boolean), GetType(OrganizationChartNode),
            New FrameworkPropertyMetadata(True, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsExpandedChanged))

        Friend Shared ReadOnly IsSelectedPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsSelected), GetType(Boolean), GetType(OrganizationChartNode), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le nœud choisi.</summary>
        Public Shared ReadOnly IsSelectedProperty As DependencyProperty = IsSelectedPropertyKey.DependencyProperty

        ''' <summary>Vrai pour un nœud qui se replie (repris de l'organigramme).</summary>
        Public Shared ReadOnly CollapsibleProperty As DependencyProperty = OrganizationChart.CollapsibleProperty.AddOwner(
            GetType(OrganizationChartNode), New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits))

        ''' <summary>Vrai pour un nœud qu'un clic choisit (repris de l'organigramme).</summary>
        Public Shared ReadOnly SelectableProperty As DependencyProperty = OrganizationChart.SelectableProperty.AddOwner(
            GetType(OrganizationChartNode), New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits))

        Private _cadre As FrameworkElement
        Private _bascule As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(OrganizationChartNode), New FrameworkPropertyMetadata(GetType(OrganizationChartNode)))
            FocusableProperty.OverrideMetadata(GetType(OrganizationChartNode), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand ses enfants se montrent.</summary>
        Public Property IsExpanded As Boolean
            Get
                Return CBool(GetValue(IsExpandedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsExpandedProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour le nœud choisi.</summary>
        Public ReadOnly Property IsSelected As Boolean
            Get
                Return CBool(GetValue(IsSelectedProperty))
            End Get
        End Property

        ''' <summary>Vrai pour un nœud qui se replie.</summary>
        Public Property Collapsible As Boolean
            Get
                Return CBool(GetValue(CollapsibleProperty))
            End Get
            Set(value As Boolean)
                SetValue(CollapsibleProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour un nœud qu'un clic choisit.</summary>
        Public Property Selectable As Boolean
            Get
                Return CBool(GetValue(SelectableProperty))
            End Get
            Set(value As Boolean)
                SetValue(SelectableProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is OrganizationChartNode
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New OrganizationChartNode()
        End Function

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _cadre IsNot Nothing Then
                RemoveHandler _cadre.MouseLeftButtonUp, AddressOf Cadre_MouseLeftButtonUp
                RemoveHandler _cadre.KeyDown, AddressOf Cadre_KeyDown
            End If
            If _bascule IsNot Nothing Then RemoveHandler _bascule.Click, AddressOf Bascule_Click
            MyBase.OnApplyTemplate()
            _cadre = TryCast(GetTemplateChild("PART_Node"), FrameworkElement)
            _bascule = TryCast(GetTemplateChild("PART_Toggle"), ButtonBase)
            If _cadre IsNot Nothing Then
                AddHandler _cadre.MouseLeftButtonUp, AddressOf Cadre_MouseLeftButtonUp
                AddHandler _cadre.KeyDown, AddressOf Cadre_KeyDown
            End If
            If _bascule IsNot Nothing Then
                AddHandler _bascule.Click, AddressOf Bascule_Click
                NommerLaBascule()
            End If
        End Sub

        Private Function Organigramme() As OrganizationChart
            Dim parent As DependencyObject = Me
            While parent IsNot Nothing AndAlso TypeOf parent IsNot OrganizationChart
                parent = ItemsControl.ItemsControlFromItemContainer(parent)
            End While
            Return TryCast(parent, OrganizationChart)
        End Function

        ''' <summary>La donnée du nœud : le nœud lui-même, écrit dans le XAML, ou ce qu'il montre.</summary>
        Friend Shared Function DonneeDe(noeud As OrganizationChartNode) As Object
            Dim parent = ItemsControl.ItemsControlFromItemContainer(noeud)
            Dim donnee = parent?.ItemContainerGenerator.ItemFromContainer(noeud)
            Return If(donnee Is Nothing OrElse donnee Is DependencyProperty.UnsetValue, noeud, donnee)
        End Function

        ''' <summary>Choisit le nœud, ou le retire s'il l'est déjà.</summary>
        Friend Sub Choisir()
            If Not Selectable Then Return
            Dim racine = Organigramme()
            If racine Is Nothing Then Return
            Dim donnee = DonneeDe(Me)
            racine.SetCurrentValue(OrganizationChart.SelectedItemProperty, If(Equals(racine.SelectedItem, donnee), Nothing, donnee))
        End Sub

        Private Sub Cadre_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
            If Not Selectable Then Return
            Choisir()
            _cadre.Focus()
            e.Handled = True
        End Sub

        Private Sub Cadre_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key <> Key.Enter AndAlso e.Key <> Key.Space Then Return
            Choisir()
            e.Handled = True
        End Sub

        Private Sub Bascule_Click(sender As Object, e As RoutedEventArgs)
            IsExpanded = Not IsExpanded
            e.Handled = True
        End Sub

        Private Shared Sub OnIsExpandedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, OrganizationChartNode).NommerLaBascule()
        End Sub

        ' La bascule dit ce qu'elle fera : « Replier » ou « Déplier ».
        Private Sub NommerLaBascule()
            If _bascule Is Nothing Then Return
            AutomationProperties.SetName(_bascule, Localization.Localizer.Current.T(If(IsExpanded, "aria.collapse", "aria.expand")))
        End Sub

        ''' <summary>Un nœud d'arbre pour les lecteurs d'écran, nommé par son en-tête ; développé ou réduit.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDuNoeud(Me)
        End Function

        Private NotInheritable Class PairDuNoeud
            Inherits FrameworkElementAutomationPeer
            Implements IExpandCollapseProvider, ISelectionItemProvider

            Public Sub New(noeud As OrganizationChartNode)
                MyBase.New(noeud)
            End Sub

            Private ReadOnly Property Noeud As OrganizationChartNode
                Get
                    Return DirectCast(Owner, OrganizationChartNode)
                End Get
            End Property

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.TreeItem
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(OrganizationChartNode)
            End Function

            Protected Overrides Function GetNameCore() As String
                Dim nom = MyBase.GetNameCore()
                If Not String.IsNullOrEmpty(nom) Then Return nom
                Return If(Noeud.Header?.ToString(), String.Empty)
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.ExpandCollapse AndAlso Noeud.HasItems Then Return Me
                If patternInterface = PatternInterface.SelectionItem AndAlso Noeud.Selectable Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
                Get
                    Return If(Noeud.IsExpanded, ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
                End Get
            End Property

            Public Sub Expand() Implements IExpandCollapseProvider.Expand
                Noeud.IsExpanded = True
            End Sub

            Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
                Noeud.IsExpanded = False
            End Sub

            Public ReadOnly Property IsSelected As Boolean Implements ISelectionItemProvider.IsSelected
                Get
                    Return Noeud.IsSelected
                End Get
            End Property

            Public ReadOnly Property SelectionContainer As IRawElementProviderSimple Implements ISelectionItemProvider.SelectionContainer
                Get
                    Return Nothing
                End Get
            End Property

            Public Sub [Select]() Implements ISelectionItemProvider.Select
                If Not Noeud.IsSelected Then Noeud.Choisir()
            End Sub

            Public Sub AddToSelection() Implements ISelectionItemProvider.AddToSelection
                [Select]()
            End Sub

            Public Sub RemoveFromSelection() Implements ISelectionItemProvider.RemoveFromSelection
                If Noeud.IsSelected Then Noeud.Choisir()
            End Sub

        End Class

    End Class

    ''' <summary>
    ''' La rangée des enfants d'un nœud d'organigramme : côte à côte, chacun entouré de la gouttière, sous la place des
    ''' traits ; elle trace le trait horizontal d'un enfant à l'autre et le trait qui descend vers chacun.
    ''' </summary>
    Friend NotInheritable Class RangeeDOrganigramme
        Inherits System.Windows.Controls.Panel

        Public Shared ReadOnly GutterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Gutter), GetType(Double), GetType(RangeeDOrganigramme), New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure))

        Public Shared ReadOnly ConnectorHeightProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ConnectorHeight), GetType(Double), GetType(RangeeDOrganigramme), New FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsMeasure))

        Public Shared ReadOnly ConnectorBrushProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ConnectorBrush), GetType(Brush), GetType(RangeeDOrganigramme), New FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender))

        Public Property Gutter As Double
            Get
                Return CDbl(GetValue(GutterProperty))
            End Get
            Set(value As Double)
                SetValue(GutterProperty, value)
            End Set
        End Property

        Public Property ConnectorHeight As Double
            Get
                Return CDbl(GetValue(ConnectorHeightProperty))
            End Get
            Set(value As Double)
                SetValue(ConnectorHeightProperty, value)
            End Set
        End Property

        Public Property ConnectorBrush As Brush
            Get
                Return DirectCast(GetValue(ConnectorBrushProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(ConnectorBrushProperty, value)
            End Set
        End Property

        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            Dim largeur = 0.0, hauteur = 0.0
            For Each enfant As UIElement In InternalChildren
                enfant.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
                largeur += enfant.DesiredSize.Width + 2 * Gutter
                hauteur = Math.Max(hauteur, enfant.DesiredSize.Height)
            Next
            Return New Size(largeur, If(InternalChildren.Count = 0, 0, hauteur + ConnectorHeight))
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim x = 0.0
            For Each enfant As UIElement In InternalChildren
                Dim w = enfant.DesiredSize.Width + 2 * Gutter
                enfant.Arrange(New Rect(x + Gutter, ConnectorHeight, enfant.DesiredSize.Width, enfant.DesiredSize.Height))
                x += w
            Next
            InvalidateVisual()
            Return finale
        End Function

        ''' <summary>Les milieux des enfants, pour les traits.</summary>
        Friend Function Milieux() As List(Of Double)
            Dim resultat As New List(Of Double)
            Dim x = 0.0
            For Each enfant As UIElement In InternalChildren
                Dim w = enfant.DesiredSize.Width + 2 * Gutter
                resultat.Add(x + w / 2)
                x += w
            Next
            Return resultat
        End Function

        Protected Overrides Sub OnRender(dc As DrawingContext)
            MyBase.OnRender(dc)
            Dim centres = Milieux()
            If centres.Count = 0 Then Return
            Dim trait As New Pen(ConnectorBrush, 1)
            Dim y = 0.5
            If centres.Count > 1 Then dc.DrawLine(trait, New Point(Math.Round(centres.First()) + 0.5, y), New Point(Math.Round(centres.Last()) + 0.5, y))
            For Each m In centres
                dc.DrawLine(trait, New Point(Math.Round(m) + 0.5, 0), New Point(Math.Round(m) + 0.5, ConnectorHeight))
            Next
        End Sub

    End Class

End Namespace
