Imports System.Windows
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>La page du texte, de la date et de la confirmation.</summary>
    Partial Public Class SaisiePage

        Public Sub New()
            InitializeComponent()
            Choisie.SelectedDate = Date.Today
            Grisee.SelectedDate = Date.Today
        End Sub

        Private Sub Demander_Click(sender As Object, e As RoutedEventArgs)
            Dim oui = ConfirmDialog.Confirm(Window.GetWindow(Me), Localizer.Current.T("showcase.inputs.question"),
                                            Localizer.Current.T("showcase.inputs.update"), Icons.Warning)
            Reponse.Text = Localizer.Current.T(If(oui, "showcase.inputs.yes", "showcase.inputs.no"))
        End Sub

    End Class

End Namespace
