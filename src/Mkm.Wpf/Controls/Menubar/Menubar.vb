Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La barre de menus de mkm (<c>Menubar</c>) : ses éléments en ligne, chacun ouvrant son sous-menu (le menu de mkm,
    ''' sous-menus en cascade compris) ; un début et une fin, à gauche et à droite.
    ''' <code>
    ''' &lt;mkm:Menubar&gt;
    '''     &lt;MenuItem Header="Fichier"&gt;
    '''         &lt;MenuItem Header="Ouvrir" /&gt;
    '''     &lt;/MenuItem&gt;
    ''' &lt;/mkm:Menubar&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' C'est le <see cref="System.Windows.Controls.Menu"/> de WPF : Alt, F10, les flèches, les touches d'accès (« _Fichier »),
    ''' les commandes et leurs raccourcis ; ses éléments sont les <see cref="MenuItem"/> de WPF.
    ''' </remarks>
    Public Class Menubar
        Inherits System.Windows.Controls.Menu

        ''' <summary>Ce qui se pose avant les éléments (un logo).</summary>
        Public Shared ReadOnly StartProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Start), GetType(Object), GetType(Menubar), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Ce qui se pose après les éléments, au bout de la barre (une recherche, un avatar).</summary>
        Public Shared ReadOnly EndProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([End]), GetType(Object), GetType(Menubar), New FrameworkPropertyMetadata(Nothing))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Menubar), New FrameworkPropertyMetadata(GetType(Menubar)))
            ' Un seul arrêt de la touche Tab pour toute la barre, comme mkm ; les flèches vont d'un élément à l'autre.
            System.Windows.Input.KeyboardNavigation.TabNavigationProperty.OverrideMetadata(GetType(Menubar),
                New FrameworkPropertyMetadata(System.Windows.Input.KeyboardNavigationMode.Once))
        End Sub

        Public Sub New()
            ' Chaque sous-menu garde la colonne d'icônes s'il en a.
            AddHandler Loaded, Sub(s, e) PoserLesColonnes()
        End Sub

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

        Private Sub PoserLesColonnes()
            For Each element In Items.OfType(Of MenuItem)()
                Colonne.Poser(element)
            Next
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            PoserLesColonnes()
        End Sub

    End Class

End Namespace
