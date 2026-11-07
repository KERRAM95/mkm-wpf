Imports System.Globalization
Imports System.Windows.Controls
Imports System.Windows.Markup
Imports Mkm.Wpf.Localization
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Localization

    ''' <summary>
    ''' <c>{mkm:T clé}</c> en XAML : le texte, puis le texte qui suit la langue sans redémarrer.
    ''' Ces tests emploient <see cref="Localizer.Current"/> et le remettent en état.
    ''' </summary>
    <TestFixture>
    Public Class TExtensionTests

        Private _table As TextTable
        Private _langueAvant As CultureInfo

        <SetUp>
        Public Sub Preparer()
            _langueAvant = Localizer.Current.Language
            _table = New TextTable()
            _table.Add("essai.title", "en", "Title")
            _table.Add("essai.title", "fr", "Titre")
            Localizer.Current.AddSource(_table)
        End Sub

        <TearDown>
        Public Sub Ranger()
            Localizer.Current.RemoveSource(_table)
            Localizer.Current.Language = _langueAvant
        End Sub

        Private Shared Function Bloc(xaml As String) As TextBlock
            Return DirectCast(XamlReader.Parse(
                "<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:mkm='urn:mkm-wpf' " & xaml & " />"), TextBlock)
        End Function

        <Test>
        Public Sub Texte_DansLaLangueChoisie()
            Localizer.Current.Language = CultureInfo.GetCultureInfo("fr")

            Assert.That(Bloc("Text='{mkm:T essai.title}'").Text, [Is].EqualTo("Titre"))
        End Sub

        <Test>
        Public Sub Texte_SuitLeChangementDeLangue()
            Localizer.Current.Language = CultureInfo.GetCultureInfo("en")
            Dim texte = Bloc("Text='{mkm:T essai.title}'")
            Assert.That(texte.Text, [Is].EqualTo("Title"))

            Localizer.Current.Language = CultureInfo.GetCultureInfo("fr")

            Assert.That(texte.Text, [Is].EqualTo("Titre"))
        End Sub

        <Test>
        Public Sub Cle_EnAttribut()
            Localizer.Current.Language = CultureInfo.GetCultureInfo("en")

            Assert.That(Bloc("Text='{mkm:T Key=essai.title}'").Text, [Is].EqualTo("Title"))
        End Sub

        <Test>
        Public Sub CleInvalide_EstRefusee()
            Assert.Throws(Of XamlParseException)(New Action(Sub() Bloc("Text='{mkm:T essai[0]}'")))
        End Sub

    End Class

End Namespace
