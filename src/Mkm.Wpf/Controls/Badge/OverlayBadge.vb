Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une pastille posée sur le coin haut-droit d'un élément (OverlayBadge de mkm) : un compteur sur une icône, sur un
    ''' bouton. La pastille est centrée sur le coin, cernée de 2 px du fond de la page.
    ''' <code>
    ''' &lt;mkm:OverlayBadge Value="4" Severity="Danger"&gt;
    '''     &lt;mkm:Icon Source="{x:Static mkm:Icons.Notification}" Width="24" Height="24" /&gt;
    ''' &lt;/mkm:OverlayBadge&gt;
    ''' </code>
    ''' </summary>
    Public Class OverlayBadge
        Inherits ContentControl

        ''' <summary>Ce que dit la pastille ; rien, et c'est un point.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = Badge.ValueProperty.AddOwner(GetType(OverlayBadge))

        ''' <summary>La couleur de la pastille.</summary>
        Public Shared ReadOnly SeverityProperty As DependencyProperty = Badge.SeverityProperty.AddOwner(GetType(OverlayBadge))

        ''' <summary>La taille de la pastille.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = Badge.SizeProperty.AddOwner(GetType(OverlayBadge))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(OverlayBadge), New FrameworkPropertyMetadata(GetType(OverlayBadge)))
            FocusableProperty.OverrideMetadata(GetType(OverlayBadge), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Ce que dit la pastille ; rien, et c'est un point.</summary>
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

        ''' <summary>Son contenu et sa pastille, pour les lecteurs d'écran.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

    End Class

    ''' <summary>
    ''' Le panneau d'un <see cref="OverlayBadge"/> : son contenu à sa place, la pastille centrée sur le coin haut-droit
    ''' (le <c>translate(50%, -50%)</c> de mkm) ; la pastille déborde sans prendre de place.
    ''' </summary>
    Friend NotInheritable Class CoinDePastille
        Inherits Panel

        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            Dim taille As New Size()
            For i = 0 To InternalChildren.Count - 1
                InternalChildren(i).Measure(If(i = 0, disponible, New Size(Double.PositiveInfinity, Double.PositiveInfinity)))
                If i = 0 Then taille = InternalChildren(i).DesiredSize
            Next
            Return taille
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            For i = 0 To InternalChildren.Count - 1
                Dim enfant = InternalChildren(i)
                If i = 0 Then
                    enfant.Arrange(New Rect(finale))
                Else
                    Dim t = enfant.DesiredSize
                    enfant.Arrange(New Rect(finale.Width - t.Width / 2, -t.Height / 2, t.Width, t.Height))
                End If
            Next
            Return finale
        End Function

    End Class

End Namespace
