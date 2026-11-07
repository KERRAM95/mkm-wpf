Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Shapes
Imports System.Windows.Threading
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Theming
Imports NUnit.Framework
Imports Barre = Mkm.Wpf.Controls.Toolbar
Imports Bascule = Mkm.Wpf.Controls.ToggleButton
Imports Radio = Mkm.Wpf.Controls.RadioButton

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les composants du cinquième lot : RadioButton, ToggleButton et Toolbar.</summary>
    <TestFixture>
    Public Class Lot5Tests

        Private NotInheritable Class Compteur
            Implements ICommand

            Public Property Fois As Integer

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return True
            End Function

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                Fois += 1
            End Sub

        End Class

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 600) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            MettreEnPage(element, largeur)
            Return element
        End Function

        Private Shared Sub MettreEnPage(element As FrameworkElement, largeur As Double)
            element.Measure(New Size(largeur, 300))
            element.Arrange(New Rect(0, 0, largeur, 300))
            element.UpdateLayout()
        End Sub

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        Private Shared Function Ressource(cle As String) As Object
            Return Application.Current.TryFindResource(ThemeResources.Prefix & cle)
        End Function

        ' Ce que la file du répartiteur attend encore (le lancement d'une commande par l'automatisation, par exemple).
        Private Shared Sub Vider()
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, New Action(Sub()
                                                                                          End Sub))
        End Sub

        <Test>
        Public Sub RadioButton_UnSeulChoixParGroupe()
            Dim plan As New Radio With {.GroupName = "vue", .Content = "Plan"}
            Dim article As New Radio With {.GroupName = "vue", .Content = "Article"}
            Dim groupe As New StackPanel()
            groupe.Children.Add(plan)
            groupe.Children.Add(article)
            Pret(groupe)

            plan.IsChecked = True
            article.IsChecked = True

            Assert.That(plan.IsChecked, [Is].False)
            Assert.That(Partie(Of UIElement)(article, "Point").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of UIElement)(plan, "Point").Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub RadioButton_TailleEtRefus()
            Dim radio = Pret(New Radio With {.Size = RadioButtonSize.Small, .Invalid = True})

            Assert.That(Partie(Of FrameworkElement)(radio, "Cadre").ActualWidth, [Is].EqualTo(12))
            Assert.That(Partie(Of Shape)(radio, "Boite").Stroke, [Is].SameAs(Ressource("radiobutton.invalid.border.color")))
            Assert.That(Partie(Of UIElement)(radio, "Ecart").Visibility, [Is].EqualTo(Visibility.Collapsed), "sans libellé, pas d'écart")
        End Sub

        <Test>
        Public Sub ToggleButton_LibelleEtIconeSuiventLEtat()
            Dim bouton = Pret(New Bascule With {.OnLabel = "Verrouillé", .OffLabel = "Déverrouillé", .OnIcon = Icons.Locked, .OffIcon = Icons.Unlocked})
            Assert.That(bouton.CurrentLabel, [Is].EqualTo("Déverrouillé"))
            Assert.That(bouton.CurrentIcon, [Is].SameAs(Icons.Unlocked))

            bouton.IsChecked = True

            Assert.That(bouton.CurrentLabel, [Is].EqualTo("Verrouillé"))
            Assert.That(bouton.CurrentIcon, [Is].SameAs(Icons.Locked))
            Assert.That(bouton.Background, [Is].SameAs(Ressource("togglebutton.checked.background")))
            Assert.That(bouton.BorderBrush, [Is].SameAs(Ressource("togglebutton.checked.border.color")))

            ' Un contenu donné remplace les libellés de l'état.
            bouton.Content = "Verrou"
            Assert.That(bouton.CurrentLabel, [Is].EqualTo("Verrou"))
        End Sub

        <Test>
        Public Sub ToggleButton_SansIconeRelachee_CelleDuBoutonEnfonce()
            Dim bouton = Pret(New Bascule With {.OnIcon = Icons.Pin})

            Assert.That(bouton.CurrentIcon, [Is].SameAs(Icons.Pin))
            Assert.That(bouton.CurrentLabel, [Is].Null)
            Assert.That(Partie(Of UIElement)(bouton, "Libelle").Visibility, [Is].EqualTo(Visibility.Collapsed), "d'icône seule")
        End Sub

        <Test>
        Public Sub ToolbarButton_SecondaireParDefaut()
            Dim commande = Pret(New ToolbarButton With {.Content = "Ajouter"})

            Assert.That(commande.Severity, [Is].EqualTo(ButtonSeverity.Secondary))
            Assert.That(commande.Background, [Is].SameAs(Ressource("button.secondary.background")))
        End Sub

        Private Shared Function BarreDe(nombre As Integer, Optional avecTrait As Integer = -1) As Barre
            Dim barre As New Barre With {.Overflow = True}
            For i = 1 To nombre
                If i = avecTrait Then barre.Items.Add(New ToolbarSeparator())
                barre.Items.Add(New ToolbarButton With {.Content = "Commande " & i, .Icon = Icons.Add})
            Next
            Return barre
        End Function

        Private Shared Function Conteneurs(barre As Barre) As UIElement()
            Return Enumerable.Range(0, barre.Items.Count).
                Select(Function(i) DirectCast(barre.ItemContainerGenerator.ContainerFromIndex(i), UIElement)).
                ToArray()
        End Function

        <Test>
        Public Sub Toolbar_SesCommandesDansDesPresentateurs()
            Dim barre = Pret(BarreDe(2))

            Assert.That(Conteneurs(barre), [Has].All.InstanceOf(Of ContentPresenter)())
            Assert.That(barre.IsOverflowing, [Is].False)
            Assert.That(Partie(Of UIElement)(barre, "PART_OverflowHost").Visibility, [Is].EqualTo(Visibility.Hidden))
        End Sub

        <Test>
        Public Sub Toolbar_LesCommandesQuiNeTiennentPas_PassentAuMenu()
            Dim barre = Pret(BarreDe(8), 300)

            Assert.That(barre.IsOverflowing, [Is].True)
            Assert.That(Partie(Of UIElement)(barre, "PART_OverflowHost").Visibility, [Is].EqualTo(Visibility.Visible))
            Dim cachees = barre.CommandesCachees()
            Assert.That(cachees, [Is].Not.Empty)
            ' Dans l'ordre : la première qui déborde passe au menu avec toutes celles qui la suivent.
            Assert.That(cachees.Last(), [Is].SameAs(barre.Items(barre.Items.Count - 1)))
            Dim choix = barre.ChoixDuMenu().OfType(Of MenuItem)().ToList()
            Assert.That(choix.Count, [Is].EqualTo(cachees.Count))
            Assert.That(choix.First().Header, [Is].EqualTo(DirectCast(cachees.First(), ToolbarButton).Content))
            Assert.That(choix.First().Icon, [Is].SameAs(Icons.Add))

            ' Assez de place : toutes reviennent, le bouton « … » s'en va.
            MettreEnPage(barre, 2000)

            Assert.That(barre.IsOverflowing, [Is].False)
            Assert.That(barre.CommandesCachees(), [Is].Empty)
        End Sub

        <Test>
        Public Sub Toolbar_UnTraitNeFinitPasLaRangee()
            ' Le trait avant la troisième commande : si elle déborde, il ne reste pas en bout de rangée.
            Dim barre = BarreDe(4, avecTrait:=3)
            Pret(barre, 10)
            Dim premier = barre.Items.OfType(Of ToolbarButton)().First()
            Dim trait = barre.Items.OfType(Of ToolbarSeparator)().Single()
            Dim largeur = DirectCast(barre.ItemContainerGenerator.ContainerFromItem(premier), UIElement).DesiredSize.Width
            Dim largeurDuTrait = DirectCast(barre.ItemContainerGenerator.ContainerFromItem(trait), UIElement).DesiredSize.Width
            Assert.That(largeurDuTrait, [Is].EqualTo(9), "1 px et 4 px de chaque côté")
            ' Assez pour deux commandes et le trait, pas pour la troisième commande.
            MettreEnPage(barre, largeur * 2 + largeurDuTrait + barre.LargeurDuDebordement + 2)

            Assert.That(barre.IsOverflowing, [Is].True)
            Assert.That(barre.CommandesCachees(), [Does].Contain(trait))
            Assert.That(barre.CommandesCachees().Count, [Is].EqualTo(3), "le trait et les deux dernières commandes")
            Assert.That(barre.ChoixDuMenu().First(), [Is].InstanceOf(Of MenuItem)(), "le menu ne commence pas par un trait")
        End Sub

        <Test>
        Public Sub Toolbar_LeMenuLanceLaCommande()
            Dim compteur As New Compteur()
            Dim barre = BarreDe(6)
            Dim derniere = barre.Items.OfType(Of ToolbarButton)().Last()
            derniere.Command = compteur
            Dim grisee = barre.Items.OfType(Of ToolbarButton)().Reverse().Skip(1).First()
            grisee.IsEnabled = False
            Pret(barre, 250)

            Dim choix = barre.ChoixDuMenu().OfType(Of MenuItem)().ToList()
            Assert.That(choix.Last().IsEnabled, [Is].True)
            Assert.That(choix(choix.Count - 2).IsEnabled, [Is].False, "une commande grisée l'est aussi dans le menu")

            choix.Last().RaiseEvent(New RoutedEventArgs(MenuItem.ClickEvent))
            Vider()

            Assert.That(compteur.Fois, [Is].EqualTo(1))
        End Sub

        <Test>
        Public Sub Toolbar_SansDebordement_PasseALaLigne()
            Dim barre = BarreDe(8)
            barre.Overflow = False
            Pret(barre, 300)

            Assert.That(barre.IsOverflowing, [Is].False)
            Assert.That(barre.CommandesCachees(), [Is].Empty)
            Assert.That(barre.ActualHeight, [Is].GreaterThan(2 * 24 - 1), "plusieurs lignes")
        End Sub

    End Class

End Namespace
