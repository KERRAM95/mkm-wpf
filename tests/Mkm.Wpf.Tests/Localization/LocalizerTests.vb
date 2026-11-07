Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Globalization
Imports System.Linq
Imports Mkm.Wpf.Localization
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Localization

    ''' <summary>
    ''' La recherche d'un texte : langue choisie, langue parente, anglais en repli, clé enfin ; les
    ''' sources de l'application avant celles de la bibliothèque ; les paramètres.
    ''' </summary>
    <TestFixture>
    Public Class LocalizerTests

        Private Shared Function Culture(nom As String) As CultureInfo
            Return CultureInfo.GetCultureInfo(nom)
        End Function

        Private Shared Function Table(ParamArray textes As String()) As TextTable
            ' « clé|langue|texte »
            Dim resultat As New TextTable()
            For Each ligne In textes
                Dim parties = ligne.Split("|"c)
                resultat.Add(parties(0), parties(1), parties(2))
            Next
            Return resultat
        End Function

        <Test>
        Public Sub Texte_DansLaLangueChoisie()
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddSource(Table("save|en|Save", "save|fr|Enregistrer"))

            Assert.That(traducteur("save"), [Is].EqualTo("Enregistrer"))
        End Sub

        <Test>
        Public Sub Texte_DeLaLangueParente()
            Dim traducteur As New Localizer With {.Language = Culture("fr-CA")}
            traducteur.AddSource(Table("save|en|Save", "save|fr|Enregistrer"))

            Assert.That(traducteur("save"), [Is].EqualTo("Enregistrer"))
        End Sub

        <Test>
        Public Sub Texte_EnAnglaisQuandIlManque()
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddSource(Table("save|en|Save"))

            Assert.That(traducteur("save"), [Is].EqualTo("Save"))
        End Sub

        <Test>
        Public Sub Texte_DUneVarianteDAnglaisQuandIlManque()
            ' Une source qui n'a que de l'anglais britannique (« GB ») sert aussi de repli.
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddSource(Table("save|en-GB|Save"))

            Assert.That(traducteur("save"), [Is].EqualTo("Save"))
        End Sub

        <Test>
        Public Sub Texte_AbsentPartout_VautSaCle()
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddSource(Table("save|en|Save"))

            Assert.That(traducteur("inconnu.cle"), [Is].EqualTo("inconnu.cle"))
        End Sub

        <Test>
        Public Sub SourcesDeLApplication_AvantCellesDeLaBibliotheque()
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddLibrarySource(Table("close|fr|Fermer", "more|fr|Plus"))
            traducteur.AddSource(Table("close|fr|Quitter"))

            Assert.That(traducteur("close"), [Is].EqualTo("Quitter"))
            Assert.That(traducteur("more"), [Is].EqualTo("Plus"))
        End Sub

        <Test>
        Public Sub LangueExacte_AvantLOrdreDesSources()
            ' Le français de la bibliothèque passe avant l'anglais de l'application.
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddLibrarySource(Table("close|fr|Fermer"))
            traducteur.AddSource(Table("close|en|Close"))

            Assert.That(traducteur("close"), [Is].EqualTo("Fermer"))
        End Sub

        <Test>
        Public Sub Parametres_ParNom()
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddSource(Table("items|fr|{count} éléments sur {total}"))

            Assert.That(traducteur.T("items", New With {.count = 3, .total = 10}), [Is].EqualTo("3 éléments sur 10"))
        End Sub

        <Test>
        Public Sub Parametres_ParPosition()
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddSource(Table("items|fr|{0} éléments sur {1}"))

            Assert.That(traducteur.T("items", 3, 10), [Is].EqualTo("3 éléments sur 10"))
        End Sub

        <Test>
        Public Sub Parametre_Absent_ResteVisible()
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            traducteur.AddSource(Table("items|fr|{count} éléments"))

            Assert.That(traducteur.T("items", New With {.total = 10}), [Is].EqualTo("{count} éléments"))
        End Sub

        <Test>
        Public Sub Langues_DesSourcesDeLApplication()
            Dim traducteur As New Localizer()
            traducteur.AddLibrarySource(Table("close|en|Close", "close|fr|Fermer"))
            traducteur.AddSource(Table("title|fr-FR|Titre", "title|en-GB|Title"))

            Assert.That(traducteur.Languages.Select(Function(l) l.Name), [Is].EquivalentTo({"fr-FR", "en-GB"}))
        End Sub

        <Test>
        Public Sub Langues_DeLaBibliotheque_SansSourceDApplication()
            Dim traducteur As New Localizer()
            traducteur.AddLibrarySource(Table("close|en|Close", "close|fr|Fermer"))

            Assert.That(traducteur.Languages.Select(Function(l) l.Name), [Is].EquivalentTo({"en", "fr"}))
        End Sub

        <TestCase("fr-FR", "fr-FR")>
        <TestCase("fr", "fr-FR")>
        <TestCase("fr-CA", "fr-FR")>
        <TestCase("en-US", "en-GB")>
        <TestCase("de-DE", "en-GB")>
        Public Sub MeilleureLangue(preferee As String, attendue As String)
            Dim traducteur As New Localizer()
            traducteur.AddSource(Table("title|fr-FR|Titre", "title|en-GB|Title"))

            Assert.That(traducteur.BestLanguage(Culture(preferee)).Name, [Is].EqualTo(attendue))
        End Sub

        <Test>
        Public Sub ChangerDeLangue_PrevientLesLiaisons()
            Dim traducteur As New Localizer With {.Language = Culture("en")}
            Dim proprietes As New List(Of String)
            Dim changements = 0
            AddHandler traducteur.PropertyChanged, Sub(s, e) proprietes.Add(e.PropertyName)
            AddHandler traducteur.LanguageChanged, Sub(s, e) changements += 1

            traducteur.Language = Culture("fr")

            Assert.That(proprietes, [Is].EqualTo({"Language", "Item[]"}))
            Assert.That(changements, [Is].EqualTo(1))
        End Sub

        <Test>
        Public Sub MemeLangue_NePrevientPersonne()
            Dim traducteur As New Localizer With {.Language = Culture("fr")}
            Dim proprietes As New List(Of String)
            AddHandler traducteur.PropertyChanged, Sub(s, e) proprietes.Add(e.PropertyName)

            traducteur.Language = Culture("fr")

            Assert.That(proprietes, [Is].Empty)
        End Sub

    End Class

End Namespace
