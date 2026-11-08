Imports System.Windows
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' L'interrupteur de mkm (ToggleSwitch) : une piste de 30 × 18 px aux bouts ronds, sa poignée ronde de 12 px à
    ''' gauche ; activé, la piste prend la couleur principale et la poignée, blanche, passe à droite. Pour un réglage qui
    ''' agit tout de suite (oui ou non), là où une case à cocher attend une validation.
    ''' <code>
    ''' &lt;mkm:ToggleSwitch IsChecked="{Binding SynchroAutomatique}" Content="Synchroniser avec 3DEXPERIENCE" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' C'est un <see cref="System.Windows.Controls.Primitives.ToggleButton"/> : <c>IsChecked</c>, Espace, et pour les
    ''' lecteurs d'écran un bouton bascule, activé ou non. Écart avec mkm : son libellé (<c>Content</c>) fait partie de
    ''' lui, à droite, comme la case à cocher ; mkm le pose à part.
    ''' </remarks>
    Public Class ToggleSwitch
        Inherits System.Windows.Controls.Primitives.ToggleButton

        ''' <summary>Vrai quand le réglage est refusé : la piste prend la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = InputText.InvalidProperty.AddOwner(GetType(ToggleSwitch))

        Private Shared ReadOnly IsFocusVisiblePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFocusVisible), GetType(Boolean), GetType(ToggleSwitch), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand l'interrupteur a le focus du clavier, et non celui d'un clic : l'anneau du focus paraît.</summary>
        Public Shared ReadOnly IsFocusVisibleProperty As DependencyProperty = IsFocusVisiblePropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ToggleSwitch), New FrameworkPropertyMetadata(GetType(ToggleSwitch)))
        End Sub

        ''' <summary>Vrai quand le réglage est refusé.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand l'interrupteur a le focus du clavier, et non celui d'un clic.</summary>
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

    End Class

End Namespace
