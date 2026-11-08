Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Markup

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le méga-menu de mkm (<c>MegaMenu</c>) : une barre (ou une colonne) d'éléments ; chacun ouvre un grand panneau, en
    ''' colonnes de groupes de liens.
    ''' <code>
    ''' &lt;mkm:MegaMenu&gt;
    '''     &lt;mkm:MegaMenuItem Header="Produits"&gt;
    '''         &lt;mkm:MegaMenuColumn&gt;
    '''             &lt;mkm:MenuGroup Header="Pièces"&gt;
    '''                 &lt;MenuItem Header="Usinées" /&gt;
    '''             &lt;/mkm:MenuGroup&gt;
    '''         &lt;/mkm:MegaMenuColumn&gt;
    '''     &lt;/mkm:MegaMenuItem&gt;
    ''' &lt;/mkm:MegaMenu&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Le <see cref="System.Windows.Controls.Menu"/> de WPF : Alt, F10, les flèches entre les éléments, ↓ ouvre le panneau.
    ''' Un lien choisi referme le panneau.
    ''' </remarks>
    Public Class MegaMenu
        Inherits System.Windows.Controls.Menu

        ''' <summary>En ligne (par défaut) ou en colonne ; en colonne, les panneaux s'ouvrent à droite.</summary>
        Public Shared ReadOnly OrientationProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Orientation", GetType(Orientation), GetType(MegaMenu),
            New FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.Inherits))

        ''' <summary>Ce qui se pose avant les éléments.</summary>
        Public Shared ReadOnly StartProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Start), GetType(Object), GetType(MegaMenu), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Ce qui se pose après les éléments.</summary>
        Public Shared ReadOnly EndProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([End]), GetType(Object), GetType(MegaMenu), New FrameworkPropertyMetadata(Nothing))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(MegaMenu), New FrameworkPropertyMetadata(GetType(MegaMenu)))
        End Sub

        Public Sub New()
            [AddHandler](MenuItem.ClickEvent, New RoutedEventHandler(AddressOf Lien_Click))
        End Sub

        ''' <summary>L'orientation du méga-menu qui contient l'élément (héritée).</summary>
        Public Shared Function GetOrientation(element As DependencyObject) As Orientation
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return DirectCast(element.GetValue(OrientationProperty), Orientation)
        End Function

        ''' <summary>Pose l'orientation.</summary>
        Public Shared Sub SetOrientation(element As DependencyObject, value As Orientation)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(OrientationProperty, value)
        End Sub

        ''' <summary>En ligne ou en colonne.</summary>
        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui se pose avant les éléments.</summary>
        Public Property Start As Object
            Get
                Return GetValue(StartProperty)
            End Get
            Set(value As Object)
                SetValue(StartProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui se pose après les éléments.</summary>
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
            Return TypeOf item Is MegaMenuItem OrElse TypeOf item Is Separator
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New MegaMenuItem()
        End Function

        ' Un lien choisi (un élément sans suite, au fond d'un panneau) referme le panneau ouvert.
        Private Sub Lien_Click(sender As Object, e As RoutedEventArgs)
            Dim lien = TryCast(e.OriginalSource, MenuItem)
            If lien Is Nothing OrElse TypeOf lien Is MegaMenuItem OrElse lien.HasItems Then Return
            For Each element In Items.OfType(Of MegaMenuItem)()
                element.IsSubmenuOpen = False
            Next
        End Sub

    End Class

    ''' <summary>
    ''' Un élément de <see cref="MegaMenu"/> : son libellé, son icône, et son panneau, des colonnes
    ''' (<see cref="MegaMenuColumn"/>) de groupes de liens (<see cref="MenuGroup"/>) ; sans colonnes, un lien.
    ''' </summary>
    Public Class MegaMenuItem
        Inherits MenuItem

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(MegaMenuItem), New FrameworkPropertyMetadata(GetType(MegaMenuItem)))
        End Sub

        Public Sub New()
            AddHandler Loaded, Sub(s, e) PoserLaColonneDIcones()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is UIElement
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New MegaMenuColumn()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            PoserLaColonneDIcones()
        End Sub

        ' Les liens du panneau gardent la colonne de leurs icônes s'ils en ont, comme le menu de mkm.
        Private Sub PoserLaColonneDIcones()
            Dim liens = Items.OfType(Of MegaMenuColumn)().
                SelectMany(Function(c) c.Items.OfType(Of Object)()).
                SelectMany(Function(o) If(TypeOf o Is MenuGroup, DirectCast(o, MenuGroup).Items.OfType(Of MenuItem)(), If(TypeOf o Is MenuItem, {DirectCast(o, MenuItem)}, Enumerable.Empty(Of MenuItem)())))
            MenuIcons.SetHasIcons(Me, liens.Any(Function(m) m.Icon IsNot Nothing))
        End Sub

    End Class

    ''' <summary>Une colonne du panneau d'un <see cref="MegaMenuItem"/> : des groupes de liens, l'un sous l'autre.</summary>
    Public Class MegaMenuColumn
        Inherits ItemsControl

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(MegaMenuColumn), New FrameworkPropertyMetadata(GetType(MegaMenuColumn)))
            FocusableProperty.OverrideMetadata(GetType(MegaMenuColumn), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is MenuGroup OrElse TypeOf item Is MenuItem OrElse TypeOf item Is Separator
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New MenuItem()
        End Function

        ''' <summary>Un groupe pour les lecteurs d'écran.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

    End Class

End Namespace
