Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Les étapes de mkm (Stepper) : une suite d'étapes numérotées reliées par un trait, l'étape en cours en couleur et
    ''' son contenu dessous ; en colonne (<c>Orientation="Vertical"</c>), le contenu sous son étape. C'est un TabControl :
    ''' <c>SelectedIndex</c> est l'étape en cours, <c>Next()</c> et <c>Previous()</c> y passent. <c>Linear</c> : on n'y
    ''' saute pas d'un clic, on avance par les boutons de l'application.
    ''' <code>
    ''' &lt;mkm:Stepper&gt;
    '''     &lt;mkm:StepperPanel Header="Plan"&gt;…&lt;/mkm:StepperPanel&gt;
    '''     &lt;mkm:StepperPanel Header="Cartouche"&gt;…&lt;/mkm:StepperPanel&gt;
    ''' &lt;/mkm:Stepper&gt;
    ''' </code>
    ''' </summary>
    Public Class Stepper
        Inherits TabControl

        ''' <summary>Vrai quand on n'avance que par <c>Next()</c> et <c>Previous()</c> (les boutons de l'application).</summary>
        Public Shared ReadOnly LinearProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Linear), GetType(Boolean), GetType(Stepper), New FrameworkPropertyMetadata(False))

        ''' <summary>Les étapes en ligne, le contenu dessous (par défaut) ; ou en colonne, chaque contenu sous son étape.</summary>
        Public Shared ReadOnly OrientationProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Orientation), GetType(Orientation), GetType(Stepper), New FrameworkPropertyMetadata(Orientation.Horizontal, AddressOf OnOrientationChanged))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Stepper), New FrameworkPropertyMetadata(GetType(Stepper)))
        End Sub

        ''' <summary>Vrai quand on n'avance que par les boutons de l'application.</summary>
        Public Property Linear As Boolean
            Get
                Return CBool(GetValue(LinearProperty))
            End Get
            Set(value As Boolean)
                SetValue(LinearProperty, value)
            End Set
        End Property

        ''' <summary>Les étapes en ligne ou en colonne.</summary>
        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        ''' <summary>Passe à l'étape suivante (rien après la dernière).</summary>
        Public Sub [Next]()
            If SelectedIndex < Items.Count - 1 Then SelectedIndex += 1
        End Sub

        ''' <summary>Revient à l'étape précédente (rien avant la première).</summary>
        Public Sub Previous()
            If SelectedIndex > 0 Then SelectedIndex -= 1
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is StepperPanel
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New StepperPanel()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Numeroter()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Numeroter()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnSelectionChanged(e As SelectionChangedEventArgs)
            MyBase.OnSelectionChanged(e)
            Numeroter()
        End Sub

        Private Shared Sub OnOrientationChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Stepper).Numeroter()
        End Sub

        ' Le numéro de chaque étape, la dernière (sans trait), celles passées (leur trait en couleur) et le sens.
        Private Sub Numeroter()
            Dim etapes = Items.Cast(Of Object)().
                Select(Function(o) TryCast(If(TryCast(o, StepperPanel), ItemContainerGenerator.ContainerFromItem(o)), StepperPanel)).ToList()
            For i = 0 To etapes.Count - 1
                etapes(i)?.Placer(i + 1, i = etapes.Count - 1, i < SelectedIndex, Orientation)
            Next
        End Sub

        ''' <summary>Linéaire, Ctrl+Tab et Ctrl+Maj+Tab ne sautent pas d'étape.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            If Linear AndAlso e.Key = Key.Tab AndAlso (Keyboard.Modifiers And ModifierKeys.Control) <> 0 Then Return
            MyBase.OnKeyDown(e)
        End Sub

    End Class

    ''' <summary>
    ''' Une étape d'un <see cref="Stepper"/> : son titre (<c>Header</c>) à côté de son numéro, son contenu (<c>Content</c>)
    ''' montré quand elle est l'étape en cours.
    ''' </summary>
    Public Class StepperPanel
        Inherits TabItem

        Private Shared ReadOnly NumberPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Number), GetType(Integer), GetType(StepperPanel), New FrameworkPropertyMetadata(1))
        ''' <summary>Son numéro, à partir de 1.</summary>
        Public Shared ReadOnly NumberProperty As DependencyProperty = NumberPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLastPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLast), GetType(Boolean), GetType(StepperPanel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour la dernière étape : sans trait après elle.</summary>
        Public Shared ReadOnly IsLastProperty As DependencyProperty = IsLastPropertyKey.DependencyProperty

        Private Shared ReadOnly IsCompletedPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsCompleted), GetType(Boolean), GetType(StepperPanel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour une étape passée : son trait en couleur.</summary>
        Public Shared ReadOnly IsCompletedProperty As DependencyProperty = IsCompletedPropertyKey.DependencyProperty

        Private Shared ReadOnly OrientationPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Orientation), GetType(Orientation), GetType(StepperPanel), New FrameworkPropertyMetadata(Orientation.Horizontal))
        ''' <summary>Le sens de son Stepper.</summary>
        Public Shared ReadOnly OrientationProperty As DependencyProperty = OrientationPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(StepperPanel), New FrameworkPropertyMetadata(GetType(StepperPanel)))
        End Sub

        ''' <summary>Son numéro.</summary>
        Public ReadOnly Property Number As Integer
            Get
                Return CInt(GetValue(NumberProperty))
            End Get
        End Property

        ''' <summary>Vrai pour la dernière étape.</summary>
        Public ReadOnly Property IsLast As Boolean
            Get
                Return CBool(GetValue(IsLastProperty))
            End Get
        End Property

        ''' <summary>Vrai pour une étape passée.</summary>
        Public ReadOnly Property IsCompleted As Boolean
            Get
                Return CBool(GetValue(IsCompletedProperty))
            End Get
        End Property

        ''' <summary>Le sens de son Stepper.</summary>
        Public ReadOnly Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
        End Property

        Friend Sub Placer(numero As Integer, derniere As Boolean, passee As Boolean, sens As Orientation)
            SetValue(NumberPropertyKey, numero)
            SetValue(IsLastPropertyKey, derniere)
            SetValue(IsCompletedPropertyKey, passee)
            SetValue(OrientationPropertyKey, sens)
        End Sub

        Private ReadOnly Property Etapes As Stepper
            Get
                Return TryCast(ItemsControl.ItemsControlFromItemContainer(Me), Stepper)
            End Get
        End Property

        ''' <summary>Linéaire, un clic sur l'étape ne la choisit pas.</summary>
        Protected Overrides Sub OnMouseLeftButtonDown(e As MouseButtonEventArgs)
            If Etapes?.Linear Then
                e.Handled = True
                Return
            End If
            MyBase.OnMouseLeftButtonDown(e)
        End Sub

        ''' <summary>Linéaire, le clavier qui arrive sur une étape ne la choisit pas.</summary>
        Protected Overrides Sub OnPreviewGotKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            If Etapes?.Linear AndAlso e.NewFocus Is Me AndAlso Not IsSelected Then
                e.Handled = True
                Return
            End If
            MyBase.OnPreviewGotKeyboardFocus(e)
        End Sub

    End Class

End Namespace
