Imports System.Collections
Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>La présentation des éléments d'un <see cref="DataView"/>.</summary>
    Public Enum DataViewLayout
        ''' <summary>L'un sous l'autre (par défaut).</summary>
        List
        ''' <summary>En grille, <see cref="DataView.GridColumns"/> par ligne.</summary>
        Grid
    End Enum

    ''' <summary>
    ''' La vue de données de mkm (<c>DataView</c>) : des éléments présentés par un gabarit, en liste ou en grille, avec un
    ''' en-tête, un pied et, au besoin, un paginateur.
    ''' <code>
    ''' &lt;mkm:DataView ItemsSource="{Binding Pieces}" Paginator="True" Rows="5"&gt;
    '''     &lt;mkm:DataView.ItemTemplate&gt;&lt;DataTemplate&gt;…&lt;/DataTemplate&gt;&lt;/mkm:DataView.ItemTemplate&gt;
    ''' &lt;/mkm:DataView&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Paginator", Type:=GetType(Paginator))>
    Public Class DataView
        Inherits Control

        ''' <summary>Les éléments.</summary>
        Public Shared ReadOnly ItemsSourceProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ItemsSource), GetType(IEnumerable), GetType(DataView), New FrameworkPropertyMetadata(Nothing, AddressOf OnItemsSourceChanged))

        ''' <summary>Le gabarit d'un élément en liste (et en grille, sans <see cref="GridItemTemplate"/>).</summary>
        Public Shared ReadOnly ItemTemplateProperty As DependencyProperty = ItemsControl.ItemTemplateProperty.AddOwner(GetType(DataView))

        ''' <summary>Le gabarit d'un élément en grille.</summary>
        Public Shared ReadOnly GridItemTemplateProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(GridItemTemplate), GetType(DataTemplate), GetType(DataView),
            New FrameworkPropertyMetadata(Nothing, Sub(d, e) d.SetValue(HasGridItemTemplatePropertyKey, e.NewValue IsNot Nothing)))

        Private Shared ReadOnly HasGridItemTemplatePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasGridItemTemplate), GetType(Boolean), GetType(DataView), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand un gabarit de grille est donné.</summary>
        Public Shared ReadOnly HasGridItemTemplateProperty As DependencyProperty = HasGridItemTemplatePropertyKey.DependencyProperty

        ''' <summary>En liste (par défaut) ou en grille.</summary>
        Public Shared ReadOnly LayoutProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Layout), GetType(DataViewLayout), GetType(DataView), New FrameworkPropertyMetadata(DataViewLayout.List))

        ''' <summary>Le nombre d'éléments par ligne de la grille : 3 par défaut.</summary>
        Public Shared ReadOnly GridColumnsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(GridColumns), GetType(Integer), GetType(DataView), New FrameworkPropertyMetadata(3))

        ''' <summary>Vrai pour un paginateur sous les éléments.</summary>
        Public Shared ReadOnly PaginatorProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Paginator), GetType(Boolean), GetType(DataView), New FrameworkPropertyMetadata(False, AddressOf OnPageChanged))

        ''' <summary>Le nombre d'éléments par page (avec <see cref="Paginator"/>).</summary>
        Public Shared ReadOnly RowsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Rows), GetType(Integer), GetType(DataView), New FrameworkPropertyMetadata(10, AddressOf OnPageChanged))

        ''' <summary>Le rang du premier élément montré. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly FirstProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(First), GetType(Integer), GetType(DataView),
            New FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnPageChanged))

        ''' <summary>L'en-tête.</summary>
        Public Shared ReadOnly HeaderProperty As DependencyProperty = HeaderedContentControl.HeaderProperty.AddOwner(GetType(DataView))

        ''' <summary>Le pied.</summary>
        Public Shared ReadOnly FooterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Footer), GetType(Object), GetType(DataView), New FrameworkPropertyMetadata(Nothing))

        Private Shared ReadOnly PageItemsPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(PageItems), GetType(IEnumerable), GetType(DataView), New FrameworkPropertyMetadata(Nothing))
        ''' <summary>Les éléments montrés : ceux de la page, ou tous sans paginateur.</summary>
        Public Shared ReadOnly PageItemsProperty As DependencyProperty = PageItemsPropertyKey.DependencyProperty

        Private Shared ReadOnly TotalRecordsPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(TotalRecords), GetType(Integer), GetType(DataView), New FrameworkPropertyMetadata(0))
        ''' <summary>Le nombre d'éléments.</summary>
        Public Shared ReadOnly TotalRecordsProperty As DependencyProperty = TotalRecordsPropertyKey.DependencyProperty

        Private Shared ReadOnly IsEmptyPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsEmpty), GetType(Boolean), GetType(DataView), New FrameworkPropertyMetadata(True))
        ''' <summary>Vrai sans éléments : la vue dit « Aucun résultat ».</summary>
        Public Shared ReadOnly IsEmptyProperty As DependencyProperty = IsEmptyPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(DataView), New FrameworkPropertyMetadata(GetType(DataView)))
            FocusableProperty.OverrideMetadata(GetType(DataView), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Les éléments.</summary>
        Public Property ItemsSource As IEnumerable
            Get
                Return DirectCast(GetValue(ItemsSourceProperty), IEnumerable)
            End Get
            Set(value As IEnumerable)
                SetValue(ItemsSourceProperty, value)
            End Set
        End Property

        ''' <summary>Le gabarit d'un élément en liste.</summary>
        Public Property ItemTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(ItemTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(ItemTemplateProperty, value)
            End Set
        End Property

        ''' <summary>Le gabarit d'un élément en grille.</summary>
        Public Property GridItemTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(GridItemTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(GridItemTemplateProperty, value)
            End Set
        End Property

        ''' <summary>En liste ou en grille.</summary>
        Public Property Layout As DataViewLayout
            Get
                Return DirectCast(GetValue(LayoutProperty), DataViewLayout)
            End Get
            Set(value As DataViewLayout)
                SetValue(LayoutProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre d'éléments par ligne de la grille.</summary>
        Public Property GridColumns As Integer
            Get
                Return CInt(GetValue(GridColumnsProperty))
            End Get
            Set(value As Integer)
                SetValue(GridColumnsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour un paginateur.</summary>
        Public Property Paginator As Boolean
            Get
                Return CBool(GetValue(PaginatorProperty))
            End Get
            Set(value As Boolean)
                SetValue(PaginatorProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre d'éléments par page.</summary>
        Public Property Rows As Integer
            Get
                Return CInt(GetValue(RowsProperty))
            End Get
            Set(value As Integer)
                SetValue(RowsProperty, value)
            End Set
        End Property

        ''' <summary>Le rang du premier élément montré.</summary>
        Public Property First As Integer
            Get
                Return CInt(GetValue(FirstProperty))
            End Get
            Set(value As Integer)
                SetValue(FirstProperty, value)
            End Set
        End Property

        ''' <summary>L'en-tête.</summary>
        Public Property Header As Object
            Get
                Return GetValue(HeaderProperty)
            End Get
            Set(value As Object)
                SetValue(HeaderProperty, value)
            End Set
        End Property

        ''' <summary>Le pied.</summary>
        Public Property Footer As Object
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As Object)
                SetValue(FooterProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand un gabarit de grille est donné.</summary>
        Public ReadOnly Property HasGridItemTemplate As Boolean
            Get
                Return CBool(GetValue(HasGridItemTemplateProperty))
            End Get
        End Property

        ''' <summary>Les éléments montrés.</summary>
        Public ReadOnly Property PageItems As IEnumerable
            Get
                Return DirectCast(GetValue(PageItemsProperty), IEnumerable)
            End Get
        End Property

        ''' <summary>Le nombre d'éléments.</summary>
        Public ReadOnly Property TotalRecords As Integer
            Get
                Return CInt(GetValue(TotalRecordsProperty))
            End Get
        End Property

        ''' <summary>Vrai sans éléments.</summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return CBool(GetValue(IsEmptyProperty))
            End Get
        End Property

        Private Shared Sub OnItemsSourceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim vue = DirectCast(d, DataView)
            Dim avant = TryCast(e.OldValue, INotifyCollectionChanged)
            Dim apres = TryCast(e.NewValue, INotifyCollectionChanged)
            If avant IsNot Nothing Then RemoveHandler avant.CollectionChanged, AddressOf vue.Elements_CollectionChanged
            If apres IsNot Nothing Then AddHandler apres.CollectionChanged, AddressOf vue.Elements_CollectionChanged
            vue.Paginer()
        End Sub

        Private Sub Elements_CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            Paginer()
        End Sub

        Private Shared Sub OnPageChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, DataView).Paginer()
        End Sub

        ' Les éléments de la page : tous sans paginateur, sinon Rows à partir de First.
        Private Sub Paginer()
            Dim tous = If(ItemsSource, Enumerable.Empty(Of Object)()).Cast(Of Object)().ToList()
            SetValue(TotalRecordsPropertyKey, tous.Count)
            SetValue(IsEmptyPropertyKey, tous.Count = 0)
            If Paginator AndAlso Rows > 0 Then
                SetValue(PageItemsPropertyKey, tous.Skip(Math.Max(0, First)).Take(Rows).ToList())
            Else
                SetValue(PageItemsPropertyKey, tous)
            End If
        End Sub

        ''' <summary>Une liste pour les lecteurs d'écran : ses éléments.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.List, Function() TryCast(Header, String))
        End Function

    End Class

End Namespace
