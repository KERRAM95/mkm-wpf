Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La comparaison d'images de mkm (ImageCompare) : deux images l'une sur l'autre, une poignée qui glisse pour montrer
    ''' l'une à gauche et l'autre à droite (avant et après une révision). Au clavier, les flèches la déplacent.
    ''' <code>
    ''' &lt;mkm:ImageCompare LeftImage="/Images/revision-a.png" RightImage="/Images/revision-b.png" Width="480" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Left", Type:=GetType(FrameworkElement))>
    Public Class ImageCompare
        Inherits Control

        ''' <summary>L'image montrée à gauche de la poignée.</summary>
        Public Shared ReadOnly LeftImageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(LeftImage), GetType(ImageSource), GetType(ImageCompare), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'image montrée à droite de la poignée.</summary>
        Public Shared ReadOnly RightImageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(RightImage), GetType(ImageSource), GetType(ImageCompare), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La place de la poignée, de 0 (tout à gauche) à 100 : 50 par défaut. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(Double), GetType(ImageCompare),
            New FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnValueChanged, AddressOf Borner))

        Private _gauche As FrameworkElement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ImageCompare), New FrameworkPropertyMetadata(GetType(ImageCompare)))
            FocusableProperty.OverrideMetadata(GetType(ImageCompare), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>L'image de gauche.</summary>
        Public Property LeftImage As ImageSource
            Get
                Return DirectCast(GetValue(LeftImageProperty), ImageSource)
            End Get
            Set(value As ImageSource)
                SetValue(LeftImageProperty, value)
            End Set
        End Property

        ''' <summary>L'image de droite.</summary>
        Public Property RightImage As ImageSource
            Get
                Return DirectCast(GetValue(RightImageProperty), ImageSource)
            End Get
            Set(value As ImageSource)
                SetValue(RightImageProperty, value)
            End Set
        End Property

        ''' <summary>La place de la poignée, de 0 à 100.</summary>
        Public Property Value As Double
            Get
                Return CDbl(GetValue(ValueProperty))
            End Get
            Set(value As Double)
                SetValue(ValueProperty, value)
            End Set
        End Property

        Private Shared Function Borner(d As DependencyObject, valeur As Object) As Object
            Return Math.Max(0, Math.Min(100, CDbl(valeur)))
        End Function

        Private Shared Sub OnValueChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, ImageCompare).Decouper()
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _gauche = TryCast(GetTemplateChild("PART_Left"), FrameworkElement)
            Decouper()
        End Sub

        ''' <summary>Sans hauteur donnée, en 16/9 de sa largeur, comme mkm.</summary>
        Protected Overrides Function MeasureOverride(contrainte As Size) As Size
            Dim taille = MyBase.MeasureOverride(contrainte)
            If Double.IsNaN(Height) AndAlso Not Double.IsInfinity(contrainte.Width) Then
                Return New Size(contrainte.Width, Math.Min(contrainte.Height, contrainte.Width * 9 / 16))
            End If
            Return taille
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim taille = MyBase.ArrangeOverride(finale)
            Decouper(finale.Width, finale.Height)
            Return taille
        End Function

        ' L'image de gauche découpée à la poignée (clip-path chez mkm).
        Private Sub Decouper(Optional largeur As Double = -1, Optional hauteur As Double = -1)
            If _gauche Is Nothing Then Return
            If largeur < 0 Then largeur = ActualWidth
            If hauteur < 0 Then hauteur = ActualHeight
            _gauche.Clip = New RectangleGeometry(New Rect(0, 0, largeur * Value / 100, hauteur))
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c> : sa poignée (un curseur).</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

End Namespace
