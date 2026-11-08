Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation

    ''' <summary>
    ''' Le code d'un exemple, comme dans ui-kit : coloré (XAML ou VB), dans un cadre qui défile en largeur, avec son
    ''' langage et un bouton qui le copie.
    ''' </summary>
    Friend NotInheritable Class BlocDeCode
        Inherits Border

        ' Les couleurs du code, celles de GitHub en clair : le code se lit pareil dans les deux thèmes.
        Private Shared ReadOnly Fond As Brush = Gele("#F6F8FA")
        Private Shared ReadOnly Bord As Brush = Gele("#D0D7DE")
        Private Shared ReadOnly Texte As Brush = Gele("#1F2328")
        Private Shared ReadOnly Balise As Brush = Gele("#116329")
        Private Shared ReadOnly Attribut As Brush = Gele("#0550AE")
        Private Shared ReadOnly Chaine As Brush = Gele("#0A3069")
        Private Shared ReadOnly Extension As Brush = Gele("#8250DF")
        Private Shared ReadOnly Ponctuation As Brush = Gele("#6E7781")
        Private Shared ReadOnly Commentaire As Brush = Gele("#6E7781")
        Private Shared ReadOnly MotCle As Brush = Gele("#CF222E")

        Private ReadOnly _code As String

        ''' <param name="code">Le code, déjà désindenté.</param>
        ''' <param name="langage">« xaml » ou « vb ».</param>
        Public Sub New(code As String, langage As String)
            _code = code
            Background = Fond
            BorderBrush = Bord
            BorderThickness = New Thickness(1)
            CornerRadius = New CornerRadius(6)
            Margin = New Thickness(0, 8, 0, 12)

            Dim entete As New DockPanel With {.Margin = New Thickness(12, 4, 4, 0)}
            Dim copier As New Mkm.Wpf.Controls.Button With {
                .Icon = Icons.Copy, .Variant = ButtonVariant.Text, .Severity = ButtonSeverity.Secondary,
                .ToolTip = "Copier le code", .Width = 28, .Height = 28}
            AddHandler copier.Click, Sub()
                                         Try
                                             Clipboard.SetText(_code.Replace(vbLf, vbCrLf))
                                             copier.ToolTip = "Copié"
                                         Catch ex As System.Runtime.InteropServices.ExternalException
                                             ' Le presse-papiers est pris par une autre application : on n'insiste pas.
                                         End Try
                                     End Sub
            DockPanel.SetDock(copier, Dock.Right)
            entete.Children.Add(copier)
            entete.Children.Add(New TextBlock With {
                .Text = If(langage = "vb", "VB", If(langage = "xml", "XML", "XAML")), .FontSize = 11, .FontWeight = FontWeights.SemiBold,
                .Foreground = Ponctuation, .VerticalAlignment = VerticalAlignment.Center})

            Dim lignes As New TextBlock With {
                .FontFamily = New FontFamily("Consolas, Courier New"), .FontSize = 13, .Foreground = Texte,
                .Margin = New Thickness(16, 4, 16, 14)}
            lignes.Inlines.AddRange(If(langage = "vb", ColorerVb(code), ColorerXaml(code)))
            Dim defileur As New ScrollViewer With {
                .Content = lignes, .HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                .VerticalScrollBarVisibility = ScrollBarVisibility.Disabled, .Focusable = False}

            Dim pile As New DockPanel()
            DockPanel.SetDock(entete, Dock.Top)
            pile.Children.Add(entete)
            pile.Children.Add(defileur)
            Child = pile
        End Sub

        ''' <summary>Le code tel qu'il est copié.</summary>
        Public ReadOnly Property Code As String
            Get
                Return _code
            End Get
        End Property

        ''' <summary>Retire l'indentation commune et les lignes vides du début et de la fin.</summary>
        Public Shared Function Desindenter(code As String) As String
            Dim lignes = code.Replace(vbCrLf, vbLf).Replace(vbTab, "    ").Split(ChrW(10)).ToList()
            While lignes.Count > 0 AndAlso lignes(0).Trim().Length = 0
                lignes.RemoveAt(0)
            End While
            While lignes.Count > 0 AndAlso lignes(lignes.Count - 1).Trim().Length = 0
                lignes.RemoveAt(lignes.Count - 1)
            End While
            Dim marge = lignes.Where(Function(l) l.Trim().Length > 0).Select(Function(l) l.Length - l.TrimStart().Length).DefaultIfEmpty(0).Min()
            ' Des fins de ligne simples : un « \r » isolé dans un Run ferait une ligne vide de plus.
            Return String.Join(vbLf, lignes.Select(Function(l) If(l.Length >= marge, l.Substring(marge), l.TrimStart()).TrimEnd()))
        End Function

        ' ---- XAML ----

        Private Shared ReadOnly JetonsXaml As New Regex(
            "(?<commentaire><!--[\s\S]*?-->)|(?<ouvre></?)(?<balise>[\w:.]+)|(?<attribut>[\w:.]+)(?<egal>=)(?<valeur>""[^""]*"")|(?<ferme>/?>)",
            RegexOptions.Compiled)

        Private Shared Function ColorerXaml(code As String) As IEnumerable(Of Inline)
            Dim morceaux As New List(Of Inline)()
            Dim position = 0
            For Each m As Match In JetonsXaml.Matches(code)
                If m.Index > position Then morceaux.Add(New Run(code.Substring(position, m.Index - position)))
                If m.Groups("commentaire").Success Then
                    morceaux.Add(New Run(m.Value) With {.Foreground = Commentaire, .FontStyle = FontStyles.Italic})
                ElseIf m.Groups("balise").Success Then
                    morceaux.Add(New Run(m.Groups("ouvre").Value) With {.Foreground = Ponctuation})
                    morceaux.Add(New Run(m.Groups("balise").Value) With {.Foreground = Balise})
                ElseIf m.Groups("attribut").Success Then
                    morceaux.Add(New Run(m.Groups("attribut").Value) With {.Foreground = Attribut})
                    morceaux.Add(New Run("=") With {.Foreground = Ponctuation})
                    Dim valeur = m.Groups("valeur").Value
                    Dim estExtension = valeur.StartsWith("""{", System.StringComparison.Ordinal)
                    morceaux.Add(New Run(valeur) With {.Foreground = If(estExtension, Extension, Chaine)})
                Else
                    morceaux.Add(New Run(m.Value) With {.Foreground = Ponctuation})
                End If
                position = m.Index + m.Length
            Next
            If position < code.Length Then morceaux.Add(New Run(code.Substring(position)))
            Return morceaux
        End Function

        ' ---- VB ----

        Private Shared ReadOnly MotsCles As New HashSet(Of String)(System.StringComparer.OrdinalIgnoreCase) From {
            "AddHandler", "AddressOf", "And", "AndAlso", "As", "Boolean", "ByVal", "Case", "Class", "Const", "Dim", "Do", "Double",
            "Each", "Else", "ElseIf", "End", "Enum", "Event", "False", "Finally", "For", "Friend", "Function", "Get", "Handles",
            "If", "Imports", "In", "Inherits", "Integer", "Is", "IsNot", "Loop", "Me", "Module", "MyBase", "Namespace", "New",
            "Next", "Not", "Nothing", "Of", "Or", "OrElse", "Overrides", "Partial", "Private", "Property", "Protected", "Public",
            "RaiseEvent", "ReadOnly", "Return", "Select", "Set", "Shared", "String", "Sub", "Then", "True", "Try", "Catch",
            "TryCast", "DirectCast", "CType", "Using", "While", "With"}

        Private Shared ReadOnly JetonsVb As New Regex(
            "(?<commentaire>'[^\n]*)|(?<chaine>\$?""(?:[^""]|"""")*"")|(?<mot>\b[A-Za-z_]\w*\b)", RegexOptions.Compiled)

        Private Shared Function ColorerVb(code As String) As IEnumerable(Of Inline)
            Dim morceaux As New List(Of Inline)()
            Dim position = 0
            For Each m As Match In JetonsVb.Matches(code)
                If m.Groups("mot").Success AndAlso Not MotsCles.Contains(m.Value) Then Continue For
                If m.Index > position Then morceaux.Add(New Run(code.Substring(position, m.Index - position)))
                If m.Groups("commentaire").Success Then
                    morceaux.Add(New Run(m.Value) With {.Foreground = Commentaire, .FontStyle = FontStyles.Italic})
                ElseIf m.Groups("chaine").Success Then
                    morceaux.Add(New Run(m.Value) With {.Foreground = Chaine})
                Else
                    morceaux.Add(New Run(m.Value) With {.Foreground = MotCle})
                End If
                position = m.Index + m.Length
            Next
            If position < code.Length Then morceaux.Add(New Run(code.Substring(position)))
            Return morceaux
        End Function

        Private Shared Function Gele(couleur As String) As Brush
            Dim pinceau = New SolidColorBrush(DirectCast(ColorConverter.ConvertFromString(couleur), Color))
            pinceau.Freeze()
            Return pinceau
        End Function

    End Class

End Namespace
