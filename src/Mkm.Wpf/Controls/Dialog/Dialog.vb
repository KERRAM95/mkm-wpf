Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La fenêtre de dialogue de mkm, comme celle de 3DEXPERIENCE : une barre de titre de 26 px au titre gras
    ''' et à la croix grise, le contenu sur fond blanc, un pied gris où les boutons se rangent à droite, des
    ''' coins de 6 px sous une ombre courte. C'est une vraie fenêtre : <c>ShowDialog</c> la rend modale et
    ''' rend <c>DialogResult</c> ; Échap la ferme, comme un bouton <c>IsCancel</c>.
    ''' <code>
    ''' &lt;mkm:Dialog Title="Renommer"&gt;
    '''     &lt;mkm:InputText Text="{Binding Nom}" /&gt;
    '''     &lt;mkm:Dialog.Footer&gt;
    '''         &lt;StackPanel Orientation="Horizontal"&gt;
    '''             &lt;mkm:Button Content="Annuler" Severity="Secondary" IsCancel="True" /&gt;
    '''             &lt;mkm:Button Content="Renommer" IsDefault="True" Click="Renommer_Click" /&gt;
    '''         &lt;/StackPanel&gt;
    '''     &lt;/mkm:Dialog.Footer&gt;
    ''' &lt;/mkm:Dialog&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Sans calque à elle, la fenêtre dessine son ombre dans ses bords : elle est transparente autour du dessin.
    ''' On la déplace par sa barre de titre ; <c>ResizeMode="CanResize"</c> lui donne la poignée du coin.
    ''' </remarks>
    <TemplatePart(Name:=Dialog.PartHeader, Type:=GetType(FrameworkElement))>
    <TemplatePart(Name:=Dialog.PartCloseButton, Type:=GetType(ButtonBase))>
    <TemplatePart(Name:=Dialog.PartResizeGrip, Type:=GetType(Thumb))>
    Public Class Dialog
        Inherits Window

        ''' <summary>Le nom de la barre de titre dans le gabarit ; on y tire la fenêtre.</summary>
        Public Const PartHeader As String = "PART_Header"
        ''' <summary>Le nom de la croix dans le gabarit.</summary>
        Public Const PartCloseButton As String = "PART_CloseButton"
        ''' <summary>Le nom de la poignée du coin dans le gabarit.</summary>
        Public Const PartResizeGrip As String = "PART_ResizeGrip"

        ''' <summary>Le pied : les boutons du dialogue, rangés à droite, 10 px entre eux.</summary>
        Public Shared ReadOnly FooterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Footer), GetType(Object), GetType(Dialog), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai (par défaut) si une croix ferme le dialogue.</summary>
        Public Shared ReadOnly ClosableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Closable), GetType(Boolean), GetType(Dialog), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai (par défaut) si Échap ferme le dialogue.</summary>
        Public Shared ReadOnly CloseOnEscapeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(CloseOnEscape), GetType(Boolean), GetType(Dialog), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai (par défaut) si l'on déplace le dialogue en tirant sa barre de titre.</summary>
        Public Shared ReadOnly DraggableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Draggable), GetType(Boolean), GetType(Dialog), New FrameworkPropertyMetadata(True))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Dialog), New FrameworkPropertyMetadata(GetType(Dialog)))
        End Sub

        ''' <summary>
        ''' Un dialogue à la taille de son contenu, au milieu de sa fenêtre propriétaire, hors de la barre des
        ''' tâches, sans poignée.
        ''' </summary>
        Public Sub New()
            ' La forme de la fenêtre se fixe avant qu'elle paraisse : ici, pas dans le style.
            WindowStyle = WindowStyle.None
            AllowsTransparency = True
            Background = Brushes.Transparent
            ShowInTaskbar = False
            SizeToContent = SizeToContent.WidthAndHeight
            WindowStartupLocation = WindowStartupLocation.CenterOwner
            ResizeMode = ResizeMode.NoResize
            If Application.Current IsNot Nothing AndAlso Application.Current.MainWindow IsNot Nothing AndAlso
               Application.Current.MainWindow IsNot Me AndAlso Application.Current.MainWindow.IsLoaded Then
                Owner = Application.Current.MainWindow
            End If
        End Sub

        ''' <summary>Le pied : les boutons du dialogue, rangés à droite, 10 px entre eux.</summary>
        Public Property Footer As Object
            Get
                Return GetValue(FooterProperty)
            End Get
            Set(value As Object)
                SetValue(FooterProperty, value)
            End Set
        End Property

        ''' <summary>Vrai (par défaut) si une croix ferme le dialogue.</summary>
        Public Property Closable As Boolean
            Get
                Return CBool(GetValue(ClosableProperty))
            End Get
            Set(value As Boolean)
                SetValue(ClosableProperty, value)
            End Set
        End Property

        ''' <summary>Vrai (par défaut) si Échap ferme le dialogue.</summary>
        Public Property CloseOnEscape As Boolean
            Get
                Return CBool(GetValue(CloseOnEscapeProperty))
            End Get
            Set(value As Boolean)
                SetValue(CloseOnEscapeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai (par défaut) si l'on déplace le dialogue en tirant sa barre de titre.</summary>
        Public Property Draggable As Boolean
            Get
                Return CBool(GetValue(DraggableProperty))
            End Get
            Set(value As Boolean)
                SetValue(DraggableProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()

            Dim barre = TryCast(GetTemplateChild(PartHeader), FrameworkElement)
            If barre IsNot Nothing Then AddHandler barre.MouseLeftButtonDown, AddressOf Tirer

            Dim croix = TryCast(GetTemplateChild(PartCloseButton), ButtonBase)
            If croix IsNot Nothing Then AddHandler croix.Click, Sub() Close()

            Dim poignee = TryCast(GetTemplateChild(PartResizeGrip), Thumb)
            If poignee IsNot Nothing Then
                AddHandler poignee.DragDelta, AddressOf Etirer
                AddHandler poignee.KeyDown, AddressOf Poignee_KeyDown
            End If
            _croix = TryCast(GetTemplateChild(PartCloseButton), ButtonBase)
        End Sub

        Private _croix As ButtonBase

        ' Au clavier, la poignée étire de 10 px par flèche, de 1 px avec Maj, comme mkm.
        Private Sub Poignee_KeyDown(sender As Object, e As KeyEventArgs)
            Dim pas = If((Keyboard.Modifiers And ModifierKeys.Shift) = ModifierKeys.Shift, 1.0, 10.0)
            Dim dx = 0.0, dy = 0.0
            Select Case e.Key
                Case Key.Left : dx = -pas
                Case Key.Right : dx = pas
                Case Key.Up : dy = -pas
                Case Key.Down : dy = pas
                Case Else : Return
            End Select
            Etirer(sender, New DragDeltaEventArgs(dx, dy))
            e.Handled = True
        End Sub

        ''' <summary>
        ''' À l'ouverture, le clavier entre dans le dialogue : sur l'élément que l'application a désigné
        ''' (FocusManager.FocusedElement, l'autofocus de mkm), sinon sur la croix, sinon sur le premier élément.
        ''' </summary>
        Protected Overrides Sub OnContentRendered(e As EventArgs)
            MyBase.OnContentRendered(e)
            If IsKeyboardFocusWithin Then Return
            Dim designe = TryCast(FocusManager.GetFocusedElement(Me), UIElement)
            If designe IsNot Nothing AndAlso designe.Focus() Then Return
            If _croix IsNot Nothing AndAlso _croix.IsVisible AndAlso _croix.Focus() Then Return
            MoveFocus(New TraversalRequest(FocusNavigationDirection.First))
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse e.Key <> Key.Escape OrElse Not CloseOnEscape Then Return
            e.Handled = True
            Close()
        End Sub

        Private Sub Tirer(sender As Object, e As MouseButtonEventArgs)
            If Not Draggable OrElse e.ButtonState <> MouseButtonState.Pressed Then Return
            DragMove()
        End Sub

        ' La poignée étire la fenêtre ; elle cesse alors de suivre son contenu.
        Private Sub Etirer(sender As Object, e As DragDeltaEventArgs)
            SizeToContent = SizeToContent.Manual
            Width = Math.Max(Math.Max(MinWidth, ActualWidth + e.HorizontalChange), 1)
            Height = Math.Max(Math.Max(MinHeight, ActualHeight + e.VerticalChange), 1)
        End Sub

    End Class

End Namespace
