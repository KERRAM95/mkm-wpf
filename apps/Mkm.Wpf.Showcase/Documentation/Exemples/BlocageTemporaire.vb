Imports System
Imports System.Windows
Imports System.Windows.Threading
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Le bouton « Bloquer » pose le voile « Voile » trois secondes.</summary>
    Friend NotInheritable Class BlocageTemporaire

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim voile = DirectCast(exemple.FindName("Voile"), BlockUI)
            AddHandler DirectCast(exemple.FindName("Bloquer"), Mkm.Wpf.Controls.Button).Click, Sub() Bloquer(voile)
        End Sub

#Region "Exemple"
        ' Blocked pose le voile ; l'application le lève quand son traitement est fini (ici, au bout de 3 secondes).
        Private Shared Sub Bloquer(voile As BlockUI)
            voile.Blocked = True
            Dim minuterie As New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(3)}
            AddHandler minuterie.Tick, Sub()
                                           minuterie.Stop()
                                           voile.Blocked = False
                                       End Sub
            minuterie.Start()
        End Sub
#End Region

    End Class

End Namespace
