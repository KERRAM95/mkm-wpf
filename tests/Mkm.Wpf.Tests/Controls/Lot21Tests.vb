Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 21 : Menubar, MegaMenu, PanelMenu.</summary>
    <TestFixture>
    Public Class Lot21Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 500, Optional hauteur As Double = 300) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        ' ---- Les éléments de menu ----

        <Test>
        Public Sub ElementDeMenu_SonSurlignageSuitLArrondiDuTheme()
            Dim element As New MenuItem With {.Header = "Information"}
            Dim menu As New Mkm.Wpf.Controls.Menu()
            menu.Items.Add(element)
            Pret(menu)
            element.ApplyTemplate()
            Dim fond = DirectCast(element.Template.FindName("Fond", element), Border)
            Assert.That(fond.CornerRadius, [Is].EqualTo(New CornerRadius(0)), "aucun arrondi dans le thème, comme mkm")

            menu.Resources("mkm.contextmenu.item.border.radius") = New CornerRadius(6)
            menu.UpdateLayout()

            Assert.That(fond.CornerRadius, [Is].EqualTo(New CornerRadius(6)), "une application peut l'arrondir")
        End Sub

        ' ---- Menubar ----

        <Test>
        Public Sub Menubar_SesElementsPrennentLeStyleDeLaBarre()
            Dim fichier As New MenuItem With {.Header = "Fichier"}
            fichier.Items.Add(New MenuItem With {.Header = "Ouvrir"})
            Dim barre As New Menubar()
            barre.Items.Add(fichier)
            Pret(barre)
            Assert.That(fichier.Style, [Is].SameAs(barre.ItemContainerStyle), "le style d'élément de la barre")
            Assert.That(fichier.Role, [Is].EqualTo(MenuItemRole.TopLevelHeader))
            Assert.That(ReferenceEquals(DirectCast(fichier.Items(0), MenuItem).Style, barre.ItemContainerStyle), [Is].False, "le sous-menu garde le menu de mkm")
        End Sub

        ' ---- MegaMenu ----

        Private Shared Function Mega(lien As MenuItem) As MegaMenu
            Dim groupe As New MenuGroup With {.Header = "Pièces"}
            groupe.Items.Add(lien)
            Dim colonne As New MegaMenuColumn()
            colonne.Items.Add(groupe)
            Dim produits As New MegaMenuItem With {.Header = "Produits"}
            produits.Items.Add(colonne)
            Dim menu As New MegaMenu()
            menu.Items.Add(produits)
            Return Pret(menu)
        End Function

        <Test>
        Public Sub MegaMenu_UnLienChoisiRefermeLePanneau()
            Dim lien As New MenuItem With {.Header = "Usinées"}
            Dim menu = Mega(lien)
            Dim produits = DirectCast(menu.Items(0), MegaMenuItem)
            Assert.That(produits.Role, [Is].EqualTo(MenuItemRole.TopLevelHeader))
            produits.IsSubmenuOpen = True
            lien.RaiseEvent(New RoutedEventArgs(MenuItem.ClickEvent, lien))
            Assert.That(produits.IsSubmenuOpen, [Is].False)
        End Sub

        <Test>
        Public Sub MegaMenu_LaColonneDIconesSelonLesLiens()
            Dim sansIcone = Mega(New MenuItem With {.Header = "Usinées"})
            Assert.That(MenuIcons.GetHasIcons(DirectCast(sansIcone.Items(0), MegaMenuItem)), [Is].False)
            Dim avecIcone = Mega(New MenuItem With {.Header = "Usinées", .Icon = Icons.Settings})
            DirectCast(avecIcone.Items(0), MegaMenuItem).RaiseEvent(New RoutedEventArgs(FrameworkElement.LoadedEvent))
            Assert.That(MenuIcons.GetHasIcons(DirectCast(avecIcone.Items(0), MegaMenuItem)), [Is].True)
        End Sub

        <Test>
        Public Sub MegaMenu_EnColonneLesElementsLeSavent()
            Dim menu = Mega(New MenuItem With {.Header = "Usinées"})
            menu.Orientation = Orientation.Vertical
            Assert.That(MegaMenu.GetOrientation(DirectCast(menu.Items(0), MegaMenuItem)), [Is].EqualTo(Orientation.Vertical), "héritée")
        End Sub

        ' ---- PanelMenu ----

        Private Shared Function Navigation(Optional multiple As Boolean = False) As PanelMenu
            Dim menu As New PanelMenu With {.Multiple = multiple}
            For Each titre In {"Documents", "Ateliers", "Statistiques"}
                Dim panneau As New PanelMenuItem With {.Header = titre}
                If titre <> "Statistiques" Then panneau.Items.Add(New PanelMenuItem With {.Header = titre & " 1"})
                menu.Items.Add(panneau)
            Next
            Return Pret(menu)
        End Function

        Private Shared Function Panneau(menu As PanelMenu, i As Integer) As PanelMenuItem
            Return DirectCast(menu.Items(i), PanelMenuItem)
        End Function

        <Test>
        Public Sub PanelMenu_UnSeulPanneauDeplie()
            Dim menu = Navigation()
            Panneau(menu, 0).Activer()
            Assert.That(Panneau(menu, 0).IsExpanded, [Is].True)
            Panneau(menu, 1).Activer()
            Assert.That(Panneau(menu, 1).IsExpanded, [Is].True)
            Assert.That(Panneau(menu, 0).IsExpanded, [Is].False, "l'autre se replie")

            Dim plusieurs = Navigation(multiple:=True)
            Panneau(plusieurs, 0).IsExpanded = True
            Panneau(plusieurs, 1).IsExpanded = True
            Assert.That(Panneau(plusieurs, 0).IsExpanded, [Is].True, "Multiple les garde")
        End Sub

        <Test>
        Public Sub PanelMenu_LesPanneauxSaventLeurPlaceEtLeurNiveau()
            Dim menu = Navigation()
            Assert.That(Panneau(menu, 0).IsFirst AndAlso Panneau(menu, 2).IsLast, [Is].True)
            Assert.That(Panneau(menu, 1).IsFirst OrElse Panneau(menu, 1).IsLast, [Is].False)
            Assert.That(Panneau(menu, 0).Level, [Is].EqualTo(0))
            Panneau(menu, 0).IsExpanded = True
            menu.UpdateLayout()
            Assert.That(DirectCast(Panneau(menu, 0).Items(0), PanelMenuItem).Level, [Is].EqualTo(1))
        End Sub

        <Test>
        Public Sub PanelMenu_UnElementSansSuiteSeLance()
            Dim menu = Navigation()
            Dim statistiques = Panneau(menu, 2)
            Dim lances = 0
            AddHandler statistiques.Click, Sub(s, e) lances += 1
            statistiques.Activer()
            Assert.That(lances, [Is].EqualTo(1))
            Assert.That(statistiques.IsExpanded, [Is].False)
        End Sub

        <Test>
        Public Sub PanelMenu_LEnTeteSeDeplieMemePourLesLecteursDEcran()
            Dim menu = Navigation()
            Dim entete = DirectCast(Panneau(menu, 0).Template.FindName("PART_Header", Panneau(menu, 0)), UIElement)
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(entete)
            Assert.That(pair.GetName(), [Is].EqualTo("Documents"))
            Dim depli = DirectCast(pair.GetPattern(PatternInterface.ExpandCollapse), IExpandCollapseProvider)
            Assert.That(depli.ExpandCollapseState, [Is].EqualTo(ExpandCollapseState.Collapsed))
            depli.Expand()
            Assert.That(Panneau(menu, 0).IsExpanded, [Is].True)

            Dim feuille = DirectCast(Panneau(menu, 2).Template.FindName("PART_Header", Panneau(menu, 2)), UIElement)
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(feuille).GetPattern(PatternInterface.ExpandCollapse), [Is].Null, "sans suite, un simple bouton")
        End Sub

    End Class

End Namespace
