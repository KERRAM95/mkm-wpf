Imports System.Collections.ObjectModel
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Un lieu (un site, un atelier) et ceux qui le suivent : les données du CascadeSelect lié.</summary>
    Public NotInheritable Class Lieu
        Public Property Nom As String
        Public Property Suite As New ObservableCollection(Of Lieu)

        ''' <summary>Le nom de l'option pour les lecteurs d'écran : sans lui, Narrateur lirait le nom de la classe.</summary>
        Public Overrides Function ToString() As String
            Return Nom
        End Function

    End Class

    ''' <summary>Le CascadeSelect « Sites » de l'exemple reçoit ses options de données.</summary>
    Friend NotInheritable Class CascadeSelectDonnees

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Remplir(DirectCast(exemple.FindName("Sites"), CascadeSelect))
        End Sub

#Region "Exemple"
        ' Une option par objet : le HierarchicalDataTemplate dit son texte et où sont les options qui la suivent.
        Private Shared Sub Remplir(champ As CascadeSelect)
            Dim lyon As New Lieu With {.Nom = "Lyon"}
            lyon.Suite.Add(New Lieu With {.Nom = "Usinage"})
            lyon.Suite.Add(New Lieu With {.Nom = "Montage"})
            Dim toulouse As New Lieu With {.Nom = "Toulouse"}
            toulouse.Suite.Add(New Lieu With {.Nom = "Peinture"})
            champ.ItemsSource = New ObservableCollection(Of Lieu) From {lyon, toulouse}
        End Sub
#End Region

    End Class

End Namespace
