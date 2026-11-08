Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 23 : OrderList, PickList, DataView.</summary>
    <TestFixture>
    Public Class Lot23Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 600, Optional hauteur As Double = 300) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Choisir(liste As OrderList, ParamArray elements As String()) As OrderList
            liste.Liste.SelectedItems.Clear()
            For Each e In elements
                liste.Liste.SelectedItems.Add(e)
            Next
            Return liste
        End Function

        ' ---- OrderList ----

        <Test>
        Public Sub OrderList_MonterEtDescendreDUnRang()
            Dim etapes As New ObservableCollection(Of String) From {"A", "B", "C", "D"}
            Dim liste = Pret(New OrderList With {.ItemsSource = etapes})
            Dim reordonnee = 0
            AddHandler liste.Reorder, Sub(s, e) reordonnee += 1
            Choisir(liste, "C").MoveUp()
            Assert.That(etapes, [Is].EqualTo({"A", "C", "B", "D"}))
            Assert.That(liste.SelectedItems.Cast(Of String)(), [Is].EqualTo({"C"}), "le choix suit l'élément")
            Choisir(liste, "A", "B").MoveDown()
            Assert.That(etapes, [Is].EqualTo({"C", "A", "D", "B"}), "chacun d'un rang")
            Choisir(liste, "C").MoveUp()
            Assert.That(reordonnee, [Is].EqualTo(2), "rien à monter : pas de Reorder")
        End Sub

        <Test>
        Public Sub OrderList_ToutEnHautEtToutEnBasGardentLOrdre()
            Dim etapes As New ObservableCollection(Of String) From {"A", "B", "C", "D", "E"}
            Dim liste = Pret(New OrderList With {.ItemsSource = etapes})
            Choisir(liste, "D", "B").MoveTop()
            Assert.That(etapes, [Is].EqualTo({"B", "D", "A", "C", "E"}))
            Choisir(liste, "A", "B").MoveBottom()
            Assert.That(etapes, [Is].EqualTo({"D", "C", "E", "B", "A"}))
        End Sub

        <Test>
        Public Sub OrderList_UneListeSansMoveSOrdonneAussi()
            Dim etapes As New Collections.ArrayList From {"A", "B", "C"}
            Dim liste = Pret(New OrderList With {.ItemsSource = etapes})
            Choisir(liste, "C").MoveTop()
            Assert.That(etapes, [Is].EqualTo({"C", "A", "B"}))
        End Sub

        ' ---- PickList ----

        <Test>
        Public Sub PickList_LesElementsPassentDUneListeALAutre()
            Dim source As New ObservableCollection(Of String) From {"Achats", "Méthodes", "Qualité"}
            Dim cible As New ObservableCollection(Of String) From {"BE"}
            Dim choix = Pret(New PickList With {.Source = source, .Target = cible})
            Dim passes As New Collections.Generic.List(Of Object)
            AddHandler choix.MovedToTarget, Sub(s, e) passes.AddRange(e.Items.Cast(Of Object)())
            Dim gauche = DirectCast(choix.Template.FindName("PART_Source", choix), OrderList)
            Choisir(gauche, "Qualité", "Achats")
            choix.MoveToTarget()
            Assert.That(source, [Is].EqualTo({"Méthodes"}))
            Assert.That(cible, [Is].EqualTo({"BE", "Achats", "Qualité"}), "dans leur ordre, à la fin")
            Assert.That(passes, [Is].EqualTo({"Achats", "Qualité"}))
            choix.MoveAllToSource()
            Assert.That(cible, [Is].Empty)
            Assert.That(source.Count, [Is].EqualTo(4))
        End Sub

        <Test>
        Public Sub PickList_LesDeuxListesOntLaMemeLargeur()
            Dim choix As New PickList With {
                .Source = New ObservableCollection(Of String) From {"QA"},
                .Target = New ObservableCollection(Of String) From {"Bureau d'études et méthodes"}}
            choix.BeginInit()
            choix.EndInit()
            choix.ApplyTemplate()
            choix.Measure(New Size(Double.PositiveInfinity, 300))
            choix.Arrange(New Rect(choix.DesiredSize))
            choix.UpdateLayout()
            Dim gauche = DirectCast(choix.Template.FindName("PART_Source", choix), OrderList)
            Dim droite = DirectCast(choix.Template.FindName("PART_Target", choix), OrderList)
            Assert.That(gauche.ActualWidth, [Is].EqualTo(droite.ActualWidth).Within(0.5), "à sa taille naturelle")
            choix.Measure(New Size(800, 300))
            choix.Arrange(New Rect(0, 0, 800, 300))
            choix.UpdateLayout()
            Assert.That(gauche.ActualWidth, [Is].EqualTo(droite.ActualWidth).Within(0.5), "étirée")
            Assert.That(gauche.ActualWidth, [Is].GreaterThan(300))
        End Sub

        ' ---- DataView ----

        <Test>
        Public Sub DataView_LaPageMontreRowsElementsDepuisFirst()
            Dim pieces = Enumerable.Range(1, 12).Select(Function(i) $"P{i}").ToList()
            Dim vue = Pret(New DataView With {.ItemsSource = pieces, .Paginator = True, .Rows = 5})
            Assert.That(vue.TotalRecords, [Is].EqualTo(12))
            Assert.That(vue.PageItems.Cast(Of String)(), [Is].EqualTo({"P1", "P2", "P3", "P4", "P5"}))
            vue.First = 10
            Assert.That(vue.PageItems.Cast(Of String)(), [Is].EqualTo({"P11", "P12"}))
            vue.Paginator = False
            Assert.That(vue.PageItems.Cast(Of String)().Count, [Is].EqualTo(12), "sans paginateur, tout")
        End Sub

        <Test>
        Public Sub DataView_SuitLaListeEtDitSonVide()
            Dim pieces As New ObservableCollection(Of String)
            Dim vue = Pret(New DataView With {.ItemsSource = pieces})
            Assert.That(vue.IsEmpty, [Is].True)
            pieces.Add("P1")
            Assert.That(vue.IsEmpty, [Is].False)
            Assert.That(vue.PageItems.Cast(Of String)(), [Is].EqualTo({"P1"}))
        End Sub

        <Test>
        Public Sub DataView_LePaginateurSuitFirst()
            Dim vue = Pret(New DataView With {.ItemsSource = Enumerable.Range(1, 30).ToList(), .Paginator = True, .Rows = 10})
            Dim pages = DirectCast(vue.Template.FindName("PART_Paginator", vue), Paginator)
            pages.ChangePage(2)
            Assert.That(vue.First, [Is].EqualTo(20), "le paginateur pousse First")
            Assert.That(vue.PageItems.Cast(Of Integer)().First(), [Is].EqualTo(21))
        End Sub

    End Class

End Namespace
