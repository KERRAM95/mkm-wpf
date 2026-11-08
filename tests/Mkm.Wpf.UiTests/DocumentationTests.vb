Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>
    ''' Chaque page de la documentation : elle s'ouvre, ses exemples se lisent (un exemple illisible est une erreur de la
    ''' vitrine), ses onglets API et Thème s'ouvrent, et chacune de ses commandes a un nom.
    ''' </summary>
    <TestFixture>
    Public Class DocumentationTests
        Inherits Scenario

        ''' <summary>Les pages, lues dans le dossier de la documentation de la vitrine.</summary>
        Public Shared ReadOnly Pages As String() =
            Directory.GetFiles(Path.Combine(Vitrine.Racine, "apps", "Mkm.Wpf.Showcase", "Documentation", "Pages"), "*.xml").
                Select(Function(f) Path.GetFileNameWithoutExtension(f)).OrderBy(Function(p) p, StringComparer.Ordinal).ToArray()

        Private Shared ReadOnly RolesANommer As ControlType() = {
            ControlType.Button, ControlType.CheckBox, ControlType.RadioButton, ControlType.MenuItem, ControlType.TabItem,
            ControlType.ListItem, ControlType.SplitButton, ControlType.Hyperlink, ControlType.TreeItem}

        <TestCaseSource(NameOf(Pages))>
        Public Sub ChaquePage_SOuvreAvecSesOnglets(page As String)
            Dim contenu = OuvrirLaDoc(page)

            Dim api = contenu.FindFirstDescendant(Function(cf) cf.ByName("API").And(cf.ByControlType(ControlType.TabItem)))
            If api Is Nothing Then Return
            Cliquer(api)
            AttendreQue(Function() api.Patterns.SelectionItem.Pattern.IsSelected.Value, "l'onglet API ouvert")
            Dim theme = TrouverParNom("Thème", ControlType.TabItem, contenu)
            Cliquer(theme)
            AttendreQue(Function() theme.Patterns.SelectionItem.Pattern.IsSelected.Value, "l'onglet Thème ouvert")
        End Sub

        <TestCaseSource(NameOf(Pages))>
        Public Sub ChaqueCommande_AUnNom(page As String)
            Dim contenu = OuvrirLaDoc(page)

            Dim sansNom As New List(Of String)()
            Parcourir(contenu, sansNom)
            Assert.That(sansNom, [Is].Empty, "Commandes sans nom :" & Environment.NewLine & String.Join(Environment.NewLine, sansNom))
        End Sub

        Private Shared Sub Parcourir(element As AutomationElement, sansNom As List(Of String))
            For Each enfant In element.FindAllChildren()
                Dim role = enfant.Properties.ControlType.ValueOrDefault
                If role = ControlType.ScrollBar Then Continue For
                If RolesANommer.Contains(role) AndAlso NomDe(enfant).Trim().Length = 0 Then sansNom.Add(Vitrine.Description(enfant))
                Parcourir(enfant, sansNom)
            Next
        End Sub

        <Test>
        Public Sub UnLienMeneASaPage()
            Dim contenu = OuvrirLaDoc("button")

            ' Le lien du texte d'Import (le sommaire a aussi des liens, vers les sections de la page).
            Cliquer(TrouverParNom("Installation", ControlType.Hyperlink, contenu))
            Trouver("page-installation")
        End Sub

        <Test>
        Public Sub LaRechercheFiltreLeMenu()
            Dim recherche = Trouver("Recherche")
            Cliquer(recherche)
            Ecrire("toast")

            AttendreQue(Function() Trouver("nav-doc-toast") IsNot Nothing AndAlso
                                   Fenetre.FindFirstDescendant(Function(cf) cf.ByAutomationId("nav-doc-button")) Is Nothing,
                        "seule l'entrée Toast reste")
            recherche.Patterns.Value.Pattern.SetValue("")
            Trouver("nav-doc-button")
        End Sub

        <Test>
        Public Sub LeBoutonCopierCopieLeCode()
            Dim contenu = OuvrirLaDoc("button")
            Dim copier = TrouverParNom("Copier le code", ControlType.Button, contenu)

            Cliquer(copier)
            AttendreQue(Function() NomDe(copier) = "Copié", "le bouton dit « Copié »")
        End Sub

    End Class

End Namespace
