Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>La fiche de l'exemple : Enregistrer la remet en lecture, et le journal le dit.</summary>
    Friend NotInheritable Class FicheEnregistrer

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim fiche = DirectCast(exemple.FindName("Fiche"), PropertyPanel)
            AddHandler fiche.Save, AddressOf Fiche_Save
            AddHandler fiche.Cancel, AddressOf Fiche_Cancel
        End Sub

#Region "Exemple"
        ' Enregistrer : l'application enregistre, puis remet la fiche en lecture (Editing à faux).
        Private Shared Sub Fiche_Save(sender As Object, e As RoutedEventArgs)
            Dim fiche = DirectCast(sender, PropertyPanel)
            fiche.Editing = False
            Journal(fiche).Text = "Fiche enregistrée."
        End Sub

        ' Annuler : la fiche est déjà revenue en lecture.
        Private Shared Sub Fiche_Cancel(sender As Object, e As RoutedEventArgs)
            Journal(DirectCast(sender, PropertyPanel)).Text = "Modification annulée."
        End Sub

        Private Shared Function Journal(fiche As PropertyPanel) As TextBlock
            Return DirectCast(fiche.FindName("Journal"), TextBlock)
        End Function
#End Region

    End Class

End Namespace
