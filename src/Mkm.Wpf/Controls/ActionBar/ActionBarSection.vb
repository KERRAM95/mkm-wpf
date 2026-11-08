Imports System.Collections.Specialized
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une section d'<see cref="ActionBar"/> : son onglet (<c>Header</c>) et ses commandes, que la barre
    ''' montre quand la section est choisie.
    ''' <code>&lt;mkm:ActionBarSection Key="edition" Header="Édition"&gt; … &lt;/mkm:ActionBarSection&gt;</code>
    ''' </summary>
    ''' <remarks>
    ''' Le gabarit de la section dessine son onglet ; ses commandes restent ses enfants logiques (elles en
    ''' héritent le DataContext) et la barre les présente dans sa rangée.
    ''' </remarks>
    Public Class ActionBarSection
        Inherits HeaderedItemsControl

        ''' <summary>La clé de la section, que <see cref="ActionBar.ActiveSection"/> désigne.</summary>
        Public Shared ReadOnly KeyProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Key), GetType(String), GetType(ActionBarSection), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai quand la section est celle que la barre montre.</summary>
        Public Shared ReadOnly IsSelectedProperty As DependencyProperty = Selector.IsSelectedProperty.AddOwner(
            GetType(ActionBarSection),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsSelectedChanged))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ActionBarSection), New FrameworkPropertyMetadata(GetType(ActionBarSection)))
            VisibilityProperty.OverrideMetadata(GetType(ActionBarSection), New FrameworkPropertyMetadata(Visibility.Visible, AddressOf OnVisibilityChanged))
        End Sub

        ''' <summary>La clé de la section.</summary>
        Public Property Key As String
            Get
                Return CStr(GetValue(KeyProperty))
            End Get
            Set(value As String)
                SetValue(KeyProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la section est celle que la barre montre.</summary>
        Public Property IsSelected As Boolean
            Get
                Return CBool(GetValue(IsSelectedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsSelectedProperty, value)
            End Set
        End Property

        ''' <summary>La barre qui porte la section.</summary>
        Friend ReadOnly Property ParentBar As ActionBar
            Get
                Return TryCast(ItemsControl.ItemsControlFromItemContainer(Me), ActionBar)
            End Get
        End Property

        ' Choisie (un clic sur son onglet, Entrée, Espace, ou le code), la section se fait montrer par sa
        ' barre ; les événements de Selector suivent, pour qui les écoute.
        Private Shared Sub OnIsSelectedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim section = DirectCast(d, ActionBarSection)
            Dim choisie = CBool(e.NewValue)
            If choisie Then section.ParentBar?.Montrer(section)
            section.RaiseEvent(New RoutedEventArgs(If(choisie, Selector.SelectedEvent, Selector.UnselectedEvent), section))
        End Sub

        ' Une section cachée n'a plus d'onglet : la barre revoit les siens.
        Private Shared Sub OnVisibilityChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, ActionBarSection).ParentBar?.SectionVisibiliteChangee()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeftButtonDown(e As MouseButtonEventArgs)
            MyBase.OnMouseLeftButtonDown(e)
            SetCurrentValue(IsSelectedProperty, True)
            Focus()
            e.Handled = True
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled Then Return
            ' « Key » est ici la clé de la section : la touche se nomme en entier.
            If e.Key = Input.Key.Enter OrElse e.Key = Input.Key.Space Then
                SetCurrentValue(IsSelectedProperty, True)
                e.Handled = True
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            ParentBar?.MettreAJourEtat()
        End Sub

    End Class

End Namespace
