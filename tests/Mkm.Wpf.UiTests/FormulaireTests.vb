Imports System
Imports System.Linq
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Les composants de saisie, sur leurs pages de documentation : la souris, le clavier, ce que lit Narrateur.</summary>
    <TestFixture>
    Public Class FormulaireTests
        Inherits Scenario

        <Test>
        Public Sub InputText_LaSaisieSEcritEnMajuscules()
            Dim page = OuvrirLaDoc("inputtext")
            Dim reference = Trouver("Reference", page)

            Assert.That(NomDe(reference), [Is].EqualTo("ex. HS-1234"), "sans libellé, son texte indicatif le nomme")
            Cliquer(reference)
            Ecrire("hs-12")
            AttendreQue(Function() reference.Patterns.Value.Pattern.Value.Value = "HS-12", "« HS-12 » : CharacterCasing=Upper")
        End Sub

        <Test>
        Public Sub Select_UnClicOuvreLaListeEtChoisit()
            Dim page = OuvrirLaDoc("select")
            Dim maturite = Trouver("Maturite", page)

            Cliquer(maturite)
            Dim fige = Attendre(Function() If(maturite.FindFirstDescendant(Function(cf) cf.ByName("Frozen").And(cf.ByControlType(ControlType.ListItem))),
                                             ChercherAilleurs("Frozen", ControlType.ListItem)), "« Frozen » dans la liste")
            Cliquer(fige)
            AttendreQue(Function() NomDe(maturite.AsComboBox().SelectedItem) = "Frozen", "« Frozen » choisie")
            Assert.That(maturite.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value, [Is].EqualTo(ExpandCollapseState.Collapsed))
        End Sub

        <Test>
        Public Sub Select_LeClavierParcourtLaListe()
            Dim page = OuvrirLaDoc("select")
            Dim maturite = Trouver("Maturite", page)
            Montrer(maturite)
            maturite.Focus()

            Taper(VirtualKeyShort.DOWN)
            AttendreQue(Function() NomDe(maturite.AsComboBox().SelectedItem) = "Private", "↓ : la première valeur")
            Taper(VirtualKeyShort.DOWN)
            AttendreQue(Function() NomDe(maturite.AsComboBox().SelectedItem) = "In Work", "↓ : la suivante")
        End Sub

        <Test>
        Public Sub Select_LaLongueListeSOuvreEtSeParcourtAuClavier()
            Dim page = OuvrirLaDoc("select")
            Dim longue = Trouver("Longue", page)
            Dim etat = Function() longue.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value

            ' 5 000 valeurs, virtualisées : la liste s'ouvre aussitôt, et le clavier la parcourt.
            Cliquer(longue)
            AttendreQue(Function() etat() = ExpandCollapseState.Expanded, "la liste ouverte")
            Taper(VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.RETURN)
            AttendreQue(Function() etat() = ExpandCollapseState.Collapsed, "Entrée referme la liste")
            AttendreQue(Function() NomDe(longue.AsComboBox().SelectedItem).StartsWith("HS-0000", StringComparison.Ordinal), "une des premières références choisie")
        End Sub

        <Test>
        Public Sub Checkbox_UnClicEtEspaceLaCochent()
            Dim page = OuvrirLaDoc("checkbox")
            Dim cas = Trouver("Toutes", page).AsCheckBox()

            Assert.That(cas.IsChecked, [Is].False)
            Cliquer(cas)
            AttendreQue(Function() cas.IsChecked.GetValueOrDefault(), "cochée au clic")
            Taper(VirtualKeyShort.SPACE)
            AttendreQue(Function() cas.IsChecked.HasValue AndAlso Not cas.IsChecked.Value, "décochée par Espace")
        End Sub

        <Test>
        Public Sub RadioButton_UnClicChoisitEtLesFlechesPassentAuVoisin()
            Dim page = OuvrirLaDoc("radiobutton")
            Dim plan = Trouver("RadioPlan", page).AsRadioButton()
            Dim article = Trouver("RadioArticle", page).AsRadioButton()

            Cliquer(article)
            AttendreQue(Function() article.IsChecked AndAlso Not plan.IsChecked, "« Article » choisi, « Plan » non")
            Taper(VirtualKeyShort.LEFT)
            AttendreQue(Function() plan.IsChecked AndAlso AFocus(plan), "← : « Plan » choisi et au clavier")
            Taper(VirtualKeyShort.RIGHT)
            AttendreQue(Function() article.IsChecked AndAlso AFocus(article), "→ : « Article » de nouveau")
        End Sub

        <Test>
        Public Sub ToggleButton_UnClicLeBasculeEtChangeSonLibelle()
            Dim page = OuvrirLaDoc("togglebutton")
            Dim verrou = Trouver("Verrou", page)
            Dim bascule = verrou.Patterns.Toggle.Pattern

            Assert.That(NomDe(verrou), [Is].EqualTo("Déverrouillé"), "son libellé d'état le nomme")
            Cliquer(verrou)
            AttendreQue(Function() bascule.ToggleState.Value = ToggleState.On, "enfoncé")
            AttendreQue(Function() NomDe(verrou) = "Verrouillé", "« Verrouillé », son libellé enfoncé")
            Taper(VirtualKeyShort.SPACE)
            AttendreQue(Function() bascule.ToggleState.Value = ToggleState.Off, "Espace : relâché")
            ' Le défaut du lot 5 : le contour du focus d'un ToggleButton plantait la vitrine au premier Tab.
            Taper(VirtualKeyShort.TAB)
            AttendreQue(Function() Not AFocus(verrou), "Tab : le clavier passe au suivant")
        End Sub

        <Test>
        Public Sub SelectButton_UnClicAjouteAuChoixMultiple()
            Dim page = OuvrirLaDoc("selectbutton")
            Dim plusieurs = Trouver("Plusieurs", page)
            Dim historique = TrouverParNom("Historique", ControlType.ListItem, plusieurs)
            Dim proprietes = TrouverParNom("Propriétés", ControlType.ListItem, plusieurs)

            Cliquer(historique)
            AttendreQue(Function() historique.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Historique » ajouté")
            Assert.That(proprietes.Patterns.SelectionItem.Pattern.IsSelected.Value, [Is].True, "les autres restent choisis")
            Cliquer(proprietes)
            AttendreQue(Function() Not proprietes.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Propriétés » retiré")
        End Sub

        <Test>
        Public Sub InputNumber_SesBoutonsEtSesFlechesSArretentALaBorne()
            Dim page = OuvrirLaDoc("inputnumber")
            Dim feuilles = Trouver("Feuilles", page)
            Dim champ = Attendre(Function() feuilles.FindFirstDescendant(Function(cf) cf.ByControlType(ControlType.Edit)), "le champ du nombre")
            Dim plus = TrouverParNom(T("aria.increment"), ControlType.Button, feuilles)
            Dim moins = TrouverParNom(T("aria.decrement"), ControlType.Button, feuilles)

            Assert.That(feuilles.Patterns.RangeValue.Pattern.Maximum.Value, [Is].EqualTo(10), "une toupie qui dit ses bornes")
            Cliquer(plus)
            Cliquer(plus)
            AttendreQue(Function() champ.Patterns.Value.Pattern.Value.Value = "3", "deux clics sur + : 3")
            For i = 1 To 4
                Cliquer(moins)
            Next
            AttendreQue(Function() champ.Patterns.Value.Pattern.Value.Value = "1", "le minimum : 1")
            Cliquer(champ)
            Taper(VirtualKeyShort.UP)
            AttendreQue(Function() champ.Patterns.Value.Pattern.Value.Value = "2", "↑ : 2")
        End Sub

    End Class

End Namespace
