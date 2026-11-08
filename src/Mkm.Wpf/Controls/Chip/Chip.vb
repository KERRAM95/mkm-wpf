Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La puce de mkm (Chip) : un mot, une personne ou un filtre dans une pastille arrondie grise, avec une icône ou une
    ''' photo ; retirable, une croix l'enlève.
    ''' <code>
    ''' &lt;mkm:Chip Content="Maturité : En cours" Removable="True" Remove="Filtre_Remove" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>Retirée, la puce se cache et lève <see cref="Remove"/>, comme mkm : l'application l'ôte de ses données.</remarks>
    <TemplatePart(Name:="PART_Remove", Type:=GetType(ButtonBase))>
    Public Class Chip
        Inherits ContentControl

        ''' <summary>Son icône (une de <see cref="Icons"/>), devant le texte.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(Chip), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Sa photo, ronde, devant le texte.</summary>
        Public Shared ReadOnly ImageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Image), GetType(ImageSource), GetType(Chip), New FrameworkPropertyMetadata(Nothing, AddressOf OnImageChanged))

        Private Shared ReadOnly HasImagePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasImage), GetType(Boolean), GetType(Chip), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand la puce a une photo.</summary>
        Public Shared ReadOnly HasImageProperty As DependencyProperty = HasImagePropertyKey.DependencyProperty

        ''' <summary>Vrai pour une puce retirable : une croix l'enlève.</summary>
        Public Shared ReadOnly RemovableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Removable), GetType(Boolean), GetType(Chip), New FrameworkPropertyMetadata(False))

        ''' <summary>Levé quand la croix retire la puce, qui se cache.</summary>
        Public Shared ReadOnly RemoveEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Remove), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(Chip))

        Private _croix As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Chip), New FrameworkPropertyMetadata(GetType(Chip)))
            FocusableProperty.OverrideMetadata(GetType(Chip), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Son icône, devant le texte.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Sa photo, ronde, devant le texte.</summary>
        Public Property Image As ImageSource
            Get
                Return DirectCast(GetValue(ImageProperty), ImageSource)
            End Get
            Set(value As ImageSource)
                SetValue(ImageProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la puce a une photo.</summary>
        Public ReadOnly Property HasImage As Boolean
            Get
                Return CBool(GetValue(HasImageProperty))
            End Get
        End Property

        Private Shared Sub OnImageChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            d.SetValue(HasImagePropertyKey, e.NewValue IsNot Nothing)
        End Sub

        ''' <summary>Vrai pour une puce retirable.</summary>
        Public Property Removable As Boolean
            Get
                Return CBool(GetValue(RemovableProperty))
            End Get
            Set(value As Boolean)
                SetValue(RemovableProperty, value)
            End Set
        End Property

        ''' <summary>Levé quand la croix retire la puce.</summary>
        Public Custom Event Remove As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                [AddHandler](RemoveEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                [RemoveHandler](RemoveEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _croix IsNot Nothing Then
                RemoveHandler _croix.Click, AddressOf Croix_Click
                RemoveHandler _croix.KeyDown, AddressOf Croix_KeyDown
            End If
            MyBase.OnApplyTemplate()
            _croix = TryCast(GetTemplateChild("PART_Remove"), ButtonBase)
            If _croix IsNot Nothing Then
                AddHandler _croix.Click, AddressOf Croix_Click
                AddHandler _croix.KeyDown, AddressOf Croix_KeyDown
            End If
        End Sub

        ''' <summary>Retire la puce : elle se cache et lève <see cref="Remove"/>.</summary>
        Public Sub Close()
            SetCurrentValue(VisibilityProperty, Visibility.Collapsed)
            Me.RaiseEvent(New RoutedEventArgs(RemoveEvent, Me))
        End Sub

        Private Sub Croix_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            Close()
        End Sub

        ' Comme mkm : Retour arrière retire aussi la puce (Entrée et Espace, le bouton les traite déjà).
        Private Sub Croix_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Back Then
                e.Handled = True
                Close()
            End If
        End Sub

        ''' <summary>Un groupe nommé par son texte pour les lecteurs d'écran ; sa croix dedans, « Retirer ».</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group, Function() TryCast(Content, String))
        End Function

    End Class

End Namespace
