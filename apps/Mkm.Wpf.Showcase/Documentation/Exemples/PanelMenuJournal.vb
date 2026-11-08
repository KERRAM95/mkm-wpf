Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>L'élément lancé dans le PanelMenu de l'exemple s'écrit dans le texte « Journal ».</summary>
    Friend NotInheritable Class PanelMenuJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            exemple.AddHandler(PanelMenuItem.ClickEvent, New RoutedEventHandler(AddressOf Element_Click))
        End Sub

#Region "Exemple"
        ' PanelMenuItem.Click remonte de l'élément lancé : un seul gestionnaire, posé sur le menu, les suit tous.
        Private Shared Sub Element_Click(sender As Object, e As RoutedEventArgs)
            Dim element = DirectCast(e.OriginalSource, PanelMenuItem)
            Dim journal = DirectCast(DirectCast(sender, FrameworkElement).FindName("Journal"), TextBlock)
            journal.Text = $"Lancé : {element.Header}"
        End Sub
#End Region

    End Class

End Namespace
