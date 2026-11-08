Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Une pièce : les données de la vue de l'exemple.</summary>
    Public NotInheritable Class PieceDuCatalogue
        Public Property Reference As String
        Public Property Nom As String
        Public Property Statut As String

        ''' <summary>Le nom de la ligne pour les lecteurs d'écran.</summary>
        Public Overrides Function ToString() As String
            Return $"{Reference} {Nom}"
        End Function
    End Class

    ''' <summary>Les DataView de l'exemple reçoivent les pièces.</summary>
    Friend NotInheritable Class DataViewPieces

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            For Each nom In {"Pieces", "Grille"}
                Dim vue = TryCast(exemple.FindName(nom), DataView)
                If vue IsNot Nothing Then Remplir(vue)
            Next
        End Sub

#Region "Exemple"
        Private Shared Sub Remplir(vue As DataView)
            Dim noms = {"Bride", "Arbre", "Palier", "Carter", "Pignon", "Couvercle", "Entretoise", "Axe", "Support", "Joint", "Vis", "Écrou"}
            Dim statuts = {"In Work", "Frozen", "Released"}
            vue.ItemsSource = New ObservableCollection(Of PieceDuCatalogue)(
                noms.Select(Function(n, i) New PieceDuCatalogue With {.Reference = $"HS-{41 + i:00000}", .Nom = n, .Statut = statuts(i Mod 3)}))
        End Sub
#End Region

    End Class

End Namespace
