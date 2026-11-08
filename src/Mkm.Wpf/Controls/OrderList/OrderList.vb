Imports System.Collections
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>De quel côté de la liste se posent les boutons d'une <see cref="OrderList"/>.</summary>
    Public Enum OrderListControlsPosition
        ''' <summary>À gauche (par défaut).</summary>
        Left
        ''' <summary>À droite (la cible d'un PickList).</summary>
        Right
    End Enum

    ''' <summary>
    ''' La liste à ordonner de mkm (<c>OrderList</c>) : une liste (<see cref="Listbox"/>) et, à sa gauche, quatre boutons
    ''' qui montent, mettent tout en haut, descendent ou mettent tout en bas les éléments choisis.
    ''' <code>
    ''' &lt;mkm:OrderList ItemsSource="{Binding Etapes}" DisplayMemberPath="Nom" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' La liste s'ordonne en place : <see cref="ItemsSource"/> est une liste modifiable (une
    ''' <c>ObservableCollection</c>, pour que la page suive) ; <see cref="Reorder"/> dit qu'elle a changé d'ordre.
    ''' </remarks>
    <TemplatePart(Name:="PART_List", Type:=GetType(Listbox))>
    <TemplatePart(Name:="PART_MoveUp", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_MoveTop", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_MoveDown", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_MoveBottom", Type:=GetType(ButtonBase))>
    Public Class OrderList
        Inherits Control

        ''' <summary>Les éléments, une liste modifiable que les boutons ordonnent.</summary>
        Public Shared ReadOnly ItemsSourceProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ItemsSource), GetType(IList), GetType(OrderList), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le gabarit d'un élément.</summary>
        Public Shared ReadOnly ItemTemplateProperty As DependencyProperty = ItemsControl.ItemTemplateProperty.AddOwner(GetType(OrderList))

        ''' <summary>La propriété dont la valeur s'écrit pour un élément, sans gabarit.</summary>
        Public Shared ReadOnly DisplayMemberPathProperty As DependencyProperty = ItemsControl.DisplayMemberPathProperty.AddOwner(GetType(OrderList))

        ''' <summary>Comment se choisissent les éléments : plusieurs, avec Ctrl et Maj, par défaut.</summary>
        Public Shared ReadOnly SelectionModeProperty As DependencyProperty = ListBox.SelectionModeProperty.AddOwner(
            GetType(OrderList), New FrameworkPropertyMetadata(SelectionMode.Extended))

        ''' <summary>L'en-tête au-dessus de la liste.</summary>
        Public Shared ReadOnly HeaderProperty As DependencyProperty = HeaderedContentControl.HeaderProperty.AddOwner(GetType(OrderList))

        ''' <summary>Vrai (par défaut) pour les quatre boutons.</summary>
        Public Shared ReadOnly ShowControlsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowControls), GetType(Boolean), GetType(OrderList), New FrameworkPropertyMetadata(True))

        ''' <summary>De quel côté se posent les boutons : à gauche par défaut.</summary>
        Public Shared ReadOnly ControlsPositionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ControlsPosition), GetType(OrderListControlsPosition), GetType(OrderList), New FrameworkPropertyMetadata(OrderListControlsPosition.Left))

        ''' <summary>La liste a changé d'ordre.</summary>
        Public Shared ReadOnly ReorderEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Reorder), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(OrderList))

        Private _liste As Listbox
        Private ReadOnly _boutons As New Dictionary(Of String, ButtonBase)

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(OrderList), New FrameworkPropertyMetadata(GetType(OrderList)))
            FocusableProperty.OverrideMetadata(GetType(OrderList), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Les éléments.</summary>
        Public Property ItemsSource As IList
            Get
                Return DirectCast(GetValue(ItemsSourceProperty), IList)
            End Get
            Set(value As IList)
                SetValue(ItemsSourceProperty, value)
            End Set
        End Property

        ''' <summary>Le gabarit d'un élément.</summary>
        Public Property ItemTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(ItemTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(ItemTemplateProperty, value)
            End Set
        End Property

        ''' <summary>La propriété dont la valeur s'écrit pour un élément.</summary>
        Public Property DisplayMemberPath As String
            Get
                Return CStr(GetValue(DisplayMemberPathProperty))
            End Get
            Set(value As String)
                SetValue(DisplayMemberPathProperty, value)
            End Set
        End Property

        ''' <summary>Comment se choisissent les éléments.</summary>
        Public Property SelectionMode As SelectionMode
            Get
                Return DirectCast(GetValue(SelectionModeProperty), SelectionMode)
            End Get
            Set(value As SelectionMode)
                SetValue(SelectionModeProperty, value)
            End Set
        End Property

        ''' <summary>L'en-tête au-dessus de la liste.</summary>
        Public Property Header As Object
            Get
                Return GetValue(HeaderProperty)
            End Get
            Set(value As Object)
                SetValue(HeaderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour les quatre boutons.</summary>
        Public Property ShowControls As Boolean
            Get
                Return CBool(GetValue(ShowControlsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowControlsProperty, value)
            End Set
        End Property

        ''' <summary>De quel côté se posent les boutons.</summary>
        Public Property ControlsPosition As OrderListControlsPosition
            Get
                Return DirectCast(GetValue(ControlsPositionProperty), OrderListControlsPosition)
            End Get
            Set(value As OrderListControlsPosition)
                SetValue(ControlsPositionProperty, value)
            End Set
        End Property

        ''' <summary>La liste a changé d'ordre.</summary>
        Public Custom Event Reorder As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                [AddHandler](ReorderEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                [RemoveHandler](ReorderEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Les éléments choisis, dans l'ordre de la liste.</summary>
        Public ReadOnly Property SelectedItems As IList
            Get
                If _liste Is Nothing Then Return New ArrayList()
                Return _liste.SelectedItems
            End Get
        End Property

        ''' <summary>La liste du gabarit (PickList s'en sert).</summary>
        Friend ReadOnly Property Liste As Listbox
            Get
                Return _liste
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            For Each bouton In _boutons.Values
                RemoveHandler bouton.Click, AddressOf Bouton_Click
            Next
            _boutons.Clear()
            MyBase.OnApplyTemplate()
            _liste = TryCast(GetTemplateChild("PART_List"), Listbox)
            For Each nom In {"PART_MoveUp", "PART_MoveTop", "PART_MoveDown", "PART_MoveBottom"}
                Dim bouton = TryCast(GetTemplateChild(nom), ButtonBase)
                If bouton Is Nothing Then Continue For
                _boutons(nom) = bouton
                AddHandler bouton.Click, AddressOf Bouton_Click
            Next
        End Sub

        Private Sub Bouton_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            Select Case _boutons.First(Function(b) b.Value Is sender).Key
                Case "PART_MoveUp" : MoveUp()
                Case "PART_MoveTop" : MoveTop()
                Case "PART_MoveDown" : MoveDown()
                Case "PART_MoveBottom" : MoveBottom()
            End Select
        End Sub

        ''' <summary>Monte chaque élément choisi d'un rang.</summary>
        Public Sub MoveUp()
            Ordonner(Function(liste, choix)
                         Dim change = False
                         For Each indice In choix.Select(Function(c) liste.IndexOf(c)).OrderBy(Function(i) i)
                             If indice > 0 AndAlso Not choix.Contains(liste(indice - 1)) Then
                                 Deplacer(liste, indice, indice - 1)
                                 change = True
                             End If
                         Next
                         Return change
                     End Function)
        End Sub

        ''' <summary>Descend chaque élément choisi d'un rang.</summary>
        Public Sub MoveDown()
            Ordonner(Function(liste, choix)
                         Dim change = False
                         For Each indice In choix.Select(Function(c) liste.IndexOf(c)).OrderByDescending(Function(i) i)
                             If indice < liste.Count - 1 AndAlso Not choix.Contains(liste(indice + 1)) Then
                                 Deplacer(liste, indice, indice + 1)
                                 change = True
                             End If
                         Next
                         Return change
                     End Function)
        End Sub

        ''' <summary>Met les éléments choisis tout en haut, dans leur ordre.</summary>
        Public Sub MoveTop()
            Ordonner(Function(liste, choix)
                         Dim change = False
                         Dim rang = 0
                         For Each element In choix.OrderBy(Function(c) liste.IndexOf(c)).ToList()
                             Dim indice = liste.IndexOf(element)
                             If indice <> rang Then
                                 Deplacer(liste, indice, rang)
                                 change = True
                             End If
                             rang += 1
                         Next
                         Return change
                     End Function)
        End Sub

        ''' <summary>Met les éléments choisis tout en bas, dans leur ordre.</summary>
        Public Sub MoveBottom()
            Ordonner(Function(liste, choix)
                         Dim change = False
                         Dim rang = liste.Count - 1
                         For Each element In choix.OrderByDescending(Function(c) liste.IndexOf(c)).ToList()
                             Dim indice = liste.IndexOf(element)
                             If indice <> rang Then
                                 Deplacer(liste, indice, rang)
                                 change = True
                             End If
                             rang -= 1
                         Next
                         Return change
                     End Function)
        End Sub

        ' Les choix gardés et montrés après le déplacement ; Reorder s'il y a eu un changement.
        Private Sub Ordonner(deplacement As Func(Of IList, List(Of Object), Boolean))
            Dim liste = ItemsSource
            If liste Is Nothing OrElse liste.IsReadOnly OrElse liste.IsFixedSize Then Return
            Dim choix = SelectedItems.Cast(Of Object)().ToList()
            If choix.Count = 0 Then Return
            If Not deplacement(liste, choix) Then Return
            If _liste IsNot Nothing Then
                _liste.SelectedItems.Clear()
                For Each element In choix
                    _liste.SelectedItems.Add(element)
                Next
                _liste.ScrollIntoView(choix.OrderBy(Function(c) liste.IndexOf(c)).First())
            End If
            Me.RaiseEvent(New RoutedEventArgs(ReorderEvent, Me))
        End Sub

        ''' <summary>Déplace un élément d'une liste : Move d'une ObservableCollection (la liste garde le choix), sinon retirer et
        ''' insérer.</summary>
        Friend Shared Sub Deplacer(liste As IList, depuis As Integer, vers As Integer)
            If depuis = vers Then Return
            Dim move = liste.GetType().GetMethod("Move", {GetType(Integer), GetType(Integer)})
            If move IsNot Nothing Then
                move.Invoke(liste, New Object() {depuis, vers})
                Return
            End If
            Dim element = liste(depuis)
            liste.RemoveAt(depuis)
            liste.Insert(vers, element)
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran : ses boutons, puis sa liste.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group, Function() TryCast(Header, String))
        End Function

    End Class

End Namespace
