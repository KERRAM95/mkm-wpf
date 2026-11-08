Imports System
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>L'exemple de base du Toast : son bouton ajoute une notification au Toast de la fenêtre.</summary>
    Friend NotInheritable Class ToastBasique

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim bouton = DirectCast(exemple.FindName("Notifier"), Mkm.Wpf.Controls.Button)
            AddHandler bouton.Click, AddressOf Notifier_Click
        End Sub

#Region "Exemple"
        ' Le Toast est posé une fois dans la fenêtre : <mkm:Toast x:Name="Notifications" />.
        Private Shared Sub Notifier_Click(sender As Object, e As RoutedEventArgs)
            Dim notifications = DirectCast(Window.GetWindow(DirectCast(sender, DependencyObject)).FindName("Notifications"), Toast)
            notifications.Add(MessageSeverity.Info, "Plan ouvert", "Le plan HS-00042 est ouvert en lecture seule.", TimeSpan.FromSeconds(5))
        End Sub
#End Region

    End Class

End Namespace
