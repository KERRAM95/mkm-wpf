Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports BarreDeMkm = Mkm.Wpf.Controls.Toolbar

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>La Toolbar de l'exemple : chaque commande lancée, de la barre ou du menu « … », s'écrit dans le journal.</summary>
    Friend NotInheritable Class ToolbarJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim barre = DirectCast(exemple.FindName("Barre"), BarreDeMkm)
            barre.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Commande_Click))
        End Sub

#Region "Exemple"
        ' Une commande du menu « … » se lance comme si on cliquait son bouton : son Click remonte aussi à la barre.
        Private Shared Sub Commande_Click(sender As Object, e As RoutedEventArgs)
            ' Le bouton « … » est une partie du gabarit (TemplatedParent) : ce n'est pas une commande.
            Dim commande = TryCast(e.OriginalSource, ContentControl)
            If commande Is Nothing OrElse commande.TemplatedParent IsNot Nothing Then Return
            Dim libelle = If(TryCast(commande.Content, String), TryCast(commande.ToolTip, String))
            If String.IsNullOrEmpty(libelle) Then Return
            Dim journal = DirectCast(DirectCast(sender, FrameworkElement).FindName("Journal"), TextBlock)
            journal.Text = $"Commande lancée : {libelle}"
        End Sub
#End Region

    End Class

End Namespace
