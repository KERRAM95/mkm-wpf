Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>Où se posent les évènements d'une <see cref="Timeline"/> par rapport à la ligne.</summary>
    Public Enum TimelineAlign
        ''' <summary>Le contenu à droite de la ligne verticale, l'opposé à gauche (par défaut).</summary>
        Left
        ''' <summary>Le contenu à gauche de la ligne verticale.</summary>
        Right
        ''' <summary>Un évènement d'un côté, le suivant de l'autre.</summary>
        Alternate
        ''' <summary>Le contenu au-dessus de la ligne horizontale.</summary>
        Top
        ''' <summary>Le contenu au-dessous de la ligne horizontale.</summary>
        Bottom
    End Enum

    ''' <summary>
    ''' La frise de mkm (Timeline) : des évènements le long d'une ligne, chacun un rond sur la ligne, son contenu d'un côté
    ''' et, de l'autre, son opposé (une date). Ses éléments sont les évènements ; <c>ItemTemplate</c> dessine leur
    ''' contenu, <c>OppositeTemplate</c> leur opposé.
    ''' <code>
    ''' &lt;mkm:Timeline ItemsSource="{Binding Revisions}"&gt;
    '''     &lt;mkm:Timeline.ItemTemplate&gt;&lt;DataTemplate&gt;&lt;TextBlock Text="{Binding Libelle}" /&gt;&lt;/DataTemplate&gt;&lt;/mkm:Timeline.ItemTemplate&gt;
    '''     &lt;mkm:Timeline.OppositeTemplate&gt;&lt;DataTemplate&gt;&lt;TextBlock Text="{Binding Date}" /&gt;&lt;/DataTemplate&gt;&lt;/mkm:Timeline.OppositeTemplate&gt;
    ''' &lt;/mkm:Timeline&gt;
    ''' </code>
    ''' </summary>
    Public Class Timeline
        Inherits ItemsControl

        ''' <summary>Le sens de la ligne : verticale (par défaut) ou horizontale.</summary>
        Public Shared ReadOnly LayoutProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Layout), GetType(Orientation), GetType(Timeline), New FrameworkPropertyMetadata(Orientation.Vertical, AddressOf OnPlacementChanged))

        ''' <summary>
        ''' Le côté du contenu : en colonne, à droite de la ligne (Left, par défaut) ou à gauche (Right) ; en ligne, sous la
        ''' ligne (Top, par défaut) ou au-dessus (Bottom) ; Alternate, d'un côté puis de l'autre.
        ''' </summary>
        Public Shared ReadOnly AlignProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Align), GetType(TimelineAlign), GetType(Timeline), New FrameworkPropertyMetadata(TimelineAlign.Left, AddressOf OnPlacementChanged))

        ''' <summary>Le modèle de l'opposé de chaque évènement (une date), de l'autre côté de la ligne.</summary>
        Public Shared ReadOnly OppositeTemplateProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(OppositeTemplate), GetType(DataTemplate), GetType(Timeline), New FrameworkPropertyMetadata(Nothing))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Timeline), New FrameworkPropertyMetadata(GetType(Timeline)))
            FocusableProperty.OverrideMetadata(GetType(Timeline), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Le sens de la ligne.</summary>
        Public Property Layout As Orientation
            Get
                Return DirectCast(GetValue(LayoutProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(LayoutProperty, value)
            End Set
        End Property

        ''' <summary>Le côté du contenu.</summary>
        Public Property Align As TimelineAlign
            Get
                Return DirectCast(GetValue(AlignProperty), TimelineAlign)
            End Get
            Set(value As TimelineAlign)
                SetValue(AlignProperty, value)
            End Set
        End Property

        ''' <summary>Le modèle de l'opposé de chaque évènement.</summary>
        Public Property OppositeTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(OppositeTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(OppositeTemplateProperty, value)
            End Set
        End Property

        ''' <summary>Une liste pour les lecteurs d'écran : ses évènements.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.List)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is TimelineEvent
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New TimelineEvent()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Dim evenement = DirectCast(element, TimelineEvent)
            If Not TypeOf item Is TimelineEvent Then
                evenement.SetCurrentValue(TimelineEvent.OppositeProperty, item)
                evenement.SetCurrentValue(TimelineEvent.OppositeTemplateProperty, OppositeTemplate)
            End If
            Placer()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Placer()
        End Sub

        Private Shared Sub OnPlacementChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Timeline).Placer()
        End Sub

        ' Le sens, le côté de chaque évènement (alterné : un sur deux retourné) et le dernier, sans trait après lui.
        Private Sub Placer()
            Dim evenements = Items.Cast(Of Object)().
                Select(Function(o) TryCast(If(TryCast(o, TimelineEvent), ItemContainerGenerator.ContainerFromItem(o)), TimelineEvent)).ToList()
            For i = 0 To evenements.Count - 1
                If evenements(i) Is Nothing Then Continue For
                ' En colonne, Right échange les côtés ; en ligne, Bottom ; Alternate, un évènement sur deux (nth-child(even)).
                Dim unSurDeux = Align = TimelineAlign.Alternate AndAlso i Mod 2 = 1
                Dim retourne = unSurDeux OrElse If(Layout = Orientation.Horizontal, Align = TimelineAlign.Bottom, Align = TimelineAlign.Right)
                evenements(i).Placer(Layout, retourne, i = evenements.Count - 1)
            Next
        End Sub

    End Class

    ''' <summary>
    ''' Un évènement d'une <see cref="Timeline"/> : son contenu (<c>Content</c>) d'un côté de la ligne, son opposé
    ''' (<c>Opposite</c>) de l'autre, un rond sur la ligne entre eux.
    ''' </summary>
    Public Class TimelineEvent
        Inherits ContentControl

        ''' <summary>L'opposé : ce qui se pose de l'autre côté de la ligne (une date).</summary>
        Public Shared ReadOnly OppositeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Opposite), GetType(Object), GetType(TimelineEvent), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le modèle de l'opposé.</summary>
        Public Shared ReadOnly OppositeTemplateProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(OppositeTemplate), GetType(DataTemplate), GetType(TimelineEvent), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le rond sur la ligne, à la place du rond de mkm (une icône dans un rond).</summary>
        Public Shared ReadOnly MarkerProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Marker), GetType(Object), GetType(TimelineEvent), New FrameworkPropertyMetadata(Nothing))

        Private Shared ReadOnly LayoutPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Layout), GetType(Orientation), GetType(TimelineEvent), New FrameworkPropertyMetadata(Orientation.Vertical))
        ''' <summary>Le sens de la ligne de sa Timeline.</summary>
        Public Shared ReadOnly LayoutProperty As DependencyProperty = LayoutPropertyKey.DependencyProperty

        Private Shared ReadOnly IsReversedPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsReversed), GetType(Boolean), GetType(TimelineEvent), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand son contenu et son opposé sont échangés (Align Right, Top, ou un sur deux en Alternate).</summary>
        Public Shared ReadOnly IsReversedProperty As DependencyProperty = IsReversedPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLastPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLast), GetType(Boolean), GetType(TimelineEvent), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le dernier évènement : sans trait après lui.</summary>
        Public Shared ReadOnly IsLastProperty As DependencyProperty = IsLastPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(TimelineEvent), New FrameworkPropertyMetadata(GetType(TimelineEvent)))
            FocusableProperty.OverrideMetadata(GetType(TimelineEvent), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>L'opposé.</summary>
        Public Property Opposite As Object
            Get
                Return GetValue(OppositeProperty)
            End Get
            Set(value As Object)
                SetValue(OppositeProperty, value)
            End Set
        End Property

        ''' <summary>Le modèle de l'opposé.</summary>
        Public Property OppositeTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(OppositeTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(OppositeTemplateProperty, value)
            End Set
        End Property

        ''' <summary>Le rond sur la ligne, à la place de celui de mkm.</summary>
        Public Property Marker As Object
            Get
                Return GetValue(MarkerProperty)
            End Get
            Set(value As Object)
                SetValue(MarkerProperty, value)
            End Set
        End Property

        ''' <summary>Le sens de la ligne.</summary>
        Public ReadOnly Property Layout As Orientation
            Get
                Return DirectCast(GetValue(LayoutProperty), Orientation)
            End Get
        End Property

        ''' <summary>Vrai quand son contenu et son opposé sont échangés.</summary>
        Public ReadOnly Property IsReversed As Boolean
            Get
                Return CBool(GetValue(IsReversedProperty))
            End Get
        End Property

        ''' <summary>Vrai pour le dernier évènement.</summary>
        Public ReadOnly Property IsLast As Boolean
            Get
                Return CBool(GetValue(IsLastProperty))
            End Get
        End Property

        Friend Sub Placer(sens As Orientation, retourne As Boolean, dernier As Boolean)
            SetValue(LayoutPropertyKey, sens)
            SetValue(IsReversedPropertyKey, retourne)
            SetValue(IsLastPropertyKey, dernier)
        End Sub

        ''' <summary>Un élément de liste pour les lecteurs d'écran : son opposé et son contenu.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.ListItem)
        End Function

    End Class

End Namespace
