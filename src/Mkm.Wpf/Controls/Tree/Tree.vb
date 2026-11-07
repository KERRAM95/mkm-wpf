Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' L'arbre de mkm (<c>Tree</c>) : des nœuds (<see cref="TreeNode"/>) de 3 px sur 6 de retrait, leur flèche dans un
    ''' rond de 21 px, une icône grise, le nœud choisi surligné ; les enfants décalés de 12 px.
    ''' <code>
    ''' &lt;mkm:Tree&gt;
    '''     &lt;mkm:TreeNode Header="Notes" Icon="{x:Static mkm:Icons.Documents}" IsExpanded="True"&gt;
    '''         &lt;mkm:TreeNode Header="Note 1" /&gt;
    '''     &lt;/mkm:TreeNode&gt;
    ''' &lt;/mkm:Tree&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' C'est le <see cref="TreeView"/> de WPF : flèches, + et -, *, le nœud choisi (<c>SelectedItem</c>), les nœuds
    ''' liés à des données (<c>ItemsSource</c>, <c>HierarchicalDataTemplate</c>), la virtualisation.
    ''' </remarks>
    Public Class Tree
        Inherits TreeView

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Tree), New FrameworkPropertyMetadata(GetType(Tree)))
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is TreeNode
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New TreeNode()
        End Function

    End Class

    ''' <summary>Un nœud de <see cref="Tree"/> : son titre (<c>Header</c>), son icône, ses enfants.</summary>
    Public Class TreeNode
        Inherits TreeViewItem

        ''' <summary>L'icône devant le titre (aucune par défaut).</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(TreeNode), New FrameworkPropertyMetadata(Nothing))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(TreeNode), New FrameworkPropertyMetadata(GetType(TreeNode)))
        End Sub

        ''' <summary>L'icône devant le titre (aucune par défaut).</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is TreeNode
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New TreeNode()
        End Function

    End Class

End Namespace
