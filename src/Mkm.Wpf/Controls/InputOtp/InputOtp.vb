Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Input
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le code à usage unique de mkm (<c>InputOtp</c>) : une case par caractère ; la frappe passe à la case suivante,
    ''' Retour arrière revient à la précédente, un code collé remplit les cases.
    ''' <code>
    ''' &lt;mkm:InputOtp Length="6" IntegerOnly="True" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Inputs", Type:=GetType(System.Windows.Controls.Panel))>
    Public Class InputOtp
        Inherits Control

        ''' <summary>Le code saisi. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(String), GetType(InputOtp),
            New FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnValueChanged))

        ''' <summary>Le nombre de cases : 4 par défaut.</summary>
        Public Shared ReadOnly LengthProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Length), GetType(Integer), GetType(InputOtp), New FrameworkPropertyMetadata(4, AddressOf OnFormeChanged), AddressOf EstUneLongueur)

        ''' <summary>Vrai pour des cases masquées, comme un mot de passe.</summary>
        Public Shared ReadOnly MaskProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Mask), GetType(Boolean), GetType(InputOtp), New FrameworkPropertyMetadata(False, AddressOf OnFormeChanged))

        ''' <summary>Vrai pour n'accepter que des chiffres.</summary>
        Public Shared ReadOnly IntegerOnlyProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IntegerOnly), GetType(Boolean), GetType(InputOtp), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai pour un code qu'on lit sans le changer.</summary>
        Public Shared ReadOnly IsReadOnlyProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsReadOnly), GetType(Boolean), GetType(InputOtp), New FrameworkPropertyMetadata(False, AddressOf OnFormeChanged))

        ''' <summary>Vrai pour la bordure rouge d'une saisie refusée.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType(InputOtp), New FrameworkPropertyMetadata(False, AddressOf OnFormeChanged))

        Private Shared _styleMasque As Style
        Private ReadOnly _cases As New List(Of Control)
        Private _jetons As String() = {}
        Private _rangee As System.Windows.Controls.Panel
        Private _ecriture As Boolean

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(InputOtp), New FrameworkPropertyMetadata(GetType(InputOtp)))
            FocusableProperty.OverrideMetadata(GetType(InputOtp), New FrameworkPropertyMetadata(False))
            IsTabStopProperty.OverrideMetadata(GetType(InputOtp), New FrameworkPropertyMetadata(False))
        End Sub

        Public Sub New()
            AddHandler Localizer.Current.LanguageChanged, Sub() Nommer()
        End Sub

        ''' <summary>Le code saisi.</summary>
        Public Property Value As String
            Get
                Return CStr(GetValue(ValueProperty))
            End Get
            Set(value As String)
                SetValue(ValueProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre de cases.</summary>
        Public Property Length As Integer
            Get
                Return CInt(GetValue(LengthProperty))
            End Get
            Set(value As Integer)
                SetValue(LengthProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour des cases masquées.</summary>
        Public Property Mask As Boolean
            Get
                Return CBool(GetValue(MaskProperty))
            End Get
            Set(value As Boolean)
                SetValue(MaskProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour n'accepter que des chiffres.</summary>
        Public Property IntegerOnly As Boolean
            Get
                Return CBool(GetValue(IntegerOnlyProperty))
            End Get
            Set(value As Boolean)
                SetValue(IntegerOnlyProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour un code qu'on lit sans le changer.</summary>
        Public Property IsReadOnly As Boolean
            Get
                Return CBool(GetValue(IsReadOnlyProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsReadOnlyProperty, value)
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

        ''' <summary>Les cases, pour les tests.</summary>
        Friend ReadOnly Property Cases As IReadOnlyList(Of Control)
            Get
                Return _cases
            End Get
        End Property

        Private Shared Function EstUneLongueur(valeur As Object) As Boolean
            Return CInt(valeur) >= 1
        End Function

        ' Le style du champ masqué de Password, pris dans son dictionnaire (une clé de chaîne ne se trouve pas depuis le code).
        Private Shared ReadOnly Property StyleMasque As Style
            Get
                If _styleMasque Is Nothing Then
                    Dim dictionnaire As New ResourceDictionary With {
                        .Source = New Uri("pack://application:,,,/Mkm.Wpf;component/Controls/Password/Password.xaml", UriKind.Absolute)}
                    _styleMasque = TryCast(dictionnaire("mkm.Password.Saisie"), Style)
                End If
                Return _styleMasque
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            _rangee?.Children.Clear()
            MyBase.OnApplyTemplate()
            _rangee = TryCast(GetTemplateChild("PART_Inputs"), System.Windows.Controls.Panel)
            Construire()
        End Sub

        Private Shared Sub OnFormeChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, InputOtp).Construire()
        End Sub

        Private Shared Sub OnValueChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim code = DirectCast(d, InputOtp)
            If code._ecriture Then Return
            code.Repartir(CStr(e.NewValue))
        End Sub

        ' Une case par caractère, champ ou champ masqué ; chacune garde un caractère.
        Private Sub Construire()
            _rangee?.Children.Clear()
            _cases.Clear()
            For i = 0 To Length - 1
                Dim caseDuCode As Control
                If Mask Then
                    Dim masque As New PasswordBox With {.MaxLength = 1, .HorizontalContentAlignment = HorizontalAlignment.Center}
                    If StyleMasque IsNot Nothing Then masque.Style = StyleMasque
                    If Invalid Then masque.SetResourceReference(BorderBrushProperty, "mkm.inputtext.invalid.border.color")
                    caseDuCode = masque
                Else
                    caseDuCode = New InputText With {.MaxLength = 1, .TextAlignment = TextAlignment.Center, .Invalid = Invalid, .IsReadOnly = IsReadOnly}
                End If
                caseDuCode.SetResourceReference(WidthProperty, "mkm.inputotp.input.width")
                AddHandler caseDuCode.PreviewTextInput, AddressOf Case_PreviewTextInput
                AddHandler caseDuCode.PreviewKeyDown, AddressOf Case_PreviewKeyDown
                AddHandler caseDuCode.GotKeyboardFocus, AddressOf Case_GotKeyboardFocus
                DataObject.AddPastingHandler(caseDuCode, AddressOf Case_Pasting)
                _cases.Add(caseDuCode)
                _rangee?.Children.Add(caseDuCode)
            Next
            Repartir(Value)
            Nommer()
        End Sub

        ' Chaque case se nomme « Caractère 2 sur 6 ».
        Private Sub Nommer()
            For i = 0 To _cases.Count - 1
                AutomationProperties.SetName(_cases(i), Localizer.Current.T("aria.otpLabel", i + 1, _cases.Count))
            Next
        End Sub

        ' Le code, un caractère par case.
        Private Sub Repartir(code As String)
            code = If(code, String.Empty)
            _jetons = New String(Length - 1) {}
            For i = 0 To Length - 1
                _jetons(i) = If(i < code.Length, code(i).ToString(), String.Empty)
                If i < _cases.Count Then Montrer(_cases(i), _jetons(i))
            Next
        End Sub

        Private Shared Sub Montrer(caseDuCode As Control, texte As String)
            Dim champ = TryCast(caseDuCode, TextBox)
            If champ IsNot Nothing Then
                If champ.Text <> texte Then champ.Text = texte
                Return
            End If
            Dim masque = TryCast(caseDuCode, PasswordBox)
            If masque IsNot Nothing AndAlso masque.Password <> texte Then masque.Password = texte
        End Sub

        Private Sub Ecrire()
            _ecriture = True
            Try
                SetCurrentValue(ValueProperty, String.Concat(_jetons))
            Finally
                _ecriture = False
            End Try
        End Sub

        Private Sub Poser(indice As Integer, texte As String)
            _jetons(indice) = texte
            Montrer(_cases(indice), texte)
            Ecrire()
        End Sub

        Private Sub Aller(indice As Integer)
            If indice < 0 OrElse indice >= _cases.Count Then Return
            _cases(indice).Focus()
        End Sub

        ' Le caractère tapé remplace celui de la case, et le clavier passe à la suivante.
        Private Sub Case_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            e.Handled = True
            If IsReadOnly OrElse String.IsNullOrEmpty(e.Text) Then Return
            Dim c = e.Text(0)
            If IntegerOnly AndAlso Not Char.IsDigit(c) Then Return
            Dim indice = _cases.IndexOf(DirectCast(sender, Control))
            Poser(indice, c.ToString())
            Aller(indice + 1)
        End Sub

        ' ← et → passent d'une case à l'autre ; Retour arrière vide la case et revient à la précédente ; Suppr la vide.
        Private Sub Case_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            Dim indice = _cases.IndexOf(DirectCast(sender, Control))
            Select Case e.Key
                Case Key.Left
                    Aller(indice - 1)
                Case Key.Right
                    Aller(indice + 1)
                Case Key.Up, Key.Down, Key.Space
                Case Key.Back
                    If Not IsReadOnly AndAlso _jetons(indice).Length > 0 Then Poser(indice, String.Empty)
                    Aller(indice - 1)
                Case Key.Delete
                    If Not IsReadOnly Then Poser(indice, String.Empty)
                Case Else
                    Return
            End Select
            e.Handled = True
        End Sub

        ' La case qui prend le clavier choisit son caractère : la frappe le remplace.
        Private Sub Case_GotKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs)
            Dispatcher.BeginInvoke(Threading.DispatcherPriority.Input, New System.Action(
                Sub()
                    TryCast(sender, TextBox)?.SelectAll()
                    TryCast(sender, PasswordBox)?.SelectAll()
                End Sub))
        End Sub

        ' Un code collé remplit les cases depuis la première, comme mkm ; avec IntegerOnly, un code de chiffres seulement.
        Private Sub Case_Pasting(sender As Object, e As DataObjectPastingEventArgs)
            e.CancelCommand()
            If IsReadOnly Then Return
            Dim texte = TryCast(e.DataObject.GetData(DataFormats.UnicodeText), String)
            If String.IsNullOrEmpty(texte) Then Return
            Coller(texte)
        End Sub

        ''' <summary>Remplit les cases depuis la première avec un code (collé).</summary>
        Friend Sub Coller(texte As String)
            Dim code = texte.Trim()
            If code.Length > Length Then code = code.Substring(0, Length)
            If IntegerOnly AndAlso Not code.All(Function(c) Char.IsDigit(c)) Then Return
            Repartir(code)
            Ecrire()
            Aller(Math.Min(code.Length, Length - 1))
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c> : ses cases.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

End Namespace
