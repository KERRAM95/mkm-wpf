Imports System
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.Input
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Les menus et les barres de commandes : ActionBar, registre des commandes, Menu, TieredMenu, menu contextuel.</summary>
    <TestFixture>
    Public Class MenuTests
        Inherits Scenario

        ' Une commande de barre que la barre ne montre pas (repliée, ou d'une autre section).
        Private Shared Function Masquee(barre As AutomationElement, libelle As String) As Boolean
            Dim commande = barre.FindFirstDescendant(Function(cf) cf.ByName(libelle).And(cf.ByControlType(ControlType.Button)))
            Return commande Is Nothing OrElse commande.IsOffscreen
        End Function

        <Test>
        Public Sub ActionBar_UneCommandeCliqueeSeLance()
            Dim page = OuvrirLaDoc("actionbar")
            Dim cadre = Exemple(page, "basique")

            Cliquer(TrouverParNom("Ajouter", ControlType.Button, Trouver("Barre", cadre)))
            AttendreQue(Function() NomDe(Trouver("Journal", cadre)) = "Commande lancée : Ajouter", "« Ajouter » lancée")
        End Sub

        <Test>
        Public Sub ActionBar_LesSectionsSontDesOngletsEtLaBarreReprendSaLargeur()
            Dim page = OuvrirLaDoc("actionbar")
            Dim barre = Trouver("Barre", Exemple(page, "sections"))
            Dim edition = TrouverParNom("Édition", ControlType.TabItem, barre)
            Dim outils = TrouverParNom("Outils", ControlType.TabItem, barre)
            Dim largeur = barre.BoundingRectangle.Width

            Assert.That(edition.Patterns.SelectionItem.Pattern.IsSelected.Value, [Is].True, "Édition, la section de départ")
            Cliquer(outils)
            AttendreQue(Function() outils.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Outils » choisie")
            AttendreQue(Function() barre.BoundingRectangle.Width > largeur + 20, "la section Outils élargit la barre")
            AttendreQue(Function() Masquee(barre, "Ajouter"), "les commandes d'Édition parties")
            ' Le défaut d'une capture : revenue d'Outils à Édition, la barre gardait la largeur d'Outils.
            Cliquer(edition)
            AttendreQue(Function() Math.Abs(barre.BoundingRectangle.Width - largeur) <= 1, $"la barre revient à sa largeur d'Édition ({largeur} px)")
        End Sub

        <Test>
        Public Sub ActionBar_DebutEtFinMenentAuxCommandesDesBouts()
            Dim page = OuvrirLaDoc("actionbar")
            Dim barre = Trouver("Barre", Exemple(page, "commandes-fixes"))
            Dim ajouter = TrouverParNom("Ajouter", ControlType.Button, barre)
            Cliquer(ajouter)
            AttendreQue(Function() AFocus(ajouter), "« Ajouter » a le clavier")

            Taper(VirtualKeyShort.END)
            AttendreQue(Function() AFocus(TrouverParNom("Copier", ControlType.Button, barre)), "Fin : « Copier », la dernière")
            Taper(VirtualKeyShort.HOME)
            AttendreQue(Function() AFocus(TrouverParNom("Enregistrer", ControlType.Button, barre)), "Début : « Enregistrer », la commande fixe")
        End Sub

        <Test>
        Public Sub ActionBar_LaBasculeEtLaListe()
            Dim page = OuvrirLaDoc("actionbar")
            Dim cadre = Exemple(page, "bascule-liste-et-separateur")
            Dim barre = Trouver("Barre", cadre)
            Dim verrou = TrouverParNom("Verrouiller", ControlType.Button, barre)

            Cliquer(verrou)
            AttendreQue(Function() verrou.Patterns.Toggle.Pattern.ToggleState.Value = ToggleState.On, "« Verrouiller » enfoncée")
            Cliquer(TrouverParNom(T("actionBar.moreCommands"), ControlType.Button, barre))
            Cliquer(TrouverAilleurs("Zoom arrière", ControlType.Button))
            AttendreQue(Function() NomDe(Trouver("Journal", cadre)) = "Commande lancée : Zoom arrière", "« Zoom arrière » lancée de la liste")
            AttendreQue(Function() Referme("Zoom arrière"), "la liste refermée")
            TrouverParNom("Zoom arrière", ControlType.Button, barre)
        End Sub

        <Test>
        Public Sub ActionBar_UneCommandeGriseeDitSonIndice()
            Dim page = OuvrirLaDoc("actionbar")
            Dim barre = Exemple(page, "grise-et-indice")
            Dim supprimer = TrouverParNom("Supprimer", ControlType.Button, barre)

            Assert.That(supprimer.IsEnabled, [Is].False)
            Assert.That(supprimer.HelpText, [Is].Not.Empty, "son indice, que lit le lecteur d'écran")
        End Sub

        <Test>
        Public Sub ActionBar_LeRepliMasqueLesCommandesPuisLesRend()
            Dim page = OuvrirLaDoc("actionbar")
            Dim barre = Trouver("Barre", Exemple(page, "basique"))

            Cliquer(TrouverParNom(T("actionBar.hideCommands"), ControlType.Button, barre))
            AttendreQue(Function() Masquee(barre, "Ajouter"), "les commandes masquées")
            Cliquer(TrouverParNom(T("actionBar.showCommands"), ControlType.Button, barre))
            AttendreQue(Function() Not Masquee(barre, "Ajouter"), "les commandes rendues")
        End Sub

        <Test>
        Public Sub Registre_LaBarreLeMenuEtLeRaccourciLancentLesCommandes()
            Dim page = OuvrirLaDoc("commands")
            Dim cadre = Exemple(page, "menu-et-barre")
            Dim barre = Trouver("BarreDuRegistre", cadre)
            Dim journal = Trouver("Journal", cadre)

            Cliquer(TrouverParNom("Ajouter", dans:=barre))
            AttendreQue(Function() NomDe(journal) = "ajouter", "« ajouter » lancée par la barre")
            Raccourci(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_L)
            AttendreQue(Function() NomDe(journal) = "verrouiller : True", "Ctrl+L : « verrouiller », cochée")
            TrouverParNom("Verrouillé", dans:=barre)

            Dim menu = Trouver("MenuDuRegistre", cadre)
            Cliquer(TrouverParNom("Copier", ControlType.MenuItem, menu))
            AttendreQue(Function() NomDe(journal) = "copier", "« copier » lancée par le menu")
            Assert.That(TrouverParNom("Supprimer", ControlType.MenuItem, menu).IsEnabled, [Is].False)
        End Sub

        <Test>
        Public Sub Menu_UnElementCliqueSeLanceSansVolerLeClavier()
            Dim page = OuvrirLaDoc("menu")
            Dim cadre = Exemple(page, "basique")
            Dim menu = Trouver("MenuPose", cadre)

            ' Le défaut du lot 7 : survoler un menu posé dans la page lui donnait le clavier.
            Dim recherche = Trouver("Recherche")
            Cliquer(recherche)
            Survoler(TrouverParNom("Exporter", ControlType.MenuItem, menu))
            Threading.Thread.Sleep(300)
            Assert.That(AFocus(recherche), [Is].True, "le survol laisse le clavier où il est")

            Cliquer(TrouverParNom("Exporter", ControlType.MenuItem, menu))
            AttendreQue(Function() NomDe(Trouver("Journal", cadre)) = "Choisi : Exporter", "« Exporter » choisi")
        End Sub

        <Test>
        Public Sub TieredMenu_LeSurvolOuvreLeSousMenu()
            Dim page = OuvrirLaDoc("tieredmenu")
            Dim cadre = Exemple(page, "basique")
            Dim menu = Trouver("MenuAEtages", cadre)

            Survoler(TrouverParNom("Exporter", ControlType.MenuItem, menu))
            Cliquer(TrouverAilleurs("PDF", ControlType.MenuItem))
            AttendreQue(Function() NomDe(Trouver("Journal", cadre)) = "Choisi : PDF", "« PDF » choisi dans le sous-menu")
            AttendreQue(Function() Referme("PDF"), "le sous-menu refermé")
        End Sub

        <Test>
        Public Sub ContextMenu_LeClicDroitOuvreLeMenu()
            Dim page = OuvrirLaDoc("contextmenu")
            Dim cadre = Exemple(page, "basique")

            Cliquer(Trouver("Zone", cadre), MouseButton.Right)
            Cliquer(TrouverAilleurs("Renommer", ControlType.MenuItem))
            AttendreQue(Function() NomDe(Trouver("Journal", cadre)) = "Choisi : Renommer", "« Renommer » choisi")
            AttendreQue(Function() Referme("Renommer"), "le menu refermé")
        End Sub

    End Class

End Namespace
