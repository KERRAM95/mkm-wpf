Imports System.Windows
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>La page de la fiche de propriétés : Enregistrer la remet en lecture et le dit par une notification.</summary>
    Partial Public Class FichePage

        Public Sub New()
            InitializeComponent()
            LaDate.SelectedDate = Date.Today
            LigneDate.Value = Date.Today.ToShortDateString()
            AddHandler LaDate.SelectedDateChanged, Sub() LigneDate.Value = LaDate.SelectedDate?.ToShortDateString()
        End Sub

        Private Sub Disposition_Changed(sender As Object, e As RoutedEventArgs)
            Fiche.Layout = If(EnLigne.IsChecked.GetValueOrDefault(), PropertyPanelLayout.Inline, PropertyPanelLayout.Stacked)
        End Sub

        Private Sub Fiche_Save(sender As Object, e As RoutedEventArgs)
            Fiche.Editing = False
            TryCast(Window.GetWindow(Me), MainWindow)?.Notifications.Add(
                MessageSeverity.Success, Localizer.Current.T("showcase.properties.saved"), Nothing, TimeSpan.FromSeconds(3))
        End Sub

    End Class

End Namespace
