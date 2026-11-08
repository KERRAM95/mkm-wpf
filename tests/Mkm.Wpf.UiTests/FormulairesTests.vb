Imports System
Imports System.Linq
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>
    ''' Les champs et les choix : InputText, Select, Checkbox, Tabs, Fieldset, SelectButton, InputNumber, IdCard.
    ''' </summary>
    <TestFixture>
    Public Class FormulairesTests
        Inherits Scenario

        <Test>
        Public Sub InputText_LaSaisieSEcritEnMajuscules()
            Ouvrir("Champs")
            Dim reference = TrouverParNom(T("showcase.fields.referenceHint"), ControlType.Edit)
            Cliquer(reference)
            Ecrire("hs-12")

            AttendreQue(Function() reference.Patterns.Value.Pattern.Value.Value = "HS-12", "« HS-12 » : CharacterCasing=Upper")
        End Sub

        <Test>
        Public Sub Select_UnClicOuvreLaListeEtChoisit()
            Ouvrir("Champs")
            Dim maturite = Trouver("Maturite")

            Cliquer(maturite)
            Dim fige = Attendre(Function() If(maturite.FindFirstDescendant(Function(cf) cf.ByName("Frozen").And(cf.ByControlType(ControlType.ListItem))),
                                             ChercherAilleurs("Frozen", ControlType.ListItem)), "« Frozen » dans la liste")
            Cliquer(fige)
            AttendreQue(Function() NomDe(maturite.AsComboBox().SelectedItem) = "Frozen", "« Frozen » choisie")
            Assert.That(maturite.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value, [Is].EqualTo(ExpandCollapseState.Collapsed))
        End Sub

        <Test>
        Public Sub Select_LeClavierParcourtLaListe()
            Ouvrir("Champs")
            Dim maturite = Trouver("Maturite")
            maturite.Focus()

            Taper(VirtualKeyShort.DOWN)
            AttendreQue(Function() NomDe(maturite.AsComboBox().SelectedItem) = "Private", "↓ : la première valeur")
            Taper(VirtualKeyShort.DOWN)
            AttendreQue(Function() NomDe(maturite.AsComboBox().SelectedItem) = "In Work", "↓ : la suivante")
        End Sub

        <Test>
        Public Sub Checkbox_UnClicEtEspaceLaCochent()
            Ouvrir("Boutons")
            Dim cas = Trouver("CaseVide").AsCheckBox()

            Assert.That(cas.IsChecked, [Is].False)
            Cliquer(cas)
            AttendreQue(Function() cas.IsChecked.GetValueOrDefault(), "cochée au clic")
            Taper(VirtualKeyShort.SPACE)
            AttendreQue(Function() cas.IsChecked.HasValue AndAlso Not cas.IsChecked.Value, "décochée par Espace")
        End Sub

        <Test>
        Public Sub Tabs_UnClicMontreSonContenu()
            Ouvrir("Conteneurs")
            Dim onglets = Trouver("Onglets")
            Dim notes = TrouverParNom(T("showcase.containers.notes"), ControlType.TabItem, onglets)

            Cliquer(notes)
            AttendreQue(Function() notes.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Notes » choisi")
            TrouverParNom(T("showcase.containers.notesText"), dans:=onglets)
        End Sub

        <Test>
        Public Sub Tabs_LesFlechesChangentDOnglet()
            Ouvrir("Conteneurs")
            Dim onglets = Trouver("Onglets")
            Dim cartouche = TrouverParNom(T("showcase.containers.titleBlock"), ControlType.TabItem, onglets)
            Dim notes = TrouverParNom(T("showcase.containers.notes"), ControlType.TabItem, onglets)
            Cliquer(cartouche)

            Taper(VirtualKeyShort.RIGHT)
            AttendreQue(Function() notes.Patterns.SelectionItem.Pattern.IsSelected.Value, "→ : « Notes »")
        End Sub

        <Test>
        Public Sub Fieldset_SaLegendeLeReplieEtLeDeplie()
            Ouvrir("Conteneurs")
            Dim groupe = Trouver("Repliable")
            Dim option1 = TrouverParNom(T("showcase.containers.option"), ControlType.CheckBox, groupe)
            Dim legende = TrouverParNom(T("showcase.containers.toggleable"), ControlType.Button, groupe)

            Cliquer(legende)
            AttendreQue(Function() option1.IsOffscreen, "replié : son contenu caché")
            Cliquer(legende)
            AttendreQue(Function() Not option1.IsOffscreen, "déplié : son contenu revenu")
        End Sub

        <Test>
        Public Sub SelectButton_UnClicAjouteAuChoixMultiple()
            Ouvrir("Identite")
            Dim plusieurs = Trouver("Plusieurs")
            Dim historique = TrouverParNom(T("showcase.containers.history"), ControlType.ListItem, plusieurs)
            Dim proprietes = TrouverParNom(T("showcase.containers.properties"), ControlType.ListItem, plusieurs)

            Cliquer(historique)
            AttendreQue(Function() historique.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Historique » ajouté")
            Assert.That(proprietes.Patterns.SelectionItem.Pattern.IsSelected.Value, [Is].True, "les autres restent choisis")
            Cliquer(proprietes)
            AttendreQue(Function() Not proprietes.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Propriétés » retiré")
        End Sub

        <Test>
        Public Sub InputNumber_SesBoutonsAvancentEtSArretentALaBorne()
            Ouvrir("Identite")
            Dim feuilles = Trouver("Feuilles")
            Dim champ = Attendre(Function() feuilles.FindFirstDescendant(Function(cf) cf.ByControlType(ControlType.Edit)), "le champ du nombre")
            Dim plus = TrouverParNom(T("aria.increment"), ControlType.Button, feuilles)
            Dim moins = TrouverParNom(T("aria.decrement"), ControlType.Button, feuilles)

            Cliquer(plus)
            Cliquer(plus)
            AttendreQue(Function() champ.Patterns.Value.Pattern.Value.Value = "3", "deux clics sur + : 3")
            For i = 1 To 4
                Cliquer(moins)
            Next
            AttendreQue(Function() champ.Patterns.Value.Pattern.Value.Value = "1", "le minimum : 1")
        End Sub

        <Test>
        Public Sub InputNumber_LesFlechesDuClavierLeChangent()
            Ouvrir("Identite")
            Dim feuilles = Trouver("Feuilles")
            Dim champ = Attendre(Function() feuilles.FindFirstDescendant(Function(cf) cf.ByControlType(ControlType.Edit)), "le champ du nombre")
            Cliquer(champ)

            Taper(VirtualKeyShort.UP)
            AttendreQue(Function() champ.Patterns.Value.Pattern.Value.Value = "2", "↑ : 2")
            Taper(VirtualKeyShort.DOWN, VirtualKeyShort.DOWN)
            AttendreQue(Function() champ.Patterns.Value.Pattern.Value.Value = "1", "↓ ↓ : 1, le minimum")
        End Sub

        <Test>
        Public Sub IdCard_SonBoutonLaReplieEtLaDeplie()
            Ouvrir("Identite")
            Dim replier = TrouverParNom(T("aria.collapse"), ControlType.Button)

            Cliquer(replier)
            AttendreQue(Function() NomDe(replier) = T("aria.expand"), "repliée : son bouton dit « Déplier »")
            Cliquer(replier)
            AttendreQue(Function() NomDe(replier) = T("aria.collapse"), "dépliée : « Replier »")
        End Sub

    End Class

End Namespace
