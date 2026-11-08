Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Threading

Namespace Mkm.Wpf.Controls

    ''' <summary>Une page d'un <see cref="Carousel"/>, pour ses indicateurs.</summary>
    Public NotInheritable Class CarouselPage
        Friend Sub New(numero As Integer, active As Boolean)
            Me.Number = numero
            Me.IsActive = active
            Label = Localization.Localizer.Current.T("aria.page", numero)
        End Sub

        ''' <summary>Son nom pour les lecteurs d'écran : « Page 2 ».</summary>
        Public ReadOnly Property Label As String

        ''' <summary>Le numéro de la page, à partir de 1.</summary>
        Public ReadOnly Property Number As Integer

        ''' <summary>Vrai pour la page montrée.</summary>
        Public ReadOnly Property IsActive As Boolean
    End Class

    ''' <summary>
    ''' Le carrousel de mkm (<c>Carousel</c>) : ses éléments page par page, <see cref="NumVisible"/> à la fois ; des
    ''' flèches pour passer à la page voisine, des indicateurs pour aller à une page ; il peut tourner seul.
    ''' <code>
    ''' &lt;mkm:Carousel ItemsSource="{Binding Pieces}" NumVisible="3" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Track", Type:=GetType(System.Windows.Controls.Panel))>
    <TemplatePart(Name:="PART_Previous", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Next", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_Indicators", Type:=GetType(ItemsControl))>
    Public Class Carousel
        Inherits ItemsControl

        ''' <summary>La page montrée, à partir de 0. Se lie dans les deux sens.</summary>
        Public Shared ReadOnly PageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Page), GetType(Integer), GetType(Carousel),
            New FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnPageChanged, AddressOf BornerLaPage))

        ''' <summary>Le nombre d'éléments montrés à la fois : 1 par défaut.</summary>
        Public Shared ReadOnly NumVisibleProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(NumVisible), GetType(Integer), GetType(Carousel), New FrameworkPropertyMetadata(1, AddressOf OnFormeChanged), AddressOf AuMoinsUn)

        ''' <summary>Le nombre d'éléments qu'une page fait défiler : 1 par défaut.</summary>
        Public Shared ReadOnly NumScrollProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(NumScroll), GetType(Integer), GetType(Carousel), New FrameworkPropertyMetadata(1, AddressOf OnFormeChanged), AddressOf AuMoinsUn)

        ''' <summary>Vrai pour revenir au début après la dernière page.</summary>
        Public Shared ReadOnly CircularProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Circular), GetType(Boolean), GetType(Carousel), New FrameworkPropertyMetadata(False, AddressOf OnFormeChanged))

        ''' <summary>L'intervalle, en millisecondes, d'un carrousel qui tourne seul ; 0 (par défaut) : il ne tourne pas.</summary>
        Public Shared ReadOnly AutoplayIntervalProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AutoplayInterval), GetType(Integer), GetType(Carousel), New FrameworkPropertyMetadata(0, AddressOf OnAutoplayChanged))

        ''' <summary>En ligne (par défaut) ou en colonne.</summary>
        Public Shared ReadOnly OrientationProperty As DependencyProperty = StackPanel.OrientationProperty.AddOwner(
            GetType(Carousel), New FrameworkPropertyMetadata(Orientation.Horizontal))

        ''' <summary>Vrai (par défaut) pour les flèches.</summary>
        Public Shared ReadOnly ShowNavigatorsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowNavigators), GetType(Boolean), GetType(Carousel), New FrameworkPropertyMetadata(True))

        ''' <summary>Vrai (par défaut) pour les indicateurs de page.</summary>
        Public Shared ReadOnly ShowIndicatorsProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(ShowIndicators), GetType(Boolean), GetType(Carousel), New FrameworkPropertyMetadata(True))

        Private Shared ReadOnly PageCountPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(PageCount), GetType(Integer), GetType(Carousel), New FrameworkPropertyMetadata(0))
        ''' <summary>Le nombre de pages.</summary>
        Public Shared ReadOnly PageCountProperty As DependencyProperty = PageCountPropertyKey.DependencyProperty

        Private Shared ReadOnly PagesPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(Pages), GetType(ObservableCollection(Of CarouselPage)), GetType(Carousel), New FrameworkPropertyMetadata(Nothing))
        ''' <summary>Les pages, pour les indicateurs.</summary>
        Public Shared ReadOnly PagesProperty As DependencyProperty = PagesPropertyKey.DependencyProperty

        Private Shared ReadOnly CanGoPreviousPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(CanGoPrevious), GetType(Boolean), GetType(Carousel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand une page précède (ou que le carrousel tourne).</summary>
        Public Shared ReadOnly CanGoPreviousProperty As DependencyProperty = CanGoPreviousPropertyKey.DependencyProperty

        Private Shared ReadOnly CanGoNextPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(CanGoNext), GetType(Boolean), GetType(Carousel), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand une page suit (ou que le carrousel tourne).</summary>
        Public Shared ReadOnly CanGoNextProperty As DependencyProperty = CanGoNextPropertyKey.DependencyProperty

        Private _piste As PisteDeCarousel
        Private _precedent As ButtonBase
        Private _suivant As ButtonBase
        Private _indicateurs As ItemsControl
        Private _minuteur As DispatcherTimer

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Carousel), New FrameworkPropertyMetadata(GetType(Carousel)))
            FocusableProperty.OverrideMetadata(GetType(Carousel), New FrameworkPropertyMetadata(False))
        End Sub

        Public Sub New()
            SetValue(PagesPropertyKey, New ObservableCollection(Of CarouselPage)())
            [AddHandler](ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf Bouton_Click))
            AddHandler Unloaded, Sub() _minuteur?.Stop()
            AddHandler Loaded, Sub() Demarrer()
        End Sub

        ''' <summary>La page montrée.</summary>
        Public Property Page As Integer
            Get
                Return CInt(GetValue(PageProperty))
            End Get
            Set(value As Integer)
                SetValue(PageProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre d'éléments montrés à la fois.</summary>
        Public Property NumVisible As Integer
            Get
                Return CInt(GetValue(NumVisibleProperty))
            End Get
            Set(value As Integer)
                SetValue(NumVisibleProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre d'éléments qu'une page fait défiler.</summary>
        Public Property NumScroll As Integer
            Get
                Return CInt(GetValue(NumScrollProperty))
            End Get
            Set(value As Integer)
                SetValue(NumScrollProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour revenir au début après la dernière page.</summary>
        Public Property Circular As Boolean
            Get
                Return CBool(GetValue(CircularProperty))
            End Get
            Set(value As Boolean)
                SetValue(CircularProperty, value)
            End Set
        End Property

        ''' <summary>L'intervalle d'un carrousel qui tourne seul, en millisecondes.</summary>
        Public Property AutoplayInterval As Integer
            Get
                Return CInt(GetValue(AutoplayIntervalProperty))
            End Get
            Set(value As Integer)
                SetValue(AutoplayIntervalProperty, value)
            End Set
        End Property

        ''' <summary>En ligne ou en colonne.</summary>
        Public Property Orientation As Orientation
            Get
                Return DirectCast(GetValue(OrientationProperty), Orientation)
            End Get
            Set(value As Orientation)
                SetValue(OrientationProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour les flèches.</summary>
        Public Property ShowNavigators As Boolean
            Get
                Return CBool(GetValue(ShowNavigatorsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowNavigatorsProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour les indicateurs.</summary>
        Public Property ShowIndicators As Boolean
            Get
                Return CBool(GetValue(ShowIndicatorsProperty))
            End Get
            Set(value As Boolean)
                SetValue(ShowIndicatorsProperty, value)
            End Set
        End Property

        ''' <summary>Le nombre de pages.</summary>
        Public ReadOnly Property PageCount As Integer
            Get
                Return CInt(GetValue(PageCountProperty))
            End Get
        End Property

        ''' <summary>Les pages, pour les indicateurs.</summary>
        Public ReadOnly Property Pages As ObservableCollection(Of CarouselPage)
            Get
                Return DirectCast(GetValue(PagesProperty), ObservableCollection(Of CarouselPage))
            End Get
        End Property

        ''' <summary>Vrai quand une page précède.</summary>
        Public ReadOnly Property CanGoPrevious As Boolean
            Get
                Return CBool(GetValue(CanGoPreviousProperty))
            End Get
        End Property

        ''' <summary>Vrai quand une page suit.</summary>
        Public ReadOnly Property CanGoNext As Boolean
            Get
                Return CBool(GetValue(CanGoNextProperty))
            End Get
        End Property

        Private Shared Function AuMoinsUn(valeur As Object) As Boolean
            Return CInt(valeur) >= 1
        End Function

        ''' <summary>Le nombre de pages : celles qu'il faut pour montrer le dernier élément.</summary>
        Friend Function CompterLesPages() As Integer
            Dim nombre = Items.Count
            If nombre = 0 Then Return 0
            If nombre <= NumVisible Then Return 1
            Return CInt(Math.Ceiling((nombre - NumVisible) / CDbl(NumScroll))) + 1
        End Function

        Private Shared Function BornerLaPage(d As DependencyObject, valeur As Object) As Object
            Dim carrousel = DirectCast(d, Carousel)
            Dim nombre = carrousel.CompterLesPages()
            Return Math.Max(0, Math.Min(Math.Max(0, nombre - 1), CInt(valeur)))
        End Function

        ''' <summary>Le rang du premier élément montré.</summary>
        Friend ReadOnly Property Premier As Integer
            Get
                Return Math.Max(0, Math.Min(Page * NumScroll, Items.Count - NumVisible))
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _precedent = TryCast(GetTemplateChild("PART_Previous"), ButtonBase)
            _suivant = TryCast(GetTemplateChild("PART_Next"), ButtonBase)
            _indicateurs = TryCast(GetTemplateChild("PART_Indicators"), ItemsControl)
            Recompter()
        End Sub

        ''' <summary>La piste du gabarit, posée par la piste elle-même (son ItemsHost).</summary>
        Friend Sub PoserLaPiste(piste As PisteDeCarousel)
            _piste = piste
            Placer(False)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            Recompter()
        End Sub

        Private Shared Sub OnFormeChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Carousel).Recompter()
        End Sub

        Private Shared Sub OnPageChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim carrousel = DirectCast(d, Carousel)
            carrousel.Recompter()
            carrousel.Placer(True)
        End Sub

        ' Les pages, les indicateurs, ce que les flèches peuvent.
        Private Sub Recompter()
            CoerceValue(PageProperty)
            Dim nombre = CompterLesPages()
            SetValue(PageCountPropertyKey, nombre)
            Dim pages = Me.Pages
            pages.Clear()
            For i = 0 To nombre - 1
                pages.Add(New CarouselPage(i + 1, i = Page))
            Next
            SetValue(CanGoPreviousPropertyKey, nombre > 1 AndAlso (Circular OrElse Page > 0))
            SetValue(CanGoNextPropertyKey, nombre > 1 AndAlso (Circular OrElse Page < nombre - 1))
            Placer(False)
        End Sub

        ' La piste glisse jusqu'au premier élément de la page.
        Private Sub Placer(anime As Boolean)
            _piste?.Aller(Premier, anime AndAlso SystemParameters.ClientAreaAnimation)
        End Sub

        ''' <summary>La page précédente ; au début, la dernière si le carrousel tourne.</summary>
        Public Sub Previous()
            If PageCount = 0 Then Return
            If Page > 0 Then
                SetCurrentValue(PageProperty, Page - 1)
            ElseIf Circular Then
                SetCurrentValue(PageProperty, PageCount - 1)
            End If
        End Sub

        ''' <summary>La page suivante ; à la fin, la première si le carrousel tourne.</summary>
        Public Sub [Next]()
            If PageCount = 0 Then Return
            If Page < PageCount - 1 Then
                SetCurrentValue(PageProperty, Page + 1)
            ElseIf Circular Then
                SetCurrentValue(PageProperty, 0)
            End If
        End Sub

        Private Sub Bouton_Click(sender As Object, e As RoutedEventArgs)
            If e.OriginalSource Is _precedent Then
                Previous()
            ElseIf e.OriginalSource Is _suivant Then
                [Next]()
            ElseIf _indicateurs IsNot Nothing AndAlso TypeOf TryCast(e.OriginalSource, FrameworkElement)?.DataContext Is CarouselPage AndAlso
                   _indicateurs.IsAncestorOf(DirectCast(e.OriginalSource, DependencyObject)) Then
                SetCurrentValue(PageProperty, DirectCast(DirectCast(e.OriginalSource, FrameworkElement).DataContext, CarouselPage).Number - 1)
            Else
                Return
            End If
            e.Handled = True
        End Sub

        ' ---- Tourner seul ----

        Private Shared Sub OnAutoplayChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Carousel).Demarrer()
        End Sub

        ' Un carrousel qui tourne seul revient au début ; le survol l'arrête, comme mkm.
        Private Sub Demarrer()
            _minuteur?.Stop()
            If AutoplayInterval <= 0 OrElse Not IsLoaded Then Return
            If _minuteur Is Nothing Then
                _minuteur = New DispatcherTimer(DispatcherPriority.Background, Dispatcher)
                AddHandler _minuteur.Tick, AddressOf Tourner
            End If
            _minuteur.Interval = TimeSpan.FromMilliseconds(AutoplayInterval)
            _minuteur.Start()
        End Sub

        Private Sub Tourner(sender As Object, e As EventArgs)
            If IsMouseOver OrElse IsKeyboardFocusWithin Then Return
            If Page < PageCount - 1 Then SetCurrentValue(PageProperty, Page + 1) Else SetCurrentValue(PageProperty, 0)
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran, nommé par <c>AutomationProperties.Name</c> : ses flèches, ses
        ''' éléments montrés, ses indicateurs.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

    ''' <summary>
    ''' La piste d'un <see cref="Carousel"/> : ses éléments côte à côte (ou l'un sous l'autre), chacun d'un
    ''' <c>NumVisible</c>-ième de la place, décalés jusqu'au premier montré ; le reste est caché.
    ''' </summary>
    Friend NotInheritable Class PisteDeCarousel
        Inherits System.Windows.Controls.Panel

        Public Shared ReadOnly GapProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Gap), GetType(Double), GetType(PisteDeCarousel), New FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange))

        Private ReadOnly _decalage As New TranslateTransform()
        Private _premier As Integer

        Public Sub New()
            ClipToBounds = True
            AddHandler Loaded, Sub() Proprietaire()?.PoserLaPiste(Me)
        End Sub

        Public Property Gap As Double
            Get
                Return CDbl(GetValue(GapProperty))
            End Get
            Set(value As Double)
                SetValue(GapProperty, value)
            End Set
        End Property

        Private Function Proprietaire() As Carousel
            Return TryCast(ItemsControl.GetItemsOwner(Me), Carousel)
        End Function

        Private ReadOnly Property EnLigne As Boolean
            Get
                Dim carrousel = Proprietaire()
                Return carrousel Is Nothing OrElse carrousel.Orientation = Orientation.Horizontal
            End Get
        End Property

        Private ReadOnly Property Visibles As Integer
            Get
                Return If(Proprietaire()?.NumVisible, 1)
            End Get
        End Property

        ' La taille d'un élément : sa part de la place, moins les écarts.
        Private Function Pas(place As Size) As Double
            Dim longueur = If(EnLigne, place.Width, place.Height)
            Return Math.Max(0, (longueur - Gap * (Visibles - 1)) / Visibles)
        End Function

        Protected Overrides Function MeasureOverride(disponible As Size) As Size
            Dim enLigne = Me.EnLigne
            Dim cote = If(enLigne, If(Double.IsInfinity(disponible.Width), 300, disponible.Width), If(Double.IsInfinity(disponible.Height), 300, disponible.Height))
            Dim taille = Pas(If(enLigne, New Size(cote, 0), New Size(0, cote)))
            Dim travers = 0.0
            For Each enfant As UIElement In InternalChildren
                enfant.Measure(If(enLigne, New Size(taille, disponible.Height), New Size(disponible.Width, taille)))
                travers = Math.Max(travers, If(enLigne, enfant.DesiredSize.Height, enfant.DesiredSize.Width))
            Next
            Return If(enLigne, New Size(cote, travers), New Size(travers, cote))
        End Function

        Protected Overrides Function ArrangeOverride(finale As Size) As Size
            Dim enLigne = Me.EnLigne
            Dim taille = Pas(finale)
            Dim position = 0.0
            For Each enfant As UIElement In InternalChildren
                enfant.Arrange(If(enLigne, New Rect(position, 0, taille, finale.Height), New Rect(0, position, finale.Width, taille)))
                enfant.RenderTransform = _decalage
                position += taille + Gap
            Next
            Aller(_premier, False)
            Return finale
        End Function

        ''' <summary>Décale la piste jusqu'à l'élément <paramref name="premier"/>, en glissant ou d'un coup.</summary>
        Friend Sub Aller(premier As Integer, anime As Boolean)
            _premier = premier
            Dim cible = -premier * (Pas(RenderSize) + Gap)
            Dim proprietes = If(EnLigne, TranslateTransform.XProperty, TranslateTransform.YProperty)
            Dim autre = If(EnLigne, TranslateTransform.YProperty, TranslateTransform.XProperty)
            _decalage.BeginAnimation(autre, Nothing)
            _decalage.SetValue(autre, 0.0)
            If anime Then
                _decalage.BeginAnimation(proprietes, New DoubleAnimation(cible, New Duration(TimeSpan.FromMilliseconds(400))) With {
                    .EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseOut}})
            Else
                _decalage.BeginAnimation(proprietes, Nothing)
                _decalage.SetValue(proprietes, cible)
            End If
        End Sub

        ''' <summary>Le rang du premier élément montré, pour les tests.</summary>
        Friend ReadOnly Property PremierMontre As Integer
            Get
                Return _premier
            End Get
        End Property

    End Class

End Namespace
