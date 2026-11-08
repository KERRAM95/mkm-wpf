Imports System.Windows
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La case à cocher de mkm, au rendu de 3DEXPERIENCE : 14 px, coins de 2 px, gris clair bordée de
    ''' gris ; cochée, la couleur principale et une coche de 2 px qui la remplit ; survolée ou au clavier
    ''' elle fonce d'un pas, enfoncée d'un pas de plus ; grisée, elle garde ses couleurs, à moitié
    ''' transparente. Son libellé (<c>Content</c>) fait partie d'elle : il l'allume au survol.
    ''' <code>&lt;mkm:Checkbox Content="Affecter toutes les planches" IsChecked="{Binding ToutesLesPlanches}" /&gt;</code>
    ''' </summary>
    ''' <remarks>
    ''' C'est un <see cref="Controls.CheckBox"/> : trois états (<c>IsThreeState</c>), clavier et
    ''' accessibilité de WPF. En VB, « Checkbox » et « CheckBox » sont un même nom : qualifier
    ''' (<c>Mkm.Wpf.Controls.Checkbox</c>) là où les deux espaces de noms sont importés.
    ''' </remarks>
    Public Class Checkbox
        Inherits System.Windows.Controls.CheckBox

        ''' <summary>Vrai quand la valeur est fausse : la case prend la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType(Checkbox), New FrameworkPropertyMetadata(False))

        Private Shared ReadOnly IsFocusVisiblePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFocusVisible), GetType(Boolean), GetType(Checkbox), New FrameworkPropertyMetadata(False))
        ''' <summary>
        ''' Vrai quand la case a le focus du clavier, et non celui d'un clic (le <c>:focus-visible</c> de mkm) : cliquée à
        ''' la souris, elle ne garde pas l'allure survolée une fois la souris partie.
        ''' </summary>
        Public Shared ReadOnly IsFocusVisibleProperty As DependencyProperty = IsFocusVisiblePropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Checkbox), New FrameworkPropertyMetadata(GetType(Checkbox)))
        End Sub

        ''' <summary>Vrai quand la case a le focus du clavier, et non celui d'un clic.</summary>
        Public ReadOnly Property IsFocusVisible As Boolean
            Get
                Return CBool(GetValue(IsFocusVisibleProperty))
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Sub OnGotKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnGotKeyboardFocus(e)
            SetValue(IsFocusVisiblePropertyKey, InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnLostKeyboardFocus(e)
            SetValue(IsFocusVisiblePropertyKey, False)
        End Sub

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
