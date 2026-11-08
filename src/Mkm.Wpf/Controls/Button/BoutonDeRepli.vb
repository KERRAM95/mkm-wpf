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
            Return New PairDuBoutonDeRepli(Me)
        End Function

        Private Shared Sub OnIsCollapsedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            If Not AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged) Then Return
            Dim pair = UIElementAutomationPeer.FromElement(DirectCast(d, UIElement))
            pair?.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                                            If(CBool(e.OldValue), ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded),
                                            If(CBool(e.NewValue), ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded))
        End Sub

        ''' <summary>Un bouton qui déplie et replie (ExpandCollapse) ; Expand et Collapse le cliquent au besoin.</summary>
        Private NotInheritable Class PairDuBoutonDeRepli
            Inherits ButtonAutomationPeer
            Implements IExpandCollapseProvider

            Public Sub New(bouton As BoutonDeRepli)
                MyBase.New(bouton)
            End Sub

            Private ReadOnly Property Bouton As BoutonDeRepli
                Get
                    Return DirectCast(Owner, BoutonDeRepli)
                End Get
            End Property

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
                    Return If(Bouton.IsCollapsed, ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded)
                End Get
            End Property

            Public Sub Expand() Implements IExpandCollapseProvider.Expand
                If Bouton.IsCollapsed Then DirectCast(Me, IInvokeProvider).Invoke()
            End Sub

            Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
                If Not Bouton.IsCollapsed Then DirectCast(Me, IInvokeProvider).Invoke()
            End Sub

        End Class

    End Class

End Namespace
