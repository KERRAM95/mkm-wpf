Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>La taille d'un <see cref="DataTable"/>.</summary>
    Public Enum DataTableSize
        ''' <summary>Des lignes de 32 px (par défaut).</summary>
        Normal
        ''' <summary>Des lignes serrées, 4 px au-dessus et au-dessous du texte.</summary>
        Small
        ''' <summary>Des lignes aérées, 12 px au-dessus et au-dessous du texte.</summary>
        Large
    End Enum

    ''' <summary>
    ''' Le tableau de données de mkm (<c>DataTable</c>), la grille de 3DEXPERIENCE : un en-tête gris clair de 27 px, ses
    ''' colonnes séparées d'un trait, les titres en graisse normale, l'icône de tri de mkm ; des lignes de 32 px sans trait
    ''' entre elles, la ligne choisie surlignée. <see cref="Striped"/> les zèbre, <see cref="GridLines"/> les quadrille ;
    ''' <see cref="EmptyMessage"/> se lit quand il n'y a rien, <see cref="Loading"/> voile le tableau.
    ''' <code>
    ''' &lt;mkm:DataTable ItemsSource="{Binding Variables}" AutoGenerateColumns="False" Striped="True"&gt;
    '''     &lt;mkm:DataTable.Columns&gt;
    '''         &lt;DataGridTextColumn Header="Nom" Binding="{Binding Nom}" Width="*" /&gt;
    '''     &lt;/mkm:DataTable.Columns&gt;
    ''' &lt;/mkm:DataTable&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' C'est le <see cref="DataGrid"/> de WPF : ses colonnes, le tri au clic sur un titre, la sélection d'une ou de
    ''' plusieurs lignes, le redimensionnement et le déplacement des colonnes, les colonnes figées, l'édition, le
    ''' détail d'une ligne, la virtualisation. Il ne propose pas de ligne vide d'ajout (<c>CanUserAddRows</c> faux).
    ''' </remarks>
    Public Class DataTable
        Inherits DataGrid

        ''' <summary>Vrai si une ligne sur deux a un fond pâle.</summary>
        Public Shared ReadOnly StripedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Striped), GetType(Boolean), GetType(DataTable), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai si les cellules sont quadrillées.</summary>
        Public Shared ReadOnly GridLinesProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(GridLines), GetType(Boolean), GetType(DataTable), New FrameworkPropertyMetadata(False))

        ''' <summary>La taille des lignes.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(DataTableSize), GetType(DataTable), New FrameworkPropertyMetadata(DataTableSize.Normal))

        ''' <summary>Ce qui se lit à la place des lignes quand il n'y en a aucune (rien par défaut, comme chez mkm).</summary>
        Public Shared ReadOnly EmptyMessageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(EmptyMessage), GetType(Object), GetType(DataTable), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai pendant un chargement : un voile et un chargeur couvrent le tableau.</summary>
        Public Shared ReadOnly LoadingProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Loading), GetType(Boolean), GetType(DataTable), New FrameworkPropertyMetadata(False, AddressOf OnLoadingChanged))

        ' Le voile est dans le gabarit, que le pair du DataGrid de WPF ne montre pas : le tableau dit lui-même aux lecteurs
        ' d'écran qu'il charge (son état, ItemStatus), comme aria-busy dans mkm.
        Private Shared Sub OnLoadingChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim avant = System.Windows.Automation.AutomationProperties.GetItemStatus(d)
            Dim apres = If(CBool(e.NewValue), Mkm.Wpf.Localization.Localizer.Current.T("aria.busy"), String.Empty)
            System.Windows.Automation.AutomationProperties.SetItemStatus(d, apres)
            If System.Windows.Automation.Peers.AutomationPeer.ListenerExists(System.Windows.Automation.Peers.AutomationEvents.PropertyChanged) Then
                System.Windows.Automation.Peers.UIElementAutomationPeer.FromElement(DirectCast(d, UIElement))?.RaisePropertyChangedEvent(
                    System.Windows.Automation.AutomationElementIdentifiers.ItemStatusProperty, If(avant, String.Empty), apres)
            End If
        End Sub

        ''' <summary>Ce qui se montre au-dessus du tableau (un titre, une recherche…).</summary>
        Public Shared ReadOnly HeaderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Header), GetType(Object), GetType(DataTable), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Ce qui se montre sous le tableau (un total…).</summary>
        Public Shared ReadOnly FooterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Footer), GetType(Object), GetType(DataTable), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le retrait des cellules, selon la taille (posé par le style).</summary>
        Public Shared ReadOnly CellPaddingProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(CellPadding), GetType(Thickness), GetType(DataTable), New FrameworkPropertyMetadata(New Thickness()))

        ''' <summary>Le retrait des titres de colonnes, selon la taille (posé par le style).</summary>
        Public Shared ReadOnly HeaderPaddingProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(HeaderPadding), GetType(Thickness), GetType(DataTable), New FrameworkPropertyMetadata(New Thickness()))

        ''' <summary>La clé du style de la case d'une colonne de cases : celle de mkm.</summary>
        Public Shared ReadOnly Property CheckBoxStyleKey As New ComponentResourceKey(GetType(DataTable), "CheckBoxStyle")

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(DataTable), New FrameworkPropertyMetadata(GetType(DataTable)))
            CanUserAddRowsProperty.OverrideMetadata(GetType(DataTable), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un tableau vide ; ses colonnes de cases prennent la case de mkm.</summary>
        Public Sub New()
            AddHandler Columns.CollectionChanged, Sub(s, e) HabillerLesCases()
            AddHandler Loaded, Sub() HabillerLesCases()
        End Sub

        ' La case d'une cellule nommée par l'en-tête de sa colonne : un lecteur d'écran dit « Active, cochée ».
        Private Shared Function NomParLaColonne() As Setter
            Return New Setter(System.Windows.Automation.AutomationProperties.NameProperty,
                              New System.Windows.Data.Binding("Column.Header") With {
                                  .RelativeSource = New System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, GetType(DataGridCell), 1)})
        End Function

        ' La case d'une colonne de cases que l'application n'a pas habillée elle-même : celle de mkm, en lecture comme
        ' en modification.
        Private Sub HabillerLesCases()
            Dim style = TryCast(TryFindResource(CheckBoxStyleKey), Style)
            If style Is Nothing Then Return
            For Each colonne In Columns.OfType(Of DataGridCheckBoxColumn)()
                If colonne.ElementStyle Is Nothing OrElse colonne.ElementStyle Is DataGridCheckBoxColumn.DefaultElementStyle Then
                    ' En lecture, comme le style de WPF : la case ne se clique pas, la cellule passe en modification.
                    Dim lecture As New Style(GetType(System.Windows.Controls.CheckBox), style)
                    lecture.Setters.Add(New Setter(IsHitTestVisibleProperty, False))
                    lecture.Setters.Add(New Setter(FocusableProperty, False))
                    lecture.Setters.Add(NomParLaColonne())
                    colonne.ElementStyle = lecture
                End If
                If colonne.EditingElementStyle Is Nothing OrElse colonne.EditingElementStyle Is DataGridCheckBoxColumn.DefaultEditingElementStyle Then
                    Dim edition As New Style(GetType(System.Windows.Controls.CheckBox), style)
                    edition.Setters.Add(NomParLaColonne())
                    colonne.EditingElementStyle = edition
                End If
            Next
        End Sub

        ''' <summary>Vrai si une ligne sur deux a un fond pâle.</summary>
        Public Property Striped As Boolean
            Get
                Return CBool(GetValue(StripedProperty))
            End Get
            Set(value As Boolean)
                SetValue(StripedProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si les cellules sont quadrillées.</summary>
        Public Property GridLines As Boolean
            Get
                Return CBool(GetValue(GridLinesProperty))
            End Get
            Set(value As Boolean)
                SetValue(GridLinesProperty, value)
            End Set
        End Property

        ''' <summary>La taille des lignes.</summary>
        Public Property Size As DataTableSize
            Get
                Return DirectCast(GetValue(SizeProperty), DataTableSize)
            End Get
            Set(value As DataTableSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui se lit à la place des lignes quand il n'y en a aucune.</summary>
        Public Property EmptyMessage As Object
            Get
                Return GetValue(EmptyMessageProperty)
            End Get
            Set(value As Object)
                SetValue(EmptyMessageProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pendant un chargement.</summary>
        Public Property Loading As Boolean
            Get
                Return CBool(GetValue(LoadingProperty))
            End Get
            Set(value As Boolean)
                SetValue(LoadingProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui se montre au-dessus du tableau.</summary>
        Public Property Header As Object
            Get
                Return GetValue(HeaderProperty)
            End Get
            Set(value As Object)
                SetValue(HeaderProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui se montre sous le tableau.</summary>
        Public Property Footer As Object
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As Object)
                SetValue(FooterProperty, value)
            End Set
        End Property

        ''' <summary>Le retrait des cellules, selon la taille.</summary>
        Public Property CellPadding As Thickness
            Get
                Return DirectCast(GetValue(CellPaddingProperty), Thickness)
            End Get
            Set(value As Thickness)
                SetValue(CellPaddingProperty, value)
            End Set
        End Property

        ''' <summary>Le retrait des titres de colonnes, selon la taille.</summary>
        Public Property HeaderPadding As Thickness
            Get
                Return DirectCast(GetValue(HeaderPaddingProperty), Thickness)
            End Get
            Set(value As Thickness)
                SetValue(HeaderPaddingProperty, value)
            End Set
        End Property

    End Class

End Namespace
