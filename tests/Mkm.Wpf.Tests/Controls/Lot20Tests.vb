Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Controls
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 20 : InputChips, FloatLabel, IftaLabel, ColorPicker.</summary>
    <TestFixture>
    Public Class Lot20Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 300, Optional hauteur As Double = 60) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(largeur, hauteur))
            element.Arrange(New Rect(0, 0, largeur, hauteur))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        ' ---- InputChips ----

        <Test>
        Public Sub InputChips_EntreeAjouteEtLaCroixRetire()
            Dim champ = Pret(New InputChips())
            Dim ajouts As New Collections.Generic.List(Of String)
            Dim retraits As New Collections.Generic.List(Of String)
            AddHandler champ.Add, Sub(s, e) ajouts.Add(e.Value)
            AddHandler champ.Remove, Sub(s, e) retraits.Add(e.Value)

            Assert.That(champ.Ajouter("  acier "), [Is].True)
            Assert.That(champ.Ajouter(""), [Is].False, "une saisie vide n'ajoute rien")
            champ.Ajouter("inox")
            Assert.That(champ.Value, [Is].EqualTo({"acier", "inox"}))
            Assert.That(champ.Puces.Count, [Is].EqualTo(2))
            Assert.That(champ.HasValues, [Is].True)

            Dim puce = champ.Puces(0)
            puce.RaiseEvent(New RoutedEventArgs(Chip.RemoveEvent, puce))
            Assert.That(champ.Value, [Is].EqualTo({"inox"}))
            Assert.That(ajouts, [Is].EqualTo({"acier", "inox"}))
            Assert.That(retraits, [Is].EqualTo({"acier"}))
        End Sub

        <Test>
        Public Sub InputChips_LimiteDoublonsEtSeparateur()
            Dim champ = Pret(New InputChips With {.Max = 2, .AllowDuplicate = False, .Separator = ","})
            champ.Ajouter("a")
            Assert.That(champ.Ajouter("a"), [Is].False, "doublon refusé")
            Dim saisie = Partie(Of TextBox)(champ, "PART_Input")
            saisie.Text = "b,c"
            Assert.That(champ.Value, [Is].EqualTo({"a", "b"}))
            Assert.That(saisie.Text, [Is].EqualTo("c"), "le morceau après le séparateur reste à saisir")
            Assert.That(champ.IsMaxedOut, [Is].True)
            Assert.That(champ.Ajouter("d"), [Is].False, "au-delà de Max")
        End Sub

        <Test>
        Public Sub InputChips_VideAussiHautQuAvecDesPuces()
            Dim champ = Pret(New InputChips(), largeur:=300, hauteur:=200)
            Dim vide = champ.DesiredSize.Height
            champ.Ajouter("acier")
            champ.UpdateLayout()
            Assert.That(champ.DesiredSize.Height, [Is].EqualTo(vide).Within(0.5), "la première puce ne le grandit pas")
            Assert.That(champ.Padding.Left, [Is].EqualTo(3), "avec des puces, 3 px de côté")
        End Sub

        <Test>
        Public Sub InputChips_LaSaisieSeNommeParSonLibelle()
            Dim libelle As New Label With {.Content = "Matières"}
            Dim champ As New InputChips With {.Placeholder = "Ajouter une matière"}
            AutomationProperties.SetLabeledBy(champ, libelle)
            Pret(champ)
            Dim saisie = Partie(Of TextBox)(champ, "PART_Input")
            Dim pair = Peers.UIElementAutomationPeer.CreatePeerForElement(saisie)
            Assert.That(pair.GetName(), [Is].EqualTo("Matières"), "le libellé l'emporte sur le texte d'aide")
            AutomationProperties.SetLabeledBy(champ, Nothing)
            Assert.That(pair.GetName(), [Is].EqualTo("Ajouter une matière"))
        End Sub

        ' ---- FloatLabel, IftaLabel ----

        <Test>
        Public Sub FloatLabel_SEnvoleQuandLeChampEstRempli()
            Dim champ As New InputText()
            Dim etiquette = Pret(New FloatLabel With {.Label = "Référence", .Content = champ})
            Assert.That(etiquette.IsFloating, [Is].False)
            champ.Text = "HS-00042"
            Assert.That(etiquette.IsFilled, [Is].True)
            Assert.That(etiquette.IsFloating, [Is].True)
            champ.Text = ""
            Assert.That(etiquette.IsFloating, [Is].False)
            champ.Placeholder = "HS-"
            Assert.That(etiquette.IsFloating, [Is].True, "un texte d'aide garde le libellé envolé")
            champ.Invalid = True
            Assert.That(etiquette.IsInvalid, [Is].True)
            Assert.That(AutomationProperties.GetName(champ), [Is].EqualTo("Référence"), "le libellé nomme le champ")
            etiquette.Label = "Code"
            Assert.That(AutomationProperties.GetName(champ), [Is].EqualTo("Code"), "et le suit")
            AutomationProperties.SetName(champ, "Nom donné")
            etiquette.Label = "Autre"
            Assert.That(AutomationProperties.GetName(champ), [Is].EqualTo("Nom donné"), "un nom donné passe avant")
        End Sub

        <Test>
        Public Sub FloatLabel_UnChoixRemplitLeChamp()
            Dim liste As New [Select]()
            liste.Items.Add("In Work")
            Dim etiquette = Pret(New FloatLabel With {.Label = "Maturité", .Content = liste})
            Assert.That(etiquette.IsFilled, [Is].False)
            liste.SelectedIndex = 0
            Assert.That(etiquette.IsFilled, [Is].True)
        End Sub

        <Test>
        Public Sub FloatLabel_DedansLeChampSAgrandit()
            Dim champ As New InputText()
            Dim etiquette = Pret(New FloatLabel With {.Label = "Référence", .Content = champ, .Variant = FloatLabelVariant.In})
            Assert.That(Double.IsNaN(champ.Height), [Is].True)
            Assert.That(champ.Padding.Top, [Is].GreaterThan(champ.Padding.Bottom), "le texte descend sous le libellé")
            etiquette.Variant = FloatLabelVariant.Over
            Assert.That(champ.ReadLocalValue(FrameworkElement.HeightProperty), [Is].EqualTo(DependencyProperty.UnsetValue))
        End Sub

        <Test>
        Public Sub IftaLabel_ToujoursEnvoleDansLeChamp()
            Dim etiquette = Pret(New IftaLabel With {.Label = "Référence", .Content = New InputText()})
            Assert.That(etiquette.IsFloating, [Is].True)
            Assert.That(etiquette.Variant, [Is].EqualTo(FloatLabelVariant.In))
            etiquette.Variant = FloatLabelVariant.Over
            Assert.That(etiquette.Variant, [Is].EqualTo(FloatLabelVariant.In), "IftaLabel reste dans le champ")
        End Sub

        ' ---- ColorPicker ----

        <Test>
        Public Sub ColorPicker_LaValeurEtLaCouleurSeSuivent()
            Dim choix = Pret(New ColorPicker())
            Assert.That(choix.Value, [Is].EqualTo("ff0000"))
            choix.Value = "#00FF00"
            Assert.That(choix.Color, [Is].EqualTo(Colors.Lime))
            Assert.That(choix.Value, [Is].EqualTo("00ff00"), "en minuscules, sans dièse")
            choix.Color = Color.FromRgb(&H3B, &H82, &HF6)
            Assert.That(choix.Value, [Is].EqualTo("3b82f6"))
            choix.Value = "pas une couleur"
            Assert.That(choix.Color, [Is].EqualTo(Color.FromRgb(&H3B, &H82, &HF6)), "une valeur fausse ne change rien")
        End Sub

        <Test>
        Public Sub ColorPicker_LesConversionsAllerRetour()
            For Each couleur In {Colors.Red, Colors.Lime, Colors.Blue, Color.FromRgb(&H3B, &H82, &HF6), Colors.Gray, Colors.Black}
                Dim t, s, l As Double
                ColorPicker.VersTsl(couleur, t, s, l)
                Assert.That(ColorPicker.VersCouleur(t, s, l), [Is].EqualTo(couleur), couleur.ToString())
            Next
        End Sub

        <Test>
        Public Sub ColorPicker_LePanneauChoisitNuanceEtTeinte()
            Dim choix = Pret(New ColorPicker With {.Inline = True}, 300, 200)
            choix.Panneau.ApplyTemplate()
            choix.Panneau.Tirer(New Point(0, 75), bande:=True)
            Assert.That(Math.Round(choix.Teinte), [Is].EqualTo(180))
            choix.Panneau.Tirer(New Point(150, 0), bande:=False)
            Assert.That(choix.Value, [Is].EqualTo("00ffff"), "teinte 180, saturation et luminosité pleines")
            choix.Panneau.Tirer(New Point(0, 150), bande:=False)
            Assert.That(choix.Value, [Is].EqualTo("000000"))
            Assert.That(choix.Teinte, [Is].EqualTo(180).Within(1), "la teinte se garde sur le noir")
        End Sub

    End Class

End Namespace
