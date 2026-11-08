Imports System.Globalization
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>Où se placent les boutons d'un <see cref="InputNumber"/>.</summary>
    Public Enum InputNumberButtonLayout
        ''' <summary>L'un sur l'autre, dans le champ, à droite (par défaut).</summary>
        Stacked
        ''' <summary>Moins à gauche du champ, plus à droite.</summary>
        Horizontal
    End Enum

    ''' <summary>
    ''' La saisie d'un nombre de mkm (<c>InputNumber</c>) : le champ d'InputText, qui n'accepte que des chiffres, écrit
    ''' le nombre au format régional (séparateur des milliers, décimales) en le quittant, le borne entre
    ''' <see cref="Minimum"/> et <see cref="Maximum"/> ; les flèches haut et bas, et les boutons
    ''' (<see cref="ShowButtons"/>), l'augmentent ou le diminuent de <see cref="[Step]"/>, en répétant tant qu'on les
    ''' tient.
    ''' <code>&lt;mkm:InputNumber Value="{Binding Masse}" Suffix=" kg" MaxFractionDigits="2" ShowButtons="True" /&gt;</code>
    ''' </summary>
    ''' <remarks><see cref="Value"/> est vide (<c>Nothing</c>) quand le champ l'est.</remarks>
    <TemplatePart(Name:=InputNumber.PartTextBox, Type:=GetType(TextBox))>
    <TemplatePart(Name:=InputNumber.PartIncrement, Type:=GetType(ButtonBase))>
    <TemplatePart(Name:=InputNumber.PartDecrement, Type:=GetType(ButtonBase))>
    Public Class InputNumber
        Inherits Control

        ''' <summary>Une toupie pour les lecteurs d'écran : sa valeur, ses bornes, son champ et ses boutons.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeNombre(Me)
        End Function

        ''' <summary>Le champ dans le gabarit.</summary>
        Public Const PartTextBox As String = "PART_TextBox"
        ''' <summary>Le bouton qui augmente.</summary>
        Public Const PartIncrement As String = "PART_Increment"
        ''' <summary>Le bouton qui diminue.</summary>
        Public Const PartDecrement As String = "PART_Decrement"

        ''' <summary>Le nombre ; <c>Nothing</c> quand le champ est vide.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(Double?), GetType(InputNumber),
            New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnValueChanged))

        ''' <summary>Le plus petit nombre accepté (aucun par défaut).</summary>
        Public Shared ReadOnly MinimumProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Minimum), GetType(Double), GetType(InputNumber), New FrameworkPropertyMetadata(Double.NegativeInfinity))

        ''' <summary>Le plus grand nombre accepté (aucun par défaut).</summary>
        Public Shared ReadOnly MaximumProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Maximum), GetType(Double), GetType(InputNumber), New FrameworkPropertyMetadata(Double.PositiveInfinity))

        ''' <summary>Le pas des flèches et des boutons (1 par défaut).</summary>
        Public Shared ReadOnly StepProperty As DependencyProperty = DependencyProperty.Register(
            "Step", GetType(Double), GetType(InputNumber), New FrameworkPropertyMetadata(1.0))

        ''' <summary>Les décimales toujours écrites (0 par défaut).</summary>
        Public Shared ReadOnly MinFractionDigitsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MinFractionDigits), GetType(Integer), GetType(InputNumber), New FrameworkPropertyMetadata(0, AddressOf OnFormatChanged))

        ''' <summary>Les décimales gardées au plus (3 par défaut, comme le format des nombres du navigateur).</summary>
        Public Shared ReadOnly MaxFractionDigitsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MaxFractionDigits), GetType(Integer), GetType(InputNumber), New FrameworkPropertyMetadata(3, AddressOf OnFormatChanged))

        ''' <summary>Vrai (par défaut) si les milliers sont séparés.</summary>
        Public Shared ReadOnly UseGroupingProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(UseGrouping), GetType(Boolean), GetType(InputNumber), New FrameworkPropertyMetadata(True, AddressOf OnFormatChanged))

        ''' <summary>Ce qui s'écrit avant le nombre.</summary>
        Public Shared ReadOnly PrefixProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Prefix), GetType(String), GetType(InputNumber), New FrameworkPropertyMetadata(Nothing, AddressOf OnFormatChanged))

        ''' <summary>Ce qui s'écrit après le nombre (une unité : « kg »…).</summary>
        Public Shared ReadOnly SuffixProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Suffix), GetType(String), GetType(InputNumber), New FrameworkPropertyMetadata(Nothing, AddressOf OnFormatChanged))

        ''' <summary>Vrai si les boutons plus et moins se montrent.</summary>
        Public Shared ReadOnly ShowButtonsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowButtons), GetType(Boolean), GetType(InputNumber), New FrameworkPropertyMetadata(False))

        ''' <summary>Où se placent les boutons.</summary>
        Public Shared ReadOnly ButtonLayoutProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ButtonLayout), GetType(InputNumberButtonLayout), GetType(InputNumber), New FrameworkPropertyMetadata(InputNumberButtonLayout.Stacked))

        ''' <summary>Le texte d'aide, montré tant que le champ est vide.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = InputText.PlaceholderProperty.AddOwner(GetType(InputNumber))

        ''' <summary>Vrai si le nombre est refusé.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = InputText.InvalidProperty.AddOwner(GetType(InputNumber))

        ''' <summary>Vrai si le nombre se lit sans se modifier.</summary>
        Public Shared ReadOnly IsReadOnlyProperty As DependencyProperty = TextBoxBase.IsReadOnlyProperty.AddOwner(GetType(InputNumber))

        ''' <summary>Les coins d'un bouton de la saisie, posés par son gabarit (empilés ou à l'horizontale).</summary>
        Public Shared ReadOnly ButtonCornerRadiusProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "ButtonCornerRadius", GetType(CornerRadius), GetType(InputNumber), New FrameworkPropertyMetadata(New CornerRadius()))

        ''' <summary>Les coins d'un bouton de la saisie.</summary>
        Public Shared Function GetButtonCornerRadius(element As DependencyObject) As CornerRadius
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return DirectCast(element.GetValue(ButtonCornerRadiusProperty), CornerRadius)
        End Function

        ''' <summary>Pose les coins d'un bouton de la saisie.</summary>
        Public Shared Sub SetButtonCornerRadius(element As DependencyObject, value As CornerRadius)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(ButtonCornerRadiusProperty, value)
        End Sub

        Private _champ As TextBox
        Private _ecritParLeCode As Boolean

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(InputNumber), New FrameworkPropertyMetadata(GetType(InputNumber)))
            FocusableProperty.OverrideMetadata(GetType(InputNumber), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Le nombre ; <c>Nothing</c> quand le champ est vide.</summary>
        Public Property Value As Double?
            Get
                Return DirectCast(GetValue(ValueProperty), Double?)
            End Get
            Set(value As Double?)
                SetValue(ValueProperty, value)
            End Set
        End Property

        ''' <summary>Le plus petit nombre accepté.</summary>
        Public Property Minimum As Double
            Get
                Return CDbl(GetValue(MinimumProperty))
            End Get
            Set(value As Double)
                SetValue(MinimumProperty, value)
            End Set
        End Property

        ''' <summary>Le plus grand nombre accepté.</summary>
        Public Property Maximum As Double
            Get
                Return CDbl(GetValue(MaximumProperty))
            End Get
            Set(value As Double)
                SetValue(MaximumProperty, value)
            End Set
        End Property

        ''' <summary>Le pas des flèches et des boutons.</summary>
        Public Property [Step] As Double
            Get
                Return CDbl(GetValue(StepProperty))
            End Get
            Set(value As Double)
                SetValue(StepProperty, value)
            End Set
        End Property

        ''' <summary>Les décimales toujours écrites.</summary>
        Public Property MinFractionDigits As Integer
            Get
                Return CInt(GetValue(MinFractionDigitsProperty))
            End Get
            Set(value As Integer)
                SetValue(MinFractionDigitsProperty, value)
            End Set
        End Property

        ''' <summary>Les décimales gardées au plus.</summary>
        Public Property MaxFractionDigits As Integer
            Get
                Return CInt(GetValue(MaxFractionDigitsProperty))
            End Get
            Set(value As Integer)
                SetValue(MaxFractionDigitsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si les milliers sont séparés.</summary>
        Public Property UseGrouping As Boolean
            Get
                Return CBool(GetValue(UseGroupingProperty))
            End Get
            Set(value As Boolean)
                SetValue(UseGroupingProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui s'écrit avant le nombre.</summary>
        Public Property Prefix As String
            Get
                Return CStr(GetValue(PrefixProperty))
            End Get
            Set(value As String)
                SetValue(PrefixProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui s'écrit après le nombre.</summary>
        Public Property Suffix As String
            Get
                Return CStr(GetValue(SuffixProperty))
            End Get
            Set(value As String)
                SetValue(SuffixProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si les boutons plus et moins se montrent.</summary>
        Public Property ShowButtons As Boolean
            Get
                Return CBool(GetValue(ShowButtonsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowButtonsProperty, value)
            End Set
        End Property

        ''' <summary>Où se placent les boutons.</summary>
        Public Property ButtonLayout As InputNumberButtonLayout
            Get
                Return DirectCast(GetValue(ButtonLayoutProperty), InputNumberButtonLayout)
            End Get
            Set(value As InputNumberButtonLayout)
                SetValue(ButtonLayoutProperty, value)
            End Set
        End Property

        ''' <summary>Le texte d'aide.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le nombre est refusé.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le nombre se lit sans se modifier.</summary>
        Public Property IsReadOnly As Boolean
            Get
                Return CBool(GetValue(IsReadOnlyProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsReadOnlyProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre tel qu'il s'écrit, au format de la langue de l'élément.</summary>
        Public Function Format(nombre As Double?) As String
            If Not nombre.HasValue Then Return String.Empty
            Dim chiffres = If(UseGrouping, "#,##0", "0")
            Dim decimales = Math.Max(0, MinFractionDigits)
            Dim plus = Math.Max(decimales, MaxFractionDigits)
            If plus > 0 Then chiffres &= "." & New String("0"c, decimales) & New String("#"c, plus - decimales)
            Return Prefix & nombre.Value.ToString(chiffres, Culture()) & Suffix
        End Function

        ''' <summary>Le nombre qu'un texte écrit (préfixe, suffixe et séparateurs des milliers compris) ; <c>Nothing</c> s'il n'en écrit pas.</summary>
        Public Function Parse(texte As String) As Double?
            If texte Is Nothing Then Return Nothing
            Dim brut = texte
            If Not String.IsNullOrEmpty(Prefix) AndAlso brut.StartsWith(Prefix, StringComparison.Ordinal) Then brut = brut.Substring(Prefix.Length)
            If Not String.IsNullOrEmpty(Suffix) AndAlso brut.EndsWith(Suffix, StringComparison.Ordinal) Then brut = brut.Substring(0, brut.Length - Suffix.Length)
            Dim format = Culture().NumberFormat
            ' Le séparateur des milliers français est une espace insécable : on ôte toutes les espaces.
            brut = brut.Replace(format.NumberGroupSeparator, "").Replace(ChrW(&HA0), "").Replace(ChrW(&H202F), "").Replace(" ", "").Trim()
            If brut.Length = 0 Then Return Nothing
            Dim nombre As Double
            If Double.TryParse(brut, NumberStyles.Float, format, nombre) Then Return nombre
            Return Nothing
        End Function

        ''' <summary>Augmente le nombre d'un pas (ou le diminue, d'un pas négatif), dans ses bornes.</summary>
        Public Sub Spin(pas As Double)
            If IsReadOnly OrElse Not IsEnabled Then Return
            Value = Borner(If(Value, 0) + pas)
            Ecrire()
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _champ IsNot Nothing Then
                RemoveHandler _champ.TextChanged, AddressOf ChampModifie
                RemoveHandler _champ.PreviewTextInput, AddressOf ChampSaisi
                RemoveHandler _champ.PreviewKeyDown, AddressOf ToucheDansLeChamp
                RemoveHandler _champ.LostKeyboardFocus, AddressOf ChampQuitte
            End If
            MyBase.OnApplyTemplate()
            _champ = TryCast(GetTemplateChild(PartTextBox), TextBox)
            If _champ IsNot Nothing Then
                AddHandler _champ.TextChanged, AddressOf ChampModifie
                AddHandler _champ.PreviewTextInput, AddressOf ChampSaisi
                AddHandler _champ.PreviewKeyDown, AddressOf ToucheDansLeChamp
                AddHandler _champ.LostKeyboardFocus, AddressOf ChampQuitte
                DataObject.AddPastingHandler(_champ, AddressOf ChampColle)
            End If
            Dim plus = TryCast(GetTemplateChild(PartIncrement), ButtonBase)
            If plus IsNot Nothing Then AddHandler plus.Click, Sub() Spin([Step])
            Dim moins = TryCast(GetTemplateChild(PartDecrement), ButtonBase)
            If moins IsNot Nothing Then AddHandler moins.Click, Sub() Spin(-[Step])
            Ecrire()
        End Sub

        ' La culture de la langue de l'élément ; celle de Windows elle-même (ses réglages personnalisés compris : un point
        ' décimal choisi en français) quand la langue est celle de Windows, comme le DatePicker de WPF.
        Private Function Culture() As CultureInfo
            If String.Equals(Language?.IetfLanguageTag, CultureInfo.CurrentCulture.IetfLanguageTag, StringComparison.OrdinalIgnoreCase) Then
                Return CultureInfo.CurrentCulture
            End If
            Try
                Return Language.GetEquivalentCulture()
            Catch ex As InvalidOperationException
                Return CultureInfo.CurrentCulture
            End Try
        End Function

        Private Function Borner(nombre As Double) As Double
            ' Arrondi comme l'affichage (au-dessus pour une moitié), et non au pair : la valeur suit ce qu'on lit.
            Dim arrondi = Math.Round(nombre, Math.Max(0, Math.Min(15, Math.Max(MinFractionDigits, MaxFractionDigits))), MidpointRounding.AwayFromZero)
            Return Math.Max(Minimum, Math.Min(Maximum, arrondi))
        End Function

        ' Le texte du champ, celui du nombre ; sans réécrire ce que l'utilisateur est en train de taper.
        Private Sub Ecrire()
            If _champ Is Nothing Then Return
            ' Le curseur garde sa place, comptée depuis la fin : une flèche ou un bouton ne le renvoie pas au début.
            Dim depuisLaFin = If(_champ.IsKeyboardFocused, _champ.Text.Length - _champ.CaretIndex, -1)
            _ecritParLeCode = True
            Try
                _champ.Text = Format(Value)
            Finally
                _ecritParLeCode = False
            End Try
            If depuisLaFin >= 0 Then _champ.CaretIndex = Math.Max(0, _champ.Text.Length - depuisLaFin)
        End Sub

        Private Sub ChampModifie(sender As Object, e As TextChangedEventArgs)
            If _ecritParLeCode Then Return
            Dim nombre = Parse(_champ.Text)
            If nombre.HasValue OrElse _champ.Text.Trim().Length = 0 Then
                _ecritParLeCode = True
                Try
                    Value = nombre
                Finally
                    _ecritParLeCode = False
                End Try
            End If
        End Sub

        ' Que des chiffres, le séparateur décimal s'il y a des décimales, le moins s'il y a des nombres négatifs.
        Private Sub ChampSaisi(sender As Object, e As TextCompositionEventArgs)
            Dim culture = Me.Culture()
            Dim format = culture.NumberFormat
            ' En français, le point du pavé numérique vaut la virgule (l'isDecimalSign de mkm) : il l'écrit.
            If MaxFractionDigits > 0 AndAlso culture.TwoLetterISOLanguageName = "fr" AndAlso (e.Text = "." OrElse e.Text = ",") AndAlso
               e.Text <> format.NumberDecimalSeparator Then
                e.Handled = True
                Dim debut = _champ.SelectionStart
                _champ.SelectedText = format.NumberDecimalSeparator
                _champ.Select(debut + format.NumberDecimalSeparator.Length, 0)
                Return
            End If
            For Each c In e.Text
                Dim admis = Char.IsDigit(c) OrElse
                    (MaxFractionDigits > 0 AndAlso format.NumberDecimalSeparator.Contains(c)) OrElse
                    (Minimum < 0 AndAlso format.NegativeSign.Contains(c)) OrElse
                    (UseGrouping AndAlso format.NumberGroupSeparator.Contains(c))
                If Not admis Then
                    e.Handled = True
                    Return
                End If
            Next
        End Sub

        Private Sub ChampColle(sender As Object, e As DataObjectPastingEventArgs)
            Dim texte = TryCast(e.DataObject.GetData(DataFormats.UnicodeText), String)
            If texte Is Nothing OrElse Not Parse(texte).HasValue Then e.CancelCommand()
        End Sub

        Private Sub ToucheDansLeChamp(sender As Object, e As KeyEventArgs)
            Select Case e.Key
                Case Input.Key.Up
                    Spin([Step])
                    e.Handled = True
                Case Input.Key.Down
                    Spin(-[Step])
                    e.Handled = True
                Case Input.Key.Enter
                    Valider()
            End Select
        End Sub

        Private Sub ChampQuitte(sender As Object, e As KeyboardFocusChangedEventArgs)
            Valider()
        End Sub

        ' En quittant le champ (ou par Entrée), le nombre se borne et s'écrit au format.
        Private Sub Valider()
            If Value.HasValue Then Value = Borner(Value.Value)
            Ecrire()
        End Sub

        Private Shared Sub OnValueChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim saisie = DirectCast(d, InputNumber)
            If Not saisie._ecritParLeCode Then saisie.Ecrire()
        End Sub

        Private Shared Sub OnFormatChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, InputNumber).Ecrire()
        End Sub

    End Class

End Namespace
