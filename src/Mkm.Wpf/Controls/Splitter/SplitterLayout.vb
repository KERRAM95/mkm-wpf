Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La disposition d'un <see cref="Splitter"/> (la partie <c>PART_Layout</c> de son gabarit) : ses panneaux, et une
    ''' <see cref="SplitterGutter"/> entre deux, chacun à sa taille. Les panneaux restent les enfants logiques du
    ''' séparateur ; la disposition ne fait que les placer.
    ''' </summary>
    Public Class SplitterLayout
        Inherits FrameworkElement

        Private _splitter As Splitter
        Private ReadOnly _enfants As New List(Of UIElement)
        Private ReadOnly _gouttieres As New List(Of SplitterGutter)

        ''' <summary>Les gouttières, dans l'ordre.</summary>
        Friend ReadOnly Property Gouttieres As IList(Of SplitterGutter)
            Get
                Return _gouttieres
            End Get
        End Property

        Friend Sub Attacher(proprietaire As Splitter)
            _splitter = proprietaire
            Reconstruire()
        End Sub

        Friend Sub Detacher()
            Vider()
            _splitter = Nothing
        End Sub

        ''' <summary>Les panneaux du séparateur, et une gouttière entre deux.</summary>
        Friend Sub Reconstruire()
            Vider()
            If _splitter Is Nothing Then Return
            Dim panneaux = _splitter.Panels
            For i = 0 To panneaux.Count - 1
                Ajouter(panneaux(i))
                If i = panneaux.Count - 1 Then Exit For
                Dim gouttiere As New SplitterGutter With {.Proprietaire = _splitter, .Index = i}
                AddHandler gouttiere.DragDelta, AddressOf OnTirer
                AddHandler gouttiere.DragCompleted, AddressOf OnLacher
                AddLogicalChild(gouttiere)
                _gouttieres.Add(gouttiere)
                Ajouter(gouttiere)
            Next
            InvalidateMeasure()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property VisualChildrenCount As Integer
            Get
                Return _enfants.Count
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function GetVisualChild(index As Integer) As Visual
            Return _enfants(index)
        End Function

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property LogicalChildren As IEnumerator
            Get
                Return _gouttieres.GetEnumerator()
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function MeasureOverride(availableSize As Size) As Size
            If _splitter Is Nothing Then Return New Size()
            Dim horizontal = _splitter.Orientation = Orientation.Horizontal
            Dim epaisseur = _splitter.GutterSize
            For Each gouttiere In _gouttieres
                gouttiere.Orientation = _splitter.Orientation
                gouttiere.Cursor = If(horizontal, Cursors.SizeWE, Cursors.SizeNS)
                gouttiere.Measure(If(horizontal, New Size(epaisseur, availableSize.Height), New Size(availableSize.Width, epaisseur)))
            Next

            Dim longueur = If(horizontal, availableSize.Width, availableSize.Height)
            Dim travers = 0.0
            Dim longueurVoulue = epaisseur * _gouttieres.Count
            Dim tailles = _splitter.GetSizes()
            Dim panneaux = _splitter.Panels
            For i = 0 To panneaux.Count - 1
                Dim part = If(Double.IsInfinity(longueur), Double.PositiveInfinity, Place(longueur) * tailles(i) / 100)
                panneaux(i).Measure(If(horizontal, New Size(part, availableSize.Height), New Size(availableSize.Width, part)))
                Dim voulue = panneaux(i).DesiredSize
                longueurVoulue += If(horizontal, voulue.Width, voulue.Height)
                travers = Math.Max(travers, If(horizontal, voulue.Height, voulue.Width))
            Next
            ' Une place finie est toute prise ; sans limite, chacun à sa taille.
            Dim prise = If(Double.IsInfinity(longueur), longueurVoulue, longueur)
            Return If(horizontal, New Size(prise, travers), New Size(travers, prise))
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function ArrangeOverride(finalSize As Size) As Size
            If _splitter Is Nothing Then Return finalSize
            Dim horizontal = _splitter.Orientation = Orientation.Horizontal
            Dim longueur = If(horizontal, finalSize.Width, finalSize.Height)
            Dim epaisseur = _splitter.GutterSize
            Dim tailles = _splitter.GetSizes()
            Dim panneaux = _splitter.Panels
            Dim position = 0.0
            For i = 0 To panneaux.Count - 1
                Dim part = Place(longueur) * tailles(i) / 100
                panneaux(i).Arrange(If(horizontal, New Rect(position, 0, part, finalSize.Height), New Rect(0, position, finalSize.Width, part)))
                position += part
                If i < _gouttieres.Count Then
                    _gouttieres(i).Arrange(If(horizontal, New Rect(position, 0, epaisseur, finalSize.Height), New Rect(0, position, finalSize.Width, epaisseur)))
                    position += epaisseur
                End If
            Next
            Return finalSize
        End Function

        ''' <summary>La place des panneaux, dans la longueur du séparateur : sa taille, moins les gouttières.</summary>
        Friend ReadOnly Property PlaceDesPanneaux As Double
            Get
                If _splitter Is Nothing Then Return 0.0
                Return Place(If(_splitter.Orientation = Orientation.Horizontal, ActualWidth, ActualHeight))
            End Get
        End Property

        ' La place des panneaux : la longueur, moins les gouttières.
        Private Function Place(longueur As Double) As Double
            Return Math.Max(0.0, longueur - _splitter.GutterSize * _gouttieres.Count)
        End Function

        ' Tirée, la gouttière déplace la frontière de ce que la souris a parcouru, rapporté à la place des panneaux ; la
        ' mise en page suit aussitôt, pour que le pas suivant parte de la nouvelle position de la gouttière.
        Private Sub OnTirer(sender As Object, e As DragDeltaEventArgs)
            Dim gouttiere = DirectCast(sender, SplitterGutter)
            Dim horizontal = _splitter.Orientation = Orientation.Horizontal
            Dim place = Me.Place(If(horizontal, ActualWidth, ActualHeight))
            If place <= 0 Then Return
            Dim parcouru = If(horizontal, e.HorizontalChange, e.VerticalChange)
            _splitter.Deplacer(gouttiere.Index, parcouru * 100 / place)
            UpdateLayout()
        End Sub

        Private Sub OnLacher(sender As Object, e As DragCompletedEventArgs)
            _splitter?.FinirLeDeplacement()
        End Sub

        Private Sub Ajouter(element As UIElement)
            _enfants.Add(element)
            AddVisualChild(element)
        End Sub

        Private Sub Vider()
            For Each element In _enfants
                RemoveVisualChild(element)
            Next
            For Each gouttiere In _gouttieres
                RemoveHandler gouttiere.DragDelta, AddressOf OnTirer
                RemoveHandler gouttiere.DragCompleted, AddressOf OnLacher
                RemoveLogicalChild(gouttiere)
            Next
            _enfants.Clear()
            _gouttieres.Clear()
        End Sub

    End Class

End Namespace
