Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>La pagination : chaque page montrée s'inscrit au journal, avec ses lignes.</summary>
    Friend NotInheritable Class PaginatorJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            AddHandler DirectCast(exemple.FindName("Pages"), Paginator).PageChange, AddressOf Pages_PageChange
        End Sub

#Region "Exemple"
        ' L'application montre les lignes de First à First + Rows.
        Private Shared Sub Pages_PageChange(sender As Object, e As PageChangeEventArgs)
            Dim journal = DirectCast(DirectCast(sender, FrameworkElement).FindName("Journal"), TextBlock)
            journal.Text = $"Page {e.Page + 1} : lignes {e.First + 1} à {e.First + e.Rows}"
        End Sub
#End Region

    End Class

End Namespace
