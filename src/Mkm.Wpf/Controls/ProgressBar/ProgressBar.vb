Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media.Animation

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La barre de progression de mkm : un trait de 6 px aux bouts ronds, rempli de la couleur principale sur un
    ''' fond pâle ; elle avance en une seconde vers sa valeur. Indéterminée (<c>IsIndeterminate</c>), deux traits la
    ''' parcourent sans fin. Trop fine pour porter son pourcentage, elle le laisse au texte qui l'accompagne.
    ''' <code>&lt;mkm:ProgressBar Value="{Binding Avancement}" Maximum="100" /&gt;</code>
    ''' </summary>
    ''' <remarks>
    ''' C'est le <see cref="System.Windows.Controls.ProgressBar"/> de WPF (<c>Minimum</c>, <c>Maximum</c>,
    ''' <c>Value</c>, accessibilité) ; en VB, à côté de celui de WPF, on écrit <c>Mkm.Wpf.Controls.ProgressBar</c>.
    ''' </remarks>
    <TemplatePart(Name:=ProgressBar.PartIndeterminate, Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:=ProgressBar.PartLong, Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:=ProgressBar.PartShort, Type:=GetType(FrameworkElement))>
    Public Class ProgressBar
        Inherits System.Windows.Controls.ProgressBar

        ''' <summary>La zone des deux traits de la barre indéterminée.</summary>
        Public Const PartIndeterminate As String = "PART_Indeterminate"
        ''' <summary>Le long trait de la barre indéterminée.</summary>
        Public Const PartLong As String = "PART_Long"
        ''' <summary>Le court trait de la barre indéterminée, qui part 1,15 s après le long.</summary>
        Public Const PartShort As String = "PART_Short"

        ' La transition de mkm : « width 1s ease-in-out ».
        Private Shared ReadOnly _avance As New Duration(TimeSpan.FromSeconds(1))
        ' Le cycle de mkm : 2,1 s ; chaque trait traverse en 60 % du cycle.
        Private Shared ReadOnly _cycle As TimeSpan = TimeSpan.FromSeconds(2.1)

        Private _indicateur As FrameworkElement
        Private _zone As FrameworkElement
        Private _long As FrameworkElement
        Private _court As FrameworkElement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ProgressBar), New FrameworkPropertyMetadata(GetType(ProgressBar)))
        End Sub

        ''' <summary>Une barre de 0 à 100.</summary>
        Public Sub New()
            AddHandler IsVisibleChanged, Sub() Animer()
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _zone IsNot Nothing Then RemoveHandler _zone.SizeChanged, AddressOf ZoneRedimensionnee
            MyBase.OnApplyTemplate()
            _indicateur = TryCast(GetTemplateChild("PART_Indicator"), FrameworkElement)
            _zone = TryCast(GetTemplateChild(PartIndeterminate), FrameworkElement)
            _long = TryCast(GetTemplateChild(PartLong), FrameworkElement)
            _court = TryCast(GetTemplateChild(PartShort), FrameworkElement)
            If _zone IsNot Nothing Then AddHandler _zone.SizeChanged, AddressOf ZoneRedimensionnee
            Animer()
            Arrondir()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnRenderSizeChanged(sizeInfo As SizeChangedInfo)
            MyBase.OnRenderSizeChanged(sizeInfo)
            Arrondir()
        End Sub

        ' Les coins du thème (999 px chez mkm : des bouts ronds), bornés comme CSS les borne : WPF dessinerait sinon
        ' un fuseau. Le masque coupe aussi la valeur et les traits à ces coins.
        Private Sub Arrondir()
            Dim masque = TryCast(GetTemplateChild("PART_Mask"), Border)
            If masque Is Nothing Then Return
            Dim coins = TryFindResource(Theming.ThemeResources.Prefix & "progressbar.border.radius")
            Dim rayon = If(TypeOf coins Is CornerRadius, DirectCast(coins, CornerRadius).TopLeft, 0.0)
            masque.CornerRadius = New CornerRadius(Math.Min(rayon, Math.Min(ActualHeight, ActualWidth) / 2))
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnValueChanged(oldValue As Double, newValue As Double)
            Dim avant = If(_indicateur Is Nothing, 0.0, _indicateur.ActualWidth)
            MyBase.OnValueChanged(oldValue, newValue)
            If _indicateur Is Nothing OrElse IsIndeterminate OrElse Not IsLoaded Then Return
            ' WPF pose la largeur de l'indicateur ; on y va en une seconde, puis l'animation rend la main à WPF.
            Dim apres = _indicateur.Width
            If Double.IsNaN(apres) OrElse Math.Abs(apres - avant) < 0.5 Then Return
            _indicateur.BeginAnimation(WidthProperty, New DoubleAnimation(avant, apres, _avance) With {
                .EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseInOut},
                .FillBehavior = FillBehavior.Stop
            })
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPropertyChanged(e As DependencyPropertyChangedEventArgs)
            MyBase.OnPropertyChanged(e)
            If e.Property Is IsIndeterminateProperty Then Animer()
        End Sub

        Private Sub ZoneRedimensionnee(sender As Object, e As SizeChangedEventArgs)
            If e.WidthChanged Then Animer()
        End Sub

        ' Les deux traits de mkm, en pourcentages de la largeur : le long de -35 % à 100 % (son bout droit de
        ' 100 % à -90 % du bord droit), le court, 1,15 s plus tard, de -200 % à 107 % (de 100 % à -8 %).
        Private Sub Animer()
            If _long Is Nothing OrElse _court Is Nothing OrElse _zone Is Nothing Then Return
            Dim largeur = _zone.ActualWidth
            If Not IsIndeterminate OrElse Not IsVisible OrElse largeur <= 0 Then
                Arreter(_long)
                Arreter(_court)
                Return
            End If
            Traverser(_long, largeur, -0.35, 1.0, 1.0, -0.9, New KeySpline(0.65, 0.815, 0.735, 0.395), TimeSpan.Zero)
            Traverser(_court, largeur, -2.0, 1.0, 1.07, -0.08, New KeySpline(0.165, 0.84, 0.44, 1), TimeSpan.FromSeconds(1.15))
        End Sub

        Private Shared Sub Traverser(trait As FrameworkElement, largeur As Double, gaucheDebut As Double, droiteDebut As Double,
                                     gaucheFin As Double, droiteFin As Double, courbe As KeySpline, retard As TimeSpan)
            Dim traversee = TimeSpan.FromTicks(CLng(_cycle.Ticks * 0.6))
            Dim gauche As New DoubleAnimationUsingKeyFrames With {.Duration = _cycle, .RepeatBehavior = RepeatBehavior.Forever, .BeginTime = retard}
            gauche.KeyFrames.Add(New DiscreteDoubleKeyFrame(gaucheDebut * largeur, KeyTime.FromTimeSpan(TimeSpan.Zero)))
            gauche.KeyFrames.Add(New SplineDoubleKeyFrame(gaucheFin * largeur, KeyTime.FromTimeSpan(traversee), courbe))
            Dim large As New DoubleAnimationUsingKeyFrames With {.Duration = _cycle, .RepeatBehavior = RepeatBehavior.Forever, .BeginTime = retard}
            large.KeyFrames.Add(New DiscreteDoubleKeyFrame(Math.Max(0, (1 - gaucheDebut - droiteDebut) * largeur), KeyTime.FromTimeSpan(TimeSpan.Zero)))
            large.KeyFrames.Add(New SplineDoubleKeyFrame(Math.Max(0, (1 - gaucheFin - droiteFin) * largeur), KeyTime.FromTimeSpan(traversee), courbe))
            trait.BeginAnimation(Canvas.LeftProperty, gauche)
            trait.BeginAnimation(WidthProperty, large)
        End Sub

        Private Shared Sub Arreter(trait As FrameworkElement)
            trait.BeginAnimation(Canvas.LeftProperty, Nothing)
            trait.BeginAnimation(WidthProperty, Nothing)
        End Sub

    End Class

End Namespace
