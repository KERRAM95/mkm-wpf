Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>Le bord où se pose un <see cref="Dock"/>.</summary>
    Public Enum DockPosition
        ''' <summary>En bas, centré (par défaut).</summary>
        Bottom
        ''' <summary>En haut, centré.</summary>
        Top
        ''' <summary>À gauche, centré, en colonne.</summary>
        Left
        ''' <summary>À droite, centré, en colonne.</summary>
        Right
    End Enum

    ''' <summary>
    ''' Le dock de mkm (<c>Dock</c>) : une rangée d'applications sur un bord de son conteneur, dans un cadre translucide ;
    ''' chacune, une icône (ou une image) de 36 px, son nom en info-bulle.
    ''' <code>
    ''' &lt;mkm:Dock&gt;
    '''     &lt;mkm:DockItem Label="Documents" Icon="{x:Static mkm:Icons.Documents}" /&gt;
    ''' &lt;/mkm:Dock&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' « Dock » est aussi l'énumération de WPF (<c>DockPanel.Dock</c>) : dans un fichier VB qui importe
    ''' <c>Mkm.Wpf.Controls</c>, écrire <c>System.Windows.Controls.Dock</c> pour celle-là.
    ''' </remarks>
    Public Class Dock
        Inherits ItemsControl

        ''' <summary>Le bord où il se pose : en bas par défaut.</summary>
        Public Shared ReadOnly PositionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Position), GetType(DockPosition), GetType(Dock), New FrameworkPropertyMetadata(DockPosition.Bottom))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Dock), New FrameworkPropertyMetadata(GetType(Dock)))
            FocusableProperty.OverrideMetadata(GetType(Dock), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Le bord où il se pose.</summary>
        Public Property Position As DockPosition
            Get
                Return DirectCast(GetValue(PositionProperty), DockPosition)
            End Get
            Set(value As DockPosition)
                SetValue(PositionProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is DockItem
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New DockItem()
        End Function

        ''' <summary>Début et Fin vont à la première et à la dernière application, comme mkm (les flèches, déjà).</summary>
        Protected Overrides Sub OnPreviewKeyDown(e As System.Windows.Input.KeyEventArgs)
            MyBase.OnPreviewKeyDown(e)
            If e.Handled OrElse (e.Key <> System.Windows.Input.Key.Home AndAlso e.Key <> System.Windows.Input.Key.End) OrElse Items.Count = 0 Then Return
            Dim rang = If(e.Key = System.Windows.Input.Key.Home, 0, Items.Count - 1)
            Dim element = TryCast(If(TryCast(Items(rang), UIElement), ItemContainerGenerator.ContainerFromIndex(rang)), UIElement)
            If element IsNot Nothing AndAlso element.Focus() Then e.Handled = True
        End Sub

        ''' <summary>Un menu pour les lecteurs d'écran, comme mkm : ses applications.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Menu)
        End Function

    End Class

    ''' <summary>Une application d'un <see cref="Dock"/> : son icône, son nom (info-bulle), son clic ou sa commande.</summary>
    Public Class DockItem
        Inherits ButtonBase

        ''' <summary>L'icône : une géométrie (une icône de mkm) ou une image.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(DockItem), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le nom de l'application : son info-bulle et son nom pour les lecteurs d'écran.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Label), GetType(String), GetType(DockItem), New FrameworkPropertyMetadata(Nothing, AddressOf OnLabelChanged))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(DockItem), New FrameworkPropertyMetadata(GetType(DockItem)))
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

        ''' <summary>Le nom de l'application.</summary>
        Public Property Label As String
            Get
                Return CStr(GetValue(LabelProperty))
            End Get
            Set(value As String)
                SetValue(LabelProperty, value)
            End Set
        End Property

        ' Le nom en info-bulle, sauf une info-bulle donnée.
        Private Shared Sub OnLabelChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim element = DirectCast(d, DockItem)
            If element.ReadLocalValue(ToolTipProperty) Is DependencyProperty.UnsetValue OrElse TypeOf element.ToolTip Is String Then
                element.ToolTip = If(String.IsNullOrEmpty(CStr(e.NewValue)), Nothing, e.NewValue)
            End If
        End Sub

        ''' <summary>Un élément de menu pour les lecteurs d'écran, nommé par son nom.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeDockItem(Me)
        End Function

        Private NotInheritable Class PairDeDockItem
            Inherits ButtonBaseAutomationPeer
            Implements IInvokeProvider

            Public Sub New(element As DockItem)
                MyBase.New(element)
            End Sub

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.MenuItem
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(DockItem)
            End Function

            Protected Overrides Function GetNameCore() As String
                Dim nom = MyBase.GetNameCore()
                Return If(String.IsNullOrEmpty(nom), If(DirectCast(Owner, DockItem).Label, String.Empty), nom)
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.Invoke Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public Sub Invoke() Implements IInvokeProvider.Invoke
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                Dim element = DirectCast(Owner, DockItem)
                Dispatcher.BeginInvoke(Threading.DispatcherPriority.Input, New System.Action(
                    Sub() element.RaiseEvent(New RoutedEventArgs(ClickEvent, element))))
            End Sub

        End Class

    End Class

End Namespace
