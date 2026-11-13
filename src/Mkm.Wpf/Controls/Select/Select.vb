Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La liste déroulante de mkm (<c>Select</c>), au rendu de 3DEXPERIENCE : un champ de 24 px et, à sa
    ''' droite, un bouton gris de 22 px qui porte la flèche ; la liste s'ouvre 4 px dessous, ses choix de
    ''' 22 px séparés d'un trait, le choix actuel cerné. <see cref="Placeholder"/> et <see cref="Invalid"/>
    ''' comme <see cref="InputText"/>. Comme dans mkm, la liste ne sort pas de sa page, ici sa fenêtre : elle s'ouvre
    ''' dessous si elle y tient, sinon du côté qui a le plus de place (dessus, près du bas de la fenêtre) ; sa hauteur
    ''' s'arrête au bord de la fenêtre (et à <c>MaxDropDownHeight</c>), le reste défile.
    ''' </summary>
    ''' <remarks>
    ''' C'est un <see cref="ComboBox"/> : <c>IsEditable</c> (une valeur hors de la liste), la recherche au
    ''' clavier, F4 et Alt+Bas, l'accessibilité sont ceux de WPF. La liste est virtualisée : seuls les choix
    ''' visibles existent, si bien qu'une liste de milliers de valeurs s'ouvre aussitôt. « Select » est un mot
    ''' réservé de VB : la classe s'écrit <c>[Select]</c> en VB, <c>mkm:Select</c> en XAML.
    ''' </remarks>
    Public Class [Select]
        Inherits ComboBox

        ''' <summary>Sans libellé, son texte indicatif le nomme pour les lecteurs d'écran.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeListe(Me)
        End Function

        ''' <summary>Le texte d'aide, montré quand rien n'est choisi.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Placeholder), GetType(String), GetType([Select]), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai quand la valeur est fausse : le champ prend la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType([Select]), New FrameworkPropertyMetadata(False))

        Private Shared ReadOnly DropDownHeightLimitKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(DropDownHeightLimit), GetType(Double), GetType([Select]), New FrameworkPropertyMetadata(Double.PositiveInfinity))

        ''' <summary>La hauteur que la liste peut prendre : la place dans la fenêtre, du côté où elle s'ouvre.</summary>
        Public Shared ReadOnly DropDownHeightLimitProperty As DependencyProperty = DropDownHeightLimitKey.DependencyProperty

        Private Shared ReadOnly OpensUpwardKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(OpensUpward), GetType(Boolean), GetType([Select]), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai quand la liste s'ouvre au-dessus du champ.</summary>
        Public Shared ReadOnly OpensUpwardProperty As DependencyProperty = OpensUpwardKey.DependencyProperty

        ' La marge gardée entre la liste et le bord de la fenêtre, et la hauteur sous laquelle la liste ne descend pas.
        Private Const MargeAuBord As Double = 4
        Private Const HauteurMinimale As Double = 60

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType([Select]), New FrameworkPropertyMetadata(GetType([Select])))
        End Sub

        Private _defilement As FermetureAuDefilement

        ''' <summary>
        ''' Ouverte, la liste se referme quand la page défile, comme dans mkm ; elle se place du côté où elle tient dans la
        ''' fenêtre, puis se replace une fois ses choix dessinés.
        ''' </summary>
        Protected Overrides Sub OnDropDownOpened(e As System.EventArgs)
            MyBase.OnDropDownOpened(e)
            If _defilement Is Nothing Then _defilement = New FermetureAuDefilement(Me, Sub() SetCurrentValue(IsDropDownOpenProperty, False))
            _defilement.Ecouter()
            PlacerDansLaFenetre()
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, New Action(AddressOf PlacerDansLaFenetre))
        End Sub

        ''' <summary>La hauteur que la liste peut prendre, du côté où elle s'ouvre ; calculée à chaque ouverture.</summary>
        Public ReadOnly Property DropDownHeightLimit As Double
            Get
                Return CDbl(GetValue(DropDownHeightLimitProperty))
            End Get
        End Property

        ''' <summary>Vrai quand la liste s'ouvre au-dessus du champ (pas la place dessous, dans sa fenêtre).</summary>
        Public ReadOnly Property OpensUpward As Boolean
            Get
                Return CBool(GetValue(OpensUpwardProperty))
            End Get
        End Property

        ' La liste dessous si elle y tient dans la fenêtre, sinon du côté qui a le plus de place ; sa hauteur s'arrête au bord
        ' de la fenêtre. Hors d'une fenêtre (ou avant d'être dessinée), elle reste dessous, à MaxDropDownHeight.
        Private Sub PlacerDansLaFenetre()
            Dim liste = TryCast(GetTemplateChild("PART_Popup"), Popup)
            Dim zone = ZoneDeLaFenetre()
            If liste Is Nothing OrElse liste.Child Is Nothing OrElse zone Is Nothing OrElse Not IsDropDownOpen Then
                SetValue(DropDownHeightLimitKey, MaxDropDownHeight)
                Return
            End If
            Dim haut As Double
            Try
                haut = TransformToAncestor(zone).Transform(New Point(0, 0)).Y
            Catch ex As InvalidOperationException
                Return
            End Try
            Dim ecart = If(TypeOf TryFindResource("mkm.select.overlay.gutter") Is Double, CDbl(TryFindResource("mkm.select.overlay.gutter")), 4.0)
            Dim dessous = zone.ActualHeight - (haut + ActualHeight) - ecart - MargeAuBord
            Dim dessus = haut - ecart - MargeAuBord
            ' La hauteur que la liste voudrait (au plus MaxDropDownHeight), et ce qui l'entoure (marges, ombre).
            Dim defilement = TryCast(GetTemplateChild("PART_Defilement"), FrameworkElement)
            SetValue(DropDownHeightLimitKey, MaxDropDownHeight)
            liste.Child.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
            Dim voulue = liste.Child.DesiredSize.Height
            Dim autour = If(defilement Is Nothing, 0, Math.Max(0, voulue - defilement.DesiredSize.Height))
            Dim enHaut = voulue > dessous AndAlso dessus > dessous
            SetValue(OpensUpwardKey, enHaut)
            liste.Placement = If(enHaut, PlacementMode.Top, PlacementMode.Bottom)
            ' Ouverte dessus, le bas de la liste (son ombre comprise) se pose sur le champ : l'écart, moins l'ombre.
            liste.VerticalOffset = If(enHaut, -ecart + EpaisseurDeLOmbre(liste), ecart)
            SetValue(DropDownHeightLimitKey, Math.Max(HauteurMinimale, Math.Min(MaxDropDownHeight, If(enHaut, dessus, dessous) - autour)))
        End Sub

        ' L'épaisseur de l'ombre sous la liste (la marge basse de son cadre), que la liste ouverte dessus fait chevaucher le champ.
        Private Shared Function EpaisseurDeLOmbre(liste As Popup) As Double
            Dim cadre = TryCast(liste.Child, FrameworkElement)
            Return If(cadre Is Nothing, 0, cadre.Margin.Bottom)
        End Function

        ' Le contenu de la fenêtre (sa zone cliente) ; rien hors d'une fenêtre.
        Private Function ZoneDeLaFenetre() As FrameworkElement
            Dim fenetre = Window.GetWindow(Me)
            If fenetre Is Nothing Then Return Nothing
            Return If(TryCast(fenetre.Content, FrameworkElement), fenetre)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Sub OnDropDownClosed(e As System.EventArgs)
            MyBase.OnDropDownClosed(e)
            _defilement?.Cesser()
        End Sub

        ''' <summary>Le texte d'aide.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la valeur est fausse.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

    End Class

End Namespace
