Imports System.Linq
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>La longue liste du Select : 5 000 références, dont seules les lignes visibles existent.</summary>
    Friend NotInheritable Class SelectLongueListe

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Remplir(DirectCast(exemple.FindName("Longue"), [Select]))
        End Sub

#Region "Exemple"
        ' La liste est virtualisée : elle ne crée que les lignes qu'on voit, et s'ouvre aussitôt.
        Private Shared Sub Remplir(liste As [Select])
            liste.ItemsSource = Enumerable.Range(1, 5000).Select(Function(i) $"HS-{i:00000}").ToList()
        End Sub
#End Region

    End Class

End Namespace
