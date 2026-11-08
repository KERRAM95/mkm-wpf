Imports System
Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le bouton « Remontrer » fait reparaître le message « Ephemere », pour une vie neuve de 5 secondes.</summary>
    Friend NotInheritable Class MessageEphemere

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim message = DirectCast(exemple.FindName("Ephemere"), Message)
            AddHandler DirectCast(exemple.FindName("Remontrer"), Mkm.Wpf.Controls.Button).Click, Sub() Remontrer(message)
        End Sub

#Region "Exemple"
        ' Fermé (par sa croix ou au bout de sa vie), le message est caché ; le remontrer, c'est le rendre visible :
        ' il est réannoncé et sa vie repart.
        Private Shared Sub Remontrer(message As Message)
            message.Visibility = Visibility.Visible
        End Sub
#End Region

    End Class

End Namespace
