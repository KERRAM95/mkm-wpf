Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La zone de texte de plusieurs lignes de mkm (<c>Textarea</c>) : bordée comme le champ, le texte à la ligne,
    ''' Entrée pour une ligne nouvelle ; <see cref="Rows"/> lignes de haut (2 par défaut, comme HTML), une barre de
    ''' défilement au-delà ; <see cref="AutoResize"/>, elle grandit avec son texte.
    ''' <code>&lt;mkm:Textarea Rows="4" Placeholder="Notes" Text="{Binding Notes}" /&gt;</code>
    ''' </summary>
    ''' <remarks>C'est un <see cref="TextBox"/> : annulation, sélection, <c>MaxLength</c>, lecture seule, accessibilité.</remarks>
    Public Class Textarea
        Inherits TextBox

        ''' <summary>Le texte d'aide, montré tant que la zone est vide.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = InputText.PlaceholderProperty.AddOwner(GetType(Textarea))

        ''' <summary>Vrai si la valeur est refusée : la zone se borde de la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = InputText.InvalidProperty.AddOwner(GetType(Textarea))

        ''' <summary>Le nombre de lignes visibles (2 par défaut).</summary>
        Public Shared ReadOnly RowsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Rows), GetType(Integer), GetType(Textarea), New FrameworkPropertyMetadata(2, AddressOf OnLignesChanged),
            Function(v) CInt(v) >= 1)

        ''' <summary>Vrai si la zone grandit avec son texte, au lieu de défiler.</summary>
        Public Shared ReadOnly AutoResizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AutoResize), GetType(Boolean), GetType(Textarea), New FrameworkPropertyMetadata(False, AddressOf OnLignesChanged))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Textarea), New FrameworkPropertyMetadata(GetType(Textarea)))
            AcceptsReturnProperty.OverrideMetadata(GetType(Textarea), New FrameworkPropertyMetadata(True))
            TextWrappingProperty.OverrideMetadata(GetType(Textarea), New FrameworkPropertyMetadata(TextWrapping.Wrap))
        End Sub

        ''' <summary>Une zone de deux lignes.</summary>
        Public Sub New()
            Dimensionner()
        End Sub

        ''' <summary>Le texte d'aide, montré tant que la zone est vide.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si la valeur est refusée.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre de lignes visibles (2 par défaut).</summary>
        Public Property Rows As Integer
            Get
                Return CInt(GetValue(RowsProperty))
            End Get
            Set(value As Integer)
                SetValue(RowsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si la zone grandit avec son texte, au lieu de défiler.</summary>
        Public Property AutoResize As Boolean
            Get
                Return CBool(GetValue(AutoResizeProperty))
            End Get
            Set(value As Boolean)
                SetValue(AutoResizeProperty, value)
            End Set
        End Property

        Private Shared Sub OnLignesChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Textarea).Dimensionner()
        End Sub

        ' Rows lignes au moins ; autant au plus, sauf si elle grandit avec son texte.
        Private Sub Dimensionner()
            MinLines = Rows
            MaxLines = If(AutoResize, Integer.MaxValue, Rows)
        End Sub

    End Class

End Namespace
