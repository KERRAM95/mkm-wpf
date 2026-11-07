Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Resources

Namespace Mkm.Wpf.Localization

    ''' <summary>
    ''' Les textes de fichiers .resx : <c>Nom.resx</c> pour la langue neutre (l'anglais
    ''' d'ordinaire), <c>Nom.fr.resx</c>, <c>Nom.de.resx</c>… pour les autres.
    ''' </summary>
    Public NotInheritable Class ResxTextSource
        Implements ITextSource

        Private ReadOnly _manager As ResourceManager
        Private ReadOnly _neutral As CultureInfo
        Private ReadOnly _languages As IReadOnlyCollection(Of CultureInfo)

        ''' <param name="manager">Le <see cref="ResourceManager"/> des fichiers .resx.</param>
        ''' <param name="neutralLanguage">La langue du fichier sans suffixe (<c>Nom.resx</c>) : « en ».</param>
        ''' <param name="otherLanguages">Les langues des autres fichiers : « fr » pour <c>Nom.fr.resx</c>.</param>
        Public Sub New(manager As ResourceManager, neutralLanguage As String, ParamArray otherLanguages As String())
            If manager Is Nothing Then Throw New ArgumentNullException(NameOf(manager))
            If neutralLanguage Is Nothing Then Throw New ArgumentNullException(NameOf(neutralLanguage))

            _manager = manager
            _neutral = CultureInfo.GetCultureInfo(neutralLanguage)

            Dim langues As New List(Of CultureInfo) From {_neutral}
            If otherLanguages IsNot Nothing Then
                langues.AddRange(otherLanguages.Select(Function(nom) CultureInfo.GetCultureInfo(nom)))
            End If
            _languages = langues.Distinct().ToList().AsReadOnly()
        End Sub

        ''' <inheritdoc/>
        Public ReadOnly Property Languages As IReadOnlyCollection(Of CultureInfo) Implements ITextSource.Languages
            Get
                Return _languages
            End Get
        End Property

        ''' <inheritdoc/>
        Public Function GetText(key As String, language As CultureInfo) As String Implements ITextSource.GetText
            If key Is Nothing OrElse language Is Nothing OrElse Not _languages.Contains(language) Then Return Nothing

            ' La langue neutre est le fichier sans suffixe, que .NET range sous la culture invariante.
            Dim culture = If(language.Equals(_neutral), CultureInfo.InvariantCulture, language)
            Dim jeu = _manager.GetResourceSet(culture, True, False)
            If jeu Is Nothing Then Return Nothing
            Return TryCast(jeu.GetObject(key), String)
        End Function

    End Class

End Namespace
