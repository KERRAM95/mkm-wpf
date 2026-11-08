Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Input
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Controls

    ''' <summary>Une ligne d'un <see cref="Terminal"/> : la commande tapée et sa réponse.</summary>
    Public NotInheritable Class TerminalEntry
        Implements INotifyPropertyChanged

        Private _response As String

        Friend Sub New(invite As String, commande As String)
            Prompt = invite
            Command = commande
        End Sub

        ''' <summary>L'invite au moment de la commande.</summary>
        Public ReadOnly Property Prompt As String

        ''' <summary>La commande tapée.</summary>
        Public ReadOnly Property Command As String

        ''' <summary>La réponse ; vide tant qu'elle n'est pas venue.</summary>
        Public Property Response As String
            Get
                Return _response
            End Get
            Friend Set(value As String)
                _response = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Response)))
            End Set
        End Property

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    End Class

    ''' <summary>Les données d'une commande tapée dans un <see cref="Terminal"/> : la commande, et la réponse à donner.</summary>
    Public Class TerminalCommandEventArgs
        Inherits RoutedEventArgs

        Public Sub New(evenement As RoutedEvent, source As Object, commande As String)
            MyBase.New(evenement, source)
            Command = commande
        End Sub

        ''' <summary>La commande tapée.</summary>
        Public ReadOnly Property Command As String

        ''' <summary>La réponse, posée par le gestionnaire ; sinon, <see cref="Terminal.Respond"/> la donnera plus tard.</summary>
        Public Property Response As String
    End Class

    ''' <summary>
    ''' Le terminal de mkm (<c>Terminal</c>) : une invite où taper une commande ; Entrée l'envoie
    ''' (<see cref="CommandEntered"/>), et sa réponse s'écrit dessous ; l'historique défile.
    ''' <code>
    ''' &lt;mkm:Terminal WelcomeMessage="Bienvenue" Prompt="hscart $" CommandEntered="Terminal_Command" /&gt;
    ''' </code>
    ''' </summary>
    <TemplatePart(Name:="PART_Input", Type:=GetType(TextBox))>
    <TemplatePart(Name:="PART_Scroll", Type:=GetType(ScrollViewer))>
    Public Class Terminal
        Inherits Control

        ''' <summary>Le message d'accueil, en tête.</summary>
        Public Shared ReadOnly WelcomeMessageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(WelcomeMessage), GetType(String), GetType(Terminal), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'invite devant la commande.</summary>
        Public Shared ReadOnly PromptProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Prompt), GetType(String), GetType(Terminal), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Une commande a été tapée.</summary>
        Public Shared ReadOnly CommandEnteredEvent As RoutedEvent = EventManager.RegisterRoutedEvent(
            NameOf(CommandEntered), RoutingStrategy.Bubble, GetType(EventHandler(Of TerminalCommandEventArgs)), GetType(Terminal))

        Private ReadOnly _historique As New ObservableCollection(Of TerminalEntry)
        Private _saisie As TextBox
        Private _defilement As ScrollViewer

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Terminal), New FrameworkPropertyMetadata(GetType(Terminal)))
            FocusableProperty.OverrideMetadata(GetType(Terminal), New FrameworkPropertyMetadata(False))
        End Sub

        Public Sub New()
            Entries = New ReadOnlyObservableCollection(Of TerminalEntry)(_historique)
            AddHandler Localizer.Current.LanguageChanged, Sub() Nommer()
        End Sub

        ''' <summary>Le message d'accueil.</summary>
        Public Property WelcomeMessage As String
            Get
                Return CStr(GetValue(WelcomeMessageProperty))
            End Get
            Set(value As String)
                SetValue(WelcomeMessageProperty, value)
            End Set
        End Property

        ''' <summary>L'invite devant la commande.</summary>
        Public Property Prompt As String
            Get
                Return CStr(GetValue(PromptProperty))
            End Get
            Set(value As String)
                SetValue(PromptProperty, value)
            End Set
        End Property

        ''' <summary>Une commande a été tapée.</summary>
        Public Custom Event CommandEntered As EventHandler(Of TerminalCommandEventArgs)
            AddHandler(value As EventHandler(Of TerminalCommandEventArgs))
                [AddHandler](CommandEnteredEvent, value)
            End AddHandler
            RemoveHandler(value As EventHandler(Of TerminalCommandEventArgs))
                [RemoveHandler](CommandEnteredEvent, value)
            End RemoveHandler
            RaiseEvent(sender As Object, e As TerminalCommandEventArgs)
                Me.RaiseEvent(e)
            End RaiseEvent
        End Event

        ''' <summary>Les commandes tapées et leurs réponses, dans l'ordre.</summary>
        Public ReadOnly Property Entries As ReadOnlyObservableCollection(Of TerminalEntry)

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _saisie IsNot Nothing Then RemoveHandler _saisie.PreviewKeyDown, AddressOf Saisie_PreviewKeyDown
            MyBase.OnApplyTemplate()
            _saisie = TryCast(GetTemplateChild("PART_Input"), TextBox)
            _defilement = TryCast(GetTemplateChild("PART_Scroll"), ScrollViewer)
            If _saisie IsNot Nothing Then AddHandler _saisie.PreviewKeyDown, AddressOf Saisie_PreviewKeyDown
            Nommer()
        End Sub

        Private Sub Nommer()
            If _saisie Is Nothing Then Return
            Dim nom = AutomationProperties.GetName(Me)
            AutomationProperties.SetName(_saisie, If(String.IsNullOrEmpty(nom), Localizer.Current.T("aria.terminalCommand"), nom))
        End Sub

        Private Sub Saisie_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            If e.Key <> Key.Enter Then Return
            Envoyer(_saisie.Text)
            _saisie.Clear()
            e.Handled = True
        End Sub

        ''' <summary>Envoie une commande, comme tapée : elle s'écrit dans l'historique, puis <see cref="CommandEntered"/>.</summary>
        Public Sub Send(commande As String)
            Envoyer(commande)
        End Sub

        Private Sub Envoyer(commande As String)
            Dim texte = If(commande, String.Empty).Trim()
            If texte.Length = 0 Then Return
            Dim ligne As New TerminalEntry(Prompt, texte)
            _historique.Add(ligne)
            Dim e As New TerminalCommandEventArgs(CommandEnteredEvent, Me, texte)
            Me.RaiseEvent(e)
            If e.Response IsNot Nothing Then ligne.Response = e.Response
            Defiler()
        End Sub

        ''' <summary>Donne la réponse de la dernière commande qui n'en a pas (une réponse qui vient plus tard).</summary>
        Public Sub Respond(reponse As String)
            For i = _historique.Count - 1 To 0 Step -1
                If _historique(i).Response Is Nothing Then
                    _historique(i).Response = reponse
                    Exit For
                End If
            Next
            Defiler()
        End Sub

        ''' <summary>Vide l'historique.</summary>
        Public Sub Clear()
            _historique.Clear()
        End Sub

        Private Sub Defiler()
            Dispatcher.BeginInvoke(Threading.DispatcherPriority.Loaded, New System.Action(Sub() _defilement?.ScrollToEnd()))
        End Sub

        ''' <summary>Un clic dans le terminal amène le clavier à l'invite.</summary>
        Protected Overrides Sub OnMouseLeftButtonUp(e As MouseButtonEventArgs)
            MyBase.OnMouseLeftButtonUp(e)
            If _saisie IsNot Nothing AndAlso Not _saisie.IsKeyboardFocused Then _saisie.Focus()
        End Sub

        ''' <summary>Un groupe pour les lecteurs d'écran : l'historique, puis l'invite.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeConteneur(Me, AutomationControlType.Group)
        End Function

    End Class

End Namespace
