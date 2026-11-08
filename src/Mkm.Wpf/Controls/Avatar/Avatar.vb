Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>La taille d'un <see cref="Avatar"/>.</summary>
    Public Enum AvatarSize
        ''' <summary>24 px (par défaut).</summary>
        Normal
        ''' <summary>36 px.</summary>
        Large
        ''' <summary>48 px.</summary>
        XLarge
    End Enum

    ''' <summary>La forme d'un <see cref="Avatar"/>.</summary>
    Public Enum AvatarShape
        ''' <summary>Un carré aux coins du thème (par défaut).</summary>
        Square
        ''' <summary>Un rond.</summary>
        Circle
    End Enum

    ''' <summary>
    ''' L'avatar de mkm : une personne ou un objet en petit carré (ou rond) de 24 px, par ses initiales
    ''' (<c>Label</c>), une icône (<c>Icon</c>) ou une image (<c>Image</c>), sur un fond gris pâle.
    ''' <code>
    ''' &lt;mkm:Avatar Label="MK" Shape="Circle" AutomationProperties.Name="Mourad Kerram" /&gt;
    ''' </code>
    ''' </summary>
    Public Class Avatar
        Inherits Control

        ''' <summary>Le texte de l'avatar : des initiales.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Label), GetType(String), GetType(Avatar), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Son icône (une de <see cref="Icons"/>), à défaut de texte.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(Avatar), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Son image (une photo), qui remplit l'avatar.</summary>
        Public Shared ReadOnly ImageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Image), GetType(ImageSource), GetType(Avatar), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Sa taille.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(AvatarSize), GetType(Avatar), New FrameworkPropertyMetadata(AvatarSize.Normal))

        ''' <summary>Sa forme.</summary>
        Public Shared ReadOnly ShapeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Shape), GetType(AvatarShape), GetType(Avatar), New FrameworkPropertyMetadata(AvatarShape.Square))

        Private Shared ReadOnly IsInGroupPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsInGroup), GetType(Boolean), GetType(Avatar), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai dans un <see cref="AvatarGroup"/> : l'avatar chevauche son voisin, cerné du fond de la page.</summary>
        Public Shared ReadOnly IsInGroupProperty As DependencyProperty = IsInGroupPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Avatar), New FrameworkPropertyMetadata(GetType(Avatar)))
            FocusableProperty.OverrideMetadata(GetType(Avatar), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Le texte de l'avatar : des initiales.</summary>
        Public Property Label As String
            Get
                Return CStr(GetValue(LabelProperty))
            End Get
            Set(value As String)
                SetValue(LabelProperty, value)
            End Set
        End Property

        ''' <summary>Son icône, à défaut de texte.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Son image, qui remplit l'avatar.</summary>
        Public Property Image As ImageSource
            Get
                Return DirectCast(GetValue(ImageProperty), ImageSource)
            End Get
            Set(value As ImageSource)
                SetValue(ImageProperty, value)
            End Set
        End Property

        ''' <summary>Sa taille.</summary>
        Public Property Size As AvatarSize
            Get
                Return DirectCast(GetValue(SizeProperty), AvatarSize)
            End Get
            Set(value As AvatarSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>Sa forme.</summary>
        Public Property Shape As AvatarShape
            Get
                Return DirectCast(GetValue(ShapeProperty), AvatarShape)
            End Get
            Set(value As AvatarShape)
                SetValue(ShapeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai dans un <see cref="AvatarGroup"/>.</summary>
        Public ReadOnly Property IsInGroup As Boolean
            Get
                Return CBool(GetValue(IsInGroupProperty))
            End Get
        End Property

        Friend Sub Grouper(groupe As Boolean)
            SetValue(IsInGroupPropertyKey, groupe)
        End Sub

        ''' <summary>
        ''' Une image pour les lecteurs d'écran, nommée par <c>AutomationProperties.Name</c> (le nom de la personne), à
        ''' défaut par son texte.
        ''' </summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Image, Function() Label)
        End Function

    End Class

    ''' <summary>
    ''' Des avatars côte à côte qui se chevauchent (AvatarGroup de mkm) : les personnes d'une équipe, d'un document.
    ''' <code>
    ''' &lt;mkm:AvatarGroup&gt;
    '''     &lt;mkm:Avatar Label="MK" Shape="Circle" /&gt;
    '''     &lt;mkm:Avatar Label="JD" Shape="Circle" /&gt;
    ''' &lt;/mkm:AvatarGroup&gt;
    ''' </code>
    ''' </summary>
    Public Class AvatarGroup
        Inherits ItemsControl

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(AvatarGroup), New FrameworkPropertyMetadata(GetType(AvatarGroup)))
            FocusableProperty.OverrideMetadata(GetType(AvatarGroup), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, ses avatars dedans.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return TypeOf item Is Avatar
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New Avatar()
        End Function

        ''' <summary>Chaque avatar après le premier chevauche le précédent (avatar.group.offset, selon sa taille).</summary>
        Protected Overrides Sub PrepareContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.PrepareContainerForItemOverride(element, item)
            Dim avatar = TryCast(element, Avatar)
            If avatar Is Nothing Then Return
            avatar.Grouper(True)
            If ItemContainerGenerator.IndexFromContainer(avatar) > 0 Then
                Dim cle = Theming.ThemeResources.Prefix & If(avatar.Size = AvatarSize.XLarge, "avatar.xl.group.offset",
                                                           If(avatar.Size = AvatarSize.Large, "avatar.lg.group.offset", "avatar.group.offset"))
                Dim ecart = TryFindResource(cle)
                avatar.Margin = New Thickness(If(TypeOf ecart Is Double, CDbl(ecart), 0), 0, 0, 0)
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub ClearContainerForItemOverride(element As DependencyObject, item As Object)
            MyBase.ClearContainerForItemOverride(element, item)
            TryCast(element, Avatar)?.Grouper(False)
        End Sub

    End Class

End Namespace
