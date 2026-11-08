Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Markup
Imports System.Windows.Media
Imports System.Xml.Linq
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Theming
Imports Mkm.Wpf.Showcase.Documentation.Mise

Namespace Mkm.Wpf.Showcase.Documentation

    ''' <summary>
    ''' Une page de la documentation, comme celles d'ui-kit : le titre et le résumé du composant, puis trois onglets.
    ''' <list type="bullet">
    ''' <item>Fonctionnalités : Import, une section par usage (son texte, l'exemple vivant, son code à copier),
    ''' l'accessibilité ; à droite, le sommaire de la page.</item>
    ''' <item>API : les propriétés, évènements et méthodes, tirés du code.</item>
    ''' <item>Thème : les jetons du composant, dans chaque thème.</item>
    ''' </list>
    ''' </summary>
    Friend NotInheritable Class PageDeDocumentation
        Inherits UserControl

        Private Const EnveloppeOuvrante As String =
            "<mkmdoc:RangeeCentree xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" " &
            "xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:mkm=""urn:mkm-wpf"" " &
            "xmlns:sys=""clr-namespace:System;assembly=mscorlib"" " &
            "xmlns:mkmdoc=""clr-namespace:Mkm.Wpf.Showcase.Documentation;assembly=Mkm.Wpf.Showcase"">"
        Private Const EnveloppeFermante As String = "</mkmdoc:RangeeCentree>"

        Private ReadOnly _page As PageDeDoc
        Private ReadOnly _defileur As ScrollViewer
        Private ReadOnly _sommaire As StackPanel
        Private ReadOnly _ancres As New List(Of KeyValuePair(Of FrameworkElement, TextBlock))()
        Private _onglets As Tabs

        ''' <param name="pourCapture">Vrai pour une capture : tout à la suite, sans défilement ni onglets.</param>
        Public Sub New(page As PageDeDoc, Optional pourCapture As Boolean = False)
            _page = page
            AutomationProperties.SetAutomationId(Me, "page-" & page.Id)
            Focusable = False

            Dim contenu As New StackPanel With {.Margin = New Thickness(40, 28, 40, 40), .MaxWidth = 900, .HorizontalAlignment = HorizontalAlignment.Left}
            contenu.Children.Add(Titre(page.Titre, 30, New Thickness(0, 0, 0, 8)))
            If page.Chapeau IsNot Nothing Then
                Dim chapeau = Paragraphe(page.Chapeau)
                chapeau.FontSize = 16
                Jeton(chapeau, TextBlock.ForegroundProperty, "text.muted.color")
                contenu.Children.Add(chapeau)
            End If

            Dim fonctionnalites As New StackPanel With {.Margin = New Thickness(0, 24, 0, 0)}
            For Each partie In page.Sections
                fonctionnalites.Children.Add(Section(partie))
            Next

            If pourCapture OrElse Not page.EstUnComposant Then
                contenu.Children.Add(fonctionnalites)
                If pourCapture AndAlso page.EstUnComposant Then
                    contenu.Children.Add(Titre("API", 24, New Thickness(0, 24, 0, 8)))
                    contenu.Children.Add(OngletApi())
                    contenu.Children.Add(Titre("Thème", 24, New Thickness(0, 24, 0, 8)))
                    contenu.Children.Add(OngletTheme())
                End If
            Else
                _onglets = New Tabs With {.Margin = New Thickness(0, 20, 0, 0), .Variant = TabsVariant.Strip}
                _onglets.Items.Add(New Tab With {.Header = "Fonctionnalités", .Content = fonctionnalites})
                _onglets.Items.Add(New Tab With {.Header = "API", .Content = OngletApi()})
                _onglets.Items.Add(New Tab With {.Header = "Thème", .Content = OngletTheme()})
                AddHandler _onglets.SelectionChanged, Sub(s, e)
                                                          If e.OriginalSource Is _onglets AndAlso _sommaire IsNot Nothing Then AjusterLeSommaire()
                                                      End Sub
                contenu.Children.Add(_onglets)
            End If

            If pourCapture Then
                Content = contenu
                Return
            End If

            _defileur = New ScrollViewer With {.Content = contenu, .VerticalScrollBarVisibility = ScrollBarVisibility.Auto, .Focusable = False}
            AddHandler _defileur.PreviewMouseWheel, AddressOf Molette
            AddHandler _defileur.ScrollChanged, Sub() MarquerLaSection()

            _sommaire = New StackPanel With {.Margin = New Thickness(0, 32, 24, 0), .Width = 190}
            _sommaire.Children.Add(New TextBlock With {.Text = "Sur cette page", .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 0, 8)})
            For Each ancre In _ancres
                _sommaire.Children.Add(ancre.Value)
            Next

            Dim grille As New DockPanel()
            DockPanel.SetDock(_sommaire, Dock.Right)
            grille.Children.Add(_sommaire)
            grille.Children.Add(_defileur)
            Content = grille
            AddHandler Loaded, Sub() MarquerLaSection()
            ' Une fenêtre étroite (un écran de portable) garde la place pour les exemples : le sommaire se retire.
            AddHandler SizeChanged, Sub() AjusterLeSommaire()
        End Sub

        Private Const LargeurSansSommaire As Double = 860

        Private Sub AjusterLeSommaire()
            Dim ongletDesFonctionnalites = _onglets Is Nothing OrElse _onglets.SelectedIndex = 0
            _sommaire.Visibility = If(ActualWidth >= LargeurSansSommaire AndAlso ongletDesFonctionnalites, Visibility.Visible, Visibility.Collapsed)
        End Sub

        ''' <summary>La page décrite.</summary>
        Public ReadOnly Property Page As PageDeDoc
            Get
                Return _page
            End Get
        End Property

        ' ---- Les sections ----

        Private Function Section(xml As XElement) As FrameworkElement
            Dim pile As New StackPanel With {.Margin = New Thickness(0, 0, 0, 36)}
            Dim intitule = CStr(xml.Attribute("titre"))
            pile.Children.Add(Titre(intitule, 22, New Thickness(0, 0, 0, 10)))
            For Each element In xml.Elements()
                pile.Children.Add(Bloc(element))
            Next
            ' Son entrée dans le sommaire : un lien, que le clavier atteint aussi (Tab, puis Entrée).
            Dim lien As New Hyperlink(New Run(intitule)) With {.TextDecorations = Nothing}
            AddHandler lien.Click, Sub() AllerA(pile)
            lien.SetResourceReference(TextElement.ForegroundProperty, ThemeResources.Prefix & "text.muted.color")
            Dim entree As New TextBlock(lien) With {.Margin = New Thickness(0, 3, 0, 3), .TextWrapping = TextWrapping.Wrap}
            _ancres.Add(New KeyValuePair(Of FrameworkElement, TextBlock)(pile, entree))
            Return pile
        End Function

        Private Function Bloc(xml As XElement) As FrameworkElement
            Select Case xml.Name.LocalName
                Case "p"
                    Return Paragraphe(xml)
                Case "h3"
                    Return Titre(xml.Value.Trim(), 17, New Thickness(0, 12, 0, 6))
                Case "exemple"
                    Return Exemple(xml)
                Case "code"
                    Return New BlocDeCode(BlocDeCode.Desindenter(xml.Value), If(CStr(xml.Attribute("langage")), "xaml"))
                Case "touches"
                    Return Tableau({"Touche", "Fonction"},
                                   xml.Elements("touche").Select(Function(t) {CStr(t.Attribute("nom")), Normaliser(t.Value)}),
                                   {New GridLength(170), Part(1)})
                Case "liste"
                    Dim liste As New StackPanel With {.Margin = New Thickness(0, 0, 0, 12)}
                    For Each element In xml.Elements("el")
                        Dim ligne = Paragraphe(element)
                        ligne.Margin = New Thickness(16, 0, 0, 4)
                        ligne.Inlines.InsertBefore(ligne.Inlines.FirstInline, New Run("•  "))
                        liste.Children.Add(ligne)
                    Next
                    Return liste
                Case "tableau"
                    Dim entetes = xml.Element("entete").Elements("c").Select(Function(c) Normaliser(c.Value)).ToArray()
                    Return Tableau(entetes,
                                   xml.Elements("ligne").Select(Function(l) l.Elements("c").Select(Function(c) Normaliser(c.Value)).ToArray()),
                                   entetes.Select(Function(e, i) If(i = 0, New GridLength(170), Part(1))).ToArray(),
                                   CStr(xml.Attribute("code")) <> "non")
                Case Else
                    Return Texte($"[bloc inconnu : {xml.Name.LocalName}]")
            End Select
        End Function

        ' Un paragraphe et ses mots marqués : <c>code</c>, <b>gras</b>, <lien page="toast">Toast</lien>.
        Private Shared Function Paragraphe(xml As XElement) As TextBlock
            Dim t As New TextBlock With {.TextWrapping = TextWrapping.Wrap, .LineHeight = 22, .Margin = New Thickness(0, 0, 0, 12)}
            For Each noeud In xml.Nodes()
                Dim morceau = TryCast(noeud, XText)
                If morceau IsNot Nothing Then
                    t.Inlines.Add(New Run(Normaliser(morceau.Value, False)))
                    Continue For
                End If
                Dim e = TryCast(noeud, XElement)
                If e Is Nothing Then Continue For
                Select Case e.Name.LocalName
                    Case "c" : t.Inlines.Add(MotDeCode(e.Value))
                    Case "b" : t.Inlines.Add(New Bold(New Run(Normaliser(e.Value, False))))
                    Case "lien"
                        Dim cible = CStr(e.Attribute("page"))
                        Dim lien As New Hyperlink(New Run(Normaliser(e.Value, False)))
                        AddHandler lien.Click, Sub() Liens.Aller(cible)
                        t.Inlines.Add(lien)
                    Case Else : t.Inlines.Add(New Run(Normaliser(e.Value, False)))
                End Select
            Next
            Return t
        End Function

        ' Les blancs d'un texte XML resserrés, comme dans une page web.
        Private Shared Function Normaliser(texte As String, Optional rogner As Boolean = True) As String
            Dim resserre = System.Text.RegularExpressions.Regex.Replace(texte, "\s+", " ")
            Return If(rogner, resserre.Trim(), resserre)
        End Function

        ' ---- Les exemples ----

        ' L'exemple vivant, lu de son XAML, puis ce même XAML à copier ; et le code VB de son comportement s'il en a un.
        Private Function Exemple(xml As XElement) As FrameworkElement
            Dim pile As New StackPanel With {.Margin = New Thickness(0, 4, 0, 12)}
            Dim xaml = BlocDeCode.Desindenter(xml.Value)
            ' Son identifiant : celui de sa section, suivi de son rang s'il y en a plusieurs (« exemple-sections-2 »).
            Dim section = CStr(xml.Parent.Attribute("titre"))
            Dim rang = xml.ElementsBeforeSelf("exemple").Count()
            Dim cadre As New CadreDExemple With {.Section = section, .BorderThickness = New Thickness(1), .CornerRadius = New CornerRadius(6), .Padding = New Thickness(24)}
            AutomationProperties.SetAutomationId(cadre, "exemple-" & CadreDExemple.Identifiant(section) & If(rang = 0, "", "-" & (rang + 1).ToString()))
            Jeton(cadre, Border.BorderBrushProperty, "content.border.color")
            Jeton(cadre, Border.BackgroundProperty, "content.background")
            Dim comportement = CStr(xml.Attribute("comportement"))
            Try
                Dim racine = DirectCast(XamlReader.Parse(EnveloppeOuvrante & xaml & EnveloppeFermante), FrameworkElement)
                If CStr(xml.Attribute("etendu")) = "oui" Then
                    ' Un exemple qui prend toute la largeur (un tableau, une mise en page) : ses éléments l'un sous l'autre.
                    Dim enfants = DirectCast(racine, Panel).Children.Cast(Of UIElement)().ToList()
                    DirectCast(racine, Panel).Children.Clear()
                    Dim colonne As New StackPanel()
                    For Each enfant In enfants
                        colonne.Children.Add(enfant)
                    Next
                    NameScope.SetNameScope(colonne, NameScope.GetNameScope(racine))
                    racine = colonne
                End If
                cadre.Child = racine
                If comportement IsNot Nothing Then
                    Dim preparer = Catalogue.Comportement(comportement)
                    If preparer Is Nothing Then Throw New InvalidOperationException($"comportement introuvable : {comportement}")
                    preparer.Invoke(Nothing, {racine})
                End If
            Catch ex As Exception
                Dim erreur = If(TypeOf ex Is Reflection.TargetInvocationException, ex.InnerException, ex)
                cadre.Child = New TextBlock With {.Text = $"Exemple illisible : {erreur.Message}", .Foreground = Brushes.Firebrick, .TextWrapping = TextWrapping.Wrap}
                Signalement.Signaler($"Documentation {_page.Id} : exemple de « {CStr(xml.Parent.Attribute("titre"))} » illisible : {erreur}")
            End Try
            pile.Children.Add(cadre)
            pile.Children.Add(New BlocDeCode(xaml, "xaml"))
            If comportement IsNot Nothing Then
                Dim source = Catalogue.SourceDuComportement(comportement)
                If source IsNot Nothing Then pile.Children.Add(New BlocDeCode(BlocDeCode.Desindenter(source), "vb"))
            End If
            Return pile
        End Function

        ' ---- L'onglet API ----

        Private Function OngletApi() As FrameworkElement
            Dim pile As New StackPanel With {.Margin = New Thickness(0, 16, 0, 0)}
            For Each nom In _page.Types
                Dim type = Api.TypeNomme(nom)
                If type Is Nothing Then
                    Signalement.Signaler($"Documentation {_page.Id} : type inconnu « {nom} »")
                    Continue For
                End If
                Dim membres = Api.Decrire(type)
                pile.Children.Add(Titre(membres.Nom, 22, New Thickness(0, 8, 0, 6)))
                If membres.Description <> "" Then pile.Children.Add(Texte(membres.Description, marge:=New Thickness(0, 0, 0, 8)))
                If membres.Parent IsNot Nothing Then pile.Children.Add(Texte($"Hérite de {membres.Parent}.", True, New Thickness(0, 0, 0, 8)))
                Ajouter(pile, "Propriétés", {"Nom", "Type", "Par défaut", "Description"}, membres.Proprietes,
                        Function(m) {m.Nom, m.Type, m.Defaut, m.Description}, {Part(1.2), Part(1), Part(0.8), Part(2.5)})
                Ajouter(pile, "Propriétés jointes", {"Nom", "Type", "Par défaut", "Description"}, membres.ProprietesJointes,
                        Function(m) {m.Nom, m.Type, m.Defaut, m.Description}, {Part(1.2), Part(1), Part(0.8), Part(2.5)})
                Ajouter(pile, "Évènements", {"Nom", "Type", "Description"}, membres.Evenements,
                        Function(m) {m.Nom, m.Type, m.Description}, {Part(1), Part(1.2), Part(2.5)})
                Ajouter(pile, "Méthodes", {"Nom", "Signature", "Description"}, membres.Methodes,
                        Function(m) {m.Nom, m.Type, m.Description}, {Part(1), Part(1.6), Part(2.2)})
                Ajouter(pile, "Valeurs", {"Nom", "Description"}, membres.Valeurs,
                        Function(m) {m.Nom, m.Description}, {Part(1), Part(3)})
            Next
            Return pile
        End Function

        Private Shared Sub Ajouter(pile As StackPanel, intitule As String, entetes As String(), membres As List(Of MembreDApi),
                                   cellules As Func(Of MembreDApi, String()), largeurs As GridLength())
            If membres.Count = 0 Then Return
            pile.Children.Add(Titre(intitule, 16, New Thickness(0, 8, 0, 2)))
            pile.Children.Add(Tableau(entetes, membres.Select(cellules), largeurs))
        End Sub

        ' ---- L'onglet Thème ----

        Private Function OngletTheme() As FrameworkElement
            Dim pile As New StackPanel With {.Margin = New Thickness(0, 16, 0, 0)}
            pile.Children.Add(Texte("Les jetons de thème du composant, et leur valeur dans chaque thème. Un gabarit les lit par " &
                                    "DynamicResource, sous la clé mkm.<jeton> : changer de thème les change tous.", True, New Thickness(0, 0, 0, 12)))
            Dim lesThemes = Themes.All
            Dim chemins = lesThemes(0).Paths.Where(Function(p) _page.Jetons.Any(Function(j) p.StartsWith(j & ".", StringComparison.Ordinal))).OrderBy(Function(p) p, StringComparer.Ordinal).ToList()
            If chemins.Count = 0 Then
                pile.Children.Add(Texte("Ce composant emploie les jetons communs du thème (couleurs, police, bordures)."))
                Return pile
            End If
            Dim entetes = {"Jeton"}.Concat(lesThemes.Select(Function(t) t.Name)).ToArray()
            Dim largeurs = {Part(2)}.Concat(lesThemes.Select(Function(t) Part(1))).ToArray()
            pile.Children.Add(Tableau(entetes, chemins.Select(Function(c) {c}.Concat(lesThemes.Select(Function(t) If(t.Resolve(c), ""))).ToArray()), largeurs,
                                      toutEnCode:=True, pastilles:=True))
            Return pile
        End Function

        ' ---- Le sommaire ----

        Private Sub AllerA(section As FrameworkElement)
            If _onglets IsNot Nothing Then _onglets.SelectedIndex = 0
            Dispatcher.BeginInvoke(New Action(Sub()
                                                  Dim haut = section.TransformToAncestor(DirectCast(_defileur.Content, Visual)).Transform(New Point(0, 0)).Y
                                                  _defileur.ScrollToVerticalOffset(haut - 12)
                                              End Sub), Threading.DispatcherPriority.Loaded)
        End Sub

        ' La section qu'on lit, en couleur dans le sommaire : la dernière dont le titre est passé en haut.
        Private Sub MarquerLaSection()
            If _defileur Is Nothing OrElse _ancres.Count = 0 OrElse Not IsLoaded Then Return
            Dim courante = _ancres(0)
            For Each ancre In _ancres
                If Not ancre.Key.IsVisible Then Continue For
                Dim haut = ancre.Key.TransformToAncestor(_defileur).Transform(New Point(0, 0)).Y
                If haut <= 40 Then courante = ancre
            Next
            For Each ancre In _ancres
                DirectCast(ancre.Value.Inlines.FirstInline, Hyperlink).SetResourceReference(
                    TextElement.ForegroundProperty, ThemeResources.Prefix & If(ancre.Value Is courante.Value, "primary.color", "text.muted.color"))
                ancre.Value.FontWeight = If(ancre.Value Is courante.Value, FontWeights.SemiBold, FontWeights.Normal)
            Next
        End Sub

        ' La molette fait défiler la page, sauf au-dessus d'un élément qui défile lui-même dans ce sens (un tableau, une
        ' liste) : comme dans un navigateur. Le ScrollViewer de WPF garde la molette même quand il ne peut plus défiler.
        Private Sub Molette(sender As Object, e As MouseWheelEventArgs)
            Dim d = TryCast(e.OriginalSource, DependencyObject)
            While d IsNot Nothing AndAlso d IsNot _defileur
                Dim interieur = TryCast(d, ScrollViewer)
                If interieur IsNot Nothing AndAlso interieur.ScrollableHeight > 0 AndAlso
                   If(e.Delta > 0, interieur.VerticalOffset > 0, interieur.VerticalOffset < interieur.ScrollableHeight) Then Return
                d = If(TypeOf d Is Visual, VisualTreeHelper.GetParent(d), LogicalTreeHelper.GetParent(d))
            End While
            If d Is Nothing Then Return
            _defileur.ScrollToVerticalOffset(_defileur.VerticalOffset - e.Delta * 0.4)
            e.Handled = True
        End Sub

    End Class

End Namespace
