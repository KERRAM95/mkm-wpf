Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Threading

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une liste de commandes d'<see cref="ActionBar"/> : la barre en montre une, et sa flèche ouvre
    ''' les autres. La commande montrée est la commande à bascule enfoncée, sinon la dernière lancée,
    ''' sinon la première, comme dans mkm.
    ''' <code>
    ''' &lt;mkm:ActionBarDropDown&gt;
    '''     &lt;mkm:ActionBarButton Label="Zoom avant" … /&gt;
    '''     &lt;mkm:ActionBarButton Label="Zoom arrière" … /&gt;
    ''' &lt;/mkm:ActionBarDropDown&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Main", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Arrow", Type:=GetType(ToggleButton))>
    <TemplatePart(Name:="PART_Popup", Type:=GetType(Popup))>
    Public Class ActionBarDropDown
        Inherits ItemsControl

        Private Shared ReadOnly ShownItemPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(ShownItem), GetType(UIElement), GetType(ActionBarDropDown), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La commande que la barre montre.</summary>
        Public Shared ReadOnly ShownItemProperty As DependencyProperty = ShownItemPropertyKey.DependencyProperty

        ''' <summary>Vrai quand la liste est ouverte.</summary>
        Public Shared ReadOnly IsOpenProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsOpen), GetType(Boolean), GetType(ActionBarDropDown),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>
        ''' Vrai dans la liste ouverte : ses commandes y prennent la forme d'une ligne de menu. Hérité.
        ''' </summary>
        Public Shared ReadOnly IsInListProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "IsInList", GetType(Boolean), GetType(ActionBarDropDown),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits))

        Private _principale As ButtonBase
        Private _fleche As ToggleButton
        Private _liste As Popup
        Private _derniere As UIElement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ActionBarDropDown), New FrameworkPropertyMetadata(GetType(ActionBarDropDown)))
            FocusableProperty.OverrideMetadata(GetType(ActionBarDropDown), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Une liste vide ; ses commandes s'ajoutent en XAML, comme ses enfants.</summary>
        Public Sub New()
            ' Les événements routés de ses commandes (AddHandler, en VB, est aussi un mot-clé : Me.AddHandler).
            Me.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf OnCommandeLancee))
            Me.AddHandler(ToggleButton.CheckedEvent, New RoutedEventHandler(AddressOf OnBascule))
            Me.AddHandler(ToggleButton.UncheckedEvent, New RoutedEventHandler(AddressOf OnBascule))
        End Sub

        ''' <summary>La commande que la barre montre.</summary>
        Public ReadOnly Property ShownItem As UIElement
            Get
                Return DirectCast(GetValue(ShownItemProperty), UIElement)
            End Get
        End Property

        ''' <summary>Vrai quand la liste est ouverte.</summary>
        Public Property IsOpen As Boolean
            Get
                Return CBool(GetValue(IsOpenProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsOpenProperty, value)
            End Set
        End Property

        ''' <summary>Vrai dans la liste ouverte.</summary>
        Public Shared Function GetIsInList(element As DependencyObject) As Boolean
            Return CBool(element.GetValue(IsInListProperty))
        End Function

        ''' <summary>Pose <see cref="IsInListProperty"/>.</summary>
        Public Shared Sub SetIsInList(element As DependencyObject, value As Boolean)
            element.SetValue(IsInListProperty, value)
        End Sub

        ''' <summary>Ce qui prend le focus pour la liste dans la barre : sa commande montrée.</summary>
        Friend ReadOnly Property FocusTarget As UIElement
            Get
                Return _principale
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _principale IsNot Nothing Then
                RemoveHandler _principale.Click, AddressOf OnPrincipale
                RemoveHandler _principale.PreviewKeyDown, AddressOf OnToucheDansLaBarre
            End If
            If _fleche IsNot Nothing Then RemoveHandler _fleche.PreviewKeyDown, AddressOf OnToucheDansLaBarre
            If _liste IsNot Nothing Then
                RemoveHandler _liste.Opened, AddressOf OnListeOuverte
                RemoveHandler _liste.Closed, AddressOf OnListeFermee
                RemoveHandler _liste.PreviewKeyDown, AddressOf OnToucheDansLaListe
            End If

            MyBase.OnApplyTemplate()
            _principale = TryCast(GetTemplateChild("PART_Main"), ButtonBase)
            _fleche = TryCast(GetTemplateChild("PART_Arrow"), ToggleButton)
            _liste = TryCast(GetTemplateChild("PART_Popup"), Popup)

            If _principale IsNot Nothing Then
                AddHandler _principale.Click, AddressOf OnPrincipale
                AddHandler _principale.PreviewKeyDown, AddressOf OnToucheDansLaBarre
            End If
            If _fleche IsNot Nothing Then AddHandler _fleche.PreviewKeyDown, AddressOf OnToucheDansLaBarre
            If _liste IsNot Nothing Then
                AddHandler _liste.Opened, AddressOf OnListeOuverte
                AddHandler _liste.Closed, AddressOf OnListeFermee
                AddHandler _liste.PreviewKeyDown, AddressOf OnToucheDansLaListe
            End If
            MettreAJour()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            MettreAJour()
        End Sub

        ''' <summary>La commande montrée : celle à bascule enfoncée, sinon la dernière lancée, sinon la première.</summary>
        Friend Sub MettreAJour()
            Dim commandes = Items.OfType(Of UIElement)().ToList()
            Dim enfoncee = commandes.OfType(Of ToggleButton)().FirstOrDefault(Function(b) b.IsChecked.GetValueOrDefault())
            Dim montree = If(enfoncee, If(commandes.Contains(_derniere), _derniere, commandes.FirstOrDefault()))
            SetValue(ShownItemPropertyKey, montree)
        End Sub

        ' La commande montrée se lance par celle qu'elle montre ; son propre clic ne remonte pas plus haut.
        Private Sub OnPrincipale(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            TryCast(ShownItem, IActionBarCommand)?.PerformClick()
        End Sub

        ' Une commande de la liste lancée devient la commande montrée, et la liste se ferme.
        Private Sub OnCommandeLancee(sender As Object, e As RoutedEventArgs)
            Dim source = TryCast(e.OriginalSource, UIElement)
            If source Is Nothing OrElse Not Items.Contains(source) Then Return
            _derniere = source
            SetCurrentValue(IsOpenProperty, False)
            MettreAJour()
        End Sub

        Private Sub OnBascule(sender As Object, e As RoutedEventArgs)
            MettreAJour()
        End Sub

        ' Pendant que la liste est ouverte, la flèche ne la rouvre pas d'un clic qui vient de la fermer.
        Private Sub OnListeOuverte(sender As Object, e As EventArgs)
            If _fleche IsNot Nothing Then _fleche.IsHitTestVisible = False
        End Sub

        ' Fermée, la liste rend le focus à la commande montrée, comme Échap.
        Private Sub OnListeFermee(sender As Object, e As EventArgs)
            If _fleche IsNot Nothing Then _fleche.IsHitTestVisible = True
            If IsKeyboardFocusWithin OrElse (_liste IsNot Nothing AndAlso _liste.IsKeyboardFocusWithin) Then _principale?.Focus()
        End Sub

        ' Flèche haut ou bas sur la commande ou sa flèche : la liste s'ouvre, le focus sur sa première commande.
        Private Sub OnToucheDansLaBarre(sender As Object, e As KeyEventArgs)
            If e.Key <> Key.Down AndAlso e.Key <> Key.Up Then Return
            e.Handled = True
            SetCurrentValue(IsOpenProperty, True)
            Dispatcher.BeginInvoke(DispatcherPriority.Input, New Action(AddressOf FocaliserLaPremiere))
        End Sub

        Private Sub OnToucheDansLaListe(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Escape Then
                e.Handled = True
                SetCurrentValue(IsOpenProperty, False)
                _principale?.Focus()
            End If
        End Sub

        Private Sub FocaliserLaPremiere()
            Dim premiere = Items.OfType(Of UIElement)().FirstOrDefault(Function(c) c.IsEnabled AndAlso c.Focusable)
            premiere?.Focus()
        End Sub

    End Class

End Namespace
