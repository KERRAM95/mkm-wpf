Imports System
Imports System.Linq
Imports System.Threading
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>
    ''' Ce qui répond à l'utilisateur : la barre de progression, le voile (BlockUI), l'info-bulle, le dialogue de
    ''' confirmation, le dialogue, les notifications et le message qui se ferme.
    ''' </summary>
    <TestFixture>
    Public Class RetoursTests
        Inherits Scenario

        <Test>
        Public Sub ProgressBar_AvanceDUnQuart()
            Ouvrir("Attente")
            Dim barre = Trouver("Barre")

            Cliquer(TrouverParNom(T("showcase.waiting.advance"), ControlType.Button))
            AttendreQue(Function() barre.Patterns.RangeValue.Pattern.Value.Value = 65, "la barre à 65")
            Assert.That(NomDe(Trouver("Pourcentage")), [Is].EqualTo("65 %"))
        End Sub

        <Test>
        Public Sub BlockUI_LeVoileRefuseLeClicPuisLeRend()
            Ouvrir("Attente")
            Dim champ = Trouver("ChampVoile")

            Cliquer(Trouver("Bloquer"))
            Cliquer(champ)
            Thread.Sleep(300)
            Assert.That(AFocus(champ), [Is].False, "sous le voile, le champ ne prend pas le clavier")

            Thread.Sleep(3200)
            Cliquer(champ)
            AttendreQue(Function() AFocus(champ), "le voile levé, le champ prend le clavier")
        End Sub

        <Test>
        Public Sub Tooltip_LeSurvolMontreLInfoBulle()
            Ouvrir("Attente")

            Survoler(TrouverParNom(T("showcase.waiting.hover"), ControlType.Button))
            TrouverAilleurs(T("showcase.waiting.tooltipText"))
        End Sub

        <Test>
        Public Sub ConfirmDialog_EntreeAccepte()
            Ouvrir("Saisie")

            Cliquer(Trouver("Demander"))
            TrouverAilleurs(T("showcase.inputs.question"))
            Taper(VirtualKeyShort.RETURN)
            AttendreQue(Function() NomDe(Trouver("Reponse")) = T("showcase.inputs.yes"), "Entrée : « Accepté »")
        End Sub

        <Test>
        Public Sub ConfirmDialog_EchapRefuse()
            Ouvrir("Saisie")

            Cliquer(Trouver("Demander"))
            TrouverAilleurs(T("showcase.inputs.question"))
            Taper(VirtualKeyShort.ESCAPE)
            AttendreQue(Function() NomDe(Trouver("Reponse")) = T("showcase.inputs.no"), "Échap : « Refusé »")
        End Sub

        <Test>
        Public Sub ConfirmDialog_SesBoutonsRepondent()
            Ouvrir("Saisie")

            Cliquer(Trouver("Demander"))
            Cliquer(TrouverAilleurs(T("locale.reject"), ControlType.Button))
            AttendreQue(Function() NomDe(Trouver("Reponse")) = T("showcase.inputs.no"), "« Non » : « Refusé »")

            Cliquer(Trouver("Demander"))
            Cliquer(TrouverAilleurs(T("locale.accept"), ControlType.Button))
            AttendreQue(Function() NomDe(Trouver("Reponse")) = T("showcase.inputs.yes"), "« Oui » : « Accepté »")
        End Sub

        <Test>
        Public Sub Dialog_RenommerRendLeNouveauNom()
            Ouvrir("Retours")

            Cliquer(Trouver("Renommer"))
            TrouverAilleurs(T("showcase.feedback.rename"))
            ' Le champ a le clavier, son texte choisi : la saisie le remplace.
            Ecrire("HS-00099")
            Taper(VirtualKeyShort.RETURN)
            AttendreQue(Function() NomDe(Trouver("Resultat")) = T("showcase.feedback.renamed", "HS-00099"), "« Renommé : HS-00099 »")
        End Sub

        <Test>
        Public Sub Dialog_EchapEtLaCroixAnnulent()
            Ouvrir("Retours")

            Cliquer(Trouver("Renommer"))
            TrouverAilleurs(T("showcase.feedback.rename"))
            Taper(VirtualKeyShort.ESCAPE)
            AttendreQue(Function() NomDe(Trouver("Resultat")) = T("showcase.feedback.cancelled"), "Échap : « Annulé »")

            Cliquer(Trouver("Renommer"))
            Dim dialogue = TrouverAilleurs(T("showcase.feedback.rename"))
            Cliquer(TrouverParNom(T("aria.close"), ControlType.Button, dialogue))
            AttendreQue(Function() Referme(T("showcase.feedback.rename")), "la croix ferme le dialogue")
            AttendreQue(Function() NomDe(Trouver("Resultat")) = T("showcase.feedback.cancelled"), "la croix : « Annulé »")
        End Sub

        <Test>
        Public Sub Toast_UneNotificationParaitEtSaCroixLaFerme()
            Ouvrir("Retours")
            Dim notifications = Trouver("Notifications")

            Cliquer(Trouver("NotifierInfo"))
            Dim detail = TrouverParNom(T("showcase.feedback.info"), dans:=notifications)
            Cliquer(TrouverParNom(T("aria.close"), ControlType.Button, notifications))
            AttendreQue(Function() notifications.FindFirstDescendant(Function(cf) cf.ByName(T("showcase.feedback.info"))) Is Nothing,
                        "la notification fermée")
        End Sub

        <Test>
        Public Sub Message_SaCroixLeFermeEtIlRevient()
            Dim page = Ouvrir("Retours")
            Dim message = Trouver("Ephemere")

            Cliquer(TrouverParNom(T("aria.close"), ControlType.Button, message))
            AttendreQue(Function() message.IsOffscreen, "le message fermé")
            Cliquer(TrouverParNom(T("showcase.feedback.again"), ControlType.Button, page))
            AttendreQue(Function() Not message.IsOffscreen, "le message revenu")
        End Sub

    End Class

End Namespace
