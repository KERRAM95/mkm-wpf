Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Une notification sans durée de vie, et la commande qui les ferme toutes.</summary>
    Friend NotInheritable Class ToastDurable

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            AddHandler DirectCast(exemple.FindName("Durable"), Mkm.Wpf.Controls.Button).Click, AddressOf Durable_Click
            AddHandler DirectCast(exemple.FindName("ToutFermer"), Mkm.Wpf.Controls.Button).Click, AddressOf ToutFermer_Click
        End Sub

#Region "Exemple"
        ' Sans durée de vie, la notification reste jusqu'à ce qu'on la ferme.
        Private Shared Sub Durable_Click(sender As Object, e As RoutedEventArgs)
            Notifications(sender).Add(MessageSeverity.Info, "Synchronisation", "La synchronisation avec 3DEXPERIENCE est en cours.")
        End Sub

        Private Shared Sub ToutFermer_Click(sender As Object, e As RoutedEventArgs)
            Notifications(sender).RemoveAll()
        End Sub

        Private Shared Function Notifications(sender As Object) As Toast
            Return DirectCast(Window.GetWindow(DirectCast(sender, DependencyObject)).FindName("Notifications"), Toast)
        End Function
#End Region

    End Class

End Namespace
