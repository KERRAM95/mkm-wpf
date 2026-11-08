Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 22 : ButtonGroup, Dock, SpeedDial.</summary>
    <TestFixture>
    Public Class Lot22Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 500, Optional hauteur As Double = 300) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        ' ---- ButtonGroup ----

        <Test>
        Public Sub ButtonGroup_LePremierEtLeDernierGardentLeursCoins()
            Dim groupe As New ButtonGroup()
            Dim boutons = {"Enregistrer", "Supprimer", "Annuler"}.Select(Function(t) New Global.Mkm.Wpf.Controls.Button With {.Content = t}).ToList()
            For Each b In boutons
                groupe.Items.Add(b)
            Next
            Pret(groupe)
            Assert.That(boutons.Select(Function(b) ButtonGroup.GetPosition(b)),
                        [Is].EqualTo({ButtonGroupPosition.First, ButtonGroupPosition.Middle, ButtonGroupPosition.Last}))
            Assert.That(boutons(0).CornerRadius.TopRight, [Is].EqualTo(0), "le premier : coins à gauche seulement")
            Assert.That(boutons(0).CornerRadius.TopLeft, [Is].GreaterThan(0))
            Assert.That(boutons(1).CornerRadius, [Is].EqualTo(New CornerRadius(0)))
            Assert.That(boutons(2).CornerRadius.TopLeft, [Is].EqualTo(0))
            Assert.That(boutons(0).BorderThickness.Right, [Is].EqualTo(0), "les boutons se touchent")
            Assert.That(boutons(2).BorderThickness.Right, [Is].EqualTo(1))

            groupe.Items.RemoveAt(2)
            groupe.Items.RemoveAt(1)
            Assert.That(ButtonGroup.GetPosition(boutons(0)), [Is].EqualTo(ButtonGroupPosition.None), "seul, il reprend sa forme")
        End Sub

        ' ---- Dock ----

        <Test>
        Public Sub Dock_SesApplicationsSontDesElementsDeMenuNommes()
            Dim dock As New Global.Mkm.Wpf.Controls.Dock()
            Dim plans As New DockItem With {.Label = "Plans", .Icon = Icons.Drawings}
            dock.Items.Add(plans)
            Pret(dock)
            Assert.That(plans.ToolTip, [Is].EqualTo("Plans"), "le nom en info-bulle")
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(dock).GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Menu))
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(plans)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.MenuItem))
            Assert.That(pair.GetName(), [Is].EqualTo("Plans"))
        End Sub

        <Test>
        Public Sub Dock_SePoseSurSonBord()
            Dim dock = Pret(New Global.Mkm.Wpf.Controls.Dock With {.Position = Global.Mkm.Wpf.Controls.DockPosition.Left})
            Assert.That(dock.HorizontalAlignment, [Is].EqualTo(HorizontalAlignment.Left))
            Dim bas = Pret(New Global.Mkm.Wpf.Controls.Dock())
            Assert.That(bas.VerticalAlignment, [Is].EqualTo(VerticalAlignment.Bottom))
        End Sub

        ' ---- SpeedDial ----

        Private Shared Function Actions(Optional type As SpeedDialType = SpeedDialType.Linear, Optional direction As SpeedDialDirection = SpeedDialDirection.Up) As SpeedDial
            Dim menu As New SpeedDial With {.Type = type, .Direction = direction, .Radius = 80}
            For Each nom In {"Copier", "Modifier", "Supprimer"}
                menu.Items.Add(New SpeedDialItem With {.Label = nom, .Icon = Icons.Copy})
            Next
            Return Pret(menu)
        End Function

        <Test>
        Public Sub SpeedDial_LeBoutonOuvreEtUneActionReferme()
            Dim menu = Actions()
            Dim bouton = DirectCast(menu.Template.FindName("PART_Button", menu), ButtonBase)
            bouton.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, bouton))
            Assert.That(menu.IsOpen, [Is].True)
            Dim copier = DirectCast(menu.Items(0), SpeedDialItem)
            copier.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, copier))
            Assert.That(menu.IsOpen, [Is].False, "une action choisie replie les actions")
        End Sub

        <Test>
        Public Sub SpeedDial_EnLigneLesActionsMontentAuDessusDuBouton()
            Dim menu = Actions()
            menu.IsOpen = True
            menu.UpdateLayout()
            Dim bouton = DirectCast(menu.Template.FindName("PART_Button", menu), FrameworkElement)
            Dim hauts = menu.Items.Cast(Of SpeedDialItem)().Select(Function(a) a.TranslatePoint(New Point(0, 0), bouton).Y).ToList()
            Assert.That(hauts.All(Function(y) y < 0), [Is].True, "au-dessus du bouton")
            Assert.That(hauts, [Is].Ordered.Descending, "la première au plus près")
        End Sub

        <Test>
        Public Sub SpeedDial_EnCercleLesActionsSontAuRayon()
            Dim menu = Actions(SpeedDialType.Circle)
            menu.IsOpen = True
            menu.UpdateLayout()
            Dim bouton = DirectCast(menu.Template.FindName("PART_Button", menu), FrameworkElement)
            Dim centre As New Point(bouton.ActualWidth / 2, bouton.ActualHeight / 2)
            For Each action In menu.Items.Cast(Of SpeedDialItem)()
                Dim milieu = action.TranslatePoint(New Point(action.ActualWidth / 2, action.ActualHeight / 2), bouton)
                Assert.That((milieu - centre).Length, [Is].EqualTo(80).Within(0.5), action.Label)
            Next
        End Sub

        <Test>
        Public Sub SpeedDial_LeBoutonSeDitDeveloppeOuReduit()
            Dim menu = Actions()
            Dim bouton = DirectCast(menu.Template.FindName("PART_Button", menu), UIElement)
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(bouton)
            Assert.That(pair.GetName(), [Is].Not.Empty)
            Dim depli = DirectCast(pair.GetPattern(PatternInterface.ExpandCollapse), IExpandCollapseProvider)
            depli.Expand()
            Assert.That(menu.IsOpen, [Is].True)
            Assert.That(depli.ExpandCollapseState, [Is].EqualTo(ExpandCollapseState.Expanded))
        End Sub

    End Class

End Namespace
