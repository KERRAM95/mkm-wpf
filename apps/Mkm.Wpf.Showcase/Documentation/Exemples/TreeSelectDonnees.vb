Imports System.Collections.ObjectModel
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le TreeSelect « NotesLiees » de l'exemple reçoit ses nœuds de données (les notes de l'arbre lié).</summary>
    Friend NotInheritable Class TreeSelectDonnees

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Remplir(DirectCast(exemple.FindName("NotesLiees"), TreeSelect))
        End Sub

#Region "Exemple"
        ' Un nœud par objet : le HierarchicalDataTemplate dit son titre et où sont ses enfants ; DisplayMemberPath, ce
        ' que le champ écrit de l'objet choisi.
        Private Shared Sub Remplir(champ As TreeSelect)
            Dim generales As New NoteDePlan With {.Titre = "Notes générales"}
            generales.Enfants.Add(New NoteDePlan With {.Titre = "Casser les angles vifs"})
            generales.Enfants.Add(New NoteDePlan With {.Titre = "Tolérances ISO 2768-mK"})
            Dim soudure As New NoteDePlan With {.Titre = "Notes de soudure"}
            soudure.Enfants.Add(New NoteDePlan With {.Titre = "Cordon continu"})
            champ.ItemsSource = New ObservableCollection(Of NoteDePlan) From {generales, soudure}
        End Sub
#End Region

    End Class

End Namespace
