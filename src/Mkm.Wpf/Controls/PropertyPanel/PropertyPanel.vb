Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>Où le libellé d'une propriété se place.</summary>
    Public Enum PropertyPanelLayout
        ''' <summary>Au-dessus de la valeur (par défaut).</summary>
        Stacked
        ''' <summary>Devant la valeur, sur 40 % de la largeur.</summary>
        Inline
    End Enum

    ''' <summary>
    ''' La fiche de propriétés de mkm (<c>PropertyPanel</c>) : les propriétés d'un objet, par sections repliables
    ''' (<see cref="PropertySection"/>), une par ligne (<see cref="PropertyRow"/>) ; en lecture, leurs valeurs ; en
    ''' modification (<see cref="Editing"/>), leurs éditeurs, la marque des propriétés obligatoires et les messages.
    ''' <see cref="Editable"/> lui donne ses boutons : Modifier, puis Enregistrer et Annuler.
    ''' <code>
    ''' &lt;mkm:PropertyPanel Editable="True" Save="Enregistrer"&gt;
    '''     &lt;mkm:PropertySection Header="Cartouche"&gt;
    '''         &lt;mkm:PropertyRow Label="Référence" Required="True" Value="{Binding Reference}"&gt;
    '''             &lt;mkm:InputText Text="{Binding Reference}" /&gt;
    '''         &lt;/mkm:PropertyRow&gt;
    '''     &lt;/mkm:PropertySection&gt;
    ''' &lt;/mkm:PropertyPanel&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Le brouillon de mkm (<c>modelValue</c>) est ici la liaison des éditeurs à l'objet : à <see cref="Save"/>,
    ''' l'application enregistre ; à <see cref="Cancel"/>, elle revient aux valeurs enregistrées.
    ''' </remarks>
    <TemplatePart(Name:=PropertyPanel.PartEditButton, Type:=GetType(ButtonBase))>
    <TemplatePart(Name:=PropertyPanel.PartSaveButton, Type:=GetType(ButtonBase))>
    <TemplatePart(Name:=PropertyPanel.PartCancelButton, Type:=GetType(ButtonBase))>
    Public Class PropertyPanel
        Inherits ItemsControl

        ''' <summary>Tout son arbre visuel pour les lecteurs d'écran et l'automatisation, gabarit compris.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeConteneur(Me, System.Windows.Automation.Peers.AutomationControlType.Group)
        End Function

        ''' <summary>Le bouton Modifier dans le gabarit.</summary>
        Public Const PartEditButton As String = "PART_EditButton"
        ''' <summary>Le bouton Enregistrer dans le gabarit.</summary>
        Public Const PartSaveButton As String = "PART_SaveButton"
        ''' <summary>Le bouton Annuler dans le gabarit.</summary>
        Public Const PartCancelButton As String = "PART_CancelButton"

        ''' <summary>Vrai quand les propriétés montrent leurs éditeurs ; les sections et les lignes en héritent.</summary>
        Public Shared ReadOnly EditingProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Editing", GetType(Boolean), GetType(PropertyPanel),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits Or FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        ''' <summary>Où se place le libellé de chaque propriété ; les lignes en héritent.</summary>
        Public Shared ReadOnly LayoutProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Layout", GetType(PropertyPanelLayout), GetType(PropertyPanel),
            New FrameworkPropertyMetadata(PropertyPanelLayout.Stacked, FrameworkPropertyMetadataOptions.Inherits))

        ''' <summary>Vrai si la fiche montre ses boutons Modifier, Enregistrer et Annuler.</summary>
        Public Shared ReadOnly EditableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Editable), GetType(Boolean), GetType(PropertyPanel), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai pendant un enregistrement : les boutons sont grisés.</summary>
        Public Shared ReadOnly SavingProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Saving), GetType(Boolean), GetType(PropertyPanel), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai (par défaut) si le bouton Enregistrer est actif.</summary>
        Public Shared ReadOnly CanSaveProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(CanSave), GetType(Boolean), GetType(PropertyPanel), New FrameworkPropertyMetadata(True))

        ''' <summary>Ce que l'en-tête montre à gauche de ses boutons (un titre…).</summary>
        Public Shared ReadOnly HeaderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Header), GetType(Object), GetType(PropertyPanel), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Ce que la fiche montre sous ses sections.</summary>
        Public Shared ReadOnly FooterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Footer), GetType(Object), GetType(PropertyPanel), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Levé par le bouton Modifier, la fiche passée en modification.</summary>
        Public Shared ReadOnly EditEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            "Edit", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(PropertyPanel))

        ''' <summary>Levé par le bouton Enregistrer : l'application enregistre, puis remet <see cref="Editing"/> à faux.</summary>
        Public Shared ReadOnly SaveEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            "Save", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(PropertyPanel))

        ''' <summary>Levé par le bouton Annuler, la fiche revenue en lecture.</summary>
        Public Shared ReadOnly CancelEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            "Cancel", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(PropertyPanel))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(PropertyPanel), New FrameworkPropertyMetadata(GetType(PropertyPanel)))
            FocusableProperty.OverrideMetadata(GetType(PropertyPanel), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Vrai quand les propriétés montrent leurs éditeurs.</summary>
        Public Property Editing As Boolean
            Get
                Return CBool(GetValue(EditingProperty))
            End Get
            Set(value As Boolean)
                SetValue(EditingProperty, value)
            End Set
        End Property

        ''' <summary>Où se place le libellé de chaque propriété.</summary>
        Public Property Layout As PropertyPanelLayout
            Get
                Return DirectCast(GetValue(LayoutProperty), PropertyPanelLayout)
            End Get
            Set(value As PropertyPanelLayout)
                SetValue(LayoutProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si la fiche montre ses boutons Modifier, Enregistrer et Annuler.</summary>
        Public Property Editable As Boolean
            Get
                Return CBool(GetValue(EditableProperty))
            End Get
            Set(value As Boolean)
                SetValue(EditableProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pendant un enregistrement.</summary>
        Public Property Saving As Boolean
            Get
                Return CBool(GetValue(SavingProperty))
            End Get
            Set(value As Boolean)
                SetValue(SavingProperty, value)
            End Set
        End Property

        ''' <summary>Vrai (par défaut) si le bouton Enregistrer est actif.</summary>
        Public Property CanSave As Boolean
            Get
                Return CBool(GetValue(CanSaveProperty))
            End Get
            Set(value As Boolean)
                SetValue(CanSaveProperty, value)
            End Set
        End Property

        ''' <summary>Ce que l'en-tête montre à gauche de ses boutons.</summary>
        Public Property Header As Object
            Get
                Return GetValue(HeaderProperty)
            End Get
            Set(value As Object)
                SetValue(HeaderProperty, value)
            End Set
        End Property

        ''' <summary>Ce que la fiche montre sous ses sections.</summary>
        Public Property Footer As Object
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As Object)
                SetValue(FooterProperty, value)
            End Set
        End Property

        ''' <summary>Levé par le bouton Modifier.</summary>
        Public Custom Event Edit As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                Me.AddHandler(EditEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                Me.RemoveHandler(EditEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Levé par le bouton Enregistrer.</summary>
        Public Custom Event Save As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                Me.AddHandler(SaveEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                Me.RemoveHandler(SaveEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Levé par le bouton Annuler.</summary>
        Public Custom Event Cancel As RoutedEventHandler
            AddHandler(value As RoutedEventHandler)
                Me.AddHandler(CancelEvent, value)
            End AddHandler
            RemoveHandler(value As RoutedEventHandler)
                Me.RemoveHandler(CancelEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As RoutedEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Si la fiche est en modification, sous <paramref name="element"/>.</summary>
        Public Shared Function GetEditing(element As DependencyObject) As Boolean
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return CBool(element.GetValue(EditingProperty))
        End Function

        ''' <summary>Met en modification, ou en lecture, ce qui est sous <paramref name="element"/>.</summary>
        Public Shared Sub SetEditing(element As DependencyObject, value As Boolean)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(EditingProperty, value)
        End Sub

        ''' <summary>La place des libellés sous <paramref name="element"/>.</summary>
        Public Shared Function GetLayout(element As DependencyObject) As PropertyPanelLayout
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return DirectCast(element.GetValue(LayoutProperty), PropertyPanelLayout)
        End Function

        ''' <summary>Pose la place des libellés sous <paramref name="element"/>.</summary>
        Public Shared Sub SetLayout(element As DependencyObject, value As PropertyPanelLayout)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(LayoutProperty, value)
        End Sub

        ''' <summary>Ouvre toutes les sections.</summary>
        Public Sub ExpandAll()
            For Each section In Items.OfType(Of PropertySection)()
                section.IsExpanded = True
            Next
        End Sub

        ''' <summary>Ferme toutes les sections.</summary>
        Public Sub CollapseAll()
            For Each section In Items.OfType(Of PropertySection)()
                section.IsExpanded = False
            Next
        End Sub

        ''' <summary>Les flèches haut et bas, Début et Fin passent d'un en-tête de section à l'autre, comme mkm.</summary>
        Protected Overrides Sub OnKeyDown(e As System.Windows.Input.KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled Then Return
            Dim entetes As New System.Collections.Generic.List(Of UIElement)
            For i = 0 To Items.Count - 1
                Dim section = TryCast(If(TryCast(Items(i), PropertySection), ItemContainerGenerator.ContainerFromIndex(i)), PropertySection)
                Dim entete = TryCast(section?.Template?.FindName("Entete", section), UIElement)
                If entete IsNot Nothing AndAlso entete.IsVisible AndAlso entete.IsEnabled Then entetes.Add(entete)
            Next
            Dim ici = entetes.FindIndex(Function(t) t.IsKeyboardFocused)
            If ici < 0 Then Return
            Dim cible As Integer
            Select Case e.Key
                Case System.Windows.Input.Key.Down : cible = (ici + 1) Mod entetes.Count
                Case System.Windows.Input.Key.Up : cible = (ici - 1 + entetes.Count) Mod entetes.Count
                Case System.Windows.Input.Key.Home : cible = 0
                Case System.Windows.Input.Key.End : cible = entetes.Count - 1
                Case Else : Return
            End Select
            entetes(cible).Focus()
            e.Handled = True
        End Sub

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            Brancher(PartEditButton, Sub()
                                         Editing = True
                                         Me.RaiseEvent(New RoutedEventArgs(EditEvent, Me))
                                     End Sub)
            Brancher(PartSaveButton, Sub() Me.RaiseEvent(New RoutedEventArgs(SaveEvent, Me)))
            Brancher(PartCancelButton, Sub()
                                           Editing = False
                                           Me.RaiseEvent(New RoutedEventArgs(CancelEvent, Me))
                                       End Sub)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is PropertySection
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New PropertySection()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Collections.Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            ' Comme chez mkm, la première section s'ouvre d'elle-même, si aucune ne l'est.
            Dim sections = Items.OfType(Of PropertySection)().ToList()
            If sections.Count > 0 AndAlso Not sections.Any(Function(s) s.IsExpanded OrElse s.ReadLocalValue(PropertySection.IsExpandedProperty) IsNot DependencyProperty.UnsetValue) Then
                sections(0).IsExpanded = True
            End If
        End Sub

        Private Sub Brancher(nom As String, action As Action)
            Dim bouton = TryCast(GetTemplateChild(nom), ButtonBase)
            If bouton IsNot Nothing Then AddHandler bouton.Click, Sub() action()
        End Sub

    End Class

End Namespace
