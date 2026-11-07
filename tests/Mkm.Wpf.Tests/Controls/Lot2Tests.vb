Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Theming
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>
    ''' Les composants du deuxième lot : l'info-bulle et le menu natifs, ProgressBar, ProgressSpinner, BlockUI,
    ''' ConfirmDialog, Textarea, DatePicker, PropertyPanel et Tree.
    ''' </summary>
    <TestFixture>
    Public Class Lot2Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(400, 300))
            element.Arrange(New Rect(0, 0, 400, 300))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        Private Shared Function Ressource(chemin As String) As Object
            Return Application.Current.TryFindResource(ThemeResources.Prefix & chemin)
        End Function

        <Test>
        Public Sub InfoBulle_ADroiteEtCentree()
            Dim places = TooltipPlacement.Right.Invoke(New Size(100, 20), New Size(50, 40), New Point(0, 0))

            Assert.That(places(0).Point, [Is].EqualTo(New Point(50, 10)))
            Assert.That(places(1).Point, [Is].EqualTo(New Point(-100, 10)))
        End Sub

        <Test>
        Public Sub InfoBulleDUneCommande_AuDessusEtCentree()
            Dim places = TooltipPlacement.Top.Invoke(New Size(100, 20), New Size(40, 40), New Point(0, 0))

            Assert.That(places(0).Point, [Is].EqualTo(New Point(-30, -20)))
            Assert.That(places(1).Point, [Is].EqualTo(New Point(-30, 40)))
        End Sub

        <Test>
        Public Sub InfoBulleEtMenu_OntLeStyleDeMkm()
            Assert.That(Application.Current.TryFindResource(GetType(ToolTip)), [Is].InstanceOf(Of Style)())
            Assert.That(Application.Current.TryFindResource(GetType(ContextMenu)), [Is].InstanceOf(Of Style)())
            Assert.That(Application.Current.TryFindResource(GetType(MenuItem)), [Is].InstanceOf(Of Style)())
        End Sub

        <Test>
        Public Sub Menu_SansIcone_SansColonneDIcone()
            Dim avec As New ContextMenu()
            avec.Items.Add(New MenuItem With {.Header = "Copier", .Icon = Icons.Copy})
            avec.Items.Add(New MenuItem With {.Header = "Coller"})
            Dim sans As New ContextMenu()
            sans.Items.Add(New MenuItem With {.Header = "Copier"})

            avec.RaiseEvent(New RoutedEventArgs(ContextMenu.OpenedEvent, avec))
            sans.RaiseEvent(New RoutedEventArgs(ContextMenu.OpenedEvent, sans))

            Assert.That(MenuIcons.GetHasIcons(avec), [Is].True)
            Assert.That(MenuIcons.GetHasIcons(sans), [Is].False)
            Assert.That(MenuIcons.GetHasIcons(DirectCast(sans.Items(0), MenuItem)), [Is].False)
        End Sub

        <Test>
        Public Sub ProgressBar_IndetermineeMontreSesTraits()
            Dim barre = Pret(New Mkm.Wpf.Controls.ProgressBar With {.Value = 50})
            Assert.That(barre.Height, [Is].EqualTo(6.0))
            Assert.That(Partie(Of UIElement)(barre, Mkm.Wpf.Controls.ProgressBar.PartIndeterminate).Visibility, [Is].EqualTo(Visibility.Collapsed))

            barre.IsIndeterminate = True

            Assert.That(Partie(Of UIElement)(barre, Mkm.Wpf.Controls.ProgressBar.PartIndeterminate).Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of UIElement)(barre, "PART_Indicator").Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub ProgressSpinner_CentPixelsALaCouleurPrincipale()
            Dim chargeur = Pret(New ProgressSpinner())

            Assert.That(chargeur.Width, [Is].EqualTo(100.0))
            Assert.That(chargeur.Stroke, [Is].SameAs(Ressource("progressspinner.color.one")))
        End Sub

        <Test>
        Public Sub BlockUI_VoileEtArreteLeClavier()
            Dim voile = Pret(New BlockUI With {.Content = New TextBox()})
            Dim masque = Partie(Of UIElement)(voile, BlockUI.PartMask)
            Assert.That(masque.Visibility, [Is].EqualTo(Visibility.Collapsed))

            voile.Blocked = True

            Assert.That(masque.Visibility, [Is].EqualTo(Visibility.Visible))
            Dim touche As New KeyEventArgs(Keyboard.PrimaryDevice, New FakeSource(), 0, Key.A) With {.RoutedEvent = Keyboard.PreviewKeyDownEvent}
            voile.RaiseEvent(touche)
            Assert.That(touche.Handled, [Is].True)
        End Sub

        <Test>
        Public Sub ConfirmDialog_OuiParDefautNonPourEchap()
            Dim demande As New ConfirmDialog With {.Message = "Mettre à jour ?"}
            Try
                Assert.That(demande.AcceptButton.IsDefault, [Is].True)
                Assert.That(demande.RejectButton.IsCancel, [Is].True)
                Assert.That(demande.Footer, [Is].Not.Null)

                demande.RejectSeverity = ButtonSeverity.Secondary

                Assert.That(demande.RejectButton.Severity, [Is].EqualTo(ButtonSeverity.Secondary))
            Finally
                demande.Close()
            End Try
        End Sub

        <Test>
        Public Sub Textarea_SesLignes()
            Dim zone As New Textarea With {.Rows = 3}
            Assert.That(zone.AcceptsReturn, [Is].True)
            Assert.That({zone.MinLines, zone.MaxLines}, [Is].EqualTo({3, 3}))

            zone.AutoResize = True

            Assert.That(zone.MaxLines, [Is].EqualTo(Integer.MaxValue))
        End Sub

        <Test>
        Public Sub DatePicker_SesPartiesEtSonCalendrier()
            Dim choix = Pret(New Mkm.Wpf.Controls.DatePicker With {.Placeholder = "jj/mm/aaaa"})

            Assert.That(Partie(Of DatePickerTextBox)(choix, "PART_TextBox"), [Is].Not.Null)
            Assert.That(Partie(Of ButtonBase)(choix, "PART_Button"), [Is].Not.Null)
            Assert.That(choix.CalendarStyle, [Is].Not.Null)
            Assert.That(choix.Height, [Is].EqualTo(24.0))
        End Sub

        <Test>
        Public Sub PropertyPanel_LaModificationMontreLesEditeurs()
            Dim champ As New InputText()
            Dim ligne As New PropertyRow With {.Label = "Référence", .Value = "HS-00042", .Required = True, .Content = champ}
            Dim lecture As New PropertyRow With {.Label = "Révision", .Value = "B", .ReadOnly = True, .Content = New InputText()}
            Dim section As New PropertySection With {.Header = "Cartouche"}
            section.Items.Add(ligne)
            section.Items.Add(lecture)
            Dim fiche As New PropertyPanel()
            fiche.Items.Add(section)
            Pret(fiche)
            Assert.That(section.IsExpanded, [Is].True, "la première section s'ouvre d'elle-même")
            Assert.That(ligne.IsEditorShown, [Is].False)

            fiche.Editing = True

            Assert.That(ligne.Editing, [Is].True)
            Assert.That(ligne.IsEditorShown, [Is].True)
            Assert.That(lecture.IsEditorShown, [Is].False, "une propriété en lecture seule le reste")
        End Sub

        <Test>
        Public Sub PropertyPanel_SesBoutons()
            Dim fiche = Pret(New PropertyPanel With {.Editable = True})
            Dim evenements As New List(Of String)
            AddHandler fiche.Edit, Sub() evenements.Add("edit")
            AddHandler fiche.Cancel, Sub() evenements.Add("cancel")

            Partie(Of ButtonBase)(fiche, PropertyPanel.PartEditButton).RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))
            Assert.That(fiche.Editing, [Is].True)
            Partie(Of ButtonBase)(fiche, PropertyPanel.PartCancelButton).RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))

            Assert.That(fiche.Editing, [Is].False)
            Assert.That(evenements, [Is].EqualTo({"edit", "cancel"}))
        End Sub

        <Test>
        Public Sub PropertyRow_Message()
            Dim ligne As New PropertyRow With {.Message = "Obligatoire"}
            Assert.That(ligne.HasMessage, [Is].True)

            ligne.Message = ""

            Assert.That(ligne.HasMessage, [Is].False)
        End Sub

        <Test>
        Public Sub Tree_SesNoeudsSontDesTreeNode()
            Dim arbre As New Tree With {.ItemsSource = {"Notes", "Historique"}}
            Pret(arbre)

            Assert.That(arbre.ItemContainerGenerator.ContainerFromIndex(0), [Is].InstanceOf(Of TreeNode)())
        End Sub

        ' Une source de présentation pour les événements du clavier d'un test.
        Private NotInheritable Class FakeSource
            Inherits PresentationSource

            Public Overrides Property RootVisual As Media.Visual

            Public Overrides ReadOnly Property IsDisposed As Boolean
                Get
                    Return False
                End Get
            End Property

            Protected Overrides Function GetCompositionTargetCore() As Media.CompositionTarget
                Return Nothing
            End Function
        End Class

    End Class

End Namespace
