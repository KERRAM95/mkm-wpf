Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La note de mkm (<c>Rating</c>) : une rangée d'étoiles, pleines jusqu'à la note ; un clic sur une étoile donne sa
    ''' note, un clic sur la note la retire.
    ''' <code>
    ''' &lt;mkm:Rating Value="3" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Pour les lecteurs d'écran, un curseur de 0 à <see cref="Stars"/> ; au clavier, les flèches changent la note.
    ''' </remarks>
    <TemplatePart(Name:="PART_Stars", Type:=GetType(System.Windows.Controls.Panel))>
    Public Class Rating
        Inherits Control

        ''' <summary>La note : de 0 (aucune) à <see cref="Stars"/>. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(Integer), GetType(Rating),
            New FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnValueChanged, AddressOf BornerLaNote))

        ''' <summary>Le nombre d'étoiles : 5 par défaut.</summary>
        Public Shared ReadOnly StarsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Stars), GetType(Integer), GetType(Rating), New FrameworkPropertyMetadata(5, AddressOf OnStarsChanged), AddressOf EstUnNombreDEtoiles)

        ''' <summary>Vrai pour une note qu'on lit sans la changer.</summary>
        Public Shared ReadOnly IsReadOnlyProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsReadOnly), GetType(Boolean), GetType(Rating), New FrameworkPropertyMetadata(False, AddressOf OnApparenceChanged))

        ''' <summary>L'icône d'une étoile pleine (une géométrie) : l'étoile pleine par défaut.</summary>
        Public Shared ReadOnly OnIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(OnIcon), GetType(Object), GetType(Rating), New FrameworkPropertyMetadata(Nothing, AddressOf OnApparenceChanged))

        ''' <summary>L'icône d'une étoile vide (une géométrie) : l'étoile tracée par défaut.</summary>
        Public Shared ReadOnly OffIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(OffIcon), GetType(Object), GetType(Rating), New FrameworkPropertyMetadata(Nothing, AddressOf OnApparenceChanged))

        ''' <summary>La note a changé.</summary>
        Public Shared ReadOnly ValueChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(ValueChanged), RoutingStrategy.Bubble, GetType(RoutedPropertyChangedEventHandler(Of Integer)), GetType(Rating))

        Private ReadOnly _etoiles As New List(Of EtoileDeRating)
        Private _rangee As System.Windows.Controls.Panel

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Rating), New FrameworkPropertyMetadata(GetType(Rating)))
            FocusableProperty.OverrideMetadata(GetType(Rating), New FrameworkPropertyMetadata(True))
        End Sub

        ''' <summary>La note.</summary>
        Public Property Value As Integer
            Get
                Return CInt(GetValue(ValueProperty))
            End Get
            Set(value As Integer)
                SetValue(ValueProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre d'étoiles.</summary>
        Public Property Stars As Integer
            Get
                Return CInt(GetValue(StarsProperty))
            End Get
            Set(value As Integer)
                SetValue(StarsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour une note qu'on lit sans la changer.</summary>
        Public Property IsReadOnly As Boolean
            Get
                Return CBool(GetValue(IsReadOnlyProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsReadOnlyProperty, value)
            End Set
        End Property

        ''' <summary>L'icône d'une étoile pleine.</summary>
        Public Property OnIcon As Object
            Get
                Return GetValue(OnIconProperty)
            End Get
            Set(value As Object)
                SetValue(OnIconProperty, value)
            End Set
        End Property

        ''' <summary>L'icône d'une étoile vide.</summary>
        Public Property OffIcon As Object
            Get
                Return GetValue(OffIconProperty)
            End Get
            Set(value As Object)
                SetValue(OffIconProperty, value)
            End Set
        End Property

        ''' <summary>La note a changé.</summary>
        Public Custom Event ValueChanged As RoutedPropertyChangedEventHandler(Of Integer)
            AddHandler(value As RoutedPropertyChangedEventHandler(Of Integer))
                [AddHandler](ValueChangedEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedPropertyChangedEventHandler(Of Integer))
                [RemoveHandler](ValueChangedEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedPropertyChangedEventArgs(Of Integer))
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Les étoiles montrées, pour les tests.</summary>
        Friend ReadOnly Property Etoiles As IReadOnlyList(Of EtoileDeRating)
            Get
                Return _etoiles
            End Get
        End Property

        Private Shared Function EstUnNombreDEtoiles(valeur As Object) As Boolean
            Return CInt(valeur) >= 1
        End Function

        Private Shared Function BornerLaNote(d As DependencyObject, valeur As Object) As Object
            Return Math.Max(0, Math.Min(DirectCast(d, Rating).Stars, CInt(valeur)))
        End Function

        Private Shared Sub OnValueChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim note = DirectCast(d, Rating)
            note.MettreAJour()
            TryCast(UIElementAutomationPeer.FromElement(note), PairDeRating)?.AnnoncerLaNote(CInt(e.OldValue), CInt(e.NewValue))
            note.RaiseEvent(New RoutedPropertyChangedEventArgs(Of Integer)(CInt(e.OldValue), CInt(e.NewValue), ValueChangedEvent))
        End Sub

        Private Shared Sub OnStarsChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim note = DirectCast(d, Rating)
            note.CoerceValue(ValueProperty)
            note.Construire()
        End Sub

        Private Shared Sub OnApparenceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Rating).MettreAJour()
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            _rangee?.Children.Clear()
            MyBase.OnApplyTemplate()
            _rangee = TryCast(GetTemplateChild("PART_Stars"), System.Windows.Controls.Panel)
            Construire()
        End Sub

        ''' <summary>L'anneau du clavier suit le focus vu au clavier.</summary>
        Protected Overrides Sub OnPropertyChanged(e As DependencyPropertyChangedEventArgs)
            MyBase.OnPropertyChanged(e)
            If e.Property Is FocusVisible.IsVisibleProperty OrElse e.Property Is IsEnabledProperty Then MettreAJour()
        End Sub

        ' Une étoile par point de note, dans la rangée du gabarit.
        Private Sub Construire()
            _etoiles.Clear()
            _rangee?.Children.Clear()
            For numero = 1 To Stars
                Dim etoile As New EtoileDeRating(numero)
                AddHandler etoile.MouseLeftButtonUp, AddressOf Etoile_MouseLeftButtonUp
                _etoiles.Add(etoile)
                _rangee?.Children.Add(etoile)
            Next
            MettreAJour()
        End Sub

        ' Pleine jusqu'à la note ; l'anneau du clavier sur l'étoile de la note (la première, sans note).
        Private Sub MettreAJour()
            Dim actif = IsEnabled AndAlso Not IsReadOnly
            Dim anneau = FocusVisible.GetIsVisible(Me)
            For Each etoile In _etoiles
                Dim pleine = etoile.Numero <= Value
                etoile.IsActive = pleine
                etoile.IsInteractive = actif
                etoile.IsRingVisible = anneau AndAlso etoile.Numero = Math.Max(1, Value)
                etoile.Icon = If(pleine, If(OnIcon, InternalIcons.Star), If(OffIcon, InternalIcons.Star))
                etoile.Filled = pleine AndAlso OnIcon Is Nothing
            Next
        End Sub

        ' Un clic donne la note de l'étoile ; sur la note, il la retire, comme mkm.
        Private Sub Etoile_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
            If IsReadOnly OrElse Not IsEnabled Then Return
            Dim numero = DirectCast(sender, EtoileDeRating).Numero
            SetCurrentValue(ValueProperty, If(numero = Value, 0, numero))
            Focus()
            e.Handled = True
        End Sub

        ''' <summary>→ et ↑ montent la note, ← et ↓ la baissent, Début et Fin la mettent à 1 et au plus haut, Suppr la retire.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse IsReadOnly Then Return
            Dim note = Value
            Select Case e.Key
                Case Key.Right, Key.Up
                    note = Math.Min(Stars, Value + 1)
                Case Key.Left, Key.Down
                    note = Math.Max(1, Value - 1)
                Case Key.Home
                    note = 1
                Case Key.End
                    note = Stars
                Case Key.Delete, Key.Back
                    note = 0
                Case Else
                    Return
            End Select
            SetCurrentValue(ValueProperty, note)
            e.Handled = True
        End Sub

        ''' <summary>Un curseur pour les lecteurs d'écran : sa valeur, la note, de 0 au nombre d'étoiles.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeRating(Me)
        End Function

        Private NotInheritable Class PairDeRating
            Inherits FrameworkElementAutomationPeer
            Implements IRangeValueProvider

            Public Sub New(note As Rating)
                MyBase.New(note)
            End Sub

            Private ReadOnly Property Note As Rating
                Get
                    Return DirectCast(Owner, Rating)
                End Get
            End Property

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.Slider
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return NameOf(Rating)
            End Function

            Protected Overrides Function IsKeyboardFocusableCore() As Boolean
                Return Note.Focusable AndAlso Note.IsEnabled
            End Function

            ' Les étoiles ne sont que son dessin.
            Protected Overrides Function GetChildrenCore() As List(Of AutomationPeer)
                Return Nothing
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.RangeValue Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public ReadOnly Property Value As Double Implements IRangeValueProvider.Value
                Get
                    Return Note.Value
                End Get
            End Property

            Public ReadOnly Property IsReadOnly As Boolean Implements IRangeValueProvider.IsReadOnly
                Get
                    Return Note.IsReadOnly OrElse Not Note.IsEnabled
                End Get
            End Property

            Public ReadOnly Property Maximum As Double Implements IRangeValueProvider.Maximum
                Get
                    Return Note.Stars
                End Get
            End Property

            Public ReadOnly Property Minimum As Double Implements IRangeValueProvider.Minimum
                Get
                    Return 0
                End Get
            End Property

            Public ReadOnly Property LargeChange As Double Implements IRangeValueProvider.LargeChange
                Get
                    Return 1
                End Get
            End Property

            Public ReadOnly Property SmallChange As Double Implements IRangeValueProvider.SmallChange
                Get
                    Return 1
                End Get
            End Property

            Public Sub SetValue(valeur As Double) Implements IRangeValueProvider.SetValue
                If IsReadOnly Then Throw New ElementNotEnabledException()
                If valeur < 0 OrElse valeur > Note.Stars Then Throw New ArgumentOutOfRangeException(NameOf(valeur))
                Note.SetCurrentValue(ValueProperty, CInt(Math.Round(valeur)))
            End Sub

            Friend Sub AnnoncerLaNote(avant As Integer, apres As Integer)
                If Not ListenerExists(AutomationEvents.PropertyChanged) Then Return
                RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, CDbl(avant), CDbl(apres))
            End Sub

        End Class

    End Class

    ''' <summary>Une étoile de <see cref="Rating"/> : son icône, pleine ou vide ; le survol l'éclaire.</summary>
    Friend NotInheritable Class EtoileDeRating
        Inherits Control

        Public Shared ReadOnly IsActiveProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsActive), GetType(Boolean), GetType(EtoileDeRating), New FrameworkPropertyMetadata(False))

        Public Shared ReadOnly IsInteractiveProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsInteractive), GetType(Boolean), GetType(EtoileDeRating), New FrameworkPropertyMetadata(True))

        Public Shared ReadOnly IsRingVisibleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsRingVisible), GetType(Boolean), GetType(EtoileDeRating), New FrameworkPropertyMetadata(False))

        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(EtoileDeRating), New FrameworkPropertyMetadata(Nothing))

        Public Shared ReadOnly FilledProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Filled), GetType(Boolean), GetType(EtoileDeRating), New FrameworkPropertyMetadata(False))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(EtoileDeRating), New FrameworkPropertyMetadata(GetType(EtoileDeRating)))
            FocusableProperty.OverrideMetadata(GetType(EtoileDeRating), New FrameworkPropertyMetadata(False))
        End Sub

        Friend Sub New(numero As Integer)
            Me.Numero = numero
        End Sub

        Friend ReadOnly Property Numero As Integer

        Public Property IsActive As Boolean
            Get
                Return CBool(GetValue(IsActiveProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsActiveProperty, value)
            End Set
        End Property

        Public Property IsInteractive As Boolean
            Get
                Return CBool(GetValue(IsInteractiveProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsInteractiveProperty, value)
            End Set
        End Property

        Public Property IsRingVisible As Boolean
            Get
                Return CBool(GetValue(IsRingVisibleProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsRingVisibleProperty, value)
            End Set
        End Property

        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        Public Property Filled As Boolean
            Get
                Return CBool(GetValue(FilledProperty))
            End Get
            Set(value As Boolean)
                SetValue(FilledProperty, value)
            End Set
        End Property

    End Class

End Namespace
