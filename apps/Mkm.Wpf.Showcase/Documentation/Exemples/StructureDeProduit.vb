Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Un nœud de la structure d'un produit : un assemblage, une pièce ou un plan, et ses enfants.</summary>
    Public NotInheritable Class NoeudDeProduit
        Public Property Nom As String
        Public Property Revision As String
        Public Property Maturite As String
        Public Property Proprietaire As String = "M. Kerram"
        Public Property Icone As Object
        Public Property Enfants As New ObservableCollection(Of NoeudDeProduit)

        Public Sub New(nom As String, icone As Object, revision As String, maturite As String, ParamArray enfants As NoeudDeProduit())
            Me.Nom = nom
            Me.Icone = icone
            Me.Revision = revision
            Me.Maturite = maturite
            For Each enfant In enfants
                Me.Enfants.Add(enfant)
            Next
        End Sub

        ''' <summary>Le nom de la ligne pour les lecteurs d'écran : sans lui, Narrateur lirait le nom de la classe.</summary>
        Public Overrides Function ToString() As String
            Return Nom
        End Function

    End Class

    ''' <summary>
    ''' Les grilles arborescentes de l'exemple reçoivent la structure d'un produit, l'assemblage déplié ; les boutons
    ''' « ToutDeplier » et « ToutReplier », s'il y en a, déplient et replient tout.
    ''' </summary>
    Friend NotInheritable Class StructureDeProduit

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            For Each grille In Tous(Of TreeGrid)(exemple)
                Remplir(grille)
                Dim deplier = TryCast(exemple.FindName("ToutDeplier"), Mkm.Wpf.Controls.Button)
                If deplier IsNot Nothing Then AddHandler deplier.Click, Sub() grille.ExpandAll()
                Dim replier = TryCast(exemple.FindName("ToutReplier"), Mkm.Wpf.Controls.Button)
                If replier IsNot Nothing Then AddHandler replier.Click, Sub() grille.CollapseAll()
            Next
        End Sub

#Region "Exemple"
        ' Les racines de l'arbre (Nodes) ; ChildrenPath="Enfants" dit où sont les enfants de chaque nœud.
        Friend Shared Sub Remplir(grille As TreeGrid)
            Dim sousEnsemble = New NoeudDeProduit("HS-SE-010", Icons.Documents, "B", "En cours",
                New NoeudDeProduit("HS-P-011 Carter", Icons.Document, "A", "Validé"),
                New NoeudDeProduit("HS-P-012 Couvercle", Icons.Document, "C", "En cours"),
                New NoeudDeProduit("HS-P-013 Joint", Icons.Document, "A", "Validé"))
            Dim racine = New NoeudDeProduit("HS-ASM-001", Icons.Documents, "A", "En cours",
                sousEnsemble,
                New NoeudDeProduit("HS-SE-020", Icons.Documents, "A", "Validé",
                    New NoeudDeProduit("HS-P-021 Axe", Icons.Document, "A", "Validé")),
                New NoeudDeProduit("HS-PL-001 Plan d'ensemble", Icons.Drawings, "B", "En cours"))
            grille.Nodes = New ObservableCollection(Of NoeudDeProduit) From {racine, New NoeudDeProduit("HS-ASM-002", Icons.Documents, "A", "Validé")}
            grille.Expand(racine)
            grille.Expand(sousEnsemble)
        End Sub
#End Region

    End Class

End Namespace
