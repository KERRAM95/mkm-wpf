Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 17 : TreeSelect, CascadeSelect.</summary>
    <TestFixture>
    Public Class Lot17Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 300, Optional hauteur As Double = 40) As T
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

        Public Class Dossier
            Public Property Nom As String
            Public Property Enfants As New ObservableCollection(Of Dossier)
        End Class

        ' ---- TreeSelect ----

        Private Shared Function Dossiers(Optional plans As TreeNode = Nothing) As TreeSelect
            Dim champ As New TreeSelect With {.Placeholder = "Choisir un dossier"}
            Dim documents As New TreeNode With {.Header = "Documents"}
            documents.Items.Add(If(plans, New TreeNode With {.Header = "Plans"}))
            champ.Items.Add(documents)
            champ.Items.Add(New TreeNode With {.Header = "Archives"})
            Return Pret(champ)
        End Function

        <Test>
        Public Sub TreeSelect_LeChampEcritLeTitreDuNoeudChoisi()
            Dim plans As New TreeNode With {.Header = "Plans"}
            Dim champ = Dossiers(plans)
            Assert.That(champ.HasSelection, [Is].False)
            Assert.That(champ.IsEmpty, [Is].False)

            Dim changements As New Collections.Generic.List(Of Object)
            AddHandler champ.SelectedItemChanged, Sub(s, e) changements.Add(e.NewValue)
            champ.Choisir(plans)
            Assert.That(champ.SelectedItem, [Is].SameAs(plans))
            Assert.That(champ.SelectionText, [Is].EqualTo("Plans"))
            Assert.That(champ.HasSelection, [Is].True)
            Assert.That(champ.IsDropDownOpen, [Is].False, "choisir referme l'arbre")
            Assert.That(changements, [Is].EqualTo({plans}))
        End Sub

        <Test>
        Public Sub TreeSelect_LaCroixRetireLeChoix()
            Dim plans As New TreeNode With {.Header = "Plans"}
            Dim champ = Dossiers(plans)
            champ.ShowClear = True
            Dim croix = Partie(Of ButtonBase)(champ, "PART_Clear")
            Assert.That(croix.Visibility, [Is].EqualTo(Visibility.Collapsed), "sans choix, pas de croix")
            champ.SelectedItem = plans
            champ.UpdateLayout()
            Assert.That(croix.Visibility, [Is].EqualTo(Visibility.Visible))
            croix.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, croix))
            Assert.That(champ.SelectedItem, [Is].Null)
            Assert.That(champ.SelectionText, [Is].Empty)
        End Sub

        <Test>
        Public Sub TreeSelect_LieADesDonneesEcritLaProprieteDite()
            Dim racine As New Dossier With {.Nom = "Documents"}
            racine.Enfants.Add(New Dossier With {.Nom = "Plans"})
            Dim champ = Pret(New TreeSelect With {.DisplayMemberPath = "Nom",
                                                  .ItemsSource = New ObservableCollection(Of Dossier) From {racine}})
            Assert.That(champ.Arbre.Items.Count, [Is].EqualTo(1))
            champ.SelectedItem = racine.Enfants(0)
            Assert.That(champ.SelectionText, [Is].EqualTo("Plans"))
        End Sub

        <Test>
        Public Sub TreeSelect_SansNoeudDitAucunResultat()
            Dim champ = Pret(New TreeSelect())
            Assert.That(champ.IsEmpty, [Is].True)
        End Sub

        <Test>
        Public Sub TreeSelect_UneListeDeroulantePourLesLecteursDEcran()
            Dim plans As New TreeNode With {.Header = "Plans"}
            Dim champ = Dossiers(plans)
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(champ)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.ComboBox))
            Assert.That(pair.GetName(), [Is].EqualTo("Choisir un dossier"), "nommé par son texte d'aide")
            champ.SelectedItem = plans
            Dim valeur = DirectCast(pair.GetPattern(PatternInterface.Value), IValueProvider)
            Assert.That(valeur.Value, [Is].EqualTo("Plans"))
            Dim depliage = DirectCast(pair.GetPattern(PatternInterface.ExpandCollapse), IExpandCollapseProvider)
            depliage.Expand()
            Assert.That(champ.IsDropDownOpen, [Is].True)
            depliage.Collapse()
            Assert.That(champ.IsDropDownOpen, [Is].False)
        End Sub

        <Test>
        Public Sub TreeSelect_LArbreEstEnfantLogiqueDuChamp()
            Dim champ = Dossiers()
            champ.DataContext = "contexte"
            Assert.That(LogicalTreeHelper.GetParent(champ.Arbre), [Is].SameAs(champ))
            Assert.That(champ.Arbre.DataContext, [Is].EqualTo("contexte"), "l'arbre hérite du contexte du champ")
        End Sub

        ' ---- CascadeSelect ----

        Private Shared Function Machines(Optional tour As CascadeSelectItem = Nothing) As CascadeSelect
            Dim champ As New CascadeSelect With {.Placeholder = "Choisir une machine"}
            Dim lyon As New CascadeSelectItem With {.Header = "Lyon"}
            lyon.Items.Add(If(tour, New CascadeSelectItem With {.Header = "Tour CN 1"}))
            champ.Items.Add(lyon)
            Return Pret(champ)
        End Function

        <Test>
        Public Sub CascadeSelect_UneOptionSansSuiteSeChoisitAuClic()
            Dim tour As New CascadeSelectItem With {.Header = "Tour CN 1"}
            Dim champ = Machines(tour)
            Dim changements As New Collections.Generic.List(Of Object)
            AddHandler champ.SelectedItemChanged, Sub(s, e) changements.Add(e.NewValue)

            tour.RaiseEvent(New RoutedEventArgs(MenuItem.ClickEvent, tour))
            Assert.That(champ.SelectedItem, [Is].SameAs(tour))
            Assert.That(champ.SelectionText, [Is].EqualTo("Tour CN 1"))
            Assert.That(changements, [Is].EqualTo({tour}))

            Dim lyon = DirectCast(champ.Items(0), CascadeSelectItem)
            lyon.RaiseEvent(New RoutedEventArgs(MenuItem.ClickEvent, lyon))
            Assert.That(champ.SelectedItem, [Is].SameAs(tour), "une option qui a une suite ne se choisit pas")
            Assert.That(CascadeSelect.DonneeDe(tour), [Is].SameAs(tour), "écrite dans le XAML, l'option est sa donnée")
        End Sub

        <Test>
        Public Sub CascadeSelect_LieADesDonneesChoisitLaDonnee()
            Dim racine As New Dossier With {.Nom = "Lyon"}
            racine.Enfants.Add(New Dossier With {.Nom = "Usinage"})
            Dim champ = Pret(New CascadeSelect With {.DisplayMemberPath = "Nom",
                                                     .ItemsSource = New ObservableCollection(Of Dossier) From {racine}})
            Assert.That(champ.Liste.Items.Count, [Is].EqualTo(1))
            champ.Choisir(racine.Enfants(0))
            Assert.That(champ.SelectionText, [Is].EqualTo("Usinage"))
        End Sub

        <Test>
        Public Sub CascadeSelect_UneListeDeroulantePourLesLecteursDEcran()
            Dim tour As New CascadeSelectItem With {.Header = "Tour CN 1"}
            Dim champ = Machines(tour)
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(champ)
            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.ComboBox))
            Assert.That(pair.GetName(), [Is].EqualTo("Choisir une machine"))
            champ.SelectedItem = tour
            Assert.That(DirectCast(pair.GetPattern(PatternInterface.Value), IValueProvider).Value, [Is].EqualTo("Tour CN 1"))
        End Sub

    End Class

End Namespace
