Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le séparateur de l'exemple : à la fin d'un glissement, ses tailles s'écrivent dans le journal.</summary>
    Friend NotInheritable Class SplitterTailles

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim partage = DirectCast(exemple.FindName("Partage"), Splitter)
            AddHandler partage.ResizeEnd, AddressOf Partage_ResizeEnd
        End Sub

#Region "Exemple"
        ' ResizeEnd : la gouttière lâchée (souris ou clavier) ; GetSizes donne les tailles, en pourcentages.
        Private Shared Sub Partage_ResizeEnd(sender As Object, e As EventArgs)
            Dim partage = DirectCast(sender, Splitter)
            Dim tailles = partage.GetSizes().Select(Function(t) t.ToString("0", CultureInfo.CurrentCulture) & " %")
            Dim journal = DirectCast(partage.FindName("Journal"), TextBlock)
            journal.Text = "Tailles : " & String.Join(" / ", tailles)
        End Sub
#End Region

    End Class

End Namespace
