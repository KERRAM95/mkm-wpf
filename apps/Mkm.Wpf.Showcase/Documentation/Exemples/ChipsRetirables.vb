Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Les filtres retirables : chaque puce retirée s'inscrit au journal ; « Tout remettre » les rend.</summary>
    Friend NotInheritable Class ChipsRetirables

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim filtres = DirectCast(exemple.FindName("Filtres"), Panel)
            AddHandler DirectCast(exemple.FindName("ToutRemettre"), Mkm.Wpf.Controls.Button).Click,
                Sub()
                    For Each puce In filtres.Children.OfType(Of Chip)()
                        puce.Visibility = Visibility.Visible
                    Next
                End Sub
            For Each puce In filtres.Children.OfType(Of Chip)()
                AddHandler puce.Remove, AddressOf Filtre_Remove
            Next
        End Sub

#Region "Exemple"
        ' La puce s'est cachée : l'application retire le filtre de sa recherche.
        Private Shared Sub Filtre_Remove(sender As Object, e As RoutedEventArgs)
            Dim puce = DirectCast(sender, Chip)
            Dim journal = DirectCast(puce.FindName("Journal"), TextBlock)
            journal.Text = $"Filtre retiré : {puce.Content}"
        End Sub
#End Region

    End Class

End Namespace
