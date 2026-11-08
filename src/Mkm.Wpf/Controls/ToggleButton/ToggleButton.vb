Imports System.Windows
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>La taille d'un <see cref="ToggleButton"/>, comme dans mkm.</summary>
    Public Enum ToggleButtonSize
        ''' <summary>4 px sur 12 de retrait, un libellé de 12 px (par défaut).</summary>
        Normal
        ''' <summary>2 px sur 8 de retrait.</summary>
        Small
        ''' <summary>6 px sur 16 de retrait, un libellé de 14 px.</summary>
        Large
    End Enum

    ''' <summary>
    ''' Le bouton à bascule de mkm, à la mesure du bouton de 3DEXPERIENCE : 24 px de haut, des coins de 4 px, un libellé
    ''' de 12 px en graisse normale, une icône grise à 6 px de lui. Au repos blanc, bordé du gris du bouton secondaire ;
    ''' survolé, gris clair ; enfoncé, la sélection de 3DEXPERIENCE (le fond bleu pâle, le texte foncé) bordée de la
    ''' couleur principale ; au clavier, l'anneau du focus ; grisé, à moitié transparent.
    ''' <code>
    ''' &lt;mkm:ToggleButton OnLabel="Verrouillé" OffLabel="Déverrouillé"
    '''                   OnIcon="{x:Static mkm:Icons.Locked}" OffIcon="{x:Static mkm:Icons.Unlocked}"
    '''                   IsChecked="{Binding Verrouille}" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' C'est un <see cref="System.Windows.Controls.Primitives.ToggleButton"/> : <c>IsChecked</c>, <c>Command</c>, le
    ''' clavier. Son libellé et son icône suivent son état (<see cref="OnLabel"/>, <see cref="OffLabel"/>,
    ''' <see cref="OnIcon"/>, <see cref="OffIcon"/>) ; un <c>Content</c> donné les remplace, comme le contenu de mkm.
    ''' « ToggleButton » est aussi le nom de celui de WPF (<c>System.Windows.Controls.Primitives</c>) : qualifier là où
    ''' les deux espaces de noms sont importés.
    ''' </remarks>
    Public Class ToggleButton
        Inherits System.Windows.Controls.Primitives.ToggleButton

        ''' <summary>Le libellé du bouton enfoncé.</summary>
        Public Shared ReadOnly OnLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(OnLabel), GetType(String), GetType(ToggleButton), New FrameworkPropertyMetadata(Nothing, AddressOf OnApparenceChanged))

        ''' <summary>Le libellé du bouton relâché.</summary>
        Public Shared ReadOnly OffLabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(OffLabel), GetType(String), GetType(ToggleButton), New FrameworkPropertyMetadata(Nothing, AddressOf OnApparenceChanged))

        ''' <summary>L'icône du bouton enfoncé (une icône de <see cref="Icons"/>).</summary>
        Public Shared ReadOnly OnIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(OnIcon), GetType(Object), GetType(ToggleButton), New FrameworkPropertyMetadata(Nothing, AddressOf OnApparenceChanged))

        ''' <summary>L'icône du bouton relâché ; sans elle, celle du bouton enfoncé.</summary>
        Public Shared ReadOnly OffIconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(OffIcon), GetType(Object), GetType(ToggleButton), New FrameworkPropertyMetadata(Nothing, AddressOf OnApparenceChanged))

        ''' <summary>Vrai quand la valeur est refusée : la bordure prend la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = InputText.InvalidProperty.AddOwner(GetType(ToggleButton))

        ''' <summary>La taille du bouton.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(ToggleButtonSize), GetType(ToggleButton), New FrameworkPropertyMetadata(ToggleButtonSize.Normal))

        Private Shared ReadOnly CurrentLabelPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(CurrentLabel), GetType(Object), GetType(ToggleButton), New FrameworkPropertyMetadata(Nothing))
        ''' <summary>Ce que montre le bouton : son contenu s'il en a un, sinon le libellé de son état.</summary>
        Public Shared ReadOnly CurrentLabelProperty As DependencyProperty = CurrentLabelPropertyKey.DependencyProperty

        Private Shared ReadOnly CurrentIconPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(CurrentIcon), GetType(Object), GetType(ToggleButton), New FrameworkPropertyMetadata(Nothing))
        ''' <summary>L'icône de l'état du bouton.</summary>
        Public Shared ReadOnly CurrentIconProperty As DependencyProperty = CurrentIconPropertyKey.DependencyProperty

        Private Shared ReadOnly IsFocusVisiblePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFocusVisible), GetType(Boolean), GetType(ToggleButton), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le bouton a le focus du clavier, et non celui d'un clic : l'anneau du focus paraît.</summary>
        Public Shared ReadOnly IsFocusVisibleProperty As DependencyProperty = IsFocusVisiblePropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ToggleButton), New FrameworkPropertyMetadata(GetType(ToggleButton)))
        End Sub

        ''' <summary>Un bouton relâché, sans libellé.</summary>
        Public Sub New()
            MettreAJour()
        End Sub

        ''' <summary>Son libellé d'état le nomme pour les lecteurs d'écran, sinon son contenu, sinon son info-bulle.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeBascule(Me)
        End Function

        ''' <summary>Le libellé du bouton enfoncé.</summary>
        Public Property OnLabel As String
            Get
                Return CStr(GetValue(OnLabelProperty))
            End Get
            Set(value As String)
                SetValue(OnLabelProperty, value)
            End Set
        End Property

        ''' <summary>Le libellé du bouton relâché.</summary>
        Public Property OffLabel As String
            Get
                Return CStr(GetValue(OffLabelProperty))
            End Get
            Set(value As String)
                SetValue(OffLabelProperty, value)
            End Set
        End Property

        ''' <summary>L'icône du bouton enfoncé.</summary>
        Public Property OnIcon As Object
            Get
                Return GetValue(OnIconProperty)
            End Get
            Set(value As Object)
                SetValue(OnIconProperty, value)
            End Set
        End Property

        ''' <summary>L'icône du bouton relâché ; sans elle, celle du bouton enfoncé.</summary>
        Public Property OffIcon As Object
            Get
                Return GetValue(OffIconProperty)
            End Get
            Set(value As Object)
                SetValue(OffIconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la valeur est refusée.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <summary>La taille du bouton.</summary>
        Public Property Size As ToggleButtonSize
            Get
                Return DirectCast(GetValue(SizeProperty), ToggleButtonSize)
            End Get
            Set(value As ToggleButtonSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>Ce que montre le bouton : son contenu s'il en a un, sinon le libellé de son état.</summary>
        Public ReadOnly Property CurrentLabel As Object
            Get
                Return GetValue(CurrentLabelProperty)
            End Get
        End Property

        ''' <summary>L'icône de l'état du bouton.</summary>
        Public ReadOnly Property CurrentIcon As Object
            Get
                Return GetValue(CurrentIconProperty)
            End Get
        End Property

        ''' <summary>Vrai quand le bouton a le focus du clavier, et non celui d'un clic.</summary>
        Public ReadOnly Property IsFocusVisible As Boolean
            Get
                Return CBool(GetValue(IsFocusVisibleProperty))
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Sub OnContentChanged(oldContent As Object, newContent As Object)
            MyBase.OnContentChanged(oldContent, newContent)
            MettreAJour()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnChecked(e As RoutedEventArgs)
            MyBase.OnChecked(e)
            MettreAJour()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnUnchecked(e As RoutedEventArgs)
            MyBase.OnUnchecked(e)
            MettreAJour()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnIndeterminate(e As RoutedEventArgs)
            MyBase.OnIndeterminate(e)
            MettreAJour()
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

        Private Shared Sub OnApparenceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, ToggleButton).MettreAJour()
        End Sub

        ' Enfoncé, le libellé et l'icône « on » ; relâché ou indéterminé, les « off », l'icône « on » à défaut.
        Private Sub MettreAJour()
            Dim enfonce = IsChecked.GetValueOrDefault()
            Dim avant = TryCast(CurrentLabel, String)
            SetValue(CurrentLabelPropertyKey, If(Content, If(enfonce, OnLabel, OffLabel)))
            SetValue(CurrentIconPropertyKey, If(enfonce, OnIcon, If(OffIcon, OnIcon)))
            ' Le nouveau libellé, dit aux lecteurs d'écran qui suivent le bouton.
            Dim apres = TryCast(CurrentLabel, String)
            If Not String.Equals(avant, apres) AndAlso
               System.Windows.Automation.Peers.AutomationPeer.ListenerExists(System.Windows.Automation.Peers.AutomationEvents.PropertyChanged) Then
                System.Windows.Automation.Peers.UIElementAutomationPeer.FromElement(Me)?.RaisePropertyChangedEvent(
                    System.Windows.Automation.AutomationElementIdentifiers.NameProperty, If(avant, String.Empty), If(apres, String.Empty))
            End If
        End Sub

    End Class

End Namespace
