Imports System
Imports System.Linq
Imports System.Threading
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>La barre de commandes du bas de la page ActionBar : ses commandes, ses sections, sa liste, son repli.</summary>
    <TestFixture>
    Public Class ActionBarTests
        Inherits Scenario

        ' Une commande lancée s'inscrit en tête du journal de la page (« 10:42:07  Ajouter »).
        Private Shared Function AuJournal(page As AutomationElement, libelle As String) As Boolean
            Return page.FindAllDescendants().Any(Function(e) NomDe(e).EndsWith("  " & libelle, StringComparison.Ordinal))
        End Function

        ' Une commande que la barre ne montre pas (repliée, ou d'une autre section).
        Private Shared Function Masquee(barre As AutomationElement, libelle As String) As Boolean
            Dim commande = barre.FindFirstDescendant(Function(cf) cf.ByName(libelle).And(cf.ByControlType(ControlType.Button)))
            Return commande Is Nothing OrElse commande.IsOffscreen
        End Function

        <Test>
        Public Sub UneCommandeCliqueeSInscritAuJournal()
            Dim page = Ouvrir("ActionBar")
            Dim barre = Trouver("Barre")

            Cliquer(TrouverParNom(T("showcase.actionbar.add"), ControlType.Button, barre))
            AttendreQue(Function() AuJournal(page, T("showcase.actionbar.add")), "« Ajouter » au journal")

            Cliquer(TrouverParNom(T("showcase.actionbar.save"), ControlType.Button, barre))
            AttendreQue(Function() AuJournal(page, T("showcase.actionbar.save")), "« Enregistrer », une commande fixe, au journal")
        End Sub

        <Test>
        Public Sub UneCommandeGriseeNeSeLancePas()
            Dim page = Ouvrir("ActionBar")
            Dim supprimer = TrouverParNom(T("showcase.actionbar.delete"), ControlType.Button, Trouver("Barre"))

            Assert.That(supprimer.IsEnabled, [Is].False)
            Assert.That(supprimer.HelpText, [Is].EqualTo(T("showcase.actionbar.deleteHint")), "son indice, que lit le lecteur d'écran")
            Cliquer(supprimer)
            Thread.Sleep(300)
            Assert.That(AuJournal(page, T("showcase.actionbar.delete")), [Is].False)
        End Sub

        <Test>
        Public Sub UneBasculeSEnfonceEtSeRelache()
            Ouvrir("ActionBar")
            Dim verrou = TrouverParNom(T("showcase.actionbar.lock"), ControlType.Button, Trouver("Barre"))
            Dim bascule = verrou.Patterns.Toggle.Pattern

            Assert.That(bascule.ToggleState.Value, [Is].EqualTo(ToggleState.Off))
            Cliquer(verrou)
            AttendreQue(Function() bascule.ToggleState.Value = ToggleState.On, "« Verrouiller » enfoncée")
            Cliquer(verrou)
            AttendreQue(Function() bascule.ToggleState.Value = ToggleState.Off, "« Verrouiller » relâchée")
        End Sub

        <Test>
        Public Sub LesSectionsSontDesOngletsChoisis()
            Ouvrir("ActionBar")
            Dim barre = Trouver("Barre")
            Dim edition = TrouverParNom(T("showcase.actionbar.edit"), ControlType.TabItem, barre)
            Dim vue = TrouverParNom(T("showcase.actionbar.view"), ControlType.TabItem, barre)

            Assert.That(edition.Patterns.SelectionItem.Pattern.IsSelected.Value, [Is].True, "Édition, la section de départ")
            Cliquer(vue)
            AttendreQue(Function() vue.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Vue » choisie")
            Assert.That(edition.Patterns.SelectionItem.Pattern.IsSelected.Value, [Is].False)
            TrouverParNom(T("showcase.actionbar.refresh"), ControlType.Button, barre)
            AttendreQue(Function() Masquee(barre, T("showcase.actionbar.add")), "les commandes d'Édition parties")
        End Sub

        ' Le défaut d'une capture : revenue d'Outils (11 commandes) à Édition (5), la barre gardait la largeur d'Outils.
        <Test>
        Public Sub LaBarreReprendLaLargeurDeSaSectionCourte()
            Ouvrir("ActionBar")
            Dim barre = Trouver("Barre")
            Dim largeur = barre.BoundingRectangle.Width

            Cliquer(TrouverParNom(T("showcase.actionbar.tools"), ControlType.TabItem, barre))
            AttendreQue(Function() barre.BoundingRectangle.Width > largeur + 20, "la section Outils élargit la barre")
            Cliquer(TrouverParNom(T("showcase.actionbar.edit"), ControlType.TabItem, barre))
            AttendreQue(Function() Math.Abs(barre.BoundingRectangle.Width - largeur) <= 1,
                        $"la barre revient à sa largeur d'Édition ({largeur} px)")
        End Sub

        <Test>
        Public Sub LaListeLanceLaCommandeChoisieEtLaMontre()
            Dim page = Ouvrir("ActionBar")
            Dim barre = Trouver("Barre")

            Cliquer(TrouverParNom(T("actionBar.moreCommands"), ControlType.Button, barre))
            Cliquer(TrouverAilleurs(T("showcase.actionbar.zoomOut"), ControlType.Button))

            AttendreQue(Function() AuJournal(page, T("showcase.actionbar.zoomOut")), "« Zoom arrière » au journal")
            AttendreQue(Function() Referme(T("showcase.actionbar.zoomOut")), "la liste refermée")
            TrouverParNom(T("showcase.actionbar.zoomOut"), ControlType.Button, barre)
        End Sub

        <Test>
        Public Sub LeRepliMasqueLesCommandesPuisLesRend()
            Ouvrir("ActionBar")
            Dim barre = Trouver("Barre")

            Cliquer(TrouverParNom(T("actionBar.hideCommands"), ControlType.Button, barre))
            AttendreQue(Function() Masquee(barre, T("showcase.actionbar.add")), "les commandes masquées")
            Cliquer(TrouverParNom(T("actionBar.showCommands"), ControlType.Button, barre))
            AttendreQue(Function() Not Masquee(barre, T("showcase.actionbar.add")), "les commandes rendues")
        End Sub

        <Test>
        Public Sub DebutEtFinMenentAuxCommandesDesBouts()
            Ouvrir("ActionBar")
            Dim barre = Trouver("Barre")
            Dim ajouter = TrouverParNom(T("showcase.actionbar.add"), ControlType.Button, barre)
            Cliquer(ajouter)
            AttendreQue(Function() AFocus(ajouter), "« Ajouter » a le clavier")

            Taper(VirtualKeyShort.END)
            Dim derniere = TrouverParNom(T("showcase.actionbar.zoomIn"), ControlType.Button, barre)
            AttendreQue(Function() AFocus(derniere), "Fin : la commande de la liste, la dernière de la rangée")
            Taper(VirtualKeyShort.HOME)
            Dim premiere = TrouverParNom(T("showcase.actionbar.save"), ControlType.Button, barre)
            AttendreQue(Function() AFocus(premiere), "Début : « Enregistrer », la première commande fixe")
        End Sub

    End Class

End Namespace
