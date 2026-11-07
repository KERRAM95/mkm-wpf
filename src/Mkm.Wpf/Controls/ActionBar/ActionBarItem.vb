Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>Une commande d'<see cref="ActionBar"/> que la barre sait lancer (la commande montrée d'une liste).</summary>
    Friend Interface IActionBarCommand

        ''' <summary>Lance la commande, comme un clic.</summary>
        Sub PerformClick()

    End Interface

    ''' <summary>
    ''' Ce que partagent les commandes d'une <see cref="ActionBar"/> : le libellé, l'icône, et l'indice
    ''' que l'info-bulle ajoute (ce qu'il faut, par exemple, pour que la commande soit disponible).
    ''' </summary>
    ''' <remarks>
    ''' L'info-bulle suit mkm : « libellé — indice », ou l'indice seul quand les libellés sont montrés.
    ''' Une commande grisée la garde : elle dit ce qui lui manque. Le libellé nomme aussi la commande pour
    ''' l'accessibilité, l'indice la décrit.
    ''' </remarks>
    Public NotInheritable Class ActionBarItem

        ''' <summary>Le libellé de la commande.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Label", GetType(String), GetType(ActionBarItem), New FrameworkPropertyMetadata(Nothing, AddressOf Rafraichir))

        ''' <summary>L'icône : une géométrie de <see cref="Icons"/> ou une image.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Icon", GetType(Object), GetType(ActionBarItem), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le texte que l'info-bulle ajoute au libellé.</summary>
        Public Shared ReadOnly HintProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Hint", GetType(String), GetType(ActionBarItem), New FrameworkPropertyMetadata(Nothing, AddressOf Rafraichir))

        Private Sub New()
        End Sub

        ''' <summary>Le libellé d'une commande.</summary>
        Public Shared Function GetLabel(element As DependencyObject) As String
            Return CStr(element.GetValue(LabelProperty))
        End Function

        ''' <summary>Pose le libellé d'une commande.</summary>
        Public Shared Sub SetLabel(element As DependencyObject, value As String)
            element.SetValue(LabelProperty, value)
        End Sub

        ''' <summary>L'icône d'une commande.</summary>
        Public Shared Function GetIcon(element As DependencyObject) As Object
            Return element.GetValue(IconProperty)
        End Function

        ''' <summary>Pose l'icône d'une commande.</summary>
        Public Shared Sub SetIcon(element As DependencyObject, value As Object)
            element.SetValue(IconProperty, value)
        End Sub

        ''' <summary>L'indice d'une commande.</summary>
        Public Shared Function GetHint(element As DependencyObject) As String
            Return CStr(element.GetValue(HintProperty))
        End Function

        ''' <summary>Pose l'indice d'une commande.</summary>
        Public Shared Sub SetHint(element As DependencyObject, value As String)
            element.SetValue(HintProperty, value)
        End Sub

        ''' <summary>Le texte de l'info-bulle, comme mkm ; <c>Nothing</c> s'il n'y en a pas.</summary>
        Friend Shared Function TexteDeLaBulle(element As DependencyObject) As String
            Dim libelle = GetLabel(element)
            Dim indice = GetHint(element)
            If ActionBar.GetShowLabels(element) Then Return If(String.IsNullOrEmpty(indice), Nothing, indice)
            If String.IsNullOrEmpty(indice) Then Return If(String.IsNullOrEmpty(libelle), Nothing, libelle)
            Return If(String.IsNullOrEmpty(libelle), indice, libelle & " — " & indice)
        End Function

        ''' <summary>Met à jour l'info-bulle et le nom accessible d'une commande.</summary>
        Friend Shared Sub Rafraichir(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim element = TryCast(d, FrameworkElement)
            If element Is Nothing OrElse Not (TypeOf element Is ButtonBase) Then Return

            AutomationProperties.SetName(element, If(GetLabel(element), String.Empty))
            AutomationProperties.SetHelpText(element, If(GetHint(element), String.Empty))

            Dim texte = TexteDeLaBulle(element)
            If texte Is Nothing Then
                element.ClearValue(FrameworkElement.ToolTipProperty)
                Return
            End If

            Dim bulle = TryCast(element.ToolTip, ToolTip)
            If bulle Is Nothing Then
                bulle = New ToolTip()
                bulle.SetResourceReference(FrameworkElement.StyleProperty, ActionBar.ToolTipStyleKey)
                element.ToolTip = bulle
                ToolTipService.SetShowOnDisabled(element, True)
            End If
            bulle.Content = texte
        End Sub

        ''' <summary>
        ''' Enfoncée, une commande de la barre rétrécit à l'échelle du thème
        ''' (<c>mkm.actionbar.item.pressed.scale</c>) ; une ligne de liste, non.
        ''' </summary>
        Friend Shared Sub Presser(element As FrameworkElement, enfoncee As Boolean)
            Dim echelle = element.TryFindResource("mkm.actionbar.item.pressed.scale")
            If enfoncee AndAlso TypeOf echelle Is Double AndAlso Not ActionBarDropDown.GetIsInList(element) Then
                element.RenderTransformOrigin = New Point(0.5, 0.5)
                element.RenderTransform = New Media.ScaleTransform(CDbl(echelle), CDbl(echelle))
            Else
                element.ClearValue(UIElement.RenderTransformProperty)
            End If
        End Sub

        ''' <summary>Au clavier, l'info-bulle suit le focus ; WPF ne l'ouvre qu'au survol.</summary>
        Friend Shared Sub OuvrirAuClavier(element As FrameworkElement)
            If Not (InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice) Then Return
            Dim bulle = TryCast(element.ToolTip, ToolTip)
            If bulle Is Nothing Then Return
            bulle.PlacementTarget = element
            bulle.IsOpen = True
        End Sub

        ''' <summary>Ferme l'info-bulle ouverte par le clavier.</summary>
        Friend Shared Sub Fermer(element As FrameworkElement)
            Dim bulle = TryCast(element.ToolTip, ToolTip)
            If bulle IsNot Nothing AndAlso bulle.IsOpen Then bulle.IsOpen = False
        End Sub

    End Class

End Namespace
