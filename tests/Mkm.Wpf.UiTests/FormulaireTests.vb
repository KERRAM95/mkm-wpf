Imports System
Imports System.Linq
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.Input
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

        ' Le défaut vu par Mourad : la liste ouverte, la molette faisait défiler la page et la liste restait à sa place,
        ' détachée de son champ. Comme mkm, la page qui défile la referme.
        <Test>
        Public Sub Select_LaListeSeRefermeQuandLaPageDefile()
            Dim page = OuvrirLaDoc("select")
            Dim maturite = Trouver("Maturite", page)
            Dim etat = Function() maturite.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value

            Cliquer(maturite)
            AttendreQue(Function() etat() = ExpandCollapseState.Expanded, "la liste ouverte")
            ' La molette au-dessus du champ, sur la page : la liste s'ouvre dessous.
            Dim c = Centre(maturite)
            Mouse.MoveTo(c.X, c.Y - 40)
            Mouse.Scroll(-3)
            Wait.UntilInputIsProcessed()
            AttendreQue(Function() etat() = ExpandCollapseState.Collapsed, "la page a défilé : la liste refermée")
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

        <Test>
        Public Sub ToggleSwitch_UnClicEtEspaceLeBasculent()
            Dim page = OuvrirLaDoc("toggleswitch")
            Dim synchro = Trouver("Synchro", page)
            Dim etat = Function() synchro.Patterns.Toggle.Pattern.ToggleState.Value

            Assert.That(NomDe(synchro), [Is].EqualTo("Synchroniser avec 3DEXPERIENCE"))
            Cliquer(synchro)
            AttendreQue(Function() etat() = ToggleState.On, "activé au clic")
            Taper(VirtualKeyShort.SPACE)
            AttendreQue(Function() etat() = ToggleState.Off, "désactivé par Espace")
        End Sub

        <Test>
        Public Sub Listbox_UnClicChoisitEtLeFiltreNeGardeQueCeQuiRepond()
            Dim page = OuvrirLaDoc("listbox")
            Dim maturites = Trouver("Maturites", page)
            Dim enCours = TrouverParNom("In Work", ControlType.ListItem, maturites)

            Cliquer(enCours)
            AttendreQue(Function() enCours.Patterns.SelectionItem.Pattern.IsSelected.Value, "« In Work » choisi")

            Dim filtree = Trouver("Filtree", page)
            Cliquer(TrouverParNom(T("aria.filter"), ControlType.Edit, filtree))
            Ecrire("fro")
            AttendreQue(Function() filtree.FindAllDescendants(Function(cf) cf.ByControlType(ControlType.ListItem)).Length = 1, "seule « Frozen » reste")
            Taper(VirtualKeyShort.DOWN)
            AttendreQue(Function() AFocus(TrouverParNom("Frozen", ControlType.ListItem, filtree)), "↓ : le clavier sur « Frozen »")
        End Sub

        <Test>
        Public Sub MultiSelect_DesChoixAuClicEtAuClavier()
            Dim page = OuvrirLaDoc("multiselect")
            Dim maturites = Trouver("Maturites", page)
            Dim etat = Function() maturites.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value
            Dim valeur = Function() maturites.Patterns.Value.Pattern.Value.Value

            Assert.That(NomDe(maturites), [Is].EqualTo("Maturités"), "nommé par son texte d'aide")
            Cliquer(maturites)
            AttendreQue(Function() etat() = ExpandCollapseState.Expanded, "la liste ouverte")
            Dim fige = Attendre(Function() If(maturites.FindFirstDescendant(Function(cf) cf.ByName("Frozen").And(cf.ByControlType(ControlType.ListItem))),
                                             ChercherAilleurs("Frozen", ControlType.ListItem)), "« Frozen » dans la liste")
            Cliquer(fige)
            AttendreQue(Function() valeur() = "Frozen", "« Frozen » choisi")
            Taper(VirtualKeyShort.HOME, VirtualKeyShort.SPACE)
            AttendreQue(Function() valeur() = "Frozen, Private", "Début, Espace : « Private » ajouté")
            Taper(VirtualKeyShort.ESCAPE)
            AttendreQue(Function() etat() = ExpandCollapseState.Collapsed, "Échap referme la liste")
        End Sub

        <Test>
        Public Sub AutoComplete_LaFrappeProposeEtLeClavierChoisit()
            Dim page = OuvrirLaDoc("autocomplete")
            Dim reference = Trouver("Reference", page)

            Cliquer(reference)
            Ecrire("hs-0004")
            TrouverAilleurs("HS-00041", ControlType.ListItem)
            Taper(VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.RETURN)
            AttendreQue(Function() NomDe(Trouver("Journal", page)) = "Choisie : HS-00041", "↓ ↓ Entrée : « HS-00041 »")
            Assert.That(reference.Patterns.Value.Pattern.Value.Value, [Is].EqualTo("HS-00041"))
        End Sub

        ' Un élément d'une liste ouverte : sous le champ pour UI Automation, ou dans sa fenêtre à part.
        Private Function DansLaListe(champ As AutomationElement, nom As String, role As ControlType) As AutomationElement
            Return Attendre(Function() If(champ.FindFirstDescendant(Function(cf) cf.ByName(nom).And(cf.ByControlType(role))),
                                          ChercherAilleurs(nom, role)), $"« {nom} » dans la liste")
        End Function

        <Test>
        Public Sub TreeSelect_UnClicEtLeClavierChoisissentUnNoeud()
            Dim page = OuvrirLaDoc("treeselect")
            Dim dossier = Trouver("Dossier", page)
            Dim etat = Function() dossier.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value
            Dim valeur = Function() dossier.Patterns.Value.Pattern.Value.Value

            Assert.That(NomDe(dossier), [Is].EqualTo("Choisir un dossier"), "nommé par son texte d'aide")
            Cliquer(dossier)
            AttendreQue(Function() etat() = ExpandCollapseState.Expanded, "l'arbre ouvert")
            Cliquer(DansLaListe(dossier, "Notices", ControlType.TreeItem))
            AttendreQue(Function() valeur() = "Notices", "un clic choisit « Notices »")
            AttendreQue(Function() etat() = ExpandCollapseState.Collapsed, "et referme l'arbre")
            AttendreQue(Function() NomDe(Trouver("Choix", page)) = "Dossier choisi : Notices", "SelectedItemChanged")

            ' Au clavier : ↓ rouvre l'arbre sur le nœud choisi, ↑ remonte à « Plans », Entrée le choisit.
            Taper(VirtualKeyShort.DOWN)
            Dim notices = DansLaListe(dossier, "Notices", ControlType.TreeItem)
            AttendreQue(Function() AFocus(notices), "le clavier sur le nœud choisi")
            Taper(VirtualKeyShort.UP, VirtualKeyShort.RETURN)
            AttendreQue(Function() valeur() = "Plans", "↑ Entrée : « Plans »")
            AttendreQue(Function() AFocus(dossier), "le clavier revient au champ")
            Taper(VirtualKeyShort.DOWN)
            AttendreQue(Function() etat() = ExpandCollapseState.Expanded, "↓ rouvre l'arbre")
            Taper(VirtualKeyShort.ESCAPE)
            AttendreQue(Function() etat() = ExpandCollapseState.Collapsed, "Échap le referme")
            Assert.That(valeur(), [Is].EqualTo("Plans"), "sans rien choisir")
        End Sub

        <Test>
        Public Sub CascadeSelect_LaSourisPuisLeClavierChoisissentUneMachine()
            Dim page = OuvrirLaDoc("cascadeselect")
            Dim machine = Trouver("Machine", page)
            Dim valeur = Function() machine.Patterns.Value.Pattern.Value.Value

            Assert.That(NomDe(machine), [Is].EqualTo("Choisir une machine"), "nommé par son texte d'aide")
            Cliquer(machine)
            Cliquer(DansLaListe(machine, "Lyon", ControlType.MenuItem))
            Cliquer(DansLaListe(machine, "Usinage", ControlType.MenuItem))
            Cliquer(DansLaListe(machine, "Fraiseuse 5 axes", ControlType.MenuItem))
            AttendreQue(Function() valeur() = "Fraiseuse 5 axes", "un clic choisit « Fraiseuse 5 axes »")
            AttendreQue(Function() NomDe(Trouver("Choix", page)) = "Machine choisie : Fraiseuse 5 axes", "SelectedItemChanged")
            AttendreQue(Function() Referme("Usinage"), "les listes refermées")

            ' Au clavier : ↓ ouvre sur « Lyon », ↓ passe à « Toulouse », → ouvre sa suite, → encore, Entrée choisit.
            AttendreQue(Function() AFocus(machine), "le clavier au champ")
            Taper(VirtualKeyShort.DOWN)
            Dim lyon = DansLaListe(machine, "Lyon", ControlType.MenuItem)
            AttendreQue(Function() AFocus(lyon), "le clavier sur « Lyon »")
            Taper(VirtualKeyShort.DOWN, VirtualKeyShort.RIGHT)
            Dim peinture = DansLaListe(machine, "Peinture", ControlType.MenuItem)
            AttendreQue(Function() AFocus(peinture), "→ ouvre la suite de « Toulouse »")
            Taper(VirtualKeyShort.RIGHT)
            Dim cabine = DansLaListe(machine, "Cabine 2", ControlType.MenuItem)
            AttendreQue(Function() AFocus(cabine), "→ encore")
            Taper(VirtualKeyShort.RETURN)
            AttendreQue(Function() valeur() = "Cabine 2", "Entrée : « Cabine 2 »")
        End Sub

        <Test>
        Public Sub Rating_UnClicEtLesFlechesChangentLaNote()
            Dim page = OuvrirLaDoc("rating")
            Dim note = Trouver("Note", page)
            Dim valeur = Function() note.Patterns.RangeValue.Pattern.Value.Value
            Assert.That(valeur(), [Is].EqualTo(3))

            ' La quatrième étoile : 3 étoiles de 12 et 3 écarts de 3, puis la moitié de 12, sur 72.
            Montrer(note)
            Dim cadre = note.BoundingRectangle
            Mouse.Click(New System.Drawing.Point(cadre.Left + CInt(cadre.Width * 51 / 72), cadre.Top + cadre.Height \ 2))
            AttendreQue(Function() valeur() = 4, "un clic sur la quatrième étoile : 4")
            AttendreQue(Function() NomDe(Trouver("Note", page)) = "Qualité du plan", "nommée")
            Taper(VirtualKeyShort.LEFT)
            AttendreQue(Function() valeur() = 3, "← : 3")
            Taper(VirtualKeyShort.DELETE)
            AttendreQue(Function() valeur() = 0, "Suppr retire la note")
        End Sub

        <Test>
        Public Sub Slider_LeClavierDeplaceLaPoignee()
            Dim page = OuvrirLaDoc("slider")
            Dim curseur = Trouver("Opacite", page)
            Dim valeur = Function() curseur.Patterns.RangeValue.Pattern.Value.Value
            Montrer(curseur)
            curseur.Focus()
            Taper(VirtualKeyShort.END)
            AttendreQue(Function() valeur() = 100, "Fin : 100")
            Taper(VirtualKeyShort.LEFT)
            AttendreQue(Function() valeur() = 99, "← : 99")
        End Sub

        <Test>
        Public Sub Knob_LeClavierEtLaSourisLeTournent()
            Dim page = OuvrirLaDoc("knob")
            Dim bouton = Trouver("Avancement", page)
            Dim valeur = Function() bouton.Patterns.RangeValue.Pattern.Value.Value
            Montrer(bouton)
            ' Le haut de l'arc : la moitié.
            Dim cadre = bouton.BoundingRectangle
            Mouse.Click(New System.Drawing.Point(cadre.Left + cadre.Width \ 2, cadre.Top + cadre.Height \ 10))
            AttendreQue(Function() Math.Abs(valeur() - 50) <= 1, "un clic en haut de l'arc : 50")
            Taper(VirtualKeyShort.PRIOR)
            AttendreQue(Function() Math.Abs(valeur() - 60) <= 1, "Page haut : 10 de plus")
            Taper(VirtualKeyShort.HOME)
            AttendreQue(Function() valeur() = 0, "Début : 0")
        End Sub

    End Class

End Namespace
