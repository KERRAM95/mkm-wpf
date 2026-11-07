Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une commande à bascule d'<see cref="ActionBar"/> : enfoncée ou non (<c>IsChecked</c>), elle se
    ''' montre pleine de la couleur principale quand elle l'est.
    ''' </summary>
    Public Class ActionBarToggleButton
        Inherits ToggleButton
        Implements IActionBarCommand

        ''' <summary>Le libellé de la commande.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = ActionBarItem.LabelProperty.AddOwner(GetType(ActionBarToggleButton))

        ''' <summary>L'icône de la commande.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = ActionBarItem.IconProperty.AddOwner(GetType(ActionBarToggleButton))

        ''' <summary>Le texte que l'info-bulle ajoute au libellé.</summary>
        Public Shared ReadOnly HintProperty As DependencyProperty = ActionBarItem.HintProperty.AddOwner(GetType(ActionBarToggleButton))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ActionBarToggleButton), New FrameworkPropertyMetadata(GetType(ActionBarToggleButton)))
        End Sub

        ''' <summary>Le libellé de la commande.</summary>
        Public Property Label As String
            Get
                Return CStr(GetValue(LabelProperty))
            End Get
            Set(value As String)
                SetValue(LabelProperty, value)
            End Set
        End Property

        ''' <summary>L'icône de la commande.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Le texte que l'info-bulle ajoute au libellé.</summary>
        Public Property Hint As String
            Get
                Return CStr(GetValue(HintProperty))
            End Get
            Set(value As String)
                SetValue(HintProperty, value)
            End Set
        End Property

        Friend Sub PerformClick() Implements IActionBarCommand.PerformClick
            OnClick()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnIsPressedChanged(e As DependencyPropertyChangedEventArgs)
            MyBase.OnIsPressedChanged(e)
            ActionBarItem.Presser(Me, IsPressed)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnGotKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnGotKeyboardFocus(e)
            ActionBarItem.OuvrirAuClavier(Me)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnLostKeyboardFocus(e)
            ActionBarItem.Fermer(Me)
        End Sub

    End Class

End Namespace
