Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Threading
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 16 : Drawer, Inplace, Image, ImageCompare.</summary>
    <TestFixture>
    Public Class Lot16Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 600, Optional hauteur As Double = 400) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        ' Ce que le Dispatcher a en attente.
        Private Shared Sub Vider()
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, New System.Action(Sub()
                                                                                                End Sub))
        End Sub

        Private Shared Function UneImage() As BitmapSource
            Dim pixels(16 * 9 * 4 - 1) As Byte
            Return BitmapSource.Create(16, 9, 96, 96, PixelFormats.Bgra32, Nothing, pixels, 16 * 4)
        End Function

        ' ---- Drawer ----

        <Test>
        Public Sub Drawer_PanelWidthRegleLaLargeurDuPanneau()
            Dim tiroir = Pret(New Drawer With {.Position = DrawerPosition.Right, .PanelWidth = 420})
            Dim panneau = DirectCast(tiroir.Template.FindName("PART_Panel", tiroir), FrameworkElement)
            Assert.That(panneau.Width, [Is].EqualTo(420))
            Assert.That(Double.IsInfinity(panneau.MaxWidth), [Is].True, "à droite, pas de maximum")
            tiroir.Position = DrawerPosition.Top
            Assert.That(Double.IsNaN(panneau.Width), [Is].True, "en haut, toute la largeur…")
            Assert.That(panneau.MaxWidth, [Is].EqualTo(420), "… jusqu'à son maximum, centré au-delà")
            tiroir.ClearValue(Drawer.PanelWidthProperty)
            Assert.That(Double.IsInfinity(panneau.MaxWidth), [Is].True, "sans largeur donnée, toute la largeur")
        End Sub

        <Test>
        Public Sub Drawer_EnBasSurUnGrandEcranLePanneauEstCentre()
            Dim tiroir = Pret(New Drawer With {.Position = DrawerPosition.Bottom, .PanelWidth = 600, .IsOpen = True},
                              largeur:=1600, hauteur:=900)
            Dim panneau = DirectCast(tiroir.Template.FindName("PART_Panel", tiroir), FrameworkElement)
            Assert.That(panneau.ActualWidth, [Is].EqualTo(600))
            Dim gauche = panneau.TranslatePoint(New Point(0, 0), tiroir).X
            Assert.That(gauche, [Is].EqualTo(500).Within(1), "centré : (1600 - 600) / 2")
        End Sub

        <Test>
        Public Sub Drawer_FermeIlNeSeVoitPas()
            Dim tiroir = Pret(New Drawer With {.Header = "Filtres", .Content = New TextBlock With {.Text = "Plans"}})
            Assert.That(tiroir.Visibility, [Is].EqualTo(Visibility.Collapsed))
            tiroir.Show()
            Assert.That(tiroir.IsOpen, [Is].True)
            Assert.That(tiroir.Visibility, [Is].EqualTo(Visibility.Visible))
            tiroir.Hide()
            Assert.That(tiroir.Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub Drawer_LaCroixLeFermeEtPeutSeRetirer()
            Dim tiroir = Pret(New Drawer With {.Header = "Filtres", .IsOpen = True})
            Dim croix = Partie(Of ButtonBase)(tiroir, "PART_Close")
            croix.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, croix))
            Assert.That(tiroir.IsOpen, [Is].False)

            tiroir.ShowCloseIcon = False
            tiroir.Show()
            tiroir.UpdateLayout()
            Assert.That(croix.Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub Drawer_SansPiedLaPlaceDuPiedSeRetire()
            Dim tiroir = Pret(New Drawer With {.Header = "Filtres", .IsOpen = True})
            Assert.That(Partie(Of FrameworkElement)(tiroir, "Pied").Visibility, [Is].EqualTo(Visibility.Collapsed))
            tiroir.Footer = New TextBlock With {.Text = "Diffuser"}
            Assert.That(Partie(Of FrameworkElement)(tiroir, "Pied").Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub Drawer_UnVoletNommeParSonTitre()
            Dim tiroir = Pret(New Drawer With {.Header = "Filtres", .IsOpen = True})
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(tiroir)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Pane))
            Assert.That(pair.GetName(), [Is].EqualTo("Filtres"))
        End Sub

        ' ---- Inplace ----

        <Test>
        Public Sub Inplace_UnClicMontreLeContenu()
            Dim place = Pret(New Inplace With {.Display = "HS-00042", .Content = New TextBox With {.Text = "HS-00042"}})
            Assert.That(place.IsActive, [Is].False)
            Dim texte = Partie(Of ButtonBase)(place, "PART_Display")
            texte.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, texte))
            Assert.That(place.IsActive, [Is].True)
            Vider()
            place.Deactivate()
            Assert.That(place.IsActive, [Is].False)
        End Sub

        ' ---- Image ----

        <Test>
        Public Sub Image_SansApercuPasDeBouton()
            Dim image = Pret(New Global.Mkm.Wpf.Controls.Image With {.Source = UneImage()}, 160, 90)
            Dim bouton = Partie(Of ButtonBase)(image, "PART_Preview")
            Assert.That(bouton Is Nothing OrElse bouton.Visibility <> Visibility.Visible, [Is].True)
            image.Preview = True
            image.UpdateLayout()
            Assert.That(Partie(Of ButtonBase)(image, "PART_Preview").Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub Image_UneImagePourLesLecteursDEcran()
            Dim image = Pret(New Global.Mkm.Wpf.Controls.Image With {.Source = UneImage()}, 160, 90)
            Automation.AutomationProperties.SetName(image, "Plan")
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(image)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Image))
            Assert.That(pair.GetName(), [Is].EqualTo("Plan"))
        End Sub

        ' ---- ImageCompare ----

        <Test>
        Public Sub ImageCompare_En16Sur9EtDecoupeeALaPoignee()
            Dim comparaison As New ImageCompare With {.LeftImage = UneImage(), .RightImage = UneImage()}
            comparaison.BeginInit()
            comparaison.EndInit()
            comparaison.ApplyTemplate()
            comparaison.Measure(New Size(400, Double.PositiveInfinity))
            Assert.That(comparaison.DesiredSize.Height, [Is].EqualTo(225).Within(0.5), "16/9 de sa largeur")
            comparaison.Arrange(New Rect(0, 0, 400, 225))
            comparaison.UpdateLayout()

            Dim gauche = Partie(Of FrameworkElement)(comparaison, "PART_Left")
            Assert.That(gauche.Clip.Bounds.Width, [Is].EqualTo(200).Within(0.5), "50 par défaut")
            comparaison.Value = 25
            comparaison.UpdateLayout()
            Assert.That(gauche.Clip.Bounds.Width, [Is].EqualTo(100).Within(0.5))
        End Sub

    End Class

End Namespace
