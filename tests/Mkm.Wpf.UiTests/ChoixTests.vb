Imports System
Imports System.Linq
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>La page des choix : RadioButton, ToggleButton, Toolbar et son menu « … », le registre des commandes.</summary>
    <TestFixture>
    Public Class ChoixTests
        Inherits Scenario

        <Test>
        Public Sub RadioButton_UnClicChoisitEtDechoisitLesAutres()
            Ouvrir("Choix")
            Dim plan = Trouver("RadioPlan").AsRadioButton()
            Dim article = Trouver("RadioArticle").AsRadioButton()

            Assert.That(plan.IsChecked, [Is].True)
            Cliquer(article)
            AttendreQue(Function() article.IsChecked, "« Article » choisi")
            Assert.That(plan.IsChecked, [Is].False, "un seul choix par groupe")
        End Sub

        <Test>
        Public Sub RadioButton_LesFlechesChoisissentLeVoisin()
            Ouvrir("Choix")
            Dim plan = Trouver("RadioPlan").AsRadioButton()
            Dim article = Trouver("RadioArticle").AsRadioButton()
            Cliquer(plan)
            AttendreQue(Function() AFocus(plan), "« Plan » a le clavier")

            Taper(VirtualKeyShort.RIGHT)
            AttendreQue(Function() AFocus(article) AndAlso article.IsChecked, "→ : « Article » choisi")
        End Sub

        <Test>
        Public Sub ToggleButton_UnClicLeBasculeEtChangeSonLibelle()
            Ouvrir("Choix")
            Dim verrou = Trouver("Verrou")
            Dim bascule = verrou.Patterns.Toggle.Pattern

            Assert.That(NomDe(verrou), [Is].EqualTo(T("showcase.choices.unlocked")), "son libellé d'état le nomme")
            Cliquer(verrou)
            AttendreQue(Function() bascule.ToggleState.Value = ToggleState.On, "enfoncé")
            AttendreQue(Function() NomDe(verrou) = T("showcase.choices.locked"), "« Verrouillé », son libellé enfoncé")
        End Sub

        ' Le défaut du lot 5 : le contour du focus d'un ToggleButton plantait la vitrine au premier Tab.
        <Test>
        Public Sub ToggleButton_LeClavierLeParcourtEtLeBascule()
            Ouvrir("Choix")
            Dim verrou = Trouver("Verrou")
            Cliquer(verrou)
            AttendreQue(Function() AFocus(verrou), "le bouton a le clavier")

            Taper(VirtualKeyShort.SPACE)
            AttendreQue(Function() verrou.Patterns.Toggle.Pattern.ToggleState.Value = ToggleState.Off, "Espace : relâché")
            Taper(VirtualKeyShort.TAB)
            AttendreQue(Function() Not AFocus(verrou), "Tab : le clavier passe au suivant")
            Taper(VirtualKeyShort.TAB, VirtualKeyShort.TAB)
        End Sub

        <Test>
        Public Sub Toolbar_LeBoutonPlusOuvreLesCommandesQuiDebordent()
            Ouvrir("Choix")
            Dim barre = Trouver("BarreEtroite")
            Dim plus = TrouverParNom(T("aria.moreCommands"), ControlType.Button, barre)
            Montrer(plus)

            Assert.That(plus.IsOffscreen, [Is].False, "visible : les commandes ne tiennent pas dans 360 px")
            Cliquer(plus)
            Dim exporter = TrouverAilleurs(T("showcase.actionbar.export"), ControlType.MenuItem)
            Assert.That(ChercherAilleurs(T("showcase.actionbar.delete"), ControlType.MenuItem)?.IsEnabled, [Is].False,
                        "la commande grisée l'est aussi dans le menu")
            Cliquer(exporter)
            AttendreQue(Function() Referme(T("showcase.actionbar.export")), "le menu « … » refermé")
        End Sub

        <Test>
        Public Sub Registre_LaBarreEtLeRaccourciLancentLesCommandes()
            Ouvrir("Choix")
            Dim barre = Trouver("BarreDuRegistre")
            Dim journal = Trouver("JournalDuRegistre")

            Cliquer(TrouverParNom(T("showcase.actionbar.add"), dans:=barre))
            AttendreQue(Function() NomDe(journal) = "ajouter", "« ajouter » lancée par la barre")

            Raccourci(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_L)
            AttendreQue(Function() NomDe(journal) = "verrouiller : True", "Ctrl+L : « verrouiller », cochée")
            TrouverParNom(T("showcase.choices.locked"), dans:=barre)
        End Sub

        <Test>
        Public Sub Registre_LeMenuLanceLaCommande()
            Ouvrir("Choix")
            Dim menu = Trouver("MenuDuRegistre")
            Dim journal = Trouver("JournalDuRegistre")

            Cliquer(TrouverParNom(T("showcase.actionbar.copy"), ControlType.MenuItem, menu))
            AttendreQue(Function() NomDe(journal) = "copier", "« copier » lancée par le menu")
            Assert.That(TrouverParNom(T("showcase.actionbar.delete"), ControlType.MenuItem, menu).IsEnabled, [Is].False)
        End Sub

    End Class

End Namespace
