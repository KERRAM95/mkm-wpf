Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Threading
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests

    ''' <summary>Le zoom de toute l'interface (MkmResources.Zoom) : ses paliers, ses raccourcis, les fenêtres qui le suivent.</summary>
    <TestFixture>
    Public Class ZoomTests

        Private Shared ReadOnly Property Reglages As MkmResources
            Get
                If MkmResources.Current Is Nothing Then Dim nouveaux As New MkmResources()
                Return MkmResources.Current
            End Get
        End Property

        <TearDown>
        Public Sub Retablir()
            Reglages.ResetZoom()
        End Sub

        ' Ce que le répartiteur a en attente (le Loaded d'une fenêtre).
        Private Shared Sub Vider()
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, New System.Action(Sub()
                                                                                                     End Sub))
        End Sub

        <Test>
        Public Sub LesPaliersEtLesBornes()
            Dim r = Reglages
            r.Zoom = 1.0
            r.ZoomIn()
            Assert.That(r.Zoom, [Is].EqualTo(1.1))
            r.ZoomIn()
            Assert.That(r.Zoom, [Is].EqualTo(1.25))
            r.Zoom = 1.3
            r.ZoomIn()
            Assert.That(r.Zoom, [Is].EqualTo(1.5), "d'un zoom hors palier, au palier au-dessus")
            r.Zoom = 1.3
            r.ZoomOut()
            Assert.That(r.Zoom, [Is].EqualTo(1.25))
            r.Zoom = 5
            Assert.That(r.Zoom, [Is].EqualTo(MkmResources.MaxZoom), "borné à 200 %")
            r.ZoomIn()
            Assert.That(r.Zoom, [Is].EqualTo(2.0))
            r.Zoom = 0.1
            Assert.That(r.Zoom, [Is].EqualTo(MkmResources.MinZoom))
            r.ResetZoom()
            Assert.That(r.Zoom, [Is].EqualTo(1.0))
        End Sub

        <Test>
        Public Sub ZoomChanged_SeulementQuandIlChange()
            Dim r = Reglages
            Dim fois = 0
            Dim compter As EventHandler = Sub(s, e) fois += 1
            AddHandler r.ZoomChanged, compter
            Try
                r.Zoom = 1.5
                r.Zoom = 1.5
                r.ResetZoom()
                Assert.That(fois, [Is].EqualTo(2))
            Finally
                RemoveHandler r.ZoomChanged, compter
            End Try
        End Sub

        <Test>
        Public Sub LesTouchesDuZoom()
            Assert.That(Agrandissement.ActionDuClavier(Key.OemPlus, "="c), [Is].EqualTo(1), "Ctrl + (la touche = / +)")
            Assert.That(Agrandissement.ActionDuClavier(Key.Add, ControlChars.NullChar), [Is].EqualTo(1), "le + du pavé")
            Assert.That(Agrandissement.ActionDuClavier(Key.OemMinus, "-"c), [Is].EqualTo(-1))
            Assert.That(Agrandissement.ActionDuClavier(Key.D6, "-"c), [Is].EqualTo(-1), "le − d'un clavier AZERTY, sur la touche 6")
            Assert.That(Agrandissement.ActionDuClavier(Key.Subtract, ControlChars.NullChar), [Is].EqualTo(-1))
            Assert.That(Agrandissement.ActionDuClavier(Key.D0, "à"c), [Is].EqualTo(0), "Ctrl + 0, « à » sur un clavier AZERTY")
            Assert.That(Agrandissement.ActionDuClavier(Key.D6, "6"c), [Is].Null, "Ctrl + 6 sur un clavier QWERTY : rien")
            Assert.That(Agrandissement.ActionDuClavier(Key.S, "S"c), [Is].Null)
        End Sub

        <Test>
        Public Sub UneFenetreSuitLeZoomEtSonTexteResteNet()
            Dim fenetre As New Window With {.Content = New TextBlock With {.Text = "Plan HS-00042"}, .Width = 200, .Height = 100,
                                            .Left = -10000, .Top = -10000, .ShowInTaskbar = False, .ShowActivated = False,
                                            .WindowStyle = WindowStyle.None}
            Dim r = Reglages
            fenetre.Show()
            Try
                Vider()
                Dim racine = Agrandissement.RacineDe(fenetre)
                Dim texte = DirectCast(fenetre.Content, TextBlock)
                ' À 100 %, le mode de la bibliothèque (Display, si MkmResources est né avant le premier élément WPF).
                Dim parDefaut = TextOptions.GetTextFormattingMode(texte)
                Assert.That(racine.LayoutTransform.Value.IsIdentity, [Is].True, "à 100 %, telle quelle")

                r.Zoom = 1.5
                Assert.That(racine.LayoutTransform.Value.M11, [Is].EqualTo(1.5))
                Assert.That(TextOptions.GetTextFormattingMode(texte), [Is].EqualTo(TextFormattingMode.Ideal),
                            "agrandi, le texte dans le mode qui reste net")

                r.ResetZoom()
                Assert.That(racine.LayoutTransform.Value.IsIdentity, [Is].True)
                Assert.That(TextOptions.GetTextFormattingMode(texte), [Is].EqualTo(parDefaut))
            Finally
                fenetre.Close()
            End Try
        End Sub

        <Test>
        Public Sub UneFenetreOuverteApresPrendLeZoom()
            Dim r = Reglages
            r.Zoom = 1.25
            Dim fenetre As New Window With {.Content = New TextBlock(), .Width = 200, .Height = 100, .Left = -10000, .Top = -10000,
                                            .ShowInTaskbar = False, .ShowActivated = False, .WindowStyle = WindowStyle.None}
            fenetre.Show()
            Try
                Vider()
                Assert.That(Agrandissement.RacineDe(fenetre).LayoutTransform.Value.M11, [Is].EqualTo(1.25))
            Finally
                fenetre.Close()
            End Try
        End Sub

        <Test>
        Public Sub AgrandieUneFenetreDefileAuLieuDeCacher()
            Dim contenu As New Border()
            Dim fenetre As New Window With {.Content = contenu, .Width = 600, .Height = 400, .Left = -10000, .Top = -10000,
                                            .ShowInTaskbar = False, .ShowActivated = False, .WindowStyle = WindowStyle.None}
            Dim r = Reglages
            fenetre.Show()
            Try
                Vider()
                r.Zoom = 2.0
                Vider()
                Dim zone = Descendants(fenetre).OfType(Of DefilementDuZoom)().FirstOrDefault()
                Assert.That(zone, [Is].Not.Null, "agrandie, la fenêtre a sa zone qui défile")
                Assert.That(fenetre.Content, [Is].SameAs(contenu), "son contenu reste le même")
                Assert.That(zone.ScrollableWidth, [Is].GreaterThan(0), "la mise en page minimale (sa propre largeur) dépasse la place")
                Assert.That(zone.ScrollableHeight, [Is].GreaterThan(0))

                r.ResetZoom()
                Vider()
                Assert.That(zone.HorizontalScrollBarVisibility, [Is].EqualTo(ScrollBarVisibility.Disabled), "à 100 %, elle s'efface")
                Assert.That(zone.ScrollableWidth, [Is].EqualTo(0))
            Finally
                fenetre.Close()
            End Try
        End Sub

        Private Shared Iterator Function Descendants(racine As DependencyObject) As IEnumerable(Of DependencyObject)
            For i = 0 To VisualTreeHelper.GetChildrenCount(racine) - 1
                Dim enfant = VisualTreeHelper.GetChild(racine, i)
                Yield enfant
                For Each petit In Descendants(enfant)
                    Yield petit
                Next
            Next
        End Function

        <Test>
        Public Sub LaTailleDuTexteDeWindows()
            Assert.That(MkmResources.WindowsTextScale, [Is].InRange(1.0, 2.25))
        End Sub

    End Class

End Namespace
