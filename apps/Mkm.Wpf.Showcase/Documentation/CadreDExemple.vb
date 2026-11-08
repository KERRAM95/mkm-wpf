Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls

Namespace Mkm.Wpf.Showcase.Documentation

    ''' <summary>
    ''' Le cadre d'un exemple vivant : un groupe que les lecteurs d'écran disent « Exemple : &lt;section&gt; », et que les
    ''' tests d'interaction trouvent par son identifiant, <c>exemple-&lt;section&gt;</c> (« exemple-sections »).
    ''' </summary>
    Public NotInheritable Class CadreDExemple
        Inherits Border

        ''' <summary>Le titre de la section de l'exemple.</summary>
        Public Property Section As String

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeCadre(Me)
        End Function

        ''' <summary>L'identifiant d'un titre : en minuscules, sans accents, des tirets entre les mots.</summary>
        Public Shared Function Identifiant(titre As String) As String
            Dim sansAccents = New String(titre.Normalize(NormalizationForm.FormD).
                Where(Function(c) CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark).ToArray())
            Dim mots = sansAccents.ToLowerInvariant().Split(sansAccents.Where(Function(c) Not Char.IsLetterOrDigit(c)).Distinct().ToArray(),
                                                            StringSplitOptions.RemoveEmptyEntries)
            Return String.Join("-", mots)
        End Function

        Private NotInheritable Class PairDeCadre
            Inherits FrameworkElementAutomationPeer

            Public Sub New(cadre As CadreDExemple)
                MyBase.New(cadre)
            End Sub

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.Group
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(CadreDExemple)
            End Function

            Protected Overrides Function GetNameCore() As String
                Return "Exemple : " & DirectCast(Owner, CadreDExemple).Section
            End Function

        End Class

    End Class

End Namespace
