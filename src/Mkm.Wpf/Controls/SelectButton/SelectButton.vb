Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le choix par boutons de mkm (<c>SelectButton</c>) : des boutons à bascule collés, 24 px de haut, coins de 4 px
    ''' aux bouts du groupe ; le bouton choisi prend la sélection de 3DEXPERIENCE (le fond bleu pâle) bordée de la
    ''' couleur principale. Un seul choix par défaut, plusieurs avec <c>SelectionMode="Multiple"</c> ; un nouveau clic
    ''' sur le choix l'enlève (<see cref="AllowEmpty"/>).
    ''' <code>
    ''' &lt;mkm:SelectButton SelectedItem="{Binding Vue}"&gt;
    '''     &lt;mkm:SelectButtonItem Content="Plan" /&gt;
    '''     &lt;mkm:SelectButtonItem Content="Article" /&gt;
    ''' &lt;/mkm:SelectButton&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>C'est un <see cref="System.Windows.Controls.ListBox"/> : <c>ItemsSource</c>, <c>SelectedItem</c>, <c>SelectedItems</c>, les flèches.</remarks>
    Public Class SelectButton
        Inherits System.Windows.Controls.ListBox

        ''' <summary>Vrai (par défaut) si un clic sur le choix l'enlève, ne laissant aucun choix.</summary>
        Public Shared ReadOnly AllowEmptyProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AllowEmpty), GetType(Boolean), GetType(SelectButton), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai si le choix est refusé : le groupe se cerne de la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = InputText.InvalidProperty.AddOwner(GetType(SelectButton))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(SelectButton), New FrameworkPropertyMetadata(GetType(SelectButton)))
        End Sub

        ''' <summary>Vrai (par défaut) si un clic sur le choix l'enlève.</summary>
        Public Property AllowEmpty As Boolean
            Get
                Return CBool(GetValue(AllowEmptyProperty))
            End Get
            Set(value As Boolean)
                SetValue(AllowEmptyProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le choix est refusé.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is SelectButtonItem
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New SelectButtonItem()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            MarquerLesBouts()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            MarquerLesBouts()
        End Sub

        ' Le premier et le dernier bouton ont les coins du groupe ; les autres n'ont pas de bordure à gauche.
        Friend Sub MarquerLesBouts()
            Dim boutons = Enumerable.Range(0, Items.Count).
                Select(Function(i) TryCast(ItemContainerGenerator.ContainerFromIndex(i), SelectButtonItem)).
                Where(Function(b) b IsNot Nothing AndAlso b.Visibility <> Visibility.Collapsed).
                ToList()
            For Each bouton In boutons
                bouton.Marquer(bouton Is boutons.First(), bouton Is boutons.Last())
            Next
        End Sub

    End Class

    ''' <summary>Un bouton de <see cref="SelectButton"/> : son libellé (le contenu) et une icône s'il le faut.</summary>
    Public Class SelectButtonItem
        Inherits ListBoxItem

        ''' <summary>L'icône devant le libellé.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(SelectButtonItem), New FrameworkPropertyMetadata(Nothing))

        Private Shared ReadOnly IsFirstPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFirst), GetType(Boolean), GetType(SelectButtonItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le premier bouton du groupe.</summary>
        Public Shared ReadOnly IsFirstProperty As DependencyProperty = IsFirstPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLastPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLast), GetType(Boolean), GetType(SelectButtonItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le dernier bouton du groupe.</summary>
        Public Shared ReadOnly IsLastProperty As DependencyProperty = IsLastPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(SelectButtonItem), New FrameworkPropertyMetadata(GetType(SelectButtonItem)))
        End Sub

        ''' <summary>L'icône devant le libellé.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour le premier bouton du groupe.</summary>
        Public ReadOnly Property IsFirst As Boolean
            Get
                Return CBool(GetValue(IsFirstProperty))
            End Get
        End Property

        ''' <summary>Vrai pour le dernier bouton du groupe.</summary>
        Public ReadOnly Property IsLast As Boolean
            Get
                Return CBool(GetValue(IsLastProperty))
            End Get
        End Property

        Friend Sub Marquer(premier As Boolean, dernier As Boolean)
            SetValue(IsFirstPropertyKey, premier)
            SetValue(IsLastPropertyKey, dernier)
        End Sub

        ' Comme allowEmpty de mkm : un clic sur le seul choix l'enlève (en choix multiple, WPF le fait déjà) ; sans
        ' AllowEmpty, le dernier choix ne s'enlève ni d'un clic, ni d'un Ctrl+clic, ni en choix multiple.
        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeftButtonDown(e As MouseButtonEventArgs)
            Dim groupe = TryCast(ItemsControl.ItemsControlFromItemContainer(Me), SelectButton)
            If IsSelected AndAlso groupe IsNot Nothing Then
                If Not groupe.AllowEmpty AndAlso groupe.SelectedItems.Count <= 1 Then
                    Focus()
                    e.Handled = True
                    Return
                End If
                If groupe.SelectionMode = SelectionMode.Single AndAlso groupe.AllowEmpty Then
                    Focus()
                    ' SetCurrentValue garde une liaison d'IsSelected (un ItemContainerStyle).
                    SetCurrentValue(IsSelectedProperty, False)
                    e.Handled = True
                    Return
                End If
            End If
            MyBase.OnMouseLeftButtonDown(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            ' Espace sur le dernier choix, sans AllowEmpty : il reste choisi.
            Dim groupe = TryCast(ItemsControl.ItemsControlFromItemContainer(Me), SelectButton)
            If e.Key = System.Windows.Input.Key.Space AndAlso IsSelected AndAlso groupe IsNot Nothing AndAlso
               Not groupe.AllowEmpty AndAlso groupe.SelectedItems.Count <= 1 Then
                e.Handled = True
                Return
            End If
            MyBase.OnKeyDown(e)
        End Sub

    End Class

End Namespace
