Imports System.Windows
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le <c>:focus-visible</c> de mkm pour tout élément, contrôles de WPF compris : <see cref="IsVisibleProperty"/> est
    ''' vrai sur l'élément qui a le focus quand le clavier le lui a donné, faux quand c'est un clic. Un gabarit montre son
    ''' anneau du focus sur lui (<c>Trigger Property="local:FocusVisible.IsVisible"</c>), et non sur
    ''' <c>IsKeyboardFocused</c>, que WPF pose aussi au clic.
    ''' </summary>
    Public NotInheritable Class FocusVisible

        ''' <summary>Vrai sur l'élément qui a le focus du clavier, quand le clavier le lui a donné.</summary>
        Public Shared ReadOnly IsVisibleProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "IsVisible", GetType(Boolean), GetType(FocusVisible), New FrameworkPropertyMetadata(False))

        Private Shared _enregistre As Boolean

        Private Sub New()
        End Sub

        ''' <summary>Vrai si l'élément a le focus du clavier, donné par le clavier.</summary>
        Public Shared Function GetIsVisible(element As DependencyObject) As Boolean
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return CBool(element.GetValue(IsVisibleProperty))
        End Function

        ''' <summary>Pose l'état : réservé à la bibliothèque, qui suit le focus elle-même.</summary>
        Public Shared Sub SetIsVisible(element As DependencyObject, value As Boolean)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(IsVisibleProperty, value)
        End Sub

        ' Une fois par application (MkmResources) : chaque élément qui prend ou perd le focus du clavier.
        Friend Shared Sub Enregistrer()
            If _enregistre Then Return
            _enregistre = True
            EventManager.RegisterClassHandler(GetType(UIElement), Keyboard.GotKeyboardFocusEvent, New KeyboardFocusChangedEventHandler(AddressOf Pris), True)
            EventManager.RegisterClassHandler(GetType(UIElement), Keyboard.LostKeyboardFocusEvent, New KeyboardFocusChangedEventHandler(AddressOf Perdu), True)
        End Sub

        Private Shared Sub Pris(sender As Object, e As KeyboardFocusChangedEventArgs)
            If e.NewFocus IsNot sender Then Return
            DirectCast(sender, UIElement).SetValue(IsVisibleProperty, InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice)
        End Sub

        Private Shared Sub Perdu(sender As Object, e As KeyboardFocusChangedEventArgs)
            If e.OldFocus IsNot sender Then Return
            DirectCast(sender, UIElement).ClearValue(IsVisibleProperty)
        End Sub

    End Class

End Namespace
