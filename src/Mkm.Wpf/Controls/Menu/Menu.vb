Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le menu de mkm (<c>Menu</c>), posé dans la page : le cadre du menu de 3DEXPERIENCE (blanc, bordé de gris clair,
    ''' sans arrondi, 150 px de large au moins), ses éléments de 24 px (les <see cref="MenuItem"/> de WPF, au style de
    ''' mkm), regroupés au besoin sous un libellé gras (<see cref="MenuGroup"/>) et séparés d'un trait. <see cref="Start"/>
    ''' et <see cref="[End]"/> reçoivent un contenu au-dessus et au-dessous de la liste.
    ''' <code>
    ''' &lt;mkm:Menu&gt;
    '''     &lt;mkm:MenuGroup Header="Plan"&gt;
    '''         &lt;MenuItem Header="Ouvrir" Icon="{x:Static mkm:Icons.Open}" /&gt;
    '''         &lt;MenuItem Header="Exporter" Icon="{x:Static mkm:Icons.Export}" /&gt;
    '''     &lt;/mkm:MenuGroup&gt;
    '''     &lt;Separator /&gt;
    '''     &lt;MenuItem Header="Paramètres" Icon="{x:Static mkm:Icons.Settings}" /&gt;
    ''' &lt;/mkm:Menu&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Le menu qui s'ouvre d'un bouton (le <c>popup</c> de mkm) est un <see cref="ContextMenu"/> de WPF, qui a déjà le
    ''' style de mkm. Comme dans tout menu de mkm, un élément sans icône garde la colonne d'icône si un autre en a une.
    ''' « Menu » est aussi le nom du menu de WPF : qualifier (<c>Mkm.Wpf.Controls.Menu</c>) là où les deux espaces de
    ''' noms sont importés.
    ''' </remarks>
    Public Class Menu
        Inherits MenuBase

        ''' <summary>Tout son arbre visuel pour les lecteurs d'écran et l'automatisation, gabarit compris.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Menu)
        End Function

        ''' <summary>Un contenu au-dessus de la liste.</summary>
        Public Shared ReadOnly StartProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Start), GetType(Object), GetType(Menu), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Un contenu au-dessous de la liste.</summary>
        Public Shared ReadOnly EndProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([End]), GetType(Object), GetType(Menu), New FrameworkPropertyMetadata(Nothing))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Menu), New FrameworkPropertyMetadata(GetType(Menu)))
            Colonne.UnSeulArret(GetType(Menu))
        End Sub

        ''' <summary>Un menu vide.</summary>
        Public Sub New()
            AddHandler Loaded, Sub(s, e) Colonne.Poser(Me)
        End Sub

        ''' <summary>Un contenu au-dessus de la liste.</summary>
        Public Property Start As Object
            Get
                Return GetValue(StartProperty)
            End Get
            Set(value As Object)
                SetValue(StartProperty, value)
            End Set
        End Property

        ''' <summary>Un contenu au-dessous de la liste.</summary>
        Public Property [End] As Object
            Get
                Return GetValue(EndProperty)
            End Get
            Set(value As Object)
                SetValue(EndProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is MenuItem OrElse TypeOf item Is Separator OrElse TypeOf item Is MenuGroup
        End Function

        ''' <summary>Le survol d'un élément le montre sans prendre le focus (<see cref="Colonne.RefuserLeSurvol"/>).</summary>
        Protected Overrides Sub OnPreviewGotKeyboardFocus(e As System.Windows.Input.KeyboardFocusChangedEventArgs)
            If Colonne.RefuserLeSurvol(e) Then Return
            MyBase.OnPreviewGotKeyboardFocus(e)
        End Sub

        ''' <summary>
        ''' Posé dans la page, le menu ne passe pas en mode menu quand le focus y entre : il ne capture pas la souris (le
        ''' mode menu de <see cref="MenuBase"/> est celui d'un menu qui s'ouvre).
        ''' </summary>
        Protected Overrides Sub OnIsKeyboardFocusWithinChanged(e As DependencyPropertyChangedEventArgs)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Colonne.Poser(Me)
        End Sub

        ''' <summary>Les traits prennent le style des menus de mkm (MenuBase ne le fait pas, à la différence du Menu de WPF).</summary>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Colonne.StylerLeTrait(element)
        End Sub

    End Class

    ''' <summary>
    ''' Un groupe d'un <see cref="Menu"/> : son libellé (<c>Header</c>), en gras, puis ses éléments, dans le menu même (le
    ''' <c>submenuLabel</c> de mkm, qui n'ouvre rien).
    ''' </summary>
    Public Class MenuGroup
        Inherits HeaderedItemsControl

        ''' <summary>Tout son arbre visuel pour les lecteurs d'écran et l'automatisation, gabarit compris.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(MenuGroup), New FrameworkPropertyMetadata(GetType(MenuGroup)))
            FocusableProperty.OverrideMetadata(GetType(MenuGroup), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is MenuItem OrElse TypeOf item Is Separator
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New MenuItem()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            ' Le trait d'un groupe : celui des menus (un menu le pose lui-même sur les siens).
            Colonne.StylerLeTrait(element)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Dim menu = TryCast(ItemsControl.ItemsControlFromItemContainer(Me), Menu)
            If menu IsNot Nothing Then Colonne.Poser(menu)
        End Sub

    End Class

    ''' <summary>
    ''' Le menu à étages de mkm (<c>TieredMenu</c>), posé dans la page : le cadre du menu de 3DEXPERIENCE, ses éléments
    ''' de 24 px ; un élément qui a des éléments ouvre son sous-menu à sa droite (au survol, au clic, ou par la flèche
    ''' droite), et ainsi de suite.
    ''' <code>
    ''' &lt;mkm:TieredMenu&gt;
    '''     &lt;MenuItem Header="Exporter" Icon="{x:Static mkm:Icons.Export}"&gt;
    '''         &lt;MenuItem Header="PDF" /&gt;
    '''         &lt;MenuItem Header="DXF" /&gt;
    '''     &lt;/MenuItem&gt;
    ''' &lt;/mkm:TieredMenu&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>Le menu à étages qui s'ouvre d'un bouton est un <see cref="ContextMenu"/> de WPF, aux sous-menus déjà de mkm.</remarks>
    Public Class TieredMenu
        Inherits MenuBase

        ''' <summary>Tout son arbre visuel pour les lecteurs d'écran et l'automatisation, gabarit compris.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Menu)
        End Function

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(TieredMenu), New FrameworkPropertyMetadata(GetType(TieredMenu)))
            Colonne.UnSeulArret(GetType(TieredMenu))
        End Sub

        ''' <summary>Un menu à étages vide.</summary>
        Public Sub New()
            AddHandler Loaded, Sub(s, e) Colonne.Poser(Me)
            [AddHandler](MenuItem.ClickEvent, New RoutedEventHandler(AddressOf ElementClique))
        End Sub

        ' Un élément choisi referme les étages ouverts, comme mkm. Posé dans la page, le menu n'entre pas en mode menu
        ' (son survol ne prend pas le clavier) : WPF, qui ferme les étages en quittant ce mode, les laissait ouverts.
        Private Sub ElementClique(sender As Object, e As RoutedEventArgs)
            Dim element = TryCast(e.OriginalSource, MenuItem)
            If element Is Nothing OrElse element.HasItems OrElse element.StaysOpenOnClick Then Return
            For Each item In Items
                Fermer(TryCast(If(TryCast(item, MenuItem), ItemContainerGenerator.ContainerFromItem(item)), MenuItem))
            Next
        End Sub

        Private Shared Sub Fermer(element As MenuItem)
            If element Is Nothing Then Return
            For Each item In element.Items
                Fermer(TryCast(If(TryCast(item, MenuItem), element.ItemContainerGenerator.ContainerFromItem(item)), MenuItem))
            Next
            element.IsSubmenuOpen = False
        End Sub

        ''' <summary>Le survol d'un élément le montre (et ouvre son étage) sans prendre le focus.</summary>
        Protected Overrides Sub OnPreviewGotKeyboardFocus(e As System.Windows.Input.KeyboardFocusChangedEventArgs)
            If Colonne.RefuserLeSurvol(e) Then Return
            MyBase.OnPreviewGotKeyboardFocus(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Colonne.Poser(Me)
        End Sub

        ''' <summary>Les traits prennent le style des menus de mkm (MenuBase ne le fait pas, à la différence du Menu de WPF).</summary>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Colonne.StylerLeTrait(element)
        End Sub

    End Class

    ' La colonne d'icône d'un menu posé dans la page : gardée si un élément de la liste (ou d'un groupe) a une icône ou
    ' se coche, comme à l'ouverture d'un menu contextuel (MenuIcons).
    Friend NotInheritable Class Colonne

        ' Un trait de menu (Separator) au style des menus de mkm, sauf un style posé par l'application.
        Friend Shared Sub StylerLeTrait(element As DependencyObject)
            Dim trait = TryCast(element, Separator)
            If trait IsNot Nothing AndAlso trait.ReadLocalValue(FrameworkElement.StyleProperty) Is DependencyProperty.UnsetValue Then
                trait.SetResourceReference(FrameworkElement.StyleProperty, MenuItem.SeparatorStyleKey)
            End If
        End Sub

        ' Un menu posé dans la page n'est qu'un arrêt de la touche Tab, sur un de ses éléments (mkm) : pas lui-même, ni
        ' chacun de ses éléments ; sans le cadre pointillé de WPF.
        Friend Shared Sub UnSeulArret(type As System.Type)
            Control.IsTabStopProperty.OverrideMetadata(type, New FrameworkPropertyMetadata(False))
            FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(type, New FrameworkPropertyMetadata(Nothing))
            System.Windows.Input.KeyboardNavigation.TabNavigationProperty.OverrideMetadata(type, New FrameworkPropertyMetadata(System.Windows.Input.KeyboardNavigationMode.Once))
        End Sub

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Vrai, et le focus refusé, quand il vient du survol d'un élément (la souris, bouton relâché) : WPF donne le focus
        ''' à l'élément de menu survolé, et celui qui tapait dans un champ le perdrait. Le survol reste montré
        ''' (<c>IsHighlighted</c>) ; un clic et le clavier, eux, donnent le focus.
        ''' </summary>
        Friend Shared Function RefuserLeSurvol(e As System.Windows.Input.KeyboardFocusChangedEventArgs) As Boolean
            If Not TypeOf e.NewFocus Is MenuItem Then Return False
            If Not (System.Windows.Input.InputManager.Current.MostRecentInputDevice Is System.Windows.Input.Mouse.PrimaryDevice) Then Return False
            If System.Windows.Input.Mouse.LeftButton = System.Windows.Input.MouseButtonState.Pressed OrElse
               System.Windows.Input.Mouse.RightButton = System.Windows.Input.MouseButtonState.Pressed Then Return False
            e.Handled = True
            Return True
        End Function

        Friend Shared Sub Poser(liste As ItemsControl)
            Dim elements = liste.Items.OfType(Of MenuItem)().
                Concat(liste.Items.OfType(Of MenuGroup)().SelectMany(Function(g) g.Items.OfType(Of MenuItem)()))
            MenuIcons.SetHasIcons(liste, elements.Any(Function(m) m.Icon IsNot Nothing OrElse m.IsCheckable))
        End Sub

    End Class

End Namespace
