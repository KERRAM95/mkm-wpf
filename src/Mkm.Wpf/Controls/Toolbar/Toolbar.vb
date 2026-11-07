Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Threading

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La barre de commandes de mkm (<c>Toolbar</c>), celle de 3DEXPERIENCE : sans fond ni bordure, de la hauteur de ses
    ''' boutons, ses commandes (<see cref="ToolbarButton"/>, des boutons secondaires) collées les unes aux autres, à droite,
    ''' séparées au besoin d'un trait (<see cref="ToolbarSeparator"/>). <see cref="Start"/>, <see cref="Center"/> et
    ''' <see cref="[End]"/> reçoivent un autre contenu (un titre, un champ de recherche). Avec <see cref="Overflow"/>, les
    ''' commandes qui ne tiennent pas passent dans le menu d'un bouton « … ».
    ''' <code>
    ''' &lt;mkm:Toolbar Overflow="True"&gt;
    '''     &lt;mkm:Toolbar.Start&gt;&lt;TextBlock Text="Variables" /&gt;&lt;/mkm:Toolbar.Start&gt;
    '''     &lt;mkm:ToolbarButton Content="Ajouter" Icon="{x:Static mkm:Icons.Add}" Command="{Binding Ajouter}" /&gt;
    '''     &lt;mkm:ToolbarSeparator /&gt;
    '''     &lt;mkm:ToolbarButton Content="Supprimer" Icon="{x:Static mkm:Icons.Delete}" Command="{Binding Supprimer}" /&gt;
    ''' &lt;/mkm:Toolbar&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Les commandes sont ses éléments (<c>Items</c> ou <c>ItemsSource</c>) ; chacune est montrée dans un
    ''' <see cref="ContentPresenter"/>, que la barre cache quand elle passe dans le menu. Une commande y garde son libellé
    ''' (<c>Content</c> s'il est un texte, sinon son nom d'accessibilité), son icône, son état grisé ; la choisir la lance
    ''' comme un clic. « Toolbar » et le <c>ToolBar</c> de WPF sont un même nom en VB : qualifier
    ''' (<c>Mkm.Wpf.Controls.Toolbar</c>) là où les deux espaces de noms sont importés.
    ''' </remarks>
    <TemplatePart(Name:="PART_Overflow", Type:=GetType(ButtonBase))>
    <TemplatePart(Name:="PART_OverflowHost", Type:=GetType(ToolbarOverflow))>
    Public Class Toolbar
        Inherits ItemsControl

        ''' <summary>Le contenu du début de la barre, à gauche.</summary>
        Public Shared ReadOnly StartProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Start), GetType(Object), GetType(Toolbar), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le contenu du milieu de la barre, après celui du début.</summary>
        Public Shared ReadOnly CenterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Center), GetType(Object), GetType(Toolbar), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le contenu de la fin de la barre, à droite, après les commandes.</summary>
        Public Shared ReadOnly EndProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([End]), GetType(Object), GetType(Toolbar), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai si les commandes qui ne tiennent pas passent dans le menu du bouton « … ».</summary>
        Public Shared ReadOnly OverflowProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Overflow), GetType(Boolean), GetType(Toolbar),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.AffectsMeasure, AddressOf OnOverflowChanged))

        Private Shared ReadOnly IsOverflowingPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsOverflowing), GetType(Boolean), GetType(Toolbar), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand des commandes ne tiennent pas : le bouton « … » paraît.</summary>
        Public Shared ReadOnly IsOverflowingProperty As DependencyProperty = IsOverflowingPropertyKey.DependencyProperty

        Private _bouton As ButtonBase
        Private _hote As ToolbarOverflow
        Private _menu As ContextMenu

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Toolbar), New FrameworkPropertyMetadata(GetType(Toolbar)))
            FocusableProperty.OverrideMetadata(GetType(Toolbar), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Le contenu du début de la barre, à gauche.</summary>
        Public Property Start As Object
            Get
                Return GetValue(StartProperty)
            End Get
            Set(value As Object)
                SetValue(StartProperty, value)
            End Set
        End Property

        ''' <summary>Le contenu du milieu de la barre, après celui du début.</summary>
        Public Property Center As Object
            Get
                Return GetValue(CenterProperty)
            End Get
            Set(value As Object)
                SetValue(CenterProperty, value)
            End Set
        End Property

        ''' <summary>Le contenu de la fin de la barre, à droite, après les commandes.</summary>
        Public Property [End] As Object
            Get
                Return GetValue(EndProperty)
            End Get
            Set(value As Object)
                SetValue(EndProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si les commandes qui ne tiennent pas passent dans le menu du bouton « … ».</summary>
        Public Property Overflow As Boolean
            Get
                Return CBool(GetValue(OverflowProperty))
            End Get
            Set(value As Boolean)
                SetValue(OverflowProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand des commandes ne tiennent pas : le bouton « … » paraît.</summary>
        Public ReadOnly Property IsOverflowing As Boolean
            Get
                Return CBool(GetValue(IsOverflowingProperty))
            End Get
        End Property

        ''' <summary>Le menu du bouton « … », créé à sa première ouverture ; ses choix sont refaits à chaque ouverture.</summary>
        Public ReadOnly Property OverflowMenu As ContextMenu
            Get
                Return _menu
            End Get
        End Property

        ''' <summary>La rangée des commandes, qui se déclare à sa première mesure.</summary>
        Friend Property Rangee As ToolbarPanel

        ''' <summary>La largeur que prend le bouton « … » et son trait, qu'il soit montré ou non.</summary>
        Friend ReadOnly Property LargeurDuDebordement As Double
            Get
                Return If(_hote Is Nothing, 0.0, _hote.LargeurDuContenu)
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _bouton IsNot Nothing Then RemoveHandler _bouton.Click, AddressOf OnBouton
            MyBase.OnApplyTemplate()
            _bouton = TryCast(GetTemplateChild("PART_Overflow"), ButtonBase)
            _hote = TryCast(GetTemplateChild("PART_OverflowHost"), ToolbarOverflow)
            If _bouton IsNot Nothing Then AddHandler _bouton.Click, AddressOf OnBouton
        End Sub

        ''' <summary>Chaque commande est montrée dans un présentateur, que la barre cache quand elle déborde.</summary>
        Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
            Return False
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetContainerForItemOverride() As DependencyObject
            Return New ContentPresenter()
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            ' La flèche bas sur le bouton « … » ouvre aussi son menu.
            If e.Key = Key.Down AndAlso e.OriginalSource Is _bouton Then
                e.Handled = True
                OuvrirLeMenu()
            End If
            MyBase.OnPreviewKeyDown(e)
        End Sub

        ''' <summary>Pose l'état de débordement, mesuré par la rangée des commandes.</summary>
        Friend Sub Deborder(deborde As Boolean)
            If deborde = IsOverflowing Then Return
            SetValue(IsOverflowingPropertyKey, deborde)
            _hote?.InvalidateMeasure()
            If Not deborde AndAlso _menu IsNot Nothing Then _menu.IsOpen = False
        End Sub

        ''' <summary>Les commandes cachées, dans l'ordre : celles qui ne tiennent pas dans la barre.</summary>
        Friend Function CommandesCachees() As IList(Of Object)
            Dim cachees As New List(Of Object)
            For i = 0 To Items.Count - 1
                Dim conteneur = TryCast(ItemContainerGenerator.ContainerFromIndex(i), UIElement)
                If conteneur IsNot Nothing AndAlso conteneur.Visibility = Visibility.Hidden Then cachees.Add(Items(i))
            Next
            Return cachees
        End Function

        ''' <summary>
        ''' Les choix du menu « … » : une commande par commande cachée, sans séparateur en tête, en queue, ni deux de suite.
        ''' </summary>
        Friend Function ChoixDuMenu() As IList(Of Control)
            Dim choix As New List(Of Control)
            For Each element In CommandesCachees()
                If TypeOf element Is ToolbarSeparator Then
                    If choix.Count > 0 AndAlso Not TypeOf choix(choix.Count - 1) Is Separator Then choix.Add(New Separator())
                    Continue For
                End If
                Dim commande = TryCast(element, ButtonBase)
                If commande Is Nothing OrElse commande.Visibility = Visibility.Collapsed Then Continue For
                choix.Add(ChoixPour(commande))
            Next
            While choix.Count > 0 AndAlso TypeOf choix(choix.Count - 1) Is Separator
                choix.RemoveAt(choix.Count - 1)
            End While
            Return choix
        End Function

        ' Le choix du menu d'une commande cachée : son libellé, son icône, son état grisé ; le choisir la lance.
        Private Function ChoixPour(commande As ButtonBase) As MenuItem
            Dim libelle = If(TryCast(commande.Content, String), Automation.AutomationProperties.GetName(commande))
            Dim element As New MenuItem With {
                .Header = libelle,
                .Icon = TryCast(commande, Button)?.Icon,
                .IsEnabled = commande.IsEnabled
            }
            AddHandler element.Click, Sub(s, e) Lancer(commande)
            Return element
        End Function

        ''' <summary>Lance une commande comme un clic : son évènement <c>Click</c> et sa <c>Command</c>.</summary>
        Friend Shared Sub Lancer(commande As ButtonBase)
            If commande Is Nothing OrElse Not commande.IsEnabled Then Return
            Dim pair = UIElementAutomationPeer.CreatePeerForElement(commande)
            Dim invoquer = TryCast(pair?.GetPattern(PatternInterface.Invoke), IInvokeProvider)
            If invoquer IsNot Nothing Then
                invoquer.Invoke()
                Return
            End If
            Dim basculer = TryCast(pair?.GetPattern(PatternInterface.Toggle), IToggleProvider)
            basculer?.Toggle()
        End Sub

        Private Sub OnBouton(sender As Object, e As RoutedEventArgs)
            If _menu IsNot Nothing AndAlso _menu.IsOpen Then
                _menu.IsOpen = False
                Return
            End If
            OuvrirLeMenu()
        End Sub

        Private Sub OuvrirLeMenu()
            Dim choix = ChoixDuMenu()
            If choix.Count = 0 OrElse _bouton Is Nothing Then Return
            If _menu Is Nothing Then
                _menu = New ContextMenu With {.Placement = PlacementMode.Bottom}
                AddHandler _menu.Opened, AddressOf OnMenuOuvert
                AddHandler _menu.Closed, AddressOf OnMenuFerme
            End If
            _menu.Items.Clear()
            For Each element In choix
                _menu.Items.Add(element)
            Next
            _menu.PlacementTarget = _bouton
            _menu.IsOpen = True
        End Sub

        ' Ouvert au clavier, le menu prend le focus sur sa première commande, comme dans mkm.
        Private Sub OnMenuOuvert(sender As Object, e As RoutedEventArgs)
            If Not (InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice) Then Return
            Dispatcher.BeginInvoke(DispatcherPriority.Input, New Action(
                Sub()
                    Dim premier = _menu?.Items.OfType(Of MenuItem)().FirstOrDefault(Function(m) m.IsEnabled)
                    premier?.Focus()
                End Sub))
        End Sub

        ' Fermé, le menu rend le focus au bouton « … », comme Échap ; à la souris aussi, comme dans mkm, mais sans son
        ' anneau, que mkm ne montre qu'au clavier.
        Private Sub OnMenuFerme(sender As Object, e As RoutedEventArgs)
            If _bouton IsNot Nothing AndAlso _bouton.IsVisible Then _bouton.Focus()
        End Sub

        Private Shared Sub OnOverflowChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim barre = DirectCast(d, Toolbar)
            If Not CBool(e.NewValue) Then barre.Deborder(False)
            barre.Rangee?.InvalidateMeasure()
        End Sub

    End Class

    ''' <summary>
    ''' Une commande de <see cref="Toolbar"/> : le bouton de mkm, secondaire (gris et bordé) comme les commandes de la
    ''' barre de 3DEXPERIENCE. Son libellé (<c>Content</c>) et son icône (<c>Icon</c>) la suivent dans le menu « … ».
    ''' </summary>
    Public Class ToolbarButton
        Inherits Button

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ToolbarButton), New FrameworkPropertyMetadata(GetType(ToolbarButton)))
            SeverityProperty.OverrideMetadata(GetType(ToolbarButton), New FrameworkPropertyMetadata(ButtonSeverity.Secondary))
        End Sub

    End Class

    ''' <summary>Le trait entre deux groupes de commandes d'une <see cref="Toolbar"/> : 24 px de haut, à 4 px de chaque côté.</summary>
    Public Class ToolbarSeparator
        Inherits Control

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ToolbarSeparator), New FrameworkPropertyMetadata(GetType(ToolbarSeparator)))
            FocusableProperty.OverrideMetadata(GetType(ToolbarSeparator), New FrameworkPropertyMetadata(False))
        End Sub

    End Class

End Namespace
