Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les composants du sixième lot : la mise en page (Splitter, Triptych, WelcomePanel, EmptyContent).</summary>
    <TestFixture>
    Public Class Lot6Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 1000, Optional hauteur As Double = 400) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            MettreEnPage(element, largeur, hauteur)
            Return element
        End Function

        Private Shared Sub MettreEnPage(element As FrameworkElement, largeur As Double, Optional hauteur As Double = 400)
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
        End Sub

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        Private Shared Sub Cliquer(bouton As ButtonBase)
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(bouton)
            DirectCast(pair.GetPattern(PatternInterface.Invoke), IInvokeProvider).Invoke()
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, New Action(Sub()
                                                                                          End Sub))
        End Sub

        <Test>
        Public Sub GapStackPanel_LEcartEntreLesVisiblesSeulement()
            Dim pile As New GapStackPanel With {.Gap = 8}
            pile.Children.Add(New Border With {.Height = 10})
            pile.Children.Add(New Border With {.Height = 10, .Visibility = Visibility.Collapsed})
            pile.Children.Add(New Border With {.Height = 10})
            Pret(pile)

            Assert.That(pile.DesiredSize.Height, [Is].EqualTo(28))
        End Sub

        <Test>
        Public Sub EmptyContent_UnOuEntreDeuxPossibilites()
            Dim ecran As New EmptyContent With {.Title = "Aucun plan", .Message = "Ouvrez un plan.", .Size = EmptyContentSize.Small}
            Dim ouvrir As New EmptyContentItem With {.Icon = Icons.Open, .Content = "Ouvrez un plan existant"}
            Dim creer As New EmptyContentItem With {.Icon = Icons.Add, .Content = "Créez-en un"}
            ecran.Items.Add(ouvrir)
            ecran.Items.Add(creer)
            Pret(ecran)

            Assert.That(ouvrir.IsLast, [Is].False)
            Assert.That(creer.IsLast, [Is].True)
            Assert.That(Partie(Of UIElement)(ouvrir, "Ou").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(ouvrir.Size, [Is].EqualTo(EmptyContentSize.Small), "la taille, héritée de l'écran vide")
            Assert.That(Partie(Of UIElement)(ecran, "Icone").Visibility, [Is].EqualTo(Visibility.Collapsed), "sans icône")
            Assert.That(Partie(Of UIElement)(ecran, "LesActions").Visibility, [Is].EqualTo(Visibility.Collapsed), "sans actions")
        End Sub

        Private Shared Function Separateur(ParamArray tailles As Double()) As Splitter
            Dim s As New Splitter()
            For Each taille In tailles
                s.Panels.Add(New SplitterPanel With {.Size = taille, .Content = New TextBlock With {.Text = "Panneau"}})
            Next
            Return s
        End Function

        <Test>
        Public Sub Splitter_LesPartsEtLesGouttieres()
            Dim s = Pret(Separateur(Double.NaN, Double.NaN), largeur:=406)
            Dim disposition = Partie(Of SplitterLayout)(s, "PART_Layout")

            Assert.That(s.GetSizes(), [Is].EqualTo({50.0, 50.0}))
            Assert.That(disposition.Gouttieres.Count, [Is].EqualTo(1))
            ' 406 - 2 de bordure - 4 de gouttière : 200 chacun.
            Assert.That(s.Panels(0).ActualWidth, [Is].EqualTo(200).Within(0.01))
            Assert.That(s.Panels(1).ActualWidth, [Is].EqualTo(200).Within(0.01))
            Assert.That(LogicalTreeHelper.GetParent(s.Panels(0)), [Is].SameAs(s), "les panneaux, enfants logiques du séparateur")
        End Sub

        <Test>
        Public Sub Splitter_TirerGardeLesTaillesMinimales()
            Dim s = Pret(Separateur(30, 70))
            s.Panels(1).MinSize = 60

            s.Deplacer(0, 25)

            Assert.That(s.Panels(0).Size, [Is].EqualTo(40).Within(0.001), "le second panneau garde ses 60 %")
            Assert.That(s.Panels(1).Size, [Is].EqualTo(60).Within(0.001))
        End Sub

        <Test>
        Public Sub Splitter_Vertical()
            Dim s = Separateur(25, 75)
            s.Orientation = Orientation.Vertical
            Pret(s, 300, 406)

            Assert.That(s.Panels(0).ActualHeight, [Is].EqualTo(100).Within(0.01))
            Assert.That(s.Panels(1).ActualHeight, [Is].EqualTo(300).Within(0.01))
        End Sub

        Private Shared Function Panneau() As WelcomePanel
            Dim p As New WelcomePanel With {.Title = "HS CART", .Selection = "recents"}
            Dim plans As New WelcomePanelSection With {.Header = "Plans"}
            plans.Items.Add(New WelcomePanelItem With {.Key = "recents", .Icon = Icons.History, .Content = "Récents"})
            plans.Items.Add(New WelcomePanelItem With {.Key = "tous", .Icon = Icons.Drawings, .Content = "Tous les plans"})
            p.Items.Add(plans)
            p.Items.Add(New WelcomePanelSeparator())
            Dim actions As New WelcomePanelSection()
            actions.Items.Add(New WelcomePanelItem With {.Key = "sync", .Selectable = False, .Icon = Icons.Refresh, .Content = "Synchroniser"})
            p.Items.Add(actions)
            Return p
        End Function

        <Test>
        Public Sub WelcomePanel_LaSelectionEtLArretDeTabulation()
            Dim p = Pret(Panneau(), 260)
            Dim entrees = p.GetItems()

            Assert.That(entrees.Count, [Is].EqualTo(3))
            Assert.That(entrees(0).IsSelected, [Is].True)
            Assert.That(entrees.Where(Function(e) e.IsTabStop).Count(), [Is].EqualTo(1), "un seul arrêt de tabulation")
            Assert.That(entrees(0).IsTabStop, [Is].True, "l'entrée choisie")
        End Sub

        <Test>
        Public Sub WelcomePanel_UnClicChoisit_SaufUneAction()
            Dim p = Pret(Panneau(), 260)
            Dim entrees = p.GetItems()
            Dim cliquees As Integer
            AddHandler p.ItemClick, Sub(s, e) cliquees += 1

            Cliquer(entrees(1))
            Assert.That(p.Selection, [Is].EqualTo("tous"))
            Assert.That(entrees(1).IsSelected, [Is].True)
            Assert.That(entrees(0).IsSelected, [Is].False)

            Cliquer(entrees(2))
            Assert.That(p.Selection, [Is].EqualTo("tous"), "une action ne devient pas l'entrée choisie")
            Assert.That(cliquees, [Is].EqualTo(2))
        End Sub

        <Test>
        Public Sub WelcomePanel_Replie_LesIconesSeules()
            Dim p = Pret(Panneau(), 260)
            p.IsCollapsed = True
            p.UpdateLayout()
            Dim entree = p.GetItems()(0)

            Assert.That(entree.IsCollapsed, [Is].True, "hérité du panneau")
            Assert.That(Partie(Of UIElement)(entree, "Libelle").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(entree.ToolTip, [Is].EqualTo("Récents"), "le libellé en info-bulle")
            Assert.That(Partie(Of UIElement)(p, "EnTete").Visibility, [Is].EqualTo(Visibility.Collapsed), "sans en-tête")
        End Sub

        Private Shared Function MiseEnPage() As Triptych
            Return New Triptych With {
                .Left = New TextBlock With {.Text = "Gauche"},
                .Content = New TextBlock With {.Text = "Principal"},
                .Right = New TextBlock With {.Text = "Droite"}}
        End Function

        <Test>
        Public Sub Triptych_LesPanneauxPoussentLePrincipal()
            Dim t = Pret(MiseEnPage())
            Dim gauche = Partie(Of FrameworkElement)(t, "PART_Left")
            Dim principal = Partie(Of FrameworkElement)(t, "PART_Main")
            Assert.That(gauche.Width, [Is].EqualTo(0), "fermé")

            t.LeftVisible = True
            t.RightVisible = True
            t.UpdateLayout()

            Assert.That(gauche.Width, [Is].EqualTo(300))
            Assert.That(principal.Margin, [Is].EqualTo(New Thickness(300, 0, 300, 0)))
            Assert.That(Partie(Of UIElement)(t, "PART_RightGutter").Visibility, [Is].EqualTo(Visibility.Visible), "la droite se tire")
            Assert.That(Partie(Of UIElement)(t, "PART_LeftGutter").Visibility, [Is].EqualTo(Visibility.Collapsed), "pas la gauche")

            t.LeftCollapsed = True
            Assert.That(gauche.Width, [Is].EqualTo(40), "la bande")
            Assert.That(principal.Margin.Left, [Is].EqualTo(40))
        End Sub

        <Test>
        Public Sub Triptych_LaPoigneeGardeLaPlaceDuPrincipal()
            Dim t = MiseEnPage()
            t.LeftVisible = True
            t.RightVisible = True
            Pret(t)

            t.Dimensionner(TriptychSide.Right, 5000)

            ' 1000 - 300 à gauche - 150 entre deux panneaux.
            Assert.That(t.RightSize, [Is].EqualTo(550))
            t.Dimensionner(TriptychSide.Right, 10)
            Assert.That(t.RightSize, [Is].EqualTo(250), "sa taille minimale")
        End Sub

        <Test>
        Public Sub Triptych_EtroitEnSuperposition()
            Dim t = MiseEnPage()
            t.LeftVisible = True
            t.RightVisible = True
            Pret(t)

            MettreEnPage(t, 500)

            Assert.That(t.IsOverlay, [Is].True)
            Assert.That(t.RightVisible, [Is].False, "un seul panneau à la fois")
            Assert.That(Partie(Of FrameworkElement)(t, "PART_Main").Margin, [Is].EqualTo(New Thickness(0)))
            Assert.That(Partie(Of FrameworkElement)(t, "PART_Left").Width, [Is].EqualTo(450), "90 % de la largeur")
            Assert.That(Partie(Of UIElement)(t, "PART_Mask").Visibility, [Is].EqualTo(Visibility.Visible))

            ' La place revient : le panneau de droite aussi.
            MettreEnPage(t, 1000)
            Assert.That(t.IsOverlay, [Is].False)
            Assert.That(t.RightVisible, [Is].True)
        End Sub

        <Test>
        Public Sub Triptych_LaLanguette()
            Dim t = MiseEnPage()
            t.ShowLeftToggle = True
            Pret(t)
            Dim languette = Partie(Of ButtonBase)(t, "PART_LeftToggle")

            Cliquer(languette)
            Assert.That(t.LeftVisible, [Is].True, "fermé, il s'ouvre")
            Assert.That(t.IsLeftExpanded, [Is].True)

            Cliquer(languette)
            Assert.That(t.LeftCollapsed, [Is].True, "ouvert, il se replie")
            Assert.That(t.IsLeftExpanded, [Is].False)
        End Sub

    End Class

End Namespace
