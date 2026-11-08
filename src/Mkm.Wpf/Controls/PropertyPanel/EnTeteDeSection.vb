Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' L'en-tête d'une <see cref="PropertySection"/> : enfoncé, la section est ouverte. Pour les lecteurs d'écran, un bouton
    ''' qui déplie et replie (ExpandCollapse, le <c>aria-expanded</c> de mkm), non une bascule « activée ».
    ''' </summary>
    Friend NotInheritable Class EnTeteDeSection
        Inherits System.Windows.Controls.Primitives.ToggleButton

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDEnTete(Me)
        End Function

        Protected Overrides Sub OnChecked(e As RoutedEventArgs)
            MyBase.OnChecked(e)
            Annoncer(ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded)
        End Sub

        Protected Overrides Sub OnUnchecked(e As RoutedEventArgs)
            MyBase.OnUnchecked(e)
            Annoncer(ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
        End Sub

        Private Sub Annoncer(avant As ExpandCollapseState, apres As ExpandCollapseState)
            If AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged) Then
                UIElementAutomationPeer.FromElement(Me)?.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, avant, apres)
            End If
        End Sub

        Private NotInheritable Class PairDEnTete
            Inherits ButtonBaseAutomationPeer
            Implements IExpandCollapseProvider

            Public Sub New(entete As EnTeteDeSection)
                MyBase.New(entete)
            End Sub

            Private ReadOnly Property EnTete As EnTeteDeSection
                Get
                    Return DirectCast(Owner, EnTeteDeSection)
                End Get
            End Property

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.Button
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(PropertySection)
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.ExpandCollapse Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
                Get
                    Return If(EnTete.IsChecked.GetValueOrDefault(), ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
                End Get
            End Property

            Public Sub Expand() Implements IExpandCollapseProvider.Expand
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                EnTete.IsChecked = True
            End Sub

            Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                EnTete.IsChecked = False
            End Sub

        End Class

    End Class

End Namespace
