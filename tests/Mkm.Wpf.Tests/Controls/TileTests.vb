Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Localization
Imports Mkm.Wpf.Theming
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>La tuile d'un objet dans une liste (Tile).</summary>
    <TestFixture>
    Public Class TileTests

        Private Shared Function Pret(tuile As Tile) As Tile
            tuile.BeginInit()
            tuile.EndInit()
            tuile.ApplyTemplate()
            tuile.Measure(New Size(340, 200))
            tuile.Arrange(New Rect(0, 0, 340, 200))
            tuile.UpdateLayout()
            Return tuile
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        Private Shared Function Ressource(cle As String) As Object
            Return Application.Current.TryFindResource(ThemeResources.Prefix & cle)
        End Function

        Private Shared Function Exemple() As Tile
            Return New Tile With {.Title = "Carter", .Subtitle = "Article | A | En cours | M. Kerram", .Description = "prd-00042",
                                  .TypeIcon = Icons.Structure, .Thumbnail = Icons.Structure}
        End Function

        <Test>
        Public Sub Tile_LesMesuresEtLesCouleursDuTheme()
            Dim tuile = Pret(Exemple())

            Assert.That(tuile.ActualHeight, [Is].EqualTo(70), "la hauteur de la tuile de 3DEXPERIENCE")
            Dim vignette = Partie(Of Border)(tuile, "Vignette")
            Assert.That(vignette.ActualWidth, [Is].EqualTo(79))
            Assert.That(vignette.ActualHeight, [Is].EqualTo(58))
            Assert.That(vignette.Background, [Is].SameAs(Ressource("tile.thumbnail.background")))
            Dim titre = Partie(Of TextBlock)(tuile, "Titre")
            Assert.That(titre.Foreground, [Is].SameAs(Ressource("tile.title.color")))
            Assert.That(titre.FontSize, [Is].EqualTo(14))
            Assert.That(titre.TextTrimming, [Is].EqualTo(TextTrimming.CharacterEllipsis))
            Dim description = Partie(Of TextBlock)(tuile, "LaDescription")
            Assert.That(description.FontWeight, [Is].EqualTo(FontWeights.Bold))
            Assert.That(description.FontSize, [Is].EqualTo(11))
        End Sub

        <Test>
        Public Sub Tile_LeTitreSurDeuxLignesAuPlus()
            Dim tuile = Exemple()
            tuile.Title = "Arbre déshuileur équipé, avec son carter, ses paliers, ses joints et sa notice de montage complète"
            Pret(tuile)
            Dim titre = Partie(Of TextBlock)(tuile, "Titre")

            Assert.That(titre.TextWrapping, [Is].EqualTo(TextWrapping.Wrap))
            Assert.That(titre.TextTrimming, [Is].EqualTo(TextTrimming.CharacterEllipsis), "coupé après la seconde ligne")
            Assert.That(titre.ActualHeight, [Is].EqualTo(30), "deux lignes de 15 px, pas trois")
        End Sub

        <TestCase("Carter")>
        <TestCase("Arbre déshuileur équipé, avec son carter, ses paliers, ses joints et sa notice de montage complète")>
        Public Sub Tile_LeSousTitreEtLaDescriptionGardentLeurPlace(titre As String)
            Dim tuile = Exemple()
            tuile.Title = titre
            Pret(tuile)

            ' Comme la recherche de 3DEXPERIENCE, sous le bord de 1 px : le titre à 4 px, le sous-titre à 36, la description à 51.
            Assert.That(Partie(Of FrameworkElement)(tuile, "Titre").TranslatePoint(New Point(0, 0), tuile).Y, [Is].EqualTo(5))
            Assert.That(Partie(Of FrameworkElement)(tuile, "SousTitre").TranslatePoint(New Point(0, 0), tuile).Y, [Is].EqualTo(37))
            Assert.That(Partie(Of FrameworkElement)(tuile, "LaDescription").TranslatePoint(New Point(0, 0), tuile).Y, [Is].EqualTo(52))
        End Sub

        <Test>
        Public Sub Tile_UneImageEnFondDe79PxDeLargeEtUnePetiteASaTaille()
            Dim tuile = Exemple()
            tuile.Thumbnail = UneImage(144, 108)
            Pret(tuile)
            Dim vignette = Partie(Of Border)(tuile, "Vignette")
            Dim grande = TrouverLImage(vignette)

            Assert.That(grande.ActualWidth, [Is].EqualTo(79), "la largeur de la zone")
            Assert.That(grande.ActualHeight, [Is].EqualTo(79.0 * 108 / 144).Within(0.01), "la hauteur à proportion")
            Assert.That(vignette.ClipToBounds, [Is].True, "ce qui dépasse la zone est coupé")

            tuile.Thumbnail = UneImage(16, 16)
            tuile.UpdateLayout()

            Assert.That(TrouverLImage(vignette).ActualWidth, [Is].EqualTo(16), "une petite image n'est pas agrandie")
        End Sub

        Private Shared Function UneImage(largeur As Integer, hauteur As Integer) As ImageSource
            Return Imaging.BitmapSource.Create(largeur, hauteur, 96, 96, PixelFormats.Bgra32, Nothing, New Byte(largeur * hauteur * 4 - 1) {}, largeur * 4)
        End Function

        Private Shared Function TrouverLImage(parent As DependencyObject) As System.Windows.Controls.Image
            For i = 0 To VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim enfant = VisualTreeHelper.GetChild(parent, i)
                Dim trouvee = TryCast(enfant, System.Windows.Controls.Image)
                If trouvee IsNot Nothing Then Return trouvee
                trouvee = TrouverLImage(enfant)
                If trouvee IsNot Nothing Then Return trouvee
            Next
            Return Nothing
        End Function

        <Test>
        Public Sub Tile_UnSousTitreTexteSeCoupe()
            Dim tuile = Pret(Exemple())

            Dim sousTitre = Partie(Of ContentPresenter)(tuile, "SousTitre")
            Dim texte = TrouverLeTexte(sousTitre)

            Assert.That(texte, [Is].Not.Null)
            Assert.That(texte.TextTrimming, [Is].EqualTo(TextTrimming.CharacterEllipsis))
            Assert.That(texte.Foreground, [Is].SameAs(Ressource("tile.subtitle.color")))
        End Sub

        Private Shared Function TrouverLeTexte(parent As DependencyObject) As TextBlock
            For i = 0 To VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim enfant = VisualTreeHelper.GetChild(parent, i)
                Dim texte = TryCast(enfant, TextBlock)
                If texte IsNot Nothing Then Return texte
                texte = TrouverLeTexte(enfant)
                If texte IsNot Nothing Then Return texte
            Next
            Return Nothing
        End Function

        <Test>
        Public Sub Tile_LeLisereSeulementAvecUneCouleur()
            Dim tuile = Pret(Exemple())
            Dim lisere = Partie(Of Border)(tuile, "Lisere")
            Assert.That(lisere.Visibility, [Is].EqualTo(Visibility.Collapsed), "aucun liseré par défaut")

            tuile.Stripe = Brushes.DarkGreen
            tuile.UpdateLayout()

            Assert.That(lisere.Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(lisere.Background, [Is].SameAs(Brushes.DarkGreen))
            Assert.That(lisere.ActualWidth, [Is].EqualTo(4))
        End Sub

        <Test>
        Public Sub Tile_LaZoneDeLaVignetteResteSansImage()
            Dim tuile = Pret(New Tile With {.Title = "Sans vignette", .Description = "prd-00043"})

            Assert.That(Partie(Of Border)(tuile, "Vignette").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of UIElement)(tuile, "IconeDuType").Visibility, [Is].EqualTo(Visibility.Collapsed), "sans icône, le titre prend sa place")
        End Sub

        <Test>
        Public Sub Tile_LeChevronSeulementAvecUnMenu()
            Dim tuile = Pret(Exemple())
            Dim chevron = Partie(Of ButtonBase)(tuile, Tile.PartMenuButton)
            Assert.That(chevron.Visibility, [Is].EqualTo(Visibility.Collapsed))

            tuile.HasMenu = True
            tuile.UpdateLayout()

            Assert.That(chevron.Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub Tile_LeChevronDemandeLeMenuEtOuvreLeMenuContextuelSiPersonneNeLeTraite()
            Dim tuile = Exemple()
            tuile.HasMenu = True
            tuile.ContextMenu = New ContextMenu()
            tuile.ContextMenu.Items.Add(New MenuItem With {.Header = "Information"})
            Pret(tuile)
            Dim chevron = Partie(Of ButtonBase)(tuile, Tile.PartMenuButton)
            Dim demandes = 0
            Dim traiter = True
            AddHandler tuile.MenuRequested, Sub(s, e)
                                                demandes += 1
                                                e.Handled = traiter
                                            End Sub

            chevron.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))
            Assert.That(demandes, [Is].EqualTo(1))
            Assert.That(tuile.ContextMenu.IsOpen, [Is].False, "l'application a traité la demande")

            traiter = False
            chevron.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))
            Try
                Assert.That(demandes, [Is].EqualTo(2))
                Assert.That(tuile.ContextMenu.IsOpen, [Is].True)
                Assert.That(tuile.ContextMenu.PlacementTarget, [Is].SameAs(chevron))
            Finally
                tuile.ContextMenu.IsOpen = False
            End Try
        End Sub

        <Test>
        Public Sub Tile_ChoisieElleEstCernee()
            Dim tuile = Pret(Exemple())
            Dim cadre = Partie(Of Border)(tuile, "Cadre")
            Assert.That(cadre.Visibility, [Is].EqualTo(Visibility.Collapsed))

            tuile.IsSelected = True
            tuile.UpdateLayout()

            Assert.That(cadre.Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(cadre.BorderBrush, [Is].SameAs(Ressource("tile.selected.border.color")))
        End Sub

        <Test>
        Public Sub Tile_EnLectureUnSqueletteSansChevronNiLisere()
            Dim tuile = Exemple()
            tuile.HasMenu = True
            tuile.Stripe = Brushes.DarkGreen
            tuile.IsLoading = True
            Pret(tuile)

            Assert.That(Partie(Of UIElement)(tuile, "SqueletteDeLaVignette").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of UIElement)(tuile, "SqueletteDuTexte").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of UIElement)(tuile, "Texte").Visibility, [Is].EqualTo(Visibility.Hidden))
            Assert.That(Partie(Of UIElement)(tuile, Tile.PartMenuButton).Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of UIElement)(tuile, "Lisere").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(tuile.ActualHeight, [Is].EqualTo(70), "la même hauteur qu'une tuile lue : rien ne saute")
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(tuile).GetName(), [Is].EqualTo(Localizer.Current.T("aria.busy")))

            tuile.IsLoading = False
            tuile.UpdateLayout()

            Assert.That(Partie(Of UIElement)(tuile, "SqueletteDuTexte").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of UIElement)(tuile, "Texte").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of UIElement)(tuile, Tile.PartMenuButton).Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of UIElement)(tuile, "Lisere").Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub Tile_UnGroupeNommeParSonTitreEtUnChevronNomme()
            Dim tuile = Exemple()
            tuile.HasMenu = True
            Pret(tuile)
            Dim chevron = Partie(Of ButtonBase)(tuile, Tile.PartMenuButton)

            Assert.That(UIElementAutomationPeer.CreatePeerForElement(tuile).GetName(), [Is].EqualTo("Carter"))
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(tuile).GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Group))
            Assert.That(AutomationProperties.GetName(chevron), [Is].EqualTo(Localizer.Current.T("tile.menu")))

            tuile.MenuLabel = "Actions de l'article"

            Assert.That(AutomationProperties.GetName(chevron), [Is].EqualTo("Actions de l'article"))
        End Sub

    End Class

End Namespace
