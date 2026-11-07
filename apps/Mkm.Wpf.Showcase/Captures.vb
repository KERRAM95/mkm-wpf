Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq
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
        Private _erreurs As Integer

        Private Sub New(dossier As String)
            _dossier = dossier
        End Sub

        ''' <summary>Fait toutes les captures dans <paramref name="dossier"/> ; rend le nombre d'erreurs.</summary>
        Public Shared Function Executer(dossier As String) As Integer
            Dim c As New Captures(dossier)
            c.Noter("début")
            ' Les erreurs de liaison de WPF (une propriété introuvable, un type qui ne va pas) vont au journal.
            PresentationTraceSources.Refresh()
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning
            PresentationTraceSources.DataBindingSource.Listeners.Add(New Ecouteur(c))
            Try
                c.Tout()
            Catch ex As Exception
                c.Noter($"ERREUR {ex}")
            End Try
            c.Noter($"fin : {c._erreurs} erreur(s)")
            Return c._erreurs
        End Function

        ''' <summary>Une ligne du journal (<c>journal.txt</c>), écrite aussitôt : un arrêt brutal la garde.</summary>
        Public Shared Sub Journal(dossier As String, ligne As String)
            Directory.CreateDirectory(dossier)
            File.AppendAllText(Path.Combine(dossier, "journal.txt"), ligne & Environment.NewLine)
        End Sub

        Private Sub Tout()
            Localizer.Current.Language = CultureInfo.GetCultureInfo("fr")
            Dim pages As New List(Of KeyValuePair(Of String, Func(Of FrameworkElement))) From {
                Page("1-actionbar", Function() New ActionBarPage()),
                Page("2-champs", Function() New ChampsPage()),
                Page("3-boutons", Function() New BoutonsPage()),
                Page("4-conteneurs", Function() New ConteneursPage()),
                Page("5-retours", Function() New RetoursPage()),
                Page("6-attente", Function() New AttentePage()),
                Page("7-saisie", Function() New SaisiePage()),
                Page("8-fiche", Function() New FichePage()),
                Page("9-arbre", Function() New ArbrePage())
            }

            For Each theme In Themes.All
                MkmResources.Current.Theme = theme.Name
                For Each p In pages
                    Essayer($"{theme.Name}/{p.Key}", Sub() Enregistrer(Seule(p.Value.Invoke()), $"{theme.Name}/{p.Key}.png"))
                Next
                Essayer($"{theme.Name}/fenetre", Sub() Fenetre(theme.Name))
                Essayer($"{theme.Name}/scènes", Sub() Scenes(theme.Name))
            Next
            Essayer("diagnostic ActionBar", Sub() DiagnostiquerActionBar())
        End Sub

        ' Ce qui ne paraît pas sur une page seule : ce qui s'ouvre dans une fenêtre flottante (le calendrier, un menu,
        ' une info-bulle) et des états qu'on n'atteint qu'en agissant (la fiche en modification, le voile posé).
        Private Sub Scenes(theme As String)
            Dim fiche As New FichePage()
            fiche.Fiche.Editing = True
            Enregistrer(Seule(fiche), $"{theme}/8-fiche-modification.png")

            Dim attente = New AttentePage()
            Dim page = Seule(attente)
            attente.Voile.Blocked = True
            Attendre(500)
            page.UpdateLayout()
            Enregistrer(page, $"{theme}/6-attente-voile.png")

            Dim choix As New Mkm.Wpf.Controls.DatePicker()
            choix.ApplyTemplate()
            Dim calendrier As New System.Windows.Controls.Calendar With {.Style = choix.CalendarStyle, .DisplayDate = Date.Today, .SelectedDate = Date.Today.AddDays(2)}
            Enregistrer(Seule(calendrier, 0), $"{theme}/calendrier.png")

            Dim bulle As New ToolTip With {.Content = Localizer.Current.T("showcase.waiting.tooltipText")}
            Enregistrer(Seule(bulle, 0), $"{theme}/infobulle.png")

            Enregistrer(Seule(UnMenu(True), 0), $"{theme}/menu.png")
            Enregistrer(Seule(UnMenu(False), 0), $"{theme}/menu-sans-icones.png")

            Dialogue(theme, "confirmation", New ConfirmDialog With {
                .Title = Localizer.Current.T("showcase.inputs.update"),
                .Message = Localizer.Current.T("showcase.inputs.question"),
                .MessageIcon = Icons.Warning})
        End Sub

        ' Un menu comme celui de la page de l'arbre, avec ou sans icônes, tel qu'il s'ouvre.
        Private Shared Function UnMenu(icones As Boolean) As ContextMenu
            Dim menu As New ContextMenu()
            Dim element = Function(cle As String, icone As Object, raccourci As String) New MenuItem With {
                .Header = Localizer.Current.T(cle), .Icon = If(icones, icone, Nothing), .InputGestureText = raccourci}
            menu.Items.Add(element("showcase.tree.add", Icons.Add, "Ctrl+N"))
            menu.Items.Add(element("showcase.tree.copy", Icons.Copy, "Ctrl+C"))
            menu.Items.Add(element("showcase.tree.rename", Icons.Rename, "F2"))
            menu.Items.Add(New Separator())
            Dim plus = element("showcase.tree.more", Nothing, Nothing)
            plus.Items.Add(element("showcase.tree.export", Icons.Export, Nothing))
            menu.Items.Add(plus)
            Dim supprimer = element("showcase.tree.delete", Icons.Delete, "Suppr")
            supprimer.IsEnabled = False
            menu.Items.Add(supprimer)
            menu.RaiseEvent(New RoutedEventArgs(ContextMenu.OpenedEvent, menu))
            Return menu
        End Function

        ' Ce que deviennent les commandes de l'ActionBar une fois dessinées : leur icône, sa taille, sa couleur.
        Private Sub DiagnostiquerActionBar()
            Dim page = Seule(New ActionBarPage())
            For Each commande In Descendants(page).OfType(Of ActionBarButton)().Take(3)
                Dim icone = Descendants(commande).OfType(Of Icon)().FirstOrDefault()
                Noter($"ActionBarButton {commande.Label} : {commande.ActualWidth}×{commande.ActualHeight}, gabarit {commande.Template IsNot Nothing}, " &
                      $"Icon {If(commande.Icon, "(aucune)")}, opacité {commande.Opacity}, visible {commande.IsVisible}")
                If icone Is Nothing Then
                    Noter("  pas d'élément Icon dans le gabarit")
                Else
                    Noter($"  élément Icon : {icone.ActualWidth}×{icone.ActualHeight}, Source {If(icone.Source, "(aucune)")}, " &
                          $"Foreground {icone.Foreground}, opacité {icone.Opacity}, visible {icone.IsVisible}")
                End If
            Next
        End Sub

        ' Écrit au journal ce que WPF trace, une ligne par message.
        Private NotInheritable Class Ecouteur
            Inherits TraceListener

            Private ReadOnly _captures As Captures
            Private _ligne As String = ""

            Public Sub New(captures As Captures)
                _captures = captures
            End Sub

            Public Overrides Sub Write(message As String)
                _ligne &= message
            End Sub

            Public Overrides Sub WriteLine(message As String)
                _captures.Noter($"WPF : {_ligne}{message}")
                _ligne = ""
            End Sub
        End Class

        Private Shared Iterator Function Descendants(racine As DependencyObject) As IEnumerable(Of DependencyObject)
            For i = 0 To VisualTreeHelper.GetChildrenCount(racine) - 1
                Dim enfant = VisualTreeHelper.GetChild(racine, i)
                Yield enfant
                For Each petit In Descendants(enfant)
                    Yield petit
                Next
            Next
        End Function

        Private Shared Function Page(nom As String, fabrique As Func(Of FrameworkElement)) As KeyValuePair(Of String, Func(Of FrameworkElement))
            Return New KeyValuePair(Of String, Func(Of FrameworkElement))(nom, fabrique)
        End Function

        ' Une page hors de toute fenêtre, à la largeur de la vitrine et à toute sa hauteur, sur le fond du thème.
        Private Shared Function Seule(element As FrameworkElement, Optional largeurVoulue As Double = Largeur) As FrameworkElement
            ' Une largeur nulle : l'élément à sa taille, avec 12 px autour.
            Dim fond As New Border With {.Child = element}
            If largeurVoulue > 0 Then fond.Width = largeurVoulue Else fond.Padding = New Thickness(12)
            fond.SetResourceReference(Border.BackgroundProperty, ThemeResources.Prefix & "content.background")
            fond.SetResourceReference(TextElement.ForegroundProperty, ThemeResources.Prefix & "text.color")
            fond.SetResourceReference(TextElement.FontFamilyProperty, ThemeResources.Prefix & "font.family")
            fond.SetResourceReference(TextElement.FontSizeProperty, ThemeResources.Prefix & "font.size")
            fond.Measure(New Size(If(largeurVoulue > 0, largeurVoulue, Double.PositiveInfinity), Double.PositiveInfinity))
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
            Journal(_dossier, ligne)
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
