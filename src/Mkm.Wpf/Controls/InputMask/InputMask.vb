Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le champ masqué de mkm (<c>InputMask</c>) : un champ qui suit un modèle, « 99/99/9999 » pour une date ; les cases à
    ''' remplir se montrent (<c>_</c>), les séparateurs s'écrivent seuls.
    ''' <code>
    ''' &lt;mkm:InputMask Mask="HS-99999" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Dans <see cref="Mask"/> : <c>9</c> un chiffre, <c>a</c> une lettre, <c>*</c> une lettre ou un chiffre ; tout autre
    ''' caractère s'écrit tel quel ; ce qui suit <c>?</c> est facultatif. <c>Text</c> est le texte montré,
    ''' <see cref="UnmaskedValue"/> les seuls caractères saisis. L'algorithme est celui de mkm (jQuery Masked Input).
    ''' </remarks>
    Public Class InputMask
        Inherits InputText

        ''' <summary>Le modèle : <c>9</c>, <c>a</c>, <c>*</c>, les séparateurs, <c>?</c> devant la partie facultative.</summary>
        Public Shared ReadOnly MaskProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Mask), GetType(String), GetType(InputMask), New FrameworkPropertyMetadata(Nothing, AddressOf OnMaskChanged))

        ''' <summary>Le caractère d'une case vide : <c>_</c> par défaut.</summary>
        Public Shared ReadOnly SlotCharProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(SlotChar), GetType(String), GetType(InputMask), New FrameworkPropertyMetadata("_", AddressOf OnMaskChanged))

        ''' <summary>Vrai (par défaut) pour vider, en quittant le champ, une saisie incomplète.</summary>
        Public Shared ReadOnly AutoClearProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(AutoClear), GetType(Boolean), GetType(InputMask), New FrameworkPropertyMetadata(True))

        Private Shared ReadOnly UnmaskedValuePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(UnmaskedValue), GetType(String), GetType(InputMask), New FrameworkPropertyMetadata(String.Empty))
        ''' <summary>Les seuls caractères saisis, sans séparateurs ni cases vides.</summary>
        Public Shared ReadOnly UnmaskedValueProperty As DependencyProperty = UnmaskedValuePropertyKey.DependencyProperty

        Private Shared ReadOnly IsCompletePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsComplete), GetType(Boolean), GetType(InputMask), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand toutes les cases obligatoires sont remplies.</summary>
        Public Shared ReadOnly IsCompleteProperty As DependencyProperty = IsCompletePropertyKey.DependencyProperty

        ' Le modèle analysé : une expression par case (Nothing pour un séparateur), la place de la partie facultative.
        Private _cases As Regex() = {}
        Private _modele As Char() = {}
        Private _tampon As Char() = {}
        Private _facultatif As Integer
        Private _premiereCase As Integer
        Private _ecriture As Boolean

        Private Shared ReadOnly Definitions As New Dictionary(Of Char, Regex) From {
            {"9"c, New Regex("[0-9]")}, {"a"c, New Regex("[A-Za-z]")}, {"*"c, New Regex("[A-Za-z0-9]")}}

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(InputMask), New FrameworkPropertyMetadata(GetType(InputMask)))
        End Sub

        Public Sub New()
            DataObject.AddPastingHandler(Me, AddressOf Coller)
            CommandManager.AddPreviewExecutedHandler(Me, AddressOf Commande)
        End Sub

        ''' <summary>Le modèle.</summary>
        Public Property Mask As String
            Get
                Return CStr(GetValue(MaskProperty))
            End Get
            Set(value As String)
                SetValue(MaskProperty, value)
            End Set
        End Property

        ''' <summary>Le caractère d'une case vide.</summary>
        Public Property SlotChar As String
            Get
                Return CStr(GetValue(SlotCharProperty))
            End Get
            Set(value As String)
                SetValue(SlotCharProperty, value)
            End Set
        End Property

        ''' <summary>Vrai pour vider une saisie incomplète en quittant le champ.</summary>
        Public Property AutoClear As Boolean
            Get
                Return CBool(GetValue(AutoClearProperty))
            End Get
            Set(value As Boolean)
                SetValue(AutoClearProperty, value)
            End Set
        End Property

        ''' <summary>Les seuls caractères saisis.</summary>
        Public ReadOnly Property UnmaskedValue As String
            Get
                Return CStr(GetValue(UnmaskedValueProperty))
            End Get
        End Property

        ''' <summary>Vrai quand toutes les cases obligatoires sont remplies.</summary>
        Public ReadOnly Property IsComplete As Boolean
            Get
                Return CBool(GetValue(IsCompleteProperty))
            End Get
        End Property

        Private ReadOnly Property Longueur As Integer
            Get
                Return _cases.Length
            End Get
        End Property

        Private Function Vide(i As Integer) As Char
            Dim cases = If(SlotChar, "_")
            If cases.Length = 0 Then Return " "c
            Return If(i < cases.Length, cases(i), cases(0))
        End Function

        Private Shared Sub OnMaskChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim champ = DirectCast(d, InputMask)
            champ.Analyser()
            champ.Ajuster(champ.IsKeyboardFocusWithin)
        End Sub

        ' Le modèle en cases : comme mkm, ce qui suit « ? » est facultatif.
        Private Sub Analyser()
            Dim modele = If(Mask, String.Empty)
            Dim cases As New List(Of Regex)
            Dim lettres As New List(Of Char)
            _facultatif = -1
            _premiereCase = -1
            For Each c In modele
                If c = "?"c Then
                    _facultatif = cases.Count
                    Continue For
                End If
                Dim definition As Regex = Nothing
                Definitions.TryGetValue(c, definition)
                If definition IsNot Nothing AndAlso _premiereCase < 0 Then _premiereCase = cases.Count
                cases.Add(definition)
                lettres.Add(c)
            Next
            If _facultatif < 0 Then _facultatif = cases.Count
            If _premiereCase < 0 Then _premiereCase = cases.Count
            _cases = cases.ToArray()
            _modele = lettres.ToArray()
            _tampon = New Char(_cases.Length - 1) {}
            For i = 0 To _cases.Length - 1
                _tampon(i) = If(_cases(i) Is Nothing, _modele(i), Vide(i))
            Next
        End Sub

        Private Function Suivante(position As Integer) As Integer
            position += 1
            While position < Longueur AndAlso _cases(position) Is Nothing
                position += 1
            End While
            Return position
        End Function

        Private Function Precedente(position As Integer) As Integer
            position -= 1
            While position >= 0 AndAlso _cases(position) Is Nothing
                position -= 1
            End While
            Return position
        End Function

        Private Function Accepte(i As Integer, c As Char) As Boolean
            Return i >= 0 AndAlso i < Longueur AndAlso _cases(i) IsNot Nothing AndAlso _cases(i).IsMatch(c.ToString())
        End Function

        Private Sub Vider(debut As Integer, fin As Integer)
            For i = debut To Math.Min(fin, Longueur) - 1
                If _cases(i) IsNot Nothing Then _tampon(i) = Vide(i)
            Next
        End Sub

        ' Décale vers la gauche ce qui suit, dans les cases qui l'acceptent (Retour arrière, Suppr).
        Private Sub DecalerAGauche(debut As Integer, fin As Integer)
            If debut < 0 Then Return
            Dim j = Suivante(fin)
            For i = debut To Longueur - 1
                If _cases(i) Is Nothing Then Continue For
                If j < Longueur AndAlso Accepte(i, _tampon(j)) Then
                    _tampon(i) = _tampon(j)
                    _tampon(j) = Vide(j)
                Else
                    Exit For
                End If
                j = Suivante(j)
            Next
            Ecrire()
            Placer(Math.Max(_premiereCase, debut))
        End Sub

        ' Décale vers la droite ce qui suit, pour une saisie au milieu.
        Private Sub DecalerADroite(position As Integer)
            Dim c = Vide(position)
            For i = position To Longueur - 1
                If _cases(i) Is Nothing Then Continue For
                Dim j = Suivante(i)
                Dim t = _tampon(i)
                _tampon(i) = c
                If j < Longueur AndAlso Accepte(j, t) Then c = t Else Exit For
            Next
        End Sub

        Private Sub Ecrire()
            _ecriture = True
            Try
                Text = New String(_tampon)
            Finally
                _ecriture = False
            End Try
            Decrire()
        End Sub

        Private Sub Placer(position As Integer)
            CaretIndex = Math.Max(0, Math.Min(position, Text.Length))
            SelectionLength = 0
        End Sub

        ' La valeur saisie seule, et si toutes les cases obligatoires sont remplies.
        Private Sub Decrire()
            Dim saisis As New Text.StringBuilder()
            Dim complet = True
            For i = 0 To Longueur - 1
                If _cases(i) Is Nothing Then Continue For
                If _tampon(i) <> Vide(i) AndAlso Accepte(i, _tampon(i)) Then
                    saisis.Append(_tampon(i))
                ElseIf i < _facultatif Then
                    complet = False
                End If
            Next
            SetValue(UnmaskedValuePropertyKey, If(String.IsNullOrEmpty(Text), String.Empty, saisis.ToString()))
            SetValue(IsCompletePropertyKey, Longueur > 0 AndAlso complet AndAlso Not String.IsNullOrEmpty(Text))
        End Sub

        ' Place le texte donné dans les cases, comme mkm (checkVal) ; rend la case qui suit le dernier caractère placé.
        Private Function Placer(texte As String, garder As Boolean) As Integer
            Dim dernier = -1
            Dim pos = 0
            Dim i = 0
            While i < Longueur
                If _cases(i) IsNot Nothing Then
                    _tampon(i) = Vide(i)
                    Dim place = False
                    While pos < texte.Length
                        Dim c = texte(pos)
                        pos += 1
                        If Accepte(i, c) Then
                            _tampon(i) = c
                            dernier = i
                            place = True
                            Exit While
                        End If
                    End While
                    If Not place AndAlso pos >= texte.Length Then
                        Vider(i + 1, Longueur)
                        Exit While
                    End If
                Else
                    If pos < texte.Length AndAlso _tampon(i) = texte(pos) Then pos += 1
                    If i < _facultatif Then dernier = Math.Max(dernier, i)
                End If
                i += 1
            End While
            If garder Then
                Ecrire()
            ElseIf dernier + 1 < _facultatif Then
                ' Incomplet : vidé (AutoClear), ou coupé après le dernier caractère.
                If AutoClear OrElse dernier < 0 Then
                    Vider(0, Longueur)
                    _ecriture = True
                    Text = String.Empty
                    _ecriture = False
                    Decrire()
                Else
                    Ecrire()
                    _ecriture = True
                    Text = Text.Substring(0, dernier + 1)
                    _ecriture = False
                    Decrire()
                End If
            Else
                Ecrire()
                _ecriture = True
                Text = Text.Substring(0, Math.Min(Text.Length, dernier + 1))
                _ecriture = False
                Decrire()
            End If
            Return If(_facultatif < Longueur, i, _premiereCase)
        End Function

        ' Le texte refait au modèle : complet au clavier, coupé (ou vidé) hors du clavier.
        Private Sub Ajuster(auClavier As Boolean)
            If Longueur = 0 Then Return
            If String.IsNullOrEmpty(Text) AndAlso Not auClavier Then
                Analyser()
                Decrire()
                Return
            End If
            Dim position = Placer(Text, auClavier)
            If auClavier Then Placer(Math.Min(position, Premiere()))
        End Sub

        ' La première case vide, où reprend la saisie.
        Private Function Premiere() As Integer
            For i = 0 To Longueur - 1
                If _cases(i) IsNot Nothing AndAlso _tampon(i) = Vide(i) Then Return i
            Next
            Return Longueur
        End Function

        ''' <summary>Le texte posé en code ou par liaison est refait au modèle.</summary>
        Protected Overrides Sub OnTextChanged(e As TextChangedEventArgs)
            MyBase.OnTextChanged(e)
            If _ecriture OrElse Longueur = 0 Then Return
            _ecriture = True
            Try
                Ajuster(IsKeyboardFocusWithin)
            Finally
                _ecriture = False
            End Try
        End Sub

        ''' <summary>Au clavier, les cases se montrent ; le curseur va à la première vide.</summary>
        Protected Overrides Sub OnGotKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnGotKeyboardFocus(e)
            _texteAuFocus = Text
            If Longueur = 0 Then Return
            Dim position = Placer(Text, True)
            Dispatcher.BeginInvoke(Threading.DispatcherPriority.Input, New System.Action(Sub() Placer(Math.Min(position, Premiere()))))
        End Sub

        ''' <summary>Hors du clavier, une saisie incomplète est vidée (<see cref="AutoClear"/>) ou coupée.</summary>
        Protected Overrides Sub OnLostKeyboardFocus(e As KeyboardFocusChangedEventArgs)
            MyBase.OnLostKeyboardFocus(e)
            If Longueur = 0 Then Return
            Placer(Text, False)
        End Sub

        ''' <summary>Chaque caractère tapé va dans la case suivante, s'il lui convient.</summary>
        Protected Overrides Sub OnPreviewTextInput(e As TextCompositionEventArgs)
            If Longueur = 0 OrElse IsReadOnly Then
                MyBase.OnPreviewTextInput(e)
                Return
            End If
            For Each c In e.Text
                Taper(c)
            Next
            e.Handled = True
        End Sub

        Friend Sub Taper(c As Char)
            If Text.Length <> Longueur Then Placer(Text, True)
            Dim debut = SelectionStart
            Dim fin = SelectionStart + SelectionLength
            If fin > debut Then
                Vider(debut, fin)
                DecalerAGauche(debut, fin - 1)
            End If
            Dim p = Suivante(debut - 1)
            If p >= Longueur OrElse Not Accepte(p, c) Then
                Ecrire()
                Placer(debut)
                Return
            End If
            DecalerADroite(p)
            _tampon(p) = c
            Ecrire()
            Placer(Suivante(p))
        End Sub

        ' Le texte qu'avait le champ en prenant le clavier : Échap le lui rend, comme mkm.
        Private _texteAuFocus As String

        ''' <summary>Retour arrière et Suppr vident une case et ramènent ce qui suit ; Échap rend la valeur d'entrée.</summary>
        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            If e.Key = Key.Escape AndAlso Not IsReadOnly AndAlso _texteAuFocus IsNot Nothing AndAlso Text <> _texteAuFocus Then
                Text = _texteAuFocus
                CaretIndex = Text.Length
                e.Handled = True
                Return
            End If
            If Longueur = 0 OrElse IsReadOnly OrElse (e.Key <> Key.Back AndAlso e.Key <> Key.Delete) Then
                MyBase.OnPreviewKeyDown(e)
                Return
            End If
            Effacer(e.Key = Key.Delete)
            e.Handled = True
        End Sub

        Friend Sub Effacer(versLAvant As Boolean)
            If Text.Length <> Longueur Then Placer(Text, True)
            Dim debut = SelectionStart
            Dim fin = SelectionStart + SelectionLength
            If fin = debut Then
                If versLAvant Then
                    fin = Suivante(debut - 1)
                    debut = fin
                    fin = Suivante(fin)
                Else
                    debut = Precedente(debut)
                    fin = debut + 1
                End If
            End If
            If debut < 0 Then Return
            Vider(debut, fin)
            DecalerAGauche(debut, fin - 1)
        End Sub

        ' Coller : chaque caractère comme s'il était tapé.
        Private Sub Coller(sender As Object, e As DataObjectPastingEventArgs)
            If Longueur = 0 Then Return
            Dim texte = TryCast(e.DataObject.GetData(DataFormats.UnicodeText), String)
            e.CancelCommand()
            If String.IsNullOrEmpty(texte) OrElse IsReadOnly Then Return
            For Each c In texte
                Taper(c)
            Next
        End Sub

        ' Couper : copier, puis effacer la sélection en gardant le modèle.
        Private Sub Commande(sender As Object, e As ExecutedRoutedEventArgs)
            If Longueur = 0 OrElse e.Command IsNot ApplicationCommands.Cut OrElse SelectionLength = 0 Then Return
            Clipboard.SetText(SelectedText)
            If Not IsReadOnly Then Effacer(False)
            e.Handled = True
        End Sub

    End Class

End Namespace
