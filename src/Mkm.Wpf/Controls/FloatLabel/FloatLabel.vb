Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>Où se pose le libellé d'un <see cref="FloatLabel"/> quand il flotte.</summary>
    Public Enum FloatLabelVariant
        ''' <summary>Au-dessus du champ (par défaut).</summary>
        Over
        ''' <summary>Dans le champ, en haut ; le champ s'agrandit pour lui.</summary>
        [In]
        ''' <summary>Sur la bordure du haut du champ.</summary>
        [On]
    End Enum

    ''' <summary>
    ''' Le libellé flottant de mkm (<c>FloatLabel</c>) : posé dans le champ vide comme un texte d'aide, il s'envole,
    ''' plus petit, quand le champ prend le clavier ou une valeur.
    ''' <code>
    ''' &lt;mkm:FloatLabel Label="Référence"&gt;
    '''     &lt;mkm:InputText /&gt;
    ''' &lt;/mkm:FloatLabel&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Le champ est le contenu : InputText, Textarea, Password, InputNumber, Select, MultiSelect, TreeSelect,
    ''' CascadeSelect, AutoComplete, DatePicker. Il est rempli quand il a un texte, un choix ou une valeur ; un champ qui
    ''' a un texte d'aide (<c>Placeholder</c>) garde le libellé envolé. Le libellé nomme le champ pour les lecteurs
    ''' d'écran.
    ''' </remarks>
    <TemplatePart(Name:="PART_Label", Type:=GetType(FrameworkElement))>
    Public Class FloatLabel
        Inherits ContentControl

        ''' <summary>Le libellé.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Label), GetType(Object), GetType(FloatLabel), New FrameworkPropertyMetadata(Nothing, AddressOf OnLabelChanged))

        ''' <summary>Où se pose le libellé envolé : au-dessus (par défaut), dans le champ, sur sa bordure.</summary>
        Public Shared ReadOnly VariantProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([Variant]), GetType(FloatLabelVariant), GetType(FloatLabel),
            New FrameworkPropertyMetadata(FloatLabelVariant.Over, AddressOf OnVariantChanged, AddressOf ContraindreLaVariante))

        Private Shared ReadOnly IsFloatingPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFloating), GetType(Boolean), GetType(FloatLabel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le libellé est envolé.</summary>
        Public Shared ReadOnly IsFloatingProperty As DependencyProperty = IsFloatingPropertyKey.DependencyProperty

        Private Shared ReadOnly IsFilledPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFilled), GetType(Boolean), GetType(FloatLabel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le champ a une valeur.</summary>
        Public Shared ReadOnly IsFilledProperty As DependencyProperty = IsFilledPropertyKey.DependencyProperty

        Private Shared ReadOnly IsInvalidPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsInvalid), GetType(Boolean), GetType(FloatLabel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le champ est refusé (sa propriété <c>Invalid</c>).</summary>
        Public Shared ReadOnly IsInvalidProperty As DependencyProperty = IsInvalidPropertyKey.DependencyProperty

        ' Les propriétés qu'on suit sur le champ, quand il les a.
        Private Shared ReadOnly Suivies As String() = {"Text", "SelectedItem", "SelectedDate", "HasSelection", "HasPassword", "HasValues", "Value", "Placeholder", "Invalid"}

        Private _libelle As FrameworkElement
        Private _champ As DependencyObject
        Private ReadOnly _suivies As New List(Of DependencyPropertyDescriptor)

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(FloatLabel), New FrameworkPropertyMetadata(GetType(FloatLabel)))
            FocusableProperty.OverrideMetadata(GetType(FloatLabel), New FrameworkPropertyMetadata(False))
        End Sub

        Public Sub New()
            ' Ce que saisissent les champs qui ne disent leur valeur que par un évènement.
            [AddHandler](PasswordBox.PasswordChangedEvent, New RoutedEventHandler(Sub() Evaluer()), True)
            [AddHandler](Password.PasswordChangedEvent, New RoutedEventHandler(Sub() Evaluer()), True)
            [AddHandler](Selector.SelectionChangedEvent, New SelectionChangedEventHandler(Sub() Evaluer()), True)
        End Sub

        ''' <summary>Le libellé.</summary>
        Public Property Label As Object
            Get
                Return GetValue(LabelProperty)
            End Get
            Set(value As Object)
                SetValue(LabelProperty, value)
            End Set
        End Property

        ''' <summary>Où se pose le libellé envolé.</summary>
        Public Property [Variant] As FloatLabelVariant
            Get
                Return DirectCast(GetValue(VariantProperty), FloatLabelVariant)
            End Get
            Set(value As FloatLabelVariant)
                SetValue(VariantProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le libellé est envolé.</summary>
        Public ReadOnly Property IsFloating As Boolean
            Get
                Return CBool(GetValue(IsFloatingProperty))
            End Get
        End Property

        ''' <summary>Vrai quand le champ a une valeur.</summary>
        Public ReadOnly Property IsFilled As Boolean
            Get
                Return CBool(GetValue(IsFilledProperty))
            End Get
        End Property

        ''' <summary>Vrai quand le champ est refusé.</summary>
        Public ReadOnly Property IsInvalid As Boolean
            Get
                Return CBool(GetValue(IsInvalidProperty))
            End Get
        End Property

        ''' <summary>Vrai pour un libellé toujours envolé, dans le champ (IftaLabel).</summary>
        Friend Overridable ReadOnly Property ToujoursEnvole As Boolean
            Get
                Return False
            End Get
        End Property

        Private Shared Function ContraindreLaVariante(d As DependencyObject, valeur As Object) As Object
            Return If(DirectCast(d, FloatLabel).ToujoursEnvole, FloatLabelVariant.In, valeur)
        End Function

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _libelle = TryCast(GetTemplateChild("PART_Label"), FrameworkElement)
            Nommer()
            Evaluer()
            Decaler()
        End Sub

        ' Le champ suivi : le contenu, ou celui d'un IconField (le champ et son icône).
        Private Function ChampSuivi() As DependencyObject
            Dim iconField = TryCast(Content, IconField)
            If iconField IsNot Nothing Then Return TryCast(iconField.Content, DependencyObject)
            Return TryCast(Content, DependencyObject)
        End Function

        ' Après l'icône d'un IconField de tête, le libellé commence où commence le texte, comme mkm.
        Private Sub Decaler()
            Dim decalage = TryCast(GetTemplateChild("Decalage"), FrameworkElement)
            If decalage Is Nothing Then Return
            Dim iconField = TryCast(Content, IconField)
            If iconField Is Nothing OrElse iconField.Icon Is Nothing OrElse iconField.IconPosition <> IconFieldPosition.Left Then
                decalage.ClearValue(MarginProperty)
                Return
            End If
            Dim retrait = TryFindResource("mkm.iconfield.start.padding")
            Dim marge = TryFindResource("mkm.floatlabel.label.margin")
            If TypeOf retrait Is Thickness AndAlso TypeOf marge Is Thickness Then
                decalage.Margin = New Thickness(Math.Max(0, DirectCast(retrait, Thickness).Left - DirectCast(marge, Thickness).Left), 0, 0, 0)
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnContentChanged(ancien As Object, nouveau As Object)
            MyBase.OnContentChanged(ancien, nouveau)
            Suivre(ChampSuivi())
            Agrandir()
            Nommer()
            Evaluer()
            Decaler()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnIsKeyboardFocusWithinChanged(e As DependencyPropertyChangedEventArgs)
            MyBase.OnIsKeyboardFocusWithinChanged(e)
            Evaluer()
        End Sub

        Private Shared Sub OnVariantChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, FloatLabel).Agrandir()
        End Sub

        ' Suit les propriétés du champ qui disent s'il est rempli, s'il a un texte d'aide, s'il est refusé.
        Private Sub Suivre(champ As DependencyObject)
            For Each suivie In _suivies
                If _champ IsNot Nothing Then suivie.RemoveValueChanged(_champ, AddressOf Champ_Change)
            Next
            _suivies.Clear()
            _champ = champ
            If champ Is Nothing Then Return
            For Each nom In Suivies
                Dim descripteur = DependencyPropertyDescriptor.FromName(nom, champ.GetType(), champ.GetType())
                If descripteur Is Nothing Then Continue For
                descripteur.AddValueChanged(champ, AddressOf Champ_Change)
                _suivies.Add(descripteur)
            Next
        End Sub

        Private Sub Champ_Change(sender As Object, e As EventArgs)
            Evaluer()
        End Sub

        Private Function Lire(nom As String) As Object
            If _champ Is Nothing Then Return Nothing
            Dim descripteur = DependencyPropertyDescriptor.FromName(nom, _champ.GetType(), _champ.GetType())
            Return descripteur?.GetValue(_champ)
        End Function

        ''' <summary>Vrai quand le champ a un texte, un choix ou une valeur.</summary>
        Friend Function EstRempli() As Boolean
            Dim motDePasse = TryCast(_champ, PasswordBox)
            If motDePasse IsNot Nothing Then Return motDePasse.Password.Length > 0
            For Each nom In {"HasSelection", "HasPassword", "HasValues"}
                Dim valeur = Lire(nom)
                If valeur IsNot Nothing Then Return CBool(valeur)
            Next
            Dim texte = TryCast(_champ, TextBox)
            If texte IsNot Nothing Then Return texte.Text.Length > 0
            Dim choix = TryCast(_champ, Selector)
            If choix IsNot Nothing Then Return choix.SelectedItem IsNot Nothing
            Dim date_ = TryCast(_champ, System.Windows.Controls.DatePicker)
            If date_ IsNot Nothing Then Return date_.SelectedDate.HasValue OrElse Not String.IsNullOrEmpty(date_.Text)
            Dim valeurDuChamp = Lire("Value")
            If valeurDuChamp IsNot Nothing Then Return Not (TypeOf valeurDuChamp Is String AndAlso CStr(valeurDuChamp).Length = 0)
            Dim texteDuChamp = TryCast(Lire("Text"), String)
            Return Not String.IsNullOrEmpty(texteDuChamp)
        End Function

        Private Sub Evaluer()
            Dim rempli = EstRempli()
            Dim aide = TryCast(Lire("Placeholder"), String)
            Dim refuse = Lire("Invalid")
            SetValue(IsFilledPropertyKey, rempli)
            SetValue(IsInvalidPropertyKey, refuse IsNot Nothing AndAlso CBool(refuse))
            SetValue(IsFloatingPropertyKey, ToujoursEnvole OrElse rempli OrElse IsKeyboardFocusWithin OrElse Not String.IsNullOrEmpty(aide))
        End Sub

        ' Dans le champ (In), le champ s'agrandit : son texte descend sous le libellé. Seul ce que le libellé a posé est
        ' retiré ensuite : une hauteur ou un retrait de l'application restent.
        Private _agrandi As Control

        Private Sub Agrandir()
            Dim champ = TryCast(ChampSuivi(), Control)
            If _agrandi IsNot Nothing AndAlso (champ IsNot _agrandi OrElse [Variant] <> FloatLabelVariant.In) Then
                _agrandi.ClearValue(PaddingProperty)
                _agrandi.ClearValue(HeightProperty)
                _agrandi = Nothing
            End If
            If champ Is Nothing OrElse [Variant] <> FloatLabelVariant.In OrElse _agrandi Is champ Then Return
            champ.SetResourceReference(PaddingProperty, "mkm.floatlabel.in.input.all.padding")
            champ.Height = Double.NaN
            _agrandi = champ
        End Sub

        Private Shared Sub OnLabelChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, FloatLabel).Nommer()
        End Sub

        ' Le libellé nomme le champ pour les lecteurs d'écran, comme le « for » d'un label : le champ prend son texte pour
        ' nom, sauf un nom qu'on lui a donné.
        Private _nomPose As String

        Private Sub Nommer()
            Dim champ = TryCast(ChampSuivi(), UIElement)
            If champ Is Nothing Then Return
            Dim texte = TryCast(Label, String)
            Dim actuel = AutomationProperties.GetName(champ)
            If String.IsNullOrEmpty(actuel) OrElse actuel = _nomPose Then
                AutomationProperties.SetName(champ, If(texte, String.Empty))
                _nomPose = texte
            End If
        End Sub

    End Class

    ''' <summary>
    ''' Le libellé dans le champ de mkm (<c>IftaLabel</c>) : toujours en haut, dans le champ, plus petit ; le texte saisi
    ''' dessous.
    ''' <code>
    ''' &lt;mkm:IftaLabel Label="Référence"&gt;
    '''     &lt;mkm:InputText /&gt;
    ''' &lt;/mkm:IftaLabel&gt;
    ''' </code>
    ''' </summary>
    Public Class IftaLabel
        Inherits FloatLabel

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(IftaLabel), New FrameworkPropertyMetadata(GetType(IftaLabel)))
        End Sub

        Public Sub New()
            CoerceValue(VariantProperty)
        End Sub

        Friend Overrides ReadOnly Property ToujoursEnvole As Boolean
            Get
                Return True
            End Get
        End Property

    End Class

End Namespace
