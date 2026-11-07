Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les champs de mkm : InputText et Select, leur gabarit et leurs états.</summary>
    <TestFixture>
    Public Class ChampsTests

        ' Un élément initialisé, son gabarit appliqué et mis en page, comme dans une fenêtre.
        Private Shared Function Pret(Of T As Control)(controle As T) As T
            controle.BeginInit()
            controle.EndInit()
            controle.ApplyTemplate()
            controle.Measure(New Size(300, 100))
            controle.Arrange(New Rect(0, 0, 300, 100))
            controle.UpdateLayout()
            Return controle
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        <Test>
        Public Sub InputText_GabaritEtHauteurDuTheme()
            Dim champ = Pret(New InputText())

            Assert.That(Partie(Of ScrollViewer)(champ, "PART_ContentHost"), [Is].Not.Null)
            Assert.That(champ.Height, [Is].EqualTo(24.0))
        End Sub

        <Test>
        Public Sub InputText_AideQuandVide()
            Dim champ = Pret(New InputText With {.Placeholder = "Référence"})
            Dim aide = Partie(Of TextBlock)(champ, "Aide")
            Assert.That(aide.Visibility, [Is].EqualTo(Visibility.Visible))

            champ.Text = "HS-1234"

            Assert.That(aide.Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub InputText_GardeCeQueFaitTextBox()
            Dim champ = Pret(New InputText With {.MaxLength = 5, .CharacterCasing = CharacterCasing.Upper, .IsReadOnly = True})

            Assert.That(champ, [Is].InstanceOf(Of TextBox)())
            Assert.That(champ.MaxLength, [Is].EqualTo(5))
        End Sub

        <Test>
        Public Sub Select_GabaritEtSesParties()
            Dim liste = Pret(New [Select]())

            Assert.That(Partie(Of Popup)(liste, "PART_Popup"), [Is].Not.Null)
            Assert.That(Partie(Of TextBox)(liste, "PART_EditableTextBox"), [Is].Not.Null)
            Assert.That(liste.Height, [Is].EqualTo(24.0))
        End Sub

        <Test>
        Public Sub Select_AideQuandRienNEstChoisi()
            Dim liste As New [Select] With {.Placeholder = "Maturité"}
            liste.Items.Add("In Work")
            liste.Items.Add("Released")
            Pret(liste)
            Dim aide = Partie(Of TextBlock)(liste, "Aide")
            Assert.That(aide.Visibility, [Is].EqualTo(Visibility.Visible))

            liste.SelectedIndex = 1

            Assert.That(aide.Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(liste.Text, [Is].EqualTo("Released"))
        End Sub

        <Test>
        Public Sub Select_Editable_MontreLaSaisie()
            Dim liste = Pret(New [Select] With {.IsEditable = True})

            Assert.That(Partie(Of TextBox)(liste, "PART_EditableTextBox").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of ToggleButton)(liste, "Zone").Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub Select_ListeVirtualisee()
            Dim liste = Pret(New [Select]())

            Assert.That(VirtualizingPanel.GetIsVirtualizing(liste), [Is].True)
            Assert.That(VirtualizingPanel.GetVirtualizationMode(liste), [Is].EqualTo(VirtualizationMode.Recycling))
            Assert.That(liste.ItemsPanel.LoadContent(), [Is].InstanceOf(Of VirtualizingStackPanel)())
        End Sub

        <Test>
        Public Sub Select_ChoixDeLaListe_AuStyleDeMkm()
            Dim liste As New [Select]()
            liste.Items.Add("Un")
            Pret(liste)
            Dim choix = DirectCast(liste.ItemContainerGenerator.ContainerFromIndex(0), ComboBoxItem)

            ' Fermée, la liste n'a pas encore créé ses choix ; son style de choix est celui de mkm.
            Assert.That(liste.ItemContainerStyle, [Is].Not.Null)
            Assert.That(liste.ItemContainerStyle.TargetType, [Is].EqualTo(GetType(ComboBoxItem)))
            If choix IsNot Nothing Then Assert.That(choix.Height, [Is].EqualTo(22.0))
        End Sub

    End Class

End Namespace
