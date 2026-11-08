Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>Où un <see cref="Toast"/> range ses notifications.</summary>
    Public Enum ToastPosition
        ''' <summary>En haut à gauche.</summary>
        TopLeft
        ''' <summary>En haut, au milieu.</summary>
        TopCenter
        ''' <summary>En haut à droite (par défaut).</summary>
        TopRight
        ''' <summary>En bas à gauche.</summary>
        BottomLeft
        ''' <summary>En bas, au milieu.</summary>
        BottomCenter
        ''' <summary>En bas à droite.</summary>
        BottomRight
        ''' <summary>Au milieu.</summary>
        Center
    End Enum

    ''' <summary>Pourquoi une notification s'est fermée.</summary>
    Public Enum ToastCloseReason
        ''' <summary>Par sa croix, ou <see cref="Toast.Remove"/>.</summary>
        Close
        ''' <summary>Au bout de sa vie.</summary>
        LifeEnd
    End Enum

    ''' <summary>La notification qui vient de se fermer, et pourquoi.</summary>
    Public Class ToastClosedEventArgs
        Inherits EventArgs

        ''' <summary>La notification et la raison de sa fermeture.</summary>
        Public Sub New(message As ToastMessage, reason As ToastCloseReason)
            Me.Message = message
            Me.Reason = reason
        End Sub

        ''' <summary>La notification fermée.</summary>
        Public ReadOnly Property Message As ToastMessage

        ''' <summary>Pourquoi elle s'est fermée.</summary>
        Public ReadOnly Property Reason As ToastCloseReason
    End Class

    ''' <summary>
    ''' Les notifications de mkm, comme celles de 3DEXPERIENCE : en haut à droite de la fenêtre, des cartes de
    ''' 250 px les unes sous les autres. Le placer au-dessus du reste de la fenêtre (dernier enfant de la
    ''' grille de la fenêtre, par exemple) ; entre ses cartes, la souris passe à ce qui est dessous.
    ''' <code>
    ''' &lt;mkm:Toast x:Name="Notifications" HighlightDuration="0:0:1" FadeOutDuration="0:0:1" /&gt;
    ''' Notifications.Add(MessageSeverity.Success, "Enregistré", "Le cartouche est à jour.", TimeSpan.FromSeconds(5))
    ''' </code>
    ''' </summary>
    Public Class Toast
        Inherits ItemsControl

        ''' <summary>Tout son arbre visuel pour les lecteurs d'écran et l'automatisation, gabarit compris.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Pane)
        End Function

        ''' <summary>Où ranger les notifications.</summary>
        Public Shared ReadOnly PositionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Position), GetType(ToastPosition), GetType(Toast), New FrameworkPropertyMetadata(ToastPosition.TopRight))

        ''' <summary>Combien de temps une notification paraît en relief à son apparition (aucun par défaut).</summary>
        Public Shared ReadOnly HighlightDurationProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(HighlightDuration), GetType(TimeSpan), GetType(Toast), New FrameworkPropertyMetadata(TimeSpan.Zero))

        ''' <summary>
        ''' Combien de temps une notification s'efface à la fin de sa vie, puis se replie, avant de se fermer
        ''' (aucun par défaut : elle se ferme d'un coup).
        ''' </summary>
        Public Shared ReadOnly FadeOutDurationProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(FadeOutDuration), GetType(TimeSpan), GetType(Toast), New FrameworkPropertyMetadata(TimeSpan.Zero))

        ''' <summary>
        ''' La part de sa vie qu'une notification retrouve quand le pointeur la quitte (entre 0 et 1) ; sans
        ''' valeur (par défaut), ce qui en restait quand il y est entré.
        ''' </summary>
        Public Shared ReadOnly LifeOnLeaveProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(LifeOnLeave), GetType(Double?), GetType(Toast), New FrameworkPropertyMetadata(Nothing))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Toast), New FrameworkPropertyMetadata(GetType(Toast)))
            FocusableProperty.OverrideMetadata(GetType(Toast), New FrameworkPropertyMetadata(False))
        End Sub

        ' La zone qui défile d'une fenêtre zoomée (DefilementDuZoom) : les notifications restent dans sa partie visible.
        Private _zone As DefilementDuZoom

        ''' <summary>Une liste de notifications vide.</summary>
        Public Sub New()
            AddHandler Loaded, Sub() SuivreLaZone(ZoneAutour(Me))
            AddHandler Unloaded, Sub() SuivreLaZone(Nothing)
        End Sub

        ''' <summary>
        ''' Suit la zone qui défile d'une fenêtre zoomée : les notifications se tiennent dans sa partie visible, à leur place
        ''' (en haut à droite…), comme un élément fixe d'une page agrandie dans un navigateur.
        ''' </summary>
        Friend Sub SuivreLaZone(zone As DefilementDuZoom)
            If _zone IsNot Nothing Then RemoveHandler _zone.ScrollChanged, AddressOf Zone_ScrollChanged
            _zone = zone
            If _zone Is Nothing Then
                ClearValue(RenderTransformProperty)
                Return
            End If
            AddHandler _zone.ScrollChanged, AddressOf Zone_ScrollChanged
            Placer()
        End Sub

        Private Shared Function ZoneAutour(element As DependencyObject) As DefilementDuZoom
            Dim d = Media.VisualTreeHelper.GetParent(element)
            While d IsNot Nothing
                Dim zone = TryCast(d, DefilementDuZoom)
                If zone IsNot Nothing Then Return zone
                d = Media.VisualTreeHelper.GetParent(d)
            End While
            Return Nothing
        End Function

        Private Sub Zone_ScrollChanged(sender As Object, e As ScrollChangedEventArgs)
            Placer()
        End Sub

        Private Sub Placer()
            Dim z = _zone
            If z Is Nothing OrElse z.ExtentWidth <= 0 Then Return
            Dim dx As Double
            Select Case HorizontalAlignment
                Case HorizontalAlignment.Left : dx = z.HorizontalOffset
                Case HorizontalAlignment.Center : dx = z.HorizontalOffset + (z.ViewportWidth - z.ExtentWidth) / 2
                Case Else : dx = z.HorizontalOffset + z.ViewportWidth - z.ExtentWidth
            End Select
            Dim dy As Double
            Select Case VerticalAlignment
                Case VerticalAlignment.Bottom : dy = z.VerticalOffset + z.ViewportHeight - z.ExtentHeight
                Case VerticalAlignment.Center : dy = z.VerticalOffset + (z.ViewportHeight - z.ExtentHeight) / 2
                Case Else : dy = z.VerticalOffset
            End Select
            RenderTransform = New Media.TranslateTransform(dx, dy)
        End Sub

        ''' <summary>Levé quand une notification se ferme, par sa croix, <see cref="Remove"/> ou la fin de sa vie.</summary>
        Public Event MessageClosed As EventHandler(Of ToastClosedEventArgs)

        ''' <summary>Où ranger les notifications.</summary>
        Public Property Position As ToastPosition
            Get
                Return DirectCast(GetValue(PositionProperty), ToastPosition)
            End Get
            Set(value As ToastPosition)
                SetValue(PositionProperty, value)
            End Set
        End Property

        ''' <summary>Combien de temps une notification paraît en relief à son apparition (aucun par défaut).</summary>
        Public Property HighlightDuration As TimeSpan
            Get
                Return DirectCast(GetValue(HighlightDurationProperty), TimeSpan)
            End Get
            Set(value As TimeSpan)
                SetValue(HighlightDurationProperty, value)
            End Set
        End Property

        ''' <summary>Combien de temps une notification s'efface à la fin de sa vie, puis se replie, avant de se fermer.</summary>
        Public Property FadeOutDuration As TimeSpan
            Get
                Return DirectCast(GetValue(FadeOutDurationProperty), TimeSpan)
            End Get
            Set(value As TimeSpan)
                SetValue(FadeOutDurationProperty, value)
            End Set
        End Property

        ''' <summary>La part de sa vie qu'une notification retrouve quand le pointeur la quitte (entre 0 et 1).</summary>
        Public Property LifeOnLeave As Double?
            Get
                Return DirectCast(GetValue(LifeOnLeaveProperty), Double?)
            End Get
            Set(value As Double?)
                SetValue(LifeOnLeaveProperty, value)
            End Set
        End Property

        ''' <summary>Montre une notification, après les autres ou, avec <paramref name="prepend"/>, devant elles.</summary>
        Public Sub Add(message As ToastMessage, Optional prepend As Boolean = False)
            If message Is Nothing Then Throw New ArgumentNullException(NameOf(message))
            If prepend Then Items.Insert(0, message) Else Items.Add(message)
        End Sub

        ''' <summary>Montre une notification et la rend ; sans <paramref name="life"/>, elle reste jusqu'à sa croix.</summary>
        Public Function Add(severity As MessageSeverity, summary As String, Optional detail As String = Nothing,
                            Optional life As TimeSpan = Nothing) As ToastMessage
            Dim notification As New ToastMessage With {.Severity = severity, .Summary = summary, .Detail = detail, .Life = life}
            Add(notification)
            Return notification
        End Function

        ''' <summary>Ferme une notification.</summary>
        Public Sub Remove(message As ToastMessage)
            If message Is Nothing Then Throw New ArgumentNullException(NameOf(message))
            If Items.Contains(message) Then Fermer(message, ToastCloseReason.Close)
        End Sub

        ''' <summary>Ferme toutes les notifications.</summary>
        Public Sub RemoveAll()
            For Each notification In Items.OfType(Of ToastMessage)().ToList()
                Fermer(notification, ToastCloseReason.Close)
            Next
        End Sub

        Friend Sub Fermer(message As ToastMessage, raison As ToastCloseReason)
            message.Arreter()
            Items.Remove(message)
            RaiseEvent MessageClosed(Me, New ToastClosedEventArgs(message, raison))
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is ToastMessage
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New ToastMessage()
        End Function

    End Class

End Namespace
