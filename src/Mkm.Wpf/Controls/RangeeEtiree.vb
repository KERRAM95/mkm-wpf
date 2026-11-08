Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une rangée qui s'étire (flex chez mkm) : en ligne, le dernier élément garde sa largeur, les autres se partagent le
    ''' reste (les étapes d'un Stepper, les évènements d'une Timeline) ; en colonne, les éléments s'empilent.
    ''' </summary>
    Friend NotInheritable Class RangeeEtiree
        Inherits System.Windows.Controls.Panel

        Public Shared ReadOnly OrientationProperty As DependencyProperty = StackPanel.OrientationProperty.AddOwner(
            GetType(RangeeEtiree), New FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure))

        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            Dim largeur = 0.0
            Dim hauteur = 0.0
            Dim enLigne = Orientation = Orientation.Horizontal
            For Each enfant As UIElement In InternalChildren
                enfant.Measure(If(enLigne, New Size(Double.PositiveInfinity, disponible.Height), New Size(disponible.Width, Double.PositiveInfinity)))
                If enLigne Then
                    largeur += enfant.DesiredSize.Width
                    hauteur = Math.Max(hauteur, enfant.DesiredSize.Height)
                Else
                    largeur = Math.Max(largeur, enfant.DesiredSize.Width)
                    hauteur += enfant.DesiredSize.Height
                End If
            Next
            If enLigne AndAlso Not Double.IsInfinity(disponible.Width) Then largeur = Math.Max(largeur, disponible.Width)
            Return New Size(largeur, hauteur)
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim n = InternalChildren.Count
            If n = 0 Then Return finale
            If Orientation = Orientation.Vertical Then
                Dim y = 0.0
                For Each enfant As UIElement In InternalChildren
                    enfant.Arrange(New Rect(0, y, finale.Width, enfant.DesiredSize.Height))
                    y += enfant.DesiredSize.Height
                Next
                Return finale
            End If
            Dim dernier = InternalChildren(n - 1).DesiredSize.Width
            Dim part = If(n > 1, Math.Max(0, finale.Width - dernier) / (n - 1), 0)
            Dim x = 0.0
            For i = 0 To n - 1
                Dim largeur = If(i = n - 1, Math.Max(dernier, finale.Width - x), part)
                InternalChildren(i).Arrange(New Rect(x, 0, largeur, finale.Height))
                x += largeur
            Next
            Return finale
        End Function

    End Class

End Namespace
