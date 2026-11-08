Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' L'image de mkm (Image) : une image, et avec <c>Preview</c> un aperçu en grand d'un clic, sur le voile du thème, à
    ''' tourner et à agrandir. « Image » est aussi l'image de WPF (<c>System.Windows.Controls.Image</c>), qu'un fichier VB
    ''' qui importe les deux écrit en entier.
    ''' <code>
    ''' &lt;mkm:Image Source="/Images/plan.png" Preview="True" Width="240" AutomationProperties.Name="Plan HS-00042" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Preview", Type:=GetType(ButtonBase))>
    Public Class Image
        Inherits Control

        ''' <summary>L'image.</summary>
        Public Shared ReadOnly SourceProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Source), GetType(ImageSource), GetType(Image), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Comment elle remplit sa place : entière (Uniform) par défaut.</summary>
        Public Shared ReadOnly StretchProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Stretch), GetType(Stretch), GetType(Image), New FrameworkPropertyMetadata(Stretch.Uniform))

        ''' <summary>Vrai pour l'aperçu en grand d'un clic (un œil au survol).</summary>
        Public Shared ReadOnly PreviewProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Preview), GetType(Boolean), GetType(Image), New FrameworkPropertyMetadata(False))

        Private _apercu As ButtonBase

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Image), New FrameworkPropertyMetadata(GetType(Image)))
            FocusableProperty.OverrideMetadata(GetType(Image), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>L'image.</summary>
        Public Property Source As ImageSource
            Get
                Return DirectCast(GetValue(SourceProperty), ImageSource)
            End Get
            Set(value As ImageSource)
                SetValue(SourceProperty, value)
            End Set
        End Property

        ''' <summary>Comment elle remplit sa place.</summary>
        Public Property Stretch As Stretch
            Get
                Return DirectCast(GetValue(StretchProperty), Stretch)
            End Get
            Set(value As Stretch)
                SetValue(StretchProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour l'aperçu en grand.</summary>
        Public Property Preview As Boolean
            Get
                Return CBool(GetValue(PreviewProperty))
            End Get
            Set(value As Boolean)
                SetValue(PreviewProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _apercu IsNot Nothing Then RemoveHandler _apercu.Click, AddressOf Apercu_Click
            MyBase.OnApplyTemplate()
            _apercu = TryCast(GetTemplateChild("PART_Preview"), ButtonBase)
            If _apercu IsNot Nothing Then AddHandler _apercu.Click, AddressOf Apercu_Click
        End Sub

        Private Sub Apercu_Click(sender As Object, e As RoutedEventArgs)
            ShowPreview()
        End Sub

        ''' <summary>Ouvre l'aperçu en grand, par-dessus sa fenêtre ; Échap, sa croix ou un clic sur le voile le ferment.</summary>
        Public Sub ShowPreview()
            If Source Is Nothing Then Return
            Dim apercu As New ApercuDImage(Me)
            apercu.Show()
        End Sub

        ''' <summary>Une image pour les lecteurs d'écran, nommée par <c>AutomationProperties.Name</c> ; avec l'aperçu, son bouton.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Image)
        End Function

    End Class

    ''' <summary>
    ''' L'aperçu en grand d'une <see cref="Image"/> : une fenêtre sans bord sur la fenêtre de l'image, le voile du thème,
    ''' l'image au milieu, et en haut à droite tourner, réduire, agrandir et fermer.
    ''' </summary>
    Friend NotInheritable Class ApercuDImage
        Inherits Window

        Private ReadOnly _rotation As New RotateTransform()
        Private ReadOnly _echelle As New ScaleTransform()

        Friend Sub New(image As Image)
            WindowStyle = WindowStyle.None
            AllowsTransparency = True
            ResizeMode = ResizeMode.NoResize
            ShowInTaskbar = False
            Dim proprietaire = Window.GetWindow(image)
            Owner = proprietaire
            If proprietaire IsNot Nothing AndAlso proprietaire.WindowState <> WindowState.Maximized Then
                Left = proprietaire.Left
                Top = proprietaire.Top
                Width = proprietaire.ActualWidth
                Height = proprietaire.ActualHeight
            Else
                WindowState = WindowState.Maximized
            End If
            SetResourceReference(BackgroundProperty, "mkm.image.preview.mask.background")
            Title = If(AutomationProperties.GetName(image), String.Empty)

            Dim vue As New System.Windows.Controls.Image With {.Source = image.Source, .Stretch = Stretch.Uniform, .Margin = New Thickness(48),
                                                               .RenderTransformOrigin = New Point(0.5, 0.5)}
            Dim transformation As New TransformGroup()
            transformation.Children.Add(_rotation)
            transformation.Children.Add(_echelle)
            vue.RenderTransform = transformation
            AutomationProperties.SetName(vue, Title)

            Dim barre As New StackPanel With {.Orientation = Orientation.Horizontal, .HorizontalAlignment = HorizontalAlignment.Right,
                                             .VerticalAlignment = VerticalAlignment.Top}
            Dim cadre As New Border With {.Child = barre, .HorizontalAlignment = HorizontalAlignment.Right, .VerticalAlignment = VerticalAlignment.Top}
            cadre.SetResourceReference(Border.BackgroundProperty, "mkm.image.toolbar.background")
            cadre.SetResourceReference(Border.BorderBrushProperty, "mkm.image.toolbar.border.color")
            cadre.SetResourceReference(Border.PaddingProperty, "mkm.image.toolbar.padding")
            cadre.SetResourceReference(Border.CornerRadiusProperty, "mkm.image.toolbar.border.radius")
            cadre.SetResourceReference(Border.MarginProperty, "mkm.image.toolbar.margin")
            cadre.BorderThickness = New Thickness(1)
            barre.Children.Add(BoutonDAction(InternalIcons.Refresh, "aria.rotateRight", Sub() _rotation.Angle += 90))
            barre.Children.Add(BoutonDAction(InternalIcons.Undo, "aria.rotateLeft", Sub() _rotation.Angle -= 90))
            barre.Children.Add(BoutonDAction(InternalIcons.SearchMinus, "aria.zoomOut", Sub() Echelle(-0.1)))
            barre.Children.Add(BoutonDAction(InternalIcons.SearchPlus, "aria.zoomIn", Sub() Echelle(0.1)))
            barre.Children.Add(BoutonDAction(InternalIcons.Times, "aria.close", AddressOf Close))

            Dim grille As New Grid()
            grille.Children.Add(vue)
            grille.Children.Add(cadre)
            Content = grille
            KeyboardNavigation.SetTabNavigation(grille, KeyboardNavigationMode.Cycle)
            ' Un clic sur le voile (hors de l'image et de la barre) ferme l'aperçu.
            AddHandler grille.MouseLeftButtonDown, Sub(s, e)
                                                       If e.OriginalSource Is grille Then Close()
                                                   End Sub
            grille.Background = Brushes.Transparent
            AddHandler Loaded, Sub() barre.Children(0).Focus()
        End Sub

        Private Sub Echelle(pas As Double)
            Dim valeur = Math.Max(0.5, Math.Min(3, _echelle.ScaleX + pas))
            _echelle.ScaleX = valeur
            _echelle.ScaleY = valeur
        End Sub

        ' Un bouton de la barre : 36 px, l'icône de 18 px, claire sur le voile.
        Private Shared Function BoutonDAction(icone As Geometry, cle As String, quoi As System.Action) As System.Windows.Controls.Button
            Dim dessin As New Mkm.Wpf.Controls.Icon With {.Source = icone}
            dessin.SetResourceReference(WidthProperty, "mkm.image.action.icon.size")
            dessin.SetResourceReference(HeightProperty, "mkm.image.action.icon.size")
            Dim bouton As New System.Windows.Controls.Button With {.Content = dessin}
            bouton.SetResourceReference(StyleProperty, "mkm.Image.Action")
            AutomationProperties.SetName(bouton, Localizer.Current.T(cle))
            AddHandler bouton.Click, Sub() quoi()
            Return bouton
        End Function

        ''' <summary>Échap ferme l'aperçu.</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Key = Key.Escape Then
                Close()
                e.Handled = True
            End If
        End Sub

    End Class

End Namespace
