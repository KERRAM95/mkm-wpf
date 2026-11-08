Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 24 : MeterGroup, OrganizationChart.</summary>
    <TestFixture>
    Public Class Lot24Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 800, Optional hauteur As Double = 400) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        ' ---- MeterGroup ----

        <Test>
        Public Sub MeterGroup_ChaqueJaugeSaPartEtSaLegende()
            Dim groupe As New MeterGroup()
            Dim plans As New MeterItem With {.Label = "Plans", .Value = 16, .Color = Brushes.Green}
            groupe.Items.Add(plans)
            groupe.Items.Add(New MeterItem With {.Label = "Pièces", .Value = 24, .Color = Brushes.Blue})
            Pret(groupe, 400, 60)
            Assert.That(plans.Percent, [Is].EqualTo(16))
            Assert.That(plans.Legend, [Does].StartWith("Plans (16"))
            Dim barre = DirectCast(groupe.Template.FindName("PART_Meters", groupe), System.Windows.Controls.Panel)
            Assert.That(barre.Children.Count, [Is].EqualTo(2))
            Dim premier = DirectCast(barre.Children(0), FrameworkElement)
            Assert.That(premier.ActualWidth, [Is].EqualTo(400 * 0.16).Within(1), "la part de la longueur")

            plans.Value = 50
            Assert.That(plans.Percent, [Is].EqualTo(50), "la part suit la valeur")
            groupe.Maximum = 200
            Assert.That(plans.Percent, [Is].EqualTo(25), "et l'étendue")
        End Sub

        ' ---- OrganizationChart ----

        Private Shared Function UnOrganigramme(Optional selectable As Boolean = False) As OrganizationChart
            Dim racine As New OrganizationChartNode With {.Header = "Direction"}
            racine.Items.Add(New OrganizationChartNode With {.Header = "BE"})
            racine.Items.Add(New OrganizationChartNode With {.Header = "Production"})
            Dim organigramme As New OrganizationChart With {.Collapsible = True, .Selectable = selectable}
            organigramme.Items.Add(racine)
            Return Pret(organigramme)
        End Function

        <Test>
        Public Sub OrganizationChart_UnChoixPoseAvantLesNoeudsLesMarque()
            Dim racine As New OrganizationChartNode With {.Header = "Direction"}
            Dim production As New OrganizationChartNode With {.Header = "Production"}
            racine.Items.Add(production)
            Dim organigramme As New OrganizationChart With {.Selectable = True, .SelectedItem = production}
            organigramme.Items.Add(racine)
            Pret(organigramme)
            Assert.That(production.IsSelected, [Is].True, "marqué à sa création")
            Assert.That(racine.IsSelected, [Is].False)
        End Sub

        <Test>
        Public Sub OrganizationChart_LesEnfantsSousLeurParent()
            Dim organigramme = UnOrganigramme()
            Dim racine = DirectCast(organigramme.Items(0), OrganizationChartNode)
            Dim be = DirectCast(racine.Items(0), OrganizationChartNode)
            Dim production = DirectCast(racine.Items(1), OrganizationChartNode)
            Dim yRacine = racine.TranslatePoint(New Point(0, 0), organigramme).Y
            Dim yBe = be.TranslatePoint(New Point(0, 0), organigramme).Y
            Assert.That(yBe, [Is].GreaterThan(yRacine + 24), "sous le parent, après les traits")
            Assert.That(production.TranslatePoint(New Point(0, 0), organigramme).X, [Is].GreaterThan(be.TranslatePoint(New Point(0, 0), organigramme).X))
            Assert.That(racine.Collapsible, [Is].True, "repris de l'organigramme")
        End Sub

        <Test>
        Public Sub OrganizationChart_LaBasculeReplieLesEnfants()
            Dim organigramme = UnOrganigramme()
            Dim racine = DirectCast(organigramme.Items(0), OrganizationChartNode)
            Dim bascule = DirectCast(racine.Template.FindName("PART_Toggle", racine), ButtonBase)
            Assert.That(bascule.Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(AutomationProperties.GetName(bascule), [Is].Not.Empty)
            bascule.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, bascule))
            Assert.That(racine.IsExpanded, [Is].False)
            racine.UpdateLayout()
            Assert.That(DirectCast(racine.Template.FindName("Enfants", racine), UIElement).Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub OrganizationChart_UnClicChoisitUnNoeud()
            Dim organigramme = UnOrganigramme(selectable:=True)
            Dim racine = DirectCast(organigramme.Items(0), OrganizationChartNode)
            Dim be = DirectCast(racine.Items(0), OrganizationChartNode)
            be.Choisir()
            Assert.That(organigramme.SelectedItem, [Is].SameAs(be))
            Assert.That(be.IsSelected, [Is].True)
            be.Choisir()
            Assert.That(organigramme.SelectedItem, [Is].Null, "un second clic le retire")
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(be)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.TreeItem))
            Assert.That(pair.GetName(), [Is].EqualTo("BE"))
            DirectCast(pair.GetPattern(PatternInterface.SelectionItem), ISelectionItemProvider).Select()
            Assert.That(be.IsSelected, [Is].True)
        End Sub

    End Class

End Namespace
