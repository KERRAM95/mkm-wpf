Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La liste de mkm (Listbox) : des options dans un cadre de champ, celle choisie sur le fond du thème. Une
    ''' coche devant l'option choisie (<c>Checkmark</c>), une ligne sur deux grisée (<c>Striped</c>), un champ de
    ''' filtre en tête (<c>Filter</c>). Plusieurs choix : <c>SelectionMode="Multiple"</c>, chaque clic ajoute ou retire.
    ''' <code>
    ''' &lt;mkm:Listbox Height="160"&gt;
    '''     &lt;ListBoxItem Content="Private" /&gt;
    '''     &lt;ListBoxItem Content="In Work" /&gt;
    ''' &lt;/mkm:Listbox&gt;
    ''' </code>
    ''' « Listbox » est aussi le nom de la liste de WPF (ListBox) : VB ne distingue pas la casse.
    ''' </summary>
    <TemplatePart(Name:="PART_Filter", Type:=GetType(TextBox))>
    Public Class Listbox
        Inherits System.Windows.Controls.ListBox

        ''' <summary>Vrai pour une coche devant l'option choisie (et la place de la coche devant les autres).</summary>
        Public Shared ReadOnly CheckmarkProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Checkmark), GetType(Boolean), GetType(Listbox), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai pour une option sur deux grisée.</summary>
        Public Shared ReadOnly StripedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Striped), GetType(Boolean), GetType(Listbox), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai pour un champ de filtre en tête de la liste.</summary>
        Public Shared ReadOnly FilterProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Filter), GetType(Boolean), GetType(Listbox), New FrameworkPropertyMetadata(False))

        ''' <summary>Le texte du filtre : seules restent les options qui le contiennent, sans égard à la casse.</summary>
        Public Shared ReadOnly FilterTextProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(FilterText), GetType(String), GetType(Listbox),
            New FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnFilterTextChanged))

        ''' <summary>Le texte d'aide du champ de filtre.</summary>
        Public Shared ReadOnly FilterPlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(FilterPlaceholder), GetType(String), GetType(Listbox), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le message d'une liste vide ; sinon celui de la langue (« Aucune option disponible », « Aucun résultat »).</summary>
        Public Shared ReadOnly EmptyMessageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(EmptyMessage), GetType(String), GetType(Listbox), New FrameworkPropertyMetadata(Nothing,
                Sub(d, e) d.SetValue(HasEmptyMessagePropertyKey, Not String.IsNullOrEmpty(CStr(e.NewValue)))))

        ''' <summary>Vrai quand la valeur est refusée : la bordure rouge.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType(Listbox), New FrameworkPropertyMetadata(False))

        Private Shared ReadOnly HasEmptyMessagePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasEmptyMessage), GetType(Boolean), GetType(Listbox), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand la liste a son propre message de liste vide.</summary>
        Public Shared ReadOnly HasEmptyMessageProperty As DependencyProperty = HasEmptyMessagePropertyKey.DependencyProperty

        Private Shared ReadOnly IsEmptyPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsEmpty), GetType(Boolean), GetType(Listbox), New FrameworkPropertyMetadata(True))
        ''' <summary>Vrai quand aucune option n'est montrée : la liste dit son message.</summary>
        Public Shared ReadOnly IsEmptyProperty As DependencyProperty = IsEmptyPropertyKey.DependencyProperty

        Private _filtre As TextBox

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Listbox), New FrameworkPropertyMetadata(GetType(Listbox)))
        End Sub

        ''' <summary>Une liste vide ; ses groupes (une vue groupée) ont l'en-tête de mkm.</summary>
        Public Sub New()
            GroupStyle.Add(New GroupStyle())
        End Sub

        ''' <summary>Vrai pour une coche devant l'option choisie.</summary>
        Public Property Checkmark As Boolean
            Get
                Return CBool(GetValue(CheckmarkProperty))
            End Get
            Set(value As Boolean)
                SetValue(CheckmarkProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour une option sur deux grisée.</summary>
        Public Property Striped As Boolean
            Get
                Return CBool(GetValue(StripedProperty))
            End Get
            Set(value As Boolean)
                SetValue(StripedProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour un champ de filtre en tête.</summary>
        Public Property Filter As Boolean
            Get
                Return CBool(GetValue(FilterProperty))
            End Get
            Set(value As Boolean)
                SetValue(FilterProperty, value)
            End Set
        End Property

        ''' <summary>Le texte du filtre.</summary>
        Public Property FilterText As String
            Get
                Return CStr(GetValue(FilterTextProperty))
            End Get
            Set(value As String)
                SetValue(FilterTextProperty, value)
            End Set
        End Property

        ''' <summary>Le texte d'aide du champ de filtre.</summary>
        Public Property FilterPlaceholder As String
            Get
                Return CStr(GetValue(FilterPlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(FilterPlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Le message d'une liste vide.</summary>
        Public Property EmptyMessage As String
            Get
                Return CStr(GetValue(EmptyMessageProperty))
            End Get
            Set(value As String)
                SetValue(EmptyMessageProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la valeur est refusée.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la liste a son propre message de liste vide.</summary>
        Public ReadOnly Property HasEmptyMessage As Boolean
            Get
                Return CBool(GetValue(HasEmptyMessageProperty))
            End Get
        End Property

        ''' <summary>Vrai quand aucune option n'est montrée.</summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return CBool(GetValue(IsEmptyProperty))
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            If _filtre IsNot Nothing Then RemoveHandler _filtre.PreviewKeyDown, AddressOf Filtre_PreviewKeyDown
            MyBase.OnApplyTemplate()
            _filtre = TryCast(GetTemplateChild("PART_Filter"), TextBox)
            If _filtre IsNot Nothing Then AddHandler _filtre.PreviewKeyDown, AddressOf Filtre_PreviewKeyDown
            Dim entete = TryCast(TryFindResource("mkm.Listbox.Group"), DataTemplate)
            If entete IsNot Nothing AndAlso GroupStyle.Count > 0 AndAlso GroupStyle(0).HeaderTemplate Is Nothing Then GroupStyle(0).HeaderTemplate = entete
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsChanged(e As Specialized.NotifyCollectionChangedEventArgs)
            MyBase.OnItemsChanged(e)
            ' Une option écrite choisie (IsSelected="True") l'est dès son arrivée : la liste d'un MultiSelect, dans sa
            ' fenêtre fermée, ne crée ses options qu'à l'ouverture.
            If e.NewItems IsNot Nothing Then
                For Each choisie In e.NewItems.OfType(Of ListBoxItem)().Where(Function(o) o.IsSelected)
                    If SelectionMode = SelectionMode.Single Then
                        SetCurrentValue(SelectedItemProperty, choisie)
                    ElseIf Not SelectedItems.Contains(choisie) Then
                        SelectedItems.Add(choisie)
                    End If
                Next
            End If
            MettreAJourLeVide()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnItemsSourceChanged(oldValue As Collections.IEnumerable, newValue As Collections.IEnumerable)
            MyBase.OnItemsSourceChanged(oldValue, newValue)
            Filtrer()
        End Sub

        Private Shared Sub OnFilterTextChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, Listbox).Filtrer()
        End Sub

        ' Seules restent les options dont le texte contient le filtre (filterMatchMode « contains » de mkm).
        Private Sub Filtrer()
            Dim texte = FilterText
            Items.Filter = If(String.IsNullOrEmpty(texte), Nothing,
                              New Predicate(Of Object)(Function(o) TexteDe(o).IndexOf(texte, StringComparison.CurrentCultureIgnoreCase) >= 0))
            MettreAJourLeVide()
        End Sub

        ''' <summary>Le texte d'une option : son contenu, sa propriété <c>DisplayMemberPath</c>, sinon son texte.</summary>
        Friend Function TexteDe(choix As Object) As String
            Dim element = TryCast(choix, ContentControl)
            If element IsNot Nothing Then Return If(element.Content?.ToString(), String.Empty)
            If choix IsNot Nothing AndAlso Not String.IsNullOrEmpty(DisplayMemberPath) Then
                Dim propriete = TypeDescriptor.GetProperties(choix).Find(DisplayMemberPath, True)
                If propriete IsNot Nothing Then Return If(propriete.GetValue(choix)?.ToString(), String.Empty)
            End If
            Return If(choix?.ToString(), String.Empty)
        End Function

        Private Sub MettreAJourLeVide()
            SetValue(IsEmptyPropertyKey, Items.Count = 0)
        End Sub

        ''' <summary>Une liste pour les lecteurs d'écran : son champ de filtre, puis ses options.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeListbox(Me)
        End Function

        ' Le pair d'un ListBox n'expose que ses options : le champ du filtre, partie du gabarit, passe devant elles.
        Private NotInheritable Class PairDeListbox
            Inherits ListBoxAutomationPeer

            Public Sub New(liste As Listbox)
                MyBase.New(liste)
            End Sub

            Protected Overrides Function GetChildrenCore() As List(Of AutomationPeer)
                Dim enfants = MyBase.GetChildrenCore()
                Dim filtre = DirectCast(Owner, Listbox)._filtre
                If filtre Is Nothing OrElse Not filtre.IsVisible Then Return enfants
                Dim pair = UIElementAutomationPeer.CreatePeerForElement(filtre)
                If pair Is Nothing Then Return enfants
                If enfants Is Nothing Then enfants = New List(Of AutomationPeer)()
                enfants.Insert(0, pair)
                Return enfants
            End Function

        End Class

        ' ↓ dans le filtre passe aux options, comme mkm.
        Private Sub Filtre_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            ToucheDuFiltre(e)
        End Sub

        ' ↓ dans le filtre : le clavier passe à l'option choisie, sinon à la première.
        Friend Overridable Sub ToucheDuFiltre(e As KeyEventArgs)
            If e.Key <> Key.Down OrElse Items.Count = 0 Then Return
            Dim indice = If(SelectedIndex >= 0, SelectedIndex, 0)
            TryCast(ItemContainerGenerator.ContainerFromIndex(indice), ListBoxItem)?.Focus()
            e.Handled = True
        End Sub

    End Class

End Namespace
