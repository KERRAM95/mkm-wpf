Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Theming
Imports NUnit.Framework
Imports BoutonDeMkm = Mkm.Wpf.Controls.Button

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les défauts relevés à la relecture des lots 1 à 4, et leur correction.</summary>
    <TestFixture>
    Public Class RelectureTests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 800) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, 400))
            element.Arrange(New Rect(0, 0, largeur, 400))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        Private Shared Function Ressource(cle As String) As Object
            Return Application.Current.TryFindResource(ThemeResources.Prefix & cle)
        End Function

        Private Shared Function Section(cle As String) As ActionBarSection
            Dim s As New ActionBarSection With {.Key = cle, .Header = cle}
            s.Items.Add(New ActionBarButton With {.Label = cle, .Icon = Icons.Add})
            Return s
        End Function

        <Test>
        Public Sub ActionBarDropDown_SesCommandesSontDesLignesDeMenu()
            Dim liste As New ActionBarDropDown()
            Dim zoom As New ActionBarButton With {.Label = "Zoom", .Icon = Icons.ZoomIn}
            liste.Items.Add(zoom)
            Pret(liste)

            Assert.That(ActionBarDropDown.GetIsInList(zoom), [Is].True, "posée sur la commande : l'héritage suit la liste, non le cadre flottant")
        End Sub

        <Test>
        Public Sub ActionBar_LaCleVoulueAttendSaSection()
            Dim barre As New ActionBar With {.ActiveSection = "b"}
            Pret(barre)

            barre.Items.Add(Section("a"))
            Assert.That(barre.ActiveSection, [Is].EqualTo("b"), "le repli sur « a » ne réécrit pas la clé voulue")
            barre.Items.Add(Section("b"))

            Assert.That(DirectCast(barre.SelectedItem, ActionBarSection).Key, [Is].EqualTo("b"))
        End Sub

        <Test>
        Public Sub ActionBar_LaSectionMontreeCachee_UneAutreLaRemplace()
            Dim barre As New ActionBar()
            Dim a = Section("a")
            Dim b = Section("b")
            barre.Items.Add(a)
            barre.Items.Add(b)
            barre.ActiveSection = "b"
            Pret(barre)

            b.Visibility = Visibility.Collapsed
            Assert.That(barre.SelectedItem, [Is].SameAs(a))
            Assert.That(barre.ActiveSection, [Is].EqualTo("b"), "la clé voulue est gardée")

            b.Visibility = Visibility.Visible
            Assert.That(barre.SelectedItem, [Is].SameAs(b), "elle revient avec sa section")
        End Sub

        <Test>
        Public Sub Button_LeBoutonLite_SonGris()
            Dim lite = Pret(New BoutonDeMkm With {.Variant = ButtonVariant.Text, .Severity = ButtonSeverity.Secondary, .Icon = Icons.Close})

            Dim fond = Partie(Of Border)(lite, "Fond")
            Assert.That(fond.GetValue(TextElement.ForegroundProperty), [Is].SameAs(Ressource("button.icon.color")))
        End Sub

        <Test>
        Public Sub Button_LAnneauDuClavier_ALaCouleurDeLaGravite()
            Dim bouton = Pret(New BoutonDeMkm With {.Content = "Supprimer", .Severity = ButtonSeverity.Danger})
            Assert.That(bouton.FocusRingBrush, [Is].SameAs(Ressource("button.danger.focus.ring.color")))
            Dim anneau = Partie(Of Shapes.Rectangle)(bouton, "Anneau")
            Assert.That(anneau.Visibility, [Is].EqualTo(Visibility.Collapsed), "au repos, pas d'anneau")
            Assert.That(anneau.Margin, [Is].EqualTo(New Thickness(-2)), "à un pixel au-dehors")
        End Sub

        <Test>
        Public Sub Menu_SesTraitsOntLeStyleDesMenusEtIlNEstQuUnArret()
            For Each menu In New System.Windows.Controls.Primitives.MenuBase() {New Global.Mkm.Wpf.Controls.Menu(), New TieredMenu()}
                Dim trait As New Separator()
                menu.Items.Add(New MenuItem With {.Header = "Ouvrir"})
                menu.Items.Add(trait)
                menu.Items.Add(New MenuItem With {.Header = "Fermer"})
                Pret(menu)
                Assert.That(trait.Style, [Is].SameAs(Application.Current.TryFindResource(MenuItem.SeparatorStyleKey)), menu.GetType().Name)
                Assert.That(menu.IsTabStop, [Is].False, "le menu lui-même n'est pas un arrêt")
                Assert.That(System.Windows.Input.KeyboardNavigation.GetTabNavigation(menu), [Is].EqualTo(System.Windows.Input.KeyboardNavigationMode.Once), "un seul arrêt")
            Next
        End Sub

        <Test>
        Public Sub Message_UneValeurDeLApplicationLEmporteSurLaGravite()
            Dim message As New Message With {.Padding = New Thickness(0)}
            message.Severity = MessageSeverity.Warn
            Pret(message)

            Assert.That(message.Padding, [Is].EqualTo(New Thickness(0)))
            message.Severity = MessageSeverity.Error
            Assert.That(message.Padding, [Is].EqualTo(New Thickness(0)))
        End Sub

        <Test>
        Public Sub Fieldset_RepliSansBascule_LeContenuDisparait()
            Dim groupe = Pret(New Fieldset With {.Header = "Cartouche", .Content = New TextBlock(), .IsCollapsed = True})

            Assert.That(Partie(Of UIElement)(groupe, "Contenu").Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub InputNumber_ArrondiCommeLAffichage()
            Dim nombre = Pret(New InputNumber With {.Value = 0, .Step = 0.5, .MaxFractionDigits = 0})

            nombre.Spin(0.5)

            Assert.That(nombre.Value, [Is].EqualTo(1), "0,5 s'arrondit à 1, comme il s'affiche, et non à 0")
        End Sub

        <Test>
        Public Sub IdCard_UneChaineVideNeSAffichePas()
            Dim carte = Pret(New IdCard With {.Title = "Carter", .Revision = "", .Maturity = ""})

            Assert.That(Partie(Of UIElement)(carte, "Revision").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of UIElement)(carte, "Maturite").Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub Icon_SansTaille_SeizePixelsMemeEtiree()
            Dim icone As New Icon With {.Source = Icons.Add}
            Dim case24 As New Grid With {.Width = 24, .Height = 24}
            case24.Children.Add(icone)
            Pret(case24)

            Assert.That(icone.ActualWidth, [Is].EqualTo(16))
            Assert.That(icone.ActualHeight, [Is].EqualTo(16))
        End Sub

        <Test>
        Public Sub LesTextesEnrichis_ALaLangueDeWindows()
            Dim attendue = Globalization.CultureInfo.CurrentCulture.IetfLanguageTag

            Assert.That(New Documents.Run().Language.IetfLanguageTag, [Is].EqualTo(attendue).IgnoreCase)
            Assert.That(New Documents.FlowDocument().Language.IetfLanguageTag, [Is].EqualTo(attendue).IgnoreCase)
        End Sub

        <Test>
        Public Sub TreeNode_SesEnfantsVirtualises()
            Dim arbre As New Tree()
            VirtualizingPanel.SetIsVirtualizing(arbre, True)
            Dim noeud As New TreeNode With {.Header = "Notes"}
            arbre.Items.Add(noeud)
            Pret(arbre)

            Assert.That(noeud.ItemsPanel.LoadContent(), [Is].InstanceOf(Of VirtualizingStackPanel)())
        End Sub

    End Class

End Namespace
