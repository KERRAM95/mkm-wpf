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

    End Class

End Namespace
