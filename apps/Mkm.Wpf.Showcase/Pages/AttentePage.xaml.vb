Imports System.Windows
Imports System.Windows.Threading

Namespace Mkm.Wpf.Showcase

    ''' <summary>La page de l'attente : la barre avance d'un quart, le voile bloque trois secondes.</summary>
    Partial Public Class AttentePage

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Avancer_Click(sender As Object, e As RoutedEventArgs)
            Barre.Value = If(Barre.Value >= 100, 0, Math.Min(100, Barre.Value + 25))
            Pourcentage.Text = $"{Barre.Value:0} %"
        End Sub

        Private Sub Bloquer_Click(sender As Object, e As RoutedEventArgs)
            Voile.Blocked = True
            Dim minuterie As New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(3)}
            AddHandler minuterie.Tick, Sub()
                                           minuterie.Stop()
                                           Voile.Blocked = False
                                       End Sub
            minuterie.Start()
        End Sub

    End Class

End Namespace
