Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La carte de mkm (Card) : un bloc blanc ombré, sans bordure, aux coins du thème ; en haut une image
    ''' (<c>Header</c>), puis son titre, son sous-titre, son contenu et son pied, à 15 px du bord.
    ''' <code>
    ''' &lt;mkm:Card Title="Plan HS-00042" Subtitle="Révision B"&gt;
    '''     &lt;TextBlock Text="Carter de transmission." /&gt;
    ''' &lt;/mkm:Card&gt;
    ''' </code>
    ''' </summary>
    Public Class Card
        Inherits ContentControl

        ''' <summary>Le haut de la carte, sans retrait : une image, un aperçu.</summary>
        Public Shared ReadOnly HeaderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Header), GetType(Object), GetType(Card), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le titre de la carte.</summary>
        Public Shared ReadOnly TitleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Title), GetType(Object), GetType(Card), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le sous-titre, sous le titre, en gris.</summary>
        Public Shared ReadOnly SubtitleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Subtitle), GetType(Object), GetType(Card), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le pied de la carte : des boutons.</summary>
        Public Shared ReadOnly FooterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Footer), GetType(Object), GetType(Card), New FrameworkPropertyMetadata(Nothing))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Card), New FrameworkPropertyMetadata(GetType(Card)))
            FocusableProperty.OverrideMetadata(GetType(Card), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Le haut de la carte, sans retrait.</summary>
        Public Property Header As Object
            Get
                Return GetValue(HeaderProperty)
            End Get
            Set(value As Object)
                SetValue(HeaderProperty, value)
            End Set
        End Property

        ''' <summary>Le titre de la carte.</summary>
        Public Property Title As Object
            Get
                Return GetValue(TitleProperty)
            End Get
            Set(value As Object)
                SetValue(TitleProperty, value)
            End Set
        End Property

        ''' <summary>Le sous-titre, sous le titre.</summary>
        Public Property Subtitle As Object
            Get
                Return GetValue(SubtitleProperty)
            End Get
            Set(value As Object)
                SetValue(SubtitleProperty, value)
            End Set
        End Property

        ''' <summary>Le pied de la carte.</summary>
        Public Property Footer As Object
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As Object)
                SetValue(FooterProperty, value)
            End Set
        End Property

        ''' <summary>Un groupe nommé par son titre pour les lecteurs d'écran.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group, Function() TryCast(Title, String))
        End Function

    End Class

End Namespace
