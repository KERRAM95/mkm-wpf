Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le panneau d'accueil de mkm (<c>WelcomePanel</c>), celui des applications 3DEXPERIENCE : un titre et une
    ''' description, puis des rubriques (<see cref="WelcomePanelSection"/>) d'entrées (<see cref="WelcomePanelItem"/>),
    ''' séparées au besoin d'un trait (<see cref="WelcomePanelSeparator"/>), et un pied. L'entrée choisie
    ''' (<see cref="Selection"/>, sa clé) est bleue, marquée d'un trait de 2 px à gauche. Replié
    ''' (<see cref="IsCollapsed"/>), il ne montre que les icônes, leur libellé en info-bulle.
    ''' <code>
    ''' &lt;mkm:WelcomePanel Title="HS CART" Selection="{Binding Vue}"&gt;
    '''     &lt;mkm:WelcomePanelSection Header="Plans"&gt;
    '''         &lt;mkm:WelcomePanelItem Key="recents" Icon="{x:Static mkm:Icons.History}" Content="Récents" /&gt;
    '''         &lt;mkm:WelcomePanelItem Key="tous" Icon="{x:Static mkm:Icons.Drawings}" Content="Tous les plans" /&gt;
    '''     &lt;/mkm:WelcomePanelSection&gt;
    ''' &lt;/mkm:WelcomePanel&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Au clavier, tout le panneau n'est qu'un arrêt de tabulation : l'entrée choisie, ou la dernière qui a eu le focus ;
    ''' les flèches haut et bas, Début et Fin passent d'une entrée à l'autre. Une entrée d'action
    ''' (<see cref="WelcomePanelItem.Selectable"/> à faux, ou sans clé) lance sa commande sans devenir l'entrée choisie.
    ''' </remarks>
    Public Class WelcomePanel
        Inherits ItemsControl

        ''' <summary>Le titre, en haut.</summary>
        Public Shared ReadOnly TitleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Title), GetType(String), GetType(WelcomePanel), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La description, sous le titre.</summary>
        Public Shared ReadOnly DescriptionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Description), GetType(String), GetType(WelcomePanel), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Un en-tête à la place du titre et de la description.</summary>
        Public Shared ReadOnly HeaderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Header), GetType(Object), GetType(WelcomePanel), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le pied, en bas du panneau.</summary>
        Public Shared ReadOnly FooterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Footer), GetType(Object), GetType(WelcomePanel), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La clé de l'entrée choisie.</summary>
        Public Shared ReadOnly SelectionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Selection), GetType(String), GetType(WelcomePanel),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnSelectionChanged))

        ''' <summary>Vrai quand le panneau est replié : les icônes seules, leur libellé en info-bulle.</summary>
        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsCollapsed), GetType(Boolean), GetType(WelcomePanel),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits Or FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>Quand une entrée est choisie, au clic ou au clavier ; sa source est l'entrée.</summary>
        Public Shared ReadOnly ItemClickEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(ItemClick), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(WelcomePanel))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(WelcomePanel), New FrameworkPropertyMetadata(GetType(WelcomePanel)))
            FocusableProperty.OverrideMetadata(GetType(WelcomePanel), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un panneau vide.</summary>
        Public Sub New()
            [AddHandler](ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf OnEntree))
            [AddHandler](Keyboard.GotKeyboardFocusEvent, New KeyboardFocusChangedEventHandler(AddressOf OnFocus))
            AddHandler Loaded, Sub(s, e) MettreAJour()
        End Sub

        ''' <summary>Quand une entrée est choisie, au clic ou au clavier.</summary>
        Public Custom Event ItemClick As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                [AddHandler](ItemClickEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                [RemoveHandler](ItemClickEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Le titre, en haut.</summary>
        Public Property Title As String
            Get
                Return CStr(GetValue(TitleProperty))
            End Get
            Set(value As String)
                SetValue(TitleProperty, value)
            End Set
        End Property

        ''' <summary>La description, sous le titre.</summary>
        Public Property Description As String
            Get
                Return CStr(GetValue(DescriptionProperty))
            End Get
            Set(value As String)
                SetValue(DescriptionProperty, value)
            End Set
        End Property

        ''' <summary>Un en-tête à la place du titre et de la description.</summary>
        Public Property Header As Object
            Get
                Return GetValue(HeaderProperty)
            End Get
            Set(value As Object)
                SetValue(HeaderProperty, value)
            End Set
        End Property

        ''' <summary>Le pied, en bas du panneau.</summary>
        Public Property Footer As Object
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As Object)
                SetValue(FooterProperty, value)
            End Set
        End Property

        ''' <summary>La clé de l'entrée choisie.</summary>
        Public Property Selection As String
            Get
                Return CStr(GetValue(SelectionProperty))
            End Get
            Set(value As String)
                SetValue(SelectionProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le panneau est replié.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        ''' <summary>Replie ou déplie le panneau.</summary>
        Public Sub Toggle()
            SetCurrentValue(IsCollapsedProperty, Not IsCollapsed)
        End Sub

        ''' <summary>Les entrées, dans l'ordre, rubrique après rubrique.</summary>
        Public Function GetItems() As IList(Of WelcomePanelItem)
            Dim resultat As New List(Of WelcomePanelItem)
            For Each element In Items
                Dim rubrique = TryCast(element, WelcomePanelSection)
                If rubrique IsNot Nothing Then resultat.AddRange(rubrique.Items.OfType(Of WelcomePanelItem)())
                Dim entree = TryCast(element, WelcomePanelItem)
                If entree IsNot Nothing Then resultat.Add(entree)
            Next
            Return resultat
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is WelcomePanelSection OrElse TypeOf item Is WelcomePanelSeparator OrElse TypeOf item Is WelcomePanelItem
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New WelcomePanelSection()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            MettreAJour()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled Then Return
            Dim entrees = GetItems().Where(Function(i) i.IsEnabled AndAlso i.IsVisible).ToList()
            If entrees.Count = 0 Then Return
            Dim actuelle = entrees.FindIndex(Function(i) i.IsKeyboardFocused)
            Dim cible As Integer
            Select Case e.Key
                Case Key.Down
                    cible = If(actuelle < 0, 0, Math.Min(actuelle + 1, entrees.Count - 1))
                Case Key.Up
                    cible = If(actuelle < 0, 0, Math.Max(actuelle - 1, 0))
                Case Key.Home
                    cible = 0
                Case Key.End
                    cible = entrees.Count - 1
                Case Else
                    Return
            End Select
            entrees(cible).Focus()
            e.Handled = True
        End Sub

        ''' <summary>Les entrées marquées (choisie ou non), et l'arrêt de tabulation posé sur l'une d'elles.</summary>
        Friend Sub MettreAJour(Optional active As WelcomePanelItem = Nothing)
            Dim entrees = GetItems()
            For Each entree In entrees
                entree.Marquer(entree.IsSelectable AndAlso Selection IsNot Nothing AndAlso String.Equals(entree.Key, Selection, StringComparison.Ordinal))
            Next
            ' Un seul arrêt de tabulation : l'entrée qui a eu le focus en dernier, sinon la choisie, sinon la première.
            If active Is Nothing Then active = entrees.FirstOrDefault(Function(i) i.IsKeyboardFocusWithin)
            If active Is Nothing Then active = entrees.FirstOrDefault(Function(i) i.IsSelected)
            If active Is Nothing Then active = entrees.FirstOrDefault(Function(i) i.IsEnabled)
            For Each entree In entrees
                entree.IsTabStop = entree Is active
            Next
        End Sub

        Private Sub OnEntree(sender As Object, e As RoutedEventArgs)
            Dim entree = TryCast(e.OriginalSource, WelcomePanelItem)
            If entree Is Nothing Then Return
            If entree.IsSelectable AndAlso Not String.Equals(Selection, entree.Key, StringComparison.Ordinal) Then
                SetCurrentValue(SelectionProperty, entree.Key)
            End If
            Me.RaiseEvent(New RoutedEventArgs(ItemClickEvent, entree))
        End Sub

        Private Sub OnFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
            Dim entree = TryCast(e.NewFocus, WelcomePanelItem)
            If entree IsNot Nothing Then MettreAJour(entree)
        End Sub

        Private Shared Sub OnSelectionChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, WelcomePanel).MettreAJour()
        End Sub

    End Class

    ''' <summary>Une rubrique d'un <see cref="WelcomePanel"/> : son libellé (<c>Header</c>) et ses entrées.</summary>
    Public Class WelcomePanelSection
        Inherits HeaderedItemsControl

        ''' <summary>Vrai quand le panneau est replié : la rubrique perd son libellé.</summary>
        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = WelcomePanel.IsCollapsedProperty.AddOwner(
            GetType(WelcomePanelSection), New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(WelcomePanelSection), New FrameworkPropertyMetadata(GetType(WelcomePanelSection)))
            FocusableProperty.OverrideMetadata(GetType(WelcomePanelSection), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand le panneau est replié.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is WelcomePanelItem OrElse TypeOf item Is WelcomePanelSeparator
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New WelcomePanelItem()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            TryCast(ItemsControl.ItemsControlFromItemContainer(Me), WelcomePanel)?.MettreAJour()
        End Sub

    End Class

    ''' <summary>
    ''' Une entrée d'un <see cref="WelcomePanel"/> : son icône, son libellé (le contenu) ; un clic la choisit (sa
    ''' <see cref="Key"/> devient la sélection du panneau) et lance sa <c>Command</c>.
    ''' </summary>
    Public Class WelcomePanelItem
        Inherits ButtonBase

        ''' <summary>La clé de l'entrée : la sélection du panneau quand elle est choisie.</summary>
        Public Shared ReadOnly KeyProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Key), GetType(String), GetType(WelcomePanelItem), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'icône, devant le libellé.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(WelcomePanelItem), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Faux pour une entrée d'action : elle lance sa commande sans devenir l'entrée choisie.</summary>
        Public Shared ReadOnly SelectableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Selectable), GetType(Boolean), GetType(WelcomePanelItem), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai quand le panneau est replié : l'icône seule, le libellé en info-bulle.</summary>
        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = WelcomePanel.IsCollapsedProperty.AddOwner(
            GetType(WelcomePanelItem), New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits))

        Private Shared ReadOnly IsSelectedPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsSelected), GetType(Boolean), GetType(WelcomePanelItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour l'entrée choisie.</summary>
        Public Shared ReadOnly IsSelectedProperty As DependencyProperty = IsSelectedPropertyKey.DependencyProperty

        Private Shared ReadOnly IsFocusVisiblePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFocusVisible), GetType(Boolean), GetType(WelcomePanelItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand l'entrée a le focus du clavier, et non celui d'un clic.</summary>
        Public Shared ReadOnly IsFocusVisibleProperty As DependencyProperty = IsFocusVisiblePropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(WelcomePanelItem), New FrameworkPropertyMetadata(GetType(WelcomePanelItem)))
        End Sub

        ''' <summary>La clé de l'entrée.</summary>
        Public Property Key As String
            Get
                Return CStr(GetValue(KeyProperty))
            End Get
            Set(value As String)
                SetValue(KeyProperty, value)
            End Set
        End Property

        ''' <summary>L'icône, devant le libellé.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Faux pour une entrée d'action.</summary>
        Public Property Selectable As Boolean
            Get
                Return CBool(GetValue(SelectableProperty))
            End Get
            Set(value As Boolean)
                SetValue(SelectableProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le panneau est replié.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour l'entrée choisie.</summary>
        Public ReadOnly Property IsSelected As Boolean
            Get
                Return CBool(GetValue(IsSelectedProperty))
            End Get
        End Property

        ''' <summary>Vrai quand l'entrée a le focus du clavier, et non celui d'un clic.</summary>
        Public ReadOnly Property IsFocusVisible As Boolean
            Get
                Return CBool(GetValue(IsFocusVisibleProperty))
            End Get
        End Property

        ''' <summary>Vrai si l'entrée peut devenir la choisie : elle a une clé et n'est pas une action.</summary>
        Friend ReadOnly Property IsSelectable As Boolean
            Get
                Return Selectable AndAlso Key IsNot Nothing
            End Get
        End Property

        Friend Sub Marquer(choisie As Boolean)
            SetValue(IsSelectedPropertyKey, choisie)
        End Sub

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

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            ' Entrée choisit l'entrée, comme Espace (que ButtonBase traite déjà).
            If e.Key = System.Windows.Input.Key.Enter AndAlso Not e.Handled Then
                e.Handled = True
                OnClick()
                Return
            End If
            MyBase.OnKeyDown(e)
        End Sub

    End Class

    ''' <summary>Le trait entre deux rubriques ou deux entrées d'un <see cref="WelcomePanel"/>.</summary>
    Public Class WelcomePanelSeparator
        Inherits Control

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(WelcomePanelSeparator), New FrameworkPropertyMetadata(GetType(WelcomePanelSeparator)))
            FocusableProperty.OverrideMetadata(GetType(WelcomePanelSeparator), New FrameworkPropertyMetadata(False))
        End Sub

    End Class

End Namespace
