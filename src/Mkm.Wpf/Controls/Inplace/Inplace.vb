Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' L'édition sur place de mkm (Inplace) : un texte (<c>Display</c>) qui, cliqué, laisse place à son contenu (un champ
    ''' pour le modifier). <c>IsActive</c> dit lequel est montré ; Échap ou <c>Deactivate()</c> rend le texte.
    ''' <code>
    ''' &lt;mkm:Inplace Display="HS-00042"&gt;&lt;mkm:InputText Text="HS-00042" /&gt;&lt;/mkm:Inplace&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Display", Type:=GetType(ButtonBase))>
    Public Class Inplace
        Inherits ContentControl

        ''' <summary>Ce qui est montré tant qu'on n'édite pas : un texte, une icône.</summary>
        Public Shared ReadOnly DisplayProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Display), GetType(Object), GetType(Inplace), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai quand le contenu est montré. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsActiveProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsActive), GetType(Boolean), GetType(Inplace),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsActiveChanged))

        Private _affichage As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Inplace), New FrameworkPropertyMetadata(GetType(Inplace)))
            FocusableProperty.OverrideMetadata(GetType(Inplace), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Ce qui est montré tant qu'on n'édite pas.</summary>
        Public Property Display As Object
            Get
                Return GetValue(DisplayProperty)
            End Get
            Set(value As Object)
                SetValue(DisplayProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le contenu est montré.</summary>
        Public Property IsActive As Boolean
            Get
                Return CBool(GetValue(IsActiveProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsActiveProperty, value)
            End Set
        End Property

        ''' <summary>Montre le contenu.</summary>
        Public Sub Activate()
            SetCurrentValue(IsActiveProperty, True)
        End Sub

        ''' <summary>Rend le texte.</summary>
        Public Sub Deactivate()
            SetCurrentValue(IsActiveProperty, False)
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _affichage IsNot Nothing Then RemoveHandler _affichage.Click, AddressOf Affichage_Click
            MyBase.OnApplyTemplate()
            _affichage = TryCast(GetTemplateChild("PART_Display"), ButtonBase)
            If _affichage IsNot Nothing Then AddHandler _affichage.Click, AddressOf Affichage_Click
        End Sub

        Private Sub Affichage_Click(sender As Object, e As RoutedEventArgs)
            Activate()
        End Sub

        ' Montré, le contenu prend le clavier ; rendu, le texte le reprend, s'il était dans le contenu.
        Private Shared Sub OnIsActiveChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim place = DirectCast(d, Inplace)
            Dim actif = CBool(e.NewValue)
            Dim avaitLeClavier = place.IsKeyboardFocusWithin
            place.Dispatcher.BeginInvoke(Threading.DispatcherPriority.Loaded, New System.Action(
                Sub()
                    If actif Then
                        TryCast(place.Content, UIElement)?.Focus()
                        If Not place.IsKeyboardFocusWithin Then TryCast(place.Content, UIElement)?.MoveFocus(New TraversalRequest(FocusNavigationDirection.First))
                    ElseIf avaitLeClavier Then
                        place._affichage?.Focus()
                    End If
                End Sub))
        End Sub

        ''' <summary>Échap, dans le contenu, rend le texte.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse e.Key <> Key.Escape OrElse Not IsActive Then Return
            Deactivate()
            e.Handled = True
        End Sub

    End Class

End Namespace
