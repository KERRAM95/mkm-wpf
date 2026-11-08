Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Peint une partie d'Avatar ou de Chip (un rond, un carré) de leur image. Posé par le code et non lié dans le
    ''' gabarit : un ImageBrush lié dans un ControlTemplate n'a pas toujours de contexte au moment de sa liaison, et WPF
    ''' le signale (« Cannot find governing FrameworkElement »).
    ''' </summary>
    Friend NotInheritable Class PinceauDImage

        Private Sub New()
        End Sub

        ''' <summary>Peint <paramref name="partie"/> (une forme ou une bordure) de <paramref name="image"/>, ou l'efface.</summary>
        Friend Shared Sub Poser(partie As DependencyObject, image As ImageSource)
            Dim pinceau As Brush = Nothing
            If image IsNot Nothing Then
                Dim photo As New ImageBrush(image) With {.Stretch = Stretch.UniformToFill}
                If photo.CanFreeze Then photo.Freeze()
                pinceau = photo
            End If
            Dim forme = TryCast(partie, Shapes.Shape)
            If forme IsNot Nothing Then forme.Fill = pinceau
            Dim bordure = TryCast(partie, Border)
            If bordure IsNot Nothing Then bordure.Background = pinceau
        End Sub

    End Class

End Namespace
