Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Xml.Linq

Namespace Mkm.Wpf.UiTests

    ''' <summary>
    ''' Les textes français de la vitrine et de mkm-wpf, lus dans leurs fichiers .resx : un scénario cherche « Ajouter »
    ''' par sa clé (<c>showcase.actionbar.add</c>), et reformuler un libellé ne le casse pas.
    ''' </summary>
    Public NotInheritable Class Textes

        Private Shared _textes As Dictionary(Of String, String)

        Private Sub New()
        End Sub

        ''' <summary>Le texte français d'une clé, ses paramètres remplis.</summary>
        Public Shared Function T(cle As String, ParamArray parametres As Object()) As String
            If _textes Is Nothing Then _textes = Charger()
            Dim texte As String = Nothing
            If Not _textes.TryGetValue(cle, texte) Then Throw New ArgumentException($"Texte inconnu : {cle}", NameOf(cle))
            Return If(parametres.Length = 0, texte, String.Format(CultureInfo.GetCultureInfo("fr-FR"), texte, parametres))
        End Function

        ' L'anglais d'abord, que le français remplace : une clé non traduite garde son texte anglais, comme dans la vitrine.
        Private Shared Function Charger() As Dictionary(Of String, String)
            Dim textes As New Dictionary(Of String, String)(StringComparer.Ordinal)
            For Each fichier In {"src/Mkm.Wpf/Localization/Strings.resx", "src/Mkm.Wpf/Localization/Strings.fr.resx",
                                 "apps/Mkm.Wpf.Showcase/Textes/Vitrine.resx", "apps/Mkm.Wpf.Showcase/Textes/Vitrine.fr.resx"}
                Dim document = XDocument.Load(Path.Combine(Vitrine.Racine, fichier))
                For Each donnee In document.Root.Elements("data")
                    textes(CStr(donnee.Attribute("name"))) = CStr(donnee.Element("value"))
                Next
            Next
            Return textes
        End Function

    End Class

End Namespace
