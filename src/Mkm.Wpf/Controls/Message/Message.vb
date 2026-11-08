Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media
Imports System.Windows.Media.Effects
Imports System.Windows.Threading
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Controls

    ''' <summary>La gravité d'un <see cref="Message"/> ou d'une notification (<see cref="ToastMessage"/>).</summary>
    Public Enum MessageSeverity
        ''' <summary>Une information (par défaut).</summary>
        Info
        ''' <summary>Une réussite.</summary>
        Success
        ''' <summary>Un avertissement.</summary>
        Warn
        ''' <summary>Une erreur.</summary>
        [Error]
        ''' <summary>Un message secondaire, gris.</summary>
        Secondary
        ''' <summary>Un message contrasté, sombre.</summary>
        Contrast
    End Enum

    ''' <summary>La forme d'un <see cref="Message"/>.</summary>
    Public Enum MessageVariant
        ''' <summary>Un fond pâle de la couleur de la gravité, bordé (par défaut).</summary>
        Filled
        ''' <summary>Sans fond, bordé de la couleur de la gravité, le texte de sa couleur.</summary>
        Outlined
        ''' <summary>Ni fond ni bord ni retrait : le texte seul, de la couleur de la gravité.</summary>
        Simple
    End Enum

    ''' <summary>La taille d'un <see cref="Message"/>.</summary>
    Public Enum MessageSize
        ''' <summary>12 px, 10 px sur 12 de retrait (par défaut).</summary>
        Normal
        ''' <summary>11 px, 6 px sur 10 de retrait.</summary>
        Small
        ''' <summary>13 px, 12 px sur 14 de retrait.</summary>
        Large
    End Enum

    ''' <summary>
    ''' Un message dans la page, comme celui de mkm : un fond pâle de la couleur de sa gravité, un filet un
    ''' peu plus soutenu, des coins de 8 px ; le texte reste gris foncé ; l'icône, s'il y en a une, prend la
    ''' couleur de la gravité assombrie. Un message de plusieurs lignes garde son icône et sa croix sur la
    ''' première.
    ''' <code>&lt;mkm:Message Severity="Warn" Icon="{x:Static mkm:Icons.Warning}" Closable="True"&gt;Le cartouche n'est pas à jour.&lt;/mkm:Message&gt;</code>
    ''' </summary>
    ''' <remarks>Les lecteurs d'écran l'annoncent dès qu'il paraît (rôle « alert » de mkm).</remarks>
    Public Class Message
        Inherits ContentControl

        ''' <summary>Un groupe pour les lecteurs d'écran, qui l'annoncent dès qu'il paraît (LiveSetting) : sans pair, ils ne le voyaient pas.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

        ' Les couleurs et les mesures de la gravité ; une valeur posée par l'application l'emporte.
        Private ReadOnly _habillage As New Habillage(Me)

        ''' <summary>La gravité du message.</summary>
        Public Shared ReadOnly SeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Severity), GetType(MessageSeverity), GetType(Message),
            New FrameworkPropertyMetadata(MessageSeverity.Info, AddressOf OnApparenceChanged))

        ''' <summary>La forme du message.</summary>
        Public Shared ReadOnly VariantProperty As DependencyProperty = DependencyProperty.Register(
            "Variant", GetType(MessageVariant), GetType(Message),
            New FrameworkPropertyMetadata(MessageVariant.Filled, AddressOf OnApparenceChanged))

        ''' <summary>La taille du message.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(MessageSize), GetType(Message),
            New FrameworkPropertyMetadata(MessageSize.Normal, AddressOf OnApparenceChanged))

        ''' <summary>L'icône devant le texte (aucune par défaut).</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(Message), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai si une croix ferme le message.</summary>
        Public Shared ReadOnly ClosableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Closable), GetType(Boolean), GetType(Message), New FrameworkPropertyMetadata(False))

        ''' <summary>Le temps au bout duquel le message se ferme seul ; <see cref="TimeSpan.Zero"/> (par défaut) : jamais.</summary>
        Public Shared ReadOnly LifeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Life), GetType(TimeSpan), GetType(Message), New FrameworkPropertyMetadata(TimeSpan.Zero, AddressOf OnLifeChanged))

        ''' <summary>La couleur de l'icône, celle de la gravité.</summary>
        Public Shared ReadOnly IconForegroundProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IconForeground), GetType(Brush), GetType(Message), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La taille de l'icône, selon la taille du message.</summary>
        Public Shared ReadOnly IconSizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IconSize), GetType(Double), GetType(Message), New FrameworkPropertyMetadata(16.0))

        ''' <summary>La taille du dessin de la croix, selon la taille du message.</summary>
        Public Shared ReadOnly CloseIconSizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(CloseIconSize), GetType(Double), GetType(Message), New FrameworkPropertyMetadata(16.0))

        ''' <summary>Le fond de la croix survolée.</summary>
        Public Shared ReadOnly CloseHoverBackgroundProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(CloseHoverBackground), GetType(Brush), GetType(Message), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'anneau de la croix au clavier.</summary>
        Public Shared ReadOnly FocusRingBrushProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(FocusRingBrush), GetType(Brush), GetType(Message), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'ombre du message (celle du message contrasté).</summary>
        Public Shared ReadOnly ShadowProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Shadow), GetType(Effect), GetType(Message), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Levé quand le message se ferme, par sa croix, <see cref="Close"/> ou la fin de sa vie.</summary>
        Public Shared ReadOnly ClosedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Closed), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(Message))

        Private _minuterie As DispatcherTimer

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Message), New FrameworkPropertyMetadata(GetType(Message)))
            AutomationProperties.LiveSettingProperty.OverrideMetadata(GetType(Message), New FrameworkPropertyMetadata(AutomationLiveSetting.Assertive))
        End Sub

        ''' <summary>Un message d'information, plein, de taille normale.</summary>
        Public Sub New()
            Colorer()
            AddHandler Loaded, AddressOf AuChargement
            AddHandler Unloaded, Sub() Arreter()
            ' Remontré (caché par sa croix ou au bout de sa vie), il est réannoncé et sa vie repart.
            AddHandler IsVisibleChanged, Sub(s, e)
                                             If CBool(e.NewValue) AndAlso IsLoaded Then AuChargement(Me, Nothing)
                                         End Sub
        End Sub

        ''' <summary>La gravité du message.</summary>
        Public Property Severity As MessageSeverity
            Get
                Return DirectCast(GetValue(SeverityProperty), MessageSeverity)
            End Get
            Set(value As MessageSeverity)
                SetValue(SeverityProperty, value)
            End Set
        End Property

        ''' <summary>La forme du message.</summary>
        Public Property [Variant] As MessageVariant
            Get
                Return DirectCast(GetValue(VariantProperty), MessageVariant)
            End Get
            Set(value As MessageVariant)
                SetValue(VariantProperty, value)
            End Set
        End Property

        ''' <summary>La taille du message.</summary>
        Public Property Size As MessageSize
            Get
                Return DirectCast(GetValue(SizeProperty), MessageSize)
            End Get
            Set(value As MessageSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>L'icône devant le texte (aucune par défaut).</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si une croix ferme le message.</summary>
        Public Property Closable As Boolean
            Get
                Return CBool(GetValue(ClosableProperty))
            End Get
            Set(value As Boolean)
                SetValue(ClosableProperty, value)
            End Set
        End Property

        ''' <summary>Le temps au bout duquel le message se ferme seul ; <see cref="TimeSpan.Zero"/> (par défaut) : jamais.</summary>
        Public Property Life As TimeSpan
            Get
                Return DirectCast(GetValue(LifeProperty), TimeSpan)
            End Get
            Set(value As TimeSpan)
                SetValue(LifeProperty, value)
            End Set
        End Property

        ''' <summary>La couleur de l'icône, celle de la gravité.</summary>
        Public Property IconForeground As Brush
            Get
                Return DirectCast(GetValue(IconForegroundProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(IconForegroundProperty, value)
            End Set
        End Property

        ''' <summary>La taille de l'icône, selon la taille du message.</summary>
        Public Property IconSize As Double
            Get
                Return CDbl(GetValue(IconSizeProperty))
            End Get
            Set(value As Double)
                SetValue(IconSizeProperty, value)
            End Set
        End Property

        ''' <summary>La taille du dessin de la croix, selon la taille du message.</summary>
        Public Property CloseIconSize As Double
            Get
                Return CDbl(GetValue(CloseIconSizeProperty))
            End Get
            Set(value As Double)
                SetValue(CloseIconSizeProperty, value)
            End Set
        End Property

        ''' <summary>Le fond de la croix survolée.</summary>
        Public Property CloseHoverBackground As Brush
            Get
                Return DirectCast(GetValue(CloseHoverBackgroundProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(CloseHoverBackgroundProperty, value)
            End Set
        End Property

        ''' <summary>L'anneau de la croix au clavier.</summary>
        Public Property FocusRingBrush As Brush
            Get
                Return DirectCast(GetValue(FocusRingBrushProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(FocusRingBrushProperty, value)
            End Set
        End Property

        ''' <summary>L'ombre du message (celle du message contrasté).</summary>
        Public Property Shadow As Effect
            Get
                Return DirectCast(GetValue(ShadowProperty), Effect)
            End Get
            Set(value As Effect)
                SetValue(ShadowProperty, value)
            End Set
        End Property

        ''' <summary>Levé quand le message se ferme, par sa croix, <see cref="Close"/> ou la fin de sa vie.</summary>
        Public Custom Event Closed As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                Me.AddHandler(ClosedEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                Me.RemoveHandler(ClosedEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Ferme le message : il disparaît de la page et lève <see cref="Closed"/>.</summary>
        Public Sub Close()
            Arreter()
            If Visibility = Visibility.Collapsed Then Return
            Visibility = Visibility.Collapsed
            Me.RaiseEvent(New RoutedEventArgs(ClosedEvent, Me))
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            Dim croix = TryCast(GetTemplateChild("PART_CloseButton"), ButtonBase)
            If croix IsNot Nothing Then AddHandler croix.Click, Sub() Close()
        End Sub

        Private Sub AuChargement(sender As Object, e As RoutedEventArgs)
            Demarrer()
            Dim pair = UIElementAutomationPeer.FromElement(Me)
            If pair Is Nothing Then pair = UIElementAutomationPeer.CreatePeerForElement(Me)
            pair?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)
        End Sub

        Private Sub Demarrer()
            Arreter()
            If Life <= TimeSpan.Zero OrElse Not IsLoaded Then Return
            _minuterie = New DispatcherTimer(DispatcherPriority.Normal, Dispatcher) With {.Interval = Life}
            AddHandler _minuterie.Tick, Sub() Close()
            _minuterie.Start()
        End Sub

        Private Sub Arreter()
            _minuterie?.Stop()
            _minuterie = Nothing
        End Sub

        Private Shared Sub OnLifeChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Message).Demarrer()
        End Sub

        Private Shared Sub OnApparenceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Message).Colorer()
        End Sub

        ' Les couleurs et les mesures, prises aux jetons de mkm selon la gravité, la forme et la taille.
        Private Sub Colorer()
            Dim g = Severity.ToString().ToLowerInvariant()
            Dim t = If(Size = MessageSize.Small, ".sm", If(Size = MessageSize.Large, ".lg", ""))

            Lier(IconForegroundProperty, $"message.{g}.icon.color")
            Lier(CloseHoverBackgroundProperty, $"message.{g}.close.button.hover.background")
            Lier(FocusRingBrushProperty, $"message.{g}.close.button.focus.ring.color")
            Lier(FontSizeProperty, $"message.text{t}.font.size")
            Lier(IconSizeProperty, $"message.icon{t}.size")
            Lier(CloseIconSizeProperty, $"message.close.icon{t}.size")
            Lier(PaddingProperty, $"message.content{t}.padding")

            Select Case [Variant]
                Case MessageVariant.Filled
                    Lier(BackgroundProperty, $"message.{g}.background")
                    Lier(BorderBrushProperty, $"message.{g}.border.color")
                    Lier(ForegroundProperty, $"message.{g}.color")
                    _habillage.Poser(BorderThicknessProperty, New Thickness(Epaisseur("message.border.width")))
                    Lier(ShadowProperty, $"message.{g}.shadow")
                Case MessageVariant.Outlined
                    _habillage.Poser(BackgroundProperty, Brushes.Transparent)
                    Lier(BorderBrushProperty, $"message.{g}.outlined.border.color")
                    Lier(ForegroundProperty, $"message.{g}.outlined.color")
                    _habillage.Poser(BorderThicknessProperty, New Thickness(Epaisseur("message.outlined.border.width")))
                    ClearValue(ShadowProperty)
                Case Else
                    _habillage.Poser(BackgroundProperty, Brushes.Transparent)
                    _habillage.Poser(BorderBrushProperty, Brushes.Transparent)
                    _habillage.Poser(BorderThicknessProperty, New Thickness(0))
                    Lier(ForegroundProperty, $"message.{g}.simple.color")
                    Lier(PaddingProperty, "message.simple.content.padding")
                    ClearValue(ShadowProperty)
            End Select
        End Sub

        ' Une largeur de bord (un nombre) ; la même dans tous les thèmes de mkm, elle est lue une fois.
        Private Function Epaisseur(chemin As String) As Double
            Dim valeur = TryFindResource(ThemeResources.Prefix & chemin)
            Return If(TypeOf valeur Is Double, CDbl(valeur), 1.0)
        End Function

        Private Sub Lier(propriete As DependencyProperty, chemin As String)
            _habillage.Lier(propriete, chemin)
        End Sub

    End Class

End Namespace
