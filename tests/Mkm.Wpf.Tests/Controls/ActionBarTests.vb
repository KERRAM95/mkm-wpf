Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>L'ActionBar, comme celle de mkm : sections, listes, bascules, repli, libellés, info-bulles.</summary>
    <TestFixture>
    Public Class ActionBarTests

        ''' <summary>Une commande d'essai, que le test rend possible ou non.</summary>
        Private NotInheritable Class Commande
            Implements ICommand

            Public Property Possible As Boolean = True
            Public Property Executions As Integer

            Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

            Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
                Return Possible
            End Function

            Public Sub Execute(parameter As Object) Implements ICommand.Execute
                Executions += 1
            End Sub

        End Class

        Private Shared Function Section(cle As String, ParamArray commandes As UIElement()) As ActionBarSection
            Dim resultat As New ActionBarSection With {.Key = cle, .Header = cle}
            For Each commande In commandes
                resultat.Items.Add(commande)
            Next
            Return resultat
        End Function

        ' Une barre initialisée, son gabarit appliqué et mise en page, comme dans une fenêtre.
        Private Shared Function Barre(ParamArray sections As ActionBarSection()) As ActionBar
            Dim resultat As New ActionBar()
            resultat.BeginInit()
            For Each s In sections
                resultat.Items.Add(s)
            Next
            resultat.EndInit()
            MettreEnPage(resultat)
            Return resultat
        End Function

        Private Shared Sub MettreEnPage(element As FrameworkElement)
            element.ApplyTemplate()
            element.Measure(New Size(800, 300))
            element.Arrange(New Rect(0, 0, 800, 300))
            element.UpdateLayout()
        End Sub

        Private Shared Function Bouton(libelle As String) As ActionBarButton
            Return New ActionBarButton With {.Label = libelle, .Icon = Icons.Add}
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        <Test>
        Public Sub Gabarit_SApplique()
            Dim barre = ActionBarTests.Barre(Section("a", Bouton("Un")))

            Assert.That(barre.Template, [Is].Not.Null)
            Assert.That(Partie(Of ItemsControl)(barre, "PART_Commands"), [Is].Not.Null)
        End Sub

        <Test>
        Public Sub SansActiveSection_LaPremiere()
            Dim a = Section("a", Bouton("Un"))
            Dim barre = ActionBarTests.Barre(a, Section("b"))

            Assert.That(barre.SelectedItem, [Is].SameAs(a))
            Assert.That(barre.ActiveSection, [Is].EqualTo("a"))
        End Sub

        <Test>
        Public Sub ActiveSection_ChoisitLaSection()
            Dim b = Section("b", Bouton("Deux"))
            Dim barre = ActionBarTests.Barre(Section("a"), b)

            barre.ActiveSection = "b"

            Assert.That(barre.SelectedItem, [Is].SameAs(b))
            Assert.That(b.IsSelected, [Is].True)
        End Sub

        <Test>
        Public Sub ChoisirUnOnglet_ChangeActiveSection()
            Dim b = Section("b")
            Dim barre = ActionBarTests.Barre(Section("a"), b)

            b.IsSelected = True

            Assert.That(barre.ActiveSection, [Is].EqualTo("b"))
        End Sub

        <Test>
        Public Sub LesCommandesDeLaSection_PassentDansLaRangee()
            Dim un = Bouton("Un")
            Dim deux = Bouton("Deux")
            Dim barre = ActionBarTests.Barre(Section("a", un), Section("b", deux))
            Dim rangee = Partie(Of ItemsControl)(barre, "PART_Commands")

            Assert.That(rangee.Items.Cast(Of Object)(), [Is].EqualTo({un}))

            barre.ActiveSection = "b"

            Assert.That(rangee.Items.Cast(Of Object)(), [Is].EqualTo({deux}))
        End Sub

        <Test>
        Public Sub Onglets_SeulementAvecPlusieursSectionsVisibles()
            Dim b = Section("b")
            Dim barre = ActionBarTests.Barre(Section("a"), b)
            Assert.That(barre.HasTabs, [Is].True)

            b.Visibility = Visibility.Collapsed

            Assert.That(barre.HasTabs, [Is].False)
        End Sub

        <Test>
        Public Sub Repli_ParSonBouton()
            Dim barre = ActionBarTests.Barre(Section("a", Bouton("Un")))
            Dim repli = Partie(Of ButtonBase)(barre, "PART_Toggle")

            repli.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))

            Assert.That(barre.IsCollapsed, [Is].True)
        End Sub

        <Test>
        Public Sub ClicDroit_MontreLesLibelles()
            Dim barre = ActionBarTests.Barre(Section("a", Bouton("Un")))
            Dim montrer = DirectCast(barre.ContextMenu.Items(0), MenuItem)

            montrer.RaiseEvent(New RoutedEventArgs(MenuItem.ClickEvent))

            Assert.That(barre.ShowLabels, [Is].True)
        End Sub

        <Test>
        Public Sub Libelles_HeritesParLesCommandes()
            Dim un = Bouton("Un")
            Dim barre = ActionBarTests.Barre(Section("a", un))

            barre.ShowLabels = True

            Assert.That(ActionBar.GetShowLabels(un), [Is].True)
        End Sub

        <Test>
        Public Sub Libelle_SurDeuxLignesAuPlus()
            Dim actualiser As New ActionBarButton With {.Label = "Actualiser la liste des plans depuis le serveur", .Icon = Icons.Refresh}
            Dim barre = ActionBarTests.Barre(Section("a", actualiser))
            barre.ShowLabels = True
            MettreEnPage(barre)

            Dim libelle = Partie(Of TextBlock)(actualiser, "Libelle")
            Assert.That(libelle.LineHeight, [Is].GreaterThan(0), "hauteur d'une ligne (thème)")
            Assert.That(libelle.MaxHeight, [Is].EqualTo(2 * libelle.LineHeight))
            Assert.That(libelle.ActualHeight, [Is].LessThanOrEqualTo(2 * libelle.LineHeight + 0.5), "deux lignes au plus")
            ' Le libellé reste dans la commande : rien ne déborde sous elle.
            Assert.That(libelle.TranslatePoint(New Point(0, libelle.ActualHeight), actualiser).Y, [Is].LessThanOrEqualTo(actualiser.ActualHeight + 0.5))
        End Sub

        <Test>
        Public Sub InfoBulle_CommeMkm()
            Dim enregistrer As New ActionBarButton With {.Label = "Enregistrer", .Hint = "Ouvrez d'abord un plan"}
            Dim barre = ActionBarTests.Barre(Section("a", enregistrer))

            Assert.That(DirectCast(enregistrer.ToolTip, ToolTip).Content, [Is].EqualTo("Enregistrer — Ouvrez d'abord un plan"))

            ' Libellés montrés, l'info-bulle ne garde que l'indice.
            barre.ShowLabels = True

            Assert.That(DirectCast(enregistrer.ToolTip, ToolTip).Content, [Is].EqualTo("Ouvrez d'abord un plan"))
        End Sub

        <Test>
        Public Sub Commande_GriseeParSonICommand()
            Dim commande As New Commande With {.Possible = False}
            Dim un As New ActionBarButton With {.Label = "Un", .Command = commande}

            Assert.That(un.IsEnabled, [Is].False)
        End Sub

        <Test>
        Public Sub Liste_MontreLaPremiere_PuisLaDerniereLancee()
            Dim un = Bouton("Un")
            Dim deux = Bouton("Deux")
            Dim liste As New ActionBarDropDown()
            liste.Items.Add(un)
            liste.Items.Add(deux)
            ActionBarTests.Barre(Section("a", liste))
            Assert.That(liste.ShownItem, [Is].SameAs(un))

            deux.PerformClick()

            Assert.That(liste.ShownItem, [Is].SameAs(deux))
        End Sub

        <Test>
        Public Sub Liste_MontreLaBasculeEnfoncee()
            Dim un = Bouton("Un")
            Dim bascule As New ActionBarToggleButton With {.Label = "Grille"}
            Dim liste As New ActionBarDropDown()
            liste.Items.Add(un)
            liste.Items.Add(bascule)
            ActionBarTests.Barre(Section("a", liste))

            bascule.IsChecked = True

            Assert.That(liste.ShownItem, [Is].SameAs(bascule))
        End Sub

        <Test>
        Public Sub Liste_LaCommandeMontree_SeLanceDepuisLaBarre()
            Dim commande As New Commande()
            Dim un As New ActionBarButton With {.Label = "Un", .Command = commande}
            Dim liste As New ActionBarDropDown()
            liste.Items.Add(un)
            ActionBarTests.Barre(Section("a", liste))
            Dim principale = Partie(Of ButtonBase)(liste, "PART_Main")

            principale.RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))

            Assert.That(commande.Executions, [Is].EqualTo(1))
        End Sub

        <Test>
        Public Sub Largeur_SuitLaSectionMontree()
            Dim large = Section("large")
            For i = 1 To 12
                large.Items.Add(Bouton("Commande " & i))
            Next
            Dim barre = ActionBarTests.Barre(Section("etroite", Bouton("Un")), large)
            Dim etroite = barre.DesiredSize.Width

            barre.ActiveSection = "large"
            MettreEnPage(barre)
            MettreEnPage(barre)
            Assert.That(barre.DesiredSize.Width, [Is].GreaterThan(etroite + 50))

            ' De retour sur la section étroite, la barre reprend sa largeur : elle ne garde pas celle de la large.
            barre.ActiveSection = "etroite"
            MettreEnPage(barre)
            MettreEnPage(barre)
            Assert.That(barre.DesiredSize.Width, [Is].EqualTo(etroite).Within(0.5))
        End Sub

        <Test>
        Public Sub SectionEtroite_CentreeDansLaBarre()
            Dim un = Bouton("Un")
            Dim barre = ActionBarTests.Barre(Section("a", un))
            barre.Width = 600
            MettreEnPage(barre)
            Dim rangee = Partie(Of ItemsControl)(barre, "PART_Commands")

            Dim milieu = rangee.TranslatePoint(New System.Windows.Point(rangee.ActualWidth / 2, 0), barre)

            Assert.That(milieu.X, [Is].EqualTo(barre.ActualWidth / 2).Within(1))
        End Sub

        <Test>
        Public Sub CommandesFixes_EnTeteDeLaRangee()
            Dim fixe = Bouton("Fixe")
            Dim un = Bouton("Un")
            Dim barre As New ActionBar()
            barre.BeginInit()
            barre.FixedItems.Add(fixe)
            barre.Items.Add(Section("a", un))
            barre.EndInit()
            MettreEnPage(barre)

            Assert.That(barre.HasFixedItems, [Is].True)
            Assert.That(barre.Commandes().ToList(), [Is].EqualTo({fixe, un}))
        End Sub

    End Class

End Namespace
