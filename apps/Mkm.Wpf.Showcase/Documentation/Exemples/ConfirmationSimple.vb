Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le bouton « Demander » pose une question ; la réponse s'écrit dans le texte « Reponse ».</summary>
    Friend NotInheritable Class ConfirmationSimple

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim reponse = DirectCast(exemple.FindName("Reponse"), TextBlock)
            AddHandler DirectCast(exemple.FindName("Demander"), Mkm.Wpf.Controls.Button).Click,
                Sub(s, e) Demander(DirectCast(s, FrameworkElement), reponse)
        End Sub

#Region "Exemple"
        ' Confirm ouvre la demande, modale, et rend vrai si l'utilisateur accepte (Oui ou Entrée).
        Private Shared Sub Demander(bouton As FrameworkElement, reponse As TextBlock)
            Dim oui = ConfirmDialog.Confirm(Window.GetWindow(bouton),
                                            "Mettre à jour le cartouche du plan avec les valeurs de 3DEXPERIENCE ?",
                                            "Mise à jour", Icons.Warning)
            reponse.Text = If(oui, "Accepté", "Refusé")
        End Sub
#End Region

    End Class

End Namespace
