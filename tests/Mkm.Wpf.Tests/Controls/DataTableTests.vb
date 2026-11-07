Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le tableau de données de mkm : ses mesures, ses lignes, son message vide, son voile, ses tailles.</summary>
    <TestFixture>
    Public Class DataTableTests

        ''' <summary>Une ligne d'exemple.</summary>
        Public Class Ligne
            Public Property Nom As String
            Public Property Valeur As String
        End Class

        Private Shared Function Tableau(Optional lignes As Integer = 3) As DataTable
            Dim t As New DataTable With {.AutoGenerateColumns = False, .Width = 400, .Height = 300}
            t.Columns.Add(New DataGridTextColumn With {.Header = "Nom", .Binding = New Binding("Nom")})
            t.Columns.Add(New DataGridTextColumn With {.Header = "Valeur", .Binding = New Binding("Valeur")})
            t.ItemsSource = New ObservableCollection(Of Ligne)(Enumerable.Range(1, lignes).Select(Function(i) New Ligne With {.Nom = $"V{i}", .Valeur = $"{i}"}))
            t.BeginInit()
            t.EndInit()
            t.ApplyTemplate()
            t.Measure(New Size(400, 300))
            t.Arrange(New Rect(0, 0, 400, 300))
            t.UpdateLayout()
            Return t
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        <Test>
        Public Sub DataTable_LesMesuresDeMkm()
            Dim t = Tableau()

            Assert.That(t.RowHeight, [Is].EqualTo(32.0))
            Assert.That(t.HeadersVisibility, [Is].EqualTo(DataGridHeadersVisibility.Column))
            Assert.That(t.GridLinesVisibility, [Is].EqualTo(DataGridGridLinesVisibility.None))
            Assert.That(t.CanUserAddRows, [Is].False, "pas de ligne vide d'ajout")
            Assert.That(t.ItemContainerGenerator.ContainerFromIndex(0), [Is].InstanceOf(Of DataGridRow)())
        End Sub

        <Test>
        Public Sub DataTable_ZebreeQuadrilleePetite()
            Dim t = Tableau()

            t.Striped = True
            t.GridLines = True
            t.Size = DataTableSize.Small

            Assert.That(t.AlternationCount, [Is].EqualTo(2))
            Assert.That(t.GridLinesVisibility, [Is].EqualTo(DataGridGridLinesVisibility.All))
            Assert.That(Double.IsNaN(t.RowHeight), [Is].True)
            Assert.That(t.CellPadding.Top, [Is].EqualTo(4.0))
        End Sub

        <Test>
        Public Sub DataTable_LesCasesDeMkm()
            Dim t = Tableau()
            Dim colonne As New DataGridCheckBoxColumn With {.Header = "Active"}

            t.Columns.Add(colonne)

            Assert.That(colonne.ElementStyle, [Is].Not.SameAs(DataGridCheckBoxColumn.DefaultElementStyle))
            Assert.That(colonne.ElementStyle.BasedOn, [Is].SameAs(t.TryFindResource(DataTable.CheckBoxStyleKey)))
            Assert.That(colonne.EditingElementStyle, [Is].SameAs(t.TryFindResource(DataTable.CheckBoxStyleKey)))
        End Sub

        <Test>
        Public Sub DataTable_LeVoileDuChargement()
            Dim t = Tableau()
            Dim voile = Partie(Of UIElement)(t, "Voile")
            Assert.That(voile.Visibility, [Is].EqualTo(Visibility.Collapsed))

            t.Loading = True

            Assert.That(voile.Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

        <Test>
        Public Sub DataTable_EnTeteEtPiedSeulementSIlsOntUnContenu()
            Dim t = Tableau()
            Assert.That(Partie(Of UIElement)(t, "EnTete").Visibility, [Is].EqualTo(Visibility.Collapsed))

            t.Header = "Variables"

            Assert.That(Partie(Of UIElement)(t, "EnTete").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of UIElement)(t, "Pied").Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub DataTable_VideMontreSonMessage()
            Dim t = Tableau(0)
            t.EmptyMessage = "Aucune variable."
            t.UpdateLayout()

            Dim defilement = Partie(Of ScrollViewer)(t, "DG_ScrollViewer")
            Dim vide = DirectCast(defilement.Template.FindName("Vide", defilement), UIElement)

            Assert.That(vide.Visibility, [Is].EqualTo(Visibility.Visible))
        End Sub

    End Class

End Namespace
