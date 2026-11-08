Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>Le sens d'un <see cref="Divider"/>.</summary>
    Public Enum DividerLayout
        ''' <summary>Un trait horizontal, entre deux blocs l'un sous l'autre (par défaut).</summary>
        Horizontal
        ''' <summary>Un trait vertical, entre deux éléments côte à côte.</summary>
        Vertical
    End Enum

    ''' <summary>Le trait d'un <see cref="Divider"/>.</summary>
    Public Enum DividerType
        ''' <summary>Plein (par défaut).</summary>
        Solid
        ''' <summary>En tirets.</summary>
        Dashed
        ''' <summary>En pointillés.</summary>
        Dotted
    End Enum

    ''' <summary>La place du contenu d'un <see cref="Divider"/> sur son trait.</summary>
    Public Enum DividerAlign
        ''' <summary>Celle de mkm : à gauche d'un trait horizontal, au milieu d'un trait vertical (par défaut).</summary>
        [Default]
        ''' <summary>À gauche, sur un trait horizontal.</summary>
        Left
        ''' <summary>Au milieu.</summary>
        Center
        ''' <summary>À droite, sur un trait horizontal.</summary>
        Right
        ''' <summary>En haut, sur un trait vertical.</summary>
        Top
        ''' <summary>En bas, sur un trait vertical.</summary>
        Bottom
    End Enum

    ''' <summary>
    ''' Le séparateur de mkm (Divider) : un trait d'un pixel, de la couleur des bordures, entre deux blocs, avec 12 px
    ''' de marge ; un texte ou une icône peut s'y poser (<c>Content</c>), sur le fond de la page.
    ''' <code>
    ''' &lt;mkm:Divider /&gt;
    ''' &lt;mkm:Divider Content="Cotes" Align="Center" /&gt;
    ''' &lt;mkm:Divider Layout="Vertical" /&gt;
    ''' </code>
    ''' </summary>
    Public Class Divider
        Inherits ContentControl

        ''' <summary>Le sens du trait.</summary>
        Public Shared ReadOnly LayoutProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Layout), GetType(DividerLayout), GetType(Divider), New FrameworkPropertyMetadata(DividerLayout.Horizontal, AddressOf OnPlaceChanged))

        ''' <summary>Le trait : plein, en tirets ou en pointillés.</summary>
        Public Shared ReadOnly TypeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([Type]), GetType(DividerType), GetType(Divider), New FrameworkPropertyMetadata(DividerType.Solid))

        ''' <summary>La place du contenu sur le trait.</summary>
        Public Shared ReadOnly AlignProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Align), GetType(DividerAlign), GetType(Divider), New FrameworkPropertyMetadata(DividerAlign.Default, AddressOf OnPlaceChanged))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Divider), New FrameworkPropertyMetadata(GetType(Divider)))
            FocusableProperty.OverrideMetadata(GetType(Divider), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un trait horizontal plein, sans contenu.</summary>
        Public Sub New()
            Placer()
        End Sub

        ''' <summary>Le sens du trait.</summary>
        Public Property Layout As DividerLayout
            Get
                Return DirectCast(GetValue(LayoutProperty), DividerLayout)
            End Get
            Set(value As DividerLayout)
                SetValue(LayoutProperty, value)
            End Set
        End Property

        ''' <summary>Le trait : plein, en tirets ou en pointillés.</summary>
        Public Property [Type] As DividerType
            Get
                Return DirectCast(GetValue(TypeProperty), DividerType)
            End Get
            Set(value As DividerType)
                SetValue(TypeProperty, value)
            End Set
        End Property

        ''' <summary>La place du contenu sur le trait.</summary>
        Public Property Align As DividerAlign
            Get
                Return DirectCast(GetValue(AlignProperty), DividerAlign)
            End Get
            Set(value As DividerAlign)
                SetValue(AlignProperty, value)
            End Set
        End Property

        ''' <summary>Un séparateur pour les lecteurs d'écran, nommé par son contenu s'il en a un.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Separator, Function() TryCast(Content, String))
        End Function

        Private Shared Sub OnPlaceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Divider).Placer()
        End Sub

        ' Le contenu sur le trait : HorizontalContentAlignment et VerticalContentAlignment, que le gabarit suit.
        Private Sub Placer()
            Dim horizontal = Layout = DividerLayout.Horizontal
            Select Case Align
                Case DividerAlign.Center
                    SetCurrentValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Center)
                    SetCurrentValue(VerticalContentAlignmentProperty, VerticalAlignment.Center)
                Case DividerAlign.Right
                    SetCurrentValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Right)
                Case DividerAlign.Left
                    SetCurrentValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Left)
                Case DividerAlign.Top
                    SetCurrentValue(VerticalContentAlignmentProperty, VerticalAlignment.Top)
                Case DividerAlign.Bottom
                    SetCurrentValue(VerticalContentAlignmentProperty, VerticalAlignment.Bottom)
                Case Else
                    SetCurrentValue(HorizontalContentAlignmentProperty, If(horizontal, HorizontalAlignment.Left, HorizontalAlignment.Center))
                    SetCurrentValue(VerticalContentAlignmentProperty, VerticalAlignment.Center)
            End Select
        End Sub

    End Class

End Namespace
