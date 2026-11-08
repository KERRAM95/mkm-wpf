Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports Mkm.Wpf.Localization
Imports Mkm.Wpf.Showcase.Documentation
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' La fenêtre de la documentation : le menu des pages à gauche, rangées par catégorie comme dans ui-kit, avec sa
    ''' recherche ; la page choisie à droite ; le thème et la langue dans la barre du haut.
    ''' </summary>
    Partial Public Class MainWindow

        Private ReadOnly _entrees As New List(Of EntreeDeMenu)()
        Private ReadOnly _vue As ListCollectionView

        Public Sub New()
            InitializeComponent()
            For Each pageDeDoc In Catalogue.Pages
                _entrees.Add(New EntreeDeMenu With {.Titre = pageDeDoc.Titre, .Categorie = pageDeDoc.Categorie, .Id = pageDeDoc.Id,
                                                    .Automatisation = "nav-doc-" & pageDeDoc.Id})
            Next
            _vue = New ListCollectionView(_entrees)
            _vue.GroupDescriptions.Add(New PropertyGroupDescription(NameOf(EntreeDeMenu.Categorie)))
            Composants.ItemsSource = _vue
            AddHandler Liens.Demande, AddressOf Ouvrir

            ChoixDuTheme.SelectedItem = Themes.Find(MkmResources.Current.Theme)
            Composants.SelectedIndex = 0
        End Sub

        ''' <summary>Les entrées du menu, dans l'ordre.</summary>
        Friend ReadOnly Property Entrees As IReadOnlyList(Of EntreeDeMenu)
            Get
                Return _entrees
            End Get
        End Property

        ''' <summary>Ouvre la page d'identifiant <paramref name="id"/> (un lien d'une autre page).</summary>
        Friend Sub Ouvrir(id As String)
            Dim entree = _entrees.FirstOrDefault(Function(e) String.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase))
            If entree Is Nothing Then Return
            Recherche.Text = ""
            Composants.SelectedItem = entree
            Composants.ScrollIntoView(entree)
        End Sub

        ' La page de l'entrée choisie.
        Private Sub Composants_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim entree = TryCast(Composants.SelectedItem, EntreeDeMenu)
            If entree IsNot Nothing Then Page.Content = New PageDeDocumentation(Catalogue.Trouver(entree.Id))
        End Sub

        ' La recherche garde les entrées dont le titre contient le texte, accents et majuscules confondus.
        Private Sub Recherche_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim cherche = Recherche.Text.Trim()
            Dim comparaison = CultureInfo.InvariantCulture.CompareInfo
            _vue.Filter = If(cherche.Length = 0, Nothing,
                             New Predicate(Of Object)(Function(o) comparaison.IndexOf(DirectCast(o, EntreeDeMenu).Titre, cherche,
                                                                                      CompareOptions.IgnoreCase Or CompareOptions.IgnoreNonSpace) >= 0))
        End Sub

        ' Une application fixe son thème ; la vitrine en change pour montrer chaque composant dans chacun.
        Private Sub ChoixDuTheme_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim theme = TryCast(ChoixDuTheme.SelectedItem, ThemeDefinition)
            If theme IsNot Nothing Then MkmResources.Current.Theme = theme.Name
        End Sub

        Protected Overrides Sub OnClosed(e As EventArgs)
            RemoveHandler Liens.Demande, AddressOf Ouvrir
            MyBase.OnClosed(e)
        End Sub

    End Class

End Namespace
