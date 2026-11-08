Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Showcase.Documentation

    ''' <summary>Ce que la documentation dit quand un exemple ne se lit pas : la vitrine le note (tests, captures).</summary>
    Friend NotInheritable Class Signalement

        Private Sub New()
        End Sub

        ''' <summary>Où vont les erreurs : le journal des tests ou des captures ; sinon la sortie de débogage.</summary>
        Public Shared Property Sortie As Action(Of String) = Sub(texte) System.Diagnostics.Debug.WriteLine(texte)

        Public Shared Sub Signaler(texte As String)
            Sortie?.Invoke(texte)
        End Sub

    End Class

    ''' <summary>Les liens entre pages : un lien d'une page demande à la fenêtre d'en ouvrir une autre.</summary>
    Friend NotInheritable Class Liens

        Private Sub New()
        End Sub

        ''' <summary>Une page demandée, par son identifiant.</summary>
        Public Shared Event Demande(id As String)

        Public Shared Sub Aller(id As String)
            RaiseEvent Demande(id)
        End Sub

    End Class

    ''' <summary>
    ''' La rangée d'un exemple, comme la carte d'ui-kit : ses éléments côte à côte, 12 px entre eux, centrés, et qui
    ''' passent à la ligne quand la place manque.
    ''' </summary>
    Public NotInheritable Class RangeeCentree
        Inherits Panel

        Private Const Ecart As Double = 12

        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            Dim largeur = 0.0, hauteur = 0.0, ligne = 0.0, haut = 0.0
            For Each enfant As UIElement In InternalChildren
                enfant.Measure(New Size(disponible.Width, Double.PositiveInfinity))
                Dim t = enfant.DesiredSize
                If ligne > 0 AndAlso ligne + Ecart + t.Width > disponible.Width Then
                    largeur = Math.Max(largeur, ligne)
                    hauteur += haut + Ecart
                    ligne = 0
                    haut = 0
                End If
                ligne += If(ligne > 0, Ecart, 0) + t.Width
                haut = Math.Max(haut, t.Height)
            Next
            Return New Size(Math.Min(Math.Max(largeur, ligne), If(Double.IsInfinity(disponible.Width), Math.Max(largeur, ligne), disponible.Width)), hauteur + haut)
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim lignes As New List(Of List(Of UIElement))()
            Dim courante As New List(Of UIElement)()
            Dim ligne = 0.0
            For Each enfant As UIElement In InternalChildren
                Dim l = enfant.DesiredSize.Width
                If courante.Count > 0 AndAlso ligne + Ecart + l > finale.Width Then
                    lignes.Add(courante)
                    courante = New List(Of UIElement)()
                    ligne = 0
                End If
                ligne += If(courante.Count > 0, Ecart, 0) + l
                courante.Add(enfant)
            Next
            If courante.Count > 0 Then lignes.Add(courante)
            Dim y = 0.0
            For Each rang In lignes
                Dim total = rang.Sum(Function(e) e.DesiredSize.Width) + Ecart * (rang.Count - 1)
                Dim haut = rang.Max(Function(e) e.DesiredSize.Height)
                Dim x = Math.Max(0, (finale.Width - total) / 2)
                For Each e In rang
                    e.Arrange(New Rect(x, y + (haut - e.DesiredSize.Height) / 2, e.DesiredSize.Width, e.DesiredSize.Height))
                    x += e.DesiredSize.Width + Ecart
                Next
                y += haut + Ecart
            Next
            Return finale
        End Function

    End Class

    ''' <summary>Les briques de la mise en page de la documentation : titres, paragraphes, tableaux, aux couleurs du thème.</summary>
    Friend NotInheritable Class Mise

        Private Sub New()
        End Sub

        ''' <summary>Pose sur un élément la couleur d'un jeton du thème (<c>DynamicResource</c>).</summary>
        Friend Shared Sub Jeton(element As FrameworkElement, propriete As DependencyProperty, chemin As String)
            element.SetResourceReference(propriete, ThemeResources.Prefix & chemin)
        End Sub

        ''' <summary>Un titre, de la taille donnée.</summary>
        Friend Shared Function Titre(texte As String, taille As Double, Optional marge As Thickness = Nothing) As TextBlock
            Dim t As New TextBlock With {.Text = texte, .FontSize = taille, .FontWeight = FontWeights.SemiBold,
                                         .TextWrapping = TextWrapping.Wrap, .Margin = marge}
            Return t
        End Function

        ''' <summary>Un texte qui passe à la ligne, gris comme un texte d'aide si <paramref name="discret"/>.</summary>
        Friend Shared Function Texte(contenu As String, Optional discret As Boolean = False, Optional marge As Thickness = Nothing) As TextBlock
            Dim t As New TextBlock With {.Text = contenu, .TextWrapping = TextWrapping.Wrap, .Margin = marge, .LineHeight = 21}
            If discret Then Jeton(t, TextBlock.ForegroundProperty, "text.muted.color")
            Return t
        End Function

        ''' <summary>Un mot de code dans une phrase : à chasse fixe, sur un fond pâle.</summary>
        Friend Shared Function MotDeCode(texte As String) As Run
            Dim r As New Run(texte) With {.FontFamily = New FontFamily("Consolas, Courier New")}
            r.SetResourceReference(TextElement.BackgroundProperty, ThemeResources.Prefix & "content.hover.background")
            r.SetResourceReference(TextElement.ForegroundProperty, ThemeResources.Prefix & "primary.color")
            Return r
        End Function

        ''' <summary>
        ''' Un tableau comme ceux d'ui-kit : un en-tête en gras sur un filet, une ligne par entrée, la première colonne en
        ''' code si <paramref name="premiereEnCode"/>.
        ''' </summary>
        Friend Shared Function Tableau(entetes As String(), lignes As IEnumerable(Of String()), largeurs As GridLength(),
                                Optional premiereEnCode As Boolean = True, Optional toutEnCode As Boolean = False,
                                Optional pastilles As Boolean = False) As FrameworkElement
            Dim grille As New Grid With {.Margin = New Thickness(0, 4, 0, 16)}
            For Each l In largeurs
                grille.ColumnDefinitions.Add(New ColumnDefinition With {.Width = l})
            Next
            Dim rang = 0
            Dim ajouter = Sub(cellules As String(), entete As Boolean)
                              grille.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
                              For c = 0 To cellules.Length - 1
                                  Dim bord As New Border With {.Padding = New Thickness(0, 8, 16, 8), .BorderThickness = New Thickness(0, 0, 0, 1)}
                                  Jeton(bord, Border.BorderBrushProperty, "content.border.color")
                                  Dim t As New TextBlock With {.Text = cellules(c), .TextWrapping = TextWrapping.Wrap}
                                  If entete Then
                                      t.FontWeight = FontWeights.SemiBold
                                  ElseIf c = 0 AndAlso premiereEnCode Then
                                      t.FontFamily = New FontFamily("Consolas, Courier New")
                                      Jeton(t, TextBlock.ForegroundProperty, "primary.color")
                                  ElseIf c > 0 AndAlso (toutEnCode OrElse (c < cellules.Length - 1 AndAlso premiereEnCode)) Then
                                      t.FontFamily = New FontFamily("Consolas, Courier New")
                                  End If
                                  Dim couleur As Color
                                  If pastilles AndAlso Not entete AndAlso c > 0 AndAlso ThemeResources.TryParseColor(cellules(c), couleur) Then
                                      ' Une pastille de la couleur devant sa valeur.
                                      Dim rangee As New StackPanel With {.Orientation = Orientation.Horizontal}
                                      Dim pastille As New Border With {.Width = 14, .Height = 14, .CornerRadius = New CornerRadius(3), .Margin = New Thickness(0, 0, 6, 0),
                                                                       .Background = New SolidColorBrush(couleur), .BorderThickness = New Thickness(1),
                                                                       .VerticalAlignment = VerticalAlignment.Top}
                                      Jeton(pastille, Border.BorderBrushProperty, "content.border.color")
                                      rangee.Children.Add(pastille)
                                      rangee.Children.Add(t)
                                      bord.Child = rangee
                                  Else
                                      bord.Child = t
                                  End If
                                  Grid.SetRow(bord, rang)
                                  Grid.SetColumn(bord, c)
                                  grille.Children.Add(bord)
                              Next
                              rang += 1
                          End Sub
            ajouter(entetes, True)
            For Each l In lignes
                ajouter(l, False)
            Next
            Return grille
        End Function

        ''' <summary>Une largeur de colonne proportionnelle.</summary>
        Friend Shared Function Part(n As Double) As GridLength
            Return New GridLength(n, GridUnitType.Star)
        End Function

    End Class

End Namespace
