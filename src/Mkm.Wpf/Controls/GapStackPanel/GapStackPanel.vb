Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une pile d'éléments séparés d'un écart (<see cref="Gap"/>), le <c>gap</c> d'une boîte flexible de CSS : l'écart ne
    ''' se pose qu'entre deux éléments visibles, ni avant le premier, ni après le dernier, ni autour d'un élément replié.
    ''' Verticale par défaut ; chaque élément y prend toute la largeur, son alignement faisant le reste.
    ''' </summary>
    Public Class GapStackPanel
        Inherits Panel

        ''' <summary>
        ''' Un groupe pour les lecteurs d'écran quand la pile est une région « vivante » (<c>AutomationProperties.LiveSetting</c>,
        ''' comme le contenu d'un EmptyContent) : sans pair, son annonce ne partait pas. Sinon, une simple mise en page,
        ''' invisible pour eux.
        ''' </summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            If System.Windows.Automation.AutomationProperties.GetLiveSetting(Me) = System.Windows.Automation.AutomationLiveSetting.Off Then Return Nothing
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

        ''' <summary>L'écart entre deux éléments visibles.</summary>
        Public Shared ReadOnly GapProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Gap), GetType(Double), GetType(GapStackPanel),
            New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure))

        ''' <summary>Le sens de la pile.</summary>
        Public Shared ReadOnly OrientationProperty As DependencyProperty = StackPanel.OrientationProperty.AddOwner(
            GetType(GapStackPanel), New FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure))

        ''' <summary>L'écart entre deux éléments visibles.</summary>
        Public Property Gap As Double
            Get
                Return CDbl(GetValue(GapProperty))
            End Get
            Set(value As Double)
                SetValue(GapProperty, value)
            End Set
        End Property

        ''' <summary>Le sens de la pile.</summary>
        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function MeasureOverride(availableSize As Size) As Size
            Dim verticale = Orientation = Orientation.Vertical
            Dim contrainte = If(verticale, New Size(availableSize.Width, Double.PositiveInfinity), New Size(Double.PositiveInfinity, availableSize.Height))
            Dim longueur = 0.0
            Dim travers = 0.0
            Dim visibles = 0
            For Each enfant In InternalChildren.OfType(Of UIElement)()
                enfant.Measure(contrainte)
                If enfant.Visibility = Visibility.Collapsed Then Continue For
                Dim taille = enfant.DesiredSize
                longueur += If(verticale, taille.Height, taille.Width)
                travers = Math.Max(travers, If(verticale, taille.Width, taille.Height))
                visibles += 1
            Next
            If visibles > 1 Then longueur += Gap * (visibles - 1)
            Return If(verticale, New Size(travers, longueur), New Size(longueur, travers))
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function ArrangeOverride(finalSize As Size) As Size
            Dim verticale = Orientation = Orientation.Vertical
            Dim position = 0.0
            For Each enfant In InternalChildren.OfType(Of UIElement)()
                If enfant.Visibility = Visibility.Collapsed Then
                    enfant.Arrange(New Rect())
                    Continue For
                End If
                Dim taille = enfant.DesiredSize
                If verticale Then
                    enfant.Arrange(New Rect(0, position, finalSize.Width, taille.Height))
                    position += taille.Height + Gap
                Else
                    enfant.Arrange(New Rect(position, 0, taille.Width, finalSize.Height))
                    position += taille.Width + Gap
                End If
            Next
            Return finalSize
        End Function

    End Class

End Namespace
