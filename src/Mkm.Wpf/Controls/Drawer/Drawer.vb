Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation

Namespace Mkm.Wpf.Controls

    ''' <summary>Le bord d'où sort un <see cref="Drawer"/>.</summary>
    Public Enum DrawerPosition
        ''' <summary>À gauche (par défaut).</summary>
        Left
        ''' <summary>À droite.</summary>
        Right
        ''' <summary>En haut.</summary>
        Top
        ''' <summary>En bas.</summary>
        Bottom
        ''' <summary>Tout l'espace.</summary>
        Full
    End Enum

    ''' <summary>
    ''' Le tiroir de mkm (Drawer) : un panneau qui sort d'un bord, par-dessus la page assombrie, avec son titre, sa croix,
    ''' son contenu et son pied. Il couvre son conteneur : posé dans la grille racine de la fenêtre, toute la fenêtre.
    ''' Échap, sa croix ou un clic sur le voile le ferment ; le clavier y entre et y tourne.
    ''' <code>
    ''' &lt;mkm:Drawer x:Name="Filtres" Header="Filtres" Position="Right"&gt;…&lt;/mkm:Drawer&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Panel", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_Mask", Type:=GetType(UIElement))>
    <TemplatePart(Name:="PART_Close", Type:=GetType(ButtonBase))>
    Public Class Drawer
        Inherits HeaderedContentControl

        ''' <summary>Vrai quand il est ouvert. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsOpen), GetType(Boolean), GetType(Drawer),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsOpenChanged))

        ''' <summary>Le bord d'où il sort : à gauche par défaut.</summary>
        Public Shared ReadOnly PositionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Position), GetType(DrawerPosition), GetType(Drawer), New FrameworkPropertyMetadata(DrawerPosition.Left, AddressOf OnTailleChanged))

        ''' <summary>
        ''' La largeur du panneau à gauche ou à droite (la section « Size » de mkm) ; sinon celle du thème. En haut ou en bas,
        ''' sa largeur maximale : plus large, le conteneur le montre centré ; plus étroit, sur toute sa largeur.
        ''' </summary>
        Public Shared ReadOnly PanelWidthProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(PanelWidth), GetType(Double), GetType(Drawer), New FrameworkPropertyMetadata(Double.NaN, AddressOf OnTailleChanged))

        ''' <summary>La hauteur du panneau en haut ou en bas ; sinon celle du thème.</summary>
        Public Shared ReadOnly PanelHeightProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(PanelHeight), GetType(Double), GetType(Drawer), New FrameworkPropertyMetadata(Double.NaN, AddressOf OnTailleChanged))

        ''' <summary>
        ''' La largeur du panneau à gauche ou à droite ; en haut ou en bas, sa largeur maximale, centré au-delà ;
        ''' Double.NaN (par défaut) : celle du thème, et toute la largeur en haut ou en bas.
        ''' </summary>
        Public Property PanelWidth As Double
            Get
                Return CDbl(GetValue(PanelWidthProperty))
            End Get
            Set(value As Double)
                SetValue(PanelWidthProperty, value)
            End Set
        End Property

        ''' <summary>La hauteur du panneau en haut ou en bas ; Double.NaN (par défaut) : celle du thème.</summary>
        Public Property PanelHeight As Double
            Get
                Return CDbl(GetValue(PanelHeightProperty))
            End Get
            Set(value As Double)
                SetValue(PanelHeightProperty, value)
            End Set
        End Property

        Private Shared Sub OnTailleChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Drawer).Dimensionner()
        End Sub

        ' La taille choisie par l'application, du seul côté où elle compte ; sinon le gabarit (jetons du thème).
        Private Sub Dimensionner()
            If _panneau Is Nothing Then Return
            Dim lateral = Position = DrawerPosition.Left OrElse Position = DrawerPosition.Right
            Dim horizontal = Position = DrawerPosition.Top OrElse Position = DrawerPosition.Bottom
            If lateral AndAlso Not Double.IsNaN(PanelWidth) Then _panneau.Width = PanelWidth Else _panneau.ClearValue(WidthProperty)
            If horizontal AndAlso Not Double.IsNaN(PanelHeight) Then _panneau.Height = PanelHeight Else _panneau.ClearValue(HeightProperty)
            ' En haut ou en bas, la largeur donnée est un maximum : sur un grand écran le panneau la garde, centré (étiré
            ' jusqu'à son maximum, WPF le centre) ; sur un écran plus étroit, il prend toute la largeur.
            If horizontal AndAlso Not Double.IsNaN(PanelWidth) Then _panneau.MaxWidth = PanelWidth Else _panneau.ClearValue(MaxWidthProperty)
        End Sub

        ''' <summary>Le pied, sous le contenu : des boutons.</summary>
        Public Shared ReadOnly FooterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Footer), GetType(Object), GetType(Drawer), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai (par défaut) pour qu'un clic sur le voile le ferme.</summary>
        Public Shared ReadOnly DismissableMaskProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(DismissableMask), GetType(Boolean), GetType(Drawer), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai (par défaut) pour sa croix dans l'en-tête.</summary>
        Public Shared ReadOnly ShowCloseIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowCloseIcon), GetType(Boolean), GetType(Drawer), New FrameworkPropertyMetadata(True))

        Private _panneau As FrameworkElement
        Private _voile As UIElement
        Private _croix As ButtonBase
        Private _avant As IInputElement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Drawer), New FrameworkPropertyMetadata(GetType(Drawer)))
            FocusableProperty.OverrideMetadata(GetType(Drawer), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand il est ouvert.</summary>
        Public Property IsOpen As Boolean
            Get
                Return CBool(GetValue(IsOpenProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsOpenProperty, value)
            End Set
        End Property

        ''' <summary>Le bord d'où il sort.</summary>
        Public Property Position As DrawerPosition
            Get
                Return DirectCast(GetValue(PositionProperty), DrawerPosition)
            End Get
            Set(value As DrawerPosition)
                SetValue(PositionProperty, value)
            End Set
        End Property

        ''' <summary>Le pied.</summary>
        Public Property Footer As Object
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As Object)
                SetValue(FooterProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour qu'un clic sur le voile le ferme.</summary>
        Public Property DismissableMask As Boolean
            Get
                Return CBool(GetValue(DismissableMaskProperty))
            End Get
            Set(value As Boolean)
                SetValue(DismissableMaskProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour sa croix.</summary>
        Public Property ShowCloseIcon As Boolean
            Get
                Return CBool(GetValue(ShowCloseIconProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowCloseIconProperty, value)
            End Set
        End Property

        ''' <summary>L'ouvre.</summary>
        Public Sub Show()
            SetCurrentValue(IsOpenProperty, True)
        End Sub

        ''' <summary>Le ferme.</summary>
        Public Sub Hide()
            SetCurrentValue(IsOpenProperty, False)
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _voile IsNot Nothing Then RemoveHandler _voile.MouseLeftButtonDown, AddressOf Voile_Clic
            If _croix IsNot Nothing Then RemoveHandler _croix.Click, AddressOf Croix_Click
            MyBase.OnApplyTemplate()
            _panneau = TryCast(GetTemplateChild("PART_Panel"), FrameworkElement)
            Dimensionner()
            _voile = TryCast(GetTemplateChild("PART_Mask"), UIElement)
            _croix = TryCast(GetTemplateChild("PART_Close"), ButtonBase)
            If _voile IsNot Nothing Then AddHandler _voile.MouseLeftButtonDown, AddressOf Voile_Clic
            If _croix IsNot Nothing Then AddHandler _croix.Click, AddressOf Croix_Click
        End Sub

        Private Sub Voile_Clic(sender As Object, e As MouseButtonEventArgs)
            If DismissableMask Then Hide()
            e.Handled = True
        End Sub

        Private Sub Croix_Click(sender As Object, e As RoutedEventArgs)
            Hide()
        End Sub

        Private Shared Sub OnIsOpenChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim tiroir = DirectCast(d, Drawer)
            If CBool(e.NewValue) Then tiroir.Ouvrir() Else tiroir.Fermer()
        End Sub

        ' Ouvert : il glisse de son bord, le clavier y entre (sur son premier élément), comme mkm.
        Private Sub Ouvrir()
            _avant = Keyboard.FocusedElement
            Dispatcher.BeginInvoke(Threading.DispatcherPriority.Loaded, New System.Action(
                Sub()
                    Glisser()
                    _panneau?.MoveFocus(New TraversalRequest(FocusNavigationDirection.First))
                End Sub))
        End Sub

        ' Fermé : le clavier revient où il était.
        Private Sub Fermer()
            If IsKeyboardFocusWithin AndAlso _avant IsNot Nothing Then Keyboard.Focus(_avant)
            _avant = Nothing
        End Sub

        Private Sub Glisser()
            If _panneau Is Nothing OrElse Not SystemParameters.ClientAreaAnimation Then Return
            Dim deplacement As New TranslateTransform()
            _panneau.RenderTransform = deplacement
            Dim depart = If(Position = DrawerPosition.Right, _panneau.ActualWidth,
                         If(Position = DrawerPosition.Left, -_panneau.ActualWidth, 0.0))
            Dim departY = If(Position = DrawerPosition.Bottom, _panneau.ActualHeight,
                          If(Position = DrawerPosition.Top, -_panneau.ActualHeight, 0.0))
            Dim duree = New Duration(TimeSpan.FromMilliseconds(250))
            Dim courbe As New CubicEase With {.EasingMode = EasingMode.EaseOut}
            deplacement.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(depart, 0, duree) With {.EasingFunction = courbe})
            deplacement.BeginAnimation(TranslateTransform.YProperty, New DoubleAnimation(departY, 0, duree) With {.EasingFunction = courbe})
        End Sub

        ''' <summary>Échap le ferme.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse e.Key <> Key.Escape OrElse Not IsOpen Then Return
            Hide()
            e.Handled = True
        End Sub

        ''' <summary>Un volet pour les lecteurs d'écran, nommé par son titre.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Pane, Function() TryCast(Header, String))
        End Function

    End Class

End Namespace
