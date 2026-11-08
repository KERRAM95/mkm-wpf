Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le bouton « Etirable » ouvre un dialogue qu'on étire par son coin.</summary>
    Friend NotInheritable Class DialogueEtirable

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            AddHandler DirectCast(exemple.FindName("Etirable"), Mkm.Wpf.Controls.Button).Click,
                Sub(s, e) Ouvrir(DirectCast(s, FrameworkElement))
        End Sub

#Region "Exemple"
        ' ResizeMode="CanResize" donne la poignée du coin ; SizeToContent="Manual" garde la taille donnée.
        Private Shared Sub Ouvrir(bouton As FrameworkElement)
            Dim dialogue As New Dialog With {
                .Title = "Propriétés du plan HS-00042 — feuille 1 sur 3, révision B, en cours",
                .Owner = Window.GetWindow(bouton),
                .ResizeMode = ResizeMode.CanResize,
                .SizeToContent = SizeToContent.Manual,
                .Width = 360,
                .Height = 240,
                .Content = New TextBlock With {.Text = "Le cartouche reprend la référence, la désignation, l'échelle et la révision du plan.",
                                               .TextWrapping = TextWrapping.Wrap},
                .Footer = New Mkm.Wpf.Controls.Button With {.Content = "Fermer", .IsCancel = True}
            }
            dialogue.ShowDialog()
        End Sub
#End Region

    End Class

End Namespace
