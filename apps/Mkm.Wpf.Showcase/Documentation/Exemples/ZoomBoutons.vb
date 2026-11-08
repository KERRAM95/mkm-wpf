Imports System.Globalization
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Les boutons du zoom : agrandir, réduire, revenir à 100 % ; le zoom actuel au journal, tant qu'il est affiché.</summary>
    Friend NotInheritable Class ZoomBoutons

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            AddHandler DirectCast(exemple.FindName("Agrandir"), Mkm.Wpf.Controls.Button).Click, AddressOf Agrandir_Click
            AddHandler DirectCast(exemple.FindName("Reduire"), Mkm.Wpf.Controls.Button).Click, AddressOf Reduire_Click
            AddHandler DirectCast(exemple.FindName("Retablir"), Mkm.Wpf.Controls.Button).Click, AddressOf Retablir_Click
            Dim journal = DirectCast(exemple.FindName("Journal"), TextBlock)
            Dim suivre As EventHandler = Sub(s, e) journal.Text = "Zoom : " & MkmResources.Current.Zoom.ToString("P0", CultureInfo.GetCultureInfo("fr-FR"))
            suivre(Nothing, EventArgs.Empty)
            AddHandler journal.Loaded, Sub() AddHandler MkmResources.Current.ZoomChanged, suivre
            AddHandler journal.Unloaded, Sub() RemoveHandler MkmResources.Current.ZoomChanged, suivre
        End Sub

#Region "Exemple"
        ' Le palier suivant : toutes les fenêtres ouvertes suivent.
        Private Shared Sub Agrandir_Click(sender As Object, e As RoutedEventArgs)
            MkmResources.Current.ZoomIn()
        End Sub

        Private Shared Sub Reduire_Click(sender As Object, e As RoutedEventArgs)
            MkmResources.Current.ZoomOut()
        End Sub

        Private Shared Sub Retablir_Click(sender As Object, e As RoutedEventArgs)
            MkmResources.Current.ResetZoom()
        End Sub
#End Region

    End Class

End Namespace
