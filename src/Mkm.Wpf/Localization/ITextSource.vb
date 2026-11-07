Imports System.Collections.Generic
Imports System.Globalization

Namespace Mkm.Wpf.Localization

    ''' <summary>
    ''' Une source de textes traduits : des fichiers .resx (<see cref="ResxTextSource"/>), ou une
    ''' table que l'application remplit depuis ses propres fichiers (<see cref="TextTable"/>).
    ''' </summary>
    Public Interface ITextSource

        ''' <summary>Les langues dont la source a des textes.</summary>
        ReadOnly Property Languages As IReadOnlyCollection(Of CultureInfo)

        ''' <summary>
        ''' Le texte de <paramref name="key"/> dans <paramref name="language"/> exactement, sans
        ''' repli sur une autre langue (c'est <see cref="Localizer"/> qui choisit le repli) ;
        ''' <c>Nothing</c> si la source ne l'a pas.
        ''' </summary>
        Function GetText(key As String, language As CultureInfo) As String

    End Interface

End Namespace
