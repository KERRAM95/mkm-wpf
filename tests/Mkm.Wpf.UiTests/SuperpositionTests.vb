Imports System
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.Input
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Ce qui s'ouvre par-dessus la page : le dialogue de confirmation, le dialogue, l'info-bulle, le Popover.</summary>
    <TestFixture>
    Public Class SuperpositionTests
        Inherits Scenario

        Private Const Question As String = "Mettre à jour le cartouche du plan avec les valeurs de 3DEXPERIENCE ?"

        <Test>
        Public Sub ConfirmDialog_EntreeAccepteEtEchapRefuse()
            Dim page = OuvrirLaDoc("confirmdialog")
            Dim reponse = Trouver("Reponse", page)

            Cliquer(Trouver("Demander", page))
            TrouverAilleurs(Question)
            Taper(VirtualKeyShort.RETURN)
            AttendreQue(Function() NomDe(reponse) = "Accepté", "Entrée : « Accepté »")

            Cliquer(Trouver("Demander", page))
            TrouverAilleurs(Question)
            Taper(VirtualKeyShort.ESCAPE)
            AttendreQue(Function() NomDe(reponse) = "Refusé", "Échap : « Refusé »")
        End Sub

        <Test>
        Public Sub ConfirmDialog_SesBoutonsRepondent()
            Dim page = OuvrirLaDoc("confirmdialog")
            Dim reponse = Trouver("Reponse", page)

            Cliquer(Trouver("Demander", page))
            Cliquer(TrouverAilleurs(T("locale.reject"), ControlType.Button))
            AttendreQue(Function() NomDe(reponse) = "Refusé", "« Non » : « Refusé »")
            Cliquer(Trouver("Demander", page))
            Cliquer(TrouverAilleurs(T("locale.accept"), ControlType.Button))
            AttendreQue(Function() NomDe(reponse) = "Accepté", "« Oui » : « Accepté »")
        End Sub

        <Test>
        Public Sub Dialog_RenommerRendLeNouveauNom()
            Dim page = OuvrirLaDoc("dialog")

            Cliquer(Trouver("Renommer", page))
            TrouverAilleurs("Renommer le plan")
            ' Le champ a le clavier, son texte choisi : la saisie le remplace.
            Ecrire("HS-00099")
            Taper(VirtualKeyShort.RETURN)
            AttendreQue(Function() NomDe(Trouver("Resultat", page)) = "Renommé : HS-00099", "« Renommé : HS-00099 »")
        End Sub

        <Test>
        Public Sub Dialog_EchapEtLaCroixAnnulent()
            Dim page = OuvrirLaDoc("dialog")
            Dim resultat = Trouver("Resultat", page)

            Cliquer(Trouver("Renommer", page))
            TrouverAilleurs("Renommer le plan")
            Taper(VirtualKeyShort.ESCAPE)
            AttendreQue(Function() NomDe(resultat) = "Annulé", "Échap : « Annulé »")

            Cliquer(Trouver("Renommer", page))
            Dim dialogue = TrouverAilleurs("Renommer le plan")
            Cliquer(TrouverParNom(T("aria.close"), ControlType.Button, dialogue))
            AttendreQue(Function() Referme("Renommer le plan"), "la croix ferme le dialogue")
            AttendreQue(Function() NomDe(resultat) = "Annulé", "la croix : « Annulé »")
        End Sub

        <Test>
        Public Sub Tooltip_LeSurvolMontreLInfoBulle()
            Dim page = OuvrirLaDoc("tooltip")

            Survoler(Trouver("Survol", page))
            TrouverAilleurs("Met à jour le cartouche du plan depuis 3DEXPERIENCE.")
        End Sub

        ' Le défaut vu par Mourad : survolé pendant que la molette faisait défiler la page, l'info-bulle restait à sa place,
        ' détachée de son bouton. Comme mkm, la page qui défile la referme.
        <Test>
        Public Sub Tooltip_SeFermeQuandLaPageDefile()
            Dim page = OuvrirLaDoc("tooltip")
            Const Texte = "Met à jour le cartouche du plan depuis 3DEXPERIENCE."

            Survoler(Trouver("Survol", page))
            TrouverAilleurs(Texte)
            Mouse.Scroll(-3)
            Wait.UntilInputIsProcessed()
            AttendreQue(Function() Referme(Texte), "la page a défilé : l'info-bulle refermée")
        End Sub

        <Test>
        Public Sub Popover_SOuvreAuClicEtSeFermeParEchapOuSonBouton()
            Dim page = OuvrirLaDoc("popover")
            Dim partager = Trouver("Partager", page)

            Cliquer(partager)
            TrouverAilleurs("Partager le document")
            TrouverAilleurs("Copier le lien", ControlType.Button)
            Taper(VirtualKeyShort.ESCAPE)
            AttendreQue(Function() Referme("Copier le lien"), "Échap le ferme")

            Cliquer(partager)
            Cliquer(TrouverAilleurs("Copier le lien", ControlType.Button))
            AttendreQue(Function() NomDe(Trouver("Journal", page)) = "Lien copié.", "son bouton agit")
            AttendreQue(Function() Referme("Copier le lien"), "et le ferme")
        End Sub

    End Class

End Namespace
