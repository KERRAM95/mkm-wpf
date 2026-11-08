Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 12 : Panel, Card, Accordion.</summary>
    <TestFixture>
    Public Class Lot12Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 400, Optional hauteur As Double = 300) As T
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

        Private Shared Function Texte(cle As String) As String
            Return Mkm.Wpf.Localization.Localizer.Current.T(cle)
        End Function

        ' ---- Panel ----

        <Test>
        Public Sub Panel_SonBoutonLeReplieEtSeNommeSelonLEtat()
            Dim panneau = Pret(New Mkm.Wpf.Controls.Panel With {.Header = "Notes", .Toggleable = True, .Content = New TextBlock With {.Text = "Casser les angles vifs."}})
            Dim bouton = Partie(Of ButtonBase)(panneau, "PART_Toggle")
            Dim contenu = Partie(Of ContentPresenter)(panneau, "Contenu")

            Assert.That(bouton.Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(System.Windows.Automation.AutomationProperties.GetName(bouton), [Is].EqualTo(Texte("aria.collapse")))
            bouton.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))
            Assert.That(panneau.IsCollapsed, [Is].True)
            Assert.That(contenu.Visibility, [Is].EqualTo(Visibility.Collapsed), "replié : seul l'en-tête reste")
            Assert.That(System.Windows.Automation.AutomationProperties.GetName(bouton), [Is].EqualTo(Texte("aria.expand")))

            ' Pour les lecteurs d'écran, un bouton qui déplie (aria-expanded de mkm).
            Dim depli = DirectCast(UIElementAutomationPeer.CreatePeerForElement(bouton).GetPattern(PatternInterface.ExpandCollapse), IExpandCollapseProvider)
            Assert.That(depli.ExpandCollapseState, [Is].EqualTo(System.Windows.Automation.ExpandCollapseState.Collapsed))
            depli.Expand()
            Assert.That(panneau.IsCollapsed, [Is].False, "Expand le déplie")
        End Sub

        <Test>
        Public Sub Panel_SansToggleablePasDeBoutonEtUnGroupeNomme()
            Dim panneau = Pret(New Mkm.Wpf.Controls.Panel With {.Header = "Cartouche", .Content = "HS-00042"})
            Assert.That(Partie(Of ButtonBase)(panneau, "PART_Toggle").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of ContentPresenter)(panneau, "Pied").Visibility, [Is].EqualTo(Visibility.Collapsed), "sans Footer, pas de pied")

            Dim pair = UIElementAutomationPeer.CreatePeerForElement(panneau)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Group))
            Assert.That(pair.GetName(), [Is].EqualTo("Cartouche"))
        End Sub

        ' ---- Card ----

        <Test>
        Public Sub Card_SesPartiesVidesDisparaissent()
            Dim carte = Pret(New Card With {.Title = "Plan d'ensemble", .Content = "Vue de face"})
            Assert.That(Partie(Of ContentPresenter)(carte, "Titre").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of ContentPresenter)(carte, "SousTitre").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of ContentPresenter)(carte, "Haut").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of ContentPresenter)(carte, "Pied").Visibility, [Is].EqualTo(Visibility.Collapsed))

            Dim pair = UIElementAutomationPeer.CreatePeerForElement(carte)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Group))
            Assert.That(pair.GetName(), [Is].EqualTo("Plan d'ensemble"))
        End Sub

        ' ---- Accordion ----

        Private Shared Function NouvelAccordeon(Optional plusieurs As Boolean = False) As Accordion
            Dim accordeon As New Accordion With {.Multiple = plusieurs}
            accordeon.Items.Add(New AccordionPanel With {.Header = "Cartouche", .IsExpanded = True, .Content = "HS-00042"})
            accordeon.Items.Add(New AccordionPanel With {.Header = "Notes", .Content = "Casser les angles vifs."})
            accordeon.Items.Add(New AccordionPanel With {.Header = "Historique", .Content = "Révision B"})
            Return Pret(accordeon)
        End Function

        Private Shared Function LaSection(accordeon As Accordion, i As Integer) As AccordionPanel
            Return DirectCast(accordeon.Items(i), AccordionPanel)
        End Function

        <Test>
        Public Sub Accordion_UneSeuleSectionOuverte()
            Dim accordeon = NouvelAccordeon()
            LaSection(accordeon, 1).IsExpanded = True
            Assert.That(LaSection(accordeon, 0).IsExpanded, [Is].False, "« Notes » ouverte referme « Cartouche »")
            Assert.That(Partie(Of Border)(LaSection(accordeon, 1), "Contenu").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of Border)(LaSection(accordeon, 0), "Contenu").Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub Accordion_MultipleLaisseOuvertes()
            Dim accordeon = NouvelAccordeon(plusieurs:=True)
            LaSection(accordeon, 1).IsExpanded = True
            Assert.That(LaSection(accordeon, 0).IsExpanded AndAlso LaSection(accordeon, 1).IsExpanded, [Is].True)
        End Sub

        <Test>
        Public Sub Accordion_LaPremiereEtLaDerniereSection()
            Dim accordeon = NouvelAccordeon()
            Assert.That(LaSection(accordeon, 0).IsFirst, [Is].True)
            Assert.That(LaSection(accordeon, 1).IsFirst OrElse LaSection(accordeon, 1).IsLast, [Is].False)
            Assert.That(LaSection(accordeon, 2).IsLast, [Is].True)

            Dim premier = Partie(Of Border)(LaSection(accordeon, 0), "EnTete")
            Dim milieu = Partie(Of Border)(LaSection(accordeon, 1), "EnTete")
            Assert.That(premier.BorderThickness.Top, [Is].EqualTo(1), "la première bordée tout autour")
            Assert.That(milieu.BorderThickness.Top, [Is].EqualTo(0), "les autres sous la précédente")
            Assert.That(premier.CornerRadius.TopLeft, [Is].GreaterThan(0))
            Assert.That(premier.CornerRadius.BottomLeft, [Is].EqualTo(0))
        End Sub

        <Test>
        Public Sub Accordion_LEnTeteSeDitDeveloppeOuReduit()
            Dim accordeon = NouvelAccordeon()
            Dim entete = Partie(Of Primitives.ToggleButton)(LaSection(accordeon, 1), "PART_Header")
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(entete)
            Dim depli = DirectCast(pair.GetPattern(PatternInterface.ExpandCollapse), IExpandCollapseProvider)

            Assert.That(pair.GetName(), [Is].EqualTo("Notes"))
            Assert.That(depli.ExpandCollapseState, [Is].EqualTo(System.Windows.Automation.ExpandCollapseState.Collapsed))
            depli.Expand()
            Assert.That(LaSection(accordeon, 1).IsExpanded, [Is].True)
            Assert.That(LaSection(accordeon, 0).IsExpanded, [Is].False)

            Dim section = UIElementAutomationPeer.CreatePeerForElement(LaSection(accordeon, 1))
            Assert.That(section.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Group))
            Assert.That(section.GetName(), [Is].EqualTo("Notes"))
        End Sub

        <Test>
        Public Sub Accordion_DeuxOuvertesAuDepartUneSeuleReste()
            Dim accordeon As New Accordion()
            accordeon.Items.Add(New AccordionPanel With {.Header = "A", .IsExpanded = True})
            accordeon.Items.Add(New AccordionPanel With {.Header = "B", .IsExpanded = True})
            Pret(accordeon)
            Assert.That(LaSection(accordeon, 0).IsExpanded, [Is].True)
            Assert.That(LaSection(accordeon, 1).IsExpanded, [Is].False)
        End Sub

    End Class

End Namespace
