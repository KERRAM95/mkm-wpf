Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Localization
Imports NUnit.Framework
Imports BasculeDeMkm = Mkm.Wpf.Controls.ToggleButton
Imports BoutonDeMkm = Mkm.Wpf.Controls.Button
Imports MenuDeMkm = Mkm.Wpf.Controls.Menu
Imports BarreDeMkm = Mkm.Wpf.Controls.Toolbar

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>
    ''' Ce que voient les lecteurs d'écran (et les tests d'interaction, qui passent par la même automatisation) : les
    ''' commandes des gabarits, et un nom pour chaque bouton.
    ''' </summary>
    <TestFixture>
    Public Class AccessibiliteTests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 800) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, 400))
            element.Arrange(New Rect(0, 0, largeur, 400))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function PairDe(element As UIElement) As AutomationPeer
            Return UIElementAutomationPeer.CreatePeerForElement(element)
        End Function

        Private Shared Iterator Function Descendants(pair As AutomationPeer) As IEnumerable(Of AutomationPeer)
            Dim enfants = pair.GetChildren()
            If enfants Is Nothing Then Return
            For Each enfant In enfants
                Yield enfant
                For Each petit In Descendants(enfant)
                    Yield petit
                Next
            Next
        End Function

        Private Shared Function NomsDesBoutons(element As UIElement) As List(Of String)
            Return Descendants(PairDe(element)).
                Where(Function(p) p.GetAutomationControlType() = AutomationControlType.Button).
                Select(Function(p) p.GetName()).ToList()
        End Function

        <Test>
        Public Sub ActionBar_SesCommandesSontVisiblesDesLecteursDEcran()
            Dim barre As New ActionBar With {.ActiveSection = "a"}
            Dim section As New ActionBarSection With {.Key = "a", .Header = "Édition"}
            section.Items.Add(New ActionBarButton With {.Label = "Ajouter", .Icon = Icons.Add})
            barre.Items.Add(section)
            barre.FixedItems.Add(New ActionBarButton With {.Label = "Enregistrer", .Icon = Icons.Download})
            Pret(barre)

            Assert.That(PairDe(barre).GetAutomationControlType(), [Is].EqualTo(AutomationControlType.ToolBar))
            Assert.That(NomsDesBoutons(barre), Does.Contain("Ajouter").And.Contain("Enregistrer"),
                        "le pair d'un Selector ne montrait que ses sections, pas les boutons de son gabarit")
        End Sub

        ' Le défaut trouvé par les tests d'interaction : repliée dès le départ, son bouton de repli n'avait pas de nom.
        <Test>
        Public Sub ActionBar_SonBoutonDeRepliSuitSonEtat()
            Dim barre As New ActionBar With {.ActiveSection = "a", .IsCollapsed = True}
            Dim section As New ActionBarSection With {.Key = "a", .Header = "Édition"}
            section.Items.Add(New ActionBarButton With {.Label = "Ajouter", .Icon = Icons.Add})
            barre.Items.Add(section)
            Pret(barre)

            Assert.That(NomsDesBoutons(barre), Does.Contain(Localizer.Current.T("actionBar.showCommands")))
            barre.IsCollapsed = False
            barre.UpdateLayout()
            Assert.That(NomsDesBoutons(barre), Does.Contain(Localizer.Current.T("actionBar.hideCommands")))
        End Sub

        <Test>
        Public Sub Toolbar_SesCommandesEtSonBoutonPlusSontVisibles()
            Dim barre As New BarreDeMkm With {.Overflow = True}
            For Each libelle In {"Ajouter", "Copier", "Importer", "Exporter"}
                barre.Items.Add(New ToolbarButton With {.Content = libelle, .Icon = Icons.Add})
            Next
            Pret(barre, 120)

            Dim noms = NomsDesBoutons(barre)
            Assert.That(PairDe(barre).GetAutomationControlType(), [Is].EqualTo(AutomationControlType.ToolBar))
            Assert.That(noms, Does.Contain("Ajouter"))
            Assert.That(noms, Does.Contain(Localizer.Current.T("aria.moreCommands")), "le bouton « … » du gabarit")
        End Sub

        <Test>
        Public Sub ConteneursDeCommandes_ExposentLeurGabarit()
            Dim vide As New EmptyContent With {.Title = "Aucun plan"}
            vide.Actions = New BoutonDeMkm With {.Content = "Ouvrir"}
            Pret(vide)
            Assert.That(NomsDesBoutons(vide), Does.Contain("Ouvrir"), "les actions d'un écran vide")

            Dim menu As New MenuDeMkm()
            menu.Items.Add(New MenuItem With {.Header = "Ouvrir"})
            Pret(menu)
            Assert.That(PairDe(menu).GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Menu))
            Assert.That(Descendants(PairDe(menu)).Any(Function(p) p.GetName() = "Ouvrir"), [Is].True)
        End Sub

        <Test>
        Public Sub BoutonDIconeSeule_SonInfoBulleLeNomme()
            Dim bouton As New BoutonDeMkm With {.Icon = Icons.Refresh, .ToolTip = "Actualiser"}
            Pret(bouton)
            Assert.That(PairDe(bouton).GetName(), [Is].EqualTo("Actualiser"))

            Dim nomme As New BoutonDeMkm With {.Content = "Enregistrer", .ToolTip = "Ctrl+S"}
            Pret(nomme)
            Assert.That(PairDe(nomme).GetName(), [Is].EqualTo("Enregistrer"), "le libellé passe avant l'info-bulle")
        End Sub

        <Test>
        Public Sub ToggleButton_SonLibelleDEtatLeNomme()
            Dim bascule As New BasculeDeMkm With {.OnLabel = "Verrouillé", .OffLabel = "Déverrouillé"}
            Pret(bascule)
            Dim pair = PairDe(bascule)
            Assert.That(pair.GetName(), [Is].EqualTo("Déverrouillé"))

            bascule.IsChecked = True
            Assert.That(pair.GetName(), [Is].EqualTo("Verrouillé"))

            Dim epingle As New BasculeDeMkm With {.OnIcon = Icons.Pin, .ToolTip = "Épingler"}
            Pret(epingle)
            Assert.That(PairDe(epingle).GetName(), [Is].EqualTo("Épingler"), "icône seule : son info-bulle")
        End Sub

        <Test>
        Public Sub ControlesSansPairDeWpf_OntLeLeur()
            Dim message As New Message With {.Content = "Le plan est ouvert."}
            Pret(message)
            Assert.That(PairDe(message), [Is].Not.Null, "sans pair, l'annonce (LiveSetting) du message ne partait pas")
            Assert.That(PairDe(message).GetLiveSetting(), [Is].EqualTo(System.Windows.Automation.AutomationLiveSetting.Assertive))

            Dim attente As New ProgressSpinner()
            Pret(attente)
            Assert.That(PairDe(attente).GetAutomationControlType(), [Is].EqualTo(AutomationControlType.ProgressBar))
            Assert.That(PairDe(attente).GetName(), [Is].EqualTo(Localizer.Current.T("aria.busy")))

            Dim carte As New IdCard With {.Title = "Plan HS-00042"}
            Pret(carte)
            Assert.That(PairDe(carte).GetName(), [Is].EqualTo("Plan HS-00042"))
        End Sub

        ' mkm dit la légende repliable « déplié » (aria-expanded) ; elle se disait « activée » quand le groupe était replié.
        <Test>
        Public Sub Fieldset_SaLegendeDitDeplieOuReplie()
            Dim groupe As New Fieldset With {.Header = "Options", .Toggleable = True, .Content = New TextBlock With {.Text = "Contenu"}}
            Pret(groupe)
            Dim legende = Descendants(PairDe(groupe)).First(Function(p) p.GetName() = "Options" AndAlso p.GetAutomationControlType() = AutomationControlType.Button)
            Dim depli = DirectCast(legende.GetPattern(PatternInterface.ExpandCollapse), System.Windows.Automation.Provider.IExpandCollapseProvider)

            Assert.That(legende.GetPattern(PatternInterface.Toggle), [Is].Null, "plus une bascule")
            Assert.That(depli.ExpandCollapseState, [Is].EqualTo(System.Windows.Automation.ExpandCollapseState.Expanded))
            depli.Collapse()
            Assert.That(groupe.IsCollapsed, [Is].True)
            Assert.That(depli.ExpandCollapseState, [Is].EqualTo(System.Windows.Automation.ExpandCollapseState.Collapsed))
        End Sub

        <Test>
        Public Sub InputNumber_EstUneToupieQuiDitSaValeur()
            Dim nombre As New InputNumber With {.Value = 3, .Minimum = 1, .Maximum = 10}
            Pret(nombre)
            Dim pair = PairDe(nombre)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Spinner))
            Dim plage = DirectCast(pair.GetPattern(PatternInterface.RangeValue), System.Windows.Automation.Provider.IRangeValueProvider)

            Assert.That(plage.Value, [Is].EqualTo(3))
            Assert.That(plage.Maximum, [Is].EqualTo(10))
            plage.SetValue(42)
            Assert.That(nombre.Value, [Is].EqualTo(10), "ramenée à la borne")
        End Sub

        <Test>
        Public Sub ChampSansLibelle_SonTexteIndicatifLeNomme()
            Dim champ As New InputText With {.Placeholder = "ex. HS-1234"}
            Pret(champ)
            Assert.That(PairDe(champ).GetName(), [Is].EqualTo("ex. HS-1234"))

            System.Windows.Automation.AutomationProperties.SetName(champ, "Référence")
            Assert.That(PairDe(champ).GetName(), [Is].EqualTo("Référence"), "son nom passe avant")
        End Sub

    End Class

End Namespace
