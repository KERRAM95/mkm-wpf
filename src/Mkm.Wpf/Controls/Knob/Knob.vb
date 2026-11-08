Imports System.Collections.Generic
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le bouton rotatif de mkm (<c>Knob</c>) : un arc gris, coloré jusqu'à la valeur, la valeur écrite au milieu ; on le
    ''' tourne à la souris ou au clavier.
    ''' <code>
    ''' &lt;mkm:Knob Value="40" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Un <see cref="RangeBase"/> : <c>Minimum</c> (0), <c>Maximum</c> (100), <c>Value</c>, <c>SmallChange</c> (le pas,
    ''' <c>step</c> chez mkm) et <c>LargeChange</c> (Page haut et bas). Pour les lecteurs d'écran, un curseur.
    ''' </remarks>
    <TemplatePart(Name:="PART_Drawing", Type:=GetType(FrameworkElement))>
    Public Class Knob
        Inherits RangeBase

        ''' <summary>Le côté du bouton, en pixels : 100 par défaut.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(Double), GetType(Knob), New FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsMeasure))

        ''' <summary>L'épaisseur de l'arc, sur un bouton de 100 : 14 par défaut.</summary>
        Public Shared ReadOnly StrokeWidthProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(StrokeWidth), GetType(Double), GetType(Knob), New FrameworkPropertyMetadata(14.0))

        ''' <summary>Vrai (par défaut) pour la valeur écrite au milieu.</summary>
        Public Shared ReadOnly ShowValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowValue), GetType(Boolean), GetType(Knob), New FrameworkPropertyMetadata(True))

        ''' <summary>Le format de la valeur écrite, « {0} » par défaut (« {0} % » : en XAML, <c>"{}{0} %"</c>).</summary>
        Public Shared ReadOnly ValueFormatProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ValueFormat), GetType(String), GetType(Knob), New FrameworkPropertyMetadata("{0}"))

        ''' <summary>Vrai pour un bouton qu'on lit sans le tourner.</summary>
        Public Shared ReadOnly IsReadOnlyProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsReadOnly), GetType(Boolean), GetType(Knob), New FrameworkPropertyMetadata(False))

        ''' <summary>La couleur de l'arc jusqu'à la valeur : celle du thème par défaut.</summary>
        Public Shared ReadOnly ValueColorProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ValueColor), GetType(Brush), GetType(Knob), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La couleur de l'arc entier, sous la valeur : un gris par défaut.</summary>
        Public Shared ReadOnly RangeColorProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(RangeColor), GetType(Brush), GetType(Knob), New FrameworkPropertyMetadata(Nothing))

        ' Les angles de mkm : l'arc va de 240° à -60°, en passant par le haut.
        Private Const Rayon As Double = 40
        Private Const Milieu As Double = 50
        Private Shared ReadOnly AngleMin As Double = 4 * Math.PI / 3
        Private Shared ReadOnly AngleMax As Double = -Math.PI / 3

        Private _dessin As FrameworkElement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Knob), New FrameworkPropertyMetadata(GetType(Knob)))
            FocusableProperty.OverrideMetadata(GetType(Knob), New FrameworkPropertyMetadata(True))
            MaximumProperty.OverrideMetadata(GetType(Knob), New FrameworkPropertyMetadata(100.0))
            LargeChangeProperty.OverrideMetadata(GetType(Knob), New FrameworkPropertyMetadata(10.0))
        End Sub

        ''' <summary>Le côté du bouton, en pixels.</summary>
        Public Property Size As Double
            Get
                Return CDbl(GetValue(SizeProperty))
            End Get
            Set(value As Double)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>L'épaisseur de l'arc, sur un bouton de 100.</summary>
        Public Property StrokeWidth As Double
            Get
                Return CDbl(GetValue(StrokeWidthProperty))
            End Get
            Set(value As Double)
                SetValue(StrokeWidthProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour la valeur écrite au milieu.</summary>
        Public Property ShowValue As Boolean
            Get
                Return CBool(GetValue(ShowValueProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowValueProperty, value)
            End Set
        End Property

        ''' <summary>Le format de la valeur écrite.</summary>
        Public Property ValueFormat As String
            Get
                Return CStr(GetValue(ValueFormatProperty))
            End Get
            Set(value As String)
                SetValue(ValueFormatProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour un bouton qu'on lit sans le tourner.</summary>
        Public Property IsReadOnly As Boolean
            Get
                Return CBool(GetValue(IsReadOnlyProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsReadOnlyProperty, value)
            End Set
        End Property

        ''' <summary>La couleur de l'arc jusqu'à la valeur.</summary>
        Public Property ValueColor As Brush
            Get
                Return DirectCast(GetValue(ValueColorProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(ValueColorProperty, value)
            End Set
        End Property

        ''' <summary>La couleur de l'arc entier.</summary>
        Public Property RangeColor As Brush
            Get
                Return DirectCast(GetValue(RangeColorProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(RangeColorProperty, value)
            End Set
        End Property

        ''' <summary>Le texte de la valeur, au format <see cref="ValueFormat"/>.</summary>
        Friend ReadOnly Property TexteDeLaValeur As String
            Get
                Return String.Format(CultureInfo.CurrentCulture, If(ValueFormat, "{0}"), Value)
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _dessin = TryCast(GetTemplateChild("PART_Drawing"), FrameworkElement)
        End Sub

        ''' <summary>Tout ce qui change le dessin le redessine.</summary>
        Protected Overrides Sub OnPropertyChanged(e As DependencyPropertyChangedEventArgs)
            MyBase.OnPropertyChanged(e)
            If e.Property Is ValueProperty OrElse e.Property Is MinimumProperty OrElse e.Property Is MaximumProperty OrElse
               e.Property Is StrokeWidthProperty OrElse e.Property Is ShowValueProperty OrElse e.Property Is ValueFormatProperty OrElse
               e.Property Is ValueColorProperty OrElse e.Property Is RangeColorProperty OrElse e.Property Is ForegroundProperty OrElse
               e.Property Is FontSizeProperty OrElse e.Property Is FontFamilyProperty Then
                _dessin?.InvalidateVisual()
            End If
        End Sub

        ''' <summary>Le carré de <see cref="Size"/>.</summary>
        Protected Overrides Function MeasureOverride(contrainte As Windows.Size) As Windows.Size
            MyBase.MeasureOverride(New Windows.Size(Size, Size))
            Return New Windows.Size(Size, Size)
        End Function

        Private Function VersAngle(valeur As Double, bas As Double, haut As Double) As Double
            Return Projeter(valeur, bas, haut, AngleMin, AngleMax)
        End Function

        Private Shared Function Projeter(x As Double, entreeMin As Double, entreeMax As Double, sortieMin As Double, sortieMax As Double) As Double
            If entreeMax = entreeMin Then Return sortieMin
            Return (x - entreeMin) * (sortieMax - sortieMin) / (entreeMax - entreeMin) + sortieMin
        End Function

        Private Shared Function Point(angle As Double) As Windows.Point
            Return New Windows.Point(Milieu + Math.Cos(angle) * Rayon, Milieu - Math.Sin(angle) * Rayon)
        End Function

        ''' <summary>Dessine l'arc, l'arc de la valeur et le texte, sur la grille de 100 de mkm.</summary>
        Friend Sub Dessiner(dc As DrawingContext, cote As Double)
            If cote <= 0 Then Return
            Dim echelle = cote / 100
            dc.PushTransform(New ScaleTransform(echelle, echelle))

            Dim arc As New PathGeometry()
            Dim entier As New PathFigure With {.StartPoint = Point(AngleMin), .IsClosed = False}
            entier.Segments.Add(New ArcSegment(Point(AngleMax), New Windows.Size(Rayon, Rayon), 0, True, SweepDirection.Clockwise, True))
            arc.Figures.Add(entier)
            dc.DrawGeometry(Nothing, New Pen(RangeColor, StrokeWidth), arc)

            ' De zéro (ou du minimum, s'il est positif) à la valeur.
            Dim zero = If(Minimum > 0 AndAlso Maximum > 0, VersAngle(Minimum, Minimum, Maximum), VersAngle(0, Minimum, Maximum))
            Dim angle = VersAngle(Value, Minimum, Maximum)
            If Math.Abs(angle - zero) > 0.0001 Then
                Dim valeur As New PathGeometry()
                Dim figure As New PathFigure With {.StartPoint = Point(zero), .IsClosed = False}
                figure.Segments.Add(New ArcSegment(Point(angle), New Windows.Size(Rayon, Rayon), 0, Math.Abs(zero - angle) >= Math.PI,
                                                   If(angle > zero, SweepDirection.Counterclockwise, SweepDirection.Clockwise), True))
                valeur.Figures.Add(figure)
                dc.DrawGeometry(Nothing, New Pen(ValueColor, StrokeWidth), valeur)
            End If

            If ShowValue Then
                Dim texte As New FormattedText(TexteDeLaValeur, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                               New Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Foreground,
                                               VisualTreeHelper.GetDpi(Me).PixelsPerDip)
                ' Centré, sa ligne de base à 57, comme mkm.
                dc.DrawText(texte, New Windows.Point(Milieu - texte.Width / 2, 57 - texte.Baseline))
            End If
            dc.Pop()
        End Sub

        ' ---- La souris ----

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeftButtonDown(e As MouseButtonEventArgs)
            MyBase.OnMouseLeftButtonDown(e)
            If IsReadOnly OrElse Not IsEnabled Then Return
            Focus()
            CaptureMouse()
            Tourner(e.GetPosition(Me))
            e.Handled = True
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            MyBase.OnMouseMove(e)
            If IsMouseCaptured Then Tourner(e.GetPosition(Me))
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeftButtonUp(e As MouseButtonEventArgs)
            MyBase.OnMouseLeftButtonUp(e)
            If IsMouseCaptured Then ReleaseMouseCapture()
        End Sub

        ' Le point visé donne son angle, et l'angle la valeur, au pas près ; le bas de l'arc (hors de lui) ne compte pas.
        Friend Sub Tourner(point As Windows.Point)
            Dim cote = If(ActualWidth > 0, ActualWidth, Size)
            Dim dx = point.X - cote / 2
            Dim dy = cote / 2 - point.Y
            Dim angle = Math.Atan2(dy, dx)
            Dim debut = -Math.PI / 2 - Math.PI / 6
            Dim valeur As Double
            If angle > AngleMax Then
                valeur = Projeter(angle, AngleMin, AngleMax, Minimum, Maximum)
            ElseIf angle < debut Then
                valeur = Projeter(angle + 2 * Math.PI, AngleMin, AngleMax, Minimum, Maximum)
            Else
                Return
            End If
            Dim pas = If(SmallChange > 0, SmallChange, 1)
            SetCurrentValue(ValueProperty, Math.Round((valeur - Minimum) / pas) * pas + Minimum)
        End Sub

        ''' <summary>→ et ↑ montent d'un pas, ← et ↓ descendent, Page haut et bas de <c>LargeChange</c>, Début et Fin aux bornes.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse IsReadOnly Then Return
            Select Case e.Key
                Case Key.Right, Key.Up
                    SetCurrentValue(ValueProperty, Value + SmallChange)
                Case Key.Left, Key.Down
                    SetCurrentValue(ValueProperty, Value - SmallChange)
                Case Key.PageUp
                    SetCurrentValue(ValueProperty, Value + LargeChange)
                Case Key.PageDown
                    SetCurrentValue(ValueProperty, Value - LargeChange)
                Case Key.Home
                    SetCurrentValue(ValueProperty, Minimum)
                Case Key.End
                    SetCurrentValue(ValueProperty, Maximum)
                Case Else
                    Return
            End Select
            e.Handled = True
        End Sub

        ''' <summary>Un curseur pour les lecteurs d'écran, avec sa valeur.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeKnob(Me)
        End Function

        Private NotInheritable Class PairDeKnob
            Inherits RangeBaseAutomationPeer

            Public Sub New(bouton As Knob)
                MyBase.New(bouton)
            End Sub

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.Slider
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(Knob)
            End Function

            Protected Overrides Function GetChildrenCore() As List(Of AutomationPeer)
                Return Nothing
            End Function

        End Class

    End Class

    ''' <summary>Le dessin d'un <see cref="Knob"/>, dans son gabarit : il demande au bouton de se dessiner.</summary>
    Friend NotInheritable Class DessinDeKnob
        Inherits FrameworkElement

        Protected Overrides Sub OnRender(dc As DrawingContext)
            TryCast(TemplatedParent, Knob)?.Dessiner(dc, Math.Min(ActualWidth, ActualHeight))
        End Sub

    End Class

End Namespace
