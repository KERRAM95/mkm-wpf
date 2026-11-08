Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une section d'une <see cref="PropertyPanel"/> : son titre (<c>Header</c>) sur une ligne de 32 px, sa flèche à
    ''' gauche ; ouverte (<see cref="IsExpanded"/>), ses propriétés (<see cref="PropertyRow"/>) dessous.
    ''' </summary>
    Public Class PropertySection
        Inherits HeaderedItemsControl

        ''' <summary>Tout son arbre visuel pour les lecteurs d'écran et l'automatisation, gabarit compris.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

        ''' <summary>Vrai quand la section montre ses propriétés.</summary>
        Public Shared ReadOnly IsExpandedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsExpanded), GetType(Boolean), GetType(PropertySection),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(PropertySection), New FrameworkPropertyMetadata(GetType(PropertySection)))
            FocusableProperty.OverrideMetadata(GetType(PropertySection), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand la section montre ses propriétés.</summary>
        Public Property IsExpanded As Boolean
            Get
                Return CBool(GetValue(IsExpandedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsExpandedProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is PropertyRow
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New PropertyRow()
        End Function

    End Class

End Namespace
