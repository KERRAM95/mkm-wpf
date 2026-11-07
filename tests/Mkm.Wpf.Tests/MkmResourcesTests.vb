Imports System.Globalization
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports Mkm.Wpf.Localization
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests

    ''' <summary>La configuration posée dans App.xaml.</summary>
    <TestFixture>
    Public Class MkmResourcesTests

        <Test>
        Public Sub Language_EstCelleDuLocalizer()
            Dim avant = Localizer.Current.Language
            Try
                Dim ressources As New MkmResources With {.Language = "fr"}

                Assert.That(ressources.Language, [Is].EqualTo("fr"))
                Assert.That(Localizer.Current.Language.Name, [Is].EqualTo("fr"))
            Finally
                Localizer.Current.Language = avant
            End Try
        End Sub

        <Test>
        Public Sub Theme_ParDefaut_Dashboard()
            Dim ressources As New MkmResources()

            Assert.That(ressources.Theme, [Is].EqualTo("Dashboard"))
            Assert.That(DirectCast(ressources("mkm.primary.color"), SolidColorBrush).Color, [Is].EqualTo(Color.FromRgb(&H36, &H8E, &HC4)))
        End Sub

        <Test>
        Public Sub Theme_ChangeAChaud()
            Dim ressources As New MkmResources()

            ressources.Theme = "Ara"

            Assert.That(ressources.Theme, [Is].EqualTo("Ara"))
            Assert.That(DirectCast(ressources("mkm.primary.color"), SolidColorBrush).Color, [Is].EqualTo(Color.FromRgb(&H1, &H44, &H91)))
            ' Le thème est remplacé, pas ajouté : ses ressources et les styles natifs.
            Assert.That(ressources.MergedDictionaries.Count, [Is].EqualTo(2))
        End Sub

        <Test>
        Public Sub StylesNatifs_HabillentLesInfoBulles()
            Dim ressources As New MkmResources()

            Assert.That(ressources(GetType(ToolTip)), [Is].InstanceOf(Of Style)())
        End Sub

        <Test>
        Public Sub Theme_Inconnu_EstRefuse()
            Dim ressources As New MkmResources()

            Assert.Throws(Of ArgumentException)(New Action(Sub() ressources.Theme = "Inconnu"))
        End Sub

        <Test>
        Public Sub Elements_DessinentLeurTexteNet()
            Assert.That(TextOptions.GetTextFormattingMode(New TextBlock()), [Is].EqualTo(TextFormattingMode.Display))
        End Sub

        <Test>
        Public Sub Elements_PrennentLeFormatRegionalDeWindows()
            ' EnvironnementWpf crée une MkmResources avant tout élément WPF, comme App.xaml.
            Assert.That(New TextBlock().Language.IetfLanguageTag,
                        [Is].EqualTo(CultureInfo.CurrentCulture.IetfLanguageTag).IgnoreCase)
        End Sub

    End Class

End Namespace
