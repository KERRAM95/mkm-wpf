Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le champ d'une ligne de mkm (<c>InputText</c>), au rendu de 3DEXPERIENCE : 24 px de haut, bordé
    ''' de gris, plus foncé au survol, la bordure au bleu du thème au focus, sans anneau. Un texte d'aide
    ''' quand il est vide (<see cref="Placeholder"/>) ; <see cref="Invalid"/> le borde de la couleur d'erreur.
    ''' </summary>
    ''' <remarks>
    ''' C'est un <see cref="TextBox"/> : <c>MaxLength</c>, <c>CharacterCasing</c> (majuscules),
    ''' <c>IsReadOnly</c>, le clavier, l'annulation et l'accessibilité sont ceux de WPF.
    ''' </remarks>
    Public Class InputText
        Inherits TextBox

        ''' <summary>Le texte d'aide, montré quand le champ est vide.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Placeholder), GetType(String), GetType(InputText), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai quand la valeur est fausse : le champ prend la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType(InputText), New FrameworkPropertyMetadata(False))

        ''' <summary>Les coins du champ (ceux du thème par défaut) ; un champ accolé à des boutons les perd.</summary>
        Public Shared ReadOnly CornerRadiusProperty As DependencyProperty = Border.CornerRadiusProperty.AddOwner(GetType(InputText))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(InputText), New FrameworkPropertyMetadata(GetType(InputText)))
        End Sub

        ''' <summary>Les coins du champ (ceux du thème par défaut).</summary>
        Public Property CornerRadius As CornerRadius
            Get
                Return DirectCast(GetValue(CornerRadiusProperty), CornerRadius)
            End Get
            Set(value As CornerRadius)
                SetValue(CornerRadiusProperty, value)
            End Set
        End Property

        ''' <summary>Le texte d'aide, montré tant que le champ est vide.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la valeur est fausse.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

    End Class

End Namespace
