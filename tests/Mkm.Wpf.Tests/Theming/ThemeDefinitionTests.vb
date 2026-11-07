Imports System.Collections.Generic
Imports System.IO
Imports Mkm.Wpf.Theming
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Theming

    ''' <summary>La lecture d'un thème, ses renvois et sa dérivation.</summary>
    <TestFixture>
    Public Class ThemeDefinitionTests

        Private Shared Function Theme(ParamArray lignes As String()) As ThemeDefinition
            Return ThemeDefinition.Load("Essai", New StringReader(String.Join(vbLf, lignes)))
        End Function

        <Test>
        Public Sub Lecture_IgnoreCommentairesEtLignesVides()
            Dim essai = Theme("# un commentaire", "", "navy.500 = #014491", "  text.color = {navy.500}  ")

            Assert.That(essai.Paths, [Is].EqualTo({"navy.500", "text.color"}))
            Assert.That(essai.GetValue("text.color"), [Is].EqualTo("{navy.500}"))
        End Sub

        <Test>
        Public Sub Lecture_LigneSansEgal_EstRefusee()
            Assert.Throws(Of FormatException)(New Action(Sub() Theme("navy.500 #014491")))
        End Sub

        <Test>
        Public Sub Renvois_EnChaine()
            Dim essai = Theme("navy.500 = #014491", "primary.color = {navy.500}", "actionbar.item.active.background = {primary.color}")

            Assert.That(essai.Resolve("actionbar.item.active.background"), [Is].EqualTo("#014491"))
        End Sub

        <Test>
        Public Sub Renvois_DansUneValeur()
            Dim essai = Theme("surface.400 = #a0a0a0", "overlay.popover.shadow = 0 1px 2px {surface.400}")

            Assert.That(essai.Resolve("overlay.popover.shadow"), [Is].EqualTo("0 1px 2px #a0a0a0"))
        End Sub

        <Test>
        Public Sub RenvoiAbsent_ResteVisible()
            Assert.That(Theme("text.color = {surface.700}").Resolve("text.color"), [Is].EqualTo("{surface.700}"))
        End Sub

        <Test>
        Public Sub RenvoiCirculaire_NeBouclePas()
            Dim essai = Theme("a.b = {c.d}", "c.d = {a.b}")

            Assert.That(essai.Resolve("a.b"), [Is].EqualTo("{a.b}"))
        End Sub

        <Test>
        Public Sub Derive_RemplaceSeulementLesJetonsDonnes()
            Dim base = Theme("brand.500 = #014491", "primary.color = {brand.500}", "font.size = 12px")
            Dim marque = base.Derive("Marque", New Dictionary(Of String, String) From {{"brand.500", "#d40000"}})

            Assert.That(marque.Name, [Is].EqualTo("Marque"))
            Assert.That(marque.Resolve("primary.color"), [Is].EqualTo("#d40000"))
            Assert.That(marque.Resolve("font.size"), [Is].EqualTo("12px"))
            Assert.That(base.Resolve("primary.color"), [Is].EqualTo("#014491"))
        End Sub

    End Class

End Namespace
