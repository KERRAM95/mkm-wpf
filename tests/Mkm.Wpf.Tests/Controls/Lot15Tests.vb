Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 15 : Stepper, Timeline, Paginator.</summary>
    <TestFixture>
    Public Class Lot15Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 600, Optional hauteur As Double = 300) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        ' ---- Timeline ----

        Private Shared Function Frise(Optional cote As TimelineAlign = TimelineAlign.Left, Optional sens As Orientation = Orientation.Vertical) As Timeline
            Dim f As New Timeline With {.Align = cote, .Layout = sens}
            For Each texte In {"Création", "Revue", "Validation"}
                f.Items.Add(New TimelineEvent With {.Content = texte, .Opposite = "2026"})
            Next
            Return Pret(f)
        End Function

        Private Shared Function Evenement(f As Timeline, i As Integer) As TimelineEvent
            Return DirectCast(f.Items(i), TimelineEvent)
        End Function

        <Test>
        Public Sub Timeline_LeDernierSansTraitEtLesCotes()
            Dim f = Frise()
            Assert.That(Evenement(f, 2).IsLast, [Is].True)
            Assert.That(Evenement(f, 0).IsLast, [Is].False)
            Assert.That(Evenement(f, 0).IsReversed OrElse Evenement(f, 1).IsReversed, [Is].False, "Left : le contenu à droite")

            Dim alterne = Frise(TimelineAlign.Alternate)
            Assert.That(Evenement(alterne, 0).IsReversed, [Is].False)
            Assert.That(Evenement(alterne, 1).IsReversed, [Is].True, "un sur deux retourné")

            Dim enLigne = Frise(TimelineAlign.Bottom, Orientation.Horizontal)
            Assert.That(Evenement(enLigne, 0).Layout, [Is].EqualTo(Orientation.Horizontal))
            Assert.That(Evenement(enLigne, 0).IsReversed, [Is].True, "en ligne, Bottom échange")
        End Sub

        <Test>
        Public Sub Timeline_EnLigneLeDernierGardeSaLargeur()
            Dim f = Frise(sens:=Orientation.Horizontal)
            Assert.That(Evenement(f, 0).ActualWidth, [Is].EqualTo(Evenement(f, 1).ActualWidth).Within(0.5), "les autres se partagent la place")
            Assert.That(Evenement(f, 2).ActualWidth, [Is].LessThan(Evenement(f, 0).ActualWidth))
        End Sub

        ' ---- Paginator ----

        <Test>
        Public Sub Paginator_LesPagesEtLesLiens()
            Dim p = Pret(New Paginator With {.TotalRecords = 95, .Rows = 10})
            Assert.That(p.PageCount, [Is].EqualTo(10))
            Assert.That(p.Pages.Select(Function(l) l.Number), [Is].EqualTo({1, 2, 3, 4, 5}))

            Dim pages As New Collections.Generic.List(Of Integer)
            AddHandler p.PageChange, Sub(s, e) pages.Add(e.Page)
            p.ChangePage(6)
            Assert.That(p.First, [Is].EqualTo(60))
            Assert.That(p.Page, [Is].EqualTo(6))
            Assert.That(p.Pages.Select(Function(l) l.Number), [Is].EqualTo({5, 6, 7, 8, 9}), "centrés sur la page montrée")
            Assert.That(p.Pages.Single(Function(l) l.IsCurrent).Number, [Is].EqualTo(7))
            p.ChangePage(99)
            Assert.That(p.Page, [Is].EqualTo(9), "bornée à la dernière")
            Assert.That(p.IsLastPage, [Is].True)
            Assert.That(pages, [Is].EqualTo({6, 9}))
        End Sub

        <Test>
        Public Sub Paginator_UnAutreNombreDeLignesGardeLaPremiereLigne()
            Dim p = Pret(New Paginator With {.TotalRecords = 120, .Rows = 10})
            p.ChangePage(5)
            p.Rows = 25
            Assert.That(p.First, [Is].EqualTo(50), "la page qui contient la ligne 51")
            Assert.That(p.Page, [Is].EqualTo(2))
            Assert.That(p.PageCount, [Is].EqualTo(5))
        End Sub

        <Test>
        Public Sub Paginator_LesChoixDeLignesSEcriventEnTexte()
            Dim convertisseur As New IntegerListConverter()
            Assert.That(convertisseur.ConvertFrom("10, 20, 50"), [Is].EqualTo({10, 20, 50}))
        End Sub

        ' ---- Stepper ----

        Private Shared Function Etapes() As Stepper
            Dim s As New Stepper()
            For Each titre In {"Plan", "Cartouche", "Diffusion"}
                s.Items.Add(New StepperPanel With {.Header = titre, .Content = New TextBlock With {.Text = titre}})
            Next
            Return Pret(s)
        End Function

        <Test>
        Public Sub Stepper_NumerotEtMarqueLesEtapesPassees()
            Dim s = Etapes()
            Dim etape = Function(i As Integer) DirectCast(s.Items(i), StepperPanel)
            Assert.That(etape(0).Number, [Is].EqualTo(1))
            Assert.That(etape(2).Number, [Is].EqualTo(3))
            Assert.That(etape(2).IsLast, [Is].True)

            s.SelectedIndex = 2
            Assert.That(etape(0).IsCompleted AndAlso etape(1).IsCompleted, [Is].True, "avant l'étape en cours")
            Assert.That(etape(2).IsCompleted, [Is].False)
        End Sub

        <Test>
        Public Sub Stepper_NextEtPreviousBornes()
            Dim s = Etapes()
            s.SelectedIndex = 0
            s.Previous()
            Assert.That(s.SelectedIndex, [Is].EqualTo(0))
            s.Next()
            s.Next()
            s.Next()
            Assert.That(s.SelectedIndex, [Is].EqualTo(2), "rien après la dernière")
        End Sub

        <Test>
        Public Sub Stepper_EnColonneLeContenuSousSonEtape()
            Dim s = Etapes()
            s.Orientation = Orientation.Vertical
            s.UpdateLayout()
            Assert.That(DirectCast(s.Items(1), StepperPanel).Orientation, [Is].EqualTo(Orientation.Vertical))
        End Sub

    End Class

End Namespace
