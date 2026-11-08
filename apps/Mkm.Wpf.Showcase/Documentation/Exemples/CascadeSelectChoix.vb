Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>La machine choisie dans le CascadeSelect de l'exemple s'écrit dans le texte « Choix ».</summary>
    Friend NotInheritable Class CascadeSelectChoix

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim champ = DirectCast(exemple.FindName("Machine"), CascadeSelect)
            Dim choix = DirectCast(exemple.FindName("Choix"), TextBlock)
            AddHandler champ.SelectedItemChanged, Sub(s, e) Montrer(champ, choix)
        End Sub

#Region "Exemple"
        ' SelectedItem est l'option choisie (un CascadeSelectItem, ou l'objet de données d'une liste liée).
        Private Shared Sub Montrer(champ As CascadeSelect, choix As TextBlock)
            Dim machine = TryCast(champ.SelectedItem, CascadeSelectItem)
            choix.Text = If(machine Is Nothing, "Aucune machine choisie", $"Machine choisie : {machine.Header}")
        End Sub
#End Region

    End Class

End Namespace
