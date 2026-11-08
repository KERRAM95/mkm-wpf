Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Input
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 18 : Rating, Slider, Knob.</summary>
    <TestFixture>
    Public Class Lot18Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 300, Optional hauteur As Double = 200) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Sub Cliquer(element As UIElement)
            element.RaiseEvent(New MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) With {.RoutedEvent = UIElement.MouseLeftButtonUpEvent})
        End Sub

        ' ---- Rating ----

        <Test>
        Public Sub Rating_PleineJusquALaNote()
            Dim note = Pret(New Rating With {.Value = 3})
            Assert.That(note.Etoiles.Count, [Is].EqualTo(5))
            Assert.That(note.Etoiles.Select(Function(e) e.IsActive), [Is].EqualTo({True, True, True, False, False}))
            Assert.That(note.Etoiles(0).Filled, [Is].True, "l'étoile pleine")
            Assert.That(note.Etoiles(4).Filled, [Is].False)

            note.Stars = 2
            Assert.That(note.Value, [Is].EqualTo(2), "bornée au nombre d'étoiles")
            Assert.That(note.Etoiles.Count, [Is].EqualTo(2))
        End Sub

        <Test>
        Public Sub Rating_UnClicDonneLaNoteEtUnClicSurLaNoteLaRetire()
            Dim note = Pret(New Rating With {.Value = 1})
            Dim changements As New Collections.Generic.List(Of Integer)
            AddHandler note.ValueChanged, Sub(s, e) changements.Add(e.NewValue)
            Cliquer(note.Etoiles(3))
            Assert.That(note.Value, [Is].EqualTo(4))
            Cliquer(note.Etoiles(3))
            Assert.That(note.Value, [Is].EqualTo(0), "un clic sur la note la retire")
            Assert.That(changements, [Is].EqualTo({4, 0}))

            note.IsReadOnly = True
            Cliquer(note.Etoiles(1))
            Assert.That(note.Value, [Is].EqualTo(0), "en lecture seule, rien ne change")
        End Sub

        <Test>
        Public Sub Rating_UnCurseurPourLesLecteursDEcran()
            Dim note = Pret(New Rating With {.Value = 2})
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(note)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Slider))
            Dim plage = DirectCast(pair.GetPattern(PatternInterface.RangeValue), IRangeValueProvider)
            Assert.That(plage.Value, [Is].EqualTo(2))
            Assert.That(plage.Maximum, [Is].EqualTo(5))
            plage.SetValue(5)
            Assert.That(note.Value, [Is].EqualTo(5))
            Assert.That(pair.GetChildren(), [Is].Null, "les étoiles ne sont que son dessin")
        End Sub

        ' ---- Slider ----

        <Test>
        Public Sub Slider_SaPoigneeEtSaPlage()
            Dim curseur = Pret(New Global.Mkm.Wpf.Controls.Slider With {.Maximum = 100, .Value = 40})
            Dim piste = DirectCast(curseur.Template.FindName("PART_Track", curseur), Primitives.Track)
            Assert.That(piste, [Is].Not.Null)
            Assert.That(piste.Thumb, [Is].Not.Null)
            Assert.That(piste.DecreaseRepeatButton.ActualWidth, [Is].GreaterThan(0), "la plage, jusqu'à la valeur")
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(curseur).GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Slider))
            Assert.That(curseur.SmallChange, [Is].EqualTo(1), "le pas de mkm, et non 0,1")
            Assert.That(curseur.LargeChange, [Is].EqualTo(10))
        End Sub

        ' ---- Knob ----

        <Test>
        Public Sub Knob_LeHautDeLArcEstLeMilieu()
            Dim bouton = Pret(New Knob With {.Value = 0})
            bouton.Tourner(New Point(50, 10))
            Assert.That(bouton.Value, [Is].EqualTo(50).Within(0.001))
            bouton.Tourner(New Point(50, 95))
            Assert.That(bouton.Value, [Is].EqualTo(50).Within(0.001), "le bas, hors de l'arc, ne compte pas")
            bouton.Tourner(New Point(10, 50))
            Assert.That(bouton.Value, [Is].EqualTo(20).Within(0.001), "à gauche : 180°, le cinquième de l'arc")
        End Sub

        <Test>
        Public Sub Knob_AuPasEtAuFormat()
            Dim bouton = Pret(New Knob With {.SmallChange = 10, .ValueFormat = "{0} %"})
            bouton.Tourner(New Point(50, 10))
            Assert.That(bouton.Value, [Is].EqualTo(50))
            bouton.Tourner(New Point(60, 10))
            Assert.That(CInt(bouton.Value) Mod 10, [Is].EqualTo(0), "au pas de 10")
            bouton.Value = 30
            Assert.That(bouton.TexteDeLaValeur, [Is].EqualTo("30 %"))
        End Sub

        <Test>
        Public Sub Knob_UnCurseurPourLesLecteursDEcran()
            Dim bouton = Pret(New Knob With {.Value = 40})
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(bouton)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Slider))
            Assert.That(DirectCast(pair.GetPattern(PatternInterface.RangeValue), IRangeValueProvider).Value, [Is].EqualTo(40))
            Assert.That(bouton.ActualWidth, [Is].EqualTo(100), "Size : 100 par défaut")
        End Sub

    End Class

End Namespace
