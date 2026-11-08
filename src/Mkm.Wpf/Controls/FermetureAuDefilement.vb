Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Referme la liste ou le calendrier ouverts d'un champ quand la page qui le contient défile, comme mkm
    ''' (ConnectedOverlayScrollHandler) : une liste ouverte restait sinon à sa place sur l'écran, détachée de son champ.
    ''' On l'écoute à l'ouverture (<see cref="Ecouter"/>), on cesse à la fermeture (<see cref="Cesser"/>).
    ''' </summary>
    Friend NotInheritable Class FermetureAuDefilement

        Private ReadOnly _champ As FrameworkElement
        Private ReadOnly _fermer As System.Action
        Private ReadOnly _gestionnaire As ScrollChangedEventHandler
        Private _fenetre As Window

        ''' <param name="champ">Le champ dont la liste est ouverte.</param>
        ''' <param name="fermer">Ce qui la referme.</param>
        Friend Sub New(champ As FrameworkElement, fermer As System.Action)
            _champ = champ
            _fermer = fermer
            _gestionnaire = AddressOf Defilement
        End Sub

        ''' <summary>À l'ouverture : écoute le défilement de la fenêtre du champ.</summary>
        Friend Sub Ecouter()
            Cesser()
            _fenetre = Window.GetWindow(_champ)
            ' Un défilement déjà marqué traité (par le ScrollViewer qui défile) compte aussi.
            _fenetre?.AddHandler(ScrollViewer.ScrollChangedEvent, _gestionnaire, True)
        End Sub

        ''' <summary>À la fermeture : cesse d'écouter.</summary>
        Friend Sub Cesser()
            _fenetre?.RemoveHandler(ScrollViewer.ScrollChangedEvent, _gestionnaire)
            _fenetre = Nothing
        End Sub

        ' Seul compte un défilement d'un ancêtre du champ : la liste ouverte a le sien, dans sa fenêtre à part.
        Private Sub Defilement(sender As Object, e As ScrollChangedEventArgs)
            If e.VerticalChange = 0 AndAlso e.HorizontalChange = 0 Then Return
            Dim defileur = TryCast(e.OriginalSource, Visual)
            If defileur Is Nothing OrElse Not defileur.IsAncestorOf(_champ) Then Return
            Cesser()
            _fermer()
        End Sub

    End Class

End Namespace
