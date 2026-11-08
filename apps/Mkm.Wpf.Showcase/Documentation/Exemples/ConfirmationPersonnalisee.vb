Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le bouton « Supprimer » ouvre une demande aux boutons renommés ; la réponse s'écrit dans « ReponseSuppression ».</summary>
    Friend NotInheritable Class ConfirmationPersonnalisee

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim reponse = DirectCast(exemple.FindName("ReponseSuppression"), TextBlock)
            AddHandler DirectCast(exemple.FindName("Supprimer"), Mkm.Wpf.Controls.Button).Click,
                Sub(s, e) Demander(DirectCast(s, FrameworkElement), reponse)
        End Sub

#Region "Exemple"
        ' Les libellés et les gravités des deux boutons se choisissent ; ShowDialog rend la réponse.
        Private Shared Sub Demander(bouton As FrameworkElement, reponse As TextBlock)
            Dim demande As New ConfirmDialog With {
                .Title = "Supprimer",
                .Message = "Supprimer la note « Cordon continu » ? Elle ne pourra pas être rétablie.",
                .MessageIcon = Icons.Delete,
                .AcceptLabel = "Supprimer",
                .AcceptSeverity = ButtonSeverity.Danger,
                .RejectLabel = "Garder",
                .RejectSeverity = ButtonSeverity.Secondary,
                .Owner = Window.GetWindow(bouton)
            }
            reponse.Text = If(demande.ShowDialog() = True, "Supprimée", "Gardée")
        End Sub
#End Region

    End Class

End Namespace
