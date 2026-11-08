Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le Popover de partage : le bouton l'ouvre, « Copier le lien » le ferme et le dit au journal.</summary>
    Friend NotInheritable Class PopoverPartager

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            AddHandler DirectCast(exemple.FindName("Partager"), Mkm.Wpf.Controls.Button).Click, AddressOf Partager_Click
            AddHandler DirectCast(exemple.FindName("Copier"), Mkm.Wpf.Controls.Button).Click, AddressOf Copier_Click
        End Sub

#Region "Exemple"
        ' Le bouton ouvre la fenêtre, ou la ferme si elle est ouverte.
        Private Shared Sub Partager_Click(sender As Object, e As RoutedEventArgs)
            Dim partage = DirectCast(DirectCast(sender, FrameworkElement).FindName("Partage"), Popover)
            partage.Toggle()
        End Sub

        ' Le lien copié, la fenêtre se ferme.
        Private Shared Sub Copier_Click(sender As Object, e As RoutedEventArgs)
            Dim copier = DirectCast(sender, FrameworkElement)
            Try
                Clipboard.SetText("https://plm.exemple.fr/HS-00042")
            Catch ex As Runtime.InteropServices.COMException
                ' Le presse-papiers tenu par une autre application : il se réessaie au clic suivant.
            End Try
            DirectCast(copier.FindName("Partage"), Popover).Hide()
            DirectCast(copier.FindName("Journal"), TextBlock).Text = "Lien copié."
        End Sub
#End Region

    End Class

End Namespace
