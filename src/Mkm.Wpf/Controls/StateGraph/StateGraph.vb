Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Globalization
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes

Namespace Mkm.Wpf.Controls

    ''' <summary>Un état d'un <see cref="StateGraph"/> : sa clé, son nom, et sa couleur, que donne l'application.</summary>
    Public Class StateGraphState
        ''' <summary>La clé de l'état (celle de <see cref="StateGraph.Current"/>, des transitions et des actions).</summary>
        Public Property Id As String
        ''' <summary>Le nom affiché.</summary>
        Public Property Label As String
        ''' <summary>La couleur de l'état ; sans elle, le gris neutre.</summary>
        Public Property Color As Brush
        ''' <summary>La couleur pâle d'un état encore à venir ; sans elle, <see cref="Color"/>.</summary>
        Public Property PaleColor As Brush
        ''' <summary>L'info-bulle ; sans elle, le nom.</summary>
        Public Property ToolTip As Object
    End Class

    ''' <summary>Une transition possible du cycle de vie, d'un état à un autre.</summary>
    Public Class StateGraphTransition
        ''' <summary>La clé de l'état de départ.</summary>
        Public Property From As String
        ''' <summary>La clé de l'état d'arrivée.</summary>
        Public Property [To] As String
    End Class

    ''' <summary>Une action offerte depuis l'état courant : la flèche vers l'état visé, et son nom, qui se clique.</summary>
    Public Class StateGraphAction
        ''' <summary>La clé de l'état visé.</summary>
        Public Property [To] As String
        ''' <summary>Le nom de l'action (« Promouvoir », « Rétrograder »).</summary>
        Public Property Label As String
    End Class

    ''' <summary>Les données de l'évènement <see cref="StateGraph.ActionClick"/>.</summary>
    Public Class StateGraphActionEventArgs
        Inherits RoutedEventArgs

        ''' <summary>L'action choisie.</summary>
        Public ReadOnly Property Action As StateGraphAction

        Friend Sub New(evenement As RoutedEvent, source As Object, action As StateGraphAction)
            MyBase.New(evenement, source)
            Me.Action = action
        End Sub
    End Class

    ''' <summary>
    ''' Le graphe du cycle de vie de mkm (<c>StateGraph</c>) : les états sur une ligne (90 × 40 px, coins de 4 px), l'état
    ''' courant cerné de 2 px de noir, ceux à venir pâlis ; d'un état au suivant, une flèche gris pâle ; depuis l'état
    ''' courant, les actions (<see cref="Actions"/>) en flèches gris foncé, droites vers un voisin, par-dessus vers un état
    ''' plus loin, par-dessous pour un retour ; leur nom bleu se clique (<see cref="ActionClick"/>). Au survol d'un nom, sa
    ''' flèche s'épaissit et défile ; pendant une action (<see cref="Pending"/>), l'état visé attend.
    ''' <code>
    ''' &lt;mkm:StateGraph Current="inwork" States="{Binding Etats}" Transitions="{Binding Transitions}"
    '''                  Actions="{Binding Actions}" ActionClick="OnAction" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Les couleurs des états sont celles de la politique de l'application (les maturités) : la bibliothèque n'en a pas.
    ''' Un état blanc (<c>#FFF</c>) est cerné d'un trait gris, son nom en noir.
    ''' </remarks>
    Public Class StateGraph
        Inherits Control

        ''' <summary>Un groupe pour les lecteurs d'écran : ses états et ses actions dedans.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

        ' La géométrie de mkm : des états de 90 × 40 sur une ligne, 30 px entre deux ; une flèche qui passe au-dessus ou
        ' au-dessous des états prend un couloir, à 21 px des états pour le premier, 42 pour le second, et ainsi de suite.
        Private Const Largeur As Double = 90
        Private Const Hauteur As Double = 40
        Private Const Pas As Double = 120
        Private Const Couloir As Double = 21
        Private Const NomDroit As Double = 40
        Private Const NomDessus As Double = 5
        Private Const NomDessous As Double = 15

        ''' <summary>Les états, dans l'ordre du cycle.</summary>
        Public Shared ReadOnly StatesProperty As DependencyProperty = Liste(NameOf(States))

        ''' <summary>Les transitions possibles.</summary>
        Public Shared ReadOnly TransitionsProperty As DependencyProperty = Liste(NameOf(Transitions))

        ''' <summary>Les actions offertes depuis l'état courant.</summary>
        Public Shared ReadOnly ActionsProperty As DependencyProperty = Liste(NameOf(Actions))

        ''' <summary>La clé de l'état courant.</summary>
        Public Shared ReadOnly CurrentProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Current), GetType(String), GetType(StateGraph), New FrameworkPropertyMetadata(Nothing, AddressOf OnDonneesChanged))

        ''' <summary>La clé de l'état visé par une action en cours : il attend, les noms ne se cliquent plus.</summary>
        Public Shared ReadOnly PendingProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Pending), GetType(String), GetType(StateGraph), New FrameworkPropertyMetadata(Nothing, AddressOf OnDonneesChanged))

        ''' <summary>Quand le nom d'une action est cliqué (ou choisi au clavier).</summary>
        Public Shared ReadOnly ActionClickEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(ActionClick), RoutingStrategy.Bubble, GetType(EventHandler(Of StateGraphActionEventArgs)), GetType(StateGraph))

        ' Un jeton du thème que suit le graphe : quand le thème change, il se redessine à ses nouvelles couleurs.
        Private Shared ReadOnly JetonProperty As DependencyProperty = DependencyProperty.Register(
            "Jeton", GetType(Object), GetType(StateGraph), New FrameworkPropertyMetadata(Nothing, AddressOf OnDonneesChanged))

        Private _toile As Canvas
        Private _survolee As String
        Private ReadOnly _traits As New Dictionary(Of String, Path)

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(StateGraph), New FrameworkPropertyMetadata(GetType(StateGraph)))
            FocusableProperty.OverrideMetadata(GetType(StateGraph), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un graphe vide.</summary>
        Public Sub New()
            SetCurrentValue(StatesProperty, New ObservableCollection(Of StateGraphState))
            SetCurrentValue(TransitionsProperty, New ObservableCollection(Of StateGraphTransition))
            SetCurrentValue(ActionsProperty, New ObservableCollection(Of StateGraphAction))
            SetResourceReference(JetonProperty, "mkm.stategraph.route.color")
        End Sub

        Private Shared Function Liste(nom As String) As DependencyProperty
            Return DependencyProperty.Register(nom, GetType(IEnumerable(Of Object)), GetType(StateGraph),
                                               New FrameworkPropertyMetadata(Nothing, AddressOf OnListeChanged))
        End Function

        ''' <summary>Les états, dans l'ordre du cycle.</summary>
        Public Property States As IEnumerable(Of Object)
            Get
                Return DirectCast(GetValue(StatesProperty), IEnumerable(Of Object))
            End Get
            Set(value As IEnumerable(Of Object))
                SetValue(StatesProperty, value)
            End Set
        End Property

        ''' <summary>Les transitions possibles.</summary>
        Public Property Transitions As IEnumerable(Of Object)
            Get
                Return DirectCast(GetValue(TransitionsProperty), IEnumerable(Of Object))
            End Get
            Set(value As IEnumerable(Of Object))
                SetValue(TransitionsProperty, value)
            End Set
        End Property

        ''' <summary>Les actions offertes depuis l'état courant.</summary>
        Public Property Actions As IEnumerable(Of Object)
            Get
                Return DirectCast(GetValue(ActionsProperty), IEnumerable(Of Object))
            End Get
            Set(value As IEnumerable(Of Object))
                SetValue(ActionsProperty, value)
            End Set
        End Property

        ''' <summary>La clé de l'état courant.</summary>
        Public Property Current As String
            Get
                Return CStr(GetValue(CurrentProperty))
            End Get
            Set(value As String)
                SetValue(CurrentProperty, value)
            End Set
        End Property

        ''' <summary>La clé de l'état visé par une action en cours.</summary>
        Public Property Pending As String
            Get
                Return CStr(GetValue(PendingProperty))
            End Get
            Set(value As String)
                SetValue(PendingProperty, value)
            End Set
        End Property

        ''' <summary>Quand le nom d'une action est cliqué (ou choisi au clavier).</summary>
        Public Custom Event ActionClick As EventHandler(Of StateGraphActionEventArgs)
            AddHandler(value As EventHandler(Of StateGraphActionEventArgs))
                [AddHandler](ActionClickEvent, value)
            End AddHandler
            RemoveHandler(value As EventHandler(Of StateGraphActionEventArgs))
                [RemoveHandler](ActionClickEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As StateGraphActionEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _toile = TryCast(GetTemplateChild("PART_Canvas"), Canvas)
            Dessiner()
        End Sub

        ''' <summary>Choisit une action, comme un clic sur son nom (rien pendant une action en cours).</summary>
        Public Sub InvokeAction(action As StateGraphAction)
            If action Is Nothing OrElse Pending IsNot Nothing Then Return
            Me.RaiseEvent(New StateGraphActionEventArgs(ActionClickEvent, Me, action))
        End Sub

        Private Shared Sub OnListeChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim graphe = DirectCast(d, StateGraph)
            ' Un abonnement faible : un modèle qui survit à sa vue ne retient pas le graphe.
            Dim ancienne = TryCast(e.OldValue, INotifyCollectionChanged)
            If ancienne IsNot Nothing Then CollectionChangedEventManager.RemoveHandler(ancienne, AddressOf graphe.OnCollectionChanged)
            Dim nouvelle = TryCast(e.NewValue, INotifyCollectionChanged)
            If nouvelle IsNot Nothing Then CollectionChangedEventManager.AddHandler(nouvelle, AddressOf graphe.OnCollectionChanged)
            graphe.Dessiner()
        End Sub

        Private Sub OnCollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            Dessiner()
        End Sub

        Private Shared Sub OnDonneesChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim graphe = DirectCast(d, StateGraph)
            ' Une action commence ou finit : la flèche survolée avant elle redevient simple.
            If e.Property Is PendingProperty Then graphe._survolee = Nothing
            graphe.Dessiner()
        End Sub

        Private Function Ressource(cle As String) As Object
            Return TryFindResource("mkm.stategraph." & cle)
        End Function

        Private Function Pinceau(cle As String) As Brush
            Return TryCast(Ressource(cle), Brush)
        End Function

        Private Function Nombre(cle As String, defaut As Double) As Double
            Dim valeur = Ressource(cle)
            Return If(TypeOf valeur Is Double, CDbl(valeur), defaut)
        End Function

        Private Function Graisse(cle As String, defaut As FontWeight) As FontWeight
            Dim valeur = Ressource(cle)
            Return If(TypeOf valeur Is FontWeight, DirectCast(valeur, FontWeight), defaut)
        End Function

        ' Blanc, #FFF ou #FFFFFF : cerné d'un trait gris, son nom en noir.
        Private Shared Function EstBlanc(pinceau As Brush) As Boolean
            Dim uni = TryCast(pinceau, SolidColorBrush)
            Return uni IsNot Nothing AndAlso uni.Color.R = 255 AndAlso uni.Color.G = 255 AndAlso uni.Color.B = 255 AndAlso uni.Color.A = 255
        End Function

        ''' <summary>Refait le dessin : les états, les transitions, les actions et leurs noms (la géométrie de mkm).</summary>
        Friend Sub Dessiner()
            If _toile Is Nothing Then Return
            ' Le nom d'action qui a le focus le garde à travers le redessin (celui de son action, redessiné).
            Dim focalise = TryCast(TryCast(Keyboard.FocusedElement, TextBlock)?.Tag, StateGraphAction)
            If focalise IsNot Nothing AndAlso Not _toile.IsKeyboardFocusWithin Then focalise = Nothing
            _toile.Children.Clear()
            _traits.Clear()
            If focalise IsNot Nothing Then Dispatcher.BeginInvoke(New Action(Sub() Refocaliser(focalise.To)))
            Dim lesEtats = If(States, Enumerable.Empty(Of Object)()).OfType(Of StateGraphState)().ToList()
            Dim lesTransitions = If(Transitions, Enumerable.Empty(Of Object)()).OfType(Of StateGraphTransition)().ToList()
            Dim index As New Dictionary(Of String, Integer)
            For i = 0 To lesEtats.Count - 1
                If lesEtats(i).Id IsNot Nothing AndAlso Not index.ContainsKey(lesEtats(i).Id) Then index.Add(lesEtats(i).Id, i)
            Next
            Dim compte = lesEtats.Count
            Dim depuis = If(Current IsNot Nothing AndAlso index.ContainsKey(Current), index(Current), -1)
            Dim existe = Function(source As String, cible As String) lesTransitions.Any(Function(t) t.From = source AndAlso t.To = cible)
            Dim lesActions = If(Me.Actions, Enumerable.Empty(Of Object)()).OfType(Of StateGraphAction)().
                Where(Function(a) a.To IsNot Nothing AndAlso index.ContainsKey(a.To) AndAlso a.To <> Current).ToList()

            ' Vers le suivant : droit. Vers le précédent : droit s'il n'a pas de transition vers l'état courant, sinon
            ' par-dessous, comme tout retour plus loin. Plus loin en avant : par-dessus.
            Dim genre = Function(a As StateGraphAction) As String
                            Dim vers = index(a.To)
                            If vers = depuis + 1 Then Return "droit"
                            If vers = depuis - 1 AndAlso Not existe(a.To, Current) Then Return "droit"
                            Return If(vers > depuis, "dessus", "dessous")
                        End Function
            Dim dessus = lesActions.Where(Function(a) genre(a) = "dessus").OrderBy(Function(a) index(a.To)).ToList()
            Dim dessous = lesActions.Where(Function(a) genre(a) = "dessous").OrderBy(Function(a) index(a.To)).ToList()
            Dim couloirs = Math.Max(dessus.Count, dessous.Count)

            Dim largeurTotale = 130 * compte + 20
            Dim hauteurTotale = 2 * Couloir * couloirs + 85
            _toile.Width = largeurTotale
            _toile.Height = hauteurTotale
            Dim haut = (hauteurTotale - Hauteur) / 2
            Dim bord = Math.Round(haut)
            Dim milieu = bord + Hauteur / 2
            Dim bas = bord + Hauteur
            Dim gauche = Function(i As Integer) 5 * compte + 26 + i * Pas
            Dim centre = Function(i As Integer) gauche(i) + Largeur / 2
            ' Les départs se partagent le bord de l'état courant : la flèche la plus proche part le plus à droite.
            Dim depart = Function(k As Integer, n As Integer) gauche(depuis) + (Largeur / (n + 1)) * k - k

            ' D'un autre état que l'état courant, seule la transition vers son suivant se dessine, en trait pâle.
            For i = 0 To compte - 2
                If i <> depuis AndAlso existe(lesEtats(i).Id, lesEtats(i + 1).Id) Then
                    Fleche(New Point() {New Point(gauche(i) + Largeur, milieu), New Point(gauche(i + 1), milieu)}, Pinceau("connector.color"), Nombre("connector.width", 1.5), Nothing)
                End If
            Next

            For Each a In lesActions.Where(Function(x) genre(x) = "droit")
                Dim vers = index(a.To)
                Dim avant = vers > depuis
                Dim x1 = If(avant, gauche(depuis) + Largeur, gauche(depuis))
                Dim x2 = If(avant, gauche(vers), gauche(vers) + Largeur)
                Route(a, lesEtats(vers), avant, {New Point(x1, milieu), New Point(x2, milieu)}, x2, milieu + NomDroit, False)
            Next
            For j = 0 To dessus.Count - 1
                Dim a = dessus(j)
                Dim x = depart(dessus.Count - j, dessus.Count)
                Dim voie = bord - Couloir * (j + 1)
                Dim vers = centre(index(a.To))
                Route(a, lesEtats(index(a.To)), True, {New Point(x, bord), New Point(x, voie), New Point(vers, voie), New Point(vers, bord)}, x, voie - NomDessus, False)
            Next
            For j = 0 To dessous.Count - 1
                Dim a = dessous(j)
                Dim x = depart(dessous.Count - j, dessous.Count)
                Dim voie = bas + Couloir * (dessous.Count - j)
                Dim vers = centre(index(a.To))
                Route(a, lesEtats(index(a.To)), False, {New Point(x, bas), New Point(x, voie), New Point(vers, voie), New Point(vers, bas)}, x, voie + NomDessous, True)
            Next

            Dim atteints = New HashSet(Of String)(lesActions.Select(Function(a) a.To))
            For i = 0 To compte - 1
                Etat(lesEtats(i), i, depuis, gauche(i), haut, atteints)
            Next
        End Sub

        Private Sub Refocaliser(cible As String)
            If _toile Is Nothing Then Return
            Dim nom = _toile.Children.OfType(Of TextBlock)().FirstOrDefault(Function(t) TryCast(t.Tag, StateGraphAction)?.To = cible)
            If nom IsNot Nothing AndAlso nom.Focusable Then nom.Focus()
        End Sub

        ' Une flèche : son trait, et sa pointe ouverte au bout (le marqueur de mkm, 7 × 6 px).
        Private Function Fleche(points As Point(), pinceau As Brush, epaisseur As Double, cible As String) As Path
            Dim figure As New PathFigure With {.StartPoint = points(0), .IsFilled = False}
            For i = 1 To points.Length - 1
                figure.Segments.Add(New LineSegment(points(i), True))
            Next
            Dim geometrie As New PathGeometry()
            geometrie.Figures.Add(figure)
            Dim trait As New Path With {.Data = geometrie, .Stroke = pinceau, .StrokeThickness = epaisseur, .StrokeLineJoin = PenLineJoin.Miter}
            _toile.Children.Add(trait)
            ' La pointe : un chevron plein dans le sens du dernier segment.
            Dim fin = points(points.Length - 1)
            Dim avant = points(points.Length - 2)
            Dim dx = fin.X - avant.X
            Dim dy = fin.Y - avant.Y
            Dim l = Math.Sqrt(dx * dx + dy * dy)
            If l > 0 Then
                dx /= l
                dy /= l
                Dim pointe As New Path With {
                    .Fill = pinceau,
                    .Data = New PathGeometry({New PathFigure(fin, {
                        New LineSegment(New Point(fin.X - 7 * dx + 3 * dy, fin.Y - 7 * dy - 3 * dx), True),
                        New LineSegment(New Point(fin.X - 7 * dx - 3 * dy, fin.Y - 7 * dy + 3 * dx), True)}, True)})}
                _toile.Children.Add(pointe)
            End If
            Return trait
        End Function

        ' Une action : sa flèche gris foncé et son nom bleu, que précède (en avant) ou suit (en arrière) un guillemet de la
        ' couleur de l'état visé ; au survol ou au clavier, la flèche s'épaissit et défile.
        Private Sub Route(a As StateGraphAction, vise As StateGraphState, avant As Boolean, points As Point(), x As Double, y As Double, finit As Boolean)
            Dim trait = Fleche(points, Pinceau("route.color"), Nombre("route.width", 1.5), a.To)
            _traits(a.To) = trait
            If a.To = _survolee OrElse a.To = Pending Then Activer(trait)

            Dim nom As New NomDAction With {.Lancement = Sub() InvokeAction(a),
                .FontSize = Nombre("label.font.size", 12), .Foreground = Pinceau("label.color"),
                .Focusable = Pending Is Nothing, .Cursor = If(Pending Is Nothing, Cursors.Hand, Cursors.Arrow),
                .IsHitTestVisible = Pending Is Nothing, .Tag = a, .FocusVisualStyle = Nothing}
            Automation.AutomationProperties.SetName(nom, a.Label)
            Dim drapeau As New Run(If(avant, "» ", "«")) With {
                .FontSize = Nombre("flag.font.size", 22), .FontWeight = Graisse("flag.font.weight", FontWeights.Bold),
                .Foreground = If(vise.Color Is Nothing OrElse EstBlanc(vise.Color), Pinceau("flag.blank.color"), vise.Color)}
            If avant Then
                nom.Inlines.Add(drapeau)
                nom.Inlines.Add(New Run(a.Label))
            Else
                nom.Inlines.Add(New Run(a.Label & " "))
                nom.Inlines.Add(drapeau)
            End If
            ' Mesuré dans la toile : dans la police qu'il en hérite, celle du thème.
            _toile.Children.Add(nom)
            nom.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
            ' Le point (x, y) est la ligne de base du nom, comme en SVG : le texte commence là, ou y finit.
            Dim ligneDeBase = nom.BaselineOffset
            If Double.IsNaN(ligneDeBase) OrElse ligneDeBase <= 0 Then ligneDeBase = nom.DesiredSize.Height * 0.8
            Canvas.SetLeft(nom, If(finit, x - nom.DesiredSize.Width, x))
            Canvas.SetTop(nom, y - ligneDeBase)
            System.Windows.Controls.Panel.SetZIndex(nom, 30)
            AddHandler nom.MouseEnter, Sub(s, e) Survoler(a.To)
            AddHandler nom.MouseLeave, Sub(s, e) Survoler(Nothing)
            AddHandler nom.GotKeyboardFocus, Sub(s, e) Survoler(a.To)
            AddHandler nom.LostKeyboardFocus, Sub(s, e) Survoler(Nothing)
            AddHandler nom.MouseLeftButtonUp, Sub(s, e) InvokeAction(a)
            AddHandler nom.KeyDown, Sub(s, e)
                                        If e.Key = Key.Enter OrElse e.Key = Key.Space Then
                                            e.Handled = True
                                            InvokeAction(a)
                                        End If
                                    End Sub
        End Sub

        ' Survolé ou au clavier, le nom d'une action active sa flèche ; la précédente redevient simple (sauf celle de l'action
        ' en cours). Seuls les traits changent : le nom garde le focus.
        Private Sub Survoler(cible As String)
            If _survolee = cible Then Return
            Dim trait As Path = Nothing
            If _survolee IsNot Nothing AndAlso _survolee <> Pending AndAlso _traits.TryGetValue(_survolee, trait) Then Desactiver(trait)
            _survolee = cible
            If cible IsNot Nothing AndAlso _traits.TryGetValue(cible, trait) Then Activer(trait)
        End Sub

        Private Sub Desactiver(trait As Path)
            trait.BeginAnimation(Shape.StrokeDashOffsetProperty, Nothing)
            trait.StrokeDashArray = Nothing
            trait.StrokeThickness = Nombre("route.width", 1.5)
        End Sub

        ' Une flèche active : plus épaisse, pointillée, et ses pointillés défilent.
        Private Sub Activer(trait As Path)
            trait.StrokeThickness = Nombre("route.hover.width", 2.5)
            trait.StrokeDashArray = New DoubleCollection({8 / trait.StrokeThickness, 4 / trait.StrokeThickness})
            If SystemParameters.ClientAreaAnimation Then
                trait.BeginAnimation(Shape.StrokeDashOffsetProperty,
                    New DoubleAnimation(0, -24 / trait.StrokeThickness, New Duration(TimeSpan.FromSeconds(1.4))) With {.RepeatBehavior = RepeatBehavior.Forever})
            End If
        End Sub

        ' Un état : sa couleur (pâlie s'il est à venir), cerné de 2 px de noir s'il est courant ; blanc, d'un trait gris ; visé
        ' par une action en cours, il attend sous un voile et trois barres.
        Private Sub Etat(e As StateGraphState, i As Integer, depuis As Integer, x As Double, y As Double, atteints As HashSet(Of String))
            Dim place = If(depuis < 0, "apres", If(i < depuis, "avant", If(i = depuis, "courant", "apres")))
            Dim fond = If(place = "apres", If(e.PaleColor, e.Color), e.Color)
            Dim blanc = e.Color IsNot Nothing AndAlso EstBlanc(e.Color)
            Dim boite As New Border With {
                .Width = Largeur, .Height = Hauteur, .CornerRadius = New CornerRadius(Nombre("state.border.radius", 4)),
                .Background = If(fond, Pinceau(If(place = "apres", "state.neutral.pale.background", "state.neutral.background"))),
                .ToolTip = If(e.ToolTip, e.Label), .SnapsToDevicePixels = True}
            Dim texte As New TextBlock With {
                .Text = e.Label, .HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center,
                .TextTrimming = TextTrimming.CharacterEllipsis, .FontSize = Nombre("state.font.size", 12),
                .FontWeight = Graisse("state.font.weight", FontWeights.Normal),
                .Foreground = Pinceau("state.color")}
            If blanc Then
                texte.Foreground = Pinceau(If(place = "apres", "state.blank.after.color", "state.blank.color"))
                boite.BorderThickness = New Thickness(1)
                boite.BorderBrush = Pinceau("state.blank.border.color")
            End If
            If place = "courant" Then
                boite.BorderThickness = New Thickness(Nombre("state.current.border.width", 2))
                boite.BorderBrush = Pinceau("state.current.border.color")
            End If
            Dim enAttente = Pending IsNot Nothing AndAlso e.Id = Pending
            If Pending IsNot Nothing AndAlso atteints.Contains(e.Id) Then
                boite.BorderThickness = New Thickness(1)
                boite.BorderBrush = Pinceau("state.disabled.border.color")
            End If
            Dim contenu As New Grid()
            contenu.Children.Add(texte)
            If enAttente Then contenu.Children.Add(Voile())
            boite.Child = contenu
            Canvas.SetLeft(boite, x)
            Canvas.SetTop(boite, y)
            System.Windows.Controls.Panel.SetZIndex(boite, 20)
            _toile.Children.Add(boite)
        End Sub

        ' Le voile d'un état qui attend : trois barres qui montent et descendent l'une après l'autre.
        Private Function Voile() As UIElement
            Dim fond As New Grid With {.Background = Pinceau("state.mask.background")}
            Dim barres As New StackPanel With {.Orientation = Orientation.Horizontal, .HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center}
            For i = 0 To 2
                Dim barre As New Rectangle With {.Width = 3, .Height = 12, .Margin = New Thickness(1, 0, 1, 0), .Fill = Pinceau("state.mask.bar.color"),
                                                 .RenderTransformOrigin = New Point(0.5, 0.5), .RenderTransform = New ScaleTransform()}
                If SystemParameters.ClientAreaAnimation Then
                    Dim monte As New DoubleAnimation(0.4, 1, New Duration(TimeSpan.FromSeconds(0.5))) With {
                        .AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever, .BeginTime = TimeSpan.FromSeconds(0.15 * i)}
                    barre.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, monte)
                End If
                barres.Children.Add(barre)
            Next
            fond.Children.Add(barres)
            Return fond
        End Function

    End Class

    ''' <summary>
    ''' Le nom d'une action du StateGraph : un bouton pour les lecteurs d'écran, qu'ils lancent comme un clic. Un simple
    ''' texte, ils l'auraient lu sans dire qu'il s'actionne.
    ''' </summary>
    Friend NotInheritable Class NomDAction
        Inherits TextBlock

        ''' <summary>Ce que fait l'action.</summary>
        Friend Property Lancement As System.Action

        ''' <inheritdoc/>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDAction(Me)
        End Function

    End Class

    ''' <summary>Le pair d'automatisation d'un nom d'action : un bouton qu'on lance (Invoke).</summary>
    Friend NotInheritable Class PairDAction
        Inherits System.Windows.Automation.Peers.TextBlockAutomationPeer
        Implements System.Windows.Automation.Provider.IInvokeProvider

        Friend Sub New(proprietaire As NomDAction)
            MyBase.New(proprietaire)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function GetAutomationControlTypeCore() As System.Windows.Automation.Peers.AutomationControlType
            Return System.Windows.Automation.Peers.AutomationControlType.Button
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetClassNameCore() As String
            Return "StateGraphAction"
        End Function

        ''' <summary>Une action ne se lance pas pendant qu'une autre attend sa fin (<c>Pending</c>).</summary>
        Protected Overrides Function IsEnabledCore() As Boolean
            Return MyBase.IsEnabledCore() AndAlso DirectCast(Owner, UIElement).IsHitTestVisible
        End Function

        ''' <inheritdoc/>
        Public Overrides Function GetPattern(patternInterface As System.Windows.Automation.Peers.PatternInterface) As Object
            If patternInterface = System.Windows.Automation.Peers.PatternInterface.Invoke Then Return Me
            Return MyBase.GetPattern(patternInterface)
        End Function

        ''' <summary>Lance l'action.</summary>
        Public Sub Invoke() Implements System.Windows.Automation.Provider.IInvokeProvider.Invoke
            If Not IsEnabled() Then Throw New System.Windows.Automation.ElementNotEnabledException()
            DirectCast(Owner, NomDAction).Lancement?.Invoke()
        End Sub

    End Class

End Namespace
