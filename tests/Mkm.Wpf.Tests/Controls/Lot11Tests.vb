Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 11 : Badge, OverlayBadge, Avatar, AvatarGroup, Chip.</summary>
    <TestFixture>
    Public Class Lot11Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 400, Optional hauteur As Double = 200) As T
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

        ' ---- Badge ----

        <Test>
        Public Sub Badge_UnPointUnRondOuUnTexte()
            Dim pastille = Pret(New Badge())
            Assert.That(pastille.IsDot, [Is].True, "sans valeur, un point")

            pastille.Value = 3
            Assert.That(pastille.IsDot, [Is].False)
            Assert.That(pastille.IsCircle, [Is].True, "un seul caractère, un rond")

            pastille.Value = "12"
            Assert.That(pastille.IsCircle, [Is].False)
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(pastille).GetName(), [Is].EqualTo("12"), "un texte pour les lecteurs d'écran")
        End Sub

        <Test>
        Public Sub OverlayBadge_LaPastilleCentreeSurLeCoin()
            Dim contenu As New Border With {.Width = 100, .Height = 40}
            Dim pose = Pret(New OverlayBadge With {.Value = "4", .Content = contenu})
            Dim pastille = Partie(Of Badge)(pose, "Pastille")
            Dim coin = pastille.TransformToAncestor(pose).Transform(New Point(0, 0))

            Assert.That(pose.ActualWidth, [Is].EqualTo(100), "la pastille ne prend pas de place")
            Assert.That(coin.X, [Is].EqualTo(100 - pastille.ActualWidth / 2).Within(0.5))
            Assert.That(coin.Y, [Is].EqualTo(-pastille.ActualHeight / 2).Within(0.5))
            Assert.That(pastille.BorderThickness.Left, [Is].EqualTo(2), "cernée du fond de la page")
        End Sub

        ' ---- Avatar ----

        <Test>
        Public Sub Avatar_TexteOuIcone()
            Dim initiales = Pret(New Avatar With {.Label = "MK"})
            Assert.That(Partie(Of Icon)(initiales, "Icone").Visibility, [Is].EqualTo(Visibility.Collapsed))

            Dim icone = Pret(New Avatar With {.Icon = Icons.Person})
            Assert.That(Partie(Of Icon)(icone, "Icone").Visibility, [Is].EqualTo(Visibility.Visible), "sans texte, l'icône")

            Dim pair = UIElementAutomationPeer.CreatePeerForElement(initiales)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Image))
            Assert.That(pair.GetName(), [Is].EqualTo("MK"))
        End Sub

        <Test>
        Public Sub AvatarGroup_LesAvatarsSeChevauchent()
            Dim groupe As New AvatarGroup()
            Dim premier As New Avatar With {.Label = "MK"}
            Dim second As New Avatar With {.Label = "JD"}
            groupe.Items.Add(premier)
            groupe.Items.Add(second)
            Pret(groupe)

            Assert.That(premier.IsInGroup AndAlso second.IsInGroup, [Is].True)
            Assert.That(premier.Margin.Left, [Is].EqualTo(0))
            Assert.That(second.Margin.Left, [Is].LessThan(0), "chevauche le précédent (avatar.group.offset)")
            Assert.That(second.BorderThickness.Left, [Is].EqualTo(2), "cerné du fond de la page")
        End Sub

        ' ---- Chip ----

        <Test>
        Public Sub Chip_RetireeElleSeCacheEtLeDit()
            Dim puce = Pret(New Chip With {.Content = "Maturité : En cours", .Removable = True})
            Dim retiree = False
            AddHandler puce.Remove, Sub() retiree = True

            Assert.That(Partie(Of ButtonBase)(puce, "PART_Remove").Visibility, [Is].EqualTo(Visibility.Visible))
            puce.Close()
            Assert.That(puce.Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(retiree, [Is].True)
        End Sub

        <Test>
        Public Sub Chip_SaCroixSeNommeEtNeParaitQueRetirable()
            Dim simple = Pret(New Chip With {.Content = "Plans"})
            Assert.That(Partie(Of ButtonBase)(simple, "PART_Remove").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(simple).GetName(), [Is].EqualTo("Plans"))

            Dim retirable = Pret(New Chip With {.Content = "Plans", .Removable = True})
            Dim croix = Partie(Of ButtonBase)(retirable, "PART_Remove")
            Assert.That(System.Windows.Automation.AutomationProperties.GetName(croix),
                        [Is].EqualTo(Mkm.Wpf.Localization.Localizer.Current.T("aria.remove")))
        End Sub

    End Class

End Namespace
