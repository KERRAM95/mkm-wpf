Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>La force d'un mot de passe, selon <see cref="Password.MediumRegex"/> et <see cref="Password.StrongRegex"/>.</summary>
    Public Enum PasswordStrength
        ''' <summary>Rien n'est saisi.</summary>
        None
        ''' <summary>Faible : ni moyen ni fort.</summary>
        Weak
        ''' <summary>Moyen.</summary>
        Medium
        ''' <summary>Fort.</summary>
        Strong
    End Enum

    ''' <summary>
    ''' Le champ de mot de passe de mkm (<c>Password</c>) : un champ masqué, l'œil qui montre ou cache le texte, et sous lui,
    ''' le temps de la saisie, une jauge de la force du mot de passe.
    ''' <code>
    ''' &lt;mkm:Password Placeholder="Mot de passe" ToggleMask="True" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Comme le <see cref="PasswordBox"/> de WPF, le mot de passe (<see cref="Password"/>) n'est pas une propriété liable :
    ''' il se lit et s'écrit en code, et <see cref="PasswordChanged"/> dit qu'il change.
    ''' </remarks>
    <TemplatePart(Name:="PART_PasswordBox", Type:=GetType(PasswordBox))>
    <TemplatePart(Name:="PART_TextBox", Type:=GetType(TextBox))>
    <TemplatePart(Name:="PART_Toggle", Type:=GetType(ButtonBase))>
    Public Class Password
        Inherits Control

        ''' <summary>Le texte gris du champ vide.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Placeholder), GetType(String), GetType(Password), New FrameworkPropertyMetadata(Nothing, AddressOf OnNomChanged))

        ''' <summary>Vrai pour la bordure rouge d'une saisie refusée.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType(Password), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai (par défaut) pour la jauge de force sous le champ, pendant la saisie.</summary>
        Public Shared ReadOnly FeedbackProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Feedback), GetType(Boolean), GetType(Password), New FrameworkPropertyMetadata(True, AddressOf OnOuvertureChanged))

        ''' <summary>Vrai pour l'œil qui montre ou cache le mot de passe.</summary>
        Public Shared ReadOnly ToggleMaskProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ToggleMask), GetType(Boolean), GetType(Password), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai quand le mot de passe se montre en clair.</summary>
        Public Shared ReadOnly IsUnmaskedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsUnmasked), GetType(Boolean), GetType(Password), New FrameworkPropertyMetadata(False, AddressOf OnIsUnmaskedChanged))

        ''' <summary>L'expression d'un mot de passe moyen : celle de mkm par défaut (6 caractères, deux genres parmi
        ''' minuscules, majuscules et chiffres).</summary>
        Public Shared ReadOnly MediumRegexProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MediumRegex), GetType(String), GetType(Password),
            New FrameworkPropertyMetadata("^(((?=.*[a-z])(?=.*[A-Z]))|((?=.*[a-z])(?=.*[0-9]))|((?=.*[A-Z])(?=.*[0-9])))(?=.{6,})", AddressOf OnMesureChanged))

        ''' <summary>L'expression d'un mot de passe fort : celle de mkm par défaut (8 caractères, minuscule, majuscule et
        ''' chiffre).</summary>
        Public Shared ReadOnly StrongRegexProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(StrongRegex), GetType(String), GetType(Password),
            New FrameworkPropertyMetadata("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,})", AddressOf OnMesureChanged))

        ''' <summary>Le texte de la jauge vide : « Saisissez un mot de passe » par défaut.</summary>
        Public Shared ReadOnly PromptLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(PromptLabel), GetType(String), GetType(Password), New FrameworkPropertyMetadata(Nothing, AddressOf OnMesureChanged))

        ''' <summary>Le texte d'un mot de passe faible : « Faible » par défaut.</summary>
        Public Shared ReadOnly WeakLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(WeakLabel), GetType(String), GetType(Password), New FrameworkPropertyMetadata(Nothing, AddressOf OnMesureChanged))

        ''' <summary>Le texte d'un mot de passe moyen : « Moyen » par défaut.</summary>
        Public Shared ReadOnly MediumLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MediumLabel), GetType(String), GetType(Password), New FrameworkPropertyMetadata(Nothing, AddressOf OnMesureChanged))

        ''' <summary>Le texte d'un mot de passe fort : « Fort » par défaut.</summary>
        Public Shared ReadOnly StrongLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(StrongLabel), GetType(String), GetType(Password), New FrameworkPropertyMetadata(Nothing, AddressOf OnMesureChanged))

        Private Shared ReadOnly StrengthPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Strength), GetType(PasswordStrength), GetType(Password), New FrameworkPropertyMetadata(PasswordStrength.None))
        ''' <summary>La force du mot de passe saisi.</summary>
        Public Shared ReadOnly StrengthProperty As DependencyProperty = StrengthPropertyKey.DependencyProperty

        Private Shared ReadOnly StrengthTextPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(StrengthText), GetType(String), GetType(Password), New FrameworkPropertyMetadata(String.Empty))
        ''' <summary>Le texte de la jauge : la force, ou l'invite à saisir.</summary>
        Public Shared ReadOnly StrengthTextProperty As DependencyProperty = StrengthTextPropertyKey.DependencyProperty

        Private Shared ReadOnly HasPasswordPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasPassword), GetType(Boolean), GetType(Password), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand un mot de passe est saisi.</summary>
        Public Shared ReadOnly HasPasswordProperty As DependencyProperty = HasPasswordPropertyKey.DependencyProperty

        Private Shared ReadOnly IsFeedbackOpenPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFeedbackOpen), GetType(Boolean), GetType(Password), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand la jauge se montre sous le champ.</summary>
        Public Shared ReadOnly IsFeedbackOpenProperty As DependencyProperty = IsFeedbackOpenPropertyKey.DependencyProperty

        ''' <summary>Le mot de passe a changé.</summary>
        Public Shared ReadOnly PasswordChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(PasswordChanged), RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(Password))

        Private _masque As PasswordBox
        Private _clair As TextBox
        Private _oeil As ButtonBase
        Private _texteDeLaJauge As FrameworkElement
        Private _valeur As String = String.Empty
        Private _synchro As Boolean
        Private _jaugeFermeeParEchap As Boolean

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Password), New FrameworkPropertyMetadata(GetType(Password)))
            FocusableProperty.OverrideMetadata(GetType(Password), New FrameworkPropertyMetadata(False))
            IsTabStopProperty.OverrideMetadata(GetType(Password), New FrameworkPropertyMetadata(False))
            AutomationProperties.NameProperty.OverrideMetadata(GetType(Password), New FrameworkPropertyMetadata(String.Empty, AddressOf OnNomChanged))
        End Sub

        Public Sub New()
            AddHandler Localizer.Current.LanguageChanged, Sub()
                                                              Mesurer()
                                                              NommerLOeil()
                                                          End Sub
            Mesurer()
        End Sub

        ''' <summary>Le mot de passe saisi.</summary>
        Public Property Password As String
            Get
                Return _valeur
            End Get
            Set(value As String)
                Ecrire(If(value, String.Empty), Nothing)
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

        ''' <summary>Vrai pour la jauge de force.</summary>
        Public Property Feedback As Boolean
            Get
                Return CBool(GetValue(FeedbackProperty))
            End Get
            Set(value As Boolean)
                SetValue(FeedbackProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour l'œil qui montre ou cache le mot de passe.</summary>
        Public Property ToggleMask As Boolean
            Get
                Return CBool(GetValue(ToggleMaskProperty))
            End Get
            Set(value As Boolean)
                SetValue(ToggleMaskProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le mot de passe se montre en clair.</summary>
        Public Property IsUnmasked As Boolean
            Get
                Return CBool(GetValue(IsUnmaskedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsUnmaskedProperty, value)
            End Set
        End Property

        ''' <summary>L'expression d'un mot de passe moyen.</summary>
        Public Property MediumRegex As String
            Get
                Return CStr(GetValue(MediumRegexProperty))
            End Get
            Set(value As String)
                SetValue(MediumRegexProperty, value)
            End Set
        End Property

        ''' <summary>L'expression d'un mot de passe fort.</summary>
        Public Property StrongRegex As String
            Get
                Return CStr(GetValue(StrongRegexProperty))
            End Get
            Set(value As String)
                SetValue(StrongRegexProperty, value)
            End Set
        End Property

        ''' <summary>Le texte de la jauge vide.</summary>
        Public Property PromptLabel As String
            Get
                Return CStr(GetValue(PromptLabelProperty))
            End Get
            Set(value As String)
                SetValue(PromptLabelProperty, value)
            End Set
        End Property

        ''' <summary>Le texte d'un mot de passe faible.</summary>
        Public Property WeakLabel As String
            Get
                Return CStr(GetValue(WeakLabelProperty))
            End Get
            Set(value As String)
                SetValue(WeakLabelProperty, value)
            End Set
        End Property

        ''' <summary>Le texte d'un mot de passe moyen.</summary>
        Public Property MediumLabel As String
            Get
                Return CStr(GetValue(MediumLabelProperty))
            End Get
            Set(value As String)
                SetValue(MediumLabelProperty, value)
            End Set
        End Property

        ''' <summary>Le texte d'un mot de passe fort.</summary>
        Public Property StrongLabel As String
            Get
                Return CStr(GetValue(StrongLabelProperty))
            End Get
            Set(value As String)
                SetValue(StrongLabelProperty, value)
            End Set
        End Property

        ''' <summary>La force du mot de passe saisi.</summary>
        Public ReadOnly Property Strength As PasswordStrength
            Get
                Return DirectCast(GetValue(StrengthProperty), PasswordStrength)
            End Get
        End Property

        ''' <summary>Le texte de la jauge.</summary>
        Public ReadOnly Property StrengthText As String
            Get
                Return CStr(GetValue(StrengthTextProperty))
            End Get
        End Property

        ''' <summary>Vrai quand un mot de passe est saisi.</summary>
        Public ReadOnly Property HasPassword As Boolean
            Get
                Return CBool(GetValue(HasPasswordProperty))
            End Get
        End Property

        ''' <summary>Vrai quand la jauge se montre.</summary>
        Public ReadOnly Property IsFeedbackOpen As Boolean
            Get
                Return CBool(GetValue(IsFeedbackOpenProperty))
            End Get
        End Property

        ''' <summary>Le mot de passe a changé.</summary>
        Public Custom Event PasswordChanged As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                [AddHandler](PasswordChangedEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                [RemoveHandler](PasswordChangedEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _masque IsNot Nothing Then RemoveHandler _masque.PasswordChanged, AddressOf Masque_PasswordChanged
            If _clair IsNot Nothing Then RemoveHandler _clair.TextChanged, AddressOf Clair_TextChanged
            If _oeil IsNot Nothing Then RemoveHandler _oeil.Click, AddressOf Oeil_Click
            MyBase.OnApplyTemplate()
            _masque = TryCast(GetTemplateChild("PART_PasswordBox"), PasswordBox)
            _clair = TryCast(GetTemplateChild("PART_TextBox"), TextBox)
            _oeil = TryCast(GetTemplateChild("PART_Toggle"), ButtonBase)
            _texteDeLaJauge = TryCast(GetTemplateChild("TexteDeLaJauge"), FrameworkElement)
            _synchro = True
            If _masque IsNot Nothing Then _masque.Password = _valeur
            If _clair IsNot Nothing Then _clair.Text = _valeur
            _synchro = False
            If _masque IsNot Nothing Then AddHandler _masque.PasswordChanged, AddressOf Masque_PasswordChanged
            If _clair IsNot Nothing Then AddHandler _clair.TextChanged, AddressOf Clair_TextChanged
            If _oeil IsNot Nothing Then AddHandler _oeil.Click, AddressOf Oeil_Click
            Nommer()
            NommerLOeil()
        End Sub

        ' L'œil dit ce qu'il fera : « Afficher le mot de passe », ou « Masquer le mot de passe ».
        Private Sub NommerLOeil()
            If _oeil Is Nothing Then Return
            AutomationProperties.SetName(_oeil, Localizer.Current.T(If(IsUnmasked, "aria.hidePassword", "aria.showPassword")))
        End Sub

        Private Sub Masque_PasswordChanged(sender As Object, e As RoutedEventArgs)
            If Not _synchro Then Ecrire(_masque.Password, _masque)
        End Sub

        Private Sub Clair_TextChanged(sender As Object, e As TextChangedEventArgs)
            If Not _synchro Then Ecrire(_clair.Text, _clair)
        End Sub

        ' La valeur, recopiée dans l'autre champ ; la jauge mesurée ; l'évènement.
        Private Sub Ecrire(valeur As String, source As Object)
            If valeur = _valeur Then Return
            _valeur = valeur
            _synchro = True
            Try
                If _masque IsNot Nothing AndAlso source IsNot _masque Then _masque.Password = valeur
                If _clair IsNot Nothing AndAlso source IsNot _clair Then _clair.Text = valeur
            Finally
                _synchro = False
            End Try
            SetValue(HasPasswordPropertyKey, valeur.Length > 0)
            _jaugeFermeeParEchap = False
            Mesurer()
            Ouvrir()
            Me.RaiseEvent(New RoutedEventArgs(PasswordChangedEvent, Me))
        End Sub

        ''' <summary>La force d'un mot de passe : fort, moyen, faible (non vide), ou rien.</summary>
        Friend Function Force(valeur As String) As PasswordStrength
            If String.IsNullOrEmpty(valeur) Then Return PasswordStrength.None
            If Correspond(StrongRegex, valeur) Then Return PasswordStrength.Strong
            If Correspond(MediumRegex, valeur) Then Return PasswordStrength.Medium
            Return PasswordStrength.Weak
        End Function

        Private Shared Function Correspond(motif As String, valeur As String) As Boolean
            If String.IsNullOrEmpty(motif) Then Return False
            Try
                Return Regex.IsMatch(valeur, motif)
            Catch ex As ArgumentException
                Return False
            End Try
        End Function

        Private Sub Mesurer()
            Dim force = Me.Force(_valeur)
            Dim texte As String
            Select Case force
                Case PasswordStrength.Weak
                    texte = If(WeakLabel, Localizer.Current.T("locale.weak"))
                Case PasswordStrength.Medium
                    texte = If(MediumLabel, Localizer.Current.T("locale.medium"))
                Case PasswordStrength.Strong
                    texte = If(StrongLabel, Localizer.Current.T("locale.strong"))
                Case Else
                    texte = If(PromptLabel, Localizer.Current.T("locale.passwordPrompt"))
            End Select
            Dim avant = StrengthText
            SetValue(StrengthPropertyKey, force)
            SetValue(StrengthTextPropertyKey, texte)
            ' Le texte de la jauge s'annonce, comme mkm (aria-live).
            If avant <> texte AndAlso _texteDeLaJauge IsNot Nothing AndAlso IsFeedbackOpen Then
                UIElementAutomationPeer.CreatePeerForElement(_texteDeLaJauge)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)
            End If
        End Sub

        Private Shared Sub OnMesureChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Password).Mesurer()
        End Sub

        Private Shared Sub OnOuvertureChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Password).Ouvrir()
        End Sub

        ' La jauge se montre pendant la saisie : le clavier dans le champ ; Échap la cache jusqu'à la touche suivante.
        Private Sub Ouvrir()
            SetValue(IsFeedbackOpenPropertyKey, Feedback AndAlso IsKeyboardFocusWithin AndAlso Not _jaugeFermeeParEchap)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnIsKeyboardFocusWithinChanged(e As DependencyPropertyChangedEventArgs)
            MyBase.OnIsKeyboardFocusWithinChanged(e)
            _jaugeFermeeParEchap = False
            Ouvrir()
        End Sub

        ''' <summary>Échap cache la jauge.</summary>
        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            MyBase.OnPreviewKeyDown(e)
            If e.Key = Key.Escape AndAlso IsFeedbackOpen Then
                _jaugeFermeeParEchap = True
                Ouvrir()
                e.Handled = True
            End If
        End Sub

        Private Sub Oeil_Click(sender As Object, e As RoutedEventArgs)
            SetCurrentValue(IsUnmaskedProperty, Not IsUnmasked)
        End Sub

        ' Montré ou caché, le clavier passe au champ visible, le curseur au bout.
        Private Shared Sub OnIsUnmaskedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, Password)
            champ.NommerLOeil()
            If Not champ.IsKeyboardFocusWithin Then Return
            champ.Dispatcher.BeginInvoke(Threading.DispatcherPriority.Input, New System.Action(
                Sub()
                    If champ.IsUnmasked Then
                        champ._clair?.Focus()
                        If champ._clair IsNot Nothing Then champ._clair.CaretIndex = champ._clair.Text.Length
                    Else
                        champ._masque?.Focus()
                    End If
                End Sub))
        End Sub

        ' Les deux champs portent le nom du Password, sinon son texte d'aide : ce sont eux que lisent les lecteurs d'écran.
        Private Shared Sub OnNomChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Password).Nommer()
        End Sub

        Private Sub Nommer()
            Dim nom = AutomationProperties.GetName(Me)
            If String.IsNullOrEmpty(nom) Then nom = Placeholder
            For Each champ In New FrameworkElement() {_masque, _clair}
                If champ Is Nothing Then Continue For
                AutomationProperties.SetName(champ, If(nom, String.Empty))
                AutomationProperties.SetLabeledBy(champ, AutomationProperties.GetLabeledBy(Me))
            Next
        End Sub

    End Class

End Namespace
