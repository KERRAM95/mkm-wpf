Imports System.Linq
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Effects
Imports Mkm.Wpf.Theming
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Theming

    ''' <summary>La conversion d'un jeton en ressource WPF, selon son chemin et sa valeur.</summary>
    <TestFixture>
    Public Class ThemeResourcesTests

        <Test>
        Public Sub Couleur_DonneUnPinceauFige()
            Dim pinceau = DirectCast(ThemeResources.Convert("navy.500", "#014491"), SolidColorBrush)

            Assert.That(pinceau.Color, [Is].EqualTo(Color.FromRgb(&H1, &H44, &H91)))
            Assert.That(pinceau.IsFrozen, [Is].True)
        End Sub

        <TestCase("#fff", 255, 255, 255, 255)>
        <TestCase("#00000080", 128, 0, 0, 0)>
        <TestCase("rgba(0, 0, 0, 0.1)", 26, 0, 0, 0)>
        <TestCase("rgb(10 20 30)", 255, 10, 20, 30)>
        <TestCase("transparent", 0, 255, 255, 255)>
        <TestCase("color-mix(in srgb, #ffffff 50%, #000000)", 255, 128, 128, 128)>
        <TestCase("color-mix(in srgb, #ffffff, #000000)", 255, 128, 128, 128)>
        <TestCase("color-mix(in srgb, #334455 50%, transparent)", 128, 51, 68, 85)>
        <TestCase("color-mix(in srgb, #ff0000, transparent 96%)", 10, 255, 0, 0)>
        <TestCase("color-mix(in srgb, rgba(0, 0, 255, 1) 25%, #ff0000 25%)", 128, 128, 0, 128)>
        <TestCase("color-mix(in srgb, #42a2da 70%, #000)", 255, &H2E, &H71, &H99)>
        <TestCase("color-mix(in srgb, #ffffff, transparent 96%)", &HA, 255, 255, 255)>
        Public Sub Couleurs_DeCss(valeur As String, a As Integer, r As Integer, g As Integer, b As Integer)
            Dim c As Color
            Assert.That(ThemeResources.TryParseColor(valeur, c), [Is].True)

            Assert.That({c.A, c.R, c.G, c.B}, [Is].EqualTo({CByte(a), CByte(r), CByte(g), CByte(b)}))
        End Sub

        <Test>
        Public Sub Pixels_DonnentUnDouble()
            Assert.That(ThemeResources.Convert("actionbar.item.size", "40px"), [Is].EqualTo(40.0))
            Assert.That(ThemeResources.Convert("focus.ring.offset", "-1px"), [Is].EqualTo(-1.0))
            Assert.That(ThemeResources.Convert("actionbar.item.pressed.scale", "0.96"), [Is].EqualTo(0.96))
        End Sub

        <Test>
        Public Sub Marges_DansLOrdreDeWpf()
            ' CSS : haut, droite, bas, gauche ; WPF : gauche, haut, droite, bas.
            Assert.That(ThemeResources.Convert("actionbar.header.padding", "0 4px 0 8px"), [Is].EqualTo(New Thickness(8, 0, 4, 0)))
            Assert.That(ThemeResources.Convert("actionbar.tab.padding", "0 16px"), [Is].EqualTo(New Thickness(16, 0, 16, 0)))
            Assert.That(ThemeResources.Convert("list.padding", "2px"), [Is].EqualTo(New Thickness(2)))
        End Sub

        <Test>
        Public Sub Rayons_DonnentDesCoins()
            Assert.That(ThemeResources.Convert("border.radius.xs", "2px"), [Is].EqualTo(New CornerRadius(2)))
            Assert.That(ThemeResources.Convert("dialog.border.radius", "6px 6px 0 0"), [Is].EqualTo(New CornerRadius(6, 6, 0, 0)))
        End Sub

        <Test>
        Public Sub Police_SansFamilleGenerique()
            Dim police = DirectCast(ThemeResources.Convert("font.family", "Arial, Helvetica, sans-serif"), FontFamily)

            Assert.That(police.Source, [Is].EqualTo("Arial, Helvetica"))
        End Sub

        <Test>
        Public Sub Graisse_Et_Duree()
            Assert.That(ThemeResources.Convert("actionbar.tab.font.weight", "600"), [Is].EqualTo(FontWeights.SemiBold))
            Assert.That(ThemeResources.Convert("actionbar.transition.duration", "0.2s"), [Is].EqualTo(New Duration(TimeSpan.FromMilliseconds(200))))
            Assert.That(ThemeResources.Convert("transition.duration", "0s"), [Is].EqualTo(New Duration(TimeSpan.Zero)))
        End Sub

        <Test>
        Public Sub Ombre_EnDropShadowEffect()
            Dim ombre = DirectCast(ThemeResources.Convert("overlay.popover.shadow", "0 1px 2px #a0a0a0"), DropShadowEffect)

            Assert.That(ombre.ShadowDepth, [Is].EqualTo(1.0))
            Assert.That(ombre.Direction, [Is].EqualTo(270.0).Within(0.001))
            Assert.That(ombre.BlurRadius, [Is].EqualTo(2.0))
            Assert.That(ombre.Color, [Is].EqualTo(Color.FromRgb(&HA0, &HA0, &HA0)))
            Assert.That(ombre.IsFrozen, [Is].True)
        End Sub

        <Test>
        Public Sub Ombre_PremiereDePlusieurs_EtSansOmbre()
            Dim ombre = DirectCast(ThemeResources.Convert("actionbar.panel.shadow", "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1)"), DropShadowEffect)

            Assert.That(ombre.ShadowDepth, [Is].EqualTo(4.0))
            Assert.That(ombre.Opacity, [Is].EqualTo(0.1).Within(0.01))
            Assert.That(ThemeResources.Convert("focus.ring.shadow", "none"), [Is].Null)
        End Sub

        <Test>
        Public Sub MotCle_ResteUnTexte()
            Assert.That(ThemeResources.Convert("focus.ring.style", "solid"), [Is].EqualTo("solid"))
        End Sub

        <Test>
        Public Sub Build_UneRessourceParJeton()
            Dim ressources = ThemeResources.Build(Themes.Ara)
            ' Un jeton sans valeur WPF (une ombre « none ») n'a pas de ressource : WPF garde son défaut.
            Dim attendues = Themes.Ara.Paths.Count(Function(p) ThemeResources.Convert(p, Themes.Ara.Resolve(p)) IsNot Nothing)

            Assert.That(ressources.Count, [Is].EqualTo(attendues))
            Assert.That(DirectCast(ressources("mkm.primary.color"), SolidColorBrush).Color, [Is].EqualTo(Color.FromRgb(&H1, &H44, &H91)))
            Assert.That(ressources("mkm.font.size"), [Is].EqualTo(12.0))
        End Sub

    End Class

End Namespace
