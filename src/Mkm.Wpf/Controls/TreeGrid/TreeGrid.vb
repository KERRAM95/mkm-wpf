Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Linq
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La place d'un nœud dans une <see cref="TreeGrid"/> : son niveau, s'il a des enfants, s'il est déplié, s'il est le
    ''' dernier de ses frères, et les traits de ses ancêtres qui passent à côté de lui.
    ''' </summary>
    Public NotInheritable Class TreeGridRowInfo
        Implements INotifyPropertyChanged

        Private _niveau As Integer = 1
        Private _aDesEnfants As Boolean
        Private _deplie As Boolean
        Private _dernier As Boolean
        Private _guides As Boolean() = Array.Empty(Of Boolean)()

        ''' <summary>Quand une propriété change.</summary>
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        ''' <summary>Le niveau : 1 pour une racine.</summary>
        Public Property Level As Integer
            Get
                Return _niveau
            End Get
            Friend Set(value As Integer)
                Poser(_niveau, value, NameOf(Level))
            End Set
        End Property

        ''' <summary>Vrai si le nœud a des enfants.</summary>
        Public Property HasChildren As Boolean
            Get
                Return _aDesEnfants
            End Get
            Friend Set(value As Boolean)
                Poser(_aDesEnfants, value, NameOf(HasChildren))
            End Set
        End Property

        ''' <summary>Vrai si le nœud est déplié.</summary>
        Public Property IsExpanded As Boolean
            Get
                Return _deplie
            End Get
            Friend Set(value As Boolean)
                Poser(_deplie, value, NameOf(IsExpanded))
            End Set
        End Property

        ''' <summary>Vrai si le nœud est le dernier de ses frères : son trait s'arrête à lui.</summary>
        Public Property IsLast As Boolean
            Get
                Return _dernier
            End Get
            Friend Set(value As Boolean)
                Poser(_dernier, value, NameOf(IsLast))
            End Set
        End Property

        ''' <summary>Pour chaque ancêtre à partir du niveau 2 : vrai s'il a encore des frères plus bas (son trait passe).</summary>
        Public Property Guides As Boolean()
            Get
                Return _guides
            End Get
            Friend Set(value As Boolean())
                If _guides.SequenceEqual(value) Then Return
                _guides = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Guides)))
            End Set
        End Property

        ''' <summary>Le parent du nœud, ou rien pour une racine.</summary>
        Friend Property Parent As Object

        Private Sub Poser(Of T)(ByRef champ As T, valeur As T, nom As String)
            If EqualityComparer(Of T).Default.Equals(champ, valeur) Then Return
            champ = valeur
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(nom))
        End Sub

    End Class

    ''' <summary>
    ''' La grille arborescente de mkm (<c>TreeGrid</c>), la DataGridView de 3DEXPERIENCE : un <see cref="DataTable"/> dont
    ''' les lignes sont les nœuds dépliés d'un arbre (<see cref="Nodes"/>, leurs enfants par <see cref="ChildrenPath"/>), et
    ''' dont une colonne (<see cref="TreeGridColumn"/>) porte l'arbre : les traits gris clair, la case qui déplie (+ et −),
    ''' l'icône et le libellé du nœud. Les nœuds se trient à l'intérieur de leurs frères ; au clavier, la flèche droite
    ''' déplie (puis descend au premier enfant), la flèche gauche replie (puis remonte au parent).
    ''' <code>
    ''' &lt;mkm:TreeGrid Nodes="{Binding Racines}" ChildrenPath="Enfants" AutoGenerateColumns="False"&gt;
    '''     &lt;mkm:TreeGrid.Columns&gt;
    '''         &lt;mkm:TreeGridColumn Header="Nom" Binding="{Binding Nom}" IconBinding="{Binding Icone}" Width="*" /&gt;
    '''         &lt;DataGridTextColumn Header="Révision" Binding="{Binding Revision}" /&gt;
    '''     &lt;/mkm:TreeGrid.Columns&gt;
    ''' &lt;/mkm:TreeGrid&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Les lignes viennent de <see cref="Nodes"/> : ne pas donner d'<c>ItemsSource</c>. Un enfant ajouté ou retiré d'une
    ''' collection qui le signale (<see cref="INotifyCollectionChanged"/>) refait les lignes ; la sélection est gardée.
    ''' </remarks>
    Public Class TreeGrid
        Inherits DataTable

        ''' <summary>La clé du style des cellules de la colonne de l'arbre.</summary>
        Public Shared ReadOnly Property TreeCellStyleKey As New ComponentResourceKey(GetType(TreeGrid), "TreeCellStyle")

        ''' <summary>Les racines de l'arbre.</summary>
        Public Shared ReadOnly NodesProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Nodes), GetType(IEnumerable), GetType(TreeGrid), New FrameworkPropertyMetadata(Nothing, AddressOf OnArbreChanged))

        ''' <summary>Le nom de la propriété qui donne les enfants d'un nœud (« Children » par défaut).</summary>
        Public Shared ReadOnly ChildrenPathProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ChildrenPath), GetType(String), GetType(TreeGrid), New FrameworkPropertyMetadata("Children", AddressOf OnArbreChanged))

        Private ReadOnly _rangees As New ObservableCollection(Of Object)
        Private ReadOnly _infos As New Dictionary(Of Object, TreeGridRowInfo)
        Private ReadOnly _deplies As New HashSet(Of Object)
        Private ReadOnly _ecoutees As New HashSet(Of INotifyCollectionChanged)
        Private ReadOnly _proprietes As New Dictionary(Of Type, PropertyInfo)
        Private _cheminDuTri As String
        Private _sensDuTri As ListSortDirection

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(TreeGrid), New FrameworkPropertyMetadata(GetType(TreeGrid)))
            ' Ses lignes sont les nœuds dépliés, non des données : Suppr ne les retire pas.
            CanUserDeleteRowsProperty.OverrideMetadata(GetType(TreeGrid), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une grille arborescente vide.</summary>
        Public Sub New()
            ItemsSource = _rangees
            AddHandler Columns.CollectionChanged, Sub(s, e) HabillerLArbre()
            AddHandler Loaded, Sub(s, e) HabillerLArbre()
        End Sub

        ' La colonne de l'arbre que l'application n'a pas habillée elle-même : le style de ses cellules (sans retrait, sur
        ' toute la hauteur de la ligne).
        Private Sub HabillerLArbre()
            Dim style = TryCast(TryFindResource(TreeCellStyleKey), Style)
            If style Is Nothing Then Return
            For Each colonne In Columns.OfType(Of TreeGridColumn)()
                If colonne.CellStyle Is Nothing Then colonne.CellStyle = style
            Next
        End Sub

        ''' <summary>Les racines de l'arbre.</summary>
        Public Property Nodes As IEnumerable
            Get
                Return DirectCast(GetValue(NodesProperty), IEnumerable)
            End Get
            Set(value As IEnumerable)
                SetValue(NodesProperty, value)
            End Set
        End Property

        ''' <summary>Le nom de la propriété qui donne les enfants d'un nœud.</summary>
        Public Property ChildrenPath As String
            Get
                Return CStr(GetValue(ChildrenPathProperty))
            End Get
            Set(value As String)
                SetValue(ChildrenPathProperty, value)
            End Set
        End Property

        ''' <summary>La place d'un nœud (son niveau, ses traits) ; rien pour un nœud que la grille ne connaît pas.</summary>
        Public Function GetRowInfo(node As Object) As TreeGridRowInfo
            Dim info As TreeGridRowInfo = Nothing
            Return If(node IsNot Nothing AndAlso _infos.TryGetValue(node, info), info, Nothing)
        End Function

        ''' <summary>Vrai si le nœud est déplié.</summary>
        Public Function IsExpanded(node As Object) As Boolean
            Return node IsNot Nothing AndAlso _deplies.Contains(node)
        End Function

        ''' <summary>Déplie un nœud : ses enfants paraissent sous lui.</summary>
        Public Sub Expand(node As Object)
            If node Is Nothing OrElse Not _deplies.Add(node) Then Return
            Reconstruire()
            Decrire(TryCast(ItemContainerGenerator.ContainerFromItem(node), DataGridRow), node)
        End Sub

        ''' <summary>Replie un nœud : ses descendants disparaissent.</summary>
        Public Sub Collapse(node As Object)
            If node Is Nothing OrElse Not _deplies.Remove(node) Then Return
            Reconstruire()
            Decrire(TryCast(ItemContainerGenerator.ContainerFromItem(node), DataGridRow), node)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Decrire(TryCast(element, DataGridRow), item)
        End Sub

        ' Pour les lecteurs d'écran, une ligne dit sa place parmi ses sœurs (aria-posinset, aria-setsize de mkm), son
        ' niveau et, si elle a des enfants, si elle est dépliée (aria-level, aria-expanded) : WPF n'a pas de niveau ni
        ' d'ExpandCollapse pour une ligne de grille, son état les porte.
        Private Sub Decrire(ligne As DataGridRow, noeud As Object)
            If ligne Is Nothing Then Return
            Dim info = GetRowInfo(noeud)
            If info Is Nothing Then Return
            Dim soeurs = If(info.Parent Is Nothing, Racines(), EnfantsDe(info.Parent))
            System.Windows.Automation.AutomationProperties.SetPositionInSet(ligne, soeurs.IndexOf(noeud) + 1)
            System.Windows.Automation.AutomationProperties.SetSizeOfSet(ligne, soeurs.Count)
            Dim etat = Localization.Localizer.Current.T("aria.level", info.Level)
            If info.HasChildren Then etat &= ", " & Localization.Localizer.Current.T(If(IsExpanded(noeud), "aria.expanded", "aria.collapsed"))
            System.Windows.Automation.AutomationProperties.SetItemStatus(ligne, etat)
        End Sub

        ''' <summary>Déplie un nœud replié, replie un nœud déplié.</summary>
        Public Sub Toggle(node As Object)
            If IsExpanded(node) Then Collapse(node) Else Expand(node)
        End Sub

        ''' <summary>Déplie tous les nœuds qui ont des enfants.</summary>
        Public Sub ExpandAll()
            Dim pile As New Stack(Of Object)(Racines())
            While pile.Count > 0
                Dim noeud = pile.Pop()
                Dim enfants = EnfantsDe(noeud)
                If enfants.Count = 0 Then Continue While
                _deplies.Add(noeud)
                For Each enfant In enfants
                    pile.Push(enfant)
                Next
            End While
            Reconstruire()
        End Sub

        ''' <summary>Replie tous les nœuds.</summary>
        Public Sub CollapseAll()
            _deplies.Clear()
            Reconstruire()
        End Sub

        ''' <summary>
        ''' Le tri d'une colonne trie les nœuds à l'intérieur de leurs frères, l'arbre gardé ; un nouveau clic inverse le
        ''' sens. L'évènement <c>Sorting</c> part d'abord, déjà traité.
        ''' </summary>
        Protected Overrides Sub OnSorting(eventArgs As DataGridSortingEventArgs)
            eventArgs.Handled = True
            MyBase.OnSorting(eventArgs)
            TrierPar(eventArgs.Column)
        End Sub

        ''' <summary>Trie les nœuds sur une colonne, à l'intérieur de leurs frères : un nouveau tri sur elle inverse le sens.</summary>
        Friend Sub TrierPar(colonne As DataGridColumn)
            If colonne Is Nothing OrElse String.IsNullOrEmpty(colonne.SortMemberPath) Then Return
            Dim sens = If(colonne.SortDirection = ListSortDirection.Ascending, ListSortDirection.Descending, ListSortDirection.Ascending)
            For Each autre In Columns
                If autre IsNot colonne Then autre.SortDirection = Nothing
            Next
            colonne.SortDirection = sens
            _cheminDuTri = colonne.SortMemberPath
            _sensDuTri = sens
            Reconstruire()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            MyBase.OnPreviewKeyDown(e)
            If e.Handled OrElse TypeOf e.OriginalSource Is TextBox Then Return
            ' Début et Fin : la première et la dernière ligne, comme mkm (le DataGrid irait à une cellule de la ligne).
            If (e.Key = Key.Home OrElse e.Key = Key.End) AndAlso Keyboard.Modifiers = ModifierKeys.None AndAlso _rangees.Count > 0 Then
                Aller(If(e.Key = Key.Home, _rangees(0), _rangees(_rangees.Count - 1)))
                e.Handled = True
                Return
            End If
            If e.Key <> Key.Right AndAlso e.Key <> Key.Left Then Return
            Dim noeud = If(CurrentItem, SelectedItem)
            Dim info = GetRowInfo(noeud)
            If info Is Nothing Then Return
            If e.Key = Key.Right Then
                If Not info.HasChildren Then Return
                If Not info.IsExpanded Then
                    Expand(noeud)
                Else
                    Aller(EnfantsDe(noeud).OrderBy(Function(n) _rangees.IndexOf(n)).FirstOrDefault(Function(n) _rangees.Contains(n)))
                End If
            Else
                If info.IsExpanded Then
                    Collapse(noeud)
                ElseIf info.Parent IsNot Nothing Then
                    Aller(info.Parent)
                Else
                    Return
                End If
            End If
            e.Handled = True
        End Sub

        Private Sub Aller(noeud As Object)
            If noeud Is Nothing Then Return
            SelectedItem = noeud
            CurrentItem = noeud
            ScrollIntoView(noeud)
            Dim ligne = TryCast(ItemContainerGenerator.ContainerFromItem(noeud), DataGridRow)
            ligne?.Focus()
        End Sub

        Private Shared Sub OnArbreChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim grille = DirectCast(d, TreeGrid)
            If e.Property Is ChildrenPathProperty Then grille._proprietes.Clear()
            grille.Reconstruire()
        End Sub

        Private Function Racines() As IList(Of Object)
            Return If(Nodes Is Nothing, New List(Of Object), Nodes.Cast(Of Object)().ToList())
        End Function

        ''' <summary>Les enfants d'un nœud, par <see cref="ChildrenPath"/>.</summary>
        Friend Function EnfantsDe(noeud As Object) As IList(Of Object)
            If noeud Is Nothing OrElse String.IsNullOrEmpty(ChildrenPath) Then Return New List(Of Object)
            Dim genre = noeud.GetType()
            Dim propriete As PropertyInfo = Nothing
            If Not _proprietes.TryGetValue(genre, propriete) Then
                propriete = genre.GetProperty(ChildrenPath, BindingFlags.Public Or BindingFlags.Instance)
                _proprietes(genre) = propriete
            End If
            Dim enfants = TryCast(propriete?.GetValue(noeud), IEnumerable)
            Return If(enfants Is Nothing, New List(Of Object), enfants.Cast(Of Object)().ToList())
        End Function

        Private Function CollectionDes(noeud As Object) As INotifyCollectionChanged
            If noeud Is Nothing Then Return TryCast(Nodes, INotifyCollectionChanged)
            Dim propriete As PropertyInfo = Nothing
            _proprietes.TryGetValue(noeud.GetType(), propriete)
            Return TryCast(propriete?.GetValue(noeud), INotifyCollectionChanged)
        End Function

        ' Des frères dans l'ordre du tri, s'il y en a un ; sinon dans le leur.
        Private Function Trier(freres As IList(Of Object)) As IList(Of Object)
            If _cheminDuTri Is Nothing Then Return freres
            Dim cle = Function(o As Object) Valeur(o, _cheminDuTri)
            Dim compare = Comparer(Of Object).Create(AddressOf ComparerLesValeurs)
            Return If(_sensDuTri = ListSortDirection.Ascending, freres.OrderBy(cle, compare), freres.OrderByDescending(cle, compare)).ToList()
        End Function

        Private Shared Function ComparerLesValeurs(a As Object, b As Object) As Integer
            If a Is Nothing Then Return If(b Is Nothing, 0, -1)
            If b Is Nothing Then Return 1
            Dim comparable = TryCast(a, IComparable)
            If comparable IsNot Nothing AndAlso a.GetType() Is b.GetType() Then Return comparable.CompareTo(b)
            Return String.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCulture)
        End Function

        ' La valeur d'un chemin simple (« Nom », « Article.Nom ») sur un nœud.
        Private Shared Function Valeur(noeud As Object, chemin As String) As Object
            Dim courant = noeud
            For Each partie In chemin.Split("."c)
                If courant Is Nothing Then Return Nothing
                courant = courant.GetType().GetProperty(partie, BindingFlags.Public Or BindingFlags.Instance)?.GetValue(courant)
            Next
            Return courant
        End Function

        ''' <summary>
        ''' Refait les lignes : les nœuds dépliés, dans l'ordre (et le tri), chacun à sa place. Les lignes qui restent ne
        ''' bougent pas de la collection : la sélection est gardée.
        ''' </summary>
        Friend Sub Reconstruire()
            For Each ecoutee In _ecoutees
                CollectionChangedEventManager.RemoveHandler(ecoutee, AddressOf OnEnfantsChanged)
            Next
            _ecoutees.Clear()
            Ecouter(TryCast(Nodes, INotifyCollectionChanged))

            Dim lignes As New List(Of Object)
            Parcourir(Racines(), Nothing, 1, New List(Of Boolean), lignes)

            ' Un nœud caché perd sa place (elle revient avec lui) ; seul un nœud sorti de l'arbre perd son dépliage : un
            ' descendant d'un parent replié garde le sien, comme les expandedKeys de mkm.
            For Each cache In _infos.Keys.Where(Function(n) Not DansLArbre(n)).ToList()
                _infos.Remove(cache)
            Next
            Dim horsDeVue = _deplies.Where(Function(n) Not DansLArbre(n)).ToList()
            If horsDeVue.Count > 0 Then
                Dim tous = TousLesNoeuds()
                For Each sorti In horsDeVue.Where(Function(n) Not tous.Contains(n))
                    _deplies.Remove(sorti)
                Next
            End If

            Dim nouveaux = New HashSet(Of Object)(lignes)
            For i = _rangees.Count - 1 To 0 Step -1
                If Not nouveaux.Contains(_rangees(i)) Then _rangees.RemoveAt(i)
            Next
            For i = 0 To lignes.Count - 1
                If i < _rangees.Count AndAlso _rangees(i) Is lignes(i) Then Continue For
                Dim j = _rangees.IndexOf(lignes(i))
                If j >= 0 Then _rangees.Move(j, i) Else _rangees.Insert(i, lignes(i))
            Next
        End Sub

        Private _vus As HashSet(Of Object)

        ' Tous les nœuds de l'arbre, dépliés ou non.
        Private Function TousLesNoeuds() As HashSet(Of Object)
            Dim tous As New HashSet(Of Object)
            Dim pile As New Stack(Of Object)(Racines())
            While pile.Count > 0
                Dim noeud = pile.Pop()
                If Not tous.Add(noeud) Then Continue While
                For Each enfant In EnfantsDe(noeud)
                    pile.Push(enfant)
                Next
            End While
            Return tous
        End Function

        Private Function DansLArbre(noeud As Object) As Boolean
            Return _vus IsNot Nothing AndAlso _vus.Contains(noeud)
        End Function

        Private Sub Parcourir(freres As IList(Of Object), parent As Object, niveau As Integer, guides As List(Of Boolean), lignes As List(Of Object))
            If parent Is Nothing Then _vus = New HashSet(Of Object)
            Dim tries = Trier(freres)
            For i = 0 To tries.Count - 1
                Dim noeud = tries(i)
                _vus.Add(noeud)
                Dim enfants = EnfantsDe(noeud)
                Dim info As TreeGridRowInfo = Nothing
                If Not _infos.TryGetValue(noeud, info) Then
                    info = New TreeGridRowInfo()
                    _infos.Add(noeud, info)
                End If
                info.Parent = parent
                info.Level = niveau
                info.IsLast = i = tries.Count - 1
                info.Guides = guides.ToArray()
                info.HasChildren = enfants.Count > 0
                info.IsExpanded = info.HasChildren AndAlso _deplies.Contains(noeud)
                lignes.Add(noeud)
                Ecouter(CollectionDes(noeud))
                If info.IsExpanded Then
                    ' Le trait d'un nœud du niveau 2 ou plus passe à côté de ses descendants s'il a encore des frères.
                    Dim suivants = New List(Of Boolean)(guides)
                    If niveau >= 2 Then suivants.Add(Not info.IsLast)
                    Parcourir(enfants, noeud, niveau + 1, suivants, lignes)
                End If
            Next
        End Sub

        Private Sub Ecouter(collection As INotifyCollectionChanged)
            If collection Is Nothing OrElse Not _ecoutees.Add(collection) Then Return
            ' Un abonnement faible : un modèle qui survit à sa vue ne retient pas la grille.
            CollectionChangedEventManager.AddHandler(collection, AddressOf OnEnfantsChanged)
        End Sub

        Private Sub OnEnfantsChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            Reconstruire()
        End Sub

    End Class

End Namespace
