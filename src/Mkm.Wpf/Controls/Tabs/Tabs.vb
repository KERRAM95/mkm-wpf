Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>Le dessin des onglets de <see cref="Tabs"/>.</summary>
    Public Enum TabsVariant
        ''' <summary>Le groupe de boutons au milieu de la barre, dans son cadre (par défaut).</summary>
        Group
        ''' <summary>Les onglets soulignés des facettes d'un panneau, sans cadre.</summary>
        Strip
    End Enum

    ''' <summary>
    ''' Les onglets de mkm, au rendu de 3DEXPERIENCE : un groupe de boutons au milieu d'une barre de
    ''' 26 px, dans un cadre gris clair bordé de gris sous une ombre courte ; chaque onglet de 23 px,
    ''' 65 px de large au moins ; l'onglet choisi d'un bleu pâle. Ni survol ni appui ne les change.
    ''' <c>Variant="Strip"</c> donne les onglets soulignés des facettes d'un panneau.
    ''' <code>
    ''' &lt;mkm:Tabs&gt;
    '''     &lt;mkm:Tab Header="Cartouche"&gt; … &lt;/mkm:Tab&gt;
    '''     &lt;mkm:Tab Header="Notes"&gt; … &lt;/mkm:Tab&gt;
    ''' &lt;/mkm:Tabs&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>C'est un <see cref="TabControl"/> : sélection, clavier (Ctrl+Tab, flèches) et accessibilité de WPF.</remarks>
    Public Class Tabs
        Inherits TabControl

        ''' <summary>Le dessin des onglets ; leurs <see cref="Tab"/> en héritent.</summary>
        Public Shared ReadOnly VariantProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Variant", GetType(TabsVariant), GetType(Tabs),
            New FrameworkPropertyMetadata(TabsVariant.Group, FrameworkPropertyMetadataOptions.Inherits))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Tabs), New FrameworkPropertyMetadata(GetType(Tabs)))
        End Sub

        ''' <summary>Le dessin des onglets.</summary>
        Public Property [Variant] As TabsVariant
            Get
                Return DirectCast(GetValue(VariantProperty), TabsVariant)
            End Get
            Set(value As TabsVariant)
                SetValue(VariantProperty, value)
            End Set
        End Property

        ''' <summary>Le dessin des onglets dont <paramref name="element"/> fait partie.</summary>
        Public Shared Function GetVariant(element As DependencyObject) As TabsVariant
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return DirectCast(element.GetValue(VariantProperty), TabsVariant)
        End Function

        ''' <summary>Fixe le dessin des onglets sous <paramref name="element"/>.</summary>
        Public Shared Sub SetVariant(element As DependencyObject, value As TabsVariant)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(VariantProperty, value)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is Tab
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New Tab()
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

        ' Le premier et le dernier onglet visibles ont leur filet de côté et leurs coins arrondis.
        Friend Sub MarquerLesBouts()
            Dim onglets = Enumerable.Range(0, Items.Count).
                Select(Function(i) TryCast(ItemContainerGenerator.ContainerFromIndex(i), Tab)).
                Where(Function(t) t IsNot Nothing AndAlso t.Visibility <> Visibility.Collapsed).
                ToList()
            For Each onglet In onglets
                onglet.Marquer(onglet Is onglets.First(), onglet Is onglets.Last())
            Next
        End Sub

    End Class

    ''' <summary>
    ''' Un onglet de <see cref="Tabs"/> : son titre (<c>Header</c>), une icône s'il le faut, et son contenu.
    ''' <code>&lt;mkm:Tab Header="Notes" Icon="{x:Static mkm:Icons.Note}"&gt; … &lt;/mkm:Tab&gt;</code>
    ''' </summary>
    Public Class Tab
        Inherits TabItem

        ''' <summary>L'icône de l'onglet, devant son titre.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(Tab), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le dessin des onglets, hérité de <see cref="Tabs"/>.</summary>
        Public Shared ReadOnly VariantProperty As DependencyProperty = Tabs.VariantProperty.AddOwner(GetType(Tab))

        Private Shared ReadOnly IsFirstPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFirst), GetType(Boolean), GetType(Tab), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le premier onglet visible.</summary>
        Public Shared ReadOnly IsFirstProperty As DependencyProperty = IsFirstPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLastPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLast), GetType(Boolean), GetType(Tab), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le dernier onglet visible.</summary>
        Public Shared ReadOnly IsLastProperty As DependencyProperty = IsLastPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Tab), New FrameworkPropertyMetadata(GetType(Tab)))
            VisibilityProperty.OverrideMetadata(GetType(Tab), New FrameworkPropertyMetadata(
                Visibility.Visible, AddressOf VisibiliteChangee))
        End Sub

        ''' <summary>L'icône de l'onglet, devant son titre.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Le dessin des onglets, hérité de <see cref="Tabs"/>.</summary>
        Public ReadOnly Property [Variant] As TabsVariant
            Get
                Return DirectCast(GetValue(VariantProperty), TabsVariant)
            End Get
        End Property

        ''' <summary>Vrai pour le premier onglet visible.</summary>
        Public ReadOnly Property IsFirst As Boolean
            Get
                Return CBool(GetValue(IsFirstProperty))
            End Get
        End Property

        ''' <summary>Vrai pour le dernier onglet visible.</summary>
        Public ReadOnly Property IsLast As Boolean
            Get
                Return CBool(GetValue(IsLastProperty))
            End Get
        End Property

        Friend Sub Marquer(premier As Boolean, dernier As Boolean)
            SetValue(IsFirstPropertyKey, premier)
            SetValue(IsLastPropertyKey, dernier)
        End Sub

        ' Un onglet qu'on cache ou qu'on montre déplace les bouts du groupe.
        Private Shared Sub VisibiliteChangee(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            TryCast(ItemsControl.ItemsControlFromItemContainer(d), Tabs)?.MarquerLesBouts()
        End Sub

    End Class

End Namespace
