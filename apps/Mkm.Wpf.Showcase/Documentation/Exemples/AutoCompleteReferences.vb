Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Les champs à suggestions : des références de plans, et des maturités.</summary>
    Friend NotInheritable Class AutoCompleteReferences

        Private Shared ReadOnly References As String() = Enumerable.Range(1, 200).Select(Function(n) $"HS-{n:00000}").ToArray()
        Private Shared ReadOnly Maturites As String() = {"Private", "In Work", "Frozen", "Released", "Obsolete"}

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim reference = TryCast(exemple.FindName("Reference"), AutoComplete)
            If reference IsNot Nothing Then
                AddHandler reference.Complete, AddressOf Reference_Complete
                AddHandler reference.ItemSelect, AddressOf Reference_ItemSelect
            End If
            Dim maturite = TryCast(exemple.FindName("Maturite"), AutoComplete)
            If maturite IsNot Nothing Then AddHandler maturite.Complete, AddressOf Maturite_Complete
        End Sub

#Region "Exemple"
        ' Les références qui commencent par le texte saisi, sans égard à la casse.
        Private Shared Sub Reference_Complete(sender As Object, e As AutoCompleteEventArgs)
            DirectCast(sender, AutoComplete).Suggestions =
                References.Where(Function(r) r.StartsWith(e.Query, StringComparison.OrdinalIgnoreCase)).Take(50).ToList()
        End Sub

        Private Shared Sub Reference_ItemSelect(sender As Object, e As RoutedEventArgs)
            Dim champ = DirectCast(sender, AutoComplete)
            DirectCast(champ.FindName("Journal"), TextBlock).Text = $"Choisie : {champ.SelectedItem}"
        End Sub

        ' Le bouton demande tout (e.Query vide) ; la frappe, les maturités qui contiennent le texte.
        Private Shared Sub Maturite_Complete(sender As Object, e As AutoCompleteEventArgs)
            DirectCast(sender, AutoComplete).Suggestions =
                Maturites.Where(Function(m) m.IndexOf(e.Query, StringComparison.OrdinalIgnoreCase) >= 0).ToList()
        End Sub
#End Region

    End Class

End Namespace
