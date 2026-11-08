Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Commands

    ''' <summary>La sorte d'une commande : une action, une case qui bascule, ou un choix dans un groupe.</summary>
    Public Enum CommandKind
        ''' <summary>Une action.</summary>
        Normal
        ''' <summary>Une case : chaque lancement l'inverse.</summary>
        Check
        ''' <summary>Un choix : il se coche, et décoche les autres de son groupe (<see cref="CommandDefinition.Group"/>).</summary>
        Radio
    End Enum

    ''' <summary>Les données d'un lancement : la commande, son contexte, et son nouvel état coché.</summary>
    Public Class CommandExecutedEventArgs
        Inherits EventArgs

        ''' <summary>La clé de la commande.</summary>
        Public ReadOnly Property Id As String
        ''' <summary>Le contexte du lancement (les lignes choisies, par exemple).</summary>
        Public ReadOnly Property Context As Object
        ''' <summary>Le nouvel état d'une case, vrai pour un choix, rien pour une action.</summary>
        Public ReadOnly Property Checked As Boolean?

        Friend Sub New(id As String, contexte As Object, coche As Boolean?)
            Me.Id = id
            Context = contexte
            Checked = coche
        End Sub
    End Class

    ''' <summary>
    ''' La définition d'une commande, déclarée une fois dans un <see cref="CommandRegistry"/> : son allure, son raccourci,
    ''' quand elle est grisée ou cachée, et ce qu'elle fait. Une valeur qui dépend du contexte est une fonction de lui.
    ''' </summary>
    Public Class CommandDefinition
        ''' <summary>La clé de la commande.</summary>
        Public Property Id As String
        ''' <summary>Le libellé.</summary>
        Public Property Label As String
        ''' <summary>Le libellé selon le contexte ; sans elle, <see cref="Label"/>.</summary>
        Public Property LabelFor As Func(Of Object, String)
        ''' <summary>L'icône (une icône de <see cref="Icons"/>).</summary>
        Public Property Icon As Object
        ''' <summary>L'aide courte, en info-bulle.</summary>
        Public Property ToolTip As String
        ''' <summary>L'aide longue.</summary>
        Public Property Help As String
        ''' <summary>Le raccourci, tel que « Ctrl+Shift+E » : les menus l'affichent, <see cref="CommandRegistry.BindShortcuts"/> le branche.</summary>
        Public Property Shortcut As String
        ''' <summary>La sorte : une action, une case, un choix.</summary>
        Public Property Kind As CommandKind
        ''' <summary>Le groupe d'un choix.</summary>
        Public Property Group As String
        ''' <summary>L'état coché que tient le registre (une case, un choix).</summary>
        Public Property Checked As Boolean
        ''' <summary>L'état coché que tient l'application : le registre le lit et ne le change jamais.</summary>
        Public Property IsChecked As Func(Of Object, Boolean)
        ''' <summary>Le libellé d'une case ou d'un choix coché.</summary>
        Public Property CheckedLabel As String
        ''' <summary>L'icône d'une case ou d'un choix coché.</summary>
        Public Property CheckedIcon As Object
        ''' <summary>L'info-bulle d'une case ou d'un choix coché.</summary>
        Public Property CheckedToolTip As String
        ''' <summary>Vrai si la commande peut se lancer dans ce contexte ; sans elle, toujours.</summary>
        Public Property IsEnabled As Func(Of Object, Boolean)
        ''' <summary>Vrai si la commande paraît dans ce contexte ; sans elle, toujours.</summary>
        Public Property IsVisible As Func(Of Object, Boolean)
        ''' <summary>Les vues où la commande paraît (<see cref="CommandRegistry.View"/>) ; sans elles, toutes.</summary>
        Public Property Views As IList(Of String)
        ''' <summary>Ce que fait la commande.</summary>
        Public Property Execute As Action(Of CommandExecutedEventArgs)
    End Class

    ''' <summary>
    ''' Une commande d'un <see cref="CommandRegistry"/>, pour WPF : un <see cref="ICommand"/> (grisé quand il ne peut se
    ''' lancer), et son allure du moment (libellé, icône, info-bulle, cochée, visible), que les boutons et les menus lient.
    ''' </summary>
    Public NotInheritable Class RegisteredCommand
        Implements ICommand, INotifyPropertyChanged

        Private ReadOnly _registre As CommandRegistry
        Private _geste As KeyGesture

        Friend Sub New(registre As CommandRegistry, definition As CommandDefinition)
            _registre = registre
            Me.Definition = definition
        End Sub

        ''' <summary>Quand l'allure de la commande change.</summary>
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        ''' <summary>Quand la commande peut, ou ne peut plus, se lancer.</summary>
        Public Custom Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            AddHandler(value As EventHandler)
                AddHandler CommandManager.RequerySuggested, value
            End AddHandler
            RemoveHandler(value As EventHandler)
                RemoveHandler CommandManager.RequerySuggested, value
            End RemoveHandler
            RaiseEvent(sender As Object, e As EventArgs)
            End RaiseEvent
        End Event

        ''' <summary>La définition de la commande.</summary>
        Public ReadOnly Property Definition As CommandDefinition

        ''' <summary>La clé de la commande.</summary>
        Public ReadOnly Property Id As String
            Get
                Return Definition.Id
            End Get
        End Property

        ''' <summary>Le libellé du moment, pour le contexte du registre.</summary>
        Public ReadOnly Property Label As String
            Get
                Return _registre.GetLabel(Me, _registre.Context)
            End Get
        End Property

        ''' <summary>L'icône du moment : celle de l'état coché, quand il en a une.</summary>
        Public ReadOnly Property Icon As Object
            Get
                Return If(IsChecked AndAlso Definition.CheckedIcon IsNot Nothing, Definition.CheckedIcon, Definition.Icon)
            End Get
        End Property

        ''' <summary>L'info-bulle du moment.</summary>
        Public ReadOnly Property ToolTip As String
            Get
                Return If(IsChecked AndAlso Definition.CheckedToolTip IsNot Nothing, Definition.CheckedToolTip, Definition.ToolTip)
            End Get
        End Property

        ''' <summary>Vrai si la case ou le choix est coché, pour le contexte du registre.</summary>
        Public ReadOnly Property IsChecked As Boolean
            Get
                Return _registre.GetChecked(Me, _registre.Context)
            End Get
        End Property

        ''' <summary>Vrai si la commande paraît, pour la vue et le contexte du registre.</summary>
        Public ReadOnly Property IsVisible As Boolean
            Get
                Return _registre.GetVisible(Me, _registre.Context)
            End Get
        End Property

        ''' <summary>La visibilité d'un bouton ou d'un élément de menu de la commande.</summary>
        Public ReadOnly Property Visibility As Visibility
            Get
                Return If(IsVisible, Visibility.Visible, Visibility.Collapsed)
            End Get
        End Property

        ''' <summary>Le raccourci, en geste de WPF ; rien sans raccourci.</summary>
        Public ReadOnly Property Gesture As KeyGesture
            Get
                If _geste Is Nothing AndAlso Not String.IsNullOrEmpty(Definition.Shortcut) Then
                    _geste = DirectCast(New KeyGestureConverter().ConvertFromInvariantString(Definition.Shortcut), KeyGesture)
                End If
                Return _geste
            End Get
        End Property

        ''' <summary>Vrai si la commande peut se lancer : visible et non grisée, pour ce contexte (sinon celui du registre).</summary>
        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _registre.CanExecute(Id, If(parameter, _registre.Context))
        End Function

        ''' <summary>Lance la commande pour ce contexte (sinon celui du registre).</summary>
        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _registre.Execute(Id, If(parameter, _registre.Context))
        End Sub

        ''' <summary>Annonce que l'allure de la commande a pu changer.</summary>
        Friend Sub Rafraichir()
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Nothing))
        End Sub

    End Class

    ''' <summary>Un sous-menu d'une disposition : son libellé, son icône, ses éléments (une disposition).</summary>
    Public Class CommandSubmenu
        ''' <summary>Le libellé ; sans lui, celui de la commande par défaut.</summary>
        Public Property Label As String
        ''' <summary>L'icône ; sans elle, celle de la commande par défaut.</summary>
        Public Property Icon As Object
        ''' <summary>Les éléments : des clés de commandes, « - » pour un trait, d'autres sous-menus.</summary>
        Public Property Items As IList(Of Object) = New List(Of Object)
        ''' <summary>La commande que lance le sous-menu lui-même (la commande partagée d'une liste de l'ActionBar).</summary>
        Public Property DefaultCommand As String
    End Class

    ''' <summary>
    ''' Le registre des commandes de mkm (<c>createCommands</c>) : une application y déclare ses commandes une fois, par
    ''' leur clé, avec leurs états et leurs actions ; ses barres et ses menus les nomment par leur clé, dans une disposition
    ''' (clés, « - » pour un trait, <see cref="CommandSubmenu"/>), et le registre en fait les éléments d'un menu, d'une
    ''' <see cref="Toolbar"/> ou d'une <see cref="ActionBar"/>, liés à la commande : grisés, cachés, cochés, renommés avec
    ''' elle. Une commande cachée ne laisse pas de trait seul.
    ''' <code>
    ''' Dim commandes As New CommandRegistry(
    '''     New CommandDefinition With {.Id = "enregistrer", .Label = "Enregistrer", .Icon = Icons.Download, .Shortcut = "Ctrl+S",
    '''                                 .Execute = Sub(e) Enregistrer()},
    '''     New CommandDefinition With {.Id = "verrouiller", .Kind = CommandKind.Check, .Label = "Verrouiller",
    '''                                 .CheckedLabel = "Déverrouiller", .Icon = Icons.Unlocked, .CheckedIcon = Icons.Locked})
    ''' Menu.ItemsSource = commandes.CreateMenuItems({"enregistrer", "-", "verrouiller"})
    ''' commandes.BindShortcuts(Me)
    ''' </code>
    ''' </summary>
    Public Class CommandRegistry

        ''' <summary>Le trait d'une disposition.</summary>
        Public Const Separator As String = "-"

        Private ReadOnly _ordre As New List(Of String)
        Private ReadOnly _commandes As New Dictionary(Of String, RegisteredCommand)(StringComparer.Ordinal)
        Private _vue As String
        Private _contexte As Object

        ''' <summary>Un registre de ces commandes.</summary>
        Public Sub New(ParamArray definitions As CommandDefinition())
            For Each definition In definitions
                Add(definition)
            Next
        End Sub

        ''' <summary>Déclare une commande.</summary>
        Public Sub Add(definition As CommandDefinition)
            If definition Is Nothing Then Throw New ArgumentNullException(NameOf(definition))
            If String.IsNullOrEmpty(definition.Id) Then Throw New ArgumentException("Une commande a une clé (Id).", NameOf(definition))
            If definition.Id = Separator Then Throw New ArgumentException($"« {Separator} » est le trait d'une disposition, pas une clé de commande.", NameOf(definition))
            If _commandes.ContainsKey(definition.Id) Then Throw New ArgumentException($"La commande « {definition.Id} » est déjà déclarée.", NameOf(definition))
            If definition.Kind = CommandKind.Radio AndAlso String.IsNullOrEmpty(definition.Group) Then
                Throw New ArgumentException($"Le choix « {definition.Id} » a besoin d'un groupe (Group).", NameOf(definition))
            End If
            Dim commande As New RegisteredCommand(Me, definition)
            ' Un raccourci illisible se voit à la déclaration, pas au premier appui.
            If Not String.IsNullOrEmpty(definition.Shortcut) Then
                Dim unused = commande.Gesture
            End If
            _ordre.Add(definition.Id)
            _commandes.Add(definition.Id, commande)
        End Sub

        ''' <summary>Les clés des commandes, dans l'ordre de leur déclaration.</summary>
        Public ReadOnly Property Ids As IReadOnlyList(Of String)
            Get
                Return _ordre.AsReadOnly()
            End Get
        End Property

        ''' <summary>La commande d'une clé.</summary>
        Default Public ReadOnly Property Item(id As String) As RegisteredCommand
            Get
                Dim commande As RegisteredCommand = Nothing
                If id Is Nothing OrElse Not _commandes.TryGetValue(id, commande) Then Throw New KeyNotFoundException($"Commande inconnue : « {id} ».")
                Return commande
            End Get
        End Property

        ''' <summary>La vue du moment : une commande qui a des vues ne paraît que dans les siennes.</summary>
        Public Property View As String
            Get
                Return _vue
            End Get
            Set(value As String)
                _vue = value
                Refresh()
            End Set
        End Property

        ''' <summary>Le contexte des commandes lancées sans contexte (les lignes choisies, par exemple).</summary>
        Public Property Context As Object
            Get
                Return _contexte
            End Get
            Set(value As Object)
                _contexte = value
                Refresh()
            End Set
        End Property

        ''' <summary>Annonce que l'état des commandes a pu changer : leurs boutons et leurs menus se remettent à jour.</summary>
        Public Sub Refresh()
            For Each commande In _commandes.Values
                commande.Rafraichir()
            Next
            CommandManager.InvalidateRequerySuggested()
        End Sub

        ''' <summary>Vrai si la commande peut se lancer dans ce contexte : visible et non grisée.</summary>
        Public Function CanExecute(id As String, Optional context As Object = Nothing) As Boolean
            Dim commande = Item(id)
            Return GetVisible(commande, context) AndAlso (commande.Definition.IsEnabled Is Nothing OrElse commande.Definition.IsEnabled(context))
        End Function

        ''' <summary>
        ''' Lance une commande si elle est visible et non grisée : une case s'inverse d'abord, un choix se coche (et décoche
        ''' les autres de son groupe). Rend vrai si elle a été lancée.
        ''' </summary>
        Public Function Execute(id As String, Optional context As Object = Nothing) As Boolean
            Dim commande = Item(id)
            If Not CanExecute(id, context) Then Return False
            Dim definition = commande.Definition
            Dim coche As Boolean? = Nothing
            Select Case definition.Kind
                Case CommandKind.Check
                    coche = Not GetChecked(commande, context)
                    If definition.IsChecked Is Nothing Then definition.Checked = coche.Value
                Case CommandKind.Radio
                    coche = True
                    If definition.IsChecked Is Nothing Then Cocher(id)
            End Select
            definition.Execute?.Invoke(New CommandExecutedEventArgs(id, context, coche))
            Refresh()
            Return True
        End Function

        ''' <summary>Coche ou décoche une case ou un choix (un choix coché décoche les autres de son groupe).</summary>
        Public Sub SetChecked(id As String, value As Boolean)
            Dim commande = Item(id)
            If commande.Definition.Kind = CommandKind.Radio AndAlso value Then
                Cocher(id)
            Else
                commande.Definition.Checked = value
            End If
            Refresh()
        End Sub

        ' Un choix coché décoche les autres de son groupe, sauf ceux dont l'état vient de l'application.
        Private Sub Cocher(id As String)
            Dim groupe = Item(id).Definition.Group
            For Each autre In _commandes.Values
                Dim d = autre.Definition
                If d.Kind = CommandKind.Radio AndAlso d.Group = groupe AndAlso d.IsChecked Is Nothing Then d.Checked = (d.Id = id)
            Next
        End Sub

        Friend Function GetVisible(commande As RegisteredCommand, context As Object) As Boolean
            Dim d = commande.Definition
            If d.Views IsNot Nothing AndAlso (_vue Is Nothing OrElse Not d.Views.Contains(_vue)) Then Return False
            Return d.IsVisible Is Nothing OrElse d.IsVisible(context)
        End Function

        Friend Function GetChecked(commande As RegisteredCommand, context As Object) As Boolean
            Dim d = commande.Definition
            If d.Kind = CommandKind.Normal Then Return False
            Return If(d.IsChecked IsNot Nothing, d.IsChecked(context), d.Checked)
        End Function

        Friend Function GetLabel(commande As RegisteredCommand, context As Object) As String
            Dim d = commande.Definition
            If GetChecked(commande, context) AndAlso d.CheckedLabel IsNot Nothing Then Return d.CheckedLabel
            Return If(d.LabelFor IsNot Nothing, d.LabelFor(context), d.Label)
        End Function

        ' Les éléments d'une disposition : toutes ses commandes, cachées comprises (leur visibilité est liée : une commande
        ' cachée à la création peut paraître plus tard) ; ni trait en tête ni en queue, ni deux de suite. Les traits
        ' suivent ensuite les commandes visibles (TraitsSuivis).
        Private Function Entrees(layout As IEnumerable(Of Object)) As IList(Of Object)
            Dim resultat As New List(Of Object)
            For Each entree In layout
                Dim cle = TryCast(entree, String)
                If cle = Separator Then
                    If resultat.Count > 0 AndAlso Not Equals(resultat(resultat.Count - 1), Separator) Then resultat.Add(Separator)
                ElseIf cle IsNot Nothing Then
                    Dim unused = Item(cle)
                    resultat.Add(cle)
                ElseIf TypeOf entree Is CommandSubmenu Then
                    Dim sous = DirectCast(entree, CommandSubmenu)
                    If sous.DefaultCommand IsNot Nothing Then Dim unused = Item(sous.DefaultCommand)
                    If Entrees(sous.Items).Count > 0 OrElse sous.DefaultCommand IsNot Nothing Then resultat.Add(sous)
                Else
                    Throw New ArgumentException($"« {entree} » n'est pas une entrée de disposition : une clé, « {Separator} » ou un CommandSubmenu.", NameOf(layout))
                End If
            Next
            While resultat.Count > 0 AndAlso Equals(resultat(resultat.Count - 1), Separator)
                resultat.RemoveAt(resultat.Count - 1)
            End While
            Return resultat
        End Function

        ' La commande que montre un élément (sa visibilité), Nothing pour un trait ou un sous-menu.
        Private Function CommandeDe(entree As Object) As RegisteredCommand
            Dim cle = TryCast(entree, String)
            If cle Is Nothing OrElse cle = Separator Then Return Nothing
            Return Item(cle)
        End Function

        Private Shared Sub Lier(cible As DependencyObject, propriete As DependencyProperty, commande As RegisteredCommand, chemin As String)
            BindingOperations.SetBinding(cible, propriete, New Binding(chemin) With {.Source = commande, .Mode = BindingMode.OneWay})
        End Sub

        ''' <summary>Les éléments d'un menu (<see cref="MenuItem"/> et <see cref="System.Windows.Controls.Separator"/>) d'une disposition.</summary>
        Public Function CreateMenuItems(layout As IEnumerable(Of Object)) As IList(Of Control)
            If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
            Dim resultat As New List(Of Control)
            Dim suivis As New TraitsSuivis()
            For Each entree In Entrees(layout)
                If Equals(entree, Separator) Then
                    resultat.Add(suivis.Trait(New System.Windows.Controls.Separator()))
                ElseIf TypeOf entree Is CommandSubmenu Then
                    Dim sous = DirectCast(entree, CommandSubmenu)
                    Dim element As New MenuItem With {.Header = sous.Label, .Icon = sous.Icon}
                    If sous.DefaultCommand IsNot Nothing Then
                        Dim parDefaut = Item(sous.DefaultCommand)
                        If sous.Label Is Nothing Then Lier(element, HeaderedItemsControl.HeaderProperty, parDefaut, NameOf(RegisteredCommand.Label))
                        If sous.Icon Is Nothing Then Lier(element, MenuItem.IconProperty, parDefaut, NameOf(RegisteredCommand.Icon))
                    End If
                    For Each enfant In CreateMenuItems(sous.Items)
                        element.Items.Add(enfant)
                    Next
                    resultat.Add(suivis.Element(element, Nothing))
                Else
                    resultat.Add(suivis.Element(ElementDeMenu(Item(CStr(entree))), Item(CStr(entree))))
                End If
            Next
            suivis.Ranger()
            Return resultat
        End Function

        Private Shared Function ElementDeMenu(commande As RegisteredCommand) As MenuItem
            Dim element As New MenuItem With {
                .Command = commande,
                .IsCheckable = commande.Definition.Kind <> CommandKind.Normal,
                .InputGestureText = If(commande.Definition.Shortcut, "")}
            Lier(element, HeaderedItemsControl.HeaderProperty, commande, NameOf(RegisteredCommand.Label))
            Lier(element, MenuItem.IconProperty, commande, NameOf(RegisteredCommand.Icon))
            Lier(element, MenuItem.IsCheckedProperty, commande, NameOf(RegisteredCommand.IsChecked))
            Lier(element, UIElement.VisibilityProperty, commande, NameOf(RegisteredCommand.Visibility))
            Lier(element, FrameworkElement.ToolTipProperty, commande, NameOf(RegisteredCommand.ToolTip))
            Return element
        End Function

        ''' <summary>Les commandes d'une <see cref="Toolbar"/> (<see cref="ToolbarButton"/> et <see cref="ToolbarSeparator"/>) d'une disposition.</summary>
        Public Function CreateToolbarItems(layout As IEnumerable(Of Object)) As IList(Of UIElement)
            If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
            Dim resultat As New List(Of UIElement)
            Dim suivis As New TraitsSuivis()
            For Each entree In Entrees(layout)
                If Equals(entree, Separator) Then
                    resultat.Add(suivis.Trait(New ToolbarSeparator()))
                    Continue For
                End If
                Dim cle = If(TryCast(entree, String), TryCast(entree, CommandSubmenu)?.DefaultCommand)
                If cle Is Nothing Then Continue For
                Dim commande = Item(cle)
                Dim bouton As New ToolbarButton With {.Command = commande}
                Lier(bouton, ContentControl.ContentProperty, commande, NameOf(RegisteredCommand.Label))
                Lier(bouton, Mkm.Wpf.Controls.Button.IconProperty, commande, NameOf(RegisteredCommand.Icon))
                Lier(bouton, FrameworkElement.ToolTipProperty, commande, NameOf(RegisteredCommand.ToolTip))
                Lier(bouton, UIElement.VisibilityProperty, commande, NameOf(RegisteredCommand.Visibility))
                resultat.Add(suivis.Element(bouton, commande))
            Next
            suivis.Ranger()
            Return resultat
        End Function

        ''' <summary>
        ''' Les commandes d'une section de l'<see cref="ActionBar"/> (<see cref="ActionBarButton"/>,
        ''' <see cref="ActionBarToggleButton"/> pour une case ou un choix, <see cref="ActionBarSeparator"/>) d'une disposition.
        ''' </summary>
        Public Function CreateActionBarItems(layout As IEnumerable(Of Object)) As IList(Of UIElement)
            If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
            Dim resultat As New List(Of UIElement)
            Dim suivis As New TraitsSuivis()
            For Each entree In Entrees(layout)
                If Equals(entree, Separator) Then
                    resultat.Add(suivis.Trait(New ActionBarSeparator()))
                    Continue For
                End If
                ' Un sous-menu : une liste de l'ActionBar, ses commandes dedans (sans traits), comme mkm.
                Dim sous = TryCast(entree, CommandSubmenu)
                If sous IsNot Nothing Then
                    Dim liste As New ActionBarDropDown()
                    For Each enfant In CreateActionBarItems(sous.Items).Where(Function(e) TypeOf e IsNot ActionBarSeparator)
                        liste.Items.Add(enfant)
                    Next
                    If liste.Items.Count > 0 Then resultat.Add(suivis.Element(liste, Nothing))
                    Continue For
                End If
                Dim cle = TryCast(entree, String)
                If cle Is Nothing Then Continue For
                Dim commande = Item(cle)
                Dim element As FrameworkElement
                If commande.Definition.Kind = CommandKind.Normal Then
                    element = New ActionBarButton With {.Command = commande}
                    Lier(element, ActionBarButton.LabelProperty, commande, NameOf(RegisteredCommand.Label))
                    Lier(element, ActionBarButton.IconProperty, commande, NameOf(RegisteredCommand.Icon))
                    Lier(element, ActionBarButton.HintProperty, commande, NameOf(RegisteredCommand.ToolTip))
                Else
                    element = New ActionBarToggleButton With {.Command = commande}
                    Lier(element, ActionBarToggleButton.LabelProperty, commande, NameOf(RegisteredCommand.Label))
                    Lier(element, ActionBarToggleButton.IconProperty, commande, NameOf(RegisteredCommand.Icon))
                    Lier(element, ActionBarToggleButton.HintProperty, commande, NameOf(RegisteredCommand.ToolTip))
                    Lier(element, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, commande, NameOf(RegisteredCommand.IsChecked))
                End If
                Lier(element, UIElement.VisibilityProperty, commande, NameOf(RegisteredCommand.Visibility))
                resultat.Add(suivis.Element(element, commande))
            Next
            suivis.Ranger()
            Return resultat
        End Function

        ''' <summary>
        ''' Branche les raccourcis des commandes sur un élément (une fenêtre) : chacun lance sa commande, pour le contexte du
        ''' registre. Un champ de saisie garde ses touches.
        ''' </summary>
        Public Sub BindShortcuts(element As UIElement)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            For Each cle In _ordre
                Dim commande = _commandes(cle)
                If commande.Gesture IsNot Nothing Then element.InputBindings.Add(New KeyBinding(New RaccourciHorsSaisie(commande), commande.Gesture))
            Next
        End Sub

    End Class

    ''' <summary>
    ''' Le raccourci d'une commande : il ne se lance pas quand le clavier est dans un champ de saisie (une zone de texte,
    ''' un mot de passe, une liste modifiable), comme mkm ; la touche reste au champ.
    ''' </summary>
    Friend NotInheritable Class RaccourciHorsSaisie
        Implements ICommand

        Friend ReadOnly Property Commande As RegisteredCommand

        Friend Sub New(commande As RegisteredCommand)
            Me.Commande = commande
        End Sub

        Public Custom Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            AddHandler(value As EventHandler)
                AddHandler Commande.CanExecuteChanged, value
            End AddHandler
            RemoveHandler(value As EventHandler)
                RemoveHandler Commande.CanExecuteChanged, value
            End RemoveHandler
            RaiseEvent(sender As Object, e As EventArgs)
            End RaiseEvent
        End Event

        Friend Shared Function EstUneSaisie(element As Object) As Boolean
            If TypeOf element Is System.Windows.Controls.Primitives.TextBoxBase OrElse TypeOf element Is PasswordBox Then Return True
            Dim liste = TryCast(element, ComboBox)
            Return liste IsNot Nothing AndAlso liste.IsEditable
        End Function

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return Not EstUneSaisie(Keyboard.FocusedElement) AndAlso DirectCast(Commande, ICommand).CanExecute(parameter)
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            DirectCast(Commande, ICommand).Execute(parameter)
        End Sub

    End Class

    ''' <summary>
    ''' Les traits d'une liste d'éléments créée par le registre : un trait ne paraît qu'entre deux éléments visibles, et
    ''' jamais deux de suite, quand des commandes se cachent ou paraissent (après un changement de contexte ou de vue).
    ''' Chaque trait le retient ; les commandes sont écoutées par un abonnement faible.
    ''' </summary>
    Friend NotInheritable Class TraitsSuivis

        Private ReadOnly _elements As New List(Of KeyValuePair(Of UIElement, RegisteredCommand))
        Private ReadOnly _traits As New HashSet(Of UIElement)

        Private Shared ReadOnly RetenuProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Retenu", GetType(TraitsSuivis), GetType(TraitsSuivis), New PropertyMetadata(Nothing))

        Friend Function Trait(Of T As UIElement)(cible As T) As T
            _elements.Add(New KeyValuePair(Of UIElement, RegisteredCommand)(cible, Nothing))
            _traits.Add(cible)
            cible.SetValue(RetenuProperty, Me)
            Return cible
        End Function

        Friend Function Element(Of T As UIElement)(cible As T, commande As RegisteredCommand) As T
            _elements.Add(New KeyValuePair(Of UIElement, RegisteredCommand)(cible, commande))
            If commande IsNot Nothing Then PropertyChangedEventManager.AddHandler(commande, AddressOf Commande_Change, NameOf(RegisteredCommand.Visibility))
            Return cible
        End Function

        Private Sub Commande_Change(sender As Object, e As PropertyChangedEventArgs)
            Ranger()
        End Sub

        Private Shared Function Visible(paire As KeyValuePair(Of UIElement, RegisteredCommand)) As Boolean
            Return If(paire.Value Is Nothing, True, paire.Value.Visibility = Visibility.Visible)
        End Function

        ' Un trait paraît après un élément visible, s'il en reste un visible après lui avant le trait suivant.
        Friend Sub Ranger()
            Dim vuAvant = False
            For i = 0 To _elements.Count - 1
                Dim paire = _elements(i)
                If _traits.Contains(paire.Key) Then
                    Dim apres = False
                    For j = i + 1 To _elements.Count - 1
                        If _traits.Contains(_elements(j).Key) Then Continue For
                        If Visible(_elements(j)) Then
                            apres = True
                            Exit For
                        End If
                    Next
                    Dim montre = vuAvant AndAlso apres
                    paire.Key.Visibility = If(montre, Visibility.Visible, Visibility.Collapsed)
                    If montre Then vuAvant = False
                ElseIf Visible(paire) Then
                    vuAvant = True
                End If
            Next
        End Sub

    End Class

End Namespace
