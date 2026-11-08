Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Referme l'info-bulle ouverte quand la page de son élément défile, comme mkm (le Tooltip de PrimeVue se cache au
    ''' défilement) : survolé pendant que la molette faisait défiler la page, il restait à sa place, détaché de son élément.
    ''' Posé sur toute info-bulle de WPF, une fois par application (<see cref="MkmResources"/>).
    ''' </summary>
    Friend NotInheritable Class InfoBulleAuDefilement

        Private Shared _enregistre As Boolean
        Private Shared ReadOnly _ecoutes As New ConditionalWeakTable(Of ToolTip, FermetureAuDefilement)()

        Private Sub New()
        End Sub

        Friend Shared Sub Enregistrer()
            If _enregistre Then Return
            _enregistre = True
            EventManager.RegisterClassHandler(GetType(ToolTip), ToolTip.OpenedEvent, New RoutedEventHandler(AddressOf Ouverte))
            EventManager.RegisterClassHandler(GetType(ToolTip), ToolTip.ClosedEvent, New RoutedEventHandler(AddressOf Fermee))
        End Sub

        Private Shared Sub Ouverte(sender As Object, e As RoutedEventArgs)
            Dim bulle = DirectCast(sender, ToolTip)
            ' Son élément : celui qu'elle désigne, sinon celui sous la souris qui l'a ouverte.
            Dim element = If(TryCast(bulle.PlacementTarget, FrameworkElement), TryCast(Mouse.DirectlyOver, FrameworkElement))
            If element Is Nothing Then Return
            Fermee(sender, e)
            Dim ecoute As New FermetureAuDefilement(element, Sub() bulle.IsOpen = False)
            _ecoutes.Add(bulle, ecoute)
            ecoute.Ecouter()
        End Sub

        Private Shared Sub Fermee(sender As Object, e As RoutedEventArgs)
            Dim bulle = DirectCast(sender, ToolTip)
            Dim ecoute As FermetureAuDefilement = Nothing
            If _ecoutes.TryGetValue(bulle, ecoute) Then
                ecoute.Cesser()
                _ecoutes.Remove(bulle)
            End If
        End Sub

    End Class

End Namespace
