Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le bouton partagé : l'action principale et celles du menu s'inscrivent au journal.</summary>
    Friend NotInheritable Class SplitButtonJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim partage = DirectCast(exemple.FindName("Enregistrer"), SplitButton)
            AddHandler partage.Click, AddressOf Enregistrer_Click
            partage.AddHandler(MenuItem.ClickEvent, New RoutedEventHandler(AddressOf Action_Click))
        End Sub

#Region "Exemple"
        ' Le bouton principal.
        Private Shared Sub Enregistrer_Click(sender As Object, e As RoutedEventArgs)
            Ecrire(sender, "Action : Enregistrer")
        End Sub

        ' Une action du menu : son Click remonte au bouton partagé.
        Private Shared Sub Action_Click(sender As Object, e As RoutedEventArgs)
            Ecrire(sender, $"Action : {DirectCast(e.OriginalSource, MenuItem).Header}")
        End Sub

        Private Shared Sub Ecrire(sender As Object, texte As String)
            DirectCast(DirectCast(sender, FrameworkElement).FindName("Journal"), TextBlock).Text = texte
        End Sub
#End Region

    End Class

End Namespace
