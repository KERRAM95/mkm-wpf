Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>L'élément choisi dans le Dock de l'exemple s'écrit dans le texte « Journal ».</summary>
    Friend NotInheritable Class DockJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            exemple.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Element_Click))
        End Sub

#Region "Exemple"
        ' Le Click de chaque élément remonte : un seul gestionnaire, posé sur le conteneur, les suit tous.
        Private Shared Sub Element_Click(sender As Object, e As RoutedEventArgs)
            Dim element = TryCast(e.OriginalSource, DockItem)
            If element Is Nothing Then Return
            Dim journal = DirectCast(DirectCast(sender, FrameworkElement).FindName("Journal"), TextBlock)
            journal.Text = $"Lancé : {element.Label}"
        End Sub
#End Region

    End Class

End Namespace
