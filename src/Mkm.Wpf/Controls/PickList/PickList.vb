Imports System.Collections
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>Les données d'un passage d'éléments d'une liste à l'autre d'un <see cref="PickList"/>.</summary>
    Public Class PickListMoveEventArgs
        Inherits RoutedEventArgs

        Public Sub New(evenement As RoutedEvent, source As Object, elements As IList)
            MyBase.New(evenement, source)
            Items = elements
        End Sub

        ''' <summary>Les éléments passés.</summary>
        Public ReadOnly Property Items As IList
    End Class

    ''' <summary>
    ''' Les deux listes de mkm (<c>PickList</c>) : une source et une cible, et entre elles quatre boutons qui font passer
    ''' les éléments choisis (ou tous) de l'une à l'autre ; chaque liste s'ordonne comme une <see cref="OrderList"/>.
    ''' <code>
    ''' &lt;mkm:PickList Source="{Binding Disponibles}" Target="{Binding Choisis}" DisplayMemberPath="Nom" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>Les deux listes changent en place : des listes modifiables (des <c>ObservableCollection</c>).
    ''' Un double clic fait passer un élément.</remarks>
    <TemplatePart(Name:="PART_Source", Type:=GetType(OrderList))>
    <TemplatePart(Name:="PART_Target", Type:=GetType(OrderList))>
    <TemplatePart(Name:="PART_MoveToTarget", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_MoveAllToTarget", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_MoveToSource", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_MoveAllToSource", Type:=GetType(ButtonBase))>
    Public Class PickList
        Inherits Control

        ''' <summary>La liste source.</summary>
        Public Shared ReadOnly SourceProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Source), GetType(IList), GetType(PickList), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La liste cible.</summary>
        Public Shared ReadOnly TargetProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Target), GetType(IList), GetType(PickList), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le gabarit d'un élément, dans les deux listes.</summary>
        Public Shared ReadOnly ItemTemplateProperty As DependencyProperty = ItemsControl.ItemTemplateProperty.AddOwner(GetType(PickList))

        ''' <summary>La propriété dont la valeur s'écrit pour un élément, sans gabarit.</summary>
        Public Shared ReadOnly DisplayMemberPathProperty As DependencyProperty = ItemsControl.DisplayMemberPathProperty.AddOwner(GetType(PickList))

        ''' <summary>L'en-tête de la source.</summary>
        Public Shared ReadOnly SourceHeaderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(SourceHeader), GetType(Object), GetType(PickList), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'en-tête de la cible.</summary>
        Public Shared ReadOnly TargetHeaderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(TargetHeader), GetType(Object), GetType(PickList), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai (par défaut) pour les boutons qui ordonnent la source.</summary>
        Public Shared ReadOnly ShowSourceControlsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowSourceControls), GetType(Boolean), GetType(PickList), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai (par défaut) pour les boutons qui ordonnent la cible.</summary>
        Public Shared ReadOnly ShowTargetControlsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowTargetControls), GetType(Boolean), GetType(PickList), New FrameworkPropertyMetadata(True))

        ''' <summary>Des éléments sont passés à la cible.</summary>
        Public Shared ReadOnly MovedToTargetEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(MovedToTarget), RoutingStrategy.Bubble, GetType(EventHandler(Of PickListMoveEventArgs)), GetType(PickList))

        ''' <summary>Des éléments sont revenus à la source.</summary>
        Public Shared ReadOnly MovedToSourceEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(MovedToSource), RoutingStrategy.Bubble, GetType(EventHandler(Of PickListMoveEventArgs)), GetType(PickList))

        Private _source As OrderList
        Private _cible As OrderList
        Private ReadOnly _boutons As New Dictionary(Of String, ButtonBase)

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(PickList), New FrameworkPropertyMetadata(GetType(PickList)))
            FocusableProperty.OverrideMetadata(GetType(PickList), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>La liste source.</summary>
        Public Property Source As IList
            Get
                Return DirectCast(GetValue(SourceProperty), IList)
            End Get
            Set(value As IList)
                SetValue(SourceProperty, value)
            End Set
        End Property

        ''' <summary>La liste cible.</summary>
        Public Property Target As IList
            Get
                Return DirectCast(GetValue(TargetProperty), IList)
            End Get
            Set(value As IList)
                SetValue(TargetProperty, value)
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

        ''' <summary>L'en-tête de la source.</summary>
        Public Property SourceHeader As Object
            Get
                Return GetValue(SourceHeaderProperty)
            End Get
            Set(value As Object)
                SetValue(SourceHeaderProperty, value)
            End Set
        End Property

        ''' <summary>L'en-tête de la cible.</summary>
        Public Property TargetHeader As Object
            Get
                Return GetValue(TargetHeaderProperty)
            End Get
            Set(value As Object)
                SetValue(TargetHeaderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour les boutons qui ordonnent la source.</summary>
        Public Property ShowSourceControls As Boolean
            Get
                Return CBool(GetValue(ShowSourceControlsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowSourceControlsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour les boutons qui ordonnent la cible.</summary>
        Public Property ShowTargetControls As Boolean
            Get
                Return CBool(GetValue(ShowTargetControlsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowTargetControlsProperty, value)
            End Set
        End Property

        ''' <summary>Des éléments sont passés à la cible.</summary>
        Public Custom Event MovedToTarget As EventHandler(Of PickListMoveEventArgs)
            AddHandler(value As EventHandler(Of PickListMoveEventArgs))
                [AddHandler](MovedToTargetEvent, value)
            End AddHandler
            RemoveHandler(value As EventHandler(Of PickListMoveEventArgs))
                [RemoveHandler](MovedToTargetEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As PickListMoveEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Des éléments sont revenus à la source.</summary>
        Public Custom Event MovedToSource As EventHandler(Of PickListMoveEventArgs)
            AddHandler(value As EventHandler(Of PickListMoveEventArgs))
                [AddHandler](MovedToSourceEvent, value)
            End AddHandler
            RemoveHandler(value As EventHandler(Of PickListMoveEventArgs))
                [RemoveHandler](MovedToSourceEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As PickListMoveEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            For Each bouton In _boutons.Values
                RemoveHandler bouton.Click, AddressOf Bouton_Click
            Next
            _boutons.Clear()
            For Each liste In {_source, _cible}
                If liste IsNot Nothing Then RemoveHandler liste.MouseDoubleClick, AddressOf Liste_MouseDoubleClick
            Next
            MyBase.OnApplyTemplate()
            _source = TryCast(GetTemplateChild("PART_Source"), OrderList)
            _cible = TryCast(GetTemplateChild("PART_Target"), OrderList)
            For Each liste In {_source, _cible}
                If liste IsNot Nothing Then AddHandler liste.MouseDoubleClick, AddressOf Liste_MouseDoubleClick
            Next
            For Each nom In {"PART_MoveToTarget", "PART_MoveAllToTarget", "PART_MoveToSource", "PART_MoveAllToSource"}
                Dim bouton = TryCast(GetTemplateChild(nom), ButtonBase)
                If bouton Is Nothing Then Continue For
                _boutons(nom) = bouton
                AddHandler bouton.Click, AddressOf Bouton_Click
            Next
        End Sub

        Private Sub Bouton_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            Select Case _boutons.First(Function(b) b.Value Is sender).Key
                Case "PART_MoveToTarget" : MoveToTarget()
                Case "PART_MoveAllToTarget" : MoveAllToTarget()
                Case "PART_MoveToSource" : MoveToSource()
                Case "PART_MoveAllToSource" : MoveAllToSource()
            End Select
        End Sub

        ' Un double clic sur un élément le fait passer de l'autre côté, comme mkm.
        Private Sub Liste_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
            Dim element = ItemsControl.ContainerFromElement(DirectCast(sender, OrderList).Liste, TryCast(e.OriginalSource, DependencyObject))
            If element Is Nothing Then Return
            If sender Is _source Then MoveToTarget() Else MoveToSource()
            e.Handled = True
        End Sub

        ''' <summary>Fait passer les éléments choisis de la source à la cible.</summary>
        Public Sub MoveToTarget()
            Passer(Source, Target, _source?.SelectedItems.Cast(Of Object)().ToList(), MovedToTargetEvent)
        End Sub

        ''' <summary>Fait passer tous les éléments de la source à la cible.</summary>
        Public Sub MoveAllToTarget()
            Passer(Source, Target, Source?.Cast(Of Object)().ToList(), MovedToTargetEvent)
        End Sub

        ''' <summary>Fait revenir les éléments choisis de la cible à la source.</summary>
        Public Sub MoveToSource()
            Passer(Target, Source, _cible?.SelectedItems.Cast(Of Object)().ToList(), MovedToSourceEvent)
        End Sub

        ''' <summary>Fait revenir tous les éléments de la cible à la source.</summary>
        Public Sub MoveAllToSource()
            Passer(Target, Source, Target?.Cast(Of Object)().ToList(), MovedToSourceEvent)
        End Sub

        ''' <summary>Passe des éléments d'une liste à l'autre, à la fin, dans leur ordre.</summary>
        Friend Sub Passer(depuis As IList, vers As IList, elements As List(Of Object), evenement As RoutedEvent)
            If depuis Is Nothing OrElse vers Is Nothing OrElse elements Is Nothing OrElse elements.Count = 0 Then Return
            If depuis.IsReadOnly OrElse depuis.IsFixedSize OrElse vers.IsReadOnly OrElse vers.IsFixedSize Then Return
            Dim passes = elements.OrderBy(Function(o) depuis.IndexOf(o)).ToList()
            For Each element In passes
                depuis.Remove(element)
                vers.Add(element)
            Next
            Me.RaiseEvent(New PickListMoveEventArgs(evenement, Me, passes))
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran : ses deux listes et ses boutons.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

End Namespace
