Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls
Imports NUnit.Framework
Imports MenuDeMkm = Mkm.Wpf.Controls.Menu

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les composants du septième lot : Menu et TieredMenu, posés dans la page.</summary>
    <TestFixture>
    Public Class Lot7Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(600, 400))
            element.Arrange(New Rect(0, 0, 600, 400))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        <Test>
        Public Sub Menu_SesGroupesEtLaColonneDIcone()
            Dim menu As New MenuDeMkm()
            Dim groupe As New MenuGroup With {.Header = "Plan"}
            Dim ouvrir As New MenuItem With {.Header = "Ouvrir", .Icon = Icons.Open}
            Dim trait As New Separator()
            groupe.Items.Add(ouvrir)
            groupe.Items.Add(trait)
            groupe.Items.Add(New MenuItem With {.Header = "Exporter", .Icon = Icons.Export})
            menu.Items.Add(groupe)
            Dim reglages As New MenuItem With {.Header = "Paramètres"}
            menu.Items.Add(reglages)
            Pret(menu)

            Assert.That(MenuIcons.GetHasIcons(reglages), [Is].True, "sans icône, il garde la colonne : un autre en a une")
            Assert.That(ouvrir.Role, [Is].EqualTo(MenuItemRole.SubmenuItem))
            Assert.That(trait.Style, [Is].SameAs(Application.Current.TryFindResource(MenuItem.SeparatorStyleKey)), "le trait des menus")
            Assert.That(Partie(Of UIElement)(groupe, "Libelle").Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub Menu_SansIcone_SansColonne()
            Dim menu As New MenuDeMkm()
            Dim seul As New MenuItem With {.Header = "Ouvrir"}
            menu.Items.Add(seul)
            Pret(menu)

            Assert.That(MenuIcons.GetHasIcons(seul), [Is].False)
        End Sub

        <Test>
        Public Sub TieredMenu_LeSousMenuSOuvreADroite()
            Dim menu As New TieredMenu()
            Dim exporter As New MenuItem With {.Header = "Exporter", .Icon = Icons.Export}
            exporter.Items.Add(New MenuItem With {.Header = "PDF"})
            menu.Items.Add(exporter)
            Pret(menu)

            Assert.That(exporter.Role, [Is].EqualTo(MenuItemRole.SubmenuHeader), "pas l'en-tête d'une barre de menus")
            Assert.That(Partie(Of Popup)(exporter, "PART_Popup").Placement, [Is].EqualTo(PlacementMode.Right))
        End Sub

    End Class

End Namespace
