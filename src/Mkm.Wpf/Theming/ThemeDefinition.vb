Imports System.Collections.Generic
Imports System.IO
Imports System.Text.RegularExpressions

Namespace Mkm.Wpf.Theming

    ''' <summary>
    ''' Un thème : ses jetons, par chemin (« text.color », « actionbar.item.size »), aux noms de mkm
    ''' (<c>dt('text.color')</c>). Une valeur peut renvoyer à d'autres jetons : « {surface.700} »,
    ''' « 0 1px 2px {surface.400} ».
    ''' </summary>
    Public NotInheritable Class ThemeDefinition

        Private Shared ReadOnly _renvoi As New Regex("\{([a-z0-9.]+)\}", RegexOptions.Compiled)

        Private ReadOnly _tokens As Dictionary(Of String, String)

        Private Sub New(name As String, tokens As Dictionary(Of String, String))
            If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Un thème a un nom.", NameOf(name))
            Me.Name = name
            _tokens = tokens
        End Sub

        ''' <summary>Le nom du thème : « Dashboard », « Ara ».</summary>
        Public ReadOnly Property Name As String

        ''' <summary>Les chemins de tous les jetons du thème.</summary>
        Public ReadOnly Property Paths As IEnumerable(Of String)
            Get
                Return _tokens.Keys
            End Get
        End Property

        ''' <summary>
        ''' Lit un thème au format des fichiers <c>.tokens</c> : une ligne « chemin = valeur » par
        ''' jeton ; une ligne vide ou qui commence par « # » est ignorée.
        ''' </summary>
        Public Shared Function Load(name As String, reader As TextReader) As ThemeDefinition
            If reader Is Nothing Then Throw New ArgumentNullException(NameOf(reader))

            Dim tokens As New Dictionary(Of String, String)(StringComparer.Ordinal)
            Dim numero = 0
            Dim ligne = reader.ReadLine()
            While ligne IsNot Nothing
                numero += 1
                Dim texte = ligne.Trim()
                If texte.Length > 0 AndAlso Not texte.StartsWith("#", StringComparison.Ordinal) Then
                    Dim egal = texte.IndexOf(" = ", StringComparison.Ordinal)
                    If egal <= 0 Then Throw New FormatException($"Thème {name}, ligne {numero} : « chemin = valeur » attendu.")
                    tokens(texte.Substring(0, egal)) = texte.Substring(egal + 3).Trim()
                End If
                ligne = reader.ReadLine()
            End While

            Return New ThemeDefinition(name, tokens)
        End Function

        ''' <summary>La valeur d'un jeton telle qu'écrite, renvois compris ; <c>Nothing</c> s'il n'existe pas.</summary>
        Public Function GetValue(path As String) As String
            Dim valeur As String = Nothing
            If path IsNot Nothing AndAlso _tokens.TryGetValue(path, valeur) Then Return valeur
            Return Nothing
        End Function

        ''' <summary>
        ''' La valeur d'un jeton, ses renvois remplacés par les valeurs qu'ils désignent. Un renvoi à un
        ''' jeton absent (ou qui reviendrait sur lui-même) reste tel quel : l'erreur se voit.
        ''' </summary>
        Public Function Resolve(path As String) As String
            Dim valeur = GetValue(path)
            If valeur Is Nothing Then Return Nothing
            Return Developper(valeur, New HashSet(Of String)(StringComparer.Ordinal) From {path})
        End Function

        ''' <summary>
        ''' Un thème dérivé de celui-ci : les jetons donnés remplacent les siens, comme
        ''' <c>definePreset</c> dans mkm. Une application y pose, par exemple, sa couleur de marque.
        ''' </summary>
        Public Function Derive(name As String, changes As IDictionary(Of String, String)) As ThemeDefinition
            If changes Is Nothing Then Throw New ArgumentNullException(NameOf(changes))

            Dim fusion As New Dictionary(Of String, String)(_tokens, StringComparer.Ordinal)
            For Each jeton In changes
                fusion(jeton.Key) = jeton.Value
            Next
            Return New ThemeDefinition(name, fusion)
        End Function

        Private Function Developper(valeur As String, suivis As HashSet(Of String)) As String
            Return _renvoi.Replace(valeur, Function(m As Match) As String
                                               Dim cible = m.Groups(1).Value
                                               Dim brut As String = Nothing
                                               If suivis.Contains(cible) OrElse Not _tokens.TryGetValue(cible, brut) Then Return m.Value

                                               Dim suite As New HashSet(Of String)(suivis, StringComparer.Ordinal)
                                               suite.Add(cible)
                                               Return Developper(brut, suite)
                                           End Function)
        End Function

    End Class

End Namespace
