Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Effects
Imports System.Windows.Threading
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une notification de <see cref="Toast"/> : sa gravité, son titre, son message, une croix. Sans icône,
    ''' elle prend celle de sa gravité. Sa vie (<see cref="Life"/>) s'arrête quand le pointeur est dessus.
    ''' </summary>
    ''' <remarks>Les lecteurs d'écran l'annoncent dès qu'elle paraît (rôle « alert » de mkm).</remarks>
    <TemplatePart(Name:=ToastMessage.PartCloseButton, Type:=GetType(ButtonBase))>
    Public Class ToastMessage
        Inherits Control

        ''' <summary>Un groupe pour les lecteurs d'écran, qui l'annoncent dès qu'il paraît (LiveSetting) : sans pair, ils ne le voyaient pas.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

        ' Les couleurs et les mesures de la gravité ; une valeur posée par l'application l'emporte.
        Private ReadOnly _habillage As New Habillage(Me)

        ''' <summary>Le nom de la croix dans le gabarit.</summary>
        Public Const PartCloseButton As String = "PART_CloseButton"

        ''' <summary>La gravité de la notification.</summary>
        Public Shared ReadOnly SeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Severity), GetType(MessageSeverity), GetType(ToastMessage),
            New FrameworkPropertyMetadata(MessageSeverity.Info, AddressOf OnApparenceChanged))

        ''' <summary>Le titre, en gras ; vide, il ne se montre pas.</summary>
        Public Shared ReadOnly SummaryProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Summary), GetType(String), GetType(ToastMessage), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le message, sous le titre ; un texte, ou ce qu'on veut.</summary>
        Public Shared ReadOnly DetailProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Detail), GetType(Object), GetType(ToastMessage), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'icône ; sans elle, celle de la gravité.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(ToastMessage), New FrameworkPropertyMetadata(Nothing, AddressOf OnApparenceChanged))

        ''' <summary>Le temps au bout duquel la notification se ferme seule ; <see cref="TimeSpan.Zero"/> (par défaut) : jamais.</summary>
        Public Shared ReadOnly LifeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Life), GetType(TimeSpan), GetType(ToastMessage), New FrameworkPropertyMetadata(TimeSpan.Zero))

        ''' <summary>Vrai (par défaut) si une croix ferme la notification.</summary>
        Public Shared ReadOnly ClosableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Closable), GetType(Boolean), GetType(ToastMessage), New FrameworkPropertyMetadata(True))

        Private Shared ReadOnly DisplayIconPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(DisplayIcon), GetType(Object), GetType(ToastMessage), New FrameworkPropertyMetadata(Nothing))
        ''' <summary>L'icône montrée : <see cref="Icon"/>, ou celle de la gravité.</summary>
        Public Shared ReadOnly DisplayIconProperty As DependencyProperty = DisplayIconPropertyKey.DependencyProperty

        Private Shared ReadOnly IsHighlightedPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsHighlighted), GetType(Boolean), GetType(ToastMessage), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand la notification paraît en relief : à son apparition, et sous le pointeur.</summary>
        Public Shared ReadOnly IsHighlightedProperty As DependencyProperty = IsHighlightedPropertyKey.DependencyProperty

        ''' <summary>La couleur de la gravité : la bande de gauche.</summary>
        Public Shared ReadOnly BandBrushProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(BandBrush), GetType(Brush), GetType(ToastMessage), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La couleur de l'icône.</summary>
        Public Shared ReadOnly IconForegroundProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IconForeground), GetType(Brush), GetType(ToastMessage), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La couleur du message, sous le titre.</summary>
        Public Shared ReadOnly DetailForegroundProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(DetailForeground), GetType(Brush), GetType(ToastMessage), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'ombre de la carte.</summary>
        Public Shared ReadOnly ShadowProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Shadow), GetType(Effect), GetType(ToastMessage), New FrameworkPropertyMetadata(Nothing))

        Private _minuterie As DispatcherTimer
        Private _relief As DispatcherTimer
        Private _reste As TimeSpan
        Private _depart As DateTime?

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ToastMessage), New FrameworkPropertyMetadata(GetType(ToastMessage)))
            FocusableProperty.OverrideMetadata(GetType(ToastMessage), New FrameworkPropertyMetadata(False))
            AutomationProperties.LiveSettingProperty.OverrideMetadata(GetType(ToastMessage), New FrameworkPropertyMetadata(AutomationLiveSetting.Assertive))
        End Sub

        ''' <summary>Une notification d'information.</summary>
        Public Sub New()
            Colorer()
            AddHandler Loaded, AddressOf AuChargement
        End Sub

        ''' <summary>La gravité de la notification.</summary>
        Public Property Severity As MessageSeverity
            Get
                Return DirectCast(GetValue(SeverityProperty), MessageSeverity)
            End Get
            Set(value As MessageSeverity)
                SetValue(SeverityProperty, value)
            End Set
        End Property

        ''' <summary>Le titre, en gras ; vide, il ne se montre pas.</summary>
        Public Property Summary As String
            Get
                Return CStr(GetValue(SummaryProperty))
            End Get
            Set(value As String)
                SetValue(SummaryProperty, value)
            End Set
        End Property

        ''' <summary>Le message, sous le titre ; un texte, ou ce qu'on veut.</summary>
        Public Property Detail As Object
            Get
                Return GetValue(DetailProperty)
            End Get
            Set(value As Object)
                SetValue(DetailProperty, value)
            End Set
        End Property

        ''' <summary>L'icône ; sans elle, celle de la gravité.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Le temps au bout duquel la notification se ferme seule ; <see cref="TimeSpan.Zero"/> (par défaut) : jamais.</summary>
        Public Property Life As TimeSpan
            Get
                Return DirectCast(GetValue(LifeProperty), TimeSpan)
            End Get
            Set(value As TimeSpan)
                SetValue(LifeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai (par défaut) si une croix ferme la notification.</summary>
        Public Property Closable As Boolean
            Get
                Return CBool(GetValue(ClosableProperty))
            End Get
            Set(value As Boolean)
                SetValue(ClosableProperty, value)
            End Set
        End Property

        ''' <summary>L'icône montrée : <see cref="Icon"/>, ou celle de la gravité.</summary>
        Public ReadOnly Property DisplayIcon As Object
            Get
                Return GetValue(DisplayIconProperty)
            End Get
        End Property

        ''' <summary>Vrai quand la notification paraît en relief : à son apparition, et sous le pointeur.</summary>
        Public ReadOnly Property IsHighlighted As Boolean
            Get
                Return CBool(GetValue(IsHighlightedProperty))
            End Get
        End Property

        ''' <summary>La couleur de la gravité : la bande de gauche.</summary>
        Public Property BandBrush As Brush
            Get
                Return DirectCast(GetValue(BandBrushProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(BandBrushProperty, value)
            End Set
        End Property

        ''' <summary>La couleur de l'icône.</summary>
        Public Property IconForeground As Brush
            Get
                Return DirectCast(GetValue(IconForegroundProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(IconForegroundProperty, value)
            End Set
        End Property

        ''' <summary>La couleur du message, sous le titre.</summary>
        Public Property DetailForeground As Brush
            Get
                Return DirectCast(GetValue(DetailForegroundProperty), Brush)
            End Get
            Set(value As Brush)
                SetValue(DetailForegroundProperty, value)
            End Set
        End Property

        ''' <summary>L'ombre de la carte.</summary>
        Public Property Shadow As Effect
            Get
                Return DirectCast(GetValue(ShadowProperty), Effect)
            End Get
            Set(value As Effect)
                SetValue(ShadowProperty, value)
            End Set
        End Property

        Private ReadOnly Property Hote As Toast
            Get
                Return TryCast(ItemsControl.ItemsControlFromItemContainer(Me), Toast)
            End Get
        End Property

        ''' <summary>Ferme la notification, comme sa croix.</summary>
        Public Sub Close()
            Fermer(ToastCloseReason.Close)
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            Dim croix = TryCast(GetTemplateChild(PartCloseButton), ButtonBase)
            If croix IsNot Nothing Then AddHandler croix.Click, Sub() Close()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseEnter(e As MouseEventArgs)
            MyBase.OnMouseEnter(e)
            SetValue(IsHighlightedPropertyKey, True)
            If Life <= TimeSpan.Zero Then Return
            ' Sa vie s'arrête ; une notification qui s'effaçait ou se repliait revient.
            If _depart.HasValue Then _reste -= DateTime.Now - _depart.Value
            _depart = Nothing
            ArreterLaVie()
            Revenir()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeave(e As MouseEventArgs)
            MyBase.OnMouseLeave(e)
            SetValue(IsHighlightedPropertyKey, False)
            ' Fermée sous le pointeur, la notification reçoit encore la sortie de la souris : elle ne revit pas.
            If Hote Is Nothing OrElse Life <= TimeSpan.Zero Then Return
            Dim part = Hote?.LifeOnLeave
            If part.HasValue Then _reste = TimeSpan.FromTicks(CLng(Life.Ticks * part.Value))
            Vivre()
        End Sub

        Friend Sub Arreter()
            ArreterLaVie()
            _relief?.Stop()
            _relief = Nothing
        End Sub

        Private Sub AuChargement(sender As Object, e As RoutedEventArgs)
            RemoveHandler Loaded, AddressOf AuChargement
            If Life > TimeSpan.Zero Then
                _reste = Life
                Vivre()
            End If

            ' En relief le temps demandé, même si le pointeur est dessus quand ce temps s'achève.
            Dim relief = If(Hote?.HighlightDuration, TimeSpan.Zero)
            If relief > TimeSpan.Zero Then
                SetValue(IsHighlightedPropertyKey, True)
                _relief = Minuterie(relief, Sub()
                                                _relief = Nothing
                                                SetValue(IsHighlightedPropertyKey, False)
                                            End Sub)
            End If

            Dim pair = UIElementAutomationPeer.FromElement(Me)
            If pair Is Nothing Then pair = UIElementAutomationPeer.CreatePeerForElement(Me)
            pair?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)
        End Sub

        ' Le reste de sa vie ; avec un fondu, elle s'efface pendant la fin de sa vie, puis se replie autant de
        ' temps avant de se fermer.
        Private Sub Vivre()
            ArreterLaVie()
            _depart = DateTime.Now
            Dim fondu = If(Hote?.FadeOutDuration, TimeSpan.Zero)
            If fondu <= TimeSpan.Zero Then
                _minuterie = Minuterie(_reste, Sub() Fermer(ToastCloseReason.LifeEnd))
                Return
            End If

            Dim avant = If(_reste > fondu, _reste - fondu, TimeSpan.Zero)
            _minuterie = Minuterie(avant, Sub()
                                              Effacer(fondu)
                                              _minuterie = Minuterie(fondu, Sub()
                                                                                Replier(fondu)
                                                                                _minuterie = Minuterie(fondu, Sub() Fermer(ToastCloseReason.LifeEnd))
                                                                            End Sub)
                                          End Sub)
        End Sub

        Private Sub ArreterLaVie()
            _minuterie?.Stop()
            _minuterie = Nothing
        End Sub

        Private Function Minuterie(duree As TimeSpan, action As Action) As DispatcherTimer
            Dim m As New DispatcherTimer(DispatcherPriority.Normal, Dispatcher) With {
                .Interval = If(duree > TimeSpan.Zero, duree, TimeSpan.FromMilliseconds(1))
            }
            AddHandler m.Tick, Sub()
                                   m.Stop()
                                   action()
                               End Sub
            m.Start()
            Return m
        End Function

        Private Sub Effacer(duree As TimeSpan)
            BeginAnimation(OpacityProperty, New DoubleAnimation(0, New Duration(duree)))
        End Sub

        Private Sub Replier(duree As TimeSpan)
            BeginAnimation(MaxHeightProperty, New DoubleAnimation(ActualHeight, 0, New Duration(duree)))
            BeginAnimation(MarginProperty, New ThicknessAnimation(New Thickness(Margin.Left, 0, Margin.Right, 0), New Duration(duree)))
        End Sub

        Private Sub Revenir()
            BeginAnimation(OpacityProperty, Nothing)
            BeginAnimation(MaxHeightProperty, Nothing)
            BeginAnimation(MarginProperty, Nothing)
        End Sub

        Private Sub Fermer(raison As ToastCloseReason)
            Arreter()
            Dim conteneur = Hote
            If conteneur IsNot Nothing Then
                conteneur.Fermer(Me, raison)
            Else
                Visibility = Visibility.Collapsed
            End If
        End Sub

        Private Shared Sub OnApparenceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, ToastMessage).Colorer()
        End Sub

        ' Les couleurs de la gravité, prises aux jetons de mkm ; l'icône et la bande de la couleur de la gravité,
        ' sauf pour les notifications secondaire et contrastée, sans bande, l'icône de la couleur du texte.
        Private Sub Colorer()
            Dim g = Severity.ToString().ToLowerInvariant()
            Lier(BackgroundProperty, $"toast.{g}.background")
            Lier(ForegroundProperty, $"toast.{g}.color")
            Lier(DetailForegroundProperty, $"toast.{g}.detail.color")
            Lier(ShadowProperty, $"toast.{g}.shadow")

            Dim icone As Object = Nothing
            Select Case Severity
                Case MessageSeverity.Info : icone = Icons.Info
                Case MessageSeverity.Success : icone = Icons.Check
                Case MessageSeverity.Warn : icone = Icons.Warning
                Case MessageSeverity.Error : icone = Icons.Error
            End Select
            If icone IsNot Nothing Then
                Lier(BandBrushProperty, $"toast.{g}.border.color")
                Lier(IconForegroundProperty, $"toast.{g}.border.color")
            Else
                _habillage.Poser(BandBrushProperty, Brushes.Transparent)
                Lier(IconForegroundProperty, $"toast.{g}.color")
            End If
            SetValue(DisplayIconPropertyKey, If(Icon, icone))
        End Sub

        Private Sub Lier(propriete As DependencyProperty, chemin As String)
            _habillage.Lier(propriete, chemin)
        End Sub

    End Class

End Namespace
