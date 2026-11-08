Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le menu contextuel de l'exemple : chaque élément choisi s'écrit dans le journal.</summary>
    Friend NotInheritable Class ContextMenuJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim zone = DirectCast(exemple.FindName("Zone"), FrameworkElement)
            Dim journal = DirectCast(exemple.FindName("Journal"), TextBlock)
            zone.ContextMenu.AddHandler(MenuItem.ClickEvent, New RoutedEventHandler(
                Sub(sender As Object, e As RoutedEventArgs) Noter(journal, e)))
        End Sub

#Region "Exemple"
        ' Un menu contextuel n'est pas dans l'arbre de la page : son MenuItem.Click s'arrête au menu. Le gestionnaire se
        ' pose donc sur le menu lui-même (ou chaque élément a sa Command).
        Private Shared Sub Noter(journal As TextBlock, e As RoutedEventArgs)
            Dim element = TryCast(e.OriginalSource, MenuItem)
            If element Is Nothing OrElse element.HasItems Then Return
            journal.Text = $"Choisi : {element.Header}"
        End Sub
#End Region

    End Class

End Namespace
