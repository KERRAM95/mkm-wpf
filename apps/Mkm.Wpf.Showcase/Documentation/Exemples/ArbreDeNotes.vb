Imports System.Collections.ObjectModel
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Une note d'un plan, et ses notes filles : les données de l'arbre lié.</summary>
    Public NotInheritable Class NoteDePlan
        Public Property Titre As String
        Public Property Icone As Object
        Public Property Enfants As New ObservableCollection(Of NoteDePlan)
    End Class

    ''' <summary>L'arbre « NotesLiees » de l'exemple reçoit ses nœuds de données.</summary>
    Friend NotInheritable Class ArbreDeNotes

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Remplir(DirectCast(exemple.FindName("NotesLiees"), Tree))
        End Sub

#Region "Exemple"
        ' Un nœud par objet : le HierarchicalDataTemplate de l'arbre dit son titre et où sont ses enfants.
        Private Shared Sub Remplir(arbre As Tree)
            Dim generales As New NoteDePlan With {.Titre = "Notes générales", .Icone = Icons.Document}
            generales.Enfants.Add(New NoteDePlan With {.Titre = "Casser les angles vifs"})
            generales.Enfants.Add(New NoteDePlan With {.Titre = "Tolérances ISO 2768-mK"})
            Dim soudure As New NoteDePlan With {.Titre = "Notes de soudure", .Icone = Icons.Document}
            soudure.Enfants.Add(New NoteDePlan With {.Titre = "Cordon continu"})
            arbre.ItemsSource = New ObservableCollection(Of NoteDePlan) From {generales, soudure}
        End Sub
#End Region

    End Class

End Namespace
