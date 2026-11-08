Imports System
Imports System.Windows
Imports System.Windows.Threading
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le chargement d'un tableau : son bouton le voile deux secondes.</summary>
    Friend NotInheritable Class TableauEnChargement

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim tableau = DirectCast(exemple.FindName("Chargement"), DataTable)
            TableauDeVariables.Remplir(tableau)
            AddHandler DirectCast(exemple.FindName("Charger"), Mkm.Wpf.Controls.Button).Click, Sub() Charger(tableau)
        End Sub

#Region "Exemple"
        ' Loading voile le tableau et montre un chargeur, le temps que les données arrivent.
        Private Shared Sub Charger(tableau As DataTable)
            tableau.Loading = True
            Dim minuterie As New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(2)}
            AddHandler minuterie.Tick, Sub()
                                           minuterie.Stop()
                                           tableau.Loading = False
                                       End Sub
            minuterie.Start()
        End Sub
#End Region

    End Class

End Namespace
