Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une commande d'<see cref="ActionBar"/> : une icône, un libellé, et ce que fait tout bouton de WPF
    ''' (<c>Click</c>, <c>Command</c> qui la grise quand il ne peut pas s'exécuter).
    ''' <code>&lt;mkm:ActionBarButton Label="Enregistrer" Icon="{x:Static mkm:Icons.Download}" Command="{Binding Enregistrer}" /&gt;</code>
    ''' </summary>
    Public Class ActionBarButton
        Inherits Button
        Implements IActionBarCommand

        ''' <summary>Le libellé de la commande.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = ActionBarItem.LabelProperty.AddOwner(GetType(ActionBarButton))

        ''' <summary>L'icône de la commande.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = ActionBarItem.IconProperty.AddOwner(GetType(ActionBarButton))

        ''' <summary>Le texte que l'info-bulle ajoute au libellé.</summary>
        Public Shared ReadOnly HintProperty As DependencyProperty = ActionBarItem.HintProperty.AddOwner(GetType(ActionBarButton))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ActionBarButton), New FrameworkPropertyMetadata(GetType(ActionBarButton)))
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
