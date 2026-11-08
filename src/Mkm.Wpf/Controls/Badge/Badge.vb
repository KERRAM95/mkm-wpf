Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>La gravité d'un <see cref="Badge"/> : sa couleur.</summary>
    Public Enum BadgeSeverity
        ''' <summary>La couleur principale (par défaut).</summary>
        Primary
        ''' <summary>Gris.</summary>
        Secondary
        ''' <summary>Vert.</summary>
        Success
        ''' <summary>Bleu.</summary>
        Info
        ''' <summary>Orange.</summary>
        Warn
        ''' <summary>Rouge.</summary>
        Danger
        ''' <summary>Noir.</summary>
        Contrast
    End Enum

    ''' <summary>La taille d'un <see cref="Badge"/>.</summary>
    Public Enum BadgeSize
        ''' <summary>18 px de haut (par défaut).</summary>
        Normal
        ''' <summary>15 px.</summary>
        Small
        ''' <summary>21 px.</summary>
        Large
        ''' <summary>24 px.</summary>
        XLarge
    End Enum

    ''' <summary>
    ''' La pastille de mkm (Badge) : un nombre ou un mot court, en gras, sur un fond de la couleur de sa gravité, aux
    ''' coins du thème ; ronde pour un seul caractère ; un point de 6 px sans valeur.
    ''' <code>
    ''' &lt;mkm:Badge Value="3" Severity="Danger" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>Posée sur un autre élément (une icône, un bouton), c'est un <see cref="OverlayBadge"/>.</remarks>
    Public Class Badge
        Inherits Control

        ''' <summary>Ce que dit la pastille : un nombre, un mot court ; rien, et c'est un point.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(Object), GetType(Badge), New FrameworkPropertyMetadata(Nothing, AddressOf OnValueChanged))

        ''' <summary>La couleur de la pastille.</summary>
        Public Shared ReadOnly SeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Severity), GetType(BadgeSeverity), GetType(Badge), New FrameworkPropertyMetadata(BadgeSeverity.Primary))

        ''' <summary>La taille de la pastille.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(BadgeSize), GetType(Badge), New FrameworkPropertyMetadata(BadgeSize.Normal))

        Private Shared ReadOnly IsDotPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsDot), GetType(Boolean), GetType(Badge), New FrameworkPropertyMetadata(True))
        ''' <summary>Vrai sans valeur : la pastille est un point.</summary>
        Public Shared ReadOnly IsDotProperty As DependencyProperty = IsDotPropertyKey.DependencyProperty

        Private Shared ReadOnly IsCirclePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsCircle), GetType(Boolean), GetType(Badge), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour une valeur d'un seul caractère : la pastille est ronde.</summary>
        Public Shared ReadOnly IsCircleProperty As DependencyProperty = IsCirclePropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Badge), New FrameworkPropertyMetadata(GetType(Badge)))
            FocusableProperty.OverrideMetadata(GetType(Badge), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Ce que dit la pastille : un nombre, un mot court ; rien, et c'est un point.</summary>
        Public Property Value As Object
            Get
                Return GetValue(ValueProperty)
            End Get
            Set(value As Object)
                SetValue(ValueProperty, value)
            End Set
        End Property

        ''' <summary>La couleur de la pastille.</summary>
        Public Property Severity As BadgeSeverity
            Get
                Return DirectCast(GetValue(SeverityProperty), BadgeSeverity)
            End Get
            Set(value As BadgeSeverity)
                SetValue(SeverityProperty, value)
            End Set
        End Property

        ''' <summary>La taille de la pastille.</summary>
        Public Property Size As BadgeSize
            Get
                Return DirectCast(GetValue(SizeProperty), BadgeSize)
            End Get
            Set(value As BadgeSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai sans valeur : la pastille est un point.</summary>
        Public ReadOnly Property IsDot As Boolean
            Get
                Return CBool(GetValue(IsDotProperty))
            End Get
        End Property

        ''' <summary>Vrai pour une valeur d'un seul caractère : la pastille est ronde.</summary>
        Public ReadOnly Property IsCircle As Boolean
            Get
                Return CBool(GetValue(IsCircleProperty))
            End Get
        End Property

        ''' <summary>Un texte pour les lecteurs d'écran : sa valeur.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Text, Function() Convert.ToString(Value, Globalization.CultureInfo.CurrentCulture))
        End Function

        Private Shared Sub OnValueChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim pastille = DirectCast(d, Badge)
            Dim texte = Convert.ToString(e.NewValue, Globalization.CultureInfo.CurrentCulture)
            pastille.SetValue(IsDotPropertyKey, String.IsNullOrEmpty(texte))
            pastille.SetValue(IsCirclePropertyKey, texte IsNot Nothing AndAlso texte.Length = 1)
        End Sub

    End Class

End Namespace
