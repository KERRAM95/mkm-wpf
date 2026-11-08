Imports System
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le bouton qui replie et déplie un panneau (Panel, IdCard) : un bouton de mkm qui se dit « développé » ou « réduit »
    ''' aux lecteurs d'écran (le <c>aria-expanded</c> de mkm). Son état vient du panneau (<see cref="IsCollapsed"/>, lié
    ''' dans le gabarit) ; son clic, que le panneau écoute, le change.
    ''' </summary>
    Friend NotInheritable Class BoutonDeRepli
        Inherits Button

        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsCollapsed), GetType(Boolean), GetType(BoutonDeRepli), New FrameworkPropertyMetadata(False, AddressOf OnIsCollapsedChanged))

        ''' <summary>Vrai quand le panneau est replié.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeRepli(Me, Function() IsCollapsed)
        End Function

        Private Shared Sub OnIsCollapsedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            PairDeRepli.Annoncer(DirectCast(d, UIElement), CBool(e.OldValue), CBool(e.NewValue))
        End Sub

    End Class

    ''' <summary>
    ''' La languette du Triptych : un bouton de WPF (son style est celui de la languette, pas celui des boutons de mkm) qui
    ''' se dit « développé » ou « réduit » comme <see cref="BoutonDeRepli"/>.
    ''' </summary>
    Friend NotInheritable Class LanguetteDeRepli
        Inherits System.Windows.Controls.Button

        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsCollapsed), GetType(Boolean), GetType(LanguetteDeRepli), New FrameworkPropertyMetadata(False, AddressOf OnIsCollapsedChanged))

        ''' <summary>Vrai quand le panneau est replié ou fermé.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeRepli(Me, Function() IsCollapsed)
        End Function

        Private Shared Sub OnIsCollapsedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            PairDeRepli.Annoncer(DirectCast(d, UIElement), CBool(e.OldValue), CBool(e.NewValue))
        End Sub

    End Class

    ''' <summary>Un bouton qui déplie et replie (ExpandCollapse) ; Expand et Collapse le cliquent au besoin.</summary>
    Friend NotInheritable Class PairDeRepli
        Inherits ButtonAutomationPeer
        Implements IExpandCollapseProvider

        Private ReadOnly _estReplie As Func(Of Boolean)

        Public Sub New(bouton As System.Windows.Controls.Button, estReplie As Func(Of Boolean))
            MyBase.New(bouton)
            _estReplie = estReplie
        End Sub

        ''' <summary>Dit aux lecteurs d'écran que l'état du bouton a changé.</summary>
        Public Shared Sub Annoncer(bouton As UIElement, replieAvant As Boolean, replie As Boolean)
            If Not AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged) Then Return
            Dim pair = UIElementAutomationPeer.FromElement(bouton)
            pair?.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                                            If(replieAvant, ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded),
                                            If(replie, ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded))
        End Sub

        Protected Overrides Function GetNameCore() As String
            Dim nom = MyBase.GetNameCore()
            Return If(String.IsNullOrEmpty(nom), InfoBulle(Owner), nom)
        End Function

        Protected Overrides Function GetClassNameCore() As String
            Return NameOf(Button)
        End Function

        Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
            If patternInterface = PatternInterface.ExpandCollapse Then Return Me
            Return MyBase.GetPattern(patternInterface)
        End Function

        Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
            Get
                Return If(_estReplie(), ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded)
            End Get
        End Property

        ' Le clic, levé tout de suite (Invoke le différerait) : le panneau, qui l'écoute, change d'état.
        Public Sub Expand() Implements IExpandCollapseProvider.Expand
            If Not IsEnabled() Then Throw New ElementNotEnabledException()
            If _estReplie() Then Cliquer()
        End Sub

        Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
            If Not IsEnabled() Then Throw New ElementNotEnabledException()
            If Not _estReplie() Then Cliquer()
        End Sub

        Private Sub Cliquer()
            Owner.RaiseEvent(New RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, Owner))
        End Sub

    End Class

End Namespace
