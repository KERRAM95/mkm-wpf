Imports System.Windows
Imports System.Windows.Documents
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une icône : une géométrie de <see cref="Icons"/>, tracée à la couleur du texte
    ''' (<see cref="Foreground"/>, héritée), ou une image de l'application.
    ''' <code>&lt;mkm:Icon Source="{x:Static mkm:Icons.Add}" Width="20" Height="20" /&gt;</code>
    ''' </summary>
    ''' <remarks>
    ''' Une géométrie se lit sur la grille de 24 de Tabler et se met à la taille de l'élément ; son trait
    ''' (<see cref="StrokeThickness"/>, 1,5 sur 24) suit la taille : 1 px pour une icône de 16 px, le trait
    ''' fin de mkm. Sans taille donnée, l'icône fait 16 px, comme dans mkm.
    ''' </remarks>
    Public Class Icon
        Inherits FrameworkElement

        ''' <summary>La grille des géométries de Tabler.</summary>
        Public Const GridSize As Double = 24

        ''' <summary>La taille d'une icône sans taille donnée, comme dans mkm (<c>icon.size</c>).</summary>
        Public Const DefaultSize As Double = 16

        ''' <summary>Ce que l'icône dessine : une <see cref="Geometry"/> (tracée) ou une <see cref="ImageSource"/>.</summary>
        Public Shared ReadOnly SourceProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Source), GetType(Object), GetType(Icon),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.AffectsRender))

        ''' <summary>La couleur du trait, héritée du texte qui l'entoure.</summary>
        Public Shared ReadOnly ForegroundProperty As DependencyProperty = TextElement.ForegroundProperty.AddOwner(
            GetType(Icon),
            New FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits Or FrameworkPropertyMetadataOptions.AffectsRender))

        ''' <summary>L'épaisseur du trait sur la grille de 24 : 1,5, le trait fin de mkm.</summary>
        Public Shared ReadOnly StrokeThicknessProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(StrokeThickness), GetType(Double), GetType(Icon),
            New FrameworkPropertyMetadata(1.5, FrameworkPropertyMetadataOptions.AffectsRender))

        Shared Sub New()
            ' Une icône ne prend pas le focus et laisse passer les clics à ce qui la porte.
            FocusableProperty.OverrideMetadata(GetType(Icon), New FrameworkPropertyMetadata(False))
            IsHitTestVisibleProperty.OverrideMetadata(GetType(Icon), New UIPropertyMetadata(False))
        End Sub

        ''' <summary>Ce que l'icône dessine.</summary>
        Public Property Source As Object
            Get
                Return GetValue(SourceProperty)
            End Get
            Set(value As Object)
                SetValue(SourceProperty, value)
            End Set
        End Property

        ''' <summary>La couleur du trait d'une géométrie.</summary>
        Public Property Foreground As Brush
            Get
                Return DirectCast(GetValue(ForegroundProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(ForegroundProperty, value)
            End Set
        End Property

        ''' <summary>L'épaisseur du trait d'une géométrie, sur la grille de 24.</summary>
        Public Property StrokeThickness As Double
            Get
                Return CDbl(GetValue(StrokeThicknessProperty))
            End Get
            Set(value As Double)
                SetValue(StrokeThicknessProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function MeasureOverride(availableSize As Size) As Size
            Return New Size(Math.Min(DefaultSize, availableSize.Width), Math.Min(DefaultSize, availableSize.Height))
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnRender(drawingContext As DrawingContext)
            Dim cote = Math.Min(ActualWidth, ActualHeight)
            If cote <= 0 Then Return

            ' Centrée dans l'élément, au plus grand carré qu'il contient.
            Dim gauche = (ActualWidth - cote) / 2
            Dim haut = (ActualHeight - cote) / 2

            Dim geometrie = TryCast(Source, Geometry)
            If geometrie IsNot Nothing Then
                Dim echelle = cote / GridSize
                Dim trait As New Pen(Foreground, StrokeThickness) With {
                    .StartLineCap = PenLineCap.Round,
                    .EndLineCap = PenLineCap.Round,
                    .LineJoin = PenLineJoin.Round
                }
                drawingContext.PushTransform(New MatrixTransform(echelle, 0, 0, echelle, gauche, haut))
                drawingContext.DrawGeometry(Nothing, trait, geometrie)
                drawingContext.Pop()
                Return
            End If

            Dim image = TryCast(Source, ImageSource)
            If image IsNot Nothing Then drawingContext.DrawImage(image, New Rect(gauche, haut, cote, cote))
        End Sub

    End Class

End Namespace
