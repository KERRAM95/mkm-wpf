Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Controls

    ''' <summary>La gravité d'un <see cref="Button"/>, comme dans mkm : sa couleur.</summary>
    Public Enum ButtonSeverity
        ''' <summary>Le bouton principal, à la couleur principale du thème.</summary>
        Primary
        ''' <summary>Le bouton secondaire, gris.</summary>
        Secondary
        Success
        Info
        Warn
        Help
        Danger
        Contrast
    End Enum

    ''' <summary>La forme d'un <see cref="Button"/>, comme dans mkm : plein, à contour, en texte, en lien.</summary>
    Public Enum ButtonVariant
        Filled
        Outlined
        Text
        Link
    End Enum

    ''' <summary>
    ''' Le bouton de mkm, au rendu de 3DEXPERIENCE : 24 px de haut, 65 px de large au moins avec un
    ''' libellé, coins de 4 px, 8 px entre l'icône et le libellé. <see cref="Severity"/> donne sa couleur,
    ''' <see cref="[Variant]"/> sa forme ; un bouton d'icône seule (sans <c>Content</c>) fait 24 px de côté,
    ''' et en texte c'est le bouton « lite » de 3DEXPERIENCE : sans fond ni bordure.
    ''' <code>&lt;mkm:Button Content="Confirmer" Icon="{x:Static mkm:Icons.Check}" Command="{Binding Confirmer}" /&gt;</code>
    ''' </summary>
    ''' <remarks>
    ''' « Button » est aussi le nom du bouton de WPF : en VB, qualifier (<c>Controls.Button</c>) là où les
    ''' deux espaces de noms sont importés. « Variant » est un mot réservé de VB : <c>[Variant]</c>.
    ''' </remarks>
    Public Class Button
        Inherits System.Windows.Controls.Button

        ' Les couleurs et les mesures de la gravité ; une valeur posée par l'application l'emporte.
        Private ReadOnly _habillage As New Habillage(Me)

        ''' <summary>L'icône, avant le libellé : une géométrie de <see cref="Icons"/> ou une image.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(Button), New FrameworkPropertyMetadata(Nothing, AddressOf OnFormeChanged))

        ''' <summary>La gravité : la couleur du bouton.</summary>
        Public Shared ReadOnly SeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Severity), GetType(ButtonSeverity), GetType(Button), New FrameworkPropertyMetadata(ButtonSeverity.Primary, AddressOf OnCouleurChanged))

        ''' <summary>La forme : plein, à contour, en texte, en lien.</summary>
        Public Shared ReadOnly VariantProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([Variant]), GetType(ButtonVariant), GetType(Button), New FrameworkPropertyMetadata(ButtonVariant.Filled, AddressOf OnCouleurChanged))

        ''' <summary>
        ''' Les coins du bouton : ceux du thème (button.border.radius) par défaut. Un bouton accolé à un autre (SplitButton)
        ''' a ses coins du côté de l'autre carrés.
        ''' </summary>
        Public Shared ReadOnly CornerRadiusProperty As DependencyProperty = System.Windows.Controls.Border.CornerRadiusProperty.AddOwner(GetType(Button))

        ''' <summary>Le fond au survol (posé par la gravité et la forme).</summary>
        Public Shared ReadOnly HoverBackgroundProperty As DependencyProperty = Pinceau(NameOf(HoverBackground))
        ''' <summary>La bordure au survol.</summary>
        Public Shared ReadOnly HoverBorderBrushProperty As DependencyProperty = Pinceau(NameOf(HoverBorderBrush))
        ''' <summary>Le texte au survol.</summary>
        Public Shared ReadOnly HoverForegroundProperty As DependencyProperty = Pinceau(NameOf(HoverForeground))
        ''' <summary>Le fond enfoncé.</summary>
        Public Shared ReadOnly PressedBackgroundProperty As DependencyProperty = Pinceau(NameOf(PressedBackground))
        ''' <summary>La bordure enfoncée.</summary>
        Public Shared ReadOnly PressedBorderBrushProperty As DependencyProperty = Pinceau(NameOf(PressedBorderBrush))
        ''' <summary>Le texte enfoncé.</summary>
        Public Shared ReadOnly PressedForegroundProperty As DependencyProperty = Pinceau(NameOf(PressedForeground))
        ''' <summary>Le fond au focus du clavier.</summary>
        Public Shared ReadOnly FocusBackgroundProperty As DependencyProperty = Pinceau(NameOf(FocusBackground))
        ''' <summary>La bordure au focus du clavier.</summary>
        Public Shared ReadOnly FocusBorderBrushProperty As DependencyProperty = Pinceau(NameOf(FocusBorderBrush))
        ''' <summary>L'anneau au focus du clavier.</summary>
        Public Shared ReadOnly FocusRingBrushProperty As DependencyProperty = Pinceau(NameOf(FocusRingBrush))

        Private Shared ReadOnly IsFocusVisiblePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFocusVisible), GetType(Boolean), GetType(Button), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le bouton a le focus du clavier (focus-visible), pas d'un clic.</summary>
        Public Shared ReadOnly IsFocusVisibleProperty As DependencyProperty = IsFocusVisiblePropertyKey.DependencyProperty

        Private Shared ReadOnly HasIconPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasIcon), GetType(Boolean), GetType(Button), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le bouton a une icône.</summary>
        Public Shared ReadOnly HasIconProperty As DependencyProperty = HasIconPropertyKey.DependencyProperty

        Private Shared ReadOnly IsIconOnlyPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsIconOnly), GetType(Boolean), GetType(Button), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le bouton n'a qu'une icône, sans libellé.</summary>
        Public Shared ReadOnly IsIconOnlyProperty As DependencyProperty = IsIconOnlyPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Button), New FrameworkPropertyMetadata(GetType(Button)))
        End Sub

        ''' <summary>Un bouton principal plein.</summary>
        Public Sub New()
            Colorer()
        End Sub

        ''' <summary>Son info-bulle le nomme pour les lecteurs d'écran quand il n'a pas de libellé (icône seule).</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeBouton(Me)
        End Function

        Private Shared Function Pinceau(nom As String) As DependencyProperty
            Return DependencyProperty.Register(nom, GetType(Brush), GetType(Button), New FrameworkPropertyMetadata(Nothing))
        End Function

        ''' <summary>Les coins du bouton.</summary>
        Public Property CornerRadius As CornerRadius
            Get
                Return DirectCast(GetValue(CornerRadiusProperty), CornerRadius)
            End Get
            Set(value As CornerRadius)
                SetValue(CornerRadiusProperty, value)
            End Set
        End Property

        ''' <summary>L'icône.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>La gravité.</summary>
        Public Property Severity As ButtonSeverity
            Get
                Return DirectCast(GetValue(SeverityProperty), ButtonSeverity)
            End Get
            Set(value As ButtonSeverity)
                SetValue(SeverityProperty, value)
            End Set
        End Property

        ''' <summary>La forme.</summary>
        Public Property [Variant] As ButtonVariant
            Get
                Return DirectCast(GetValue(VariantProperty), ButtonVariant)
            End Get
            Set(value As ButtonVariant)
                SetValue(VariantProperty, value)
            End Set
        End Property

        ''' <summary>Le fond au survol.</summary>
        Public ReadOnly Property HoverBackground As Brush
            Get
                Return DirectCast(GetValue(HoverBackgroundProperty), Brush)
            End Get
        End Property

        ''' <summary>La bordure au survol.</summary>
        Public ReadOnly Property HoverBorderBrush As Brush
            Get
                Return DirectCast(GetValue(HoverBorderBrushProperty), Brush)
            End Get
        End Property

        ''' <summary>Le texte au survol.</summary>
        Public ReadOnly Property HoverForeground As Brush
            Get
                Return DirectCast(GetValue(HoverForegroundProperty), Brush)
            End Get
        End Property

        ''' <summary>Le fond enfoncé.</summary>
        Public ReadOnly Property PressedBackground As Brush
            Get
                Return DirectCast(GetValue(PressedBackgroundProperty), Brush)
            End Get
        End Property

        ''' <summary>La bordure enfoncée.</summary>
        Public ReadOnly Property PressedBorderBrush As Brush
            Get
                Return DirectCast(GetValue(PressedBorderBrushProperty), Brush)
            End Get
        End Property

        ''' <summary>Le texte enfoncé.</summary>
        Public ReadOnly Property PressedForeground As Brush
            Get
                Return DirectCast(GetValue(PressedForegroundProperty), Brush)
            End Get
        End Property

        ''' <summary>Le fond au focus du clavier.</summary>
        Public ReadOnly Property FocusBackground As Brush
            Get
                Return DirectCast(GetValue(FocusBackgroundProperty), Brush)
            End Get
        End Property

        ''' <summary>La bordure au focus du clavier.</summary>
        Public ReadOnly Property FocusBorderBrush As Brush
            Get
                Return DirectCast(GetValue(FocusBorderBrushProperty), Brush)
            End Get
        End Property

        ''' <summary>L'anneau au focus du clavier, de la couleur de la gravité.</summary>
        Public ReadOnly Property FocusRingBrush As Brush
            Get
                Return DirectCast(GetValue(FocusRingBrushProperty), Brush)
            End Get
        End Property

        ''' <summary>Vrai quand le bouton a le focus du clavier.</summary>
        Public ReadOnly Property IsFocusVisible As Boolean
            Get
                Return CBool(GetValue(IsFocusVisibleProperty))
            End Get
        End Property

        ''' <summary>Vrai quand le bouton a une icône.</summary>
        Public ReadOnly Property HasIcon As Boolean
            Get
                Return CBool(GetValue(HasIconProperty))
            End Get
        End Property

        ''' <summary>Vrai quand le bouton n'a qu'une icône.</summary>
        Public ReadOnly Property IsIconOnly As Boolean
            Get
                Return CBool(GetValue(IsIconOnlyProperty))
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Sub OnContentChanged(oldContent As Object, newContent As Object)
            MyBase.OnContentChanged(oldContent, newContent)
            MettreAJourLaForme()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnGotKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnGotKeyboardFocus(e)
            SetValue(IsFocusVisiblePropertyKey, InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnLostKeyboardFocus(e)
            SetValue(IsFocusVisiblePropertyKey, False)
        End Sub

        Private Shared Sub OnFormeChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Button).MettreAJourLaForme()
        End Sub

        Private Shared Sub OnCouleurChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Button).Colorer()
        End Sub

        Private Sub MettreAJourLaForme()
            Dim avecIcone = Icon IsNot Nothing
            SetValue(HasIconPropertyKey, avecIcone)
            SetValue(IsIconOnlyPropertyKey, avecIcone AndAlso (Content Is Nothing OrElse TypeOf Content Is String AndAlso CStr(Content).Length = 0))
        End Sub

        ' Les couleurs de chaque état, prises aux jetons de mkm selon la gravité et la forme.
        Private Sub Colorer()
            Dim g = Severity.ToString().ToLowerInvariant()
            Lier(FocusRingBrushProperty, $"button.{g}.focus.ring.color")
            Select Case [Variant]
                Case ButtonVariant.Filled
                    Lier(BackgroundProperty, $"button.{g}.background")
                    Lier(BorderBrushProperty, $"button.{g}.border.color")
                    Lier(ForegroundProperty, $"button.{g}.color")
                    Lier(HoverBackgroundProperty, $"button.{g}.hover.background")
                    Lier(HoverBorderBrushProperty, $"button.{g}.hover.border.color")
                    Lier(HoverForegroundProperty, $"button.{g}.hover.color")
                    If Severity = ButtonSeverity.Primary Then
                        ' Le principal de 3DEXPERIENCE : ses bleus au clavier et enfoncé (Tokens/<thème>.css.tokens).
                        Lier(PressedBackgroundProperty, "button.primary.pressed.background")
                        Lier(PressedBorderBrushProperty, "button.primary.pressed.border.color")
                        Lier(FocusBackgroundProperty, "button.primary.focus.background")
                        Lier(FocusBorderBrushProperty, "button.primary.focus.border.color")
                    Else
                        Lier(PressedBackgroundProperty, $"button.{g}.active.background")
                        Lier(PressedBorderBrushProperty, $"button.{g}.active.border.color")
                        Lier(FocusBackgroundProperty, $"button.{g}.hover.background")
                        Lier(FocusBorderBrushProperty, $"button.{g}.hover.border.color")
                    End If
                    Lier(PressedForegroundProperty, $"button.{g}.active.color")

                Case ButtonVariant.Outlined
                    _habillage.Poser(BackgroundProperty, Brushes.Transparent)
                    Lier(BorderBrushProperty, $"button.outlined.{g}.border.color")
                    Lier(ForegroundProperty, $"button.outlined.{g}.color")
                    Lier(HoverBackgroundProperty, $"button.outlined.{g}.hover.background")
                    Lier(HoverBorderBrushProperty, $"button.outlined.{g}.border.color")
                    Lier(HoverForegroundProperty, $"button.outlined.{g}.color")
                    Lier(PressedBackgroundProperty, $"button.outlined.{g}.active.background")
                    Lier(PressedBorderBrushProperty, $"button.outlined.{g}.border.color")
                    Lier(PressedForegroundProperty, $"button.outlined.{g}.color")
                    Lier(FocusBackgroundProperty, $"button.outlined.{g}.hover.background")
                    Lier(FocusBorderBrushProperty, $"button.outlined.{g}.border.color")

                Case ButtonVariant.Text
                    _habillage.Poser(BackgroundProperty, Brushes.Transparent)
                    _habillage.Poser(BorderBrushProperty, Brushes.Transparent)
                    Lier(ForegroundProperty, $"button.text.{g}.color")
                    Lier(HoverBackgroundProperty, $"button.text.{g}.hover.background")
                    _habillage.Poser(HoverBorderBrushProperty, Brushes.Transparent)
                    Lier(HoverForegroundProperty, $"button.text.{g}.color")
                    Lier(PressedBackgroundProperty, $"button.text.{g}.active.background")
                    _habillage.Poser(PressedBorderBrushProperty, Brushes.Transparent)
                    Lier(PressedForegroundProperty, $"button.text.{g}.color")
                    Lier(FocusBackgroundProperty, $"button.text.{g}.hover.background")
                    _habillage.Poser(FocusBorderBrushProperty, Brushes.Transparent)

                Case ButtonVariant.Link
                    _habillage.Poser(BackgroundProperty, Brushes.Transparent)
                    _habillage.Poser(BorderBrushProperty, Brushes.Transparent)
                    Lier(ForegroundProperty, "button.link.color")
                    _habillage.Poser(HoverBackgroundProperty, Brushes.Transparent)
                    _habillage.Poser(HoverBorderBrushProperty, Brushes.Transparent)
                    Lier(HoverForegroundProperty, "button.link.hover.color")
                    _habillage.Poser(PressedBackgroundProperty, Brushes.Transparent)
                    _habillage.Poser(PressedBorderBrushProperty, Brushes.Transparent)
                    Lier(PressedForegroundProperty, "button.link.active.color")
                    _habillage.Poser(FocusBackgroundProperty, Brushes.Transparent)
                    _habillage.Poser(FocusBorderBrushProperty, Brushes.Transparent)
            End Select
        End Sub

        Private Sub Lier(propriete As DependencyProperty, jeton As String)
            _habillage.Lier(propriete, jeton)
        End Sub

    End Class

End Namespace
