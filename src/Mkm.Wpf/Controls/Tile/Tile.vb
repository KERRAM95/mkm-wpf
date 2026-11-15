Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La tuile d'un objet dans une liste, celle des résultats de recherche de 3DEXPERIENCE : 70 px de haut, sa vignette
    ''' à gauche dans une zone grise de 79 × 58, puis l'icône de son type et son titre en bleu, une ligne de sous-titre
    ''' en gris (type, révision, état, propriétaire…), une ligne de description en gras (son identifiant), et au bord
    ''' droit le chevron de son menu. Un liseré de couleur, à gauche, peut dire son état.
    ''' <code>
    ''' &lt;mkm:Tile Thumbnail="{Binding Vignette}" TypeIcon="{Binding IconeDuType}" Title="Carter"
    '''           Subtitle="Article | A | En cours | M. Kerram" Description="prd-00042"
    '''           Stripe="{Binding CouleurDeLEtat}" HasMenu="True" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' La tuile ne prend pas le clavier : la ligne de la liste qui la porte le prend, et lie <see cref="IsSelected"/> à
    ''' son choix. Le liseré est la politique de l'application (<see cref="Stripe"/>), aucun par défaut.
    ''' </remarks>
    <TemplatePart(Name:=Tile.PartMenuButton, Type:=GetType(ButtonBase))>
    Public Class Tile
        Inherits Control

        ''' <summary>Le chevron qui ouvre le menu de la tuile, dans le gabarit.</summary>
        Public Const PartMenuButton As String = "PART_MenuButton"

        ''' <summary>Sa vignette : une image, tenue dans sa zone sans être rognée ni agrandie, ou une géométrie.</summary>
        Public Shared ReadOnly ThumbnailProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Thumbnail), GetType(Object), GetType(Tile), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'icône de son type, devant le titre (une image ou une géométrie).</summary>
        Public Shared ReadOnly TypeIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(TypeIcon), GetType(Object), GetType(Tile), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Son titre, sur une ligne.</summary>
        Public Shared ReadOnly TitleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Title), GetType(String), GetType(Tile), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Son sous-titre : un texte, ou un élément (un TextBlock dont un Run prend la couleur d'un lien).</summary>
        Public Shared ReadOnly SubtitleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Subtitle), GetType(Object), GetType(Tile), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Sa description, en gras, sur une ligne (souvent son identifiant).</summary>
        Public Shared ReadOnly DescriptionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Description), GetType(String), GetType(Tile), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La couleur de son liseré, à gauche, donnée par l'application (sa politique) ; aucun liseré sans elle.</summary>
        Public Shared ReadOnly StripeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Stripe), GetType(Brush), GetType(Tile), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai si le chevron de son menu est là (aucun par défaut).</summary>
        Public Shared ReadOnly HasMenuProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(HasMenu), GetType(Boolean), GetType(Tile), New FrameworkPropertyMetadata(False))

        ''' <summary>Le nom du chevron pour un lecteur d'écran (par défaut « Actions », <c>tile.menu</c>).</summary>
        Public Shared ReadOnly MenuLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MenuLabel), GetType(String), GetType(Tile), New FrameworkPropertyMetadata(Nothing, AddressOf NomDuMenuChange))

        ''' <summary>Vrai quand la tuile est choisie : elle est cernée.</summary>
        Public Shared ReadOnly IsSelectedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsSelected), GetType(Boolean), GetType(Tile),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>
        ''' Vrai pendant que ses valeurs et ses images se lisent : la tuile montre un squelette animé (<see cref="Skeleton"/>)
        ''' à la place de sa vignette et de ses trois lignes, sans chevron ni liseré.
        ''' </summary>
        Public Shared ReadOnly IsLoadingProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsLoading), GetType(Boolean), GetType(Tile), New FrameworkPropertyMetadata(False))

        ''' <summary>
        ''' Levé par le chevron. Non traité, le <see cref="FrameworkElement.ContextMenu"/> de la tuile s'ouvre sous lui ;
        ''' l'application qui ouvre autre chose (un <see cref="Popover"/>) le traite.
        ''' </summary>
        Public Shared ReadOnly MenuRequestedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            "MenuRequested", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(Tile))

        Private _chevron As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Tile), New FrameworkPropertyMetadata(GetType(Tile)))
            FocusableProperty.OverrideMetadata(GetType(Tile), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une tuile vide.</summary>
        Public Sub New()
            AddHandler Loaded, Sub() AddHandler Localizer.Current.LanguageChanged, AddressOf Langue
            AddHandler Unloaded, Sub() RemoveHandler Localizer.Current.LanguageChanged, AddressOf Langue
        End Sub

        ''' <summary>Un groupe nommé par son titre pour les lecteurs d'écran (« Chargement » pendant qu'elle se lit), son gabarit compris.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group,
                                       Function() If(IsLoading, Localizer.Current.T("aria.busy"), Title))
        End Function

        ''' <summary>Relie le chevron du gabarit.</summary>
        Public Overrides Sub OnApplyTemplate()
            If _chevron IsNot Nothing Then RemoveHandler _chevron.Click, AddressOf Chevron_Click
            MyBase.OnApplyTemplate()
            _chevron = TryCast(GetTemplateChild(PartMenuButton), ButtonBase)
            If _chevron IsNot Nothing Then AddHandler _chevron.Click, AddressOf Chevron_Click
            NommerLeChevron()
        End Sub

        Private Sub Chevron_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            Dim demande As New RoutedEventArgs(MenuRequestedEvent, Me)
            Me.RaiseEvent(demande)
            If demande.Handled OrElse ContextMenu Is Nothing Then Return
            ContextMenu.PlacementTarget = _chevron
            ContextMenu.Placement = PlacementMode.Bottom
            ContextMenu.IsOpen = True
        End Sub

        Private Shared Sub NomDuMenuChange(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Tile).NommerLeChevron()
        End Sub

        Private Sub Langue(sender As Object, e As EventArgs)
            NommerLeChevron()
        End Sub

        Private Sub NommerLeChevron()
            If _chevron Is Nothing Then Return
            AutomationProperties.SetName(_chevron, If(MenuLabel, Localizer.Current.T("tile.menu")))
        End Sub

        ''' <summary>Levé par le chevron ; non traité, le menu contextuel de la tuile s'ouvre sous lui.</summary>
        Public Custom Event MenuRequested As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                Me.AddHandler(MenuRequestedEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                Me.RemoveHandler(MenuRequestedEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Sa vignette.</summary>
        Public Property Thumbnail As Object
            Get
                Return GetValue(ThumbnailProperty)
            End Get
            Set(value As Object)
                SetValue(ThumbnailProperty, value)
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

        ''' <summary>Son titre.</summary>
        Public Property Title As String
            Get
                Return CStr(GetValue(TitleProperty))
            End Get
            Set(value As String)
                SetValue(TitleProperty, value)
            End Set
        End Property

        ''' <summary>Son sous-titre.</summary>
        Public Property Subtitle As Object
            Get
                Return GetValue(SubtitleProperty)
            End Get
            Set(value As Object)
                SetValue(SubtitleProperty, value)
            End Set
        End Property

        ''' <summary>Sa description.</summary>
        Public Property Description As String
            Get
                Return CStr(GetValue(DescriptionProperty))
            End Get
            Set(value As String)
                SetValue(DescriptionProperty, value)
            End Set
        End Property

        ''' <summary>La couleur de son liseré.</summary>
        Public Property Stripe As Brush
            Get
                Return DirectCast(GetValue(StripeProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(StripeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le chevron de son menu est là.</summary>
        Public Property HasMenu As Boolean
            Get
                Return CBool(GetValue(HasMenuProperty))
            End Get
            Set(value As Boolean)
                SetValue(HasMenuProperty, value)
            End Set
        End Property

        ''' <summary>Le nom du chevron pour un lecteur d'écran.</summary>
        Public Property MenuLabel As String
            Get
                Return CStr(GetValue(MenuLabelProperty))
            End Get
            Set(value As String)
                SetValue(MenuLabelProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pendant que ses valeurs et ses images se lisent.</summary>
        Public Property IsLoading As Boolean
            Get
                Return CBool(GetValue(IsLoadingProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsLoadingProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la tuile est choisie.</summary>
        Public Property IsSelected As Boolean
            Get
                Return CBool(GetValue(IsSelectedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsSelectedProperty, value)
            End Set
        End Property

    End Class

End Namespace
