Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les défauts relevés au contrôle qualité des lots 5 à 9, et leur correction.</summary>
    <TestFixture>
    Public Class ControleQualiteTests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 1000) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            MettreEnPage(element, largeur)
            Return element
        End Function

        Private Shared Sub MettreEnPage(element As FrameworkElement, largeur As Double)
            element.Measure(New Size(largeur, 400))
            element.Arrange(New Rect(0, 0, largeur, 400))
            element.UpdateLayout()
        End Sub

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        Private Shared Function Arbre(ByRef vues As Lot8Tests.Noeud, ByRef plan As Lot8Tests.Noeud) As TreeGrid
            vues = New Lot8Tests.Noeud("Vues", New Lot8Tests.Noeud("Face"))
            plan = New Lot8Tests.Noeud("Plan", vues)
            Dim g As New TreeGrid With {.Nodes = New ObservableCollection(Of Lot8Tests.Noeud)({plan})}
            g.Columns.Add(New TreeGridColumn With {.Header = "Nom", .Binding = New Binding("Nom")})
            Return Pret(g)
        End Function

        <Test>
        Public Sub TreeGrid_SupprNeRetirePasDeLigne()
            Dim vues As Lot8Tests.Noeud = Nothing
            Dim plan As Lot8Tests.Noeud = Nothing
            Dim g = Arbre(vues, plan)

            Assert.That(g.CanUserDeleteRows, [Is].False)
        End Sub

        <Test>
        Public Sub TreeGrid_LeDepliageDesDescendantsEstGarde()
            Dim vues As Lot8Tests.Noeud = Nothing
            Dim plan As Lot8Tests.Noeud = Nothing
            Dim g = Arbre(vues, plan)
            ' Déplier un enfant avant son parent : gardé pour quand le parent se déplie.
            g.Expand(vues)
            g.Expand(plan)
            Assert.That(g.Items.Count, [Is].EqualTo(3))

            g.Collapse(plan)
            g.Expand(plan)

            Assert.That(g.IsExpanded(vues), [Is].True)
            Assert.That(g.Items.Count, [Is].EqualTo(3), "Plan, Vues, Face")
        End Sub

        <Test>
        Public Sub Triptych_UneOuvertureParLiaison_FermeLAutreEnSuperposition()
            Dim t As New Triptych With {.Left = New TextBlock(), .Right = New TextBlock(), .LeftVisible = True}
            Pret(t, 500)
            Assert.That(t.IsOverlay, [Is].True)

            ' Comme une liaison : la propriété change, sans passer par Show.
            t.SetCurrentValue(Triptych.RightVisibleProperty, True)

            Assert.That(t.LeftVisible, [Is].False, "un seul panneau à la fois")
        End Sub

        <Test>
        Public Sub WelcomePanel_UnEnTeteRemplaceLeTitre()
            Dim p = Pret(New WelcomePanel With {.Title = "HS CART", .Header = New TextBlock With {.Text = "Autre"}}, 260)

            Assert.That(Partie(Of UIElement)(p, "TitreEtDescription").Visibility, [Is].EqualTo(Visibility.Collapsed))
            Assert.That(Partie(Of UIElement)(p, "EnTetePropre").Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub Splitter_LePasDuClavierEnPixels()
            Dim s As New Splitter()
            s.Panels.Add(New SplitterPanel())
            s.Panels.Add(New SplitterPanel())
            Pret(s, 406)

            ' 406 - 2 de bordure - 4 de gouttière : 400 px ; 5 px en font 1,25 %.
            Assert.That(s.PasEnPourcentage(), [Is].EqualTo(1.25).Within(0.0001))
        End Sub

    End Class

End Namespace
