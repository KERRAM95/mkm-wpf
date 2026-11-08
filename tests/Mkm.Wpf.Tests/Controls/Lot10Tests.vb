Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Shapes
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 10 : les barres de défilement et ScrollPanel, Divider, ToggleSwitch, Skeleton.</summary>
    <TestFixture>
    Public Class Lot10Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 400, Optional hauteur As Double = 200) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        ' ---- ToggleSwitch ----

        <Test>
        Public Sub ToggleSwitch_SaPoigneePasseADroiteActive()
            Dim interrupteur = Pret(New ToggleSwitch With {.Content = "Synchroniser"})
            Dim poignee = Partie(Of Ellipse)(interrupteur, "Poignee")
            Assert.That(poignee.HorizontalAlignment, [Is].EqualTo(HorizontalAlignment.Left))

            interrupteur.IsChecked = True
            Assert.That(poignee.HorizontalAlignment, [Is].EqualTo(HorizontalAlignment.Right))
        End Sub

        <Test>
        Public Sub ToggleSwitch_UnLecteurDEcranLeBascule()
            Dim interrupteur = Pret(New ToggleSwitch With {.Content = "Synchroniser"})
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(interrupteur)

            Assert.That(pair.GetName(), [Is].EqualTo("Synchroniser"))
            DirectCast(pair.GetPattern(PatternInterface.Toggle), IToggleProvider).Toggle()
            Assert.That(interrupteur.IsChecked, [Is].True)
        End Sub

        ' ---- Divider ----

        <Test>
        Public Sub Divider_SonContenuSePlaceSelonAlign()
            Dim trait = Pret(New Divider With {.Content = "Cotes"})
            Assert.That(trait.HorizontalContentAlignment, [Is].EqualTo(HorizontalAlignment.Left), "à gauche par défaut, comme mkm")

            trait.Align = DividerAlign.Center
            Assert.That(trait.HorizontalContentAlignment, [Is].EqualTo(HorizontalAlignment.Center))

            Dim vertical = Pret(New Divider With {.Layout = DividerLayout.Vertical})
            Assert.That(vertical.VerticalContentAlignment, [Is].EqualTo(VerticalAlignment.Center), "au milieu d'un trait vertical")
            Assert.That(Partie(Of Path)(vertical, "Trait").Width, [Is].EqualTo(1))
        End Sub

        <Test>
        Public Sub Divider_EstUnSeparateurPourLesLecteursDEcran()
            Dim trait = Pret(New Divider With {.Content = "Cotes"})
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(trait)

            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Separator))
            Assert.That(pair.GetName(), [Is].EqualTo("Cotes"))
            Assert.That(trait.Focusable, [Is].False)
        End Sub

        <Test>
        Public Sub Divider_SesTraits()
            Dim tirets = Pret(New Divider With {.[Type] = DividerType.Dashed})
            Assert.That(Partie(Of Path)(tirets, "Trait").StrokeDashArray.Count, [Is].GreaterThan(0))
            Dim plein = Pret(New Divider())
            Assert.That(Partie(Of Path)(plein, "Trait").StrokeDashArray.Count, [Is].EqualTo(0))
        End Sub

        ' ---- Skeleton ----

        <Test>
        Public Sub Skeleton_RectangleOuRond()
            Dim silhouette = Pret(New Skeleton With {.Width = 100})
            Assert.That(silhouette.Height, [Is].EqualTo(12), "la hauteur d'une ligne, par défaut")
            Assert.That(Partie(Of Border)(silhouette, "Forme").Visibility, [Is].EqualTo(Visibility.Visible))

            silhouette.Shape = SkeletonShape.Circle
            Assert.That(Partie(Of Border)(silhouette, "Forme").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of Ellipse)(silhouette, "Rond").Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub Skeleton_EstInvisibleAuxLecteursDEcran()
            Dim silhouette = Pret(New Skeleton With {.Width = 100})
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(silhouette), [Is].Null, "un dessin : son conteneur dit qu'il charge")
            Assert.That(silhouette.Focusable, [Is].False)
        End Sub

        ' ---- Les barres de défilement ----

        <Test>
        Public Sub BarresDeDefilement_CellesDeMkmPartout()
            Dim barre = Pret(New ScrollBar With {.Orientation = Orientation.Vertical})
            Assert.That(barre.Width, [Is].EqualTo(9), "9 px (scrollpanel.bar.size), et non celle de Windows")
            Assert.That(Partie(Of Track)(barre, "PART_Track"), [Is].Not.Null)

            Dim horizontale = Pret(New ScrollBar With {.Orientation = Orientation.Horizontal})
            Assert.That(horizontale.Height, [Is].EqualTo(9))
        End Sub

        <Test>
        Public Sub ScrollPanel_SesBarresParaissentAuSurvol()
            Dim zone = Pret(New ScrollPanel With {.Height = 100, .Content = New Border With {.Height = 400, .Width = 600}}, 300, 100)
            Dim verticale = Partie(Of ScrollBar)(zone, "PART_VerticalScrollBar")

            Assert.That(verticale.Opacity, [Is].EqualTo(0), "cachées hors du survol")
            Assert.That(zone.ScrollableHeight, [Is].GreaterThan(0))
            Assert.That(zone.ComputedHorizontalScrollBarVisibility, [Is].EqualTo(Visibility.Visible), "il défile aussi en largeur")
        End Sub

    End Class

End Namespace
