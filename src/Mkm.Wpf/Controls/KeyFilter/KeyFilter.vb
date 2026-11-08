Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le filtre de touches de mkm (<c>v-keyfilter</c>) : posé sur un champ, il refuse les caractères qui ne conviennent
    ''' pas, à la frappe comme au collage.
    ''' <code>
    ''' &lt;mkm:InputText mkm:KeyFilter.Pattern="pint" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' <see cref="PatternProperty"/> : un des filtres de mkm, <c>pint</c> (chiffres), <c>int</c> (chiffres et moins),
    ''' <c>pnum</c> (chiffres et point), <c>num</c> (chiffres, moins et point), <c>money</c> (chiffres, point, espace et
    ''' virgule), <c>hex</c>, <c>email</c>, <c>alpha</c> (lettres et soulignés), <c>alphanum</c> ; ou une expression
    ''' régulière, que chaque caractère doit satisfaire (« [A-Z] »).
    ''' </remarks>
    Public NotInheritable Class KeyFilter

        ''' <summary>Le filtre du champ : un nom de filtre de mkm, ou une expression régulière pour chaque caractère.</summary>
        Public Shared ReadOnly PatternProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "Pattern", GetType(String), GetType(KeyFilter), New FrameworkPropertyMetadata(Nothing, AddressOf OnPatternChanged))

        Private Shared ReadOnly Filtres As New Dictionary(Of String, Regex)(StringComparer.Ordinal) From {
            {"pint", New Regex("[\d]")},
            {"int", New Regex("[\d\-]")},
            {"pnum", New Regex("[\d.]")},
            {"money", New Regex("[\d.\s,]")},
            {"num", New Regex("[\d\-.]")},
            {"hex", New Regex("[0-9a-f]", RegexOptions.IgnoreCase)},
            {"email", New Regex("[a-z0-9_.\-@]", RegexOptions.IgnoreCase)},
            {"alpha", New Regex("[a-z_]", RegexOptions.IgnoreCase)},
            {"alphanum", New Regex("[a-z0-9_]")}}

        Private Sub New()
        End Sub

        ''' <summary>Le filtre du champ.</summary>
        Public Shared Function GetPattern(element As DependencyObject) As String
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return CStr(element.GetValue(PatternProperty))
        End Function

        ''' <summary>Pose le filtre du champ.</summary>
        Public Shared Sub SetPattern(element As DependencyObject, value As String)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(PatternProperty, value)
        End Sub

        ''' <summary>L'expression d'un filtre : celle de mkm pour un nom connu, sinon le filtre lui-même ; Nothing sans filtre
        ''' ou pour une expression fausse.</summary>
        Friend Shared Function Expression(filtre As String) As Regex
            If String.IsNullOrEmpty(filtre) Then Return Nothing
            Dim connue As Regex = Nothing
            If Filtres.TryGetValue(filtre, connue) Then Return connue
            Try
                Return New Regex(filtre)
            Catch ex As ArgumentException
                Return Nothing
            End Try
        End Function

        ''' <summary>Vrai quand chaque caractère du texte passe le filtre.</summary>
        Friend Shared Function Accepte(filtre As String, texte As String) As Boolean
            Dim motif = Expression(filtre)
            If motif Is Nothing OrElse String.IsNullOrEmpty(texte) Then Return True
            For Each c In texte
                If Not motif.IsMatch(c.ToString()) Then Return False
            Next
            Return True
        End Function

        Private Shared Sub OnPatternChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = TryCast(d, UIElement)
            If champ Is Nothing Then Return
            RemoveHandler champ.PreviewTextInput, AddressOf Frappe
            RemoveHandler champ.PreviewKeyDown, AddressOf Espace
            DataObject.RemovePastingHandler(champ, AddressOf Collage)
            If String.IsNullOrEmpty(CStr(e.NewValue)) Then Return
            AddHandler champ.PreviewTextInput, AddressOf Frappe
            AddHandler champ.PreviewKeyDown, AddressOf Espace
            DataObject.AddPastingHandler(champ, AddressOf Collage)
        End Sub

        ' L'Espace : le TextBox de WPF l'écrit par une commande, sans PreviewTextInput ; il passe donc ici au filtre.
        Private Shared Sub Espace(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Space AndAlso Keyboard.Modifiers = ModifierKeys.None AndAlso
               Not Accepte(GetPattern(DirectCast(sender, DependencyObject)), " ") Then e.Handled = True
        End Sub

        Private Shared Sub Frappe(sender As Object, e As TextCompositionEventArgs)
            If Not Accepte(GetPattern(DirectCast(sender, DependencyObject)), e.Text) Then e.Handled = True
        End Sub

        ' Un texte collé passe en entier, ou pas du tout, comme mkm.
        Private Shared Sub Collage(sender As Object, e As DataObjectPastingEventArgs)
            Dim texte = TryCast(e.DataObject.GetData(DataFormats.UnicodeText), String)
            If Not Accepte(GetPattern(DirectCast(sender, DependencyObject)), texte) Then e.CancelCommand()
        End Sub

    End Class

End Namespace
