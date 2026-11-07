Imports System.Windows.Controls
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' Fenêtre principale de la vitrine.
    ''' </summary>
    Partial Public Class MainWindow

        Public Sub New()
            InitializeComponent()
            ChoixDuTheme.SelectedItem = Themes.Find(MkmResources.Current.Theme)
        End Sub

        ' Une application fixe son thème ; la vitrine en change pour montrer chaque composant dans chacun.
        Private Sub ChoixDuTheme_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim theme = TryCast(ChoixDuTheme.SelectedItem, ThemeDefinition)
            If theme IsNot Nothing Then MkmResources.Current.Theme = theme.Name
        End Sub

    End Class

End Namespace
