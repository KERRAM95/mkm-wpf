Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>Les données d'un ajout ou d'un retrait d'<see cref="InputChips"/> : la valeur.</summary>
    Public Class InputChipsEventArgs
        Inherits RoutedEventArgs

        Public Sub New(evenement As RoutedEvent, source As Object, valeur As String)
            MyBase.New(evenement, source)
            Value = valeur
        End Sub

        ''' <summary>La valeur ajoutée ou retirée.</summary>
        Public ReadOnly Property Value As String
    End Class

    ''' <summary>
    ''' Le champ à puces de mkm (<c>InputChips</c>) : chaque valeur saisie, validée par Entrée, devient une puce, que sa
    ''' croix retire.
    ''' <code>
    ''' &lt;mkm:InputChips Placeholder="Mots-clés" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Input", Type:=GetType(TextBox))>
    <TemplatePart(Name:="PART_Chips", Type:=GetType(System.Windows.Controls.Panel))>
    Public Class InputChips
        Inherits Control

        ''' <summary>Les valeurs, une puce chacune. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(ObservableCollection(Of String)), GetType(InputChips),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnValueChanged))

        ''' <summary>Le texte gris du champ vide.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Placeholder), GetType(String), GetType(InputChips), New FrameworkPropertyMetadata(Nothing, AddressOf OnNomChanged))

        ''' <summary>Vrai pour la bordure rouge d'une saisie refusée.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType(InputChips), New FrameworkPropertyMetadata(False))

        ''' <summary>Un séparateur (« , ») qui, tapé ou collé, valide aussi la valeur.</summary>
        Public Shared ReadOnly SeparatorProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Separator), GetType(String), GetType(InputChips), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le nombre de valeurs au plus ; 0 (par défaut) : sans limite.</summary>
        Public Shared ReadOnly MaxProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Max), GetType(Integer), GetType(InputChips), New FrameworkPropertyMetadata(0, AddressOf OnLimiteChanged))

        ''' <summary>Vrai (par défaut) pour accepter deux fois la même valeur.</summary>
        Public Shared ReadOnly AllowDuplicateProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AllowDuplicate), GetType(Boolean), GetType(InputChips), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai pour valider aussi la valeur en quittant le champ.</summary>
        Public Shared ReadOnly AddOnBlurProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AddOnBlur), GetType(Boolean), GetType(InputChips), New FrameworkPropertyMetadata(False))

        Private Shared ReadOnly IsMaxedOutPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsMaxedOut), GetType(Boolean), GetType(InputChips), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le nombre de valeurs atteint <see cref="Max"/> : la saisie s'arrête.</summary>
        Public Shared ReadOnly IsMaxedOutProperty As DependencyProperty = IsMaxedOutPropertyKey.DependencyProperty

        Private Shared ReadOnly HasValuesPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasValues), GetType(Boolean), GetType(InputChips), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand il y a au moins une valeur.</summary>
        Public Shared ReadOnly HasValuesProperty As DependencyProperty = HasValuesPropertyKey.DependencyProperty

        ''' <summary>Une valeur a été ajoutée.</summary>
        Public Shared ReadOnly AddEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Add), RoutingStrategy.Bubble, GetType(EventHandler(Of InputChipsEventArgs)), GetType(InputChips))

        ''' <summary>Une valeur a été retirée.</summary>
        Public Shared ReadOnly RemoveEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(Remove), RoutingStrategy.Bubble, GetType(EventHandler(Of InputChipsEventArgs)), GetType(InputChips))

        Private _saisie As TextBox
        Private _rangee As System.Windows.Controls.Panel
        Private ReadOnly _puces As New List(Of Chip)

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(InputChips), New FrameworkPropertyMetadata(GetType(InputChips)))
            FocusableProperty.OverrideMetadata(GetType(InputChips), New FrameworkPropertyMetadata(False))
            AutomationProperties.NameProperty.OverrideMetadata(GetType(InputChips), New FrameworkPropertyMetadata(String.Empty, AddressOf OnNomChanged))
        End Sub

        Public Sub New()
            SetCurrentValue(ValueProperty, New ObservableCollection(Of String)())
            [AddHandler](Chip.RemoveEvent, New RoutedEventHandler(AddressOf Puce_Remove))
        End Sub

        ''' <summary>Les valeurs.</summary>
        Public Property Value As ObservableCollection(Of String)
            Get
                Return DirectCast(GetValue(ValueProperty), ObservableCollection(Of String))
            End Get
            Set(value As ObservableCollection(Of String))
                SetValue(ValueProperty, value)
            End Set
        End Property

        ''' <summary>Le texte gris du champ vide.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour la bordure rouge d'une saisie refusée.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <summary>Un séparateur qui valide aussi la valeur.</summary>
        Public Property Separator As String
            Get
                Return CStr(GetValue(SeparatorProperty))
            End Get
            Set(value As String)
                SetValue(SeparatorProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre de valeurs au plus ; 0 : sans limite.</summary>
        Public Property Max As Integer
            Get
                Return CInt(GetValue(MaxProperty))
            End Get
            Set(value As Integer)
                SetValue(MaxProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour accepter deux fois la même valeur.</summary>
        Public Property AllowDuplicate As Boolean
            Get
                Return CBool(GetValue(AllowDuplicateProperty))
            End Get
            Set(value As Boolean)
                SetValue(AllowDuplicateProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour valider aussi la valeur en quittant le champ.</summary>
        Public Property AddOnBlur As Boolean
            Get
                Return CBool(GetValue(AddOnBlurProperty))
            End Get
            Set(value As Boolean)
                SetValue(AddOnBlurProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le nombre de valeurs atteint <see cref="Max"/>.</summary>
        Public ReadOnly Property IsMaxedOut As Boolean
            Get
                Return CBool(GetValue(IsMaxedOutProperty))
            End Get
        End Property

        ''' <summary>Vrai quand il y a au moins une valeur.</summary>
        Public ReadOnly Property HasValues As Boolean
            Get
                Return CBool(GetValue(HasValuesProperty))
            End Get
        End Property

        ''' <summary>Une valeur a été ajoutée.</summary>
        Public Custom Event Add As EventHandler(Of InputChipsEventArgs)
            AddHandler(value As EventHandler(Of InputChipsEventArgs))
                [AddHandler](AddEvent, value)
            End AddHandler
            RemoveHandler(value As EventHandler(Of InputChipsEventArgs))
                [RemoveHandler](AddEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As InputChipsEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Une valeur a été retirée.</summary>
        Public Custom Event Remove As EventHandler(Of InputChipsEventArgs)
            AddHandler(value As EventHandler(Of InputChipsEventArgs))
                [AddHandler](RemoveEvent, value)
            End AddHandler
            RemoveHandler(value As EventHandler(Of InputChipsEventArgs))
                [RemoveHandler](RemoveEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As InputChipsEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Les puces montrées, pour les tests.</summary>
        Friend ReadOnly Property Puces As IReadOnlyList(Of Chip)
            Get
                Return _puces
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _saisie IsNot Nothing Then
                RemoveHandler _saisie.PreviewKeyDown, AddressOf Saisie_PreviewKeyDown
                RemoveHandler _saisie.TextChanged, AddressOf Saisie_TextChanged
                RemoveHandler _saisie.LostKeyboardFocus, AddressOf Saisie_LostKeyboardFocus
            End If
            For Each puce In _puces
                _rangee?.Children.Remove(puce)
            Next
            MyBase.OnApplyTemplate()
            _saisie = TryCast(GetTemplateChild("PART_Input"), TextBox)
            _rangee = TryCast(GetTemplateChild("PART_Chips"), System.Windows.Controls.Panel)
            If _saisie IsNot Nothing Then
                AddHandler _saisie.PreviewKeyDown, AddressOf Saisie_PreviewKeyDown
                AddHandler _saisie.TextChanged, AddressOf Saisie_TextChanged
                AddHandler _saisie.LostKeyboardFocus, AddressOf Saisie_LostKeyboardFocus
            End If
            Montrer()
            Nommer()
        End Sub

        Private Shared Sub OnValueChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, InputChips)
            Dim avant = TryCast(e.OldValue, INotifyCollectionChanged)
            Dim apres = TryCast(e.NewValue, INotifyCollectionChanged)
            If avant IsNot Nothing Then RemoveHandler avant.CollectionChanged, AddressOf champ.Valeurs_CollectionChanged
            If apres IsNot Nothing Then AddHandler apres.CollectionChanged, AddressOf champ.Valeurs_CollectionChanged
            champ.Montrer()
        End Sub

        Private Sub Valeurs_CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            Montrer()
        End Sub

        Private Shared Sub OnLimiteChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, InputChips).Montrer()
        End Sub

        ' Une puce par valeur, devant la saisie.
        Private Sub Montrer()
            For Each puce In _puces
                _rangee?.Children.Remove(puce)
            Next
            _puces.Clear()
            Dim valeurs = If(Value, New ObservableCollection(Of String)())
            For i = 0 To valeurs.Count - 1
                Dim puce As New Chip With {.Content = valeurs(i), .Removable = True}
                puce.SetResourceReference(PaddingProperty, "mkm.inputchips.chip.padding")
                puce.SetResourceReference(Chip.CornerRadiusProperty, "mkm.inputchips.chip.border.radius")
                _puces.Add(puce)
                _rangee?.Children.Insert(i, puce)
            Next
            SetValue(HasValuesPropertyKey, valeurs.Count > 0)
            SetValue(IsMaxedOutPropertyKey, Max > 0 AndAlso valeurs.Count >= Max)
        End Sub

        ''' <summary>Ajoute une valeur, si elle convient (non vide, sous la limite, sans doublon refusé).</summary>
        Friend Function Ajouter(texte As String) As Boolean
            Dim valeur = If(texte, String.Empty).Trim()
            If valeur.Length = 0 OrElse Value Is Nothing Then Return False
            If Max > 0 AndAlso Value.Count >= Max Then Return False
            If Not AllowDuplicate AndAlso Value.Contains(valeur) Then Return False
            Value.Add(valeur)
            Me.RaiseEvent(New InputChipsEventArgs(AddEvent, Me, valeur))
            Return True
        End Function

        ''' <summary>Retire la valeur de ce rang.</summary>
        Friend Sub Retirer(indice As Integer)
            If Value Is Nothing OrElse indice < 0 OrElse indice >= Value.Count Then Return
            Dim valeur = Value(indice)
            Value.RemoveAt(indice)
            Me.RaiseEvent(New InputChipsEventArgs(RemoveEvent, Me, valeur))
        End Sub

        ' Entrée valide la saisie ; Retour arrière, la saisie vide, retire la dernière valeur.
        Private Sub Saisie_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            Select Case e.Key
                Case Key.Enter
                    If Ajouter(_saisie.Text) Then _saisie.Clear()
                    e.Handled = True
                Case Key.Back
                    If _saisie.Text.Length = 0 AndAlso _saisie.SelectionLength = 0 AndAlso Value IsNot Nothing AndAlso Value.Count > 0 Then
                        Retirer(Value.Count - 1)
                        e.Handled = True
                    End If
            End Select
        End Sub

        ' Le séparateur, tapé ou collé, valide chaque morceau qui le précède.
        Private Sub Saisie_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim separateur = Separator
            If String.IsNullOrEmpty(separateur) OrElse Not _saisie.Text.Contains(separateur) Then Return
            Dim morceaux = _saisie.Text.Split({separateur}, StringSplitOptions.None)
            For i = 0 To morceaux.Length - 2
                Ajouter(morceaux(i))
            Next
            _saisie.Text = morceaux(morceaux.Length - 1)
            _saisie.CaretIndex = _saisie.Text.Length
        End Sub

        Private Sub Saisie_LostKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
            If AddOnBlur AndAlso Ajouter(_saisie.Text) Then _saisie.Clear()
        End Sub

        ' La croix d'une puce retire sa valeur.
        Private Sub Puce_Remove(sender As Object, e As RoutedEventArgs)
            Dim puce = TryCast(e.OriginalSource, Chip)
            If puce Is Nothing Then Return
            Dim indice = _puces.IndexOf(puce)
            If indice < 0 Then Return
            e.Handled = True
            Retirer(indice)
            _saisie?.Focus()
        End Sub

        ''' <summary>Un clic dans le champ, hors des puces, amène le clavier à la saisie.</summary>
        Protected Overrides Sub OnMouseLeftButtonDown(e As MouseButtonEventArgs)
            MyBase.OnMouseLeftButtonDown(e)
            If e.Handled OrElse _saisie Is Nothing Then Return
            _saisie.Focus()
            e.Handled = True
        End Sub

        Private Shared Sub OnNomChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, InputChips).Nommer()
        End Sub

        ' La saisie porte le nom du champ, sinon son texte d'aide.
        Private Sub Nommer()
            If _saisie Is Nothing Then Return
            Dim nom = AutomationProperties.GetName(Me)
            AutomationProperties.SetName(_saisie, If(String.IsNullOrEmpty(nom), If(Placeholder, String.Empty), nom))
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c> : ses puces et sa saisie.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group, Function() Placeholder)
        End Function

    End Class

End Namespace
