Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le nœud choisi dans l'arbre de l'exemple s'écrit dans le texte « Choix ».</summary>
    Friend NotInheritable Class NoeudChoisi

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim arbre = DirectCast(exemple.FindName("Notes"), Tree)
            Dim choix = DirectCast(exemple.FindName("Choix"), TextBlock)
            AddHandler arbre.SelectedItemChanged, Sub(s, e) Montrer(arbre, choix)
        End Sub

#Region "Exemple"
        ' SelectedItem est le nœud choisi (un TreeNode, ou l'objet de données d'un arbre lié par ItemsSource).
        Private Shared Sub Montrer(arbre As Tree, choix As TextBlock)
            Dim noeud = TryCast(arbre.SelectedItem, TreeNode)
            choix.Text = If(noeud Is Nothing, "Aucun nœud choisi", $"Nœud choisi : {noeud.Header}")
        End Sub
#End Region

    End Class

End Namespace
