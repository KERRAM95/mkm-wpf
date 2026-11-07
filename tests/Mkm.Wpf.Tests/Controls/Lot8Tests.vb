Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les composants du huitième lot : TreeGrid et StateGraph.</summary>
    <TestFixture>
    Public Class Lot8Tests

        Public Class Noeud
            Public Property Nom As String
            Public Property Revision As String
            Public Property Children As New ObservableCollection(Of Noeud)

            Public Sub New(nom As String, ParamArray enfants As Noeud())
                Me.Nom = nom
                Revision = "A"
                For Each enfant In enfants
                    Children.Add(enfant)
                Next
            End Sub

            Public Overrides Function ToString() As String
                Return Nom
            End Function
        End Class

        Private Shared Function Pret(Of T As FrameworkElement)(element As T) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(600, 400))
            element.Arrange(New Rect(0, 0, 600, 400))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        Private _plan As Noeud
        Private _vues As Noeud
        Private _cartouche As Noeud
        Private _article As Noeud

        Private Function Grille() As TreeGrid
            _cartouche = New Noeud("Cartouche")
            _vues = New Noeud("Vues", New Noeud("Face"), New Noeud("Dessus"))
            _plan = New Noeud("Plan", _vues, _cartouche)
            _article = New Noeud("Article")
            Dim g As New TreeGrid With {.Nodes = New ObservableCollection(Of Noeud)({_plan, _article})}
            g.Columns.Add(New TreeGridColumn With {.Header = "Nom", .Binding = New Binding("Nom"), .SortMemberPath = "Nom"})
            g.Columns.Add(New DataGridTextColumn With {.Header = "Révision", .Binding = New Binding("Revision")})
            Return Pret(g)
        End Function

        Private Shared Function Noms(g As TreeGrid) As String()
            Return g.Items.Cast(Of Noeud)().Select(Function(n) n.Nom).ToArray()
        End Function

        <Test>
        Public Sub TreeGrid_LesRacines_PuisLeDepliage()
            Dim g = Grille()
            Assert.That(Noms(g), [Is].EqualTo({"Plan", "Article"}))

            g.Expand(_plan)
            Assert.That(Noms(g), [Is].EqualTo({"Plan", "Vues", "Cartouche", "Article"}))
            g.Expand(_vues)
            Assert.That(Noms(g), [Is].EqualTo({"Plan", "Vues", "Face", "Dessus", "Cartouche", "Article"}))

            Dim face = g.GetRowInfo(_vues.Children(0))
            Assert.That(face.Level, [Is].EqualTo(3))
            Assert.That(face.Guides, [Is].EqualTo({True}), "le trait de Vues passe : Cartouche le suit")
            Assert.That(g.GetRowInfo(_cartouche).IsLast, [Is].True)
            Assert.That(g.GetRowInfo(_plan).IsExpanded, [Is].True)

            g.Collapse(_plan)
            Assert.That(Noms(g), [Is].EqualTo({"Plan", "Article"}))
            g.ExpandAll()
            Assert.That(g.Items.Count, [Is].EqualTo(6))
        End Sub

        <Test>
        Public Sub TreeGrid_LeTriGardeLArbre()
            Dim g = Grille()
            g.ExpandAll()

            g.TrierPar(g.Columns(0))
            Assert.That(Noms(g), [Is].EqualTo({"Article", "Plan", "Cartouche", "Vues", "Dessus", "Face"}))

            g.TrierPar(g.Columns(0))
            Assert.That(Noms(g), [Is].EqualTo({"Plan", "Vues", "Face", "Dessus", "Cartouche", "Article"}))
            Assert.That(g.Columns(0).SortDirection, [Is].EqualTo(ComponentModel.ListSortDirection.Descending))
        End Sub

        <Test>
        Public Sub TreeGrid_UnEnfantAjoute_LaSelectionGardee()
            Dim g = Grille()
            g.Expand(_plan)
            g.SelectedItem = _article

            _plan.Children.Add(New Noeud("Nomenclature"))

            Assert.That(Noms(g), [Is].EqualTo({"Plan", "Vues", "Cartouche", "Nomenclature", "Article"}))
            Assert.That(g.SelectedItem, [Is].SameAs(_article))
            Assert.That(g.GetRowInfo(_cartouche).IsLast, [Is].False, "Nomenclature le suit")
        End Sub

        <Test>
        Public Sub TreeGrid_LaCelluleDeLArbre()
            Dim g = Grille()
            g.Expand(_plan)
            g.UpdateLayout()
            Dim ligne = DirectCast(g.ItemContainerGenerator.ContainerFromItem(_vues), DataGridRow)
            Assert.That(ligne, [Is].Not.Null)
            Dim cellule = TrouverDescendant(Of TreeGridCell)(ligne)

            Assert.That(cellule.Info.Level, [Is].EqualTo(2))
            ' 19 + 25 × (2 - 1) - (15 - 1) / 2 : la case se pose sur le trait du parent.
            Assert.That(cellule.Indent.Left, [Is].EqualTo(37))
            Assert.That(cellule.Content, [Is].EqualTo("Vues"))
            Dim feuille = TrouverDescendant(Of TreeGridCell)(DirectCast(g.ItemContainerGenerator.ContainerFromItem(_cartouche), DataGridRow))
            Assert.That(Partie(Of UIElement)(feuille, "PART_Toggle").Visibility, [Is].EqualTo(Visibility.Hidden), "une feuille n'a pas de case")
        End Sub

        Private Shared Function TrouverDescendant(Of T As DependencyObject)(racine As DependencyObject) As T
            For i = 0 To VisualTreeHelper.GetChildrenCount(racine) - 1
                Dim enfant = VisualTreeHelper.GetChild(racine, i)
                If TypeOf enfant Is T Then Return DirectCast(enfant, T)
                Dim trouve = TrouverDescendant(Of T)(enfant)
                If trouve IsNot Nothing Then Return trouve
            Next
            Return Nothing
        End Function

        Private Shared Function Graphe() As StateGraph
            Dim g As New StateGraph With {.Current = "work"}
            Dim etats = DirectCast(g.States, ObservableCollection(Of StateGraphState))
            etats.Add(New StateGraphState With {.Id = "private", .Label = "Privé", .Color = Brushes.Gray})
            etats.Add(New StateGraphState With {.Id = "work", .Label = "En cours", .Color = Brushes.SteelBlue})
            etats.Add(New StateGraphState With {.Id = "frozen", .Label = "Gelé", .Color = Brushes.SkyBlue})
            etats.Add(New StateGraphState With {.Id = "released", .Label = "Validé", .Color = Brushes.Green})
            Dim transitions = DirectCast(g.Transitions, ObservableCollection(Of StateGraphTransition))
            transitions.Add(New StateGraphTransition With {.From = "private", .To = "work"})
            transitions.Add(New StateGraphTransition With {.From = "work", .To = "frozen"})
            transitions.Add(New StateGraphTransition With {.From = "frozen", .To = "released"})
            Dim actions = DirectCast(g.Actions, ObservableCollection(Of StateGraphAction))
            actions.Add(New StateGraphAction With {.To = "frozen", .Label = "Geler"})
            actions.Add(New StateGraphAction With {.To = "released", .Label = "Valider"})
            actions.Add(New StateGraphAction With {.To = "private", .Label = "Rétrograder"})
            Return Pret(g)
        End Function

        <Test>
        Public Sub StateGraph_SesEtatsEtSesActions()
            Dim g = Graphe()
            Dim toile = Partie(Of Canvas)(g, "PART_Canvas")
            Dim boites = toile.Children.OfType(Of Border)().ToList()
            Dim noms = toile.Children.OfType(Of TextBlock)().ToList()

            Assert.That(boites.Count, [Is].EqualTo(4))
            Assert.That(boites(1).BorderThickness, [Is].EqualTo(New Thickness(2)), "l'état courant, cerné")
            Assert.That(noms.Count, [Is].EqualTo(3), "un nom par action")
            ' 130 × 4 + 20 ; un couloir dessus (Valider) et un dessous (Rétrograder : Privé mène à l'état courant) : 2 × 21 + 85.
            Assert.That(toile.Width, [Is].EqualTo(540))
            Assert.That(toile.Height, [Is].EqualTo(127))
        End Sub

        <Test>
        Public Sub StateGraph_UneActionCliquee_SaufPendantUneAutre()
            Dim g = Graphe()
            Dim choisie As StateGraphAction = Nothing
            AddHandler g.ActionClick, Sub(s, e) choisie = e.Action
            Dim geler = g.Actions.OfType(Of StateGraphAction)().First()

            g.InvokeAction(geler)
            Assert.That(choisie, [Is].SameAs(geler))

            choisie = Nothing
            g.Pending = "frozen"
            g.InvokeAction(geler)
            Assert.That(choisie, [Is].Null, "une action est en cours")
        End Sub

    End Class

End Namespace
