Imports System.Collections.ObjectModel
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Les listes de l'exemple reçoivent leurs éléments : l'OrderList « Etapes », le PickList « Diffusion ».</summary>
    Friend NotInheritable Class ListesDEtapes

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim etapes = TryCast(exemple.FindName("Etapes"), OrderList)
            If etapes IsNot Nothing Then Ordonner(etapes)
            Dim diffusion = TryCast(exemple.FindName("Diffusion"), PickList)
            If diffusion IsNot Nothing Then Repartir(diffusion)
        End Sub

#Region "Exemple"
        ' Des ObservableCollection : les boutons les changent en place, et la page suit.
        Private Shared Sub Ordonner(etapes As OrderList)
            etapes.ItemsSource = New ObservableCollection(Of String) From {
                "Créer le plan", "Coter", "Vérifier le cartouche", "Faire valider", "Diffuser"}
        End Sub

        Private Shared Sub Repartir(diffusion As PickList)
            diffusion.Source = New ObservableCollection(Of String) From {"Achats", "Méthodes", "Qualité", "Production", "Client"}
            diffusion.Target = New ObservableCollection(Of String) From {"Bureau d'études"}
        End Sub
#End Region

    End Class

End Namespace
