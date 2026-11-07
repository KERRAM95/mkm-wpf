Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports Mkm.Wpf.Commands
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Commands

    ''' <summary>Le registre des commandes : déclarer une fois, lancer, cocher, cacher, fabriquer les menus et les barres.</summary>
    <TestFixture>
    Public Class CommandRegistryTests

        Private _lances As List(Of String)

        Private Function Registre() As CommandRegistry
            _lances = New List(Of String)
            Dim noter = Sub(e As CommandExecutedEventArgs) _lances.Add(e.Id)
            Return New CommandRegistry(
                New CommandDefinition With {.Id = "enregistrer", .Label = "Enregistrer", .Icon = Icons.Download, .Shortcut = "Ctrl+S", .Execute = noter},
                New CommandDefinition With {.Id = "supprimer", .Label = "Supprimer", .IsEnabled = Function(c) c IsNot Nothing, .Execute = noter},
                New CommandDefinition With {.Id = "verrouiller", .Kind = CommandKind.Check, .Label = "Verrouiller", .CheckedLabel = "Déverrouiller",
                                            .Icon = Icons.Unlocked, .CheckedIcon = Icons.Locked, .Execute = noter},
                New CommandDefinition With {.Id = "plan", .Kind = CommandKind.Radio, .Group = "vue", .Label = "Plan", .Checked = True},
                New CommandDefinition With {.Id = "article", .Kind = CommandKind.Radio, .Group = "vue", .Label = "Article"},
                New CommandDefinition With {.Id = "cartouche", .Label = "Cartouche", .Views = {"plan"}})
        End Function

        <Test>
        Public Sub Execute_LanceSaufGrisee()
            Dim r = Registre()

            Assert.That(r.Execute("enregistrer"), [Is].True)
            Assert.That(r.Execute("supprimer"), [Is].False, "grisée sans contexte")
            Assert.That(r("supprimer").CanExecute(Nothing), [Is].False)
            Assert.That(r.Execute("supprimer", "ligne 1"), [Is].True)
            Assert.That(_lances, [Is].EqualTo({"enregistrer", "supprimer"}))
        End Sub

        <Test>
        Public Sub UneCase_SInverse_EtPrendSonAllureCochee()
            Dim r = Registre()
            Dim verrou = r("verrouiller")
            Assert.That(verrou.Label, [Is].EqualTo("Verrouiller"))

            r.Execute("verrouiller")

            Assert.That(verrou.IsChecked, [Is].True)
            Assert.That(verrou.Label, [Is].EqualTo("Déverrouiller"))
            Assert.That(verrou.Icon, [Is].SameAs(Icons.Locked))
            r.Execute("verrouiller")
            Assert.That(verrou.IsChecked, [Is].False)
        End Sub

        <Test>
        Public Sub UnChoix_DecocheLesAutresDeSonGroupe()
            Dim r = Registre()

            r.Execute("article")

            Assert.That(r("article").IsChecked, [Is].True)
            Assert.That(r("plan").IsChecked, [Is].False)
            r.SetChecked("plan", True)
            Assert.That(r("article").IsChecked, [Is].False)
        End Sub

        <Test>
        Public Sub UneVue_CacheLesCommandesDesAutres()
            Dim r = Registre()
            Assert.That(r("cartouche").IsVisible, [Is].False, "sans vue")
            Assert.That(r.Execute("cartouche"), [Is].False)

            r.View = "plan"

            Assert.That(r("cartouche").IsVisible, [Is].True)
            Assert.That(r("cartouche").Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub LeMenu_SansTraitSeul()
            Dim r = Registre()

            Dim elements = r.CreateMenuItems({"-", "enregistrer", "-", "-", "cartouche", "-", "verrouiller", "-"})

            Assert.That(elements.Select(Function(e) e.GetType().Name), [Is].EqualTo({"MenuItem", "Separator", "MenuItem"}),
                        "ni trait en tête ni en queue, ni deux de suite ; la commande cachée laissée de côté")
            Dim enregistrer = DirectCast(elements(0), MenuItem)
            Assert.That(enregistrer.Header, [Is].EqualTo("Enregistrer"))
            Assert.That(enregistrer.InputGestureText, [Is].EqualTo("Ctrl+S"))
            Assert.That(enregistrer.Command, [Is].SameAs(r("enregistrer")))
            Dim verrou = DirectCast(elements(2), MenuItem)
            Assert.That(verrou.IsCheckable, [Is].True)

            r.Execute("verrouiller")
            Assert.That(verrou.IsChecked, [Is].True, "lié à l'état de la commande")
            Assert.That(verrou.Header, [Is].EqualTo("Déverrouiller"))
        End Sub

        <Test>
        Public Sub LesBarres_EtLesSousMenus()
            Dim r = Registre()

            Dim barre = r.CreateToolbarItems({"enregistrer", "-", "supprimer"})
            Dim actions = r.CreateActionBarItems({"enregistrer", "verrouiller"})
            Dim menu = r.CreateMenuItems({New CommandSubmenu With {.Label = "Fichier", .Items = {CObj("enregistrer"), "supprimer"}}})

            Assert.That(barre.Count, [Is].EqualTo(3))
            Assert.That(DirectCast(barre(0), ToolbarButton).Content, [Is].EqualTo("Enregistrer"))
            Assert.That(barre(1), [Is].InstanceOf(Of ToolbarSeparator)())
            Assert.That(actions(0), [Is].InstanceOf(Of ActionBarButton)())
            Assert.That(actions(1), [Is].InstanceOf(Of ActionBarToggleButton)(), "une case : une bascule")
            Assert.That(DirectCast(menu.Single(), MenuItem).Items.Count, [Is].EqualTo(2))
        End Sub

        <Test>
        Public Sub LesRaccourcis()
            Dim r = Registre()
            Dim fenetre As New Border()

            r.BindShortcuts(fenetre)

            Dim liaison = fenetre.InputBindings.OfType(Of KeyBinding)().Single()
            Assert.That(liaison.Key, [Is].EqualTo(Key.S))
            Assert.That(liaison.Modifiers, [Is].EqualTo(ModifierKeys.Control))
            Assert.That(liaison.Command, [Is].SameAs(r("enregistrer")))
        End Sub

        <Test>
        Public Sub LesErreursDeDeclaration()
            Dim r = Registre()

            Assert.Throws(Of KeyNotFoundException)(New Action(Sub() r.Execute("inconnue")))
            Assert.Throws(Of ArgumentException)(New Action(Sub() r.Add(New CommandDefinition With {.Id = "choix", .Kind = CommandKind.Radio})))
            Assert.Throws(Of ArgumentException)(New Action(Sub() r.Add(New CommandDefinition With {.Id = "enregistrer"})))
            Assert.Throws(Of ArgumentException)(New Action(Sub() r.CreateMenuItems({CObj(42)})))
        End Sub

    End Class

End Namespace
