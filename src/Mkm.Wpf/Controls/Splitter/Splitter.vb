Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Markup

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le séparateur de panneaux de mkm (<c>Splitter</c>) : des panneaux (<see cref="SplitterPanel"/>) côte à côte ou
    ''' l'un sous l'autre, séparés d'une gouttière grise de 4 px qu'on tire pour partager la place ; au survol, au
    ''' clavier ou pendant qu'on la tire, une pilule grise paraît au milieu. Les flèches la déplacent par pas
    ''' (<see cref="[Step]"/>, en pourcentage).
    ''' <code>
    ''' &lt;mkm:Splitter&gt;
    '''     &lt;mkm:SplitterPanel Size="30" MinSize="20"&gt;…&lt;/mkm:SplitterPanel&gt;
    '''     &lt;mkm:SplitterPanel Size="70"&gt;…&lt;/mkm:SplitterPanel&gt;
    ''' &lt;/mkm:Splitter&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' La taille d'un panneau (<see cref="SplitterPanel.Size"/>) est un pourcentage de la place que laissent les
    ''' gouttières ; sans taille, les panneaux se partagent ce qui reste à parts égales. Tirer une gouttière change la
    ''' taille des deux panneaux qu'elle sépare, sans passer sous leur taille minimale.
    ''' </remarks>
    <ContentProperty("Panels")>
    <TemplatePart(Name:="PART_Layout", Type:=GetType(SplitterLayout))>
    Public Class Splitter
        Inherits Control

        ''' <summary>Le sens : côte à côte (<c>Horizontal</c>, par défaut) ou l'un sous l'autre.</summary>
        Public Shared ReadOnly OrientationProperty As DependencyProperty = StackPanel.OrientationProperty.AddOwner(
            GetType(Splitter), New FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure))

        ''' <summary>L'épaisseur des gouttières, en pixels (4 par défaut).</summary>
        Public Shared ReadOnly GutterSizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(GutterSize), GetType(Double), GetType(Splitter), New FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsMeasure))

        ''' <summary>Le pas d'une flèche du clavier sur une gouttière, en pourcentage (5 par défaut).</summary>
        Public Shared ReadOnly StepProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([Step]), GetType(Double), GetType(Splitter), New FrameworkPropertyMetadata(5.0))

        Private ReadOnly _panneaux As New ObservableCollection(Of SplitterPanel)
        Private _disposition As SplitterLayout

        ''' <summary>Après qu'une gouttière a été tirée (à la souris ou au clavier) : les tailles des panneaux ont changé.</summary>
        Public Event ResizeEnd As EventHandler

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Splitter), New FrameworkPropertyMetadata(GetType(Splitter)))
            FocusableProperty.OverrideMetadata(GetType(Splitter), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un séparateur sans panneau.</summary>
        Public Sub New()
            AddHandler _panneaux.CollectionChanged, AddressOf OnPanneauxChanged
        End Sub

        ''' <summary>Les panneaux, dans l'ordre.</summary>
        Public ReadOnly Property Panels As Collection(Of SplitterPanel)
            Get
                Return _panneaux
            End Get
        End Property

        ''' <summary>Le sens : côte à côte ou l'un sous l'autre.</summary>
        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        ''' <summary>L'épaisseur des gouttières, en pixels.</summary>
        Public Property GutterSize As Double
            Get
                Return CDbl(GetValue(GutterSizeProperty))
            End Get
            Set(value As Double)
                SetValue(GutterSizeProperty, value)
            End Set
        End Property

        ''' <summary>Le pas d'une flèche du clavier sur une gouttière, en pourcentage.</summary>
        Public Property [Step] As Double
            Get
                Return CDbl(GetValue(StepProperty))
            End Get
            Set(value As Double)
                SetValue(StepProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property LogicalChildren As IEnumerator
            Get
                Return _panneaux.GetEnumerator()
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            _disposition?.Detacher()
            MyBase.OnApplyTemplate()
            _disposition = TryCast(GetTemplateChild("PART_Layout"), SplitterLayout)
            _disposition?.Attacher(Me)
        End Sub

        ''' <summary>
        ''' La taille de chaque panneau, en pourcentage de la place que laissent les gouttières : celle qu'il a, ou une part
        ''' égale de ce que laissent les autres. La somme fait 100.
        ''' </summary>
        Public Function GetSizes() As Double()
            Dim tailles = _panneaux.Select(Function(p) p.Size).ToArray()
            Dim donnees = tailles.Where(Function(t) Not Double.IsNaN(t)).Sum()
            Dim libres = tailles.Count(Function(t) Double.IsNaN(t))
            Dim part = If(libres = 0, 0.0, Math.Max(0.0, 100 - donnees) / libres)
            Dim resultat = tailles.Select(Function(t) If(Double.IsNaN(t), part, t)).ToArray()
            Dim somme = resultat.Sum()
            If somme > 0 Then resultat = resultat.Select(Function(t) t * 100 / somme).ToArray()
            Return resultat
        End Function

        ''' <summary>
        ''' Déplace la gouttière qui suit le panneau <paramref name="index"/> de <paramref name="pourcentage"/> (positif :
        ''' vers la fin) ; les deux panneaux qu'elle sépare gardent leur taille minimale.
        ''' </summary>
        Friend Sub Deplacer(index As Integer, pourcentage As Double)
            If index < 0 OrElse index >= _panneaux.Count - 1 Then Return
            Dim tailles = GetSizes()
            Dim avant = _panneaux(index)
            Dim apres = _panneaux(index + 1)
            Dim paire = tailles(index) + tailles(index + 1)
            Dim nouvelle = tailles(index) + pourcentage
            nouvelle = Math.Max(nouvelle, avant.MinSize)
            nouvelle = Math.Min(nouvelle, paire - apres.MinSize)
            If nouvelle < avant.MinSize Then Return
            ' Toutes les tailles deviennent explicites : le partage ne bouge plus quand un panneau change.
            For i = 0 To _panneaux.Count - 1
                _panneaux(i).Size = tailles(i)
            Next
            avant.Size = nouvelle
            apres.Size = paire - nouvelle
        End Sub

        Friend Sub FinirLeDeplacement()
            RaiseEvent ResizeEnd(Me, EventArgs.Empty)
        End Sub

        Private Sub OnPanneauxChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            If e.OldItems IsNot Nothing Then
                For Each panneau As SplitterPanel In e.OldItems
                    RemoveLogicalChild(panneau)
                Next
            End If
            If e.NewItems IsNot Nothing Then
                For Each panneau As SplitterPanel In e.NewItems
                    AddLogicalChild(panneau)
                Next
            End If
            _disposition?.Reconstruire()
        End Sub

    End Class

    ''' <summary>Un panneau d'un <see cref="Splitter"/> : son contenu, sa taille et sa taille minimale, en pourcentage.</summary>
    Public Class SplitterPanel
        Inherits ContentControl

        ''' <summary>La taille, en pourcentage ; sans elle (<c>NaN</c>), une part égale de ce que laissent les autres.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(Double), GetType(SplitterPanel),
            New FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsParentMeasure Or FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>La taille minimale, en pourcentage (0 par défaut).</summary>
        Public Shared ReadOnly MinSizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MinSize), GetType(Double), GetType(SplitterPanel), New FrameworkPropertyMetadata(0.0))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(SplitterPanel), New FrameworkPropertyMetadata(GetType(SplitterPanel)))
            FocusableProperty.OverrideMetadata(GetType(SplitterPanel), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>La taille, en pourcentage ; sans elle, une part égale de ce que laissent les autres.</summary>
        Public Property Size As Double
            Get
                Return CDbl(GetValue(SizeProperty))
            End Get
            Set(value As Double)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>La taille minimale, en pourcentage.</summary>
        Public Property MinSize As Double
            Get
                Return CDbl(GetValue(MinSizeProperty))
            End Get
            Set(value As Double)
                SetValue(MinSizeProperty, value)
            End Set
        End Property

    End Class

    ''' <summary>
    ''' Une gouttière d'un <see cref="Splitter"/> : on la tire à la souris ; au clavier, les flèches la déplacent d'un pas,
    ''' Début et Fin jusqu'aux tailles minimales. Sa pilule paraît au survol, au clavier ou pendant qu'on la tire.
    ''' </summary>
    Public Class SplitterGutter
        Inherits Thumb

        ''' <summary>Le sens du séparateur : une gouttière d'un séparateur horizontal est verticale.</summary>
        Public Shared ReadOnly OrientationProperty As DependencyProperty = StackPanel.OrientationProperty.AddOwner(
            GetType(SplitterGutter), New FrameworkPropertyMetadata(Orientation.Horizontal))

        Private Shared ReadOnly IsFocusVisiblePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFocusVisible), GetType(Boolean), GetType(SplitterGutter), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand la gouttière a le focus du clavier, et non celui d'un clic.</summary>
        Public Shared ReadOnly IsFocusVisibleProperty As DependencyProperty = IsFocusVisiblePropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(SplitterGutter), New FrameworkPropertyMetadata(GetType(SplitterGutter)))
            FocusableProperty.OverrideMetadata(GetType(SplitterGutter), New FrameworkPropertyMetadata(True))
        End Sub

        ''' <summary>Le sens du séparateur.</summary>
        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        ''' <summary>Le séparateur de la gouttière.</summary>
        Friend Property Proprietaire As Splitter

        ''' <summary>Le panneau qui précède la gouttière.</summary>
        Friend Property Index As Integer

        ''' <summary>Vrai quand la gouttière a le focus du clavier, et non celui d'un clic.</summary>
        Public ReadOnly Property IsFocusVisible As Boolean
            Get
                Return CBool(GetValue(IsFocusVisibleProperty))
            End Get
        End Property

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

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            Dim s = Proprietaire
            If s Is Nothing OrElse e.Handled Then Return
            Dim horizontal = s.Orientation = Orientation.Horizontal
            Dim avance = If(horizontal, Key.Right, Key.Down)
            Dim recule = If(horizontal, Key.Left, Key.Up)
            Select Case e.Key
                Case avance
                    s.Deplacer(Index, s.Step)
                Case recule
                    s.Deplacer(Index, -s.Step)
                Case Key.Home
                    s.Deplacer(Index, -100)
                Case Key.End
                    s.Deplacer(Index, 100)
                Case Else
                    Return
            End Select
            e.Handled = True
            s.FinirLeDeplacement()
        End Sub

    End Class

End Namespace
