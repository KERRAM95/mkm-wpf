Imports System.ComponentModel
Imports System.Globalization
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>Une jauge d'un <see cref="MeterGroup"/> : son libellé, sa valeur, sa couleur, son icône.</summary>
    Public Class MeterItem
        Inherits DependencyObject

        ''' <summary>Le libellé, dans la légende.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Label), GetType(String), GetType(MeterItem), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La valeur, entre le minimum et le maximum du groupe.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(Double), GetType(MeterItem), New FrameworkPropertyMetadata(0.0))

        ''' <summary>La couleur de la jauge et de son repère.</summary>
        Public Shared ReadOnly ColorProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Color), GetType(Brush), GetType(MeterItem), New FrameworkPropertyMetadata(Brushes.Gray))

        ''' <summary>Une icône à la place du repère rond (une géométrie).</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(MeterItem),
            New FrameworkPropertyMetadata(Nothing, Sub(d, e) d.SetValue(HasIconPropertyKey, e.NewValue IsNot Nothing)))

        Private Shared ReadOnly HasIconPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasIcon), GetType(Boolean), GetType(MeterItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand une icône remplace le repère.</summary>
        Public Shared ReadOnly HasIconProperty As DependencyProperty = HasIconPropertyKey.DependencyProperty

        Private Shared ReadOnly PercentPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Percent), GetType(Double), GetType(MeterItem), New FrameworkPropertyMetadata(0.0))
        ''' <summary>La part de la jauge, en pour cent de l'étendue du groupe.</summary>
        Public Shared ReadOnly PercentProperty As DependencyProperty = PercentPropertyKey.DependencyProperty

        Private Shared ReadOnly LegendPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Legend), GetType(String), GetType(MeterItem), New FrameworkPropertyMetadata(String.Empty))
        ''' <summary>Le texte de la légende : « Plans (16 %) ».</summary>
        Public Shared ReadOnly LegendProperty As DependencyProperty = LegendPropertyKey.DependencyProperty

        ''' <summary>Le libellé.</summary>
        Public Property Label As String
            Get
                Return CStr(GetValue(LabelProperty))
            End Get
            Set(value As String)
                SetValue(LabelProperty, value)
            End Set
        End Property

        ''' <summary>La valeur.</summary>
        Public Property Value As Double
            Get
                Return CDbl(GetValue(ValueProperty))
            End Get
            Set(value As Double)
                SetValue(ValueProperty, value)
            End Set
        End Property

        ''' <summary>La couleur.</summary>
        Public Property Color As Brush
            Get
                Return DirectCast(GetValue(ColorProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(ColorProperty, value)
            End Set
        End Property

        ''' <summary>Une icône à la place du repère.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand une icône remplace le repère.</summary>
        Public ReadOnly Property HasIcon As Boolean
            Get
                Return CBool(GetValue(HasIconProperty))
            End Get
        End Property

        ''' <summary>La part de la jauge, en pour cent.</summary>
        Public ReadOnly Property Percent As Double
            Get
                Return CDbl(GetValue(PercentProperty))
            End Get
        End Property

        ''' <summary>Le texte de la légende.</summary>
        Public ReadOnly Property Legend As String
            Get
                Return CStr(GetValue(LegendProperty))
            End Get
        End Property

        Friend Sub Poser(pourcent As Double)
            SetValue(PercentPropertyKey, pourcent)
            SetValue(LegendPropertyKey, String.Format(CultureInfo.CurrentCulture, "{0} ({1:0} %)", Label, pourcent))
        End Sub

    End Class

    ''' <summary>
    ''' Le groupe de jauges de mkm (<c>MeterGroup</c>) : des valeurs côte à côte dans une même barre, chacune de sa couleur,
    ''' et leur légende (libellé et part).
    ''' <code>
    ''' &lt;mkm:MeterGroup&gt;
    '''     &lt;mkm:MeterItem Label="Plans" Value="16" Color="#34D399" /&gt;
    ''' &lt;/mkm:MeterGroup&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Meters", Type:=GetType(System.Windows.Controls.Panel))>
    Public Class MeterGroup
        Inherits ItemsControl

        ''' <summary>Le bas de l'étendue : 0 par défaut.</summary>
        Public Shared ReadOnly MinimumProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Minimum), GetType(Double), GetType(MeterGroup), New FrameworkPropertyMetadata(0.0, AddressOf OnEtendueChanged))

        ''' <summary>Le haut de l'étendue : 100 par défaut.</summary>
        Public Shared ReadOnly MaximumProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Maximum), GetType(Double), GetType(MeterGroup), New FrameworkPropertyMetadata(100.0, AddressOf OnEtendueChanged))

        ''' <summary>La barre en ligne (par défaut, la légende dessous) ou en colonne (la légende à droite).</summary>
        Public Shared ReadOnly OrientationProperty As DependencyProperty = StackPanel.OrientationProperty.AddOwner(
            GetType(MeterGroup), New FrameworkPropertyMetadata(Orientation.Horizontal))

        ''' <summary>La légende après la barre (par défaut) ou avant.</summary>
        Public Shared ReadOnly LabelPositionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(LabelPosition), GetType(MeterGroupLabelPosition), GetType(MeterGroup), New FrameworkPropertyMetadata(MeterGroupLabelPosition.End))

        ''' <summary>La légende en ligne (par défaut) ou en colonne.</summary>
        Public Shared ReadOnly LabelOrientationProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(LabelOrientation), GetType(Orientation), GetType(MeterGroup), New FrameworkPropertyMetadata(Orientation.Horizontal))

        Private _barre As System.Windows.Controls.Panel
        Private ReadOnly _suivis As New List(Of MeterItem)

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(MeterGroup), New FrameworkPropertyMetadata(GetType(MeterGroup)))
            FocusableProperty.OverrideMetadata(GetType(MeterGroup), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un groupe de jauges vide.</summary>
        Public Sub New()
            AddHandler Loaded, Sub() Mesurer()
            AddHandler Unloaded, Sub() Detacher()
        End Sub

        ''' <summary>Le bas de l'étendue.</summary>
        Public Property Minimum As Double
            Get
                Return CDbl(GetValue(MinimumProperty))
            End Get
            Set(value As Double)
                SetValue(MinimumProperty, value)
            End Set
        End Property

        ''' <summary>Le haut de l'étendue.</summary>
        Public Property Maximum As Double
            Get
                Return CDbl(GetValue(MaximumProperty))
            End Get
            Set(value As Double)
                SetValue(MaximumProperty, value)
            End Set
        End Property

        ''' <summary>La barre en ligne ou en colonne.</summary>
        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        ''' <summary>La légende après la barre ou avant.</summary>
        Public Property LabelPosition As MeterGroupLabelPosition
            Get
                Return DirectCast(GetValue(LabelPositionProperty), MeterGroupLabelPosition)
            End Get
            Set(value As MeterGroupLabelPosition)
                SetValue(LabelPositionProperty, value)
            End Set
        End Property

        ''' <summary>La légende en ligne ou en colonne.</summary>
        Public Property LabelOrientation As Orientation
            Get
                Return DirectCast(GetValue(LabelOrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(LabelOrientationProperty, value)
            End Set
        End Property

        ''' <summary>Les jauges du groupe (ses éléments qui sont des MeterItem).</summary>
        Friend ReadOnly Property Jauges As List(Of MeterItem)
            Get
                Return Items.OfType(Of MeterItem)().ToList()
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _barre = TryCast(GetTemplateChild("PART_Meters"), System.Windows.Controls.Panel)
            Mesurer()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Mesurer()
        End Sub

        Private Shared Sub OnEtendueChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, MeterGroup).Mesurer()
        End Sub

        Private Sub Jauge_Change(sender As Object, e As EventArgs)
            Mesurer()
        End Sub

        ' La part de chaque jauge, et un trait de sa couleur dans la barre.
        Friend Sub Mesurer()
            Detacher()
            Dim etendue = Math.Max(Double.Epsilon, Maximum - Minimum)
            _barre?.Children.Clear()
            For Each jauge In Jauges
                For Each p In {MeterItem.ValueProperty, MeterItem.ColorProperty, MeterItem.LabelProperty}
                    DependencyPropertyDescriptor.FromProperty(p, GetType(MeterItem)).AddValueChanged(jauge, AddressOf Jauge_Change)
                Next
                _suivis.Add(jauge)
                Dim part = Math.Max(0, Math.Min(100, (jauge.Value - Minimum) * 100 / etendue))
                jauge.Poser(part)
                If _barre IsNot Nothing Then
                    Dim trait As New Border With {.Background = jauge.Color, .Tag = part / 100}
                    _barre.Children.Add(trait)
                End If
            Next
        End Sub

        ' Les jauges ne sont suivies que tant que le groupe est chargé : l'abonnement (DependencyPropertyDescriptor) le
        ' retiendrait en mémoire.
        Private Sub Detacher()
            For Each suivi In _suivis
                For Each p In {MeterItem.ValueProperty, MeterItem.ColorProperty, MeterItem.LabelProperty}
                    DependencyPropertyDescriptor.FromProperty(p, GetType(MeterItem)).RemoveValueChanged(suivi, AddressOf Jauge_Change)
                Next
            Next
            _suivis.Clear()
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c> : sa légende.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

    ''' <summary>Où se pose la légende d'un <see cref="MeterGroup"/>.</summary>
    Public Enum MeterGroupLabelPosition
        ''' <summary>Après la barre (dessous, ou à droite en colonne).</summary>
        [End]
        ''' <summary>Avant la barre.</summary>
        Start
    End Enum

    ''' <summary>La barre d'un <see cref="MeterGroup"/> : chaque trait, sa part (Tag) de la longueur, l'un après l'autre.</summary>
    Friend NotInheritable Class BarreDeJauges
        Inherits System.Windows.Controls.Panel

        Public Shared ReadOnly OrientationProperty As DependencyProperty = StackPanel.OrientationProperty.AddOwner(
            GetType(BarreDeJauges), New FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsArrange))

        Public Shared ReadOnly CornerRadiusProperty As DependencyProperty = Border.CornerRadiusProperty.AddOwner(
            GetType(BarreDeJauges), New FrameworkPropertyMetadata(New CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsArrange))

        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        Public Property CornerRadius As CornerRadius
            Get
                Return DirectCast(GetValue(CornerRadiusProperty), CornerRadius)
            End Get
            Set(value As CornerRadius)
                SetValue(CornerRadiusProperty, value)
            End Set
        End Property

        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            For Each enfant As UIElement In InternalChildren
                enfant.Measure(disponible)
            Next
            Return New Size(0, 0)
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim enLigne = Orientation = Orientation.Horizontal
            Dim longueur = If(enLigne, finale.Width, finale.Height)
            Dim depart = 0.0
            For Each enfant As FrameworkElement In InternalChildren
                Dim part = If(TypeOf enfant.Tag Is Double, CDbl(enfant.Tag), 0)
                Dim taille = part * longueur
                enfant.Arrange(If(enLigne, New Rect(depart, 0, taille, finale.Height), New Rect(0, depart, finale.Width, taille)))
                depart += taille
            Next
            ' Les bouts de la barre, arrondis : les traits y sont découpés.
            Clip = New RectangleGeometry(New Rect(finale), CornerRadius.TopLeft, CornerRadius.TopLeft)
            Return finale
        End Function

    End Class

End Namespace
