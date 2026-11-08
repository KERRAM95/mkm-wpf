Imports System
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>La barre qui avance : son bouton l'avance d'un quart, et son texte suit.</summary>
    Friend NotInheritable Class ProgressBarAvancer

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            AddHandler DirectCast(exemple.FindName("Avancer"), Mkm.Wpf.Controls.Button).Click, AddressOf Avancer_Click
        End Sub

#Region "Exemple"
        Private Shared Sub Avancer_Click(sender As Object, e As RoutedEventArgs)
            Dim bouton = DirectCast(sender, FrameworkElement)
            Dim barre = DirectCast(bouton.FindName("Avancement"), Mkm.Wpf.Controls.ProgressBar)
            Dim pourcentage = DirectCast(bouton.FindName("Pourcentage"), TextBlock)
            barre.Value = If(barre.Value >= 100, 0, Math.Min(100, barre.Value + 25))
            pourcentage.Text = $"{barre.Value:0} %"
        End Sub
#End Region

    End Class

End Namespace
