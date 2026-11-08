Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Les menus de l'exemple : chaque élément choisi s'écrit dans le journal.</summary>
    Friend NotInheritable Class MenuJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            exemple.AddHandler(MenuItem.ClickEvent, New RoutedEventHandler(AddressOf Element_Click))
        End Sub

#Region "Exemple"
        ' MenuItem.Click remonte du menu choisi : un seul gestionnaire, posé sur le conteneur des menus, les suit tous.
        Private Shared Sub Element_Click(sender As Object, e As RoutedEventArgs)
            Dim element = TryCast(e.OriginalSource, MenuItem)
            If element Is Nothing OrElse element.HasItems Then Return
            Dim journal = DirectCast(DirectCast(sender, FrameworkElement).FindName("Journal"), TextBlock)
            journal.Text = $"Choisi : {element.Header}"
        End Sub
#End Region

    End Class

End Namespace
