Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Threading
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le voile d'attente de mkm (<c>BlockUI</c>) : <see cref="Blocked"/>, un voile sombre couvre son contenu, aux
    ''' coins du contenu, et arrête la souris et le clavier ; <see cref="Overlay"/> se montre au milieu (un chargeur,
    ''' un texte). Posé à la racine d'une fenêtre, il la bloque toute.
    ''' <code>
    ''' &lt;mkm:BlockUI Blocked="{Binding EnCours}"&gt;
    '''     &lt;mkm:BlockUI.Overlay&gt;&lt;mkm:ProgressSpinner Width="40" Height="40" /&gt;&lt;/mkm:BlockUI.Overlay&gt;
    '''     … le contenu …
    ''' &lt;/mkm:BlockUI&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Le voile paraît en fondu (<c>mask.transition.duration</c>). Le contenu garde son apparence : il n'est pas grisé,
    ''' seulement hors d'atteinte. Le clavier y est arrêté aussi, ce que PrimeVue ne fait pas.
    ''' </remarks>
    <TemplatePart(Name:=BlockUI.PartMask, Type:=GetType(FrameworkElement))>
    Public Class BlockUI
        Inherits ContentControl

        ''' <summary>Le nom du voile dans le gabarit.</summary>
        Public Const PartMask As String = "PART_Mask"

        ''' <summary>Vrai quand le contenu est voilé et hors d'atteinte.</summary>
        Public Shared ReadOnly BlockedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Blocked), GetType(Boolean), GetType(BlockUI), New FrameworkPropertyMetadata(False, AddressOf OnBlockedChanged))

        ''' <summary>Ce qui se montre au milieu du voile (rien par défaut).</summary>
        Public Shared ReadOnly OverlayProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Overlay), GetType(Object), GetType(BlockUI), New FrameworkPropertyMetadata(Nothing))

        Private _voile As FrameworkElement
        Private _contenu As FrameworkElement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(BlockUI), New FrameworkPropertyMetadata(GetType(BlockUI)))
            FocusableProperty.OverrideMetadata(GetType(BlockUI), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand le contenu est voilé et hors d'atteinte.</summary>
        Public Property Blocked As Boolean
            Get
                Return CBool(GetValue(BlockedProperty))
            End Get
            Set(value As Boolean)
                SetValue(BlockedProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui se montre au milieu du voile (rien par défaut).</summary>
        Public Property Overlay As Object
            Get
                Return GetValue(OverlayProperty)
            End Get
            Set(value As Object)
                SetValue(OverlayProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _voile = TryCast(GetTemplateChild(PartMask), FrameworkElement)
            _contenu = TryCast(GetTemplateChild("Contenu"), FrameworkElement)
        End Sub

        ' Le clavier n'atteint plus le contenu voilé : ses touches et sa saisie s'arrêtent ici. Celles du voile et de ce qu'il
        ' montre (un bouton Annuler) passent : Tab en sort, Alt+F4 ferme la fenêtre.
        ''' <inheritdoc/>
        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            If Blocked AndAlso DansLeContenu(e.OriginalSource) Then e.Handled = True
            MyBase.OnPreviewKeyDown(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPreviewTextInput(e As TextCompositionEventArgs)
            If Blocked AndAlso DansLeContenu(e.OriginalSource) Then e.Handled = True
            MyBase.OnPreviewTextInput(e)
        End Sub

        Private Function DansLeContenu(source As Object) As Boolean
            Dim element = TryCast(source, DependencyObject)
            While element IsNot Nothing AndAlso element IsNot Me
                If element Is _contenu Then Return True
                element = If(TypeOf element Is Visual, VisualTreeHelper.GetParent(element), LogicalTreeHelper.GetParent(element))
            End While
            Return False
        End Function

        Private Shared Sub OnBlockedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, BlockUI).Voiler(CBool(e.NewValue))
        End Sub

        ' Le voile paraît en fondu ; levé, il disparaît aussitôt.
        Private Sub Voiler(bloque As Boolean)
            If _voile Is Nothing Then Return
            If Not bloque Then
                _voile.BeginAnimation(OpacityProperty, Nothing)
                Return
            End If
            Dim duree = TryFindResource(ThemeResources.Prefix & "mask.transition.duration")
            If TypeOf duree Is Duration Then
                _voile.BeginAnimation(OpacityProperty, New DoubleAnimation(0, 1, DirectCast(duree, Duration)))
            End If
            ' Le focus quitte le contenu voilé, une fois le voile paru (ses déclencheurs passent après ce rappel, et un
            ' élément replié ne prend pas le focus).
            Dispatcher.BeginInvoke(DispatcherPriority.Input, New Action(
                Sub()
                    If Blocked AndAlso DansLeContenu(Keyboard.FocusedElement) Then _voile?.Focus()
                End Sub))
        End Sub

    End Class

End Namespace
