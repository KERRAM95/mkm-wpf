Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>La forme d'un <see cref="Skeleton"/>.</summary>
    Public Enum SkeletonShape
        ''' <summary>Un rectangle aux coins arrondis du thème (par défaut) : une ligne de texte, un champ, une image.</summary>
        Rectangle
        ''' <summary>Un rond : un avatar, une icône.</summary>
        Circle
    End Enum

    ''' <summary>L'animation d'un <see cref="Skeleton"/>.</summary>
    Public Enum SkeletonAnimation
        ''' <summary>Une vague claire qui passe de gauche à droite en 1,2 s (par défaut).</summary>
        Wave
        ''' <summary>Aucune.</summary>
        None
    End Enum

    ''' <summary>
    ''' La silhouette de mkm (Skeleton) : un bloc gris pâle à la place d'un contenu qui charge, qu'une vague claire
    ''' traverse. Plusieurs silhouettes dessinent la forme de ce qui va venir (des lignes, un avatar, un tableau).
    ''' <code>
    ''' &lt;mkm:Skeleton Width="200" /&gt;
    ''' &lt;mkm:Skeleton Shape="Circle" Width="40" Height="40" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Un dessin, que les lecteurs d'écran ignorent : c'est le conteneur qui dit qu'il charge (le BlockUI, l'état d'un
    ''' DataTable).
    ''' </remarks>
    Public Class Skeleton
        Inherits Control

        ''' <summary>La forme : un rectangle ou un rond.</summary>
        Public Shared ReadOnly ShapeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Shape), GetType(SkeletonShape), GetType(Skeleton), New FrameworkPropertyMetadata(SkeletonShape.Rectangle))

        ''' <summary>L'animation : la vague, ou aucune.</summary>
        Public Shared ReadOnly AnimationProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Animation), GetType(SkeletonAnimation), GetType(Skeleton), New FrameworkPropertyMetadata(SkeletonAnimation.Wave))

        ''' <summary>Les coins d'un rectangle (ceux du thème par défaut).</summary>
        Public Shared ReadOnly CornerRadiusProperty As DependencyProperty = Border.CornerRadiusProperty.AddOwner(GetType(Skeleton))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Skeleton), New FrameworkPropertyMetadata(GetType(Skeleton)))
            FocusableProperty.OverrideMetadata(GetType(Skeleton), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>La forme : un rectangle ou un rond.</summary>
        Public Property Shape As SkeletonShape
            Get
                Return DirectCast(GetValue(ShapeProperty), SkeletonShape)
            End Get
            Set(value As SkeletonShape)
                SetValue(ShapeProperty, value)
            End Set
        End Property

        ''' <summary>L'animation : la vague, ou aucune.</summary>
        Public Property Animation As SkeletonAnimation
            Get
                Return DirectCast(GetValue(AnimationProperty), SkeletonAnimation)
            End Get
            Set(value As SkeletonAnimation)
                SetValue(AnimationProperty, value)
            End Set
        End Property

        ''' <summary>Les coins d'un rectangle (ceux du thème par défaut).</summary>
        Public Property CornerRadius As CornerRadius
            Get
                Return DirectCast(GetValue(CornerRadiusProperty), CornerRadius)
            End Get
            Set(value As CornerRadius)
                SetValue(CornerRadiusProperty, value)
            End Set
        End Property

    End Class

End Namespace
