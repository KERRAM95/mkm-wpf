Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' L'accordéon de mkm (Accordion) : des sections l'une sous l'autre, chacune un en-tête en gras qui déplie son
    ''' contenu ; une seule ouverte à la fois, sauf <c>Multiple</c>.
    ''' <code>
    ''' &lt;mkm:Accordion&gt;
    '''     &lt;mkm:AccordionPanel Header="Cartouche" IsExpanded="True"&gt;…&lt;/mkm:AccordionPanel&gt;
    '''     &lt;mkm:AccordionPanel Header="Notes"&gt;…&lt;/mkm:AccordionPanel&gt;
    ''' &lt;/mkm:Accordion&gt;
    ''' </code>
    ''' </summary>
    Public Class Accordion
        Inherits ItemsControl

        ''' <summary>Vrai quand plusieurs sections peuvent être ouvertes à la fois.</summary>
        Public Shared ReadOnly MultipleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Multiple), GetType(Boolean), GetType(Accordion), New FrameworkPropertyMetadata(False))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Accordion), New FrameworkPropertyMetadata(GetType(Accordion)))
            FocusableProperty.OverrideMetadata(GetType(Accordion), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand plusieurs sections peuvent être ouvertes à la fois.</summary>
        Public Property Multiple As Boolean
            Get
                Return CBool(GetValue(MultipleProperty))
            End Get
            Set(value As Boolean)
                SetValue(MultipleProperty, value)
            End Set
        End Property

        ''' <summary>Un groupe pour les lecteurs d'écran : ses sections et leurs en-têtes.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is AccordionPanel
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New AccordionPanel()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Marquer()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Marquer()
        End Sub

        ' La première et la dernière section : les coins arrondis de l'accordéon.
        Private Sub Marquer()
            Dim sections = LesSections()
            For i = 0 To sections.Count - 1
                sections(i).Placer(i = 0, i = sections.Count - 1)
            Next
            ' Sans Multiple, une seule ouverte : la première dite ouverte (IsExpanded posé avant d'entrer dans l'accordéon).
            If Multiple Then Return
            For Each autre In sections.Where(Function(s) s.IsExpanded).Skip(1)
                autre.SetCurrentValue(AccordionPanel.IsExpandedProperty, False)
            Next
        End Sub

        Private Function LesSections() As Collections.Generic.List(Of AccordionPanel)
            Return Items.Cast(Of Object)().Select(Function(o) TryCast(If(TryCast(o, AccordionPanel), ItemContainerGenerator.ContainerFromItem(o)), AccordionPanel)).
                Where(Function(s) s IsNot Nothing).ToList()
        End Function

        ' Une seule section ouverte, sauf Multiple : celle qui s'ouvre referme les autres.
        Friend Sub SectionOuverte(section As AccordionPanel)
            If Multiple Then Return
            For Each autre In LesSections()
                If autre IsNot section Then autre.SetCurrentValue(AccordionPanel.IsExpandedProperty, False)
            Next
        End Sub

        ''' <summary>Les flèches, Début et Fin passent d'un en-tête à l'autre, comme mkm.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled Then Return
            Dim sections = LesSections().Where(Function(s) s.IsEnabled AndAlso s.IsVisible).ToList()
            Dim ici = sections.FindIndex(Function(s) s.EnTeteAuClavier)
            If ici < 0 OrElse sections.Count = 0 Then Return
            Dim cible As Integer
            Select Case e.Key
                Case Key.Down : cible = (ici + 1) Mod sections.Count
                Case Key.Up : cible = (ici - 1 + sections.Count) Mod sections.Count
                Case Key.Home : cible = 0
                Case Key.End : cible = sections.Count - 1
                Case Else : Return
            End Select
            sections(cible).DonnerLeClavier()
            e.Handled = True
        End Sub

    End Class

    ''' <summary>Une section d'un <see cref="Accordion"/> : son en-tête (<c>Header</c>) et son contenu.</summary>
    <TemplatePart(Name:="PART_Header", Type:=GetType(ButtonBase))>
    Public Class AccordionPanel
        Inherits HeaderedContentControl

        ''' <summary>Vrai quand la section est ouverte. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly IsExpandedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsExpanded), GetType(Boolean), GetType(AccordionPanel),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnIsExpandedChanged))

        Private Shared ReadOnly IsFirstPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFirst), GetType(Boolean), GetType(AccordionPanel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour la première section : les coins du haut de l'accordéon.</summary>
        Public Shared ReadOnly IsFirstProperty As DependencyProperty = IsFirstPropertyKey.DependencyProperty

        Private Shared ReadOnly IsLastPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsLast), GetType(Boolean), GetType(AccordionPanel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai pour la dernière section : les coins du bas de l'accordéon.</summary>
        Public Shared ReadOnly IsLastProperty As DependencyProperty = IsLastPropertyKey.DependencyProperty

        Private _entete As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(AccordionPanel), New FrameworkPropertyMetadata(GetType(AccordionPanel)))
            FocusableProperty.OverrideMetadata(GetType(AccordionPanel), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand la section est ouverte.</summary>
        Public Property IsExpanded As Boolean
            Get
                Return CBool(GetValue(IsExpandedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsExpandedProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour la première section.</summary>
        Public ReadOnly Property IsFirst As Boolean
            Get
                Return CBool(GetValue(IsFirstProperty))
            End Get
        End Property

        ''' <summary>Vrai pour la dernière section.</summary>
        Public ReadOnly Property IsLast As Boolean
            Get
                Return CBool(GetValue(IsLastProperty))
            End Get
        End Property

        Friend Sub Placer(premiere As Boolean, derniere As Boolean)
            SetValue(IsFirstPropertyKey, premiere)
            SetValue(IsLastPropertyKey, derniere)
        End Sub

        Friend ReadOnly Property EnTeteAuClavier As Boolean
            Get
                Return _entete IsNot Nothing AndAlso _entete.IsKeyboardFocused
            End Get
        End Property

        Friend Sub DonnerLeClavier()
            _entete?.Focus()
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _entete = TryCast(GetTemplateChild("PART_Header"), ButtonBase)
        End Sub

        Private Shared Sub OnIsExpandedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim section = DirectCast(d, AccordionPanel)
            If CBool(e.NewValue) Then TryCast(ItemsControl.ItemsControlFromItemContainer(section), Accordion)?.SectionOuverte(section)
        End Sub

        ''' <summary>Un groupe nommé par son en-tête pour les lecteurs d'écran ; son en-tête se dit développé ou réduit.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group, Function() TryCast(Header, String))
        End Function

    End Class

    ''' <summary>
    ''' L'en-tête d'une section d'accordéon, partie de son gabarit : enfoncé quand la section est ouverte. Les lecteurs
    ''' d'écran le disent « développé » ou « réduit » (ExpandCollapse), comme <c>aria-expanded</c> dans mkm.
    ''' </summary>
    Friend NotInheritable Class EnTeteDAccordeon
        Inherits System.Windows.Controls.Primitives.ToggleButton

        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDEnTete(Me)
        End Function

        Protected Overrides Sub OnChecked(e As RoutedEventArgs)
            MyBase.OnChecked(e)
            Annoncer(ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded)
        End Sub

        Protected Overrides Sub OnUnchecked(e As RoutedEventArgs)
            MyBase.OnUnchecked(e)
            Annoncer(ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
        End Sub

        Private Sub Annoncer(avant As ExpandCollapseState, apres As ExpandCollapseState)
            If AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged) Then
                UIElementAutomationPeer.FromElement(Me)?.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, avant, apres)
            End If
        End Sub

        Private NotInheritable Class PairDEnTete
            Inherits ButtonBaseAutomationPeer
            Implements IExpandCollapseProvider

            Public Sub New(entete As EnTeteDAccordeon)
                MyBase.New(entete)
            End Sub

            Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
                Return AutomationControlType.Button
            End Function

            Protected Overrides Function GetClassNameCore() As String
                Return "AccordionHeader"
            End Function

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.ExpandCollapse Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Private ReadOnly Property EnTete As EnTeteDAccordeon
                Get
                    Return DirectCast(Owner, EnTeteDAccordeon)
                End Get
            End Property

            Public ReadOnly Property ExpandCollapseState As ExpandCollapseState Implements IExpandCollapseProvider.ExpandCollapseState
                Get
                    Return If(EnTete.IsChecked.GetValueOrDefault(), ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed)
                End Get
            End Property

            Public Sub Expand() Implements IExpandCollapseProvider.Expand
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                EnTete.IsChecked = True
            End Sub

            Public Sub Collapse() Implements IExpandCollapseProvider.Collapse
                If Not IsEnabled() Then Throw New ElementNotEnabledException()
                EnTete.IsChecked = False
            End Sub

        End Class

    End Class

End Namespace
