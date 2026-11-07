Imports System.Linq

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' La page des champs ; la longue liste (5 000 valeurs) montre la virtualisation de Select.
    ''' </summary>
    Partial Public Class ChampsPage

        Public Sub New()
            InitializeComponent()
            Longue.ItemsSource = Enumerable.Range(1, 5000).Select(Function(i) $"HS-{i:00000}").ToList()
        End Sub

    End Class

End Namespace
