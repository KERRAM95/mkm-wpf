Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le panneau d'accueil de l'exemple : chaque entrée choisie s'écrit dans le journal.</summary>
    Friend NotInheritable Class WelcomePanelJournal

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim accueil = DirectCast(exemple.FindName("Accueil"), WelcomePanel)
            AddHandler accueil.ItemClick, AddressOf Entree_Click
        End Sub

#Region "Exemple"
        ' ItemClick : au clic ou au clavier ; sa source est l'entrée. Selection donne la clé de l'entrée choisie.
        Private Shared Sub Entree_Click(sender As Object, e As RoutedEventArgs)
            Dim accueil = DirectCast(sender, WelcomePanel)
            Dim entree = TryCast(e.OriginalSource, WelcomePanelItem)
            If entree Is Nothing Then Return
            Dim journal = DirectCast(accueil.FindName("Journal"), TextBlock)
            journal.Text = $"Entrée : {entree.Content} — choisie : {accueil.Selection}"
        End Sub
#End Region

    End Class

End Namespace
