Imports System.Globalization
Imports System.Windows.Controls
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
        Public Sub Elements_PrennentLeFormatRegionalDeWindows()
            ' EnvironnementWpf crée une MkmResources avant tout élément WPF, comme App.xaml.
            Assert.That(New TextBlock().Language.IetfLanguageTag,
                        [Is].EqualTo(CultureInfo.CurrentCulture.IetfLanguageTag).IgnoreCase)
        End Sub

    End Class

End Namespace
