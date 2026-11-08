Imports System
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Ce qui s'ouvre par-dessus la page : le dialogue de confirmation, le dialogue, l'info-bulle.</summary>
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

    End Class

End Namespace
