Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 25 : Carousel, Galleria, Terminal.</summary>
    <TestFixture>
    Public Class Lot25Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 600, Optional hauteur As Double = 300) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        ' ---- Carousel ----

        Private Shared Function Carrousel(nombre As Integer, visibles As Integer, Optional circulaire As Boolean = False) As Carousel
            Dim c As New Carousel With {.NumVisible = visibles, .Circular = circulaire}
            For i = 1 To nombre
                c.Items.Add($"E{i}")
            Next
            Return Pret(c)
        End Function

        <Test>
        Public Sub Carousel_LesPagesEtLesFleches()
            Dim c = Carrousel(5, 3)
            Assert.That(c.PageCount, [Is].EqualTo(3), "5 éléments, 3 visibles, 1 par page : 3 pages")
            Assert.That(c.CanGoPrevious, [Is].False)
            c.Next()
            c.Next()
            c.Next()
            Assert.That(c.Page, [Is].EqualTo(2), "bornée à la dernière")
            Assert.That(c.CanGoNext, [Is].False)
            Assert.That(c.Pages.Select(Function(p) p.IsActive), [Is].EqualTo({False, False, True}))
            Assert.That(c.Premier, [Is].EqualTo(2))
        End Sub

        <Test>
        Public Sub Carousel_CirculaireRevientAuDebut()
            Dim c = Carrousel(4, 2, circulaire:=True)
            c.Previous()
            Assert.That(c.Page, [Is].EqualTo(c.PageCount - 1))
            c.Next()
            Assert.That(c.Page, [Is].EqualTo(0))
            Assert.That(c.CanGoPrevious AndAlso c.CanGoNext, [Is].True)
        End Sub

        <Test>
        Public Sub Carousel_UnIndicateurMeneASaPage()
            Dim c = Carrousel(6, 2)
            Dim indicateurs = DirectCast(c.Template.FindName("PART_Indicators", c), ItemsControl)
            c.UpdateLayout()
            Dim troisieme = DirectCast(indicateurs.ItemContainerGenerator.ContainerFromIndex(2), ContentPresenter)
            Dim bouton = DirectCast(Media.VisualTreeHelper.GetChild(troisieme, 0), ButtonBase)
            bouton.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, bouton))
            Assert.That(c.Page, [Is].EqualTo(2))
        End Sub

        ' ---- Galleria ----

        <Test>
        Public Sub Galleria_LElementMontreEtLesFleches()
            Dim g = Pret(New Galleria With {.ItemsSource = {"A", "B", "C"}})
            Assert.That(g.ActiveItem, [Is].EqualTo("A"))
            g.Next()
            Assert.That(g.ActiveItem, [Is].EqualTo("B"))
            g.ActiveIndex = 99
            Assert.That(g.ActiveIndex, [Is].EqualTo(2), "borné au dernier")
            g.Next()
            Assert.That(g.ActiveIndex, [Is].EqualTo(2), "sans Circular, rien après le dernier")
            g.Circular = True
            g.Next()
            Assert.That(g.ActiveIndex, [Is].EqualTo(0))
            Assert.That(g.Positions.Count, [Is].EqualTo(3))
        End Sub

        ' ---- Terminal ----

        <Test>
        Public Sub Terminal_UneCommandeEtSaReponse()
            Dim t = Pret(New Terminal With {.Prompt = "$"})
            AddHandler t.CommandEntered, Sub(s, e) If e.Command = "date" Then e.Response = "aujourd'hui"
            t.Send("date")
            t.Send("plus tard")
            t.Send("   ")
            Assert.That(t.Entries.Count, [Is].EqualTo(2), "une commande vide ne compte pas")
            Assert.That(t.Entries(0).Response, [Is].EqualTo("aujourd'hui"))
            Assert.That(t.Entries(1).Response, [Is].Null)
            t.Respond("enfin")
            Assert.That(t.Entries(1).Response, [Is].EqualTo("enfin"), "une réponse qui vient plus tard")
            Assert.That(t.Entries(0).Prompt, [Is].EqualTo("$"))
            t.Clear()
            Assert.That(t.Entries, [Is].Empty)
        End Sub

        <Test>
        Public Sub Terminal_EntreeEnvoieLaSaisie()
            Dim t = Pret(New Terminal())
            Dim saisie = DirectCast(t.Template.FindName("PART_Input", t), TextBox)
            Assert.That(Automation.AutomationProperties.GetName(saisie), [Is].Not.Empty, "la saisie est nommée")
        End Sub

    End Class

End Namespace
