Imports System
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
    ''' recherche ; la page choisie à droite ; le thème, la langue et le zoom dans la barre du haut.
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
            For Each palier In MkmResources.ZoomLevels
                ChoixDuZoom.Items.Add(New ComboBoxItem With {.Content = palier.ToString("P0", CultureInfo.GetCultureInfo("fr-FR")), .Tag = palier})
            Next
            MontrerLeZoom()
            AddHandler MkmResources.Current.ZoomChanged, AddressOf Zoom_Change
            Composants.SelectedIndex = 0
        End Sub

        ' Le palier du zoom actuel dans la liste (un zoom hors des paliers, du réglage de Windows : aucun).
        Private Sub MontrerLeZoom()
            Dim actuel = MkmResources.Current.Zoom
            ChoixDuZoom.SelectedItem = ChoixDuZoom.Items.OfType(Of ComboBoxItem)().FirstOrDefault(Function(i) Math.Abs(CDbl(i.Tag) - actuel) < 0.001)
        End Sub

        Private Sub Zoom_Change(sender As Object, e As EventArgs)
            MontrerLeZoom()
        End Sub

        Private Sub ChoixDuZoom_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim choix = TryCast(ChoixDuZoom.SelectedItem, ComboBoxItem)
            If choix IsNot Nothing Then MkmResources.Current.Zoom = CDbl(choix.Tag)
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

        ' La page de l'entrée choisie ; dans une fenêtre étroite, le menu se referme sur elle.
        Private Sub Composants_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim entree = TryCast(Composants.SelectedItem, EntreeDeMenu)
            If entree IsNot Nothing Then Page.Content = New PageDeDocumentation(Catalogue.Trouver(entree.Id))
            If _etroite Then Menu.Visibility = Visibility.Collapsed
        End Sub

        ' Sous 600 px de large (un fort zoom : 1 024 px à 200 %), le menu des pages laisse la place à la page : il
        ' s'ouvre par le bouton de la barre, par-dessus elle. La barre passe ses réglages à la ligne quand ils ne
        ' tiennent plus à côté du titre.
        Private Const LargeurEtroite As Double = 600
        Private _etroite As Boolean

        Private Sub Corps_SizeChanged(sender As Object, e As SizeChangedEventArgs)
            Dim etroite = Corps.ActualWidth < LargeurEtroite
            If etroite <> _etroite OrElse e.PreviousSize.Width = 0 Then
                _etroite = etroite
                ColonneDuMenu.Width = If(etroite, New GridLength(0), New GridLength(250))
                Grid.SetColumnSpan(Menu, If(etroite, 2, 1))
                Menu.Width = If(etroite, 250, Double.NaN)
                Menu.HorizontalAlignment = If(etroite, HorizontalAlignment.Left, HorizontalAlignment.Stretch)
                Panel.SetZIndex(Menu, If(etroite, 1, 0))
                Menu.Visibility = If(etroite, Visibility.Collapsed, Visibility.Visible)
                Grid.SetColumn(Page, If(etroite, 0, 1))
                Grid.SetColumnSpan(Page, If(etroite, 2, 1))
                BoutonPages.Visibility = If(etroite, Visibility.Visible, Visibility.Collapsed)
            End If
            Reglages.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
            Dim aLaLigne = Reglages.DesiredSize.Width + 140 > Barre.ActualWidth - Barre.Padding.Left - Barre.Padding.Right
            DockPanel.SetDock(Reglages, If(aLaLigne, System.Windows.Controls.Dock.Bottom, System.Windows.Controls.Dock.Right))
            Reglages.Margin = If(aLaLigne, New Thickness(0, 8, 0, 0), New Thickness())
        End Sub

        Private Sub BoutonPages_Click(sender As Object, e As RoutedEventArgs)
            Menu.Visibility = If(Menu.Visibility = Visibility.Visible, Visibility.Collapsed, Visibility.Visible)
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
            RemoveHandler MkmResources.Current.ZoomChanged, AddressOf Zoom_Change
            MyBase.OnClosed(e)
        End Sub

    End Class

End Namespace
