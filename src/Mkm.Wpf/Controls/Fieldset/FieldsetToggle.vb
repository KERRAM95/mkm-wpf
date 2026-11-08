Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La légende repliable d'un <see cref="Fieldset"/>, partie de son gabarit : enfoncée quand le groupe est replié.
    ''' Les lecteurs d'écran la disent « déplié » ou « réduit », comme <c>aria-expanded</c> dans mkm, et non « activée »
    ''' quand le groupe est replié.
    ''' </summary>
    Friend NotInheritable Class FieldsetToggle
        Inherits System.Windows.Controls.Primitives.ToggleButton

        ''' <inheritdoc/>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeLegende(Me)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnChecked(e As RoutedEventArgs)
            MyBase.OnChecked(e)
            Annoncer(ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnUnchecked(e As RoutedEventArgs)
            MyBase.OnUnchecked(e)
            Annoncer(ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded)
        End Sub

        Private Sub Annoncer(avant As ExpandCollapseState, apres As ExpandCollapseState)
            If AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged) Then
                UIElementAutomationPeer.FromElement(Me)?.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, avant, apres)
            End If
        End Sub

        ''' <summary>Le pair de la légende : un bouton qui déplie et replie (ExpandCollapse), non une bascule.</summary>
        Private NotInheritable Class PairDeLegende
            Inherits ButtonBaseAutomationPeer
            Implements IExpandCollapseProvider

            Public Sub New(legende As FieldsetToggle)
                MyBase.New(legende)
            End Sub

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.Button
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(FieldsetToggle)
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.ExpandCollapse Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Private ReadOnly Property Legende As FieldsetToggle
                Get
                    Return DirectCast(Owner, FieldsetToggle)
                End Get
            End Property

            ' Enfoncée, la légende a replié le groupe.
            Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
                Get
                    Return If(Legende.IsChecked.GetValueOrDefault(), ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded)
                End Get
            End Property

            Public Sub Expand() Implements IExpandCollapseProvider.Expand
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                Legende.IsChecked = False
            End Sub

            Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                Legende.IsChecked = True
            End Sub

        End Class

    End Class

End Namespace
