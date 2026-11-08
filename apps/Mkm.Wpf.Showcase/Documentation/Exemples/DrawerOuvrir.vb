Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Les boutons du tiroir : chacun l'ouvre à son bord.</summary>
    Friend NotInheritable Class DrawerOuvrir

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            exemple.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Bouton_Click))
        End Sub

#Region "Exemple"
        ' Le bouton dit le bord (son Tag) ; le tiroir sort de là.
        Private Shared Sub Bouton_Click(sender As Object, e As RoutedEventArgs)
            Dim bouton = TryCast(e.OriginalSource, Mkm.Wpf.Controls.Button)
            Dim bord As DrawerPosition
            If bouton Is Nothing OrElse Not [Enum].TryParse(CStr(bouton.Tag), bord) Then Return
            Dim tiroir = DirectCast(bouton.FindName("Tiroir"), Drawer)
            tiroir.Position = bord
            tiroir.Show()
        End Sub
#End Region

    End Class

End Namespace
