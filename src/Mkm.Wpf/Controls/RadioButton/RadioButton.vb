Imports System.Windows
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>La taille d'un <see cref="RadioButton"/>, comme dans mkm.</summary>
    Public Enum RadioButtonSize
        ''' <summary>Un rond de 15 px, un point de 9 (par défaut).</summary>
        Normal
        ''' <summary>Un rond de 12 px, un point de 6.</summary>
        Small
        ''' <summary>Un rond de 18 px, un point de 12.</summary>
        Large
    End Enum

    ''' <summary>
    ''' Le bouton radio de mkm : un rond de 15 px, blanc, bordé du gris des champs ; choisi, un point de 9 px de la
    ''' couleur principale ; survolé, sa bordure fonce ; au clavier, la bordure du focus et l'anneau ; grisé, le fond et
    ''' le point des champs grisés. Son libellé (<c>Content</c>) fait partie de lui, à 6 px : il l'allume au survol.
    ''' <code>
    ''' &lt;mkm:RadioButton GroupName="vue" Content="Plan" IsChecked="{Binding VuePlan}" /&gt;
    ''' &lt;mkm:RadioButton GroupName="vue" Content="Article" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' C'est un <see cref="System.Windows.Controls.RadioButton"/> : <c>GroupName</c>, le clavier et l'accessibilité de
    ''' WPF. Le groupe de mkm (<c>RadioButtonGroup</c>) n'a pas d'équivalent ici : <c>GroupName</c>, ou un même parent,
    ''' en tient lieu. « RadioButton » est aussi le nom de celui de WPF : qualifier (<c>Mkm.Wpf.Controls.RadioButton</c>)
    ''' là où les deux espaces de noms sont importés.
    ''' </remarks>
    Public Class RadioButton
        Inherits System.Windows.Controls.RadioButton

        ''' <summary>Vrai quand le choix est refusé : le rond prend la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = InputText.InvalidProperty.AddOwner(GetType(RadioButton))

        ''' <summary>La taille du rond et de son point.</summary>
        Public Shared ReadOnly SizeProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Size), GetType(RadioButtonSize), GetType(RadioButton), New FrameworkPropertyMetadata(RadioButtonSize.Normal))

        Private Shared ReadOnly IsFocusVisiblePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsFocusVisible), GetType(Boolean), GetType(RadioButton), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand le bouton a le focus du clavier, et non celui d'un clic : l'anneau du focus paraît.</summary>
        Public Shared ReadOnly IsFocusVisibleProperty As DependencyProperty = IsFocusVisiblePropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(RadioButton), New FrameworkPropertyMetadata(GetType(RadioButton)))
        End Sub

        ''' <summary>Vrai quand le choix est refusé.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

        ''' <summary>La taille du rond et de son point.</summary>
        Public Property Size As RadioButtonSize
            Get
                Return DirectCast(GetValue(SizeProperty), RadioButtonSize)
            End Get
            Set(value As RadioButtonSize)
                SetValue(SizeProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand le bouton a le focus du clavier, et non celui d'un clic.</summary>
        Public ReadOnly Property IsFocusVisible As Boolean
            Get
                Return CBool(GetValue(IsFocusVisibleProperty))
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Sub OnGotKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnGotKeyboardFocus(e)
            SetValue(IsFocusVisiblePropertyKey, InputManager.Current.MostRecentInputDevice Is Keyboard.PrimaryDevice)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnLostKeyboardFocus(e)
            SetValue(IsFocusVisiblePropertyKey, False)
        End Sub

        ''' <summary>
        ''' Les flèches, comme mkm et Windows : → et ↓ choisissent le bouton suivant du groupe, ← et ↑ le précédent, du
        ''' dernier on revient au premier ; le choisi prend le clavier.
        ''' </summary>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Handled Then Return
            Dim sens As Integer
            Select Case e.Key
                Case Key.Right, Key.Down : sens = 1
                Case Key.Left, Key.Up : sens = -1
                Case Else : Return
            End Select
            Dim groupe = BoutonsDuGroupe()
            Dim ici = groupe.IndexOf(Me)
            If groupe.Count < 2 OrElse ici < 0 Then Return
            Dim voisin = groupe((ici + sens + groupe.Count) Mod groupe.Count)
            voisin.Focus()
            voisin.IsChecked = True
            e.Handled = True
        End Sub

        ' Les boutons du même groupe qu'on peut choisir, dans l'ordre de l'écran : même GroupName dans la même fenêtre, ou,
        ' sans GroupName, les frères du même panneau (la règle de WPF).
        Private Function BoutonsDuGroupe() As System.Collections.Generic.List(Of RadioButton)
            Dim racine As DependencyObject
            If String.IsNullOrEmpty(GroupName) Then
                racine = System.Windows.Media.VisualTreeHelper.GetParent(Me)
            Else
                racine = Me
                Dim parent = System.Windows.Media.VisualTreeHelper.GetParent(racine)
                While parent IsNot Nothing
                    racine = parent
                    parent = System.Windows.Media.VisualTreeHelper.GetParent(racine)
                End While
            End If
            Dim boutons As New System.Collections.Generic.List(Of RadioButton)()
            If racine IsNot Nothing Then Rassembler(racine, String.IsNullOrEmpty(GroupName), boutons)
            Return boutons
        End Function

        Private Sub Rassembler(parent As DependencyObject, freresSeuls As Boolean, boutons As System.Collections.Generic.List(Of RadioButton))
            For i = 0 To System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim enfant = System.Windows.Media.VisualTreeHelper.GetChild(parent, i)
                Dim bouton = TryCast(enfant, RadioButton)
                If bouton IsNot Nothing Then
                    If String.Equals(If(bouton.GroupName, ""), If(GroupName, ""), StringComparison.Ordinal) AndAlso
                       (bouton Is Me OrElse (bouton.IsEnabled AndAlso bouton.IsVisible)) Then boutons.Add(bouton)
                ElseIf Not freresSeuls Then
                    Rassembler(enfant, freresSeuls, boutons)
                End If
            Next
        End Sub

    End Class

End Namespace
