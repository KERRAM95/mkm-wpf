Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
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
            ' Un exemple de la documentation qui ne se lit pas est une erreur des captures.
            Documentation.Signalement.Sortie = Sub(texte) c.Noter("ERREUR " & texte)
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
            For Each theme In Themes.All
                MkmResources.Current.Theme = theme.Name
                ' Chaque page de la documentation, en entier : ses fonctionnalités, son API et ses jetons à la suite.
                For Each doc In Documentation.Catalogue.Pages
                    Essayer($"{theme.Name}/doc-{doc.Id}", Sub() Enregistrer(Seule(New Documentation.PageDeDocumentation(doc, True)), $"{theme.Name}/doc-{doc.Id}.png"))
                Next
                Essayer($"{theme.Name}/fenetre", Sub() Fenetre(theme.Name))
                Essayer($"{theme.Name}/scènes", Sub() Scenes(theme.Name))
            Next
        End Sub

        ' Ce qui ne paraît pas sur une page seule : ce qui s'ouvre dans une fenêtre flottante (le calendrier, un menu,
        ' une info-bulle) et des états qu'on n'atteint qu'en agissant (la fiche en modification, le voile posé), pris dans
        ' les exemples de la documentation.
        Private Sub Scenes(theme As String)
            Dim fiche = Seule(New Documentation.PageDeDocumentation(Documentation.Catalogue.Trouver("propertypanel"), True))
            Nomme(Of PropertyPanel)(fiche, "Fiche").Editing = True
            fiche.UpdateLayout()
            Enregistrer(CadreDe(Nomme(Of PropertyPanel)(fiche, "Fiche")), $"{theme}/fiche-modification.png")

            Dim attente = Seule(New Documentation.PageDeDocumentation(Documentation.Catalogue.Trouver("blockui"), True))
            Nomme(Of BlockUI)(attente, "Voile").Blocked = True
            Attendre(500)
            attente.UpdateLayout()
            Enregistrer(CadreDe(Nomme(Of BlockUI)(attente, "Voile")), $"{theme}/voile.png")

            ' Initialisé, le DatePicker prend son style (et son calendrier) comme dans une fenêtre.
            Dim choix As New Mkm.Wpf.Controls.DatePicker()
            choix.BeginInit()
            choix.EndInit()
            choix.ApplyTemplate()
            Dim calendrier As New System.Windows.Controls.Calendar With {.Style = choix.CalendarStyle, .DisplayDate = Date.Today, .SelectedDate = Date.Today.AddDays(2)}
            Enregistrer(Seule(calendrier, 0), $"{theme}/calendrier.png")

            ' La mise en page étroite : sous 550 px, le panneau de gauche recouvre le principal, un voile dessous ; celui de
            ' droite a cédé la place.
            Dim etroite = Seule(New Documentation.PageDeDocumentation(Documentation.Catalogue.Trouver("triptych"), True), 520)
            etroite.UpdateLayout()
            Enregistrer(etroite, $"{theme}/triptych-etroit.png")
            Dialogue(theme, "confirmation", New ConfirmDialog With {
                .Title = "Mise à jour",
                .Message = "Mettre à jour le cartouche du plan avec les valeurs de 3DEXPERIENCE ?",
                .MessageIcon = Icons.Warning})
        End Sub

        ' L'élément de ce nom dans l'arbre visuel (le premier qui remplit la condition) ; une erreur s'il n'y en a pas.
        Private Shared Function Nomme(Of T As FrameworkElement)(racine As DependencyObject, nom As String,
                                                             Optional condition As Func(Of T, Boolean) = Nothing) As T
            Dim trouve = Descendants(racine).OfType(Of T)().FirstOrDefault(Function(e) e.Name = nom AndAlso (condition Is Nothing OrElse condition(e)))
            If trouve Is Nothing Then Throw New InvalidOperationException($"« {nom} » introuvable")
            Return trouve
        End Function

        ' Le cadre de l'exemple qui contient un élément.
        Private Shared Function CadreDe(element As DependencyObject) As FrameworkElement
            Dim d = element
            While d IsNot Nothing AndAlso Not TypeOf d Is Documentation.CadreDExemple
                d = VisualTreeHelper.GetParent(d)
            End While
            Return DirectCast(d, FrameworkElement)
        End Function

        ' Un menu comme celui de la page de l'arbre, avec ou sans icônes, tel qu'il s'ouvre.
        Private Shared Function UnMenu(icones As Boolean) As ContextMenu
            Dim menu As New ContextMenu()
            Dim element = Function(cle As String, icone As Object, raccourci As String) New MenuItem With {
                .Header = cle, .Icon = If(icones, icone, Nothing), .InputGestureText = raccourci}
            menu.Items.Add(element("Ajouter une note", Icons.Add, "Ctrl+N"))
            menu.Items.Add(element("Copier", Icons.Copy, "Ctrl+C"))
            menu.Items.Add(element("Renommer", Icons.Rename, "F2"))
            menu.Items.Add(New Separator())
            Dim plus = element("Plus", Nothing, Nothing)
            plus.Items.Add(element("Exporter", Icons.Export, Nothing))
            menu.Items.Add(plus)
            Dim supprimer = element("Supprimer", Icons.Delete, "Suppr")
            supprimer.IsEnabled = False
            menu.Items.Add(supprimer)
            menu.RaiseEvent(New RoutedEventArgs(ContextMenu.OpenedEvent, menu))
            Return menu
        End Function

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
            ' Une seconde passe, comme dans une fenêtre : un DataGrid répartit ses colonnes en étoile (« 2* ») selon la
            ' largeur de la passe précédente ; à la première, elles restaient à leur minimum.
            Attendre(50)
            fond.InvalidateMeasure()
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

                fenetre.Notifications.Add(MessageSeverity.Info, "Info", "Le plan est ouvert en lecture seule.")
                fenetre.Notifications.Add(MessageSeverity.Success, "Success", "Le cartouche est à jour.")
                fenetre.Notifications.Add(MessageSeverity.Warn, "Warn", "Le cartouche n'est pas à jour.")
                fenetre.Notifications.Add(MessageSeverity.Error, Nothing, "Le plan n'a pas pu être enregistré.")
                Attendre(200)
                Enregistrer(DirectCast(fenetre.Content, FrameworkElement), $"{theme}/notifications-relief.png")
                Attendre(1500)
                Enregistrer(DirectCast(fenetre.Content, FrameworkElement), $"{theme}/notifications.png")
                fenetre.Notifications.RemoveAll()

                ' Une info-bulle et un menu ne se dessinent qu'ouverts, dans leur fenêtre flottante.
                Flottante(theme, "infobulle", fenetre.Composants,
                          New ToolTip With {.Content = "Met à jour le cartouche du plan depuis 3DEXPERIENCE."})
                Flottante(theme, "menu", fenetre.Composants, UnMenu(True))
                Flottante(theme, "menu-sans-icones", fenetre.Composants, UnMenu(False))
                ' La page des choix : le menu « … » de sa barre étroite, ouvert par son bouton.
                MenuDeLaBarre(theme, fenetre)

                Dim pied As New StackPanel With {.Orientation = Orientation.Horizontal}
                pied.Children.Add(New Mkm.Wpf.Controls.Button With {.Content = "Annuler", .Severity = ButtonSeverity.Secondary})
                pied.Children.Add(New Mkm.Wpf.Controls.Button With {.Content = "Renommer"})
                Dim contenu As New StackPanel()
                contenu.Children.Add(New TextBlock With {.Text = "Nouveau nom", .Margin = New Thickness(0, 0, 0, 4)})
                contenu.Children.Add(New InputText With {.Text = "HS-00042", .Width = 280})
                Dialogue(theme, "dialogue", New Dialog With {.Title = "Renommer le plan", .Content = contenu, .Footer = pied})
                Dialogue(theme, "dialogue-etirable", New Dialog With {
                    .Title = "Propriétés du plan HS-00042 — feuille 1 sur 3, révision B, en cours", .ResizeMode = ResizeMode.CanResize,
                    .SizeToContent = SizeToContent.Manual, .Width = 360, .Height = 240,
                    .Content = New TextBlock With {.Text = "Le cartouche du plan : référence, désignation, échelle.", .TextWrapping = TextWrapping.Wrap}})
            Finally
                fenetre.Close()
            End Try
        End Sub

        ' Le menu « … » de la barre étroite de la page des choix, ouvert comme d'un clic sur son bouton.
        Private Sub MenuDeLaBarre(theme As String, fenetre As MainWindow)
            fenetre.Ouvrir("toolbar")
            Attendre(500)
            Dim barre = Nomme(Of Mkm.Wpf.Controls.Toolbar)(DirectCast(fenetre.Page.Content, DependencyObject), "Barre", Function(b) b.Overflow)
            Dim bouton = DirectCast(barre.Template.FindName("PART_Overflow", barre), System.Windows.Controls.Button)
            Dim pair As New ButtonAutomationPeer(bouton)
            DirectCast(pair.GetPattern(PatternInterface.Invoke), IInvokeProvider).Invoke()
            Try
                Attendre(300)
                Enregistrer(barre.OverflowMenu, $"{theme}/barre-menu.png")
            Finally
                If barre.OverflowMenu IsNot Nothing Then barre.OverflowMenu.IsOpen = False
            End Try
        End Sub

        ' Ouvre une info-bulle ou un menu contre un élément de la fenêtre, le dessine, le ferme.
        Private Sub Flottante(theme As String, nom As String, cible As UIElement, flottante As FrameworkElement)
            Dim bulle = TryCast(flottante, ToolTip)
            Dim menu = TryCast(flottante, ContextMenu)
            If bulle IsNot Nothing Then
                bulle.PlacementTarget = cible
                bulle.IsOpen = True
            ElseIf menu IsNot Nothing Then
                menu.PlacementTarget = cible
                menu.IsOpen = True
            End If
            Try
                Attendre(300)
                Enregistrer(flottante, $"{theme}/{nom}.png")
            Finally
                If bulle IsNot Nothing Then bulle.IsOpen = False
                If menu IsNot Nothing Then menu.IsOpen = False
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
