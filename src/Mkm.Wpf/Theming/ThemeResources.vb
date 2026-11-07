Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Effects

Namespace Mkm.Wpf.Theming

    ''' <summary>
    ''' Les ressources WPF d'un thème : une par jeton, sous la clé « mkm. » suivie de son chemin
    ''' (<c>{DynamicResource mkm.text.color}</c> pour <c>dt('text.color')</c> dans mkm).
    ''' </summary>
    ''' <remarks>
    ''' Le type d'une ressource vient du chemin et de la valeur du jeton : un segment « radius » donne
    ''' un <see cref="CornerRadius"/>, « padding » ou « margin » une <see cref="Thickness"/>,
    ''' « shadow » une ombre, « font.family » une police, « font.weight » une graisse, « duration »
    ''' une durée ; puis une couleur donne un pinceau figé, un nombre (en px ou sans unité) un
    ''' <see cref="Double"/>. Le reste (« solid »…) reste un texte. Un pixel CSS vaut une unité WPF.
    ''' </remarks>
    Public NotInheritable Class ThemeResources

        ''' <summary>Le préfixe des clés : « mkm. ».</summary>
        Public Const Prefix As String = "mkm."

        Private Shared ReadOnly _nombre As New Regex("^(-?\d*\.?\d+)(px)?$", RegexOptions.Compiled)
        Private Shared ReadOnly _couleurDansOmbre As New Regex("color-mix\((?:[^()]|\([^()]*\))*\)|rgba?\([^)]*\)|#[0-9a-fA-F]{3,8}\b|\b[a-zA-Z]+$", RegexOptions.Compiled)
        Private Shared ReadOnly _part As New Regex("^(.+?)(?:\s+(\d*\.?\d+)%)?$", RegexOptions.Compiled)
        Private Shared ReadOnly _famillesGeneriques As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "serif", "sans-serif", "monospace", "cursive", "fantasy", "system-ui"}

        Private Sub New()
        End Sub

        ''' <summary>Le dictionnaire des ressources d'un thème.</summary>
        Public Shared Function Build(theme As ThemeDefinition) As ResourceDictionary
            If theme Is Nothing Then Throw New ArgumentNullException(NameOf(theme))

            Dim ressources As New ResourceDictionary()
            For Each chemin In theme.Paths
                ressources(Prefix & chemin) = Convert(chemin, theme.Resolve(chemin))
            Next
            Return ressources
        End Function

        ''' <summary>La ressource WPF d'un jeton, sa valeur déjà résolue.</summary>
        Public Shared Function Convert(path As String, value As String) As Object
            If path Is Nothing Then Throw New ArgumentNullException(NameOf(path))
            If value Is Nothing Then Return Nothing

            Dim segments = path.Split("."c)
            Dim dernier = segments(segments.Length - 1)
            Dim avantDernier = If(segments.Length > 1, segments(segments.Length - 2), String.Empty)

            If segments.Contains("shadow") Then Return Ombre(value)
            If segments.Contains("radius") Then Return If(Coins(value), CObj(value))
            If segments.Contains("padding") OrElse segments.Contains("margin") Then Return If(Marges(value), CObj(value))
            If avantDernier = "font" AndAlso dernier = "family" Then Return Police(value)
            If avantDernier = "font" AndAlso dernier = "weight" Then Return If(Graisse(value), CObj(value))
            If dernier = "duration" Then Return If(Duree(value), CObj(value))

            Dim couleur As Color
            If TryParseColor(value, couleur) Then Return Pinceau(couleur)

            Dim nombre As Double
            If TryParsePixels(value, nombre) Then Return nombre

            Return value
        End Function

        ''' <summary>
        ''' Une couleur CSS : « #rgb », « #rrggbb », « #rrggbbaa », « rgb(…) », « rgba(…) »,
        ''' « transparent », un nom (« white »), ou « color-mix(in srgb, A p%, B) ».
        ''' </summary>
        Public Shared Function TryParseColor(value As String, ByRef color As Color) As Boolean
            If String.IsNullOrWhiteSpace(value) Then Return False
            Dim texte = value.Trim()

            If texte.StartsWith("#", StringComparison.Ordinal) Then Return Hexa(texte.Substring(1), color)
            If texte.StartsWith("rgb", StringComparison.OrdinalIgnoreCase) Then Return Rvb(texte, color)
            If texte.StartsWith("color-mix(", StringComparison.OrdinalIgnoreCase) Then Return Melange(texte, color)
            If texte.Equals("transparent", StringComparison.OrdinalIgnoreCase) Then
                color = Colors.Transparent
                Return True
            End If

            ' Un nom de couleur (white, black…) : WPF connaît ceux de CSS.
            If Not texte.All(AddressOf Char.IsLetter) Then Return False
            Try
                color = DirectCast(ColorConverter.ConvertFromString(texte), Color)
                Return True
            Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is NotSupportedException
                Return False
            End Try
        End Function

        ''' <summary>Un nombre, en pixels (« 12px », « -1px ») ou sans unité (« 0 », « 1.25 »).</summary>
        Public Shared Function TryParsePixels(value As String, ByRef number As Double) As Boolean
            If value Is Nothing Then Return False
            Dim m = _nombre.Match(value.Trim())
            Return m.Success AndAlso Double.TryParse(m.Groups(1).Value, NumberStyles.Float, CultureInfo.InvariantCulture, number)
        End Function

        Private Shared Function Pixels(value As String) As Double()
            Dim parties = value.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
            Dim nombres(parties.Length - 1) As Double
            For i = 0 To parties.Length - 1
                If Not TryParsePixels(parties(i), nombres(i)) Then Return Nothing
            Next
            Return nombres
        End Function

        ' Ordre de CSS : haut, droite, bas, gauche ; WPF : gauche, haut, droite, bas.
        Private Shared Function Marges(value As String) As Object
            Dim n = Pixels(value)
            If n Is Nothing Then Return Nothing
            Select Case n.Length
                Case 1 : Return New Thickness(n(0))
                Case 2 : Return New Thickness(n(1), n(0), n(1), n(0))
                Case 3 : Return New Thickness(n(1), n(0), n(1), n(2))
                Case 4 : Return New Thickness(n(3), n(0), n(1), n(2))
                Case Else : Return Nothing
            End Select
        End Function

        ' Ordre de CSS : haut-gauche, haut-droite, bas-droite, bas-gauche ; WPF de même.
        Private Shared Function Coins(value As String) As Object
            Dim n = Pixels(value)
            If n Is Nothing Then Return Nothing
            Select Case n.Length
                Case 1 : Return New CornerRadius(n(0))
                Case 2 : Return New CornerRadius(n(0), n(1), n(0), n(1))
                Case 3 : Return New CornerRadius(n(0), n(1), n(2), n(1))
                Case 4 : Return New CornerRadius(n(0), n(1), n(2), n(3))
                Case Else : Return Nothing
            End Select
        End Function

        Private Shared Function Police(value As String) As FontFamily
            Dim familles = value.Split(","c).
                Select(Function(f) f.Trim().Trim(""""c, "'"c)).
                Where(Function(f) f.Length > 0 AndAlso Not _famillesGeneriques.Contains(f))
            Return New FontFamily(String.Join(", ", familles))
        End Function

        Private Shared Function Graisse(value As String) As Object
            Dim poids As Integer
            If Integer.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, poids) AndAlso poids >= 1 AndAlso poids <= 999 Then
                Return FontWeight.FromOpenTypeWeight(poids)
            End If
            Try
                Return New FontWeightConverter().ConvertFromInvariantString(value)
            Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is NotSupportedException
                Return Nothing
            End Try
        End Function

        Private Shared Function Duree(value As String) As Object
            Dim texte = value.Trim()
            Dim nombre As Double
            If texte.EndsWith("ms", StringComparison.Ordinal) AndAlso Double.TryParse(texte.Substring(0, texte.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, nombre) Then
                Return New Duration(TimeSpan.FromMilliseconds(nombre))
            End If
            If texte.EndsWith("s", StringComparison.Ordinal) AndAlso Double.TryParse(texte.Substring(0, texte.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, nombre) Then
                Return New Duration(TimeSpan.FromSeconds(nombre))
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' La première ombre de « x y flou [étalement] couleur, … » en <see cref="DropShadowEffect"/> ;
        ''' « none » n'en donne pas. L'étalement n'a pas d'équivalent dans WPF.
        ''' </summary>
        Private Shared Function Ombre(value As String) As Object
            Dim texte = value.Trim()
            If texte.Length = 0 OrElse texte.Equals("none", StringComparison.OrdinalIgnoreCase) Then Return Nothing

            Dim premiere = PremiereOmbre(texte)
            Dim trouvee = _couleurDansOmbre.Match(premiere)
            Dim couleur = Colors.Black
            If trouvee.Success AndAlso Not TryParseColor(trouvee.Value, couleur) Then Return Nothing
            Dim mesures = Pixels(If(trouvee.Success, premiere.Remove(trouvee.Index, trouvee.Length), premiere).Trim())
            If mesures Is Nothing OrElse mesures.Length < 2 Then Return Nothing

            Dim x = mesures(0)
            Dim y = mesures(1)
            Dim flou = If(mesures.Length > 2, mesures(2), 0)
            Dim angle = Math.Atan2(-y, x) * 180 / Math.PI
            Dim effet As New DropShadowEffect With {
                .Color = Color.FromRgb(couleur.R, couleur.G, couleur.B),
                .Opacity = couleur.A / 255.0,
                .ShadowDepth = Math.Sqrt(x * x + y * y),
                .Direction = If(angle < 0, angle + 360, angle),
                .BlurRadius = flou
            }
            effet.Freeze()
            Return effet
        End Function

        ' Jusqu'à la première virgule hors des parenthèses de rgba(…).
        Private Shared Function PremiereOmbre(value As String) As String
            Dim profondeur = 0
            For i = 0 To value.Length - 1
                Select Case value(i)
                    Case "("c : profondeur += 1
                    Case ")"c : profondeur -= 1
                    Case ","c : If profondeur = 0 Then Return value.Substring(0, i).Trim()
                End Select
            Next
            Return value
        End Function

        Private Shared Function Pinceau(couleur As Color) As SolidColorBrush
            Dim resultat As New SolidColorBrush(couleur)
            resultat.Freeze()
            Return resultat
        End Function

        Private Shared Function Hexa(chiffres As String, ByRef couleur As Color) As Boolean
            Dim octets As Byte()
            Select Case chiffres.Length
                Case 3, 4
                    octets = chiffres.Select(Function(c) CByte(Convert16(c) * 17)).ToArray()
                Case 6, 8
                    octets = Enumerable.Range(0, chiffres.Length \ 2).Select(Function(i) CByte(Convert16(chiffres(2 * i)) * 16 + Convert16(chiffres(2 * i + 1)))).ToArray()
                Case Else
                    Return False
            End Select
            If Not chiffres.All(AddressOf Uri.IsHexDigit) Then Return False

            Dim alpha = If(octets.Length = 4, octets(3), CByte(255))
            couleur = Color.FromArgb(alpha, octets(0), octets(1), octets(2))
            Return True
        End Function

        Private Shared Function Convert16(chiffre As Char) As Integer
            Return If(Uri.IsHexDigit(chiffre), Uri.FromHex(chiffre), 0)
        End Function

        Private Shared Function Rvb(value As String, ByRef couleur As Color) As Boolean
            Dim debut = value.IndexOf("("c)
            Dim fin = value.LastIndexOf(")"c)
            If debut < 0 OrElse fin < debut Then Return False

            Dim parties = value.Substring(debut + 1, fin - debut - 1).Split({","c, " "c, "/"c}, StringSplitOptions.RemoveEmptyEntries)
            If parties.Length < 3 OrElse parties.Length > 4 Then Return False

            Dim canaux(3) As Double
            canaux(3) = 1
            For i = 0 To parties.Length - 1
                Dim texte = parties(i).Trim()
                Dim pourcent = texte.EndsWith("%", StringComparison.Ordinal)
                If Not Double.TryParse(texte.TrimEnd("%"c), NumberStyles.Float, CultureInfo.InvariantCulture, canaux(i)) Then Return False
                If pourcent Then canaux(i) = canaux(i) / 100 * If(i = 3, 1, 255)
            Next

            couleur = Color.FromArgb(Octet(canaux(3) * 255), Octet(canaux(0)), Octet(canaux(1)), Octet(canaux(2)))
            Return True
        End Function

        ' color-mix(in srgb, A [p%], B [q%]), comme CSS Color 5 : les parts manquantes complètent à
        ' 100 %, des parts dont la somme passe sous 100 % rendent le mélange d'autant plus transparent,
        ' et les canaux se mélangent pondérés de leur opacité (« transparent » n'assombrit pas).
        Private Shared Function Melange(value As String, ByRef couleur As Color) As Boolean
            Dim texte = value.Trim()
            If Not texte.EndsWith(")", StringComparison.Ordinal) Then Return False
            Dim parties = Virgules(texte.Substring("color-mix(".Length, texte.Length - "color-mix(".Length - 1))
            If parties.Count <> 3 OrElse Not parties(0).Equals("in srgb", StringComparison.OrdinalIgnoreCase) Then Return False

            Dim a, b As Color
            Dim p, q As Double?
            If Not Part(parties(1), a, p) OrElse Not Part(parties(2), b, q) Then Return False
            If Not p.HasValue AndAlso Not q.HasValue Then
                p = 0.5
                q = 0.5
            ElseIf Not p.HasValue Then
                p = 1 - q.Value
            ElseIf Not q.HasValue Then
                q = 1 - p.Value
            End If
            Dim total = p.Value + q.Value
            If total <= 0 Then Return False
            Dim pa = p.Value / total
            Dim pb = q.Value / total

            Dim alpha = a.A * pa + b.A * pb
            Dim canal = Function(ca As Byte, cb As Byte) If(alpha <= 0, 0.0, (CDbl(ca) * a.A * pa + CDbl(cb) * b.A * pb) / alpha)
            couleur = Color.FromArgb(
                Octet(alpha * Math.Min(1, total)),
                Octet(canal(a.R, b.R)),
                Octet(canal(a.G, b.G)),
                Octet(canal(a.B, b.B)))
            Return True
        End Function

        ' Une couleur de color-mix et sa part, en fraction (« #fff 40% » → 0,4).
        Private Shared Function Part(texte As String, ByRef couleur As Color, ByRef fraction As Double?) As Boolean
            Dim m = _part.Match(texte.Trim())
            If Not m.Success OrElse Not TryParseColor(m.Groups(1).Value, couleur) Then Return False
            fraction = If(m.Groups(2).Success, Double.Parse(m.Groups(2).Value, CultureInfo.InvariantCulture) / 100, New Double?())
            Return True
        End Function

        ' Les morceaux d'une liste séparée de virgules, hors des parenthèses.
        Private Shared Function Virgules(texte As String) As List(Of String)
            Dim morceaux As New List(Of String)
            Dim profondeur = 0
            Dim debut = 0
            For i = 0 To texte.Length - 1
                Select Case texte(i)
                    Case "("c : profondeur += 1
                    Case ")"c : profondeur -= 1
                    Case ","c
                        If profondeur = 0 Then
                            morceaux.Add(texte.Substring(debut, i - debut).Trim())
                            debut = i + 1
                        End If
                End Select
            Next
            morceaux.Add(texte.Substring(debut).Trim())
            Return morceaux
        End Function

        Private Shared Function Octet(valeur As Double) As Byte
            Return CByte(Math.Max(0, Math.Min(255, Math.Round(valeur))))
        End Function

    End Class

End Namespace
