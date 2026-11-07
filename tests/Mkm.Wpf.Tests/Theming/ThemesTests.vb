Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Windows.Media
Imports Mkm.Wpf.Theming
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Theming

    ''' <summary>
    ''' Les thèmes de mkm-wpf, vérifiés comme mkm vérifie les siens (<c>ara.spec.ts</c>,
    ''' <c>contrast.ts</c>) : renvois résolus, ARA qui ne diffère de Dashboard que par ses couleurs,
    ''' chaque texte lisible sur son fond.
    ''' </summary>
    <TestFixture>
    Public Class ThemesTests

        ' Une teinte de palette (« navy.500 », « surface.0 ») ou une couleur nommée de 3DEXPERIENCE.
        Private Shared ReadOnly _palette As New Regex("^([a-z]+\.\d+|dspalette\..+)$")
        ' Une couleur écrite dans une valeur : #…, rgb(…), rgba(…), color-mix(…).
        Private Shared ReadOnly _couleur As New Regex("color-mix\((?:[^()]|\([^()]*\))*\)|rgba?\([^)]*\)|#[0-9a-fA-F]{3,8}\b")

        ' Les paires texte/fond de mkm (TEXT_CONTRAST_PAIRS) que les jetons importés portent.
        Private Shared ReadOnly _paires As String(,) = {
            {"text.color", "content.background"},
            {"text.color", "content.hover.background"},
            {"text.color", "overlay.select.background"},
            {"text.color", "overlay.popover.background"},
            {"text.color", "overlay.modal.background"},
            {"text.muted.color", "content.background"},
            {"text.muted.color", "content.hover.background"},
            {"text.muted.color", "overlay.popover.background"},
            {"content.color", "content.background"},
            {"highlight.color", "highlight.background"},
            {"list.option.color", "overlay.select.background"},
            {"list.option.focus.color", "list.option.focus.background"},
            {"list.option.selected.color", "list.option.selected.background"},
            {"menu.item.color", "menu.background"},
            {"menu.item.focus.color", "menu.item.focus.background"},
            {"contextmenu.item.color", "contextmenu.background"},
            {"contextmenu.item.focus.color", "contextmenu.item.focus.background"},
            {"tooltip.color", "tooltip.background"},
            {"actionbar.tab.color", "actionbar.panel.background"},
            {"actionbar.tab.active.color", "actionbar.panel.background"},
            {"actionbar.item.color", "actionbar.panel.background"},
            {"actionbar.item.label.color", "actionbar.panel.background"},
            {"actionbar.item.active.color", "actionbar.item.active.background"},
            {"inputtext.color", "inputtext.background"},
            {"select.color", "select.background"},
            {"select.option.color", "select.overlay.background"},
            {"select.option.selected.color", "select.option.selected.background"},
            {"tabs.tab.color", "tabs.tab.background"},
            {"tabs.tab.active.color", "tabs.tab.active.background"},
            {"fieldset.color", "fieldset.background"},
            {"fieldset.legend.color", "fieldset.legend.background"},
            {"dialog.color", "dialog.background"},
            {"dialog.color", "dialog.footer.background"}}

        ' Les paires de mkm par gravité : les boutons dans chaque état et chaque forme, les messages, les notifications.
        Private Shared Function PairesParGravite() As IEnumerable(Of String())
            Dim paires As New List(Of String())
            For Each g In {"primary", "secondary", "success", "info", "warn", "help", "danger", "contrast"}
                For Each etat In {"", "hover.", "active."}
                    paires.Add({$"button.{g}.{etat}color", $"button.{g}.{etat}background"})
                Next
                paires.Add({$"button.outlined.{g}.color", "content.background"})
                paires.Add({$"button.text.{g}.color", "content.background"})
                paires.Add({$"button.text.{g}.color", $"button.text.{g}.hover.background"})
            Next
            paires.Add({"button.link.color", "content.background"})
            paires.Add({"button.link.hover.color", "content.background"})
            For Each g In {"info", "success", "warn", "error", "secondary", "contrast"}
                paires.Add({$"message.{g}.color", $"message.{g}.background"})
                paires.Add({$"message.{g}.outlined.color", "content.background"})
                paires.Add({$"message.{g}.simple.color", "content.background"})
                paires.Add({$"toast.{g}.color", $"toast.{g}.background"})
                paires.Add({$"toast.{g}.detail.color", $"toast.{g}.background"})
            Next
            Return paires
        End Function

        ' Les paires qui restent sous 4,5:1 exprès, comme chez mkm (contrast.spec.ts), au-dessus de 3:1 (texte large).
        ' Le bouton principal de 3DX : le blanc sur le bleu #368ec4 du tableau de bord, 3,6:1 ; son survol et son appui
        ' tiennent 4,5.
        Private Shared ReadOnly _permises As String() = {"Dashboard : button.primary.color sur button.primary.background"}

        Private Shared Function Integres() As IEnumerable(Of TestCaseData)
            Return {New TestCaseData(Themes.Dashboard).SetName("Dashboard"), New TestCaseData(Themes.Ara).SetName("Ara")}
        End Function

        <Test>
        Public Sub DashboardEtAra_SontConnus()
            Assert.That(Themes.Find("dashboard"), [Is].SameAs(Themes.Dashboard))
            Assert.That(Themes.Find("ARA"), [Is].SameAs(Themes.Ara))
            Assert.That(Themes.Find("inconnu"), [Is].Null)
        End Sub

        <TestCaseSource(NameOf(Integres))>
        Public Sub ChaqueRenvoi_SeResout(theme As ThemeDefinition)
            Dim nonResolus = theme.Paths.Where(Function(p) theme.Resolve(p).Contains("{")).ToList()

            Assert.That(nonResolus, [Is].Empty)
        End Sub

        <TestCaseSource(NameOf(Integres))>
        Public Sub ChaqueTexte_SeLitSurSonFond(theme As ThemeDefinition)
            Dim paires = Enumerable.Range(0, _paires.GetLength(0)).
                Select(Function(i) {_paires(i, 0), _paires(i, 1)}).
                Concat(PairesParGravite())
            Dim illisibles As New List(Of String)
            For Each paire In paires
                Dim fond = Couleur(theme, paire(1), Colors.White)
                Dim texte = Couleur(theme, paire(0), fond)
                Dim rapport = Contraste(texte, fond)
                Dim cle = $"{theme.Name} : {paire(0)} sur {paire(1)}"
                Dim seuil = If(_permises.Contains(cle), 3.0, 4.5)
                If rapport < seuil Then illisibles.Add($"{cle} : {rapport:0.00}")
            Next

            Assert.That(illisibles, [Is].Empty)
        End Sub

        <Test>
        Public Sub Ara_NeDiffereDeDashboard_QueParSesCouleurs()
            Dim ecarts = Themes.Dashboard.Paths.
                Intersect(Themes.Ara.Paths).
                Where(Function(p) Not _palette.IsMatch(p)).
                Where(Function(p) SansCouleurs(Themes.Dashboard.Resolve(p)) <> SansCouleurs(Themes.Ara.Resolve(p))).
                Select(Function(p) $"{p} : {Themes.Dashboard.Resolve(p)} / {Themes.Ara.Resolve(p)}").
                ToList()

            Assert.That(ecarts, [Is].Empty)
        End Sub

        <Test>
        Public Sub Ara_ALesJetonsDeDashboard()
            Dim manquants = Themes.Dashboard.Paths.
                Where(Function(p) Not _palette.IsMatch(p)).
                Except(Themes.Ara.Paths).
                ToList()

            Assert.That(manquants, [Is].Empty)
        End Sub

        ' Une valeur où chaque couleur devient « C » : deux valeurs qui ne diffèrent que par leurs couleurs se confondent.
        Private Shared Function SansCouleurs(valeur As String) As String
            Return _couleur.Replace(valeur, "C")
        End Function

        ' La couleur d'un jeton, posée sur un fond quand elle est transparente.
        Private Shared Function Couleur(theme As ThemeDefinition, chemin As String, fond As Color) As Color
            Dim valeur = theme.Resolve(chemin)
            Assert.That(valeur, [Is].Not.Null, $"{theme.Name} : pas de jeton {chemin}")
            Dim c As Color
            Assert.That(ThemeResources.TryParseColor(valeur, c), [Is].True, $"{theme.Name} : {chemin} n'est pas une couleur ({valeur})")
            Dim a = c.A / 255.0
            Return Color.FromRgb(CByte(Math.Round(c.R * a + fond.R * (1 - a))),
                                 CByte(Math.Round(c.G * a + fond.G * (1 - a))),
                                 CByte(Math.Round(c.B * a + fond.B * (1 - a))))
        End Function

        ' Le rapport de contraste du WCAG 2.
        Private Shared Function Contraste(a As Color, b As Color) As Double
            Dim la = Luminance(a)
            Dim lb = Luminance(b)
            Return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05)
        End Function

        Private Shared Function Luminance(c As Color) As Double
            Dim canal = Function(v As Byte) As Double
                            Dim x = v / 255.0
                            Return If(x <= 0.03928, x / 12.92, Math.Pow((x + 0.055) / 1.055, 2.4))
                        End Function
            Return 0.2126 * canal(c.R) + 0.7152 * canal(c.G) + 0.0722 * canal(c.B)
        End Function

    End Class

End Namespace
