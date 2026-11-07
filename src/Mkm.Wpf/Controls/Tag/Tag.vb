Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Controls

    ''' <summary>La gravité d'une <see cref="Tag"/>.</summary>
    Public Enum TagSeverity
        ''' <summary>La couleur principale (par défaut).</summary>
        Primary
        ''' <summary>Un gris clair.</summary>
        Secondary
        ''' <summary>Une réussite.</summary>
        Success
        ''' <summary>Une information.</summary>
        Info
        ''' <summary>Un avertissement.</summary>
        Warn
        ''' <summary>Un danger.</summary>
        Danger
        ''' <summary>Un gris foncé.</summary>
        Contrast
    End Enum

    ''' <summary>
    ''' L'étiquette de mkm, celle de la grille de 3DEXPERIENCE : 18 px de haut, à angles droits, un texte de 12 px en
    ''' graisse normale, blanc sur la couleur de sa gravité assombrie (lisible), une icône de 12 px s'il le faut.
    ''' <code>&lt;mkm:Tag Severity="Success" Content="À jour" /&gt;</code>
    ''' </summary>
    ''' <remarks>
    ''' Une couleur qui vient d'ailleurs (une maturité, la politique de l'application) se pose par <see cref="Color"/> :
    ''' le texte prend alors le noir ou le blanc, le plus lisible dessus. La bibliothèque n'a pas de couleurs d'état.
    ''' </remarks>
    Public Class Tag
        Inherits ContentControl

        ''' <summary>La gravité de l'étiquette.</summary>
        Public Shared ReadOnly SeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Severity), GetType(TagSeverity), GetType(Tag), New FrameworkPropertyMetadata(TagSeverity.Primary, AddressOf OnSeverityChanged))

        ''' <summary>L'icône devant le texte (aucune par défaut).</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(Tag), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>
        ''' Une couleur venue d'ailleurs (la politique de l'application : une maturité…), qui l'emporte sur la gravité ;
        ''' le texte prend le noir ou le blanc, le plus lisible dessus (le <c>maturity.color</c> de mkm).
        ''' </summary>
        Public Shared ReadOnly ColorProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Color), GetType(Media.Brush), GetType(Tag), New FrameworkPropertyMetadata(Nothing, AddressOf OnColorChanged))

        ''' <summary>Vrai si l'étiquette a des bouts ronds.</summary>
        Public Shared ReadOnly RoundedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Rounded), GetType(Boolean), GetType(Tag), New FrameworkPropertyMetadata(False))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Tag), New FrameworkPropertyMetadata(GetType(Tag)))
            FocusableProperty.OverrideMetadata(GetType(Tag), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une étiquette principale.</summary>
        Public Sub New()
            Colorer()
        End Sub

        ''' <summary>La gravité de l'étiquette.</summary>
        Public Property Severity As TagSeverity
            Get
                Return DirectCast(GetValue(SeverityProperty), TagSeverity)
            End Get
            Set(value As TagSeverity)
                SetValue(SeverityProperty, value)
            End Set
        End Property

        ''' <summary>L'icône devant le texte (aucune par défaut).</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Une couleur venue d'ailleurs, qui l'emporte sur la gravité ; le texte le plus lisible dessus.</summary>
        Public Property Color As Media.Brush
            Get
                Return DirectCast(GetValue(ColorProperty), Media.Brush)
            End Get
            Set(value As Media.Brush)
                SetValue(ColorProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si l'étiquette a des bouts ronds.</summary>
        Public Property Rounded As Boolean
            Get
                Return CBool(GetValue(RoundedProperty))
            End Get
            Set(value As Boolean)
                SetValue(RoundedProperty, value)
            End Set
        End Property

        ''' <summary>
        ''' Le noir ou le blanc, le plus lisible sur <paramref name="fond"/> (le <c>textOnColor</c> de mkm) ; rien si le
        ''' fond n'est pas une couleur unie.
        ''' </summary>
        Public Shared Function ReadableForeground(fond As Media.Brush) As Media.Brush
            Dim uni = TryCast(fond, Media.SolidColorBrush)
            If uni Is Nothing Then Return Nothing
            Dim c = uni.Color
            Dim l = Luminance(c)
            ' Le contraste du noir (l + 0,05) / 0,05 contre celui du blanc 1,05 / (l + 0,05).
            Return If((l + 0.05) / 0.05 >= 1.05 / (l + 0.05), Media.Brushes.Black, Media.Brushes.White)
        End Function

        Private Shared Function Luminance(c As Media.Color) As Double
            Dim canal = Function(v As Byte)
                            Dim x = v / 255.0
                            Return If(x <= 0.03928, x / 12.92, Math.Pow((x + 0.055) / 1.055, 2.4))
                        End Function
            Return 0.2126 * canal(c.R) + 0.7152 * canal(c.G) + 0.0722 * canal(c.B)
        End Function

        Private Shared Sub OnSeverityChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Tag).Colorer()
        End Sub

        Private Shared Sub OnColorChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Tag).Colorer()
        End Sub

        ' Les couleurs : celle qu'on donne (Color) et le texte le plus lisible dessus, sinon celles de la gravité.
        Private Sub Colorer()
            If Color IsNot Nothing Then
                SetValue(BackgroundProperty, Color)
                SetValue(ForegroundProperty, If(ReadableForeground(Color), CType(Media.Brushes.White, Media.Brush)))
                Return
            End If
            Dim g = Severity.ToString().ToLowerInvariant()
            SetResourceReference(BackgroundProperty, ThemeResources.Prefix & $"tag.{g}.background")
            SetResourceReference(ForegroundProperty, ThemeResources.Prefix & $"tag.{g}.color")
        End Sub

    End Class

End Namespace
