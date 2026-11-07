Imports System.Windows
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La place d'une info-bulle de mkm : à droite de son élément, centrée sur sa hauteur, à l'écart que donne
    ''' son <c>HorizontalOffset</c> (aucun par défaut : la flèche de la bulle fait l'écart) ; à gauche, si la droite de l'écran
    ''' manque de place.
    ''' </summary>
    ''' <remarks>Le style des info-bulles (<c>Themes/Natifs.xaml</c>) la pose sur toute info-bulle de WPF.</remarks>
    Public NotInheritable Class TooltipPlacement

        Private Sub New()
        End Sub

        ''' <summary>À droite de l'élément, centrée ; à gauche en second choix.</summary>
        Public Shared ReadOnly Property Right As New CustomPopupPlacementCallback(AddressOf ADroite)

        Private Shared Function ADroite(popupSize As Size, targetSize As Size, offset As Point) As CustomPopupPlacement()
            Dim y = (targetSize.Height - popupSize.Height) / 2
            Return {
                New CustomPopupPlacement(New Point(targetSize.Width + offset.X, y), PopupPrimaryAxis.Horizontal),
                New CustomPopupPlacement(New Point(-popupSize.Width - offset.X, y), PopupPrimaryAxis.Horizontal)
            }
        End Function

    End Class

End Namespace
