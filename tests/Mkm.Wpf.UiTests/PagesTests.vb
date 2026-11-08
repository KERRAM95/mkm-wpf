Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>
    ''' Chaque page de la vitrine : elle s'ouvre sans erreur, et chacune de ses commandes a un nom pour les lecteurs
    ''' d'écran (une commande sans nom, Narrateur la dit « bouton », sans plus).
    ''' </summary>
    <TestFixture>
    Public Class PagesTests
        Inherits Scenario

        ''' <summary>Les pages de la vitrine, par l'étiquette de leur entrée dans la liste de gauche.</summary>
        Public Shared ReadOnly Pages As String() = {
            "ActionBar", "Champs", "Boutons", "Conteneurs", "Retours", "Attente", "Saisie",
            "Fiche", "Arbre", "Identite", "Tableau", "Choix", "MiseEnPage", "Arborescence"}

        ' Les rôles qu'un utilisateur actionne : chacun doit dire ce qu'il fait.
        Private Shared ReadOnly RolesANommer As ControlType() = {
            ControlType.Button, ControlType.CheckBox, ControlType.RadioButton, ControlType.MenuItem, ControlType.TabItem,
            ControlType.ListItem, ControlType.SplitButton, ControlType.Hyperlink, ControlType.TreeItem}

        <TestCaseSource(NameOf(Pages))>
        Public Sub ChaquePage_SOuvreSansErreur(page As String)
            Dim contenu = Ouvrir(page)

            Assert.That(contenu.FindAllDescendants().Length, [Is].GreaterThan(5), "la page montre ses composants")
        End Sub

        <TestCaseSource(NameOf(Pages))>
        Public Sub ChaqueCommande_AUnNomPourLesLecteursDEcran(page As String)
            Dim contenu = Ouvrir(page)

            Dim sansNom As New List(Of String)()
            Parcourir(contenu, sansNom)
            Assert.That(sansNom, [Is].Empty, "Commandes sans nom :" & Environment.NewLine & String.Join(Environment.NewLine, sansNom))
        End Sub

        Private Shared Sub Parcourir(element As AutomationElement, sansNom As List(Of String))
            For Each enfant In element.FindAllChildren()
                Dim role = enfant.Properties.ControlType.ValueOrDefault
                ' Les flèches et la poignée d'une barre de défilement : celles de WPF, que Windows nomme lui-même.
                If role = ControlType.ScrollBar Then Continue For
                If RolesANommer.Contains(role) AndAlso NomDe(enfant).Trim().Length = 0 Then sansNom.Add(Vitrine.Description(enfant))
                Parcourir(enfant, sansNom)
            Next
        End Sub

    End Class

End Namespace
