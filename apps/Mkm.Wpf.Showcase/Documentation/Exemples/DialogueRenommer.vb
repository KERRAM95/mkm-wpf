Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le bouton « Renommer » ouvre un dialogue modal ; sa réponse s'écrit dans le texte « Resultat ».</summary>
    Friend NotInheritable Class DialogueRenommer

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim resultat = DirectCast(exemple.FindName("Resultat"), TextBlock)
            AddHandler DirectCast(exemple.FindName("Renommer"), Mkm.Wpf.Controls.Button).Click,
                Sub(s, e) Renommer(DirectCast(s, FrameworkElement), resultat)
        End Sub

#Region "Exemple"
        ' Un dialogue modal : un champ, Annuler (Échap) et Renommer (Entrée) dans son pied.
        Private Shared Sub Renommer(bouton As FrameworkElement, resultat As TextBlock)
            Dim nom As New InputText With {.Text = "HS-00042", .Width = 280}
            Dim valider As New Mkm.Wpf.Controls.Button With {.Content = "Renommer", .IsDefault = True}
            Dim pied As New StackPanel With {.Orientation = Orientation.Horizontal}
            pied.Children.Add(New Mkm.Wpf.Controls.Button With {.Content = "Annuler", .Severity = ButtonSeverity.Secondary, .IsCancel = True})
            pied.Children.Add(valider)

            Dim contenu As New StackPanel()
            contenu.Children.Add(New TextBlock With {.Text = "Nouveau nom", .Margin = New Thickness(0, 0, 0, 4)})
            contenu.Children.Add(nom)

            Dim dialogue As New Dialog With {
                .Title = "Renommer le plan",
                .Owner = Window.GetWindow(bouton),
                .Content = contenu,
                .Footer = pied
            }
            AddHandler valider.Click, Sub() dialogue.DialogResult = True
            AddHandler dialogue.Loaded, Sub()
                                            nom.Focus()
                                            nom.SelectAll()
                                        End Sub
            resultat.Text = If(dialogue.ShowDialog() = True, $"Renommé : {nom.Text}", "Annulé")
        End Sub
#End Region

    End Class

End Namespace
