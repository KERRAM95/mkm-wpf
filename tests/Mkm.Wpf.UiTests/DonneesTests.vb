Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Les données, sur leurs pages : DataTable, TreeGrid, Tree, StateGraph.</summary>
    <TestFixture>
    Public Class DonneesTests
        Inherits Scenario

        ' Le texte d'une cellule : sa valeur, ou ce qu'elle montre.
        Private Shared Function Cellule(grille As AutomationElement, ligne As Integer, colonne As Integer) As String
            Dim element = grille.Patterns.Grid.Pattern.GetItem(ligne, colonne)
            If element.Patterns.Value.IsSupported Then Return element.Patterns.Value.Pattern.Value.Value
            Return TexteDe(element).Trim()
        End Function

        Private Shared Function Colonne(grille As AutomationElement, index As Integer) As List(Of String)
            Dim lignes = grille.Patterns.Grid.Pattern.RowCount.Value
            Return Enumerable.Range(0, lignes).Select(Function(l) Cellule(grille, l, index)).ToList()
        End Function

        <Test>
        Public Sub DataTable_UnClicSurLEnTeteTrieDansLesDeuxSens()
            Dim page = OuvrirLaDoc("datatable")
            Dim variables = Trouver("Variables", page)
            Dim entete = TrouverParNom("Nom", ControlType.HeaderItem, variables)

            ' Les premières lignes, celles que le tableau montre (les autres ne sont pas encore dessinées).
            Cliquer(entete)
            AttendreQue(Function() Cellule(variables, 0, 0) = "CATDLLPath", "triée de A à Z : « CATDLLPath » en tête")
            Cliquer(entete)
            AttendreQue(Function() Cellule(variables, 0, 0) = "HSCART_TENANT", "triée de Z à A : « HSCART_TENANT » en tête")
            Assert.That(String.Compare(Cellule(variables, 1, 0), Cellule(variables, 0, 0), StringComparison.CurrentCulture), [Is].LessThan(0))
            ' Le titre cliqué a le clavier : Espace le trie à nouveau, comme mkm.
            Taper(VirtualKeyShort.SPACE)
            AttendreQue(Function() Cellule(variables, 0, 0) = "CATDLLPath", "Espace : de nouveau de A à Z")
        End Sub

        <Test>
        Public Sub DataTable_LeChargementSeDitAuxLecteursDEcran()
            Dim page = OuvrirLaDoc("datatable")
            Dim chargement = Trouver("Chargement", page)

            Cliquer(Trouver("Charger", page))
            AttendreQue(Function() chargement.ItemStatus = T("aria.busy"), "« Chargement en cours », son état")
            AttendreQue(Function() String.IsNullOrEmpty(chargement.ItemStatus), "chargé : plus d'état", TimeSpan.FromSeconds(8))
        End Sub

        <Test>
        Public Sub TreeGrid_SonBoutonReplieEtDeplieUnNoeud()
            Dim page = OuvrirLaDoc("treegrid")
            Dim produit = Trouver("Produit", page)
            Dim lignes = Function() produit.Patterns.Grid.Pattern.RowCount.Value

            AttendreQue(Function() lignes() = 8, "8 lignes : la racine et son sous-ensemble dépliés")
            Cliquer(TrouverParNom(T("aria.collapse"), ControlType.Button, produit))
            AttendreQue(Function() lignes() = 2, "la racine repliée : 2 lignes")
            Cliquer(TrouverParNom(T("aria.expand"), ControlType.Button, produit))
            AttendreQue(Function() lignes() = 8, "dépliée de nouveau, son sous-ensemble aussi : 8 lignes")
        End Sub

        <Test>
        Public Sub TreeGrid_ToutReplierEtToutDeplier()
            Dim page = OuvrirLaDoc("treegrid")
            Dim cadre = Exemple(page, "deplier-et-replier")
            Dim grille = Attendre(Function() cadre.FindFirstDescendant(Function(cf) cf.ByClassName("TreeGrid")), "la TreeGrid")
            Dim lignes = Function() grille.Patterns.Grid.Pattern.RowCount.Value

            Cliquer(Trouver("ToutReplier", cadre))
            AttendreQue(Function() lignes() = 2, "tout replié : les 2 racines")
            Cliquer(Trouver("ToutDeplier", cadre))
            AttendreQue(Function() lignes() > 8, "tout déplié")
        End Sub

        <Test>
        Public Sub Tree_UnClicChoisitEtLesFlechesReplient()
            Dim page = OuvrirLaDoc("tree")
            Dim notes = Trouver("Notes", page)
            Dim generales = TrouverParNom("Notes générales", ControlType.TreeItem, notes)

            Cliquer(TrouverParNom("Notes de soudure", ControlType.TreeItem, notes))
            AttendreQue(Function() NomDe(Trouver("Choix", page)) = "Nœud choisi : Notes de soudure", "le nœud choisi")
            Cliquer(TrouverParNom("Notes générales", dans:=generales))
            Taper(VirtualKeyShort.LEFT)
            AttendreQue(Function() generales.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value = ExpandCollapseState.Collapsed, "← : replié")
            Taper(VirtualKeyShort.RIGHT)
            AttendreQue(Function() generales.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value = ExpandCollapseState.Expanded, "→ : déplié")
        End Sub

        <Test>
        Public Sub StateGraph_UneActionSeLanceALaSourisEtAuClavier()
            Dim page = OuvrirLaDoc("stategraph")
            Dim cadre = Exemple(page, "actions")
            Dim journal = Trouver("Journal", cadre)

            Cliquer(TrouverParNom("Geler", ControlType.Button, cadre))
            AttendreQue(Function() NomDe(journal) = "Action choisie : Geler", "« Geler » au clic")
            Dim valider = TrouverParNom("Valider", ControlType.Button, cadre)
            valider.Patterns.Invoke.Pattern.Invoke()
            AttendreQue(Function() NomDe(journal) = "Action choisie : Valider", "« Valider » lancée par un lecteur d'écran")
        End Sub

        <Test>
        Public Sub Paginator_SesFlechesEtSesLiensMenentALaPage()
            Dim page = OuvrirLaDoc("paginator")
            Dim pages = Trouver("Pages", page)
            Dim journal = Trouver("Journal", page)

            Cliquer(TrouverParNom(T("aria.nextPage"), ControlType.Button, pages))
            AttendreQue(Function() NomDe(journal) = "Page 2 : lignes 11 à 20", "« Page suivante »")
            Cliquer(TrouverParNom(T("aria.page", 4), ControlType.Button, pages))
            AttendreQue(Function() NomDe(journal) = "Page 4 : lignes 31 à 40", "« Page 4 »")
            Cliquer(TrouverParNom(T("aria.lastPage"), ControlType.Button, pages))
            AttendreQue(Function() NomDe(journal) = "Page 12 : lignes 111 à 120", "« Dernière page »")
            Assert.That(TrouverParNom(T("aria.nextPage"), ControlType.Button, pages).IsEnabled, [Is].False, "grisée à la dernière page")
        End Sub

        <Test>
        Public Sub Timeline_UneListeDEvenements()
            Dim page = OuvrirLaDoc("timeline")
            Dim frise = Trouver("Revisions", page)

            Assert.That(frise.ControlType, [Is].EqualTo(ControlType.List))
            Assert.That(frise.FindAllChildren(Function(cf) cf.ByControlType(ControlType.ListItem)).Length, [Is].EqualTo(4))
        End Sub

        <Test>
        Public Sub OrderList_UnElementChoisiMonteToutEnHaut()
            Dim page = OuvrirLaDoc("orderlist")
            Dim liste = Trouver("Etapes", page)
            Cliquer(TrouverParNom("Faire valider", ControlType.ListItem, liste))
            Cliquer(TrouverParNom(T("aria.moveTop"), ControlType.Button, liste))
            AttendreQue(Function() NomDe(liste.FindFirstDescendant(Function(cf) cf.ByControlType(ControlType.ListItem))) = "Faire valider",
                        "« Faire valider » tout en haut")
        End Sub

        <Test>
        Public Sub PickList_UnDoubleClicFaitPasserUnElement()
            Dim page = OuvrirLaDoc("picklist")
            Dim choix = Trouver("Diffusion", page)
            Dim cible = Trouver("PART_Target", choix)
            DoubleCliquer(TrouverParNom("Qualité", ControlType.ListItem, Trouver("PART_Source", choix)))
            AttendreQue(Function() cible.FindFirstDescendant(Function(cf) cf.ByName("Qualité").And(cf.ByControlType(ControlType.ListItem))) IsNot Nothing,
                        "« Qualité » dans la cible")
            Cliquer(TrouverParNom(T("aria.moveAllToSource"), ControlType.Button, choix))
            AttendreQue(Function() cible.FindFirstDescendant(Function(cf) cf.ByControlType(ControlType.ListItem)) Is Nothing, "la cible vidée")
        End Sub

        <Test>
        Public Sub DataView_LePaginateurChangeDePage()
            Dim page = OuvrirLaDoc("dataview")
            Dim vue = Trouver("Pieces", page)
            Trouver("PART_Paginator", vue)
            Cliquer(TrouverParNom(T("aria.nextPage"), ControlType.Button, vue))
            AttendreQue(Function() TexteDe(vue).Contains("Pignon"), "la deuxième page : « Pignon »")
            Assert.That(TexteDe(vue).Contains("Bride"), [Is].False, "la première n'est plus montrée")
        End Sub

        <Test>
        Public Sub OrganizationChart_UnNoeudSeChoisitEtSeReplie()
            Dim page = OuvrirLaDoc("organizationchart")
            Dim organigramme = Trouver("Organigramme", page)
            Dim production = TrouverParNom("Production", ControlType.TreeItem, organigramme)
            Cliquer(TrouverParNom("Production", ControlType.Text, production))
            AttendreQue(Function() production.Patterns.SelectionItem.Pattern.IsSelected.Value, "« Production » choisi")
            Cliquer(TrouverParNom(T("aria.collapse"), ControlType.Button, production))
            AttendreQue(Function() production.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value = ExpandCollapseState.Collapsed, "replié")
            AttendreQue(Function()
                            Dim usinage = organigramme.FindFirstDescendant(Function(cf) cf.ByName("Usinage").And(cf.ByControlType(ControlType.TreeItem)))
                            Return usinage Is Nothing OrElse usinage.IsOffscreen
                        End Function, "ses enfants cachés")
        End Sub

    End Class

End Namespace
