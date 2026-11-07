Imports System.Collections.Generic
Imports System.Globalization

Namespace Mkm.Wpf.Localization

    ''' <summary>
    ''' Une table de textes que l'application remplit elle-même — par exemple depuis des fichiers
    ''' XML qu'un administrateur édite sans recompiler : clé, langue, texte.
    ''' </summary>
    ''' <remarks>Elle se remplit et se lit sur le thread de l'interface.</remarks>
    Public NotInheritable Class TextTable
        Implements ITextSource

        Private ReadOnly _texts As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.Ordinal)
        Private ReadOnly _languages As New List(Of CultureInfo)

        ''' <summary>Pose le texte d'une clé dans une langue ; un texte déjà posé est remplacé.</summary>
        Public Sub Add(key As String, language As CultureInfo, text As String)
            If key Is Nothing Then Throw New ArgumentNullException(NameOf(key))
            If language Is Nothing Then Throw New ArgumentNullException(NameOf(language))

            Dim parLangue As Dictionary(Of String, String) = Nothing
            If Not _texts.TryGetValue(key, parLangue) Then
                parLangue = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                _texts.Add(key, parLangue)
            End If
            parLangue(language.Name) = text

            If Not _languages.Contains(language) Then _languages.Add(language)
        End Sub

        ''' <summary>Pose le texte d'une clé dans une langue donnée par son nom de culture (« fr », « en-GB »).</summary>
        Public Sub Add(key As String, language As String, text As String)
            Add(key, CultureInfo.GetCultureInfo(language), text)
        End Sub

        ''' <inheritdoc/>
        Public ReadOnly Property Languages As IReadOnlyCollection(Of CultureInfo) Implements ITextSource.Languages
            Get
                Return _languages.AsReadOnly()
            End Get
        End Property

        ''' <inheritdoc/>
        Public Function GetText(key As String, language As CultureInfo) As String Implements ITextSource.GetText
            If key Is Nothing OrElse language Is Nothing Then Return Nothing

            Dim parLangue As Dictionary(Of String, String) = Nothing
            Dim texte As String = Nothing
            If _texts.TryGetValue(key, parLangue) AndAlso parLangue.TryGetValue(language.Name, texte) Then Return texte
            Return Nothing
        End Function

    End Class

End Namespace
