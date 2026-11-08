Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La rangée des commandes d'une <see cref="Toolbar"/> : à droite, séparées d'un écart (<see cref="Gap"/>). Sans
    ''' débordement, elle passe à la ligne quand la place manque (le <c>flex-wrap</c> de mkm), chaque ligne à droite.
    ''' Avec <see cref="Toolbar.Overflow"/>, elle garde dans l'ordre celles qui tiennent avant le bouton « … » et cache
    ''' les autres ; un trait ne finit pas la rangée : celui qui précéderait le bouton « … » se cache aussi.
    ''' </summary>
    Public Class ToolbarPanel
        Inherits System.Windows.Controls.Panel

        ''' <summary>L'écart entre deux commandes visibles.</summary>
        Public Shared ReadOnly GapProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Gap), GetType(Double), GetType(ToolbarPanel),
            New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure))

        ' Les lignes de la dernière mesure, chacune avec les commandes qu'elle montre.
        Private ReadOnly _lignes As New List(Of List(Of UIElement))

        ''' <summary>L'écart entre deux commandes visibles.</summary>
        Public Property Gap As Double
            Get
                Return CDbl(GetValue(GapProperty))
            End Get
            Set(value As Double)
                SetValue(GapProperty, value)
            End Set
        End Property

        Private ReadOnly Property Barre As Toolbar
            Get
                Return TryCast(ItemsControl.GetItemsOwner(Me), Toolbar)
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function MeasureOverride(availableSize As Size) As Size
            Dim barre = Me.Barre
            If barre IsNot Nothing Then barre.Rangee = Me

            Dim enfants = InternalChildren.OfType(Of UIElement)().ToList()
            For Each enfant In enfants
                enfant.Measure(New Size(Double.PositiveInfinity, availableSize.Height))
            Next
            Dim presents = enfants.Where(AddressOf EstPresent).ToList()
            Dim total = Largeur(presents)
            Dim montres As List(Of UIElement)
            _lignes.Clear()

            If barre IsNot Nothing AndAlso barre.Overflow AndAlso total > availableSize.Width Then
                ' Celles qui tiennent avec le bouton « … », dans l'ordre ; la première qui déborde passe au menu avec
                ' toutes celles qui la suivent.
                Dim place = availableSize.Width - barre.LargeurDuDebordement
                Dim prises As New List(Of UIElement)
                Dim prise = 0.0
                For Each enfant In presents
                    Dim besoin = enfant.DesiredSize.Width + If(prises.Count > 0, Gap, 0.0)
                    If prise + besoin > place Then Exit For
                    prise += besoin
                    prises.Add(enfant)
                Next
                ' Un trait ne finit pas la rangée.
                montres = prises.Where(Function(e, i) Not EstUnTrait(e) OrElse prises.Skip(i + 1).Any(Function(s) Not EstUnTrait(s))).ToList()
                _lignes.Add(montres)
                barre.Deborder(montres.Count < presents.Count)
            Else
                barre?.Deborder(False)
                montres = presents
                ' Sans débordement, à la ligne quand la place manque.
                Dim ligne As New List(Of UIElement)
                Dim largeurDeLigne = 0.0
                For Each enfant In presents
                    Dim besoin = enfant.DesiredSize.Width + If(ligne.Count > 0, Gap, 0.0)
                    If ligne.Count > 0 AndAlso largeurDeLigne + besoin > availableSize.Width Then
                        _lignes.Add(ligne)
                        ligne = New List(Of UIElement)
                        largeurDeLigne = 0.0
                        besoin = enfant.DesiredSize.Width
                    End If
                    largeurDeLigne += besoin
                    ligne.Add(enfant)
                Next
                If ligne.Count > 0 Then _lignes.Add(ligne)
            End If

            ' Cachées, et non repliées : elles restent mesurées, prêtes à revenir dès que la place le permet.
            For Each enfant In enfants
                enfant.Visibility = If(montres.Contains(enfant) OrElse Not EstPresent(enfant), Visibility.Visible, Visibility.Hidden)
            Next

            Dim largeurVoulue = If(_lignes.Count = 0, 0.0, _lignes.Max(Function(l) Largeur(l)))
            Dim hauteurVoulue = _lignes.Sum(Function(l) Hauteur(l))
            Return New Size(largeurVoulue, hauteurVoulue)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function ArrangeOverride(finalSize As Size) As Size
            Dim barre = Me.Barre
            Dim reserve = If(barre IsNot Nothing AndAlso barre.IsOverflowing, barre.LargeurDuDebordement, 0.0)
            Dim places As New HashSet(Of UIElement)
            Dim y = 0.0
            Dim hauteurTotale = _lignes.Sum(Function(l) Hauteur(l))
            ' Une seule ligne se centre en hauteur dans la barre ; plusieurs s'empilent.
            If _lignes.Count = 1 Then y = Math.Max(0.0, (finalSize.Height - hauteurTotale) / 2)
            For Each ligne In _lignes
                Dim hauteurDeLigne = Hauteur(ligne)
                Dim x = Math.Max(0.0, finalSize.Width - reserve - Largeur(ligne))
                For Each enfant In ligne
                    Dim taille = enfant.DesiredSize
                    enfant.Arrange(New Rect(x, y + (hauteurDeLigne - taille.Height) / 2, taille.Width, taille.Height))
                    x += taille.Width + Gap
                    places.Add(enfant)
                Next
                y += hauteurDeLigne
            Next
            For Each enfant In InternalChildren.OfType(Of UIElement)()
                If Not places.Contains(enfant) Then enfant.Arrange(New Rect())
            Next
            Return finalSize
        End Function

        Private Function Largeur(enfants As IList(Of UIElement)) As Double
            If enfants.Count = 0 Then Return 0.0
            Return enfants.Sum(Function(e) e.DesiredSize.Width) + Gap * (enfants.Count - 1)
        End Function

        Private Shared Function Hauteur(enfants As IList(Of UIElement)) As Double
            Return If(enfants.Count = 0, 0.0, enfants.Max(Function(e) e.DesiredSize.Height))
        End Function

        ' Une commande repliée (Visibility.Collapsed) n'est pas dans la rangée, ni dans le menu.
        Private Shared Function EstPresent(conteneur As UIElement) As Boolean
            Dim element = If(TryCast(TryCast(conteneur, ContentPresenter)?.Content, UIElement), conteneur)
            Return element.Visibility <> Visibility.Collapsed
        End Function

        Private Shared Function EstUnTrait(conteneur As UIElement) As Boolean
            Return TypeOf TryCast(conteneur, ContentPresenter)?.Content Is ToolbarSeparator OrElse TypeOf conteneur Is ToolbarSeparator
        End Function

    End Class

    ''' <summary>
    ''' La place du bouton « … » d'une <see cref="Toolbar"/> : son contenu est toujours mesuré (la rangée des commandes
    ''' lui réserve sa largeur), mais ne prend de place, en largeur comme en hauteur, que quand la barre déborde.
    ''' </summary>
    Public Class ToolbarOverflow
        Inherits Decorator

        ''' <summary>La largeur du bouton « … » et de son trait, montrés ou non.</summary>
        Friend ReadOnly Property LargeurDuContenu As Double
            Get
                Return If(Child Is Nothing, 0.0, Child.DesiredSize.Width)
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function MeasureOverride(constraint As Size) As Size
            If Child Is Nothing Then Return New Size()
            Child.Measure(New Size(Double.PositiveInfinity, constraint.Height))
            Dim barre = TryCast(TemplatedParent, Toolbar)
            Return If(barre IsNot Nothing AndAlso barre.IsOverflowing, Child.DesiredSize, New Size())
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function ArrangeOverride(arrangeSize As Size) As Size
            If Child IsNot Nothing Then
                Dim taille = Child.DesiredSize
                Child.Arrange(New Rect(arrangeSize.Width - taille.Width, (arrangeSize.Height - taille.Height) / 2, taille.Width, taille.Height))
            End If
            Return arrangeSize
        End Function

    End Class

End Namespace
