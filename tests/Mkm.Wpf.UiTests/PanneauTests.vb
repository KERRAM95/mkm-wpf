Imports System
Imports System.Linq
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.Input
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Les panneaux, sur leurs pages : onglets, groupes, carte, fiche, barre, séparateur, panneau d'accueil.</summary>
    <TestFixture>
    Public Class PanneauTests
        Inherits Scenario

        <Test>
        Public Sub Tabs_UnClicEtLesFlechesChangentDOnglet()
            Dim page = OuvrirLaDoc("tabs")
            Dim onglets = Trouver("Onglets", page)
            Dim cartouche = TrouverParNom("Cartouche", ControlType.TabItem, onglets)
            Dim notes = TrouverParNom("Notes", ControlType.TabItem, onglets)

            Cliquer(notes)
            AttendreQue(Function() notes.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Notes » choisi")
            TrouverParNom("Casser les angles vifs. Tolérances ISO 2768-mK.", dans:=onglets)
            Cliquer(cartouche)
            Taper(VirtualKeyShort.RIGHT)
            AttendreQue(Function() notes.Patterns.SelectionItem.Pattern.IsSelected.Value, "→ : « Notes »")
        End Sub

        <Test>
        Public Sub Fieldset_SaLegendeLeReplieEtLeDeplie()
            Dim page = OuvrirLaDoc("fieldset")
            Dim groupe = Trouver("Groupe", page)
            Dim cotes = TrouverParNom("Exporter les cotes", ControlType.CheckBox, groupe)
            Dim legende = TrouverParNom("Options d'export", ControlType.Button, groupe)

            Cliquer(legende)
            AttendreQue(Function() cotes.IsOffscreen, "replié : son contenu caché")
            Cliquer(legende)
            AttendreQue(Function() Not cotes.IsOffscreen, "déplié : son contenu revenu")
        End Sub

        <Test>
        Public Sub IdCard_SesBoutonsLaReplientEtLaFerment()
            Dim page = OuvrirLaDoc("idcard")
            Dim carte = Trouver("Carte", page)
            Dim replier = TrouverParNom(T("aria.collapse"), ControlType.Button, carte)

            Assert.That(NomDe(carte), [Is].EqualTo("Plan d'ensemble HS-00042"), "un groupe nommé par son titre")
            Cliquer(replier)
            AttendreQue(Function() NomDe(replier) = T("aria.expand"), "repliée : son bouton dit « Déplier »")
            Cliquer(replier)
            AttendreQue(Function() NomDe(replier) = T("aria.collapse"), "dépliée : « Replier »")
            Cliquer(TrouverParNom(T("aria.close"), ControlType.Button, carte))
            AttendreQue(Function() NomDe(Trouver("Journal", page)) = "Carte fermée : Plan d'ensemble HS-00042", "la croix ferme la carte")
        End Sub

        <Test>
        Public Sub PropertyPanel_ModifierPuisEnregistrer()
            Dim page = OuvrirLaDoc("propertypanel")
            Dim fiche = Trouver("Fiche", page)

            Cliquer(TrouverParNom(T("locale.edit"), ControlType.Button, fiche))
            Dim reference = Trouver("Reference", fiche)
            AttendreQue(Function() Not reference.IsOffscreen, "en modification : l'éditeur paraît")
            Cliquer(TrouverParNom(T("locale.save"), ControlType.Button, fiche))
            AttendreQue(Function() NomDe(Trouver("Journal", page)) = "Fiche enregistrée.", "« Fiche enregistrée. »")
        End Sub

        <Test>
        Public Sub Toolbar_UneCommandeSeLanceEtLeBoutonPlusOuvreLesAutres()
            Dim page = OuvrirLaDoc("toolbar")
            Dim basique = Exemple(page, "basique")
            Cliquer(TrouverParNom("Ajouter", ControlType.Button, Trouver("Barre", basique)))
            AttendreQue(Function() NomDe(Trouver("Journal", basique)) = "Commande lancée : Ajouter", "« Ajouter » lancée")
            Assert.That(TrouverParNom("Actualiser", ControlType.Button, basique).IsEnabled, [Is].True, "l'icône seule nommée par son info-bulle")

            Dim debordement = Exemple(page, "debordement")
            Dim plus = TrouverParNom(T("aria.moreCommands"), ControlType.Button, debordement)
            Assert.That(plus.IsOffscreen, [Is].False, "visible : les commandes ne tiennent pas dans 360 px")
            Cliquer(plus)
            Dim exporter = TrouverAilleurs("Exporter", ControlType.MenuItem)
            Assert.That(ChercherAilleurs("Supprimer", ControlType.MenuItem)?.IsEnabled, [Is].False, "grisée aussi dans le menu")
            Cliquer(exporter)
            AttendreQue(Function() NomDe(Trouver("Journal", debordement)) = "Commande lancée : Exporter", "« Exporter » lancée du menu")
            AttendreQue(Function() Referme("Exporter"), "le menu « … » refermé")
        End Sub

        <Test>
        Public Sub Splitter_LaGouttiereSeTireALaSourisEtAuClavier()
            Dim page = OuvrirLaDoc("splitter")
            Dim partage = Trouver("Partage", page)
            Dim gouttiere = TrouverParNom(T("aria.resizePanel"), dans:=partage)
            Dim journal = Trouver("Journal", page)
            Dim avant = gouttiere.BoundingRectangle.Left

            Montrer(gouttiere)
            Mouse.Drag(Centre(gouttiere), 80, 0)
            Wait.UntilInputIsProcessed()
            AttendreQue(Function() gouttiere.BoundingRectangle.Left > avant + 40, "la gouttière suit la souris")
            AttendreQue(Function() NomDe(journal).StartsWith("Tailles : ", StringComparison.Ordinal), "ResizeEnd donne les tailles")

            Dim tire = gouttiere.BoundingRectangle.Left
            gouttiere.Focus()
            Taper(VirtualKeyShort.LEFT, VirtualKeyShort.LEFT)
            AttendreQue(Function() gouttiere.BoundingRectangle.Left < tire, "← : la gouttière recule")
        End Sub

        <Test>
        Public Sub WelcomePanel_UneEntreeSeChoisitAuClicEtAuClavier()
            Dim page = OuvrirLaDoc("welcomepanel")
            Dim accueil = Trouver("Accueil", page)
            Dim tous = TrouverParNom("Tous les plans", ControlType.Button, accueil)
            Dim recents = TrouverParNom("Récents", ControlType.Button, accueil)

            Assert.That(recents.Patterns.SelectionItem.Pattern.IsSelected.Value, [Is].True, "« Récents », l'entrée de départ")
            Cliquer(tous)
            AttendreQue(Function() tous.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Tous les plans » choisie")
            Assert.That(recents.Patterns.SelectionItem.Pattern.IsSelected.Value, [Is].False)
            AttendreQue(Function() NomDe(Trouver("Journal", page)) = "Entrée : Tous les plans — choisie : tous", "son évènement")
        End Sub

        <Test>
        Public Sub Divider_UnSeparateurNommeParSonContenu()
            Dim page = OuvrirLaDoc("divider")
            Dim titre = Trouver("Titre", page)

            Assert.That(titre.ControlType, [Is].EqualTo(ControlType.Separator))
            Assert.That(NomDe(titre), [Is].EqualTo("Cotes"))
        End Sub

        <Test>
        Public Sub ScrollPanel_DefileALaMoletteSansLaPage()
            Dim page = OuvrirLaDoc("scrollpanel")
            Dim zone = Trouver("Zone", page)
            Montrer(zone)
            Dim defilement = zone.Patterns.Scroll.Pattern

            Assert.That(defilement.VerticallyScrollable.Value, [Is].True, "le texte dépasse la hauteur de la zone")
            Assert.That(defilement.VerticalScrollPercent.Value, [Is].EqualTo(0).Within(0.1))
            Survoler(zone)
            Mouse.Scroll(-2)
            Wait.UntilInputIsProcessed()
            AttendreQue(Function() defilement.VerticalScrollPercent.Value > 0, "la zone a défilé à la molette")
        End Sub

    End Class

End Namespace
