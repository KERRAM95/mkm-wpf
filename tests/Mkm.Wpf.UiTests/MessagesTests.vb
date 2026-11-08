Imports System
Imports System.Threading
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Ce qui renseigne l'utilisateur : Message, Toast, ProgressBar, BlockUI.</summary>
    <TestFixture>
    Public Class MessagesTests
        Inherits Scenario

        <Test>
        Public Sub Message_SaCroixLeFermeEtIlRevient()
            Dim page = OuvrirLaDoc("message")
            Dim message = Trouver("Ephemere", page)

            Assert.That(message.Properties.LiveSetting.ValueOrDefault, [Is].EqualTo(LiveSetting.Assertive), "annoncé dès qu'il paraît")
            Cliquer(TrouverParNom(T("aria.close"), ControlType.Button, message))
            AttendreQue(Function() message.IsOffscreen, "le message fermé")
            Cliquer(Trouver("Remontrer", page))
            AttendreQue(Function() Not message.IsOffscreen, "le message revenu")
        End Sub

        <Test>
        Public Sub Toast_UneNotificationParaitEtSaCroixLaFerme()
            Dim page = OuvrirLaDoc("toast")
            Dim notifications = Trouver("Notifications")

            Cliquer(Trouver("Notifier", page))
            Dim detail = "Le plan HS-00042 est ouvert en lecture seule."
            TrouverParNom(detail, dans:=notifications)
            Cliquer(TrouverParNom(T("aria.close"), ControlType.Button, notifications))
            AttendreQue(Function() notifications.FindFirstDescendant(Function(cf) cf.ByName(detail)) Is Nothing, "la notification fermée")
        End Sub

        <Test>
        Public Sub ProgressBar_AvanceDUnQuart()
            Dim page = OuvrirLaDoc("progressbar")
            Dim barre = Trouver("Avancement", page)

            Cliquer(Trouver("Avancer", page))
            AttendreQue(Function() barre.Patterns.RangeValue.Pattern.Value.Value = 65, "la barre à 65")
            Assert.That(NomDe(Trouver("Pourcentage", page)), [Is].EqualTo("65 %"))
        End Sub

        <Test>
        Public Sub BlockUI_LeVoileRefuseLeClicPuisLeRend()
            Dim page = OuvrirLaDoc("blockui")
            Dim champ = Trouver("ChampVoile", page)

            Cliquer(Trouver("Bloquer", page))
            Cliquer(champ)
            Thread.Sleep(300)
            Assert.That(AFocus(champ), [Is].False, "sous le voile, le champ ne prend pas le clavier")

            Thread.Sleep(3200)
            Cliquer(champ)
            AttendreQue(Function() AFocus(champ), "le voile levé, le champ prend le clavier")
        End Sub

    End Class

End Namespace
