Imports System.Linq
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les icônes de mkm et l'élément qui les dessine.</summary>
    <TestFixture>
    Public Class IconTests

        Private Shared Function Geometries() As PropertyInfo()
            Return GetType(Icons).GetProperties(BindingFlags.Public Or BindingFlags.Static).
                Where(Function(p) p.PropertyType Is GetType(Geometry)).
                ToArray()
        End Function

        <Test>
        Public Sub LeJeuDeMkm_AQuatreVingtQuinzeIcones()
            Assert.That(Geometries().Length, [Is].EqualTo(95))
        End Sub

        <Test>
        Public Sub ChaqueIcone_EstUneGeometrieFigeeSurLaGrilleDe24()
            Dim fautives = Geometries().
                Select(Function(p) New With {p.Name, .Geometrie = DirectCast(p.GetValue(Nothing), Geometry)}).
                Where(Function(i) i.Geometrie Is Nothing OrElse Not i.Geometrie.IsFrozen OrElse i.Geometrie.Bounds.IsEmpty OrElse
                                  i.Geometrie.Bounds.Left < -0.5 OrElse i.Geometrie.Bounds.Top < -0.5 OrElse
                                  i.Geometrie.Bounds.Right > 24.5 OrElse i.Geometrie.Bounds.Bottom > 24.5).
                Select(Function(i) i.Name).
                ToList()

            Assert.That(fautives, [Is].Empty)
        End Sub

        <Test>
        Public Sub SansTaille_SeizePixels()
            Dim icone As New Icon With {.Source = Icons.Add}

            icone.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))

            Assert.That(icone.DesiredSize, [Is].EqualTo(New Size(16, 16)))
        End Sub

        <Test>
        Public Sub Couleur_HeriteeDuTexte()
            Dim porteur As New StackPanel()
            TextElement.SetForeground(porteur, Brushes.Red)
            Dim icone As New Icon With {.Source = Icons.Add}
            porteur.Children.Add(icone)

            Assert.That(icone.Foreground, [Is].SameAs(Brushes.Red))
        End Sub

        <Test>
        Public Sub NePrendNiLeFocusNiLesClics()
            Dim icone As New Icon()

            Assert.That(icone.Focusable, [Is].False)
            Assert.That(icone.IsHitTestVisible, [Is].False)
        End Sub

    End Class

End Namespace
