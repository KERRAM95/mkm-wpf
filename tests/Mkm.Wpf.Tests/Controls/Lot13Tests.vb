Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 13 : Breadcrumb, SplitButton, Popover.</summary>
    <TestFixture>
    Public Class Lot13Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 600, Optional hauteur As Double = 200) As T
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

        ' Ce que le Dispatcher a en attente (un Invoke d'automatisation passe par lui).
        Private Shared Sub Vider()
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, New System.Action(Sub()
                                                                                                End Sub))
        End Sub

        ' ---- Breadcrumb ----

        Private Shared Function Chemin() As Breadcrumb
            Dim fil As New Breadcrumb()
            fil.Items.Add(New BreadcrumbItem With {.Icon = Icons.Home, .ToolTip = "Accueil"})
            fil.Items.Add(New BreadcrumbItem With {.Content = "Projets"})
            fil.Items.Add(New BreadcrumbItem With {.Content = "HS-00042"})
            Return Pret(fil)
        End Function

        <Test>
        Public Sub Breadcrumb_LePremierLienSansChevron()
            Dim fil = Chemin()
            Dim accueil = DirectCast(fil.Items(0), BreadcrumbItem)
            Dim projets = DirectCast(fil.Items(1), BreadcrumbItem)
            Dim dernier = DirectCast(fil.Items(2), BreadcrumbItem)

            Assert.That(accueil.IsFirst AndAlso dernier.IsLast, [Is].True)
            Assert.That(Partie(Of Icon)(accueil, "Separateur").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of Icon)(projets, "Separateur").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of ContentPresenter)(accueil, "Libelle").Visibility, [Is].EqualTo(Visibility.Collapsed), "l'accueil, une icône seule")
            Assert.That(Partie(Of Icon)(projets, "Icone").Visibility, [Is].EqualTo(Visibility.Collapsed), "sans icône, le libellé seul")
        End Sub

        <Test>
        Public Sub BreadcrumbItem_UnLienNommeQuiSeSuit()
            Dim fil = Chemin()
            Dim accueil = DirectCast(fil.Items(0), BreadcrumbItem)
            Dim suivi As Object = Nothing
            fil.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(Sub(s, e) suivi = e.OriginalSource))

            Dim pair = UIElementAutomationPeer.CreatePeerForElement(accueil)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Hyperlink))
            Assert.That(pair.GetName(), [Is].EqualTo("Accueil"), "une icône seule, nommée par son info-bulle")
            DirectCast(pair.GetPattern(PatternInterface.Invoke), IInvokeProvider).Invoke()
            Vider()
            Assert.That(suivi, [Is].SameAs(accueil), "son clic remonte au fil d'Ariane")
        End Sub

        ' ---- SplitButton ----

        <Test>
        Public Sub SplitButton_LePrincipalEtLeChevronAccoles()
            Dim partage = Pret(New SplitButton With {.Content = "Enregistrer"})
            partage.Items.Add(New MenuItem With {.Header = "Enregistrer sous…"})
            Dim principal = Partie(Of Mkm.Wpf.Controls.Button)(partage, "PART_Button")
            Dim chevron = Partie(Of Mkm.Wpf.Controls.Button)(partage, "PART_DropDown")

            Assert.That(principal.Content, [Is].EqualTo("Enregistrer"))
            Assert.That(principal.BorderThickness.Right, [Is].EqualTo(0), "sans bordure à droite")
            Assert.That(principal.CornerRadius.TopRight, [Is].EqualTo(0))
            Assert.That(principal.CornerRadius.TopLeft, [Is].GreaterThan(0))
            Assert.That(chevron.CornerRadius.TopLeft, [Is].EqualTo(0))
            Assert.That(chevron.IsIconOnly, [Is].True)
            Assert.That(partage.Items.Count, [Is].EqualTo(1), "ses éléments sont ceux du menu")
            Assert.That(partage.DropDownMenu.Items.Count, [Is].EqualTo(1))
        End Sub

        <Test>
        Public Sub SplitButton_SonClicEstCeluiDuPrincipal()
            Dim partage = Pret(New SplitButton With {.Content = "Enregistrer"})
            Dim clics = 0
            AddHandler partage.Click, Sub() clics += 1
            Dim principal = Partie(Of ButtonBase)(partage, "PART_Button")
            Dim chevron = Partie(Of ButtonBase)(partage, "PART_DropDown")

            principal.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, principal))
            Assert.That(clics, [Is].EqualTo(1))
            chevron.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, chevron))
            Assert.That(clics, [Is].EqualTo(1), "le chevron n'est pas l'action principale")
            Assert.That(partage.IsDropDownOpen, [Is].False, "sans élément, pas de menu")
        End Sub

        <Test>
        Public Sub SplitButton_LeChevronSeNommeEtSeDitReduit()
            Dim partage = Pret(New SplitButton With {.Content = "Enregistrer"})
            Dim chevron = Partie(Of ButtonBase)(partage, "PART_DropDown")
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(chevron)

            Assert.That(pair.GetName(), [Is].EqualTo(Mkm.Wpf.Localization.Localizer.Current.T("aria.moreCommands")))
            Dim depli = DirectCast(pair.GetPattern(PatternInterface.ExpandCollapse), IExpandCollapseProvider)
            Assert.That(depli.ExpandCollapseState, [Is].EqualTo(System.Windows.Automation.ExpandCollapseState.Collapsed))

            Dim groupe = UIElementAutomationPeer.CreatePeerForElement(partage)
            Assert.That(groupe.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Group))
            Assert.That(groupe.GetName(), [Is].EqualTo("Enregistrer"))
        End Sub

        <Test>
        Public Sub SplitButton_LeClicDUneActionRemonteAuBoutonPartage()
            Dim partage = Pret(New SplitButton With {.Content = "Enregistrer"})
            Dim action As New MenuItem With {.Header = "Enregistrer sous…"}
            partage.Items.Add(action)
            Dim recu As Object = Nothing
            partage.AddHandler(MenuItem.ClickEvent, New RoutedEventHandler(Sub(s, e) recu = e.OriginalSource))

            action.RaiseEvent(New RoutedEventArgs(MenuItem.ClickEvent, action))
            Assert.That(recu, [Is].SameAs(action))
        End Sub

        ' ---- Popover ----

        <Test>
        Public Sub Popover_NePrendPasDePlaceDansLaPage()
            Dim fenetre = Pret(New Popover With {.Content = New TextBlock With {.Text = "Lien du document"}})
            Assert.That(fenetre.DesiredSize.Width, [Is].EqualTo(0))
            Assert.That(fenetre.DesiredSize.Height, [Is].EqualTo(0))
            Assert.That(fenetre.IsOpen, [Is].False)
            Assert.That(Partie(Of Popup)(fenetre, "PART_Popup").StaysOpen, [Is].False, "un clic ailleurs le ferme")
        End Sub

        <Test>
        Public Sub Popover_ToggleLOuvrePuisLeFerme()
            Dim cible As New Border With {.Width = 80, .Height = 24}
            Dim fenetre = Pret(New Popover With {.Content = New TextBlock With {.Text = "Lien du document"}})
            Try
                fenetre.Toggle(cible)
                Assert.That(fenetre.IsOpen, [Is].True)
                Assert.That(fenetre.Target, [Is].SameAs(cible))
                ' Hors d'une fenêtre visible, WPF garde la fenêtre flottante fermée : les tests d'interaction l'ouvrent.
                fenetre.Toggle()
                Assert.That(fenetre.IsOpen, [Is].False)
            Finally
                fenetre.Hide()
            End Try
        End Sub

    End Class

End Namespace
