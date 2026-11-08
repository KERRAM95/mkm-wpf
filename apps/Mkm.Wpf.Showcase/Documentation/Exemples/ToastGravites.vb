Imports System
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Les gravités du Toast : chaque bouton ajoute une notification de la gravité écrite dans son Tag.</summary>
    Friend NotInheritable Class ToastGravites

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            For Each bouton In LogicalTreeHelper.GetChildren(exemple).OfType(Of Mkm.Wpf.Controls.Button)()
                AddHandler bouton.Click, AddressOf Notifier_Click
            Next
        End Sub

#Region "Exemple"
        Private Shared Sub Notifier_Click(sender As Object, e As RoutedEventArgs)
            Dim bouton = DirectCast(sender, Mkm.Wpf.Controls.Button)
            Dim gravite = DirectCast([Enum].Parse(GetType(MessageSeverity), CStr(bouton.Tag)), MessageSeverity)
            Dim notifications = DirectCast(Window.GetWindow(bouton).FindName("Notifications"), Toast)
            Select Case gravite
                Case MessageSeverity.Success
                    notifications.Add(gravite, "Enregistré", "Le cartouche est à jour.", TimeSpan.FromSeconds(5))
                Case MessageSeverity.Warn
                    notifications.Add(gravite, "Attention", "L'échelle diffère de celle de la feuille.", TimeSpan.FromSeconds(5))
                Case MessageSeverity.Error
                    notifications.Add(gravite, "Erreur", "Le plan n'a pas pu être enregistré.", TimeSpan.FromSeconds(5))
                Case Else
                    notifications.Add(gravite, "Information", "Le plan est ouvert en lecture seule.", TimeSpan.FromSeconds(5))
            End Select
        End Sub
#End Region

    End Class

End Namespace
