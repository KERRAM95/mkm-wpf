Imports System
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Les composants « Divers » des lots 10 et 11 : Chip, Badge, Avatar.</summary>
    <TestFixture>
    Public Class DiversTests
        Inherits Scenario

        <Test>
        Public Sub Chip_LaCroixRetireLaPuceEtLeDit()
            Dim page = OuvrirLaDoc("chip")
            Dim cadre = Exemple(page, "retirable")
            Dim puce = Trouver("Maturite", cadre)

            Assert.That(NomDe(puce), [Is].EqualTo("Maturité : En cours"), "un groupe nommé par son texte")
            Cliquer(TrouverParNom(T("aria.remove"), ControlType.Button, puce))
            AttendreQue(Function() puce.IsOffscreen, "la puce cachée")
            AttendreQue(Function() NomDe(Trouver("Journal", cadre)) = "Filtre retiré : Maturité : En cours", "Remove au journal")
            Cliquer(Trouver("ToutRemettre", cadre))
            AttendreQue(Function() Not puce.IsOffscreen, "la puce revenue")
        End Sub

        <Test>
        Public Sub Chip_RetourArriereRetireLaPuce()
            Dim page = OuvrirLaDoc("chip")
            Dim cadre = Exemple(page, "retirable")
            Dim puce = Trouver("Maturite", cadre)
            Dim croix = TrouverParNom(T("aria.remove"), ControlType.Button, puce)
            croix.Focus()

            Taper(VirtualKeyShort.BACK)
            AttendreQue(Function() puce.IsOffscreen, "Retour arrière : la puce retirée, comme mkm")
        End Sub

        <Test>
        Public Sub OverlayBadge_LaPastilleSeLit()
            Dim page = OuvrirLaDoc("badge")
            Dim alertes = Trouver("Alertes", page)

            TrouverParNom("4", ControlType.Text, alertes)
        End Sub

        <Test>
        Public Sub Avatar_UneImageNommee()
            Dim page = OuvrirLaDoc("avatar")
            Dim initiales = Trouver("Initiales", page)

            Assert.That(initiales.ControlType, [Is].EqualTo(ControlType.Image))
            Assert.That(NomDe(initiales), [Is].EqualTo("Mourad Kerram"))
            Assert.That(Trouver("Equipe", page).FindAllChildren().Length, [Is].EqualTo(6), "les six avatars du groupe")
            Assert.That(NomDe(Trouver("Photo", page)), [Is].EqualTo("Walter"), "une photo nommée")
        End Sub

    End Class

End Namespace
