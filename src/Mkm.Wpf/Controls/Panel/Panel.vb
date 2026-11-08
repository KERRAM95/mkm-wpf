Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le panneau de mkm (Panel) : un bloc bordé, son titre en gras dans un en-tête séparé d'un trait, son contenu, son
    ''' pied ; repliable (<c>Toggleable</c>), un bouton moins / plus le replie et le déplie.
    ''' <code>
    ''' &lt;mkm:Panel Header="Cartouche" Toggleable="True"&gt;
    '''     &lt;TextBlock Text="Référence, désignation, échelle." /&gt;
    ''' &lt;/mkm:Panel&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' « Panel » est aussi le nom de la classe des panneaux de mise en page de WPF : qualifier
    ''' (<c>Mkm.Wpf.Controls.Panel</c>) là où les deux espaces de noms sont importés.
    ''' </remarks>
    <TemplatePart(Name:="PART_Toggle", Type:=GetType(ButtonBase))>
    Public Class Panel
        Inherits HeaderedContentControl

        ''' <summary>Vrai quand le panneau se replie : un bouton moins / plus dans son en-tête.</summary>
        Public Shared ReadOnly ToggleableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Toggleable), GetType(Boolean), GetType(Panel), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai quand le panneau est replié : seul son en-tête reste. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsCollapsed), GetType(Boolean), GetType(Panel),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>Ce qui se pose à droite de l'en-tête : des boutons d'action.</summary>
        Public Shared ReadOnly ActionsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Actions), GetType(Object), GetType(Panel), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le pied du panneau, sous son contenu.</summary>
        Public Shared ReadOnly FooterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Footer), GetType(Object), GetType(Panel), New FrameworkPropertyMetadata(Nothing))

        Private _bascule As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Panel), New FrameworkPropertyMetadata(GetType(Panel)))
            FocusableProperty.OverrideMetadata(GetType(Panel), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand le panneau se replie.</summary>
        Public Property Toggleable As Boolean
            Get
                Return CBool(GetValue(ToggleableProperty))
            End Get
            Set(value As Boolean)
                SetValue(ToggleableProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le panneau est replié.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        ''' <summary>Ce qui se pose à droite de l'en-tête.</summary>
        Public Property Actions As Object
            Get
                Return GetValue(ActionsProperty)
            End Get
            Set(value As Object)
                SetValue(ActionsProperty, value)
            End Set
        End Property

        ''' <summary>Le pied du panneau.</summary>
        Public Property Footer As Object
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As Object)
                SetValue(FooterProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _bascule IsNot Nothing Then RemoveHandler _bascule.Click, AddressOf Bascule_Click
            MyBase.OnApplyTemplate()
            _bascule = TryCast(GetTemplateChild("PART_Toggle"), ButtonBase)
            If _bascule Is Nothing Then Return
            AddHandler _bascule.Click, AddressOf Bascule_Click
            ' Son nom suit l'état et la langue (« Replier », « Déplier ») : lié en code, un déclencheur du gabarit le
            ' laissait vide pour un panneau replié dès le départ.
            Dim nom As New MultiBinding With {.Converter = TexteDuRepli.Instance}
            nom.Bindings.Add(New Binding(NameOf(IsCollapsed)) With {.Source = Me})
            nom.Bindings.Add(New Binding("[aria.expand]") With {.Source = Localizer.Current})
            nom.Bindings.Add(New Binding("[aria.collapse]") With {.Source = Localizer.Current})
            BindingOperations.SetBinding(_bascule, System.Windows.Automation.AutomationProperties.NameProperty, nom)
        End Sub

        Private Sub Bascule_Click(sender As Object, e As RoutedEventArgs)
            SetCurrentValue(IsCollapsedProperty, Not IsCollapsed)
        End Sub

        ''' <summary>Un groupe nommé par son titre pour les lecteurs d'écran ; son bouton dit « Replier » ou « Déplier ».</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group, Function() TryCast(Header, String))
        End Function

    End Class

End Namespace
