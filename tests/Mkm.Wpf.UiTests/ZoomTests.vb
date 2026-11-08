Imports System
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>Le zoom de toute l'interface : ses raccourcis, la fenêtre et ses listes qui suivent, le retour à 100 %.</summary>
    <TestFixture>
    Public Class ZoomTests
        Inherits Scenario

        ' Le texte d'un palier, tel que la liste du zoom l'écrit (« 110 % », l'espace insécable de la France).
        Private Shared Function Palier(element As AutomationElement) As String
            Return NomDe(element).Replace(ChrW(&HA0), " "c).Replace(ChrW(&H202F), " "c)
        End Function

        <Test>
        Public Sub CtrlPlusAgranditToutEtCtrl0LeRamene()
            Dim page = OuvrirLaDoc("select")
            Dim recherche = Trouver("Recherche")
            Dim zoom = Trouver("ChoixDuZoom").AsComboBox()
            Dim maturite = Trouver("Maturite", page)
            Dim hauteur = recherche.BoundingRectangle.Height
            Try
                Cliquer(recherche)
                Raccourci(VirtualKeyShort.CONTROL, VirtualKeyShort.ADD)
                AttendreQue(Function() Math.Abs(recherche.BoundingRectangle.Height - hauteur * 1.1) <= 2, "Ctrl + : tout à 110 %")
                AttendreQue(Function() Palier(zoom.SelectedItem) = "110 %", "la liste du zoom suit")
                Raccourci(VirtualKeyShort.CONTROL, VirtualKeyShort.ADD)
                Raccourci(VirtualKeyShort.CONTROL, VirtualKeyShort.ADD)
                AttendreQue(Function() Math.Abs(recherche.BoundingRectangle.Height - hauteur * 1.5) <= 2, "deux fois de plus : 150 %")

                ' Une liste ouverte, dans sa fenêtre à part, suit le zoom de son champ : ses choix de 22 px en font 33.
                Cliquer(maturite)
                Dim fige = Attendre(Function() If(maturite.FindFirstDescendant(Function(cf) cf.ByName("Frozen").And(cf.ByControlType(ControlType.ListItem))),
                                                 ChercherAilleurs("Frozen", ControlType.ListItem)), "« Frozen » dans la liste")
                AttendreQue(Function() Math.Abs(fige.BoundingRectangle.Height - 33) <= 3, "la liste à 150 %")
                Taper(VirtualKeyShort.ESCAPE)
            Finally
                Cliquer(recherche)
                Raccourci(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_0)
            End Try
            AttendreQue(Function() Math.Abs(recherche.BoundingRectangle.Height - hauteur) <= 1, "Ctrl + 0 : 100 %")
            Assert.That(Palier(zoom.SelectedItem), [Is].EqualTo("100 %"))
        End Sub

        <Test>
        Public Sub LaListeDuZoomLeChange()
            Dim zoom = Trouver("ChoixDuZoom").AsComboBox()
            Dim recherche = Trouver("Recherche")
            Dim hauteur = recherche.BoundingRectangle.Height
            Try
                zoom.Select(5)
                AttendreQue(Function() Math.Abs(recherche.BoundingRectangle.Height - hauteur * 1.5) <= 2, "150 % choisi dans la liste")
                ' Ce qui ne tient plus dans la fenêtre se trouve en la faisant défiler, au lieu d'être caché.
                Dim zone = Trouver("DefilementDuZoom").Patterns.Scroll.Pattern
                AttendreQue(Function() zone.HorizontallyScrollable.Value AndAlso zone.VerticallyScrollable.Value, "la fenêtre défile")
            Finally
                zoom.Select(2)
            End Try
            AttendreQue(Function() Math.Abs(recherche.BoundingRectangle.Height - hauteur) <= 1, "100 % de nouveau")
            Dim apres = Trouver("DefilementDuZoom").Patterns.Scroll.Pattern
            AttendreQue(Function() Not apres.HorizontallyScrollable.Value, "à 100 %, plus rien à faire défiler")
        End Sub

    End Class

End Namespace
