Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le dossier choisi dans le TreeSelect de l'exemple s'écrit dans le texte « Choix ».</summary>
    Friend NotInheritable Class TreeSelectChoix

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim champ = DirectCast(exemple.FindName("Dossier"), TreeSelect)
            Dim choix = DirectCast(exemple.FindName("Choix"), TextBlock)
            AddHandler champ.SelectedItemChanged, Sub(s, e) Montrer(champ, choix)
        End Sub

#Region "Exemple"
        ' SelectedItem est le nœud choisi (un TreeNode, ou l'objet de données d'un arbre lié par ItemsSource).
        Private Shared Sub Montrer(champ As TreeSelect, choix As TextBlock)
            Dim noeud = TryCast(champ.SelectedItem, TreeNode)
            choix.Text = If(noeud Is Nothing, "Aucun dossier choisi", $"Dossier choisi : {noeud.Header}")
        End Sub
#End Region

    End Class

End Namespace
