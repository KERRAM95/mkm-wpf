Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une rangée d'éléments, centrés en hauteur, séparés d'un écart (<see cref="Gap"/>) : le
    ''' <c>gap</c> de CSS, que <see cref="StackPanel"/> n'a pas. Sert aux onglets et aux commandes
    ''' d'une <see cref="ActionBar"/>.
    ''' </summary>
    Public Class ActionBarPanel
        Inherits System.Windows.Controls.Panel

        ''' <summary>L'écart entre deux éléments visibles.</summary>
        Public Shared ReadOnly GapProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Gap), GetType(Double), GetType(ActionBarPanel),
            New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure))

        ''' <summary>L'écart entre deux éléments visibles.</summary>
        Public Property Gap As Double
            Get
                Return CDbl(GetValue(GapProperty))
            End Get
            Set(value As Double)
                SetValue(GapProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function MeasureOverride(availableSize As Size) As Size
            Dim largeur = 0.0
            Dim hauteur = 0.0
            Dim visibles = 0
            For Each enfant As UIElement In InternalChildren
                If enfant Is Nothing Then Continue For
                enfant.Measure(New Size(Double.PositiveInfinity, availableSize.Height))
                If enfant.Visibility = Visibility.Collapsed Then Continue For
                largeur += enfant.DesiredSize.Width
                hauteur = Math.Max(hauteur, enfant.DesiredSize.Height)
                visibles += 1
            Next
            If visibles > 1 Then largeur += Gap * (visibles - 1)
            Return New Size(largeur, hauteur)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function ArrangeOverride(finalSize As Size) As Size
            Dim x = 0.0
            For Each enfant As UIElement In InternalChildren
                If enfant Is Nothing Then Continue For
                If enfant.Visibility = Visibility.Collapsed Then
                    enfant.Arrange(New Rect())
                    Continue For
                End If
                Dim taille = enfant.DesiredSize
                enfant.Arrange(New Rect(x, (finalSize.Height - taille.Height) / 2, taille.Width, taille.Height))
                x += taille.Width + Gap
            Next
            Return finalSize
        End Function

    End Class

End Namespace
