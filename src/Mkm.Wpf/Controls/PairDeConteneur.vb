Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Ce que voient les lecteurs d'écran et l'automatisation d'un contrôle qui contient d'autres commandes : tout son arbre
    ''' visuel, gabarit compris (les commandes de l'ActionBar, le bouton « … » de la Toolbar, les actions d'un écran vide),
    ''' sous le rôle voulu. Le pair d'un <c>ItemsControl</c> de WPF ne montre que ses éléments : le reste de son gabarit lui
    ''' était invisible.
    ''' </summary>
    Friend Class PairDeConteneur
        Inherits FrameworkElementAutomationPeer

        Private ReadOnly _role As AutomationControlType
        Private ReadOnly _nom As Func(Of String)

        ''' <param name="nom">Son nom, quand l'application ne lui en donne pas (son titre, son libellé).</param>
        Friend Sub New(proprietaire As FrameworkElement, role As AutomationControlType, Optional nom As Func(Of String) = Nothing)
            MyBase.New(proprietaire)
            _role = role
            _nom = nom
        End Sub

        ''' <summary>
        ''' Le nom posé (AutomationProperties.Name, LabeledBy), sinon celui que donne le contrôle (son titre), et seulement
        ''' ensuite le texte de son contenu : celui d'une carte nommait la carte.
        ''' </summary>
        Protected Overrides Function GetNameCore() As String
            Dim nom = AutomationProperties.GetName(Owner)
            If String.IsNullOrEmpty(nom) Then nom = GetLabeledByCore()?.GetName()
            If String.IsNullOrEmpty(nom) AndAlso _nom IsNot Nothing Then nom = _nom()
            Return If(String.IsNullOrEmpty(nom), MyBase.GetNameCore(), nom)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetAutomationControlTypeCore() As AutomationControlType
            Return _role
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetClassNameCore() As String
            Return Owner.GetType().Name
        End Function

        ''' <summary>
        ''' Les pairs de l'arbre visuel, comme WPF les cherche, à une différence : un <c>ItemsControl</c> tout simple du
        ''' gabarit (la rangée des commandes de l'ActionBar) se traverse. Son pair n'en montrerait que des lignes de données
        ''' sans rôle, au lieu des boutons qu'il porte.
        ''' </summary>
        Protected Overrides Function GetChildrenCore() As List(Of AutomationPeer)
            Dim enfants As New List(Of AutomationPeer)()
            Parcourir(Owner, enfants)
            Return If(enfants.Count = 0, Nothing, enfants)
        End Function

        Private Shared Sub Parcourir(parent As DependencyObject, enfants As List(Of AutomationPeer))
            For i = 0 To VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim enfant = VisualTreeHelper.GetChild(parent, i)
                Dim element = TryCast(enfant, UIElement)
                Dim pair = If(element Is Nothing OrElse element.GetType() Is GetType(ItemsControl),
                              Nothing, UIElementAutomationPeer.CreatePeerForElement(element))
                If pair IsNot Nothing Then
                    enfants.Add(pair)
                Else
                    Parcourir(enfant, enfants)
                End If
            Next
        End Sub

    End Class

    ''' <summary>
    ''' Le pair d'une section de l'ActionBar : un onglet, que les lecteurs d'écran disent choisi ou non, et qu'ils choisissent.
    ''' </summary>
    Friend NotInheritable Class PairDeSection
        Inherits PairDeConteneur
        Implements ISelectionItemProvider

        Friend Sub New(proprietaire As ActionBarSection)
            MyBase.New(proprietaire, AutomationControlType.TabItem)
        End Sub

        Private ReadOnly Property Section As ActionBarSection
            Get
                Return DirectCast(Owner, ActionBarSection)
            End Get
        End Property

        Private ReadOnly Property Barre As ActionBar
            Get
                Return TryCast(ItemsControl.ItemsControlFromItemContainer(Section), ActionBar)
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
            If patternInterface = PatternInterface.SelectionItem Then Return Me
            Return MyBase.GetPattern(patternInterface)
        End Function

        ''' <summary>Son en-tête, quand l'application ne lui a pas donné de nom.</summary>
        Protected Overrides Function GetNameCore() As String
            Dim nom = MyBase.GetNameCore()
            Return If(String.IsNullOrEmpty(nom), If(TryCast(Section.Header, String), String.Empty), nom)
        End Function

        ''' <summary>Vrai pour la section affichée.</summary>
        Public ReadOnly Property IsSelected As Boolean Implements ISelectionItemProvider.IsSelected
            Get
                Dim b = Barre
                Return b IsNot Nothing AndAlso b.SelectedItem Is Section
            End Get
        End Property

        ''' <summary>La barre.</summary>
        Public ReadOnly Property SelectionContainer As IRawElementProviderSimple Implements ISelectionItemProvider.SelectionContainer
            Get
                Dim b = Barre
                Return If(b Is Nothing, Nothing, ProviderFromPeer(UIElementAutomationPeer.CreatePeerForElement(b)))
            End Get
        End Property

        ''' <summary>Affiche la section.</summary>
        Public Sub [Select]() Implements ISelectionItemProvider.Select
            If Not IsEnabled() Then Throw New ElementNotEnabledException()
            Dim b = Barre
            If b IsNot Nothing Then b.SelectedItem = Section
        End Sub

        ''' <summary>Une seule section à la fois : l'ajouter, c'est l'afficher.</summary>
        Public Sub AddToSelection() Implements ISelectionItemProvider.AddToSelection
            [Select]()
        End Sub

        ''' <summary>Une section affichée ne se retire pas : on en affiche une autre.</summary>
        Public Sub RemoveFromSelection() Implements ISelectionItemProvider.RemoveFromSelection
            If IsSelected Then Throw New InvalidOperationException()
        End Sub

    End Class

End Namespace
