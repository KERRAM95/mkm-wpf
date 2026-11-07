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

        ' Les jetons importés de mkm (Theming/Tokens/*.tokens), embarqués dans l'assembly.
        Private Shared Function Embarque(nom As String) As ThemeDefinition
            Dim ressource = "Mkm.Wpf.Tokens." & nom & ".tokens"
            Using flux = GetType(Themes).Assembly.GetManifestResourceStream(ressource)
                If flux Is Nothing Then Throw New InvalidOperationException($"Ressource introuvable : {ressource}.")
                Using lecteur As New StreamReader(flux, Encoding.UTF8)
                    Return ThemeDefinition.Load(nom, lecteur)
                End Using
            End Using
        End Function

    End Class

End Namespace
