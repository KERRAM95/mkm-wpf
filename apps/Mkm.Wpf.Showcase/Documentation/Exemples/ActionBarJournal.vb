Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>L'ActionBar de l'exemple : chaque commande lancée s'écrit dans le journal sous la barre.</summary>
    Friend NotInheritable Class ActionBarJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim barre = DirectCast(exemple.FindName("Barre"), ActionBar)
            barre.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Commande_Click))
        End Sub

#Region "Exemple"
        ' Toutes les commandes de la barre remontent ici : ButtonBase.Click sur la barre, et ActionBarItem.GetLabel
        ' donne le libellé de la commande lancée.
        Private Shared Sub Commande_Click(sender As Object, e As RoutedEventArgs)
            Dim libelle = ActionBarItem.GetLabel(TryCast(e.OriginalSource, DependencyObject))
            If String.IsNullOrEmpty(libelle) Then Return
            Dim journal = DirectCast(DirectCast(sender, FrameworkElement).FindName("Journal"), TextBlock)
            journal.Text = $"Commande lancée : {libelle}"
        End Sub
#End Region

    End Class

End Namespace
