Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Theming
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>
    ''' Les composants de mkm après les champs : Button, Checkbox, Tabs, Fieldset, Message, Toast et Dialog,
    ''' leur gabarit et ce qu'ils font.
    ''' </summary>
    <TestFixture>
    Public Class ComposantsTests

        ' Un élément initialisé, son gabarit appliqué et mis en page, comme dans une fenêtre.
        Private Shared Function Pret(Of T As Control)(controle As T) As T
            controle.BeginInit()
            controle.EndInit()
            controle.ApplyTemplate()
            controle.Measure(New Size(400, 300))
            controle.Arrange(New Rect(0, 0, 400, 300))
            controle.UpdateLayout()
            Return controle
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        Private Shared Function Ressource(chemin As String) As Object
            Return Application.Current.TryFindResource(ThemeResources.Prefix & chemin)
        End Function

        <Test>
        Public Sub Button_HauteurDuThemeEtIconeSeule()
            Dim bouton = Pret(New Mkm.Wpf.Controls.Button With {.Icon = Icons.Download})

            Assert.That(bouton.Height, [Is].EqualTo(24.0))
            Assert.That(bouton.IsIconOnly, [Is].True)

            bouton.Content = "Télécharger"

            Assert.That(bouton.IsIconOnly, [Is].False)
            Assert.That(bouton.HasIcon, [Is].True)
        End Sub

        <Test>
        Public Sub Button_LaGraviteDonneSesCouleurs()
            Dim bouton = Pret(New Mkm.Wpf.Controls.Button With {.Content = "Supprimer", .Severity = ButtonSeverity.Danger})

            Assert.That(bouton.Background, [Is].SameAs(Ressource("button.danger.background")))

            bouton.Variant = ButtonVariant.Outlined

            Assert.That(bouton.BorderBrush, [Is].SameAs(Ressource("button.outlined.danger.border.color")))
        End Sub

        <Test>
        Public Sub Checkbox_IndetermineeMontreUnTrait()
            Dim c = Pret(New Mkm.Wpf.Controls.Checkbox With {.IsThreeState = True, .IsChecked = Nothing})
            Dim coche = Partie(Of Icon)(c, "Coche")

            Assert.That(coche.Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(coche.Source, [Is].SameAs(Icons.Remove))

            c.IsChecked = True

            Assert.That(coche.Source, [Is].SameAs(Icons.Check))
        End Sub

        <Test>
        Public Sub Tabs_LesBoutsDuGroupe()
            Dim onglets As New Tabs()
            Dim a As New Tab With {.Header = "Cartouche"}
            Dim b As New Tab With {.Header = "Notes"}
            Dim c As New Tab With {.Header = "Historique"}
            onglets.Items.Add(a)
            onglets.Items.Add(b)
            onglets.Items.Add(c)
            Pret(onglets)

            Assert.That({a.IsFirst, a.IsLast, b.IsFirst, b.IsLast, c.IsFirst, c.IsLast},
                        [Is].EqualTo({True, False, False, False, False, True}))

            c.Visibility = Visibility.Collapsed

            Assert.That(b.IsLast, [Is].True)
        End Sub

        <Test>
        Public Sub Tabs_LeDessinPasseAuxOnglets()
            Dim onglets As New Tabs With {.Variant = TabsVariant.Strip}
            Dim a As New Tab With {.Header = "Propriétés"}
            onglets.Items.Add(a)
            Pret(onglets)

            Assert.That(a.Variant, [Is].EqualTo(TabsVariant.Strip))
            Assert.That(a.Height, [Is].EqualTo(26.0))
            Assert.That(onglets.SelectedItem, [Is].SameAs(a))
        End Sub

        <Test>
        Public Sub Fieldset_LegendeAuDroitDuContenu()
            Dim groupe = Pret(New Fieldset With {.Header = "Cartouche", .Content = New TextBlock()})

            ' La bordure (1 px) et le retrait de gauche (13,5 px) de mkm.
            Assert.That(Partie(Of FrameworkElement)(groupe, Fieldset.PartLegend).Margin.Left, [Is].EqualTo(14.5))
        End Sub

        <Test>
        Public Sub Fieldset_SeReplieParSaLegende()
            Dim groupe = Pret(New Fieldset With {.Header = "Notes", .Toggleable = True, .Content = New TextBlock()})
            Dim bascule = Partie(Of ToggleButton)(groupe, "Bascule")
            Dim contenu = Partie(Of ContentPresenter)(groupe, "Contenu")
            Assert.That(bascule.Visibility, [Is].EqualTo(Visibility.Visible))

            bascule.IsChecked = True

            Assert.That(groupe.IsCollapsed, [Is].True)
            Assert.That(contenu.Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub Message_CouleursEtMesuresDuTheme()
            Dim m = Pret(New Message With {.Content = "Le cartouche n'est pas à jour.", .Severity = MessageSeverity.Error})

            Assert.That(m.IconForeground, [Is].SameAs(Ressource("message.error.icon.color")))
            Assert.That(m.Background, [Is].SameAs(Ressource("message.error.background")))
            Assert.That(m.FontSize, [Is].EqualTo(12.0))

            m.Size = MessageSize.Small
            m.Variant = MessageVariant.Simple

            Assert.That(m.FontSize, [Is].EqualTo(11.0))
            Assert.That(m.BorderThickness, [Is].EqualTo(New Thickness(0)))
            Assert.That(m.Padding, [Is].EqualTo(New Thickness(0)))
        End Sub

        <Test>
        Public Sub Message_SeFermeEtLeDit()
            Dim m = Pret(New Message With {.Content = "Enregistré.", .Closable = True})
            Dim fermetures = 0
            AddHandler m.Closed, Sub() fermetures += 1
            Assert.That(Partie(Of UIElement)(m, "PART_CloseButton").Visibility, [Is].EqualTo(Visibility.Visible))

            m.Close()
            m.Close()

            Assert.That(m.Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(fermetures, [Is].EqualTo(1))
        End Sub

        <Test>
        Public Sub Toast_AjouteDevantOuDerriereEtFerme()
            Dim notifications = Pret(New Toast())
            Dim fermees As New List(Of ToastClosedEventArgs)
            AddHandler notifications.MessageClosed, Sub(s, e) fermees.Add(e)

            Dim premiere = notifications.Add(MessageSeverity.Success, "Enregistré", "Le cartouche est à jour.")
            Dim devant As New ToastMessage With {.Summary = "Nouveau"}
            notifications.Add(devant, prepend:=True)

            Assert.That(notifications.Items.Cast(Of Object)(), [Is].EqualTo(New Object() {devant, premiere}))

            notifications.Remove(premiere)

            Assert.That(notifications.Items.Count, [Is].EqualTo(1))
            Assert.That(fermees.Single().Message, [Is].SameAs(premiere))
            Assert.That(fermees.Single().Reason, [Is].EqualTo(ToastCloseReason.Close))
        End Sub

        <Test>
        Public Sub ToastMessage_LIconeDeSaGravite()
            Dim n As New ToastMessage With {.Severity = MessageSeverity.Warn}
            Assert.That(n.DisplayIcon, [Is].SameAs(Icons.Warning))
            Assert.That(n.BandBrush, [Is].SameAs(Ressource("toast.warn.border.color")))

            n.Severity = MessageSeverity.Secondary
            Assert.That(n.DisplayIcon, [Is].Null)

            n.Icon = Icons.Info
            Assert.That(n.DisplayIcon, [Is].SameAs(Icons.Info))
        End Sub

        <Test>
        Public Sub Dialog_UneFenetreSansBordAuxPartiesDeMkm()
            Dim d As New Dialog With {.Title = "Renommer", .Content = New TextBlock()}
            Try
                d.ApplyTemplate()

                Assert.That(d.WindowStyle, [Is].EqualTo(WindowStyle.None))
                Assert.That(d.AllowsTransparency, [Is].True)
                Assert.That(Partie(Of FrameworkElement)(d, Dialog.PartHeader), [Is].Not.Null)
                Assert.That(Partie(Of ButtonBase)(d, Dialog.PartCloseButton), [Is].Not.Null)
                ' Sans pied, le contenu ferme le cadre ; sans poignée tant qu'on ne l'étire pas.
                Assert.That(Partie(Of UIElement)(d, "Pied").Visibility, [Is].EqualTo(Visibility.Collapsed))
                Assert.That(Partie(Of UIElement)(d, Dialog.PartResizeGrip).Visibility, [Is].EqualTo(Visibility.Collapsed))

                d.Footer = New Mkm.Wpf.Controls.Button With {.Content = "OK"}
                d.ResizeMode = ResizeMode.CanResize

                Assert.That(Partie(Of UIElement)(d, "Pied").Visibility, [Is].EqualTo(Visibility.Visible))
                Assert.That(Partie(Of UIElement)(d, Dialog.PartResizeGrip).Visibility, [Is].EqualTo(Visibility.Visible))
            Finally
                d.Close()
            End Try
        End Sub

    End Class

End Namespace
