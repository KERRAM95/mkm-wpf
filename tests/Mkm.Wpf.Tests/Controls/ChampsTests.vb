Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
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

        ' Ce que le Dispatcher a en attente.
        Private Shared Sub Vider()
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, New System.Action(Sub()
                                                                                                End Sub))
        End Sub

        ' Une liste de 40 choix posée en haut ou en bas d'une fenêtre de 300 de haut, ouverte.
        Private Shared Function ListeOuverteDans(fenetre As Window, enBas As Boolean) As [Select]
            Dim liste As New [Select] With {.Width = 200, .VerticalAlignment = If(enBas, VerticalAlignment.Bottom, VerticalAlignment.Top),
                                            .Margin = New Thickness(20)}
            For i = 1 To 40
                liste.Items.Add($"Choix {i}")
            Next
            Dim grille As New Grid()
            grille.Children.Add(liste)
            fenetre.Content = grille
            fenetre.Show()
            Vider()
            liste.IsDropDownOpen = True
            Vider()
            Return liste
        End Function

        <Test>
        Public Sub Select_LaListeResteDansSaFenetre()
            Dim fenetre As New Window With {.Width = 400, .Height = 300, .Left = -10000, .Top = -10000,
                                            .ShowInTaskbar = False, .ShowActivated = False, .WindowStyle = WindowStyle.None}
            Try
                ' En bas de la fenêtre : pas la place dessous, la liste s'ouvre dessus et s'arrête au bord du haut.
                Dim enBas = ListeOuverteDans(fenetre, True)
                Assert.That(enBas.OpensUpward, [Is].True, "pas la place dessous : elle s'ouvre dessus")
                Dim dessus = enBas.TransformToAncestor(fenetre).Transform(New Point(0, 0)).Y
                Assert.That(enBas.DropDownHeightLimit, [Is].LessThan(dessus), "sa hauteur s'arrête au bord de la fenêtre")
                Assert.That(DirectCast(Partie(Of System.Windows.Controls.Primitives.Popup)(enBas, "PART_Popup"), System.Windows.Controls.Primitives.Popup).Placement,
                            [Is].EqualTo(PlacementMode.Top))
                Dim defilement = Partie(Of ScrollViewer)(enBas, "PART_Defilement")
                Assert.That(defilement.ScrollableHeight, [Is].GreaterThan(0), "le reste défile")
                enBas.IsDropDownOpen = False
                Vider()

                ' En haut : la place dessous, elle s'ouvre dessous, comme avant.
                Dim enHaut = ListeOuverteDans(fenetre, False)
                Assert.That(enHaut.OpensUpward, [Is].False, "la place dessous : elle s'ouvre dessous")
                Dim dessous = fenetre.ActualHeight - (enHaut.TransformToAncestor(fenetre).Transform(New Point(0, 0)).Y + enHaut.ActualHeight)
                Assert.That(enHaut.DropDownHeightLimit, [Is].LessThan(dessous))
                enHaut.IsDropDownOpen = False
            Finally
                fenetre.Close()
            End Try
        End Sub

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
            Assert.That(Partie(Of Primitives.ToggleButton)(liste, "Zone").Visibility, [Is].EqualTo(Visibility.Collapsed))
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
