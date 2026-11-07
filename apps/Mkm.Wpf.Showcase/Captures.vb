Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Threading
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Localization
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' Les captures de la vitrine (<c>Mkm.Wpf.Showcase.exe --captures &lt;dossier&gt;</c>) : chaque page dans
    ''' chaque thème, en entier, puis la fenêtre, des notifications et un dialogue, en PNG à leur taille
    ''' réelle (96 ppp). La CI Windows les publie : on voit le rendu sans poste Windows.
    ''' </summary>
    Friend NotInheritable Class Captures

        Private Const Largeur As Double = 880

        Private ReadOnly _dossier As String
        Private ReadOnly _journal As New List(Of String)
        Private _erreurs As Integer

        Private Sub New(dossier As String)
            _dossier = dossier
        End Sub

        ''' <summary>Fait toutes les captures dans <paramref name="dossier"/> ; rend le nombre d'erreurs.</summary>
        Public Shared Function Executer(dossier As String) As Integer
            Dim c As New Captures(dossier)
            Directory.CreateDirectory(dossier)
            Try
                c.Tout()
            Catch ex As Exception
                c.Noter($"ERREUR {ex}")
            End Try
            File.WriteAllLines(Path.Combine(dossier, "journal.txt"), c._journal)
            Return c._erreurs
        End Function

        Private Sub Tout()
            Localizer.Current.Language = CultureInfo.GetCultureInfo("fr")
            Dim pages As New List(Of KeyValuePair(Of String, Func(Of FrameworkElement))) From {
                Page("1-actionbar", Function() New ActionBarPage()),
                Page("2-champs", Function() New ChampsPage()),
                Page("3-boutons", Function() New BoutonsPage()),
                Page("4-conteneurs", Function() New ConteneursPage()),
                Page("5-retours", Function() New RetoursPage())
            }

            For Each theme In Themes.All
                MkmResources.Current.Theme = theme.Name
                For Each p In pages
                    Essayer($"{theme.Name}/{p.Key}", Sub() Enregistrer(Seule(p.Value.Invoke()), $"{theme.Name}/{p.Key}.png"))
                Next
                Essayer($"{theme.Name}/fenetre", Sub() Fenetre(theme.Name))
            Next
        End Sub

        Private Shared Function Page(nom As String, fabrique As Func(Of FrameworkElement)) As KeyValuePair(Of String, Func(Of FrameworkElement))
            Return New KeyValuePair(Of String, Func(Of FrameworkElement))(nom, fabrique)
        End Function

        ' Une page hors de toute fenêtre, à la largeur de la vitrine et à toute sa hauteur, sur le fond du thème.
        Private Shared Function Seule(element As FrameworkElement) As FrameworkElement
            Dim fond As New Border With {.Width = Largeur, .Child = element}
            fond.SetResourceReference(Border.BackgroundProperty, ThemeResources.Prefix & "content.background")
            fond.SetResourceReference(TextElement.ForegroundProperty, ThemeResources.Prefix & "text.color")
            fond.SetResourceReference(TextElement.FontFamilyProperty, ThemeResources.Prefix & "font.family")
            fond.SetResourceReference(TextElement.FontSizeProperty, ThemeResources.Prefix & "font.size")
            fond.Measure(New Size(Largeur, Double.PositiveInfinity))
            fond.Arrange(New Rect(fond.DesiredSize))
            fond.UpdateLayout()
            Return fond
        End Function

        ' La fenêtre ouverte : chaque page dedans, puis des notifications et un dialogue sur la dernière.
        Private Sub Fenetre(theme As String)
            Dim fenetre As New MainWindow With {.WindowStartupLocation = WindowStartupLocation.Manual, .Left = 0, .Top = 0, .ShowActivated = False}
            fenetre.Show()
            Try
                For i = 0 To fenetre.Composants.Items.Count - 1
                    fenetre.Composants.SelectedIndex = i
                    Attendre(300)
                    Enregistrer(DirectCast(fenetre.Content, FrameworkElement), $"{theme}/fenetre-{i + 1}.png")
                Next

                fenetre.Notifications.Add(MessageSeverity.Info, "Info", Localizer.Current.T("showcase.feedback.info"))
                fenetre.Notifications.Add(MessageSeverity.Success, "Success", Localizer.Current.T("showcase.feedback.success"))
                fenetre.Notifications.Add(MessageSeverity.Warn, "Warn", Localizer.Current.T("showcase.feedback.warn"))
                fenetre.Notifications.Add(MessageSeverity.Error, Nothing, Localizer.Current.T("showcase.feedback.error"))
                Attendre(200)
                Enregistrer(DirectCast(fenetre.Content, FrameworkElement), $"{theme}/notifications-relief.png")
                Attendre(1500)
                Enregistrer(DirectCast(fenetre.Content, FrameworkElement), $"{theme}/notifications.png")
                fenetre.Notifications.RemoveAll()

                Dim pied As New StackPanel With {.Orientation = Orientation.Horizontal}
                pied.Children.Add(New Mkm.Wpf.Controls.Button With {.Content = Localizer.Current.T("showcase.feedback.cancel"), .Severity = ButtonSeverity.Secondary})
                pied.Children.Add(New Mkm.Wpf.Controls.Button With {.Content = Localizer.Current.T("showcase.feedback.ok")})
                Dim contenu As New StackPanel()
                contenu.Children.Add(New TextBlock With {.Text = Localizer.Current.T("showcase.feedback.newName"), .Margin = New Thickness(0, 0, 0, 4)})
                contenu.Children.Add(New InputText With {.Text = "HS-00042", .Width = 280})
                Dialogue(theme, "dialogue", New Dialog With {.Title = Localizer.Current.T("showcase.feedback.rename"), .Content = contenu, .Footer = pied})
                Dialogue(theme, "dialogue-etirable", New Dialog With {
                    .Title = Localizer.Current.T("showcase.feedback.longTitle"), .ResizeMode = ResizeMode.CanResize,
                    .SizeToContent = SizeToContent.Manual, .Width = 360, .Height = 240,
                    .Content = New TextBlock With {.Text = Localizer.Current.T("showcase.containers.titleBlockText"), .TextWrapping = TextWrapping.Wrap}})
            Finally
                fenetre.Close()
            End Try
        End Sub

        Private Sub Dialogue(theme As String, nom As String, dialogue As Dialog)
            dialogue.WindowStartupLocation = WindowStartupLocation.Manual
            dialogue.Left = 0
            dialogue.Top = 0
            dialogue.ShowActivated = False
            dialogue.Show()
            Try
                Attendre(300)
                Enregistrer(dialogue, $"{theme}/{nom}.png")
            Finally
                dialogue.Close()
            End Try
        End Sub

        Private Sub Essayer(nom As String, action As Action)
            Try
                action()
            Catch ex As Exception
                Noter($"ERREUR {nom} : {ex}")
            End Try
        End Sub

        Private Sub Noter(ligne As String)
            _journal.Add(ligne)
            If ligne.StartsWith("ERREUR", StringComparison.Ordinal) Then _erreurs += 1
        End Sub

        ' Le rendu d'un élément à sa taille, en PNG.
        Private Sub Enregistrer(element As FrameworkElement, nom As String)
            Dim largeurPx = CInt(Math.Ceiling(element.ActualWidth))
            Dim hauteurPx = CInt(Math.Ceiling(element.ActualHeight))
            If largeurPx <= 0 OrElse hauteurPx <= 0 Then Throw New InvalidOperationException($"{nom} : taille nulle")

            Dim image As New RenderTargetBitmap(largeurPx, hauteurPx, 96, 96, PixelFormats.Pbgra32)
            image.Render(element)
            Dim encodeur As New PngBitmapEncoder()
            encodeur.Frames.Add(BitmapFrame.Create(image))
            Dim chemin = Path.Combine(_dossier, nom)
            Directory.CreateDirectory(Path.GetDirectoryName(chemin))
            Using flux = File.Create(chemin)
                encodeur.Save(flux)
            End Using
            Noter($"{nom} {largeurPx}×{hauteurPx}")
        End Sub

        ' Laisse le répartiteur mettre en page, dessiner et faire tourner les minuteries.
        Private Shared Sub Attendre(millisecondes As Integer)
            Dim cadre As New DispatcherFrame()
            Dim minuterie As New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(millisecondes)}
            AddHandler minuterie.Tick, Sub()
                                           minuterie.Stop()
                                           cadre.Continue = False
                                       End Sub
            minuterie.Start()
            Dispatcher.PushFrame(cadre)
        End Sub

    End Class

End Namespace
