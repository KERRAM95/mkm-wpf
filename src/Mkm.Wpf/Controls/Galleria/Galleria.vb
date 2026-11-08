Imports System.Collections
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Threading

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La galerie de mkm (<c>Galleria</c>) : une image en grand, et dessous ses vignettes, qui défilent ; un clic sur une
    ''' vignette la montre. Des flèches sur l'image, des indicateurs, une légende, une lecture automatique, au besoin.
    ''' <code>
    ''' &lt;mkm:Galleria ItemsSource="{Binding Photos}" ItemTemplate="…" ThumbnailTemplate="…" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Thumbnails", Type:=GetType(Carousel))>
    <TemplatePart(Name:="PART_Previous", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Next", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Indicators", Type:=GetType(ItemsControl))>
    Public Class Galleria
        Inherits Control

        ''' <summary>Les éléments (des images, ou les données qui les décrivent).</summary>
        Public Shared ReadOnly ItemsSourceProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ItemsSource), GetType(IEnumerable), GetType(Galleria), New FrameworkPropertyMetadata(Nothing, AddressOf OnItemsSourceChanged))

        ''' <summary>Le gabarit de l'élément montré en grand.</summary>
        Public Shared ReadOnly ItemTemplateProperty As DependencyProperty = ItemsControl.ItemTemplateProperty.AddOwner(GetType(Galleria))

        ''' <summary>Le gabarit d'une vignette.</summary>
        Public Shared ReadOnly ThumbnailTemplateProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ThumbnailTemplate), GetType(DataTemplate), GetType(Galleria), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le gabarit de la légende, en bas de l'élément montré (aucune par défaut).</summary>
        Public Shared ReadOnly CaptionTemplateProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(CaptionTemplate), GetType(DataTemplate), GetType(Galleria),
            New FrameworkPropertyMetadata(Nothing, Sub(d, e) d.SetValue(HasCaptionPropertyKey, e.NewValue IsNot Nothing)))

        Private Shared ReadOnly HasCaptionPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasCaption), GetType(Boolean), GetType(Galleria), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand une légende est donnée.</summary>
        Public Shared ReadOnly HasCaptionProperty As DependencyProperty = HasCaptionPropertyKey.DependencyProperty

        ''' <summary>Le rang de l'élément montré. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly ActiveIndexProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ActiveIndex), GetType(Integer), GetType(Galleria),
            New FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnActiveIndexChanged, AddressOf BornerLeRang))

        ''' <summary>Le nombre de vignettes montrées : 5 par défaut.</summary>
        Public Shared ReadOnly NumVisibleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(NumVisible), GetType(Integer), GetType(Galleria), New FrameworkPropertyMetadata(5))

        ''' <summary>Vrai (par défaut) pour les vignettes.</summary>
        Public Shared ReadOnly ShowThumbnailsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowThumbnails), GetType(Boolean), GetType(Galleria), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai pour des flèches sur l'élément montré.</summary>
        Public Shared ReadOnly ShowItemNavigatorsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowItemNavigators), GetType(Boolean), GetType(Galleria), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai pour un indicateur par élément, sous l'élément montré.</summary>
        Public Shared ReadOnly ShowIndicatorsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowIndicators), GetType(Boolean), GetType(Galleria), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai pour revenir au premier après le dernier.</summary>
        Public Shared ReadOnly CircularProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Circular), GetType(Boolean), GetType(Galleria), New FrameworkPropertyMetadata(False, AddressOf OnNavigationChanged))

        ''' <summary>Vrai pour passer seul d'un élément au suivant.</summary>
        Public Shared ReadOnly AutoPlayProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AutoPlay), GetType(Boolean), GetType(Galleria), New FrameworkPropertyMetadata(False, AddressOf OnAutoPlayChanged))

        ''' <summary>L'intervalle de la lecture automatique, en millisecondes : 4000 par défaut.</summary>
        Public Shared ReadOnly TransitionIntervalProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(TransitionInterval), GetType(Integer), GetType(Galleria), New FrameworkPropertyMetadata(4000, AddressOf OnAutoPlayChanged))

        Private Shared ReadOnly ActiveItemPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(ActiveItem), GetType(Object), GetType(Galleria), New FrameworkPropertyMetadata(Nothing))
        ''' <summary>L'élément montré.</summary>
        Public Shared ReadOnly ActiveItemProperty As DependencyProperty = ActiveItemPropertyKey.DependencyProperty

        Private Shared ReadOnly PositionsPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Positions), GetType(ObservableCollection(Of CarouselPage)), GetType(Galleria), New FrameworkPropertyMetadata(Nothing))
        ''' <summary>Un indicateur par élément.</summary>
        Public Shared ReadOnly PositionsProperty As DependencyProperty = PositionsPropertyKey.DependencyProperty

        Private Shared ReadOnly CanGoPreviousPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(CanGoPrevious), GetType(Boolean), GetType(Galleria), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand un élément précède (ou que la galerie tourne).</summary>
        Public Shared ReadOnly CanGoPreviousProperty As DependencyProperty = CanGoPreviousPropertyKey.DependencyProperty

        Private Shared ReadOnly CanGoNextPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(CanGoNext), GetType(Boolean), GetType(Galleria), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand un élément suit (ou que la galerie tourne).</summary>
        Public Shared ReadOnly CanGoNextProperty As DependencyProperty = CanGoNextPropertyKey.DependencyProperty

        Private _elements As New List(Of Object)
        Private _vignettes As Carousel
        Private _precedent As ButtonBase
        Private _suivant As ButtonBase
        Private _indicateurs As ItemsControl
        Private _minuteur As DispatcherTimer

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Galleria), New FrameworkPropertyMetadata(GetType(Galleria)))
            FocusableProperty.OverrideMetadata(GetType(Galleria), New FrameworkPropertyMetadata(False))
        End Sub

        Public Sub New()
            SetValue(PositionsPropertyKey, New ObservableCollection(Of CarouselPage)())
            [AddHandler](ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Bouton_Click))
            AddHandler Loaded, Sub() Demarrer()
            AddHandler Unloaded, Sub() _minuteur?.Stop()
        End Sub

        ''' <summary>Les éléments.</summary>
        Public Property ItemsSource As IEnumerable
            Get
                Return DirectCast(GetValue(ItemsSourceProperty), IEnumerable)
            End Get
            Set(value As IEnumerable)
                SetValue(ItemsSourceProperty, value)
            End Set
        End Property

        ''' <summary>Le gabarit de l'élément montré.</summary>
        Public Property ItemTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(ItemTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(ItemTemplateProperty, value)
            End Set
        End Property

        ''' <summary>Le gabarit d'une vignette.</summary>
        Public Property ThumbnailTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(ThumbnailTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(ThumbnailTemplateProperty, value)
            End Set
        End Property

        ''' <summary>Le gabarit de la légende.</summary>
        Public Property CaptionTemplate As DataTemplate
            Get
                Return DirectCast(GetValue(CaptionTemplateProperty), DataTemplate)
            End Get
            Set(value As DataTemplate)
                SetValue(CaptionTemplateProperty, value)
            End Set
        End Property

        ''' <summary>Le rang de l'élément montré.</summary>
        Public Property ActiveIndex As Integer
            Get
                Return CInt(GetValue(ActiveIndexProperty))
            End Get
            Set(value As Integer)
                SetValue(ActiveIndexProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre de vignettes montrées.</summary>
        Public Property NumVisible As Integer
            Get
                Return CInt(GetValue(NumVisibleProperty))
            End Get
            Set(value As Integer)
                SetValue(NumVisibleProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour les vignettes.</summary>
        Public Property ShowThumbnails As Boolean
            Get
                Return CBool(GetValue(ShowThumbnailsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowThumbnailsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour des flèches sur l'élément montré.</summary>
        Public Property ShowItemNavigators As Boolean
            Get
                Return CBool(GetValue(ShowItemNavigatorsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowItemNavigatorsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour un indicateur par élément.</summary>
        Public Property ShowIndicators As Boolean
            Get
                Return CBool(GetValue(ShowIndicatorsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowIndicatorsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour revenir au premier après le dernier.</summary>
        Public Property Circular As Boolean
            Get
                Return CBool(GetValue(CircularProperty))
            End Get
            Set(value As Boolean)
                SetValue(CircularProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour passer seul d'un élément au suivant.</summary>
        Public Property AutoPlay As Boolean
            Get
                Return CBool(GetValue(AutoPlayProperty))
            End Get
            Set(value As Boolean)
                SetValue(AutoPlayProperty, value)
            End Set
        End Property

        ''' <summary>L'intervalle de la lecture automatique, en millisecondes.</summary>
        Public Property TransitionInterval As Integer
            Get
                Return CInt(GetValue(TransitionIntervalProperty))
            End Get
            Set(value As Integer)
                SetValue(TransitionIntervalProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand une légende est donnée.</summary>
        Public ReadOnly Property HasCaption As Boolean
            Get
                Return CBool(GetValue(HasCaptionProperty))
            End Get
        End Property

        ''' <summary>L'élément montré.</summary>
        Public ReadOnly Property ActiveItem As Object
            Get
                Return GetValue(ActiveItemProperty)
            End Get
        End Property

        ''' <summary>Un indicateur par élément.</summary>
        Public ReadOnly Property Positions As ObservableCollection(Of CarouselPage)
            Get
                Return DirectCast(GetValue(PositionsProperty), ObservableCollection(Of CarouselPage))
            End Get
        End Property

        ''' <summary>Vrai quand un élément précède.</summary>
        Public ReadOnly Property CanGoPrevious As Boolean
            Get
                Return CBool(GetValue(CanGoPreviousProperty))
            End Get
        End Property

        ''' <summary>Vrai quand un élément suit.</summary>
        Public ReadOnly Property CanGoNext As Boolean
            Get
                Return CBool(GetValue(CanGoNextProperty))
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _vignettes = TryCast(GetTemplateChild("PART_Thumbnails"), Carousel)
            ' Les flèches de la bande changent l'image montrée (la bande suit), comme mkm ; une action de l'utilisateur
            ' arrête la lecture automatique.
            If _vignettes IsNot Nothing Then
                _vignettes.FlechesDeleguees = Sub(sens)
                                                  ArreterLaLecture()
                                                  If sens < 0 Then Previous() Else [Next]()
                                              End Sub
            End If
            _precedent = TryCast(GetTemplateChild("PART_Previous"), ButtonBase)
            _suivant = TryCast(GetTemplateChild("PART_Next"), ButtonBase)
            _indicateurs = TryCast(GetTemplateChild("PART_Indicators"), ItemsControl)
            Montrer()
        End Sub

        Private Shared Sub OnItemsSourceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim galerie = DirectCast(d, Galleria)
            Dim avant = TryCast(e.OldValue, INotifyCollectionChanged)
            Dim apres = TryCast(e.NewValue, INotifyCollectionChanged)
            ' Abonnement faible : une liste qui vit plus longtemps que la galerie ne la retient pas.
            If avant IsNot Nothing Then CollectionChangedEventManager.RemoveHandler(avant, AddressOf galerie.Elements_CollectionChanged)
            If apres IsNot Nothing Then CollectionChangedEventManager.AddHandler(apres, AddressOf galerie.Elements_CollectionChanged)
            galerie.Relire()
        End Sub

        Private Sub Elements_CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            Relire()
        End Sub

        Private Sub Relire()
            _elements = If(ItemsSource, Enumerable.Empty(Of Object)()).Cast(Of Object)().ToList()
            CoerceValue(ActiveIndexProperty)
            Montrer()
        End Sub

        Private Shared Function BornerLeRang(d As DependencyObject, valeur As Object) As Object
            Dim nombre = DirectCast(d, Galleria)._elements.Count
            Return Math.Max(0, Math.Min(Math.Max(0, nombre - 1), CInt(valeur)))
        End Function

        Private Shared Sub OnActiveIndexChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Galleria).Montrer()
        End Sub

        Private Shared Sub OnNavigationChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Galleria).Montrer()
        End Sub

        ' L'élément montré, les indicateurs, les flèches ; les vignettes suivent l'élément montré et le marquent.
        Private Sub Montrer()
            Dim nombre = _elements.Count
            SetValue(ActiveItemPropertyKey, If(nombre = 0, Nothing, _elements(ActiveIndex)))
            ' Les indicateurs gardés tant que leur nombre ne change pas : celui qui a le clavier le garde.
            If Positions.Count <> nombre Then
                Positions.Clear()
                For i = 0 To nombre - 1
                    Positions.Add(New CarouselPage(i + 1, i = ActiveIndex))
                Next
            Else
                For i = 0 To nombre - 1
                    Positions(i).IsActive = (i = ActiveIndex)
                Next
            End If
            SetValue(CanGoPreviousPropertyKey, nombre > 1 AndAlso (Circular OrElse ActiveIndex > 0))
            SetValue(CanGoNextPropertyKey, nombre > 1 AndAlso (Circular OrElse ActiveIndex < nombre - 1))
            If _vignettes IsNot Nothing Then
                Dim premier = _vignettes.Page * Math.Max(1, _vignettes.NumScroll)
                If ActiveIndex < premier Then
                    _vignettes.SetCurrentValue(Carousel.PageProperty, ActiveIndex)
                ElseIf ActiveIndex > premier + NumVisible - 1 Then
                    _vignettes.SetCurrentValue(Carousel.PageProperty, ActiveIndex - NumVisible + 1)
                End If
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, New System.Action(AddressOf MarquerLesVignettes))
            End If
        End Sub

        ' La vignette montrée, entière ; les autres, pâlies, comme mkm (et entières sous la souris).
        Friend Sub MarquerLesVignettes()
            If _vignettes Is Nothing Then Return
            For i = 0 To _vignettes.Items.Count - 1
                Dim conteneur = TryCast(_vignettes.ItemContainerGenerator.ContainerFromIndex(i), UIElement)
                If conteneur IsNot Nothing Then conteneur.Opacity = If(i = ActiveIndex OrElse conteneur.IsMouseOver, 1.0, 0.5)
            Next
        End Sub

        ''' <summary>L'élément précédent ; au début, le dernier si la galerie tourne.</summary>
        Public Sub Previous()
            If _elements.Count = 0 Then Return
            If ActiveIndex > 0 Then
                SetCurrentValue(ActiveIndexProperty, ActiveIndex - 1)
            ElseIf Circular Then
                SetCurrentValue(ActiveIndexProperty, _elements.Count - 1)
            End If
        End Sub

        ''' <summary>L'élément suivant ; à la fin, le premier si la galerie tourne.</summary>
        Public Sub [Next]()
            If _elements.Count = 0 Then Return
            If ActiveIndex < _elements.Count - 1 Then
                SetCurrentValue(ActiveIndexProperty, ActiveIndex + 1)
            ElseIf Circular OrElse AutoPlay Then
                SetCurrentValue(ActiveIndexProperty, 0)
            End If
        End Sub

        ' Une vignette cliquée se montre ; les flèches et les indicateurs déplacent l'élément montré.
        Private Sub Bouton_Click(sender As Object, e As RoutedEventArgs)
            Dim source = TryCast(e.OriginalSource, FrameworkElement)
            If source Is Nothing Then Return
            ' Une navigation de l'utilisateur arrête la lecture automatique (stopSlideShow de mkm).
            If source Is _precedent OrElse source Is _suivant OrElse TypeOf source Is BoutonDeVignette OrElse
               (_indicateurs IsNot Nothing AndAlso _indicateurs.IsAncestorOf(source)) Then ArreterLaLecture()
            If source Is _precedent Then
                Previous()
            ElseIf source Is _suivant Then
                [Next]()
            ElseIf TypeOf source Is BoutonDeVignette Then
                Dim rang = _elements.IndexOf(source.DataContext)
                If rang >= 0 Then SetCurrentValue(ActiveIndexProperty, rang)
            ElseIf TypeOf source.DataContext Is CarouselPage AndAlso _indicateurs IsNot Nothing AndAlso _indicateurs.IsAncestorOf(source) Then
                SetCurrentValue(ActiveIndexProperty, DirectCast(source.DataContext, CarouselPage).Number - 1)
            Else
                Return
            End If
            e.Handled = True
        End Sub

        ''' <summary>← et → passent à l'élément voisin, quand le clavier est dans la galerie (hors des vignettes).</summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled OrElse (_vignettes IsNot Nothing AndAlso _vignettes.IsKeyboardFocusWithin) Then Return
            Select Case e.Key
                Case Key.Left : Previous()
                Case Key.Right : [Next]()
                Case Else : Return
            End Select
            e.Handled = True
        End Sub

        Private Shared Sub OnAutoPlayChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Galleria).Demarrer()
        End Sub

        Private Sub ArreterLaLecture()
            _minuteur?.Stop()
        End Sub

        Private Sub Demarrer()
            _minuteur?.Stop()
            If Not AutoPlay OrElse Not IsLoaded Then Return
            If _minuteur Is Nothing Then
                _minuteur = New DispatcherTimer(DispatcherPriority.Background, Dispatcher)
                AddHandler _minuteur.Tick, Sub() If Not IsMouseOver Then [Next]()
            End If
            _minuteur.Interval = TimeSpan.FromMilliseconds(Math.Max(500, TransitionInterval))
            _minuteur.Start()
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c>.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

    ''' <summary>Une vignette d'une <see cref="Galleria"/> : un bouton qui montre son élément.</summary>
    Friend NotInheritable Class BoutonDeVignette
        Inherits System.Windows.Controls.Button

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(BoutonDeVignette), New FrameworkPropertyMetadata(GetType(BoutonDeVignette)))
        End Sub

        ' Sous la souris, la vignette s'éclaire ; en partant, elle reprend sa marque.
        Protected Overrides Sub OnMouseEnter(e As MouseEventArgs)
            MyBase.OnMouseEnter(e)
            Galerie(Me)?.MarquerLesVignettes()
        End Sub

        Protected Overrides Sub OnMouseLeave(e As MouseEventArgs)
            MyBase.OnMouseLeave(e)
            Galerie(Me)?.MarquerLesVignettes()
        End Sub

        Private Shared Function Galerie(element As DependencyObject) As Galleria
            Dim parent = element
            While parent IsNot Nothing AndAlso TypeOf parent IsNot Galleria
                parent = Media.VisualTreeHelper.GetParent(parent)
            End While
            Return TryCast(parent, Galleria)
        End Function

        ''' <summary>Un bouton qui se dit sélectionné quand son élément est montré.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeVignette(Me)
        End Function

        Private NotInheritable Class PairDeVignette
            Inherits ButtonAutomationPeer
            Implements System.Windows.Automation.Provider.ISelectionItemProvider

            Public Sub New(vignette As BoutonDeVignette)
                MyBase.New(vignette)
            End Sub

            Private ReadOnly Property Galerie As Galleria
                Get
                    Dim parent As DependencyObject = Owner
                    While parent IsNot Nothing AndAlso TypeOf parent IsNot Galleria
                        parent = Media.VisualTreeHelper.GetParent(parent)
                    End While
                    Return TryCast(parent, Galleria)
                End Get
            End Property

            Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
                If patternInterface = PatternInterface.SelectionItem Then Return Me
                Return MyBase.GetPattern(patternInterface)
            End Function

            Public ReadOnly Property IsSelected As Boolean Implements System.Windows.Automation.Provider.ISelectionItemProvider.IsSelected
                Get
                    Dim g = Galerie
                    Return g IsNot Nothing AndAlso Equals(g.ActiveItem, DirectCast(Owner, FrameworkElement).DataContext)
                End Get
            End Property

            Public ReadOnly Property SelectionContainer As System.Windows.Automation.Provider.IRawElementProviderSimple Implements System.Windows.Automation.Provider.ISelectionItemProvider.SelectionContainer
                Get
                    Return Nothing
                End Get
            End Property

            Public Sub [Select]() Implements System.Windows.Automation.Provider.ISelectionItemProvider.Select
                DirectCast(Owner, ButtonBase).RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent, Owner))
            End Sub

            Public Sub AddToSelection() Implements System.Windows.Automation.Provider.ISelectionItemProvider.AddToSelection
                [Select]()
            End Sub

            Public Sub RemoveFromSelection() Implements System.Windows.Automation.Provider.ISelectionItemProvider.RemoveFromSelection
            End Sub

        End Class

    End Class

End Namespace
