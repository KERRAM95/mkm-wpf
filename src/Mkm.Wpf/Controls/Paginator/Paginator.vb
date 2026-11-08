Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>Les données de <see cref="Paginator.PageChange"/> : la page montrée.</summary>
    Public Class PageChangeEventArgs
        Inherits RoutedEventArgs

        ''' <summary>Les données d'un changement de page.</summary>
        Public Sub New(evenement As RoutedEvent, source As Object, page As Integer, first As Integer, rows As Integer, pageCount As Integer)
            MyBase.New(evenement, source)
            Me.Page = page
            Me.First = first
            Me.Rows = rows
            Me.PageCount = pageCount
        End Sub

        ''' <summary>La page montrée, à partir de 0.</summary>
        Public ReadOnly Property Page As Integer
        ''' <summary>L'indice de sa première ligne.</summary>
        Public ReadOnly Property First As Integer
        ''' <summary>Le nombre de lignes par page.</summary>
        Public ReadOnly Property Rows As Integer
        ''' <summary>Le nombre de pages.</summary>
        Public ReadOnly Property PageCount As Integer

    End Class

    ''' <summary>Un lien de page d'un <see cref="Paginator"/> : son numéro, et s'il est la page montrée.</summary>
    Public NotInheritable Class PaginatorPage

        Friend Sub New(index As Integer, actuelle As Boolean)
            Me.Index = index
            IsCurrent = actuelle
        End Sub

        ''' <summary>La page, à partir de 0.</summary>
        Public ReadOnly Property Index As Integer
        ''' <summary>Son numéro, à partir de 1.</summary>
        Public ReadOnly Property Number As Integer
            Get
                Return Index + 1
            End Get
        End Property
        ''' <summary>Vrai pour la page montrée.</summary>
        Public ReadOnly Property IsCurrent As Boolean
        ''' <summary>Ce que dit un lecteur d'écran : « Page 3 ».</summary>
        Public ReadOnly Property Label As String
            Get
                Return Localizer.Current.T("aria.page", Number)
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Function ToString() As String
            Return Number.ToString()
        End Function

    End Class

    ''' <summary>
    ''' La pagination de mkm (Paginator) : aller à la première page, à la précédente, à une page de numéro, à la suivante, à
    ''' la dernière ; le nombre de lignes par page ; « 3 sur 12 ». Elle ne montre pas les données : l'application montre
    ''' les lignes de <c>First</c> à <c>First + Rows</c>, à chaque <c>PageChange</c>.
    ''' <code>
    ''' &lt;mkm:Paginator TotalRecords="120" Rows="10" RowsPerPageOptions="10, 20, 50" PageChange="Pages_PageChange" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_First", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Previous", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Next", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Last", Type:=GetType(ButtonBase))>
    Public Class Paginator
        Inherits Control

        ''' <summary>L'indice de la première ligne de la page montrée. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly FirstProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(First), GetType(Integer), GetType(Paginator),
            New FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnChange))

        ''' <summary>Le nombre de lignes par page : 10 par défaut. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly RowsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Rows), GetType(Integer), GetType(Paginator),
            New FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnRowsChanged))

        ''' <summary>Le nombre total de lignes.</summary>
        Public Shared ReadOnly TotalRecordsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(TotalRecords), GetType(Integer), GetType(Paginator), New FrameworkPropertyMetadata(0, AddressOf OnChange))

        ''' <summary>Le nombre de liens de page montrés à la fois : 5 par défaut.</summary>
        Public Shared ReadOnly PageLinkSizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(PageLinkSize), GetType(Integer), GetType(Paginator), New FrameworkPropertyMetadata(5, AddressOf OnChange))

        ''' <summary>Les nombres de lignes par page proposés (10, 20, 50) ; sans eux, pas de choix.</summary>
        Public Shared ReadOnly RowsPerPageOptionsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(RowsPerPageOptions), GetType(IEnumerable(Of Integer)), GetType(Paginator), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai pour « 3 sur 12 » après les liens.</summary>
        Public Shared ReadOnly ShowCurrentPageReportProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowCurrentPageReport), GetType(Boolean), GetType(Paginator), New FrameworkPropertyMetadata(False))

        Private Shared ReadOnly PagesPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Pages), GetType(ReadOnlyObservableCollection(Of PaginatorPage)), GetType(Paginator), New FrameworkPropertyMetadata(Nothing))
        ''' <summary>Les liens de page montrés.</summary>
        Public Shared ReadOnly PagesProperty As DependencyProperty = PagesPropertyKey.DependencyProperty

        Private Shared ReadOnly PagePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Page), GetType(Integer), GetType(Paginator), New FrameworkPropertyMetadata(0))
        ''' <summary>La page montrée, à partir de 0.</summary>
        Public Shared ReadOnly PageProperty As DependencyProperty = PagePropertyKey.DependencyProperty

        Private Shared ReadOnly PageCountPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(PageCount), GetType(Integer), GetType(Paginator), New FrameworkPropertyMetadata(1))
        ''' <summary>Le nombre de pages (1 au moins).</summary>
        Public Shared ReadOnly PageCountProperty As DependencyProperty = PageCountPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLastPagePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLastPage), GetType(Boolean), GetType(Paginator), New FrameworkPropertyMetadata(True))
        ''' <summary>Vrai sur la dernière page : les flèches suivante et dernière grisées.</summary>
        Public Shared ReadOnly IsLastPageProperty As DependencyProperty = IsLastPagePropertyKey.DependencyProperty

        Private Shared ReadOnly CurrentPageReportPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(CurrentPageReport), GetType(String), GetType(Paginator), New FrameworkPropertyMetadata(String.Empty))
        ''' <summary>« 3 sur 12 ».</summary>
        Public Shared ReadOnly CurrentPageReportProperty As DependencyProperty = CurrentPageReportPropertyKey.DependencyProperty

        ''' <summary>Quand la page montrée change (un lien, une flèche, le nombre de lignes).</summary>
        Public Shared ReadOnly PageChangeEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(PageChange), RoutingStrategy.Bubble, GetType(EventHandler(Of PageChangeEventArgs)), GetType(Paginator))

        Private ReadOnly _pages As New ObservableCollection(Of PaginatorPage)()

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Paginator), New FrameworkPropertyMetadata(GetType(Paginator)))
            FocusableProperty.OverrideMetadata(GetType(Paginator), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une pagination vide.</summary>
        Public Sub New()
            SetValue(PagesPropertyKey, New ReadOnlyObservableCollection(Of PaginatorPage)(_pages))
            [AddHandler](ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Clic))
            AddHandler Loaded, Sub() AddHandler Localizer.Current.LanguageChanged, AddressOf Langue
            AddHandler Unloaded, Sub() RemoveHandler Localizer.Current.LanguageChanged, AddressOf Langue
            Calculer()
        End Sub

        ''' <summary>L'indice de la première ligne de la page montrée.</summary>
        Public Property First As Integer
            Get
                Return CInt(GetValue(FirstProperty))
            End Get
            Set(value As Integer)
                SetValue(FirstProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre de lignes par page.</summary>
        Public Property Rows As Integer
            Get
                Return CInt(GetValue(RowsProperty))
            End Get
            Set(value As Integer)
                SetValue(RowsProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre total de lignes.</summary>
        Public Property TotalRecords As Integer
            Get
                Return CInt(GetValue(TotalRecordsProperty))
            End Get
            Set(value As Integer)
                SetValue(TotalRecordsProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre de liens de page montrés à la fois.</summary>
        Public Property PageLinkSize As Integer
            Get
                Return CInt(GetValue(PageLinkSizeProperty))
            End Get
            Set(value As Integer)
                SetValue(PageLinkSizeProperty, value)
            End Set
        End Property

        ''' <summary>Les nombres de lignes par page proposés.</summary>
        <System.ComponentModel.TypeConverter(GetType(IntegerListConverter))>
        Public Property RowsPerPageOptions As IEnumerable(Of Integer)
            Get
                Return DirectCast(GetValue(RowsPerPageOptionsProperty), IEnumerable(Of Integer))
            End Get
            Set(value As IEnumerable(Of Integer))
                SetValue(RowsPerPageOptionsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour « 3 sur 12 ».</summary>
        Public Property ShowCurrentPageReport As Boolean
            Get
                Return CBool(GetValue(ShowCurrentPageReportProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowCurrentPageReportProperty, value)
            End Set
        End Property

        ''' <summary>Les liens de page montrés.</summary>
        Public ReadOnly Property Pages As ReadOnlyObservableCollection(Of PaginatorPage)
            Get
                Return DirectCast(GetValue(PagesProperty), ReadOnlyObservableCollection(Of PaginatorPage))
            End Get
        End Property

        ''' <summary>La page montrée, à partir de 0.</summary>
        Public ReadOnly Property Page As Integer
            Get
                Return CInt(GetValue(PageProperty))
            End Get
        End Property

        ''' <summary>Le nombre de pages.</summary>
        Public ReadOnly Property PageCount As Integer
            Get
                Return CInt(GetValue(PageCountProperty))
            End Get
        End Property

        ''' <summary>Vrai sur la dernière page.</summary>
        Public ReadOnly Property IsLastPage As Boolean
            Get
                Return CBool(GetValue(IsLastPageProperty))
            End Get
        End Property

        ''' <summary>« 3 sur 12 ».</summary>
        Public ReadOnly Property CurrentPageReport As String
            Get
                Return CStr(GetValue(CurrentPageReportProperty))
            End Get
        End Property

        ''' <summary>Quand la page montrée change.</summary>
        Public Custom Event PageChange As EventHandler(Of PageChangeEventArgs)
            AddHandler(value As EventHandler(Of PageChangeEventArgs))
                [AddHandler](PageChangeEvent, value)
            End AddHandler
            RemoveHandler(value As EventHandler(Of PageChangeEventArgs))
                [RemoveHandler](PageChangeEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As PageChangeEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Montre la page <paramref name="index"/> (à partir de 0), bornée aux pages qui existent.</summary>
        Public Sub ChangePage(index As Integer)
            Dim borne = Math.Max(0, Math.Min(PageCount - 1, index))
            SetCurrentValue(FirstProperty, borne * Math.Max(1, Rows))
        End Sub

        ' Vrai pendant que le paginateur range First lui-même : il n'annonce la page qu'une fois, à la fin.
        Private _range As Boolean

        Private Shared Sub OnChange(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim pagination = DirectCast(d, Paginator)
            Dim avant = pagination.Page
            pagination.Calculer()
            If pagination._range Then Return
            If e.Property Is FirstProperty AndAlso pagination.Page <> avant Then pagination.Annoncer()
            ' Moins de lignes qu'avant, First au-delà : la dernière page, annoncée (mkm ramène à pageCount - 1).
            If e.Property Is TotalRecordsProperty AndAlso pagination.TotalRecords > 0 AndAlso pagination.First >= pagination.TotalRecords Then
                pagination.Ranger((pagination.PageCount - 1) * Math.Max(1, pagination.Rows))
            End If
        End Sub

        ' Un autre nombre de lignes : la page qui contient la première ligne montrée, comme mkm.
        Private Shared Sub OnRowsChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim pagination = DirectCast(d, Paginator)
            Dim lignes = Math.Max(1, CInt(e.NewValue))
            pagination.Ranger((pagination.First \ lignes) * lignes)
        End Sub

        ' Pose First, recalcule, et annonce la page une seule fois.
        Private Sub Ranger(premier As Integer)
            _range = True
            Try
                SetCurrentValue(FirstProperty, premier)
            Finally
                _range = False
            End Try
            Calculer()
            Annoncer()
        End Sub

        Private Sub Annoncer()
            Me.RaiseEvent(New PageChangeEventArgs(PageChangeEvent, Me, Page, First, Rows, PageCount))
        End Sub

        Private Sub Langue(sender As Object, e As EventArgs)
            Calculer()
        End Sub

        ' La page, le nombre de pages, les liens montrés (centrés sur la page, comme mkm) et « 3 sur 12 ».
        Private Sub Calculer()
            Dim lignes = Math.Max(1, Rows)
            Dim nombre = Math.Max(1, CInt(Math.Ceiling(TotalRecords / CDbl(lignes))))
            Dim actuelle = Math.Max(0, Math.Min(nombre - 1, First \ lignes))
            SetValue(PageCountPropertyKey, nombre)
            SetValue(PagePropertyKey, actuelle)
            SetValue(IsLastPagePropertyKey, actuelle >= nombre - 1)
            Dim taille = Math.Max(1, Math.Min(PageLinkSize, nombre))
            Dim debut = Math.Max(0, Math.Min(nombre - taille, actuelle - taille \ 2))
            _pages.Clear()
            For i = debut To debut + taille - 1
                _pages.Add(New PaginatorPage(i, i = actuelle))
            Next
            SetValue(CurrentPageReportPropertyKey, Localizer.Current.T("locale.currentPageReport", actuelle + 1, nombre))
        End Sub

        ' Les flèches et les liens de page.
        Private Sub Clic(sender As Object, e As RoutedEventArgs)
            Dim bouton = TryCast(e.OriginalSource, FrameworkElement)
            If bouton Is Nothing Then Return
            Select Case bouton.Name
                Case "PART_First" : ChangePage(0)
                Case "PART_Previous" : ChangePage(Page - 1)
                Case "PART_Next" : ChangePage(Page + 1)
                Case "PART_Last" : ChangePage(PageCount - 1)
                Case Else
                    Dim lien = TryCast(bouton.DataContext, PaginatorPage)
                    If lien Is Nothing Then Return
                    ChangePage(lien.Index)
            End Select
            e.Handled = True
        End Sub

        ''' <summary>Une barre d'outils pour les lecteurs d'écran : ses boutons nommés (« Page suivante », « Page 3 »).</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.ToolBar)
        End Function

    End Class

End Namespace

Namespace Mkm.Wpf.Controls

    ''' <summary>Lit « 10, 20, 50 » en une liste d'entiers : les choix de lignes par page s'écrivent ainsi en XAML.</summary>
    Public NotInheritable Class IntegerListConverter
        Inherits System.ComponentModel.TypeConverter

        ''' <inheritdoc/>
        Public Overrides Function CanConvertFrom(context As System.ComponentModel.ITypeDescriptorContext, sourceType As Type) As Boolean
            Return sourceType Is GetType(String) OrElse MyBase.CanConvertFrom(context, sourceType)
        End Function

        ''' <inheritdoc/>
        Public Overrides Function ConvertFrom(context As System.ComponentModel.ITypeDescriptorContext, culture As Globalization.CultureInfo, value As Object) As Object
            Dim texte = TryCast(value, String)
            If texte Is Nothing Then Return MyBase.ConvertFrom(context, culture, value)
            Return texte.Split({","c, " "c, ";"c}, StringSplitOptions.RemoveEmptyEntries).
                Select(Function(n) Integer.Parse(n, Globalization.CultureInfo.InvariantCulture)).ToArray()
        End Function

    End Class

End Namespace
