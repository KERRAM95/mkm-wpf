Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une rangée qui passe à la ligne, ses éléments à <see cref="Gap"/> l'un de l'autre et d'une ligne à l'autre (le
    ''' <c>flex-wrap</c> avec <c>gap</c> de mkm) ; le dernier élément prend la place qui reste sur sa ligne (la saisie
    ''' d'InputChips).
    ''' </summary>
    Friend NotInheritable Class GapWrapPanel
        Inherits System.Windows.Controls.Panel

        Public Shared ReadOnly GapProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Gap), GetType(Double), GetType(GapWrapPanel),
            New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure))

        Public Property Gap As Double
            Get
                Return CDbl(GetValue(GapProperty))
            End Get
            Set(value As Double)
                SetValue(GapProperty, value)
            End Set
        End Property

        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            Dim enfant As New Size(disponible.Width, Double.PositiveInfinity)
            For Each element As UIElement In InternalChildren
                element.Measure(enfant)
            Next
            Return Mesurer(disponible.Width)
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Centrer(Visibles(), finale.Width)
            Return finale
        End Function

        Private Function Visibles() As List(Of UIElement)
            Return InternalChildren.Cast(Of UIElement)().Where(Function(e) e.Visibility <> Visibility.Collapsed).ToList()
        End Function

        ' La taille des lignes : chacune aussi haute que son plus haut élément.
        Private Function Mesurer(largeur As Double) As Size
            Dim x = 0.0, y = 0.0, hauteurDeLigne = 0.0, plusLarge = 0.0
            For Each element In Visibles()
                Dim taille = element.DesiredSize
                If x > 0 AndAlso Not Double.IsInfinity(largeur) AndAlso x + taille.Width > largeur Then
                    x = 0
                    y += hauteurDeLigne + Gap
                    hauteurDeLigne = 0
                End If
                hauteurDeLigne = Math.Max(hauteurDeLigne, taille.Height)
                x += taille.Width + Gap
                plusLarge = Math.Max(plusLarge, x - Gap)
            Next
            Return New Size(If(Double.IsInfinity(largeur), plusLarge, Math.Min(plusLarge, largeur)), y + hauteurDeLigne)
        End Function

        ' Ligne après ligne, chaque élément centré sur la hauteur de la sienne ; le dernier s'étire jusqu'au bout.
        Private Sub Centrer(visibles As List(Of UIElement), largeur As Double)
            Dim x = 0.0, y = 0.0
            Dim ligne As New List(Of UIElement)
            Dim hauteur = 0.0
            Dim poserLigne = Sub()
                                 Dim gauche = 0.0
                                 For Each e In ligne
                                     Dim w = If(e Is visibles(visibles.Count - 1) AndAlso Not Double.IsInfinity(largeur), Math.Max(e.DesiredSize.Width, largeur - gauche), e.DesiredSize.Width)
                                     e.Arrange(New Rect(gauche, y + (hauteur - e.DesiredSize.Height) / 2, w, e.DesiredSize.Height))
                                     gauche += w + Gap
                                 Next
                             End Sub
            For Each element In visibles
                Dim w = element.DesiredSize.Width
                If x > 0 AndAlso Not Double.IsInfinity(largeur) AndAlso x + w > largeur Then
                    poserLigne()
                    y += hauteur + Gap
                    x = 0
                    hauteur = 0
                    ligne.Clear()
                End If
                ligne.Add(element)
                hauteur = Math.Max(hauteur, element.DesiredSize.Height)
                x += w + Gap
            Next
            If ligne.Count > 0 Then poserLigne()
        End Sub

    End Class

End Namespace
