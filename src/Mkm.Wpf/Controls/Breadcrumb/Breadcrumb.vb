Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le fil d'Ariane de mkm (Breadcrumb) : le chemin jusqu'à la page actuelle, des liens séparés d'un chevron. Le
    ''' premier sert d'accueil : une icône seule, nommée par son info-bulle.
    ''' <code>
    ''' &lt;mkm:Breadcrumb&gt;
    '''     &lt;mkm:BreadcrumbItem Icon="{x:Static mkm:Icons.Home}" ToolTip="Accueil" /&gt;
    '''     &lt;mkm:BreadcrumbItem Content="Projets" /&gt;
    '''     &lt;mkm:BreadcrumbItem Content="HS-00042" /&gt;
    ''' &lt;/mkm:Breadcrumb&gt;
    ''' </code>
    ''' Lié à des données (<c>ItemsSource</c>), chaque élément devient un <see cref="BreadcrumbItem"/> qui le montre.
    ''' </summary>
    Public Class Breadcrumb
        Inherits ItemsControl

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Breadcrumb), New FrameworkPropertyMetadata(GetType(Breadcrumb)))
            FocusableProperty.OverrideMetadata(GetType(Breadcrumb), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c> : ses liens.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is BreadcrumbItem
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New BreadcrumbItem()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Marquer()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Marquer()
        End Sub

        ' Le premier n'a pas de chevron devant lui ; le dernier est la page actuelle.
        Private Sub Marquer()
            Dim liens = Items.Cast(Of Object)().
                Select(Function(o) TryCast(If(TryCast(o, BreadcrumbItem), ItemContainerGenerator.ContainerFromItem(o)), BreadcrumbItem)).
                Where(Function(l) l IsNot Nothing).ToList()
            For i = 0 To liens.Count - 1
                liens(i).Placer(i = 0, i = liens.Count - 1)
            Next
        End Sub

    End Class

    ''' <summary>
    ''' Un lien d'un <see cref="Breadcrumb"/> : une icône (<c>Icon</c>) et un libellé (<c>Content</c>), précédés d'un
    ''' chevron sauf le premier. Un bouton : <c>Click</c> et <c>Command</c> mènent à sa page.
    ''' </summary>
    Public Class BreadcrumbItem
        Inherits ButtonBase

        ''' <summary>L'icône, avant le libellé : une géométrie de <see cref="Icons"/>.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(BreadcrumbItem), New FrameworkPropertyMetadata(Nothing))

        Private Shared ReadOnly IsFirstPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFirst), GetType(Boolean), GetType(BreadcrumbItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le premier lien : sans chevron devant lui.</summary>
        Public Shared ReadOnly IsFirstProperty As DependencyProperty = IsFirstPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLastPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLast), GetType(Boolean), GetType(BreadcrumbItem), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour le dernier lien : la page actuelle.</summary>
        Public Shared ReadOnly IsLastProperty As DependencyProperty = IsLastPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(BreadcrumbItem), New FrameworkPropertyMetadata(GetType(BreadcrumbItem)))
        End Sub

        ''' <summary>L'icône.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour le premier lien.</summary>
        Public ReadOnly Property IsFirst As Boolean
            Get
                Return CBool(GetValue(IsFirstProperty))
            End Get
        End Property

        ''' <summary>Vrai pour le dernier lien, la page actuelle.</summary>
        Public ReadOnly Property IsLast As Boolean
            Get
                Return CBool(GetValue(IsLastProperty))
            End Get
        End Property

        ' Comme un clic : Click, puis Command.
        Friend Sub Activer()
            OnClick()
        End Sub

        Friend Sub Placer(premier As Boolean, dernier As Boolean)
            SetValue(IsFirstPropertyKey, premier)
            SetValue(IsLastPropertyKey, dernier)
        End Sub

        ''' <summary>Un lien pour les lecteurs d'écran, nommé par son libellé, sinon son info-bulle.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeLien(Me)
        End Function

        Private NotInheritable Class PairDeLien
            Inherits ButtonBaseAutomationPeer
            Implements System.Windows.Automation.Provider.IInvokeProvider

            Public Sub New(lien As BreadcrumbItem)
                MyBase.New(lien)
            End Sub

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.Hyperlink
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(BreadcrumbItem)
            End Function

            Protected Overrides Function GetNameCore() As String
                Dim nom = MyBase.GetNameCore()
                Return If(String.IsNullOrEmpty(nom), InfoBulle(Owner), nom)
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.Invoke Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public Sub Invoke() Implements System.Windows.Automation.Provider.IInvokeProvider.Invoke
                If Not IsEnabled() Then Throw New System.Windows.Automation.ElementNotEnabledException()
                Dim lien = DirectCast(Owner, BreadcrumbItem)
                lien.Dispatcher.BeginInvoke(New System.Action(AddressOf lien.Activer))
            End Sub

        End Class

    End Class

End Namespace
