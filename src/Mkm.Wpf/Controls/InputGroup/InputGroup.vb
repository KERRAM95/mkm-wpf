Imports System.ComponentModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Un groupe de saisie de mkm (InputGroup) : des champs, des compléments (<see cref="InputGroupAddon"/>) et des
    ''' boutons accolés sur une ligne, aux coins du groupe aux deux bouts. Les champs se partagent la largeur ; les
    ''' compléments et les boutons gardent la leur.
    ''' <code>
    ''' &lt;mkm:InputGroup&gt;
    '''     &lt;mkm:InputGroupAddon&gt;€&lt;/mkm:InputGroupAddon&gt;
    '''     &lt;mkm:InputText Placeholder="Prix" /&gt;
    '''     &lt;mkm:InputGroupAddon&gt;HT&lt;/mkm:InputGroupAddon&gt;
    ''' &lt;/mkm:InputGroup&gt;
    ''' </code>
    ''' </summary>
    Public Class InputGroup
        Inherits ItemsControl

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(InputGroup), New FrameworkPropertyMetadata(GetType(InputGroup)))
            FocusableProperty.OverrideMetadata(GetType(InputGroup), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c> : ses champs et ses boutons.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            If e.OldItems IsNot Nothing Then
                For Each ancien In e.OldItems.OfType(Of DependencyObject)()
                    If APropreCoins(ancien) Then ancien.ClearValue(Border.CornerRadiusProperty)
                Next
            End If
            Placer()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Placer()
        End Sub

        ' Les coins du groupe aux deux bouts, aucun au milieu ; les bordures des compléments.
        Private Sub Placer()
            Dim elements = Items.Cast(Of Object)().
                Select(Function(o) If(TryCast(o, UIElement), TryCast(ItemContainerGenerator.ContainerFromItem(o), UIElement))).
                Where(Function(el) el IsNot Nothing).ToList()
            For i = 0 To elements.Count - 1
                Dim premier = i = 0
                Dim dernier = i = elements.Count - 1
                Dim complement = TryCast(elements(i), InputGroupAddon)
                If complement IsNot Nothing Then
                    complement.Placer(premier OrElse TypeOf elements(i - 1) Is InputGroupAddon, dernier)
                End If
                If APropreCoins(elements(i)) Then
                    DirectCast(elements(i), FrameworkElement).SetResourceReference(Border.CornerRadiusProperty,
                        If(premier AndAlso dernier, "mkm.inputgroup.addon.border.radius",
                        If(premier, "mkm.inputgroup.first.corner.radius",
                        If(dernier, "mkm.inputgroup.last.corner.radius", "mkm.inputgroup.middle.corner.radius"))))
                End If
            Next
        End Sub

        ' Un champ, un bouton ou un complément de mkm qui ont leurs coins (CornerRadius, celui de Border).
        Private Shared Function APropreCoins(element As DependencyObject) As Boolean
            Return TypeOf element Is FrameworkElement AndAlso
                DependencyPropertyDescriptor.FromProperty(Border.CornerRadiusProperty, element.GetType()) IsNot Nothing
        End Function

    End Class

    ''' <summary>
    ''' Un complément d'un <see cref="InputGroup"/> (InputGroupAddon de mkm) : un texte, une icône, une case, accolés à un
    ''' champ, sur un fond de champ, bordés en haut et en bas.
    ''' </summary>
    Public Class InputGroupAddon
        Inherits ContentControl

        ''' <summary>Les coins : ceux du groupe au premier et au dernier élément, posés par le groupe.</summary>
        Public Shared ReadOnly CornerRadiusProperty As DependencyProperty = Border.CornerRadiusProperty.AddOwner(GetType(InputGroupAddon))

        Private Shared ReadOnly HasStartBorderPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasStartBorder), GetType(Boolean), GetType(InputGroupAddon), New FrameworkPropertyMetadata(True))
        ''' <summary>Vrai quand il est bordé à gauche : le premier du groupe, ou après un autre complément.</summary>
        Public Shared ReadOnly HasStartBorderProperty As DependencyProperty = HasStartBorderPropertyKey.DependencyProperty

        Private Shared ReadOnly HasEndBorderPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasEndBorder), GetType(Boolean), GetType(InputGroupAddon), New FrameworkPropertyMetadata(True))
        ''' <summary>Vrai quand il est bordé à droite : le dernier du groupe.</summary>
        Public Shared ReadOnly HasEndBorderProperty As DependencyProperty = HasEndBorderPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(InputGroupAddon), New FrameworkPropertyMetadata(GetType(InputGroupAddon)))
            FocusableProperty.OverrideMetadata(GetType(InputGroupAddon), New FrameworkPropertyMetadata(False))
            IsTabStopProperty.OverrideMetadata(GetType(InputGroupAddon), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Les coins.</summary>
        Public Property CornerRadius As CornerRadius
            Get
                Return DirectCast(GetValue(CornerRadiusProperty), CornerRadius)
            End Get
            Set(value As CornerRadius)
                SetValue(CornerRadiusProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand il est bordé à gauche.</summary>
        Public ReadOnly Property HasStartBorder As Boolean
            Get
                Return CBool(GetValue(HasStartBorderProperty))
            End Get
        End Property

        ''' <summary>Vrai quand il est bordé à droite.</summary>
        Public ReadOnly Property HasEndBorder As Boolean
            Get
                Return CBool(GetValue(HasEndBorderProperty))
            End Get
        End Property

        Friend Sub Placer(aGauche As Boolean, aDroite As Boolean)
            SetValue(HasStartBorderPropertyKey, aGauche)
            SetValue(HasEndBorderPropertyKey, aDroite)
        End Sub

    End Class

    ''' <summary>
    ''' La ligne d'un <see cref="InputGroup"/> (flex chez mkm) : les compléments et les boutons à leur largeur, les champs se
    ''' partagent le reste ; tous à la hauteur de la ligne.
    ''' </summary>
    Friend NotInheritable Class RangeeDuGroupe
        Inherits System.Windows.Controls.Panel

        Private Shared Function EstUnChamp(element As UIElement) As Boolean
            Dim contenu = If(TryCast(TryCast(element, ContentPresenter)?.Content, UIElement), element)
            Return Not (TypeOf contenu Is InputGroupAddon OrElse TypeOf contenu Is ButtonBase)
        End Function

        ' La hauteur de la ligne est celle des champs (24 px pour celui de 3DEXPERIENCE) : les compléments et les boutons
        ' la prennent (align-items: stretch chez mkm) ; sans champ, celle du plus haut.
        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            Dim fixe = 0.0
            Dim hauteur = 0.0
            Dim hauteurDesChamps = 0.0
            Dim champs = 0
            For Each enfant As UIElement In InternalChildren
                If EstUnChamp(enfant) Then
                    champs += 1
                    Continue For
                End If
                enfant.Measure(New Size(Double.PositiveInfinity, disponible.Height))
                fixe += enfant.DesiredSize.Width
                hauteur = Math.Max(hauteur, enfant.DesiredSize.Height)
            Next
            Dim part = If(Double.IsInfinity(disponible.Width) OrElse champs = 0, Double.PositiveInfinity,
                          Math.Max(0, disponible.Width - fixe) / champs)
            Dim largeurDesChamps = 0.0
            For Each enfant As UIElement In InternalChildren
                If Not EstUnChamp(enfant) Then Continue For
                enfant.Measure(New Size(part, disponible.Height))
                largeurDesChamps += enfant.DesiredSize.Width
                hauteurDesChamps = Math.Max(hauteurDesChamps, enfant.DesiredSize.Height)
            Next
            If champs > 0 Then hauteur = hauteurDesChamps
            Dim largeur = If(Double.IsInfinity(disponible.Width), fixe + largeurDesChamps, Math.Max(fixe + largeurDesChamps, disponible.Width))
            Return New Size(If(champs = 0, fixe, largeur), hauteur)
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim fixe = 0.0
            Dim champs = 0
            For Each enfant As UIElement In InternalChildren
                If EstUnChamp(enfant) Then champs += 1 Else fixe += enfant.DesiredSize.Width
            Next
            Dim part = If(champs = 0, 0, Math.Max(0, finale.Width - fixe) / champs)
            Dim x = 0.0
            For Each enfant As UIElement In InternalChildren
                Dim largeur = If(EstUnChamp(enfant), part, enfant.DesiredSize.Width)
                enfant.Arrange(New Rect(x, 0, largeur, finale.Height))
                x += largeur
            Next
            Return finale
        End Function

    End Class

End Namespace
