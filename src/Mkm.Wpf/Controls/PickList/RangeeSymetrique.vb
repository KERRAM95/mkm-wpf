Imports System.Windows

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La rangée de PickList : trois éléments, la source, les boutons du milieu, la cible ; les deux côtés ont toujours
    ''' la même largeur (le <c>flex: 1 1 50%</c> de mkm), que la rangée s'étire ou prenne sa taille naturelle (une grille
    ''' à colonnes en étoile, mesurée sans largeur, laisserait chaque côté à sa taille).
    ''' </summary>
    Friend NotInheritable Class RangeeSymetrique
        Inherits System.Windows.Controls.Panel

        Private Function Enfant(i As Integer) As UIElement
            Return If(InternalChildren.Count > i, InternalChildren(i), Nothing)
        End Function

        Private Function LargeurDuMilieu() As Double
            Dim milieu = Enfant(1)
            Return If(milieu Is Nothing, 0, milieu.DesiredSize.Width)
        End Function

        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            Dim milieu = Enfant(1)
            milieu?.Measure(disponible)
            Dim cote = If(Double.IsInfinity(disponible.Width), Double.PositiveInfinity,
                          Math.Max(0, (disponible.Width - LargeurDuMilieu()) / 2))
            Dim largeur = 0.0
            Dim hauteur = If(milieu Is Nothing, 0, milieu.DesiredSize.Height)
            For Each i In {0, 2}
                Dim element = Enfant(i)
                If element Is Nothing Then Continue For
                element.Measure(New Size(cote, disponible.Height))
                largeur = Math.Max(largeur, element.DesiredSize.Width)
                hauteur = Math.Max(hauteur, element.DesiredSize.Height)
            Next
            Return New Size(2 * largeur + LargeurDuMilieu(), hauteur)
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim milieu = LargeurDuMilieu()
            Dim cote = Math.Max(0, (finale.Width - milieu) / 2)
            Enfant(0)?.Arrange(New Rect(0, 0, cote, finale.Height))
            Enfant(1)?.Arrange(New Rect(cote, 0, milieu, finale.Height))
            Enfant(2)?.Arrange(New Rect(cote + milieu, 0, cote, finale.Height))
            Return finale
        End Function

    End Class

End Namespace
