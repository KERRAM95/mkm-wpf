Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Animation

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le chargeur de mkm : un arc de la couleur principale qui tourne et s'allonge, comme celui de PrimeVue
    ''' (100 px par défaut ; un cercle aux quatre cinquièmes de sa taille, un trait de 2 sur 50). Il ne tourne que
    ''' visible.
    ''' <code>&lt;mkm:ProgressSpinner Width="32" Height="32" /&gt;</code>
    ''' </summary>
    ''' <remarks>Il se nomme « En cours » pour l'accessibilité (rôle « progressbar » de mkm).</remarks>
    Public Class ProgressSpinner
        Inherits Control

        ''' <summary>La couleur de l'arc.</summary>
        Public Shared ReadOnly StrokeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Stroke), GetType(Brush), GetType(ProgressSpinner),
            New FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender))

        ''' <summary>L'épaisseur du trait, sur une grille de 50 comme le SVG de PrimeVue (2 par défaut).</summary>
        Public Shared ReadOnly StrokeWidthProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(StrokeWidth), GetType(Double), GetType(ProgressSpinner),
            New FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender))

        ''' <summary>La durée d'un tour (2 s par défaut, comme <c>animationDuration</c> de PrimeVue).</summary>
        Public Shared ReadOnly AnimationDurationProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AnimationDuration), GetType(Duration), GetType(ProgressSpinner),
            New FrameworkPropertyMetadata(New Duration(TimeSpan.FromSeconds(2)), AddressOf OnAnimationChanged))

        ' Le début et la fin de l'arc et la rotation, en degrés, que les animations font varier.
        Private Shared ReadOnly DebutProperty As DependencyProperty = DependencyProperty.Register(
            "Debut", GetType(Double), GetType(ProgressSpinner), New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender))
        Private Shared ReadOnly FinProperty As DependencyProperty = DependencyProperty.Register(
            "Fin", GetType(Double), GetType(ProgressSpinner), New FrameworkPropertyMetadata(255.0, FrameworkPropertyMetadataOptions.AffectsRender))
        Private Shared ReadOnly RotationProperty As DependencyProperty = DependencyProperty.Register(
            "Rotation", GetType(Double), GetType(ProgressSpinner), New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender))

        ' Le trait de PrimeVue : stroke-dasharray 89 sur un cercle de rayon 20 (125,66), en degrés.
        Private Const Longueur As Double = 89 / (2 * Math.PI * 20) * 360
        ' Son décalage : -35 à mi-course, -124 à la fin.
        Private Const DecalageMilieu As Double = 35 / (2 * Math.PI * 20) * 360
        Private Const DecalageFin As Double = 124 / (2 * Math.PI * 20) * 360
        Private Const UnPoint As Double = 1 / (2 * Math.PI * 20) * 360

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ProgressSpinner), New FrameworkPropertyMetadata(GetType(ProgressSpinner)))
            FocusableProperty.OverrideMetadata(GetType(ProgressSpinner), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un chargeur de 100 px.</summary>
        Public Sub New()
            AddHandler IsVisibleChanged, Sub() Animer()
        End Sub

        ''' <summary>La couleur de l'arc.</summary>
        Public Property Stroke As Brush
            Get
                Return DirectCast(GetValue(StrokeProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(StrokeProperty, value)
            End Set
        End Property

        ''' <summary>L'épaisseur du trait, sur une grille de 50 comme le SVG de PrimeVue (2 par défaut).</summary>
        Public Property StrokeWidth As Double
            Get
                Return CDbl(GetValue(StrokeWidthProperty))
            End Get
            Set(value As Double)
                SetValue(StrokeWidthProperty, value)
            End Set
        End Property

        ''' <summary>La durée d'un tour (2 s par défaut).</summary>
        Public Property AnimationDuration As Duration
            Get
                Return DirectCast(GetValue(AnimationDurationProperty), Duration)
            End Get
            Set(value As Duration)
                SetValue(AnimationDurationProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Sub OnRender(drawingContext As DrawingContext)
            MyBase.OnRender(drawingContext)
            Dim cote = Math.Min(ActualWidth, ActualHeight)
            If cote <= 0 Then Return
            ' Le SVG de PrimeVue : viewBox de 50, cercle de rayon 20, trait de StrokeWidth.
            Dim echelle = cote / 50
            Dim rayon = 20 * echelle
            Dim centre As New Point(ActualWidth / 2, ActualHeight / 2)
            Dim debut = CDbl(GetValue(DebutProperty))
            Dim fin = Math.Min(CDbl(GetValue(FinProperty)), 360)
            If fin - debut < 0.1 Then Return

            Dim rotation = CDbl(GetValue(RotationProperty))
            Dim crayon As New Pen(Stroke, StrokeWidth * echelle) With {.StartLineCap = PenLineCap.Round, .EndLineCap = PenLineCap.Round}
            Dim arc As New StreamGeometry()
            Using g = arc.Open()
                g.BeginFigure(SurLeCercle(centre, rayon, debut + rotation), False, False)
                g.ArcTo(SurLeCercle(centre, rayon, fin + rotation), New Size(rayon, rayon), 0, fin - debut > 180, SweepDirection.Clockwise, True, False)
            End Using
            arc.Freeze()
            drawingContext.DrawGeometry(Nothing, crayon, arc)
        End Sub

        Private Shared Function SurLeCercle(centre As Point, rayon As Double, degres As Double) As Point
            Dim radians = degres * Math.PI / 180
            Return New Point(centre.X + rayon * Math.Cos(radians), centre.Y + rayon * Math.Sin(radians))
        End Function

        Private Shared Sub OnAnimationChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, ProgressSpinner).Animer()
        End Sub

        ' Les animations de PrimeVue : un tour en 2 s, linéaire ; le trait, en 1,5 s (ease-in-out), d'un point à
        ' un arc de 89 décalé de 35, puis décalé de 124, où il redevient un point au bout du cercle.
        Private Sub Animer()
            If Not IsVisible Then
                BeginAnimation(RotationProperty, Nothing)
                BeginAnimation(DebutProperty, Nothing)
                BeginAnimation(FinProperty, Nothing)
                Return
            End If

            BeginAnimation(RotationProperty, New DoubleAnimation(0, 360, AnimationDuration) With {.RepeatBehavior = RepeatBehavior.Forever})

            Dim demi = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.75))
            Dim tout = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5))
            Dim doux As New KeySpline(0.42, 0, 0.58, 1)
            Dim debut As New DoubleAnimationUsingKeyFrames With {.RepeatBehavior = RepeatBehavior.Forever}
            debut.KeyFrames.Add(New DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)))
            debut.KeyFrames.Add(New SplineDoubleKeyFrame(DecalageMilieu, demi, doux))
            debut.KeyFrames.Add(New SplineDoubleKeyFrame(DecalageFin, tout, doux))
            Dim fin As New DoubleAnimationUsingKeyFrames With {.RepeatBehavior = RepeatBehavior.Forever}
            fin.KeyFrames.Add(New DiscreteDoubleKeyFrame(UnPoint, KeyTime.FromTimeSpan(TimeSpan.Zero)))
            fin.KeyFrames.Add(New SplineDoubleKeyFrame(DecalageMilieu + Longueur, demi, doux))
            fin.KeyFrames.Add(New SplineDoubleKeyFrame(DecalageFin + Longueur, tout, doux))
            BeginAnimation(DebutProperty, debut)
            BeginAnimation(FinProperty, fin)
        End Sub

    End Class

End Namespace
