Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le fil d'Ariane : chaque lien suivi s'inscrit au journal.</summary>
    Friend NotInheritable Class BreadcrumbJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim chemin = DirectCast(exemple.FindName("Chemin"), Breadcrumb)
            chemin.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Chemin_Click))
        End Sub

#Region "Exemple"
        ' Le clic d'un lien remonte au fil d'Ariane : l'application ouvre sa page.
        Private Shared Sub Chemin_Click(sender As Object, e As RoutedEventArgs)
            Dim lien = TryCast(e.OriginalSource, BreadcrumbItem)
            If lien Is Nothing Then Return
            Dim journal = DirectCast(lien.FindName("Journal"), TextBlock)
            journal.Text = $"Lien suivi : {If(lien.Content, lien.ToolTip)}"
        End Sub
#End Region

    End Class

End Namespace
