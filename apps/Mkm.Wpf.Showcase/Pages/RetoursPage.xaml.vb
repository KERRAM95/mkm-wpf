Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' La page des retours : les notifications vont au Toast de la fenêtre principale ; le dialogue de renommage
    ''' s'ouvre modal et rend sa réponse.
    ''' </summary>
    Partial Public Class RetoursPage

        Public Sub New()
            InitializeComponent()
        End Sub

        Private ReadOnly Property Notifications As Toast
            Get
                Return DirectCast(Window.GetWindow(Me), MainWindow).Notifications
            End Get
        End Property

        Private Sub Remontrer_Click(sender As Object, e As RoutedEventArgs)
            Ephemere.Visibility = Visibility.Visible
            ' Une vie neuve : la fixer de nouveau relance son compte.
            Ephemere.Life = TimeSpan.Zero
            Ephemere.Life = TimeSpan.FromSeconds(5)
        End Sub

        Private Sub Notifier_Click(sender As Object, e As RoutedEventArgs)
            Dim gravite = DirectCast([Enum].Parse(GetType(MessageSeverity), CStr(DirectCast(sender, FrameworkElement).Tag)), MessageSeverity)
            Dim detail = Localizer.Current.T($"showcase.feedback.{gravite.ToString().ToLowerInvariant()}")
            Notifications.Add(gravite, gravite.ToString(), detail, TimeSpan.FromSeconds(5))
        End Sub

        Private Sub Durable_Click(sender As Object, e As RoutedEventArgs)
            Notifications.Add(MessageSeverity.Info, Localizer.Current.T("showcase.feedback.saved"), Localizer.Current.T("showcase.feedback.success"))
        End Sub

        Private Sub ToutFermer_Click(sender As Object, e As RoutedEventArgs)
            Notifications.RemoveAll()
        End Sub

        ' Un dialogue modal : un champ, Annuler (Échap) et Renommer (Entrée).
        Private Sub Renommer_Click(sender As Object, e As RoutedEventArgs)
            Dim nom As New InputText With {.Text = "HS-00042", .Width = 280}
            Dim valider As New Mkm.Wpf.Controls.Button With {.Content = Localizer.Current.T("showcase.feedback.ok"), .IsDefault = True}
            Dim pied As New StackPanel With {.Orientation = Orientation.Horizontal}
            pied.Children.Add(New Mkm.Wpf.Controls.Button With {
                .Content = Localizer.Current.T("showcase.feedback.cancel"), .Severity = ButtonSeverity.Secondary, .IsCancel = True})
            pied.Children.Add(valider)

            Dim contenu As New StackPanel()
            contenu.Children.Add(New TextBlock With {.Text = Localizer.Current.T("showcase.feedback.newName"), .Margin = New Thickness(0, 0, 0, 4)})
            contenu.Children.Add(nom)

            Dim dialogue As New Dialog With {
                .Title = Localizer.Current.T("showcase.feedback.rename"),
                .Owner = Window.GetWindow(Me),
                .Content = contenu,
                .Footer = pied
            }
            AddHandler valider.Click, Sub() dialogue.DialogResult = True
            AddHandler dialogue.Loaded, Sub()
                                            nom.Focus()
                                            nom.SelectAll()
                                        End Sub

            Resultat.Text = If(dialogue.ShowDialog() = True,
                               Localizer.Current.T("showcase.feedback.renamed", nom.Text),
                               Localizer.Current.T("showcase.feedback.cancelled"))
        End Sub

        ' Un dialogue qu'on étire par son coin, au titre trop long pour sa barre.
        Private Sub Etirable_Click(sender As Object, e As RoutedEventArgs)
            Dim fermer As New Mkm.Wpf.Controls.Button With {.Content = Localizer.Current.T("showcase.feedback.close"), .IsCancel = True}
            Dim dialogue As New Dialog With {
                .Title = Localizer.Current.T("showcase.feedback.longTitle"),
                .Owner = Window.GetWindow(Me),
                .ResizeMode = ResizeMode.CanResize,
                .Width = 360,
                .Height = 240,
                .SizeToContent = SizeToContent.Manual,
                .Content = New TextBlock With {.Text = Localizer.Current.T("showcase.containers.titleBlockText"), .TextWrapping = TextWrapping.Wrap},
                .Footer = fermer
            }
            dialogue.ShowDialog()
        End Sub

    End Class

End Namespace
