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
            Composants.SelectedIndex = 0
        End Sub

        ' La page du composant choisi dans la liste de gauche.
        Private Sub Composants_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim entree = TryCast(Composants.SelectedItem, ListBoxItem)
            Select Case CStr(entree?.Tag)
                Case "ActionBar" : Page.Content = New ActionBarPage()
                Case "Champs" : Page.Content = New ChampsPage()
                Case "Boutons" : Page.Content = New BoutonsPage()
                Case "Conteneurs" : Page.Content = New ConteneursPage()
                Case "Retours" : Page.Content = New RetoursPage()
                Case "Attente" : Page.Content = New AttentePage()
                Case "Saisie" : Page.Content = New SaisiePage()
                Case "Fiche" : Page.Content = New FichePage()
                Case "Arbre" : Page.Content = New ArbrePage()
                Case "Identite" : Page.Content = New IdentitePage()
                Case "Tableau" : Page.Content = New TableauPage()
            End Select
        End Sub

        ' Une application fixe son thème ; la vitrine en change pour montrer chaque composant dans chacun.
        Private Sub ChoixDuTheme_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim theme = TryCast(ChoixDuTheme.SelectedItem, ThemeDefinition)
            If theme IsNot Nothing Then MkmResources.Current.Theme = theme.Name
        End Sub

    End Class

End Namespace
