Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>La place d'un bouton dans un <see cref="ButtonGroup"/> : ses coins et sa bordure en dépendent.</summary>
    Public Enum ButtonGroupPosition
        ''' <summary>Hors d'un groupe, ou seul dedans.</summary>
        None
        ''' <summary>Le premier : ses coins à gauche.</summary>
        First
        ''' <summary>Entre deux : sans coins.</summary>
        Middle
        ''' <summary>Le dernier : ses coins à droite.</summary>
        Last
    End Enum

    ''' <summary>
    ''' Le groupe de boutons de mkm (<c>ButtonGroup</c>) : des boutons collés, une seule forme ; seuls le premier et le
    ''' dernier gardent leurs coins.
    ''' <code>
    ''' &lt;mkm:ButtonGroup&gt;
    '''     &lt;mkm:Button Content="Enregistrer" /&gt;
    '''     &lt;mkm:Button Content="Annuler" /&gt;
    ''' &lt;/mkm:ButtonGroup&gt;
    ''' </code>
    ''' </summary>
    Public Class ButtonGroup
        Inherits ItemsControl

        ''' <summary>La place d'un bouton dans son groupe, posée par le groupe.</summary>
        Public Shared ReadOnly PositionProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Position", GetType(ButtonGroupPosition), GetType(ButtonGroup), New FrameworkPropertyMetadata(ButtonGroupPosition.None))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ButtonGroup), New FrameworkPropertyMetadata(GetType(ButtonGroup)))
            FocusableProperty.OverrideMetadata(GetType(ButtonGroup), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>La place d'un bouton dans son groupe.</summary>
        Public Shared Function GetPosition(element As DependencyObject) As ButtonGroupPosition
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return DirectCast(element.GetValue(PositionProperty), ButtonGroupPosition)
        End Function

        ''' <summary>Pose la place d'un bouton (le groupe le fait).</summary>
        Public Shared Sub SetPosition(element As DependencyObject, value As ButtonGroupPosition)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(PositionProperty, value)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Ranger()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Ranger()
        End Sub

        ' Chaque bouton visible prend sa place : le premier, ceux du milieu, le dernier.
        Friend Sub Ranger()
            Dim boutons = Items.OfType(Of UIElement)().Where(Function(b) b.Visibility <> Visibility.Collapsed).ToList()
            For Each element In Items.OfType(Of UIElement)()
                element.ClearValue(PositionProperty)
            Next
            If boutons.Count < 2 Then Return
            For i = 0 To boutons.Count - 1
                SetPosition(boutons(i), If(i = 0, ButtonGroupPosition.First, If(i = boutons.Count - 1, ButtonGroupPosition.Last, ButtonGroupPosition.Middle)))
            Next
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c>.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

End Namespace
