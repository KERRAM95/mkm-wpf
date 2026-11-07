Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text

Namespace Mkm.Wpf.Theming

    ''' <summary>
    ''' Les thèmes connus : ceux de mkm-wpf (<see cref="Dashboard"/>, <see cref="Ara"/>) et ceux
    ''' qu'une application enregistre (<see cref="Register"/>).
    ''' </summary>
    Public NotInheritable Class Themes

        Private Shared ReadOnly _connus As New Dictionary(Of String, ThemeDefinition)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>L'apparence de 3DEXPERIENCE, mesurée par mkm : le thème par défaut.</summary>
        Public Shared ReadOnly Property Dashboard As ThemeDefinition = Embarque("Dashboard")

        ''' <summary>Les mesures de <see cref="Dashboard"/>, aux couleurs de la charte ARA.</summary>
        Public Shared ReadOnly Property Ara As ThemeDefinition = Embarque("Ara")

        Shared Sub New()
            Register(Dashboard)
            Register(Ara)
        End Sub

        Private Sub New()
        End Sub

        ''' <summary>Tous les thèmes connus, dans l'ordre de leur enregistrement.</summary>
        Public Shared ReadOnly Property All As IReadOnlyList(Of ThemeDefinition)
            Get
                Return _connus.Values.ToList().AsReadOnly()
            End Get
        End Property

        ''' <summary>Rend un thème connu par son nom ; un thème de même nom est remplacé.</summary>
        Public Shared Sub Register(theme As ThemeDefinition)
            If theme Is Nothing Then Throw New ArgumentNullException(NameOf(theme))
            _connus(theme.Name) = theme
        End Sub

        ''' <summary>Le thème de ce nom, sans tenir compte de la casse ; <c>Nothing</c> s'il est inconnu.</summary>
        Public Shared Function Find(name As String) As ThemeDefinition
            Dim theme As ThemeDefinition = Nothing
            If name IsNot Nothing AndAlso _connus.TryGetValue(name, theme) Then Return theme
            Return Nothing
        End Function

        ' Les jetons d'un thème, embarqués dans l'assembly : ceux importés de mkm (<nom>.tokens), puis ce que
        ' la feuille CSS de mkm dit sans jeton (<nom>.css.tokens, écrit à la main), qui passe après.
        Private Shared Function Embarque(nom As String) As ThemeDefinition
            Dim texte = Lire(nom & ".tokens", obligatoire:=True) & vbLf & Lire(nom & ".css.tokens", obligatoire:=False)
            Using lecteur As New StringReader(texte)
                Return ThemeDefinition.Load(nom, lecteur)
            End Using
        End Function

        Private Shared Function Lire(fichier As String, obligatoire As Boolean) As String
            Dim ressource = "Mkm.Wpf.Tokens." & fichier
            Using flux = GetType(Themes).Assembly.GetManifestResourceStream(ressource)
                If flux Is Nothing Then
                    If obligatoire Then Throw New InvalidOperationException($"Ressource introuvable : {ressource}.")
                    Return String.Empty
                End If
                Using lecteur As New StreamReader(flux, Encoding.UTF8)
                    Return lecteur.ReadToEnd()
                End Using
            End Using
        End Function

    End Class

End Namespace
