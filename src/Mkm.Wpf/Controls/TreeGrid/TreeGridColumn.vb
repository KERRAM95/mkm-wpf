Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La colonne de l'arbre d'une <see cref="TreeGrid"/> : pour chaque nœud, ses traits, sa case de dépliage, son icône
    ''' (<see cref="IconBinding"/>) et son libellé (<c>Binding</c>). Elle se trie sur le chemin de son <c>Binding</c>.
    ''' </summary>
    Public Class TreeGridColumn
        Inherits DataGridBoundColumn

        ''' <summary>La liaison de l'icône du nœud (une icône de <see cref="Icons"/>, ou rien).</summary>
        Public Property IconBinding As BindingBase

        ''' <summary>Une colonne d'arbre, en lecture seule ; sa grille lui donne le style de ses cellules.</summary>
        Public Sub New()
            IsReadOnly = True
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function GenerateElement(cell As DataGridCell, dataItem As Object) As FrameworkElement
            Dim grille = TryCast(DataGridOwner, TreeGrid)
            Dim cellule As New TreeGridCell With {.Grid = grille}
            Dim info = grille?.GetRowInfo(dataItem)
            If info IsNot Nothing Then cellule.Info = info
            If Binding IsNot Nothing Then BindingOperations.SetBinding(cellule, ContentControl.ContentProperty, Binding)
            If IconBinding IsNot Nothing Then BindingOperations.SetBinding(cellule, TreeGridCell.IconProperty, IconBinding)
            Return cellule
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GenerateEditingElement(cell As DataGridCell, dataItem As Object) As FrameworkElement
            Return GenerateElement(cell, dataItem)
        End Function

    End Class

    ''' <summary>
    ''' La cellule de l'arbre d'une <see cref="TreeGrid"/> : les traits gris clair de ses ancêtres et le sien, la case de
    ''' dépliage (15 px, + ou −) posée sur le trait de son parent, son icône (22 px) et son libellé, 7 px entre eux. Les
    ''' colonnes de l'arbre sont à 19 px + 25 px par niveau.
    ''' </summary>
    <TemplatePart(Name:="PART_Toggle", Type:=GetType(ButtonBase))>
    Public Class TreeGridCell
        Inherits ContentControl

        ''' <summary>La place du nœud dans l'arbre.</summary>
        Public Shared ReadOnly InfoProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Info), GetType(TreeGridRowInfo), GetType(TreeGridCell),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.AffectsRender, AddressOf OnInfoChanged))

        ''' <summary>L'icône du nœud.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(TreeGridCell), New FrameworkPropertyMetadata(Nothing))

        Private Shared ReadOnly IndentPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Indent), GetType(Thickness), GetType(TreeGridCell), New FrameworkPropertyMetadata(New Thickness()))
        ''' <summary>Le retrait de la case de dépliage : elle se pose sur le trait du parent.</summary>
        Public Shared ReadOnly IndentProperty As DependencyProperty = IndentPropertyKey.DependencyProperty

        Private Shared ReadOnly HasChildrenPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasChildren), GetType(Boolean), GetType(TreeGridCell), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai si le nœud a des enfants : sa case de dépliage paraît.</summary>
        Public Shared ReadOnly HasChildrenProperty As DependencyProperty = HasChildrenPropertyKey.DependencyProperty

        Private Shared ReadOnly IsExpandedPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsExpanded), GetType(Boolean), GetType(TreeGridCell), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai si le nœud est déplié : sa case montre −.</summary>
        Public Shared ReadOnly IsExpandedProperty As DependencyProperty = IsExpandedPropertyKey.DependencyProperty

        Private _bascule As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(TreeGridCell), New FrameworkPropertyMetadata(GetType(TreeGridCell)))
            FocusableProperty.OverrideMetadata(GetType(TreeGridCell), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une cellule d'arbre : elle suit le nœud de sa ligne.</summary>
        Public Sub New()
            ' Une ligne recyclée par la grille garde ses cellules : seul le nœud change ; sa place suit.
            AddHandler DataContextChanged, Sub(s, e) Lier()
        End Sub

        ''' <summary>La grille de la cellule.</summary>
        Friend Property Grid As TreeGrid

        ''' <summary>Prend la place du nœud de la ligne dans la grille.</summary>
        Friend Sub Lier()
            Dim grille = If(Grid, TryCast(ItemsControl.ItemsControlFromItemContainer(DataGridRow.GetRowContainingElement(Me)), TreeGrid))
            Dim place = grille?.GetRowInfo(DataContext)
            If place IsNot Nothing Then Info = place
        End Sub

        ''' <summary>La place du nœud dans l'arbre.</summary>
        Public Property Info As TreeGridRowInfo
            Get
                Return DirectCast(GetValue(InfoProperty), TreeGridRowInfo)
            End Get
            Set(value As TreeGridRowInfo)
                SetValue(InfoProperty, value)
            End Set
        End Property

        ''' <summary>L'icône du nœud.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le nœud a des enfants.</summary>
        Public ReadOnly Property HasChildren As Boolean
            Get
                Return CBool(GetValue(HasChildrenProperty))
            End Get
        End Property

        ''' <summary>Vrai si le nœud est déplié.</summary>
        Public ReadOnly Property IsExpanded As Boolean
            Get
                Return CBool(GetValue(IsExpandedProperty))
            End Get
        End Property

        ''' <summary>Le retrait de la case de dépliage.</summary>
        Public ReadOnly Property Indent As Thickness
            Get
                Return DirectCast(GetValue(IndentProperty), Thickness)
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _bascule IsNot Nothing Then RemoveHandler _bascule.Click, AddressOf OnBascule
            MyBase.OnApplyTemplate()
            _bascule = TryCast(GetTemplateChild("PART_Toggle"), ButtonBase)
            If _bascule IsNot Nothing Then AddHandler _bascule.Click, AddressOf OnBascule
        End Sub

        Private Sub OnBascule(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            Dim grille = If(Grid, TryCast(ItemsControl.ItemsControlFromItemContainer(DataGridRow.GetRowContainingElement(Me)), TreeGrid))
            grille?.Toggle(DataContext)
        End Sub

        Private Function Mesure(cle As String, defaut As Double) As Double
            Dim valeur = TryFindResource("mkm.treegrid." & cle)
            Return If(TypeOf valeur Is Double, CDbl(valeur), defaut)
        End Function

        Private Sub MettreAJour()
            Dim niveau = If(Info Is Nothing, 1, Info.Level)
            Dim taille = Mesure("node.toggle.button.size", 15)
            SetValue(IndentPropertyKey, New Thickness(Mesure("tree.origin", 19) + Mesure("tree.indent", 25) * (niveau - 1) - (taille - 1) / 2, 0, 0, 0))
            SetValue(HasChildrenPropertyKey, Info IsNot Nothing AndAlso Info.HasChildren)
            SetValue(IsExpandedPropertyKey, Info IsNot Nothing AndAlso Info.IsExpanded)
            InvalidateVisual()
        End Sub

        Private Shared Sub OnInfoChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim cellule = DirectCast(d, TreeGridCell)
            Dim ancienne = TryCast(e.OldValue, TreeGridRowInfo)
            If ancienne IsNot Nothing Then RemoveHandler ancienne.PropertyChanged, AddressOf cellule.OnInfoProprieteChanged
            Dim nouvelle = TryCast(e.NewValue, TreeGridRowInfo)
            If nouvelle IsNot Nothing Then AddHandler nouvelle.PropertyChanged, AddressOf cellule.OnInfoProprieteChanged
            cellule.MettreAJour()
        End Sub

        Private Sub OnInfoProprieteChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
            MettreAJour()
        End Sub

        ''' <summary>
        ''' Les traits : ceux des ancêtres qui ont encore des frères plus bas, sur toute la hauteur ; le sien, qui relie le
        ''' nœud à son parent, jusqu'à son milieu seulement s'il est le dernier de ses frères ; une feuille, un trait court
        ''' vers la droite à mi-hauteur (la règle de mkm). Les racines n'en ont pas.
        ''' </summary>
        Protected Overrides Sub OnRender(drawingContext As DrawingContext)
            MyBase.OnRender(drawingContext)
            Dim i = Info
            If i Is Nothing OrElse i.Level < 2 Then Return
            Dim pinceau = TryCast(TryFindResource("mkm.treegrid.tree.line.color"), Brush)
            If pinceau Is Nothing Then Return
            Dim crayon As New Pen(pinceau, 1)
            crayon.Freeze()
            Dim origine = Mesure("tree.origin", 19)
            Dim retrait = Mesure("tree.indent", 25)
            Dim taille = Mesure("node.toggle.button.size", 15)
            Dim hauteur = ActualHeight
            ' Au demi-pixel : un trait d'un pixel, net.
            Dim x = Function(profondeur As Integer) Math.Round(origine + retrait * profondeur) + 0.5
            For d = 0 To i.Guides.Length - 1
                If i.Guides(d) Then drawingContext.DrawLine(crayon, New Point(x(d + 1), 0), New Point(x(d + 1), hauteur))
            Next
            Dim sien = x(i.Level - 1)
            Dim milieu = Math.Round(hauteur / 2) + 0.5
            drawingContext.DrawLine(crayon, New Point(sien, 0), New Point(sien, If(i.IsLast, milieu, hauteur)))
            If Not i.HasChildren Then drawingContext.DrawLine(crayon, New Point(sien, milieu), New Point(sien + (taille + 1) / 2, milieu))
        End Sub

    End Class

End Namespace
