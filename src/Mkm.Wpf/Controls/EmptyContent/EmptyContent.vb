Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>La taille d'un <see cref="EmptyContent"/>, comme dans mkm.</summary>
    Public Enum EmptyContentSize
        ''' <summary>Une icône de 32 px, un titre de 16 px, un message de 14 px (par défaut).</summary>
        Normal
        ''' <summary>Une icône de 24 px, un titre de 14 px, un message de 12 px : pour un panneau étroit.</summary>
        Small
    End Enum

    ''' <summary>
    ''' L'écran vide de mkm (<c>EmptyContent</c>) : dans un panneau, une grille ou une liste sans rien à montrer, un bloc
    ''' centré de 420 px au plus — une icône grise, un titre, un message gris — puis, l'une sous l'autre, les possibilités
    ''' de le remplir (<see cref="EmptyContentItem"/>), chacune son icône dans une pastille ronde de la couleur principale
    ''' pâlie, séparées d'un « ou » entre deux traits courts ; les actions (<see cref="Actions"/>) en dessous.
    ''' <code>
    ''' &lt;mkm:EmptyContent Icon="{x:Static mkm:Icons.Documents}" Title="Aucun plan" Message="Ouvrez un plan dans CATIA."&gt;
    '''     &lt;mkm:EmptyContentItem Icon="{x:Static mkm:Icons.Open}" Content="Ouvrez un plan existant" /&gt;
    '''     &lt;mkm:EmptyContentItem Icon="{x:Static mkm:Icons.Add}" Content="Créez-en un" /&gt;
    '''     &lt;mkm:EmptyContent.Actions&gt;&lt;mkm:Button Content="Ouvrir" /&gt;&lt;/mkm:EmptyContent.Actions&gt;
    ''' &lt;/mkm:EmptyContent&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Ses éléments sont les possibilités (<c>Items</c>, ou <c>ItemsSource</c> : un texte y devient le message d'une
    ''' possibilité sans icône). Le message et les possibilités sont annoncés aux lecteurs d'écran ; les actions non.
    ''' </remarks>
    Public Class EmptyContent
        Inherits ItemsControl

        ''' <summary>Le titre, en gras.</summary>
        Public Shared ReadOnly TitleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Title), GetType(String), GetType(EmptyContent), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le message, sous le titre : un texte ou un élément.</summary>
        Public Shared ReadOnly MessageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Message), GetType(Object), GetType(EmptyContent), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'icône, au-dessus du titre (une icône de <see cref="Icons"/>).</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(EmptyContent), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Les actions, sous le bloc (des boutons).</summary>
        Public Shared ReadOnly ActionsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Actions), GetType(Object), GetType(EmptyContent), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La taille, héritée par les possibilités.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(EmptyContentSize), GetType(EmptyContent),
            New FrameworkPropertyMetadata(EmptyContentSize.Normal, FrameworkPropertyMetadataOptions.Inherits))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(EmptyContent), New FrameworkPropertyMetadata(GetType(EmptyContent)))
            FocusableProperty.OverrideMetadata(GetType(EmptyContent), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Le titre, en gras.</summary>
        Public Property Title As String
            Get
                Return CStr(GetValue(TitleProperty))
            End Get
            Set(value As String)
                SetValue(TitleProperty, value)
            End Set
        End Property

        ''' <summary>Le message, sous le titre.</summary>
        Public Property Message As Object
            Get
                Return GetValue(MessageProperty)
            End Get
            Set(value As Object)
                SetValue(MessageProperty, value)
            End Set
        End Property

        ''' <summary>L'icône, au-dessus du titre.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Les actions, sous le bloc.</summary>
        Public Property Actions As Object
            Get
                Return GetValue(ActionsProperty)
            End Get
            Set(value As Object)
                SetValue(ActionsProperty, value)
            End Set
        End Property

        ''' <summary>La taille, héritée par les possibilités.</summary>
        Public Property Size As EmptyContentSize
            Get
                Return DirectCast(GetValue(SizeProperty), EmptyContentSize)
            End Get
            Set(value As EmptyContentSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is EmptyContentItem
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New EmptyContentItem()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            MarquerLaDerniere()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            MarquerLaDerniere()
        End Sub

        ' Un « ou » sépare deux possibilités : toutes en ont un, sauf la dernière.
        Private Sub MarquerLaDerniere()
            Dim possibilites = Enumerable.Range(0, Items.Count).
                Select(Function(i) TryCast(ItemContainerGenerator.ContainerFromIndex(i), EmptyContentItem)).
                Where(Function(p) p IsNot Nothing AndAlso p.Visibility <> Visibility.Collapsed).
                ToList()
            For Each possibilite In possibilites
                possibilite.Marquer(possibilite Is possibilites.Last())
            Next
        End Sub

    End Class

    ''' <summary>
    ''' Une possibilité d'un <see cref="EmptyContent"/> : son icône (<see cref="Icon"/>) dans une pastille ronde, puis son
    ''' message (le contenu) ; un « ou » la sépare de la suivante.
    ''' </summary>
    Public Class EmptyContentItem
        Inherits ContentControl

        ''' <summary>L'icône, dans la pastille.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(EmptyContentItem), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La taille, celle de l'écran vide.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = EmptyContent.SizeProperty.AddOwner(
            GetType(EmptyContentItem), New FrameworkPropertyMetadata(EmptyContentSize.Normal, FrameworkPropertyMetadataOptions.Inherits))

        Private Shared ReadOnly IsLastPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLast), GetType(Boolean), GetType(EmptyContentItem), New FrameworkPropertyMetadata(True))
        ''' <summary>Vrai pour la dernière possibilité : pas de « ou » après elle.</summary>
        Public Shared ReadOnly IsLastProperty As DependencyProperty = IsLastPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(EmptyContentItem), New FrameworkPropertyMetadata(GetType(EmptyContentItem)))
            FocusableProperty.OverrideMetadata(GetType(EmptyContentItem), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>L'icône, dans la pastille.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>La taille, celle de l'écran vide.</summary>
        Public Property Size As EmptyContentSize
            Get
                Return DirectCast(GetValue(SizeProperty), EmptyContentSize)
            End Get
            Set(value As EmptyContentSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour la dernière possibilité.</summary>
        Public ReadOnly Property IsLast As Boolean
            Get
                Return CBool(GetValue(IsLastProperty))
            End Get
        End Property

        Friend Sub Marquer(derniere As Boolean)
            SetValue(IsLastPropertyKey, derniere)
        End Sub

    End Class

End Namespace
