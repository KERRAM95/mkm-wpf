Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>La carte de l'exemple : sa croix lève Close, que l'application traite (ici, elle cache la carte).</summary>
    Friend NotInheritable Class IdCardFermer

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim carte = DirectCast(exemple.FindName("Carte"), IdCard)
            AddHandler carte.Close, AddressOf Carte_Close
        End Sub

#Region "Exemple"
        ' La carte ne se retire pas d'elle-même : l'application décide (fermer l'objet, le retirer de la liste).
        Private Shared Sub Carte_Close(sender As Object, e As RoutedEventArgs)
            Dim carte = DirectCast(sender, IdCard)
            carte.Visibility = Visibility.Collapsed
            Dim journal = DirectCast(carte.FindName("Journal"), TextBlock)
            journal.Text = $"Carte fermée : {carte.Title}"
        End Sub
#End Region

    End Class

End Namespace
