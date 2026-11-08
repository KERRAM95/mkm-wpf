Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 14 : IconField, InputGroup, Listbox, MultiSelect, AutoComplete.</summary>
    <TestFixture>
    Public Class Lot14Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 400, Optional hauteur As Double = 300) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Options(ParamArray textes As String()) As ListBoxItem()
            Return textes.Select(Function(t) New ListBoxItem With {.Content = t}).ToArray()
        End Function

        ' ---- IconField ----

        <Test>
        Public Sub IconField_LeChampReculeSonTexteDuCoteDeLIcone()
            Dim champ As New InputText()
            Dim icone = Pret(New IconField With {.Icon = Icons.Search, .Content = champ})
            Assert.That(champ.Padding.Left, [Is].EqualTo(30), "deux retraits et l'icône")
            Assert.That(champ.Padding.Right, [Is].EqualTo(6))

            icone.IconPosition = IconFieldPosition.Right
            Assert.That(champ.Padding.Right, [Is].EqualTo(30))
            Assert.That(champ.Padding.Left, [Is].EqualTo(6))
        End Sub

        <Test>
        Public Sub IconField_LeRetraitPoseParLApplicationLEmporte()
            Dim champ As New InputText With {.Padding = New Thickness(2)}
            Pret(New IconField With {.Icon = Icons.Search, .Content = champ})
            Assert.That(champ.Padding.Left, [Is].EqualTo(2))
        End Sub

        ' ---- InputGroup ----

        <Test>
        Public Sub InputGroup_LesCoinsAuxBoutsEtLesBorduresDesComplements()
            Dim groupe As New InputGroup()
            Dim euro As New InputGroupAddon With {.Content = "€"}
            Dim prix As New InputText()
            Dim ht As New InputGroupAddon With {.Content = "HT"}
            groupe.Items.Add(euro)
            groupe.Items.Add(prix)
            groupe.Items.Add(ht)
            Pret(groupe)

            Assert.That(euro.CornerRadius.TopLeft, [Is].GreaterThan(0))
            Assert.That(euro.CornerRadius.TopRight, [Is].EqualTo(0))
            Assert.That(prix.CornerRadius, [Is].EqualTo(New CornerRadius(0)), "au milieu, sans coins")
            Assert.That(ht.CornerRadius.TopRight, [Is].GreaterThan(0))
            Assert.That(euro.HasStartBorder AndAlso Not euro.HasEndBorder, [Is].True)
            Assert.That(Not ht.HasStartBorder AndAlso ht.HasEndBorder, [Is].True, "après un champ, sans trait à gauche")
            Assert.That(euro.BorderThickness.Left, [Is].EqualTo(1))
            Assert.That(euro.BorderThickness.Right, [Is].EqualTo(0))
        End Sub

        <Test>
        Public Sub InputGroup_LeChampPrendLaLargeurQueLaissentLesComplements()
            Dim groupe As New InputGroup()
            Dim euro As New InputGroupAddon With {.Content = "€"}
            Dim prix As New InputText()
            groupe.Items.Add(euro)
            groupe.Items.Add(prix)
            Pret(groupe, 300, 24)
            Assert.That(euro.ActualWidth + prix.ActualWidth, [Is].EqualTo(300).Within(0.5))
            Assert.That(euro.ActualWidth, [Is].EqualTo(30).Within(0.5), "inputgroup.addon.min.width")
        End Sub

        ' ---- Listbox ----

        <Test>
        Public Sub Listbox_LeFiltreNeGardeQueCeQuiLeContient()
            Dim liste As New Mkm.Wpf.Controls.Listbox With {.Filter = True}
            For Each o In Options("Private", "In Work", "Frozen")
                liste.Items.Add(o)
            Next
            Pret(liste)

            liste.FilterText = "FRO"
            Assert.That(liste.Items.Count, [Is].EqualTo(1), "sans égard à la casse")
            Assert.That(liste.IsEmpty, [Is].False)
            liste.FilterText = "zzz"
            Assert.That(liste.IsEmpty, [Is].True, "une liste vide le dit")
            liste.FilterText = ""
            Assert.That(liste.Items.Count, [Is].EqualTo(3))
        End Sub

        <Test>
        Public Sub Listbox_LeTexteDUneOption()
            Dim liste = Pret(New Mkm.Wpf.Controls.Listbox With {.DisplayMemberPath = "Length"})
            Assert.That(liste.TexteDe("Frozen"), [Is].EqualTo("6"), "sa propriété DisplayMemberPath")
            Assert.That(liste.TexteDe(New ListBoxItem With {.Content = "Frozen"}), [Is].EqualTo("Frozen"))
        End Sub

        ' ---- MultiSelect ----

        Private Shared Function Maturites() As MultiSelect
            Dim liste As New MultiSelect With {.Placeholder = "Maturités"}
            For Each o In Options("Private", "In Work", "Frozen")
                liste.Items.Add(o)
            Next
            Return Pret(liste)
        End Function

        <Test>
        Public Sub MultiSelect_LeChampMontreLesChoixDansLeurOrdre()
            Dim liste = Maturites()
            Assert.That(liste.SelectionMode, [Is].EqualTo(SelectionMode.Multiple))
            Assert.That(liste.HasSelection, [Is].False)

            liste.SelectedItems.Add(liste.Items(2))
            liste.SelectedItems.Add(liste.Items(0))
            Assert.That(liste.SelectionText, [Is].EqualTo("Frozen, Private"))
            Assert.That(liste.SelectedChips, [Is].EqualTo({"Frozen", "Private"}))

            liste.MaxSelectedLabels = 1
            Assert.That(liste.SelectionText, [Is].EqualTo(Mkm.Wpf.Localization.Localizer.Current.T("locale.selectionMessage", 2)))
        End Sub

        <Test>
        Public Sub MultiSelect_UneListeDeroulantePourLesLecteursDEcran()
            Dim liste = Maturites()
            liste.SelectedItems.Add(liste.Items(1))
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(liste)

            Assert.That(pair.GetAutomationControlType(), [Is].EqualTo(AutomationControlType.ComboBox))
            Assert.That(pair.GetName(), [Is].EqualTo("Maturités"), "nommée par son texte d'aide")
            Assert.That(DirectCast(pair.GetPattern(PatternInterface.Value), IValueProvider).Value, [Is].EqualTo("In Work"))
            Dim depli = DirectCast(pair.GetPattern(PatternInterface.ExpandCollapse), IExpandCollapseProvider)
            Assert.That(depli.ExpandCollapseState, [Is].EqualTo(System.Windows.Automation.ExpandCollapseState.Collapsed))
        End Sub

        <Test>
        Public Sub MultiSelect_UneOptionEcriteChoisieLEstDesLeDepart()
            Dim liste As New MultiSelect()
            liste.Items.Add(New ListBoxItem With {.Content = "Plans", .IsSelected = True})
            liste.Items.Add(New ListBoxItem With {.Content = "Pièces"})
            Pret(liste)
            Assert.That(liste.SelectionText, [Is].EqualTo("Plans"), "sa liste fermée n'a pas encore créé ses options")
        End Sub

        ' ---- AutoComplete ----

        <Test>
        Public Sub AutoComplete_SearchDemandeEtSelectItemChoisit()
            Dim champ = Pret(New AutoComplete())
            Dim demande As String = Nothing
            Dim choisie = False
            AddHandler champ.Complete, Sub(s, e) demande = e.Query
            AddHandler champ.ItemSelect, Sub() choisie = True

            champ.Search("HS-0004")
            Assert.That(demande, [Is].EqualTo("HS-0004"))
            champ.Suggestions = {"HS-00041", "HS-00042"}
            Assert.That(champ.HasSuggestions, [Is].True)

            champ.SelectItem("HS-00042")
            Assert.That(champ.Text, [Is].EqualTo("HS-00042"))
            Assert.That(champ.SelectedItem, [Is].EqualTo("HS-00042"))
            Assert.That(choisie, [Is].True)
            Assert.That(champ.IsDropDownOpen, [Is].False)
        End Sub

        <Test>
        Public Sub AutoComplete_UnObjetSeMontreParSaPropriete()
            Dim champ = Pret(New AutoComplete With {.DisplayMemberPath = "Length"})
            champ.SelectItem("Frozen")
            Assert.That(champ.Text, [Is].EqualTo("6"))
        End Sub

    End Class

End Namespace
