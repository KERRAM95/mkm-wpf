Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media.Animation

Namespace Mkm.Wpf.Controls

    ''' <summary>Un panneau de côté d'un <see cref="Triptych"/>.</summary>
    Public Enum TriptychSide
        ''' <summary>Le panneau de gauche.</summary>
        Left
        ''' <summary>Le panneau de droite.</summary>
        Right
    End Enum

    ''' <summary>
    ''' La mise en page à trois panneaux de mkm (<c>Triptych</c>), celle des applications 3DEXPERIENCE : un panneau de
    ''' gauche (<see cref="Left"/>), le panneau principal (le contenu) et un panneau de droite (<see cref="Right"/>). Les
    ''' panneaux de côté s'ouvrent et se ferment en glissant ; celui de gauche se replie en une bande
    ''' (<see cref="LeftCollapsed"/>) ; une poignée invisible de 10 px les élargit ; sous une largeur
    ''' (<see cref="Breakpoint"/>), ils recouvrent le panneau principal, un voile dessous, au lieu de le pousser.
    ''' <code>
    ''' &lt;mkm:Triptych LeftVisible="True" RightVisible="{Binding ProprietesOuvertes}" ShowLeftToggle="True"&gt;
    '''     &lt;mkm:Triptych.Left&gt;&lt;mkm:WelcomePanel … /&gt;&lt;/mkm:Triptych.Left&gt;
    '''     &lt;mkm:DataTable … /&gt;
    '''     &lt;mkm:Triptych.Right&gt;&lt;mkm:PropertyPanel … /&gt;&lt;/mkm:Triptych.Right&gt;
    ''' &lt;/mkm:Triptych&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Les tailles sont en pixels. Le panneau principal garde toujours 100 px (150 entre deux panneaux ouverts) : une
    ''' poignée ne peut pas le cacher. En superposition, un seul panneau de côté à la fois : ouvrir l'un ferme l'autre ;
    ''' le voile (<see cref="Dismissable"/>) et Échap (<see cref="CloseOnEscape"/>) le ferment ; le panneau de droite que la
    ''' superposition a fermé revient avec la place.
    ''' </remarks>
    <TemplatePart(Name:="PART_Left", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_Right", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_Main", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_Mask", Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:="PART_LeftGutter", Type:=GetType(TriptychGutter))>
    <TemplatePart(Name:="PART_RightGutter", Type:=GetType(TriptychGutter))>
    <TemplatePart(Name:="PART_LeftToggle", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_LeftClose", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_RightClose", Type:=GetType(ButtonBase))>
    Public Class Triptych
        Inherits ContentControl

        ' Le panneau principal garde toujours cette place quand un seul panneau est ouvert, et un peu plus quand les deux
        ' le sont (mkm).
        Private Const PlaceDuPrincipal As Double = 100
        Private Const PlaceDuPrincipalEntreDeux As Double = 150

        ''' <summary>Le contenu du panneau de gauche.</summary>
        Public Shared ReadOnly LeftProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Left), GetType(Object), GetType(Triptych), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le contenu du panneau de droite.</summary>
        Public Shared ReadOnly RightProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Right), GetType(Object), GetType(Triptych), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai quand le panneau de gauche est ouvert.</summary>
        Public Shared ReadOnly LeftVisibleProperty As DependencyProperty = Registre(NameOf(LeftVisible), GetType(Boolean), False, True)

        ''' <summary>Vrai quand le panneau de droite est ouvert.</summary>
        Public Shared ReadOnly RightVisibleProperty As DependencyProperty = Registre(NameOf(RightVisible), GetType(Boolean), False, True)

        ''' <summary>Vrai quand le panneau de gauche est replié en une bande (<see cref="LeftCollapsedSize"/>).</summary>
        Public Shared ReadOnly LeftCollapsedProperty As DependencyProperty = Registre(NameOf(LeftCollapsed), GetType(Boolean), False, True)

        ''' <summary>La largeur du panneau de gauche ouvert, en pixels (300).</summary>
        Public Shared ReadOnly LeftSizeProperty As DependencyProperty = Registre(NameOf(LeftSize), GetType(Double), 300.0, True)

        ''' <summary>La largeur de la bande du panneau de gauche replié (40).</summary>
        Public Shared ReadOnly LeftCollapsedSizeProperty As DependencyProperty = Registre(NameOf(LeftCollapsedSize), GetType(Double), 40.0)

        ''' <summary>La largeur minimale du panneau de gauche (40).</summary>
        Public Shared ReadOnly LeftMinSizeProperty As DependencyProperty = Registre(NameOf(LeftMinSize), GetType(Double), 40.0)

        ''' <summary>La largeur du panneau de droite ouvert, en pixels (300).</summary>
        Public Shared ReadOnly RightSizeProperty As DependencyProperty = Registre(NameOf(RightSize), GetType(Double), 300.0, True)

        ''' <summary>La largeur minimale du panneau de droite (250).</summary>
        Public Shared ReadOnly RightMinSizeProperty As DependencyProperty = Registre(NameOf(RightMinSize), GetType(Double), 250.0)

        ''' <summary>Vrai si une poignée élargit le panneau de gauche (faux par défaut).</summary>
        Public Shared ReadOnly LeftResizableProperty As DependencyProperty = Registre(NameOf(LeftResizable), GetType(Boolean), False)

        ''' <summary>Vrai si une poignée élargit le panneau de droite (vrai par défaut).</summary>
        Public Shared ReadOnly RightResizableProperty As DependencyProperty = Registre(NameOf(RightResizable), GetType(Boolean), True)

        ''' <summary>Le pas d'une flèche du clavier sur une poignée, en pixels (10).</summary>
        Public Shared ReadOnly StepProperty As DependencyProperty = Registre(NameOf([Step]), GetType(Double), 10.0)

        ''' <summary>Vrai si le panneau de gauche a un bouton de fermeture.</summary>
        Public Shared ReadOnly ShowLeftCloseIconProperty As DependencyProperty = Registre(NameOf(ShowLeftCloseIcon), GetType(Boolean), False)

        ''' <summary>Vrai si le panneau de droite a un bouton de fermeture.</summary>
        Public Shared ReadOnly ShowRightCloseIconProperty As DependencyProperty = Registre(NameOf(ShowRightCloseIcon), GetType(Boolean), False)

        ''' <summary>Vrai si une languette, en haut à gauche du panneau principal, ouvre, replie et déplie le panneau de gauche.</summary>
        Public Shared ReadOnly ShowLeftToggleProperty As DependencyProperty = Registre(NameOf(ShowLeftToggle), GetType(Boolean), False)

        ''' <summary>La largeur sous laquelle les panneaux de côté recouvrent le panneau principal (550).</summary>
        Public Shared ReadOnly BreakpointProperty As DependencyProperty = Registre(NameOf(Breakpoint), GetType(Double), 550.0)

        ''' <summary>La part de la largeur que prend un panneau de côté en superposition (0,9).</summary>
        Public Shared ReadOnly OverlaySizeProperty As DependencyProperty = Registre(NameOf(OverlaySize), GetType(Double), 0.9)

        ''' <summary>Vrai (par défaut) si un clic sur le voile ferme le panneau ouvert en superposition.</summary>
        Public Shared ReadOnly DismissableProperty As DependencyProperty = Registre(NameOf(Dismissable), GetType(Boolean), True)

        ''' <summary>Vrai (par défaut) si Échap ferme le panneau ouvert en superposition.</summary>
        Public Shared ReadOnly CloseOnEscapeProperty As DependencyProperty = Registre(NameOf(CloseOnEscape), GetType(Boolean), True)

        ''' <summary>Vrai si le panneau de droite ouvert couvre d'un voile le reste de la mise en page.</summary>
        Public Shared ReadOnly ModalProperty As DependencyProperty = Registre(NameOf(Modal), GetType(Boolean), False)

        Private Shared ReadOnly IsOverlayPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsOverlay), GetType(Boolean), GetType(Triptych), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai sous <see cref="Breakpoint"/> : les panneaux de côté recouvrent le panneau principal.</summary>
        Public Shared ReadOnly IsOverlayProperty As DependencyProperty = IsOverlayPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLeftExpandedPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLeftExpanded), GetType(Boolean), GetType(Triptych), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le panneau de gauche est ouvert et déplié : la languette montre la flèche qui le replie.</summary>
        Public Shared ReadOnly IsLeftExpandedProperty As DependencyProperty = IsLeftExpandedPropertyKey.DependencyProperty

        Private _gauche As FrameworkElement
        Private _droite As FrameworkElement
        Private _principal As FrameworkElement
        Private _voile As FrameworkElement
        Private _poigneeGauche As TriptychGutter
        Private _poigneeDroite As TriptychGutter
        Private _languette As ButtonBase
        Private _fermerGauche As ButtonBase
        Private _fermerDroite As ButtonBase
        Private _droiteFermeeParLaPlace As Boolean
        Private ReadOnly _ouvreurs As New System.Collections.Generic.Dictionary(Of TriptychSide, IInputElement)
        Private _tire As Boolean

        ''' <summary>Quand la mise en page passe en superposition, ou en sort.</summary>
        Public Event ModeChange As EventHandler

        ''' <summary>Après qu'une poignée a été tirée (à la souris ou au clavier).</summary>
        Public Event ResizeEnd As EventHandler

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Triptych), New FrameworkPropertyMetadata(GetType(Triptych)))
            FocusableProperty.OverrideMetadata(GetType(Triptych), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une mise en page, ses panneaux de côté fermés.</summary>
        Public Sub New()
            AddHandler SizeChanged, Sub(s, e) MettreAJourLeMode(e.NewSize.Width)
        End Sub

        Private Shared Function Registre(nom As String, genre As Type, defaut As Object, Optional deuxSens As Boolean = False) As DependencyProperty
            Dim options = FrameworkPropertyMetadataOptions.None
            If deuxSens Then options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            Return DependencyProperty.Register(nom, genre, GetType(Triptych), New FrameworkPropertyMetadata(defaut, options, AddressOf OnGeometrieChanged))
        End Function

        ''' <summary>Le contenu du panneau de gauche.</summary>
        Public Property Left As Object
            Get
                Return GetValue(LeftProperty)
            End Get
            Set(value As Object)
                SetValue(LeftProperty, value)
            End Set
        End Property

        ''' <summary>Le contenu du panneau de droite.</summary>
        Public Property Right As Object
            Get
                Return GetValue(RightProperty)
            End Get
            Set(value As Object)
                SetValue(RightProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le panneau de gauche est ouvert.</summary>
        Public Property LeftVisible As Boolean
            Get
                Return CBool(GetValue(LeftVisibleProperty))
            End Get
            Set(value As Boolean)
                SetValue(LeftVisibleProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le panneau de droite est ouvert.</summary>
        Public Property RightVisible As Boolean
            Get
                Return CBool(GetValue(RightVisibleProperty))
            End Get
            Set(value As Boolean)
                SetValue(RightVisibleProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le panneau de gauche est replié en une bande.</summary>
        Public Property LeftCollapsed As Boolean
            Get
                Return CBool(GetValue(LeftCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(LeftCollapsedProperty, value)
            End Set
        End Property

        ''' <summary>La largeur du panneau de gauche ouvert, en pixels.</summary>
        Public Property LeftSize As Double
            Get
                Return CDbl(GetValue(LeftSizeProperty))
            End Get
            Set(value As Double)
                SetValue(LeftSizeProperty, value)
            End Set
        End Property

        ''' <summary>La largeur de la bande du panneau de gauche replié.</summary>
        Public Property LeftCollapsedSize As Double
            Get
                Return CDbl(GetValue(LeftCollapsedSizeProperty))
            End Get
            Set(value As Double)
                SetValue(LeftCollapsedSizeProperty, value)
            End Set
        End Property

        ''' <summary>La largeur minimale du panneau de gauche.</summary>
        Public Property LeftMinSize As Double
            Get
                Return CDbl(GetValue(LeftMinSizeProperty))
            End Get
            Set(value As Double)
                SetValue(LeftMinSizeProperty, value)
            End Set
        End Property

        ''' <summary>La largeur du panneau de droite ouvert, en pixels.</summary>
        Public Property RightSize As Double
            Get
                Return CDbl(GetValue(RightSizeProperty))
            End Get
            Set(value As Double)
                SetValue(RightSizeProperty, value)
            End Set
        End Property

        ''' <summary>La largeur minimale du panneau de droite.</summary>
        Public Property RightMinSize As Double
            Get
                Return CDbl(GetValue(RightMinSizeProperty))
            End Get
            Set(value As Double)
                SetValue(RightMinSizeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si une poignée élargit le panneau de gauche.</summary>
        Public Property LeftResizable As Boolean
            Get
                Return CBool(GetValue(LeftResizableProperty))
            End Get
            Set(value As Boolean)
                SetValue(LeftResizableProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si une poignée élargit le panneau de droite.</summary>
        Public Property RightResizable As Boolean
            Get
                Return CBool(GetValue(RightResizableProperty))
            End Get
            Set(value As Boolean)
                SetValue(RightResizableProperty, value)
            End Set
        End Property

        ''' <summary>Le pas d'une flèche du clavier sur une poignée, en pixels.</summary>
        Public Property [Step] As Double
            Get
                Return CDbl(GetValue(StepProperty))
            End Get
            Set(value As Double)
                SetValue(StepProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le panneau de gauche a un bouton de fermeture.</summary>
        Public Property ShowLeftCloseIcon As Boolean
            Get
                Return CBool(GetValue(ShowLeftCloseIconProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowLeftCloseIconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le panneau de droite a un bouton de fermeture.</summary>
        Public Property ShowRightCloseIcon As Boolean
            Get
                Return CBool(GetValue(ShowRightCloseIconProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowRightCloseIconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si une languette ouvre, replie et déplie le panneau de gauche.</summary>
        Public Property ShowLeftToggle As Boolean
            Get
                Return CBool(GetValue(ShowLeftToggleProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowLeftToggleProperty, value)
            End Set
        End Property

        ''' <summary>La largeur sous laquelle les panneaux de côté recouvrent le panneau principal.</summary>
        Public Property Breakpoint As Double
            Get
                Return CDbl(GetValue(BreakpointProperty))
            End Get
            Set(value As Double)
                SetValue(BreakpointProperty, value)
            End Set
        End Property

        ''' <summary>La part de la largeur que prend un panneau de côté en superposition.</summary>
        Public Property OverlaySize As Double
            Get
                Return CDbl(GetValue(OverlaySizeProperty))
            End Get
            Set(value As Double)
                SetValue(OverlaySizeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si un clic sur le voile ferme le panneau ouvert en superposition.</summary>
        Public Property Dismissable As Boolean
            Get
                Return CBool(GetValue(DismissableProperty))
            End Get
            Set(value As Boolean)
                SetValue(DismissableProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si Échap ferme le panneau ouvert en superposition.</summary>
        Public Property CloseOnEscape As Boolean
            Get
                Return CBool(GetValue(CloseOnEscapeProperty))
            End Get
            Set(value As Boolean)
                SetValue(CloseOnEscapeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le panneau de droite ouvert couvre d'un voile le reste de la mise en page.</summary>
        Public Property Modal As Boolean
            Get
                Return CBool(GetValue(ModalProperty))
            End Get
            Set(value As Boolean)
                SetValue(ModalProperty, value)
            End Set
        End Property

        ''' <summary>Vrai sous <see cref="Breakpoint"/>.</summary>
        Public ReadOnly Property IsOverlay As Boolean
            Get
                Return CBool(GetValue(IsOverlayProperty))
            End Get
        End Property

        ''' <summary>Vrai quand le panneau de gauche est ouvert et déplié.</summary>
        Public ReadOnly Property IsLeftExpanded As Boolean
            Get
                Return CBool(GetValue(IsLeftExpandedProperty))
            End Get
        End Property

        ''' <summary>Ouvre un panneau de côté ; en superposition, ferme l'autre.</summary>
        Public Sub Show(side As TriptychSide)
            ChangerLaVisibilite(side, True)
        End Sub

        ''' <summary>Ferme un panneau de côté.</summary>
        Public Sub Hide(side As TriptychSide)
            ChangerLaVisibilite(side, False)
        End Sub

        ''' <summary>Ouvre ou ferme un panneau de côté.</summary>
        Public Sub Toggle(side As TriptychSide)
            ChangerLaVisibilite(side, Not EstOuvert(side))
        End Sub

        ''' <summary>La largeur du panneau de gauche ouvert, bornée : sa bande s'il est replié.</summary>
        Public ReadOnly Property LeftPixels As Double
            Get
                Return If(LeftCollapsed, LeftCollapsedSize, Borner(TriptychSide.Left, LeftSize))
            End Get
        End Property

        ''' <summary>La largeur du panneau de droite ouvert, bornée.</summary>
        Public ReadOnly Property RightPixels As Double
            Get
                Return Borner(TriptychSide.Right, RightSize)
            End Get
        End Property

        ''' <summary>La largeur la plus grande qu'un panneau de côté peut prendre, le panneau principal gardant sa place.</summary>
        Public Function GetMaxSize(side As TriptychSide) As Double
            Dim minimale = If(side = TriptychSide.Left, LeftMinSize, RightMinSize)
            Dim largeur = ActualWidth
            If largeur <= 0 Then Return Math.Max(minimale, If(side = TriptychSide.Left, LeftSize, RightSize))
            ' Le panneau de gauche passe d'abord : celui de droite garde au moins sa taille minimale.
            If side = TriptychSide.Left Then
                Return Math.Max(minimale, Math.Round(largeur - If(RightVisible, RightMinSize + PlaceDuPrincipalEntreDeux, PlaceDuPrincipal)))
            End If
            Dim gauche = If(LeftVisible, LeftPixels, 0.0)
            Return Math.Max(minimale, Math.Round(largeur - gauche - If(gauche > 0, PlaceDuPrincipalEntreDeux, PlaceDuPrincipal)))
        End Function

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _poigneeGauche IsNot Nothing Then Detacher(_poigneeGauche)
            If _poigneeDroite IsNot Nothing Then Detacher(_poigneeDroite)
            If _languette IsNot Nothing Then RemoveHandler _languette.Click, AddressOf OnLanguette
            If _fermerGauche IsNot Nothing Then RemoveHandler _fermerGauche.Click, AddressOf OnFermerGauche
            If _fermerDroite IsNot Nothing Then RemoveHandler _fermerDroite.Click, AddressOf OnFermerDroite
            If _voile IsNot Nothing Then RemoveHandler _voile.MouseLeftButtonDown, AddressOf OnVoile
            MyBase.OnApplyTemplate()
            _gauche = TryCast(GetTemplateChild("PART_Left"), FrameworkElement)
            _droite = TryCast(GetTemplateChild("PART_Right"), FrameworkElement)
            _principal = TryCast(GetTemplateChild("PART_Main"), FrameworkElement)
            _voile = TryCast(GetTemplateChild("PART_Mask"), FrameworkElement)
            _poigneeGauche = TryCast(GetTemplateChild("PART_LeftGutter"), TriptychGutter)
            _poigneeDroite = TryCast(GetTemplateChild("PART_RightGutter"), TriptychGutter)
            _languette = TryCast(GetTemplateChild("PART_LeftToggle"), ButtonBase)
            _fermerGauche = TryCast(GetTemplateChild("PART_LeftClose"), ButtonBase)
            _fermerDroite = TryCast(GetTemplateChild("PART_RightClose"), ButtonBase)
            If _poigneeGauche IsNot Nothing Then Attacher(_poigneeGauche, TriptychSide.Left)
            If _poigneeDroite IsNot Nothing Then Attacher(_poigneeDroite, TriptychSide.Right)
            If _languette IsNot Nothing Then AddHandler _languette.Click, AddressOf OnLanguette
            If _fermerGauche IsNot Nothing Then AddHandler _fermerGauche.Click, AddressOf OnFermerGauche
            If _fermerDroite IsNot Nothing Then AddHandler _fermerDroite.Click, AddressOf OnFermerDroite
            If _voile IsNot Nothing Then AddHandler _voile.MouseLeftButtonDown, AddressOf OnVoile
            Placer(False)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse e.Key <> Key.Escape OrElse Not CloseOnEscape OrElse Not IsOverlay Then Return
            If LeftVisible Then
                Hide(TriptychSide.Left)
            ElseIf RightVisible Then
                Hide(TriptychSide.Right)
            Else
                Return
            End If
            e.Handled = True
        End Sub

        ''' <summary>Passe en superposition sous la largeur limite, ou en sort ; une largeur nulle (caché) ne change rien.</summary>
        Friend Sub MettreAJourLeMode(largeur As Double)
            If largeur <= 0 Then Return
            Dim superpose = largeur < Breakpoint
            If superpose = IsOverlay Then
                Placer(False)
                Return
            End If
            SetValue(IsOverlayPropertyKey, superpose)
            If superpose Then
                ' Un seul panneau à la fois : celui de droite cède, et reviendra avec la place.
                If LeftVisible AndAlso RightVisible Then ChangerLaVisibilite(TriptychSide.Right, False, parLaPlace:=True)
            Else
                If _droiteFermeeParLaPlace Then ChangerLaVisibilite(TriptychSide.Right, True, parLaPlace:=True)
                ' La bande repliée fait partie de la mise en page : elle revient quand la superposition prend fin.
                If LeftCollapsed AndAlso Not LeftVisible Then ChangerLaVisibilite(TriptychSide.Left, True, parLaPlace:=True)
            End If
            Placer(False)
            RaiseEvent ModeChange(Me, EventArgs.Empty)
        End Sub

        Private Function EstOuvert(side As TriptychSide) As Boolean
            Return If(side = TriptychSide.Left, LeftVisible, RightVisible)
        End Function

        Private Sub ChangerLaVisibilite(side As TriptychSide, ouvert As Boolean, Optional parLaPlace As Boolean = False)
            If EstOuvert(side) = ouvert Then Return
            ' Le panneau de droite que la mise en page ferme d'elle-même revient quand la place revient ; un geste sur lui
            ' efface ce souvenir.
            If side = TriptychSide.Right Then _droiteFermeeParLaPlace = parLaPlace AndAlso Not ouvert
            SetCurrentValue(If(side = TriptychSide.Left, LeftVisibleProperty, RightVisibleProperty), ouvert)
        End Sub

        ' En superposition, qu'un panneau s'ouvre d'un geste, d'une méthode ou d'une liaison : un seul à la fois (l'autre
        ' cède, et reviendra avec la place), le focus y entre ; fermé, il rend le focus à l'élément qui l'avait ouvert
        ' (restoreFocus de mkm), si le focus était resté en lui.
        Private Sub Superposer(side As TriptychSide, ouvert As Boolean)
            If Not IsOverlay Then Return
            Dim panneau = If(side = TriptychSide.Left, _gauche, _droite)
            If ouvert Then
                Dim autre = If(side = TriptychSide.Left, TriptychSide.Right, TriptychSide.Left)
                If EstOuvert(autre) Then ChangerLaVisibilite(autre, False, parLaPlace:=True)
                _ouvreurs(side) = Keyboard.FocusedElement
                If panneau IsNot Nothing AndAlso Not panneau.IsKeyboardFocusWithin Then
                    Dispatcher.BeginInvoke(New Action(Sub() panneau.MoveFocus(New TraversalRequest(FocusNavigationDirection.First))))
                End If
            Else
                Dim ouvreur As IInputElement = Nothing
                _ouvreurs.TryGetValue(side, ouvreur)
                _ouvreurs.Remove(side)
                If ouvreur IsNot Nothing AndAlso panneau IsNot Nothing AndAlso panneau.IsKeyboardFocusWithin Then
                    Dispatcher.BeginInvoke(New Action(Sub() ouvreur.Focus()))
                End If
            End If
        End Sub

        Private Function Borner(side As TriptychSide, taille As Double) As Double
            Dim minimale = If(side = TriptychSide.Left, LeftMinSize, RightMinSize)
            Return Math.Min(Math.Max(minimale, Math.Round(taille)), GetMaxSize(side))
        End Function

        ''' <summary>Pose la géométrie : la largeur des panneaux, la place du panneau principal, celle des poignées, le voile.</summary>
        Private Sub Placer(animer As Boolean)
            SetValue(IsLeftExpandedPropertyKey, LeftVisible AndAlso (IsOverlay OrElse Not LeftCollapsed))
            If _gauche Is Nothing OrElse _droite Is Nothing OrElse _principal Is Nothing Then Return

            Dim superpose = IsOverlay
            Dim largeurGauche = If(LeftVisible, If(superpose, ActualWidth * OverlaySize, LeftPixels), 0.0)
            Dim largeurDroite = If(RightVisible, If(superpose, ActualWidth * OverlaySize, RightPixels), 0.0)
            Dim marge = New Thickness(If(superpose, 0.0, largeurGauche), 0, If(superpose, 0.0, largeurDroite), 0)
            animer = animer AndAlso Not _tire AndAlso IsLoaded AndAlso SystemParameters.ClientAreaAnimation

            Glisser(_gauche, WidthProperty, largeurGauche, animer)
            Glisser(_droite, WidthProperty, largeurDroite, animer)
            If animer Then
                _principal.BeginAnimation(MarginProperty, New ThicknessAnimation(marge, Duree()) With {.EasingFunction = Courbe()})
            Else
                _principal.BeginAnimation(MarginProperty, Nothing)
                _principal.Margin = marge
            End If

            ' Les poignées : à cheval sur le bord du panneau ouvert, et seulement là où l'on peut tirer.
            Dim voile = CoteDuVoile()
            Poser(_poigneeGauche, Not superpose AndAlso LeftVisible AndAlso LeftResizable AndAlso Not LeftCollapsed AndAlso voile <> "modal",
                  New Thickness(LeftPixels - Epaisseur(_poigneeGauche) / 2, 0, 0, 0))
            Poser(_poigneeDroite, Not superpose AndAlso RightVisible AndAlso RightResizable AndAlso voile <> "modal",
                  New Thickness(0, 0, RightPixels - Epaisseur(_poigneeDroite) / 2, 0))

            ' Fermés, les panneaux ne prennent ni la souris ni le clavier ; le panneau principal non plus sous un voile.
            Inerte(_gauche, Not LeftVisible OrElse voile = "modal")
            Inerte(_droite, Not RightVisible)
            Inerte(_principal, voile IsNot Nothing)
            If superpose Then
                KeyboardNavigation.SetTabNavigation(_gauche, If(LeftVisible, KeyboardNavigationMode.Cycle, KeyboardNavigationMode.None))
                KeyboardNavigation.SetTabNavigation(_droite, If(RightVisible, KeyboardNavigationMode.Cycle, KeyboardNavigationMode.None))
            End If

            If _voile IsNot Nothing Then
                _voile.Visibility = If(voile Is Nothing, Visibility.Collapsed, Visibility.Visible)
                If voile IsNot Nothing Then _voile.SetResourceReference(Panel.BackgroundProperty, $"mkm.triptych.mask.{voile}.background")
            End If
        End Sub

        ' Le voile : sous le panneau ouvert en superposition, ou sous le panneau de droite modal.
        Private Function CoteDuVoile() As String
            If IsOverlay Then Return If(LeftVisible, "left", If(RightVisible, "right", Nothing))
            Return If(Modal AndAlso RightVisible, "modal", Nothing)
        End Function

        Private Shared Sub Inerte(element As FrameworkElement, oui As Boolean)
            element.IsHitTestVisible = Not oui
            KeyboardNavigation.SetTabNavigation(element, If(oui, KeyboardNavigationMode.None, KeyboardNavigationMode.Continue))
        End Sub

        Private Shared Sub Poser(poignee As TriptychGutter, visible As Boolean, marge As Thickness)
            If poignee Is Nothing Then Return
            poignee.Visibility = If(visible, Visibility.Visible, Visibility.Collapsed)
            poignee.Margin = marge
        End Sub

        Private Shared Function Epaisseur(poignee As TriptychGutter) As Double
            If poignee Is Nothing Then Return 0.0
            Return If(Double.IsNaN(poignee.Width), 0.0, poignee.Width)
        End Function

        Private Sub Glisser(element As FrameworkElement, propriete As DependencyProperty, valeur As Double, anime As Boolean)
            If anime Then
                element.BeginAnimation(propriete, New DoubleAnimation(valeur, Duree()) With {.EasingFunction = Courbe()})
            Else
                element.BeginAnimation(propriete, Nothing)
                element.SetValue(propriete, valeur)
            End If
        End Sub

        ' 0,4 s (le jeton de mkm), ease-in-out.
        Private Function Duree() As Duration
            Dim jeton = TryFindResource("mkm.triptych.transition.duration")
            Return If(TypeOf jeton Is Duration, DirectCast(jeton, Duration), New Duration(TimeSpan.FromSeconds(0.4)))
        End Function

        Private Shared Function Courbe() As IEasingFunction
            Return New CubicEase With {.EasingMode = EasingMode.EaseInOut}
        End Function

        Private Sub Attacher(poignee As TriptychGutter, side As TriptychSide)
            poignee.Side = side
            AddHandler poignee.DragStarted, AddressOf OnTirageCommence
            AddHandler poignee.DragDelta, AddressOf OnTirage
            AddHandler poignee.DragCompleted, AddressOf OnTirageFini
            AddHandler poignee.KeyDown, AddressOf OnToucheDePoignee
        End Sub

        Private Sub Detacher(poignee As TriptychGutter)
            RemoveHandler poignee.DragStarted, AddressOf OnTirageCommence
            RemoveHandler poignee.DragDelta, AddressOf OnTirage
            RemoveHandler poignee.DragCompleted, AddressOf OnTirageFini
            RemoveHandler poignee.KeyDown, AddressOf OnToucheDePoignee
        End Sub

        ''' <summary>Donne une largeur à un panneau de côté, bornée, sans glissement.</summary>
        Friend Sub Dimensionner(side As TriptychSide, taille As Double)
            _tire = True
            Try
                SetCurrentValue(If(side = TriptychSide.Left, LeftSizeProperty, RightSizeProperty), Borner(side, taille))
            Finally
                _tire = False
            End Try
        End Sub

        Private Sub OnTirageCommence(sender As Object, e As DragStartedEventArgs)
            _tire = True
        End Sub

        ' La poignée de gauche s'élargit vers la droite, celle de droite vers la gauche ; la mise en page suit aussitôt.
        Private Sub OnTirage(sender As Object, e As DragDeltaEventArgs)
            Dim poignee = DirectCast(sender, TriptychGutter)
            If poignee.Side = TriptychSide.Left Then
                Dimensionner(TriptychSide.Left, LeftPixels + e.HorizontalChange)
            Else
                Dimensionner(TriptychSide.Right, RightPixels - e.HorizontalChange)
            End If
            _tire = True
            UpdateLayout()
        End Sub

        Private Sub OnTirageFini(sender As Object, e As DragCompletedEventArgs)
            _tire = False
            RaiseEvent ResizeEnd(Me, EventArgs.Empty)
        End Sub

        Private Sub OnToucheDePoignee(sender As Object, e As KeyEventArgs)
            Dim poignee = DirectCast(sender, TriptychGutter)
            Dim gauche = poignee.Side = TriptychSide.Left
            Dim actuelle = If(gauche, LeftPixels, RightPixels)
            Dim grandit = If(gauche, Key.Right, Key.Left)
            Dim retrecit = If(gauche, Key.Left, Key.Right)
            Dim taille As Double
            Select Case e.Key
                Case grandit
                    taille = actuelle + [Step]
                Case retrecit
                    taille = actuelle - [Step]
                Case Key.Home
                    taille = If(gauche, LeftMinSize, RightMinSize)
                Case Key.End
                    taille = GetMaxSize(poignee.Side)
                Case Else
                    Return
            End Select
            Dimensionner(poignee.Side, taille)
            e.Handled = True
            RaiseEvent ResizeEnd(Me, EventArgs.Empty)
        End Sub

        ' La languette : fermé, le panneau de gauche s'ouvre ; en superposition, il se ferme ; sinon, il se replie ou se déplie.
        Private Sub OnLanguette(sender As Object, e As RoutedEventArgs)
            If Not LeftVisible Then
                Show(TriptychSide.Left)
            ElseIf IsOverlay Then
                Hide(TriptychSide.Left)
            Else
                SetCurrentValue(LeftCollapsedProperty, Not LeftCollapsed)
            End If
        End Sub

        Private Sub OnFermerGauche(sender As Object, e As RoutedEventArgs)
            Hide(TriptychSide.Left)
        End Sub

        Private Sub OnFermerDroite(sender As Object, e As RoutedEventArgs)
            Hide(TriptychSide.Right)
        End Sub

        Private Sub OnVoile(sender As Object, e As MouseButtonEventArgs)
            If Not IsOverlay OrElse Not Dismissable Then Return
            e.Handled = True
            If LeftVisible Then Hide(TriptychSide.Left) Else Hide(TriptychSide.Right)
        End Sub

        Private Shared Sub OnGeometrieChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim mise = DirectCast(d, Triptych)
            If e.Property Is BreakpointProperty Then
                mise.MettreAJourLeMode(mise.ActualWidth)
                Return
            End If
            ' Un geste de l'application sur le panneau de droite efface le souvenir de sa fermeture par la place.
            If e.Property Is RightVisibleProperty AndAlso CBool(e.NewValue) Then mise._droiteFermeeParLaPlace = False
            If e.Property Is LeftVisibleProperty Then mise.Superposer(TriptychSide.Left, CBool(e.NewValue))
            If e.Property Is RightVisibleProperty Then mise.Superposer(TriptychSide.Right, CBool(e.NewValue))
            mise.Placer(Not mise._tire)
        End Sub

    End Class

    ''' <summary>
    ''' La poignée d'un panneau de côté d'un <see cref="Triptych"/> : une bande invisible de 10 px à cheval sur son bord ;
    ''' au survol, pendant qu'on la tire ou au clavier, une pilule grise de 4 × 40 px paraît au milieu.
    ''' </summary>
    Public Class TriptychGutter
        Inherits Thumb

        Private Shared ReadOnly IsFocusVisiblePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFocusVisible), GetType(Boolean), GetType(TriptychGutter), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand la poignée a le focus du clavier, et non celui d'un clic.</summary>
        Public Shared ReadOnly IsFocusVisibleProperty As DependencyProperty = IsFocusVisiblePropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(TriptychGutter), New FrameworkPropertyMetadata(GetType(TriptychGutter)))
            FocusableProperty.OverrideMetadata(GetType(TriptychGutter), New FrameworkPropertyMetadata(True))
        End Sub

        ''' <summary>Le panneau de côté que la poignée élargit.</summary>
        Public Property Side As TriptychSide

        ''' <summary>Vrai quand la poignée a le focus du clavier, et non celui d'un clic.</summary>
        Public ReadOnly Property IsFocusVisible As Boolean
            Get
                Return CBool(GetValue(IsFocusVisibleProperty))
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Sub OnGotKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnGotKeyboardFocus(e)
            SetValue(IsFocusVisiblePropertyKey, InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnLostKeyboardFocus(e)
            SetValue(IsFocusVisiblePropertyKey, False)
        End Sub

    End Class

End Namespace
