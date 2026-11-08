Imports System.Globalization
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le terminal de l'exemple répond à quelques commandes.</summary>
    Friend NotInheritable Class TerminalCommandes

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim console = DirectCast(exemple.FindName("Console"), Terminal)
            AddHandler console.CommandEntered, AddressOf Repondre
        End Sub

#Region "Exemple"
        ' La réponse se pose dans e.Response ; une réponse qui vient plus tard passe par Terminal.Respond.
        Private Shared Sub Repondre(sender As Object, e As TerminalCommandEventArgs)
            Select Case e.Command.ToLowerInvariant()
                Case "aide"
                    e.Response = "Commandes : date, bonjour, aide."
                Case "date"
                    e.Response = Date.Now.ToString("D", CultureInfo.CurrentCulture)
                Case "bonjour"
                    e.Response = "Bonjour !"
                Case Else
                    e.Response = $"Commande inconnue : {e.Command}"
            End Select
        End Sub
#End Region

    End Class

End Namespace
