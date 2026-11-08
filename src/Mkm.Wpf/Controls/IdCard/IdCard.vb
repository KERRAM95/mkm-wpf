Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>La taille du titre d'une <see cref="IdCard"/>.</summary>
    Public Enum IdCardSize
        ''' <summary>16 px (par défaut).</summary>
        Normal
        ''' <summary>14 px.</summary>
        Small
    End Enum

    ''' <summary>
    ''' La carte d'identité d'un objet de mkm (<c>IdCard</c>), celle de 3DEXPERIENCE : sa vignette dans une tuile de
    ''' 72 px, son titre en 16 px semi-gras et sa révision dans une pastille grise, ses boutons (replier, fermer, et
    ''' ceux qu'on ajoute), et dessous ses détails en gris : le propriétaire, le type, la maturité, le reste. Ce qu'elle
    ''' contient (des facettes : un <see cref="Tabs"/> « strip »…) vient sous elle.
    ''' <code>
    ''' &lt;mkm:IdCard Title="Carter" Revision="A" Owner="M. Kerram" Type="Plan" Maturity="In Work"
    '''             MaturityColor="{Binding CouleurDeMaturite}" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' La couleur de la maturité vient de l'application (sa politique) : <see cref="MaturityColor"/> ; le texte prend
    ''' le noir ou le blanc, le plus lisible. Sans couleur, la maturité est une étiquette grise.
    ''' </remarks>
    <TemplatePart(Name:=IdCard.PartCollapseButton, Type:=GetType(ButtonBase))>
    <TemplatePart(Name:=IdCard.PartCloseButton, Type:=GetType(ButtonBase))>
    Public Class IdCard
        Inherits ContentControl

        ''' <summary>Un groupe nommé par son titre pour les lecteurs d'écran, son gabarit compris.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group, Function() Title)
        End Function

        ''' <summary>Le bouton qui replie la carte, dans le gabarit.</summary>
        Public Const PartCollapseButton As String = "PART_CollapseButton"
        ''' <summary>Le bouton qui la ferme, dans le gabarit.</summary>
        Public Const PartCloseButton As String = "PART_CloseButton"

        ''' <summary>Le titre de l'objet.</summary>
        Public Shared ReadOnly TitleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Title), GetType(String), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Sa révision, dans une pastille après le titre.</summary>
        Public Shared ReadOnly RevisionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Revision), GetType(String), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Son type.</summary>
        Public Shared ReadOnly TypeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Type), GetType(String), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'icône de son type (une géométrie d'<see cref="Icons"/> ou une image).</summary>
        Public Shared ReadOnly TypeIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(TypeIcon), GetType(Object), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Sa vignette : une image, tenue dans sa tuile sans être rognée ni agrandie, ou une géométrie.</summary>
        Public Shared ReadOnly ThumbnailProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Thumbnail), GetType(Object), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Son propriétaire, le premier des détails.</summary>
        Public Shared ReadOnly OwnerProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Owner), GetType(String), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le nom de sa maturité.</summary>
        Public Shared ReadOnly MaturityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Maturity), GetType(String), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La couleur de sa maturité, donnée par l'application (sa politique) ; sans elle, une étiquette grise.</summary>
        Public Shared ReadOnly MaturityColorProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MaturityColor), GetType(Brush), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Ce qui s'ajoute aux boutons de la carte, devant eux.</summary>
        Public Shared ReadOnly ActionsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Actions), GetType(Object), GetType(IdCard), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai (par défaut) si un bouton replie la carte.</summary>
        Public Shared ReadOnly CollapsibleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Collapsible), GetType(Boolean), GetType(IdCard), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai quand la carte est réduite à son titre.</summary>
        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsCollapsed), GetType(Boolean), GetType(IdCard),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>Vrai si un bouton ferme la carte (aucun par défaut).</summary>
        Public Shared ReadOnly ClosableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Closable), GetType(Boolean), GetType(IdCard), New FrameworkPropertyMetadata(False))

        ''' <summary>La taille du titre.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(IdCardSize), GetType(IdCard), New FrameworkPropertyMetadata(IdCardSize.Normal))

        ''' <summary>Levé par le bouton qui ferme la carte ; l'application la retire.</summary>
        Public Shared ReadOnly CloseEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            "Close", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(IdCard))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(IdCard), New FrameworkPropertyMetadata(GetType(IdCard)))
            FocusableProperty.OverrideMetadata(GetType(IdCard), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une carte vide ; ses détails ajoutés se lisent après la maturité.</summary>
        Public Sub New()
            Details = New ObservableCollection(Of Object)()
        End Sub

        ''' <summary>Les autres détails, après le propriétaire, le type et la maturité.</summary>
        Public ReadOnly Property Details As ObservableCollection(Of Object)

        ''' <summary>Le titre de l'objet.</summary>
        Public Property Title As String
            Get
                Return CStr(GetValue(TitleProperty))
            End Get
            Set(value As String)
                SetValue(TitleProperty, value)
            End Set
        End Property

        ''' <summary>Sa révision.</summary>
        Public Property Revision As String
            Get
                Return CStr(GetValue(RevisionProperty))
            End Get
            Set(value As String)
                SetValue(RevisionProperty, value)
            End Set
        End Property

        ''' <summary>Son type.</summary>
        Public Property Type As String
            Get
                Return CStr(GetValue(TypeProperty))
            End Get
            Set(value As String)
                SetValue(TypeProperty, value)
            End Set
        End Property

        ''' <summary>L'icône de son type.</summary>
        Public Property TypeIcon As Object
            Get
                Return GetValue(TypeIconProperty)
            End Get
            Set(value As Object)
                SetValue(TypeIconProperty, value)
            End Set
        End Property

        ''' <summary>Sa vignette.</summary>
        Public Property Thumbnail As Object
            Get
                Return GetValue(ThumbnailProperty)
            End Get
            Set(value As Object)
                SetValue(ThumbnailProperty, value)
            End Set
        End Property

        ''' <summary>Son propriétaire.</summary>
        Public Property Owner As String
            Get
                Return CStr(GetValue(OwnerProperty))
            End Get
            Set(value As String)
                SetValue(OwnerProperty, value)
            End Set
        End Property

        ''' <summary>Le nom de sa maturité.</summary>
        Public Property Maturity As String
            Get
                Return CStr(GetValue(MaturityProperty))
            End Get
            Set(value As String)
                SetValue(MaturityProperty, value)
            End Set
        End Property

        ''' <summary>La couleur de sa maturité, donnée par l'application.</summary>
        Public Property MaturityColor As Brush
            Get
                Return DirectCast(GetValue(MaturityColorProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(MaturityColorProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui s'ajoute aux boutons de la carte.</summary>
        Public Property Actions As Object
            Get
                Return GetValue(ActionsProperty)
            End Get
            Set(value As Object)
                SetValue(ActionsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai (par défaut) si un bouton replie la carte.</summary>
        Public Property Collapsible As Boolean
            Get
                Return CBool(GetValue(CollapsibleProperty))
            End Get
            Set(value As Boolean)
                SetValue(CollapsibleProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la carte est réduite à son titre.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si un bouton ferme la carte.</summary>
        Public Property Closable As Boolean
            Get
                Return CBool(GetValue(ClosableProperty))
            End Get
            Set(value As Boolean)
                SetValue(ClosableProperty, value)
            End Set
        End Property

        ''' <summary>La taille du titre.</summary>
        Public Property Size As IdCardSize
            Get
                Return DirectCast(GetValue(SizeProperty), IdCardSize)
            End Get
            Set(value As IdCardSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>Levé par le bouton qui ferme la carte.</summary>
        Public Custom Event Close As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                Me.AddHandler(CloseEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                Me.RemoveHandler(CloseEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            Dim replier = TryCast(GetTemplateChild(PartCollapseButton), ButtonBase)
            If replier IsNot Nothing Then AddHandler replier.Click, Sub() IsCollapsed = Not IsCollapsed
            Dim fermer = TryCast(GetTemplateChild(PartCloseButton), ButtonBase)
            If fermer IsNot Nothing Then AddHandler fermer.Click, Sub() Me.RaiseEvent(New RoutedEventArgs(CloseEvent, Me))
        End Sub

    End Class

End Namespace
