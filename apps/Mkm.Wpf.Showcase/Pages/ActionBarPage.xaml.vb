Imports System.Windows
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' La page de l'ActionBar : chaque commande lancée s'inscrit en tête du journal.
    ''' </summary>
    Partial Public Class ActionBarPage

        Public Sub New()
            InitializeComponent()
        End Sub

        ' Toutes les commandes de la barre remontent ici (ButtonBase.Click sur la barre).
        Private Sub Commande_Click(sender As Object, e As RoutedEventArgs)
            Dim libelle = ActionBarItem.GetLabel(TryCast(e.OriginalSource, DependencyObject))
            If String.IsNullOrEmpty(libelle) Then Return
            Journal.Items.Insert(0, $"{Date.Now:HH:mm:ss}  {libelle}")
        End Sub

    End Class

End Namespace
