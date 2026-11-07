Imports System.Collections
Imports System.Globalization
Imports System.Linq
Imports System.Resources
Imports Mkm.Wpf.Localization
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Localization

    ''' <summary>
    ''' Les textes des fichiers .resx d'essai (Textes/Essai.resx, en anglais, et Essai.fr.resx).
    ''' </summary>
    <TestFixture>
    Public Class ResxTextSourceTests

        ' VB nomme une ressource par le nom de son fichier, sans son dossier.
        Private Shared ReadOnly _textes As New ResourceManager("Essai", GetType(ResxTextSourceTests).Assembly)

        Private Shared Function Source() As ResxTextSource
            Return New ResxTextSource(_textes, "en", "fr")
        End Function

        <Test>
        Public Sub LangueNeutre_EstLeFichierSansSuffixe()
            Assert.That(Source().GetText("essai.title", CultureInfo.GetCultureInfo("en")), [Is].EqualTo("Sample title"))
        End Sub

        <Test>
        Public Sub AutreLangue_EstSonFichier()
            Assert.That(Source().GetText("essai.title", CultureInfo.GetCultureInfo("fr")), [Is].EqualTo("Titre d'essai"))
        End Sub

        <Test>
        Public Sub TexteAbsentDUneLangue_PasDeRepliDansLaSource()
            ' Le repli sur l'anglais est l'affaire du Localizer, pas de la source.
            Assert.That(Source().GetText("essai.onlyEnglish", CultureInfo.GetCultureInfo("fr")), [Is].Null)
        End Sub

        <Test>
        Public Sub LangueNonDeclaree_Rien()
            Assert.That(Source().GetText("essai.title", CultureInfo.GetCultureInfo("de")), [Is].Null)
        End Sub

        <Test>
        Public Sub DansLeLocalizer_RepliSurLAnglais()
            Dim traducteur As New Localizer With {.Language = CultureInfo.GetCultureInfo("fr-FR")}
            traducteur.AddSource(Source())

            Assert.That(traducteur("essai.title"), [Is].EqualTo("Titre d'essai"))
            Assert.That(traducteur("essai.onlyEnglish"), [Is].EqualTo("Only in English"))
            Assert.That(traducteur.T("essai.items", New With {.count = 2}), [Is].EqualTo("Il y a 2 éléments"))
        End Sub

        ''' <summary>
        ''' La garde de chaque fichier .resx de mkm-wpf : une clé de la langue neutre sans sa
        ''' traduction fait échouer ce test. Ici sur l'essai, qui en a une exprès.
        ''' </summary>
        <Test>
        Public Sub CleSansTraduction_EstReperee()
            Dim manquantes = ClesSansTraduction(_textes, CultureInfo.GetCultureInfo("fr"))

            Assert.That(manquantes, [Is].EqualTo({"essai.onlyEnglish"}))
        End Sub

        ''' <summary>Les clés de la langue neutre qui n'ont pas de texte dans <paramref name="langue"/>.</summary>
        Friend Shared Function ClesSansTraduction(textes As ResourceManager, langue As CultureInfo) As String()
            Dim cles = Function(culture As CultureInfo) textes.GetResourceSet(culture, True, False).
                Cast(Of DictionaryEntry)().
                Select(Function(entree) CStr(entree.Key))

            Return cles(CultureInfo.InvariantCulture).Except(cles(langue)).OrderBy(Function(c) c, StringComparer.Ordinal).ToArray()
        End Function

    End Class

End Namespace
