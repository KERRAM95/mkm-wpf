Imports System.Linq
Imports System.Windows
Imports System.Windows.Automation.Peers
Imports System.Windows.Controls
Imports Mkm.Wpf.Controls
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Le lot 19 : Password, InputMask, InputOtp, KeyFilter.</summary>
    <TestFixture>
    Public Class Lot19Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T, Optional largeur As Double = 300, Optional hauteur As Double = 40) As T
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

        ' ---- Password ----

        <Test>
        Public Sub Password_LaForceSelonLesReglesDeMkm()
            Dim champ = Pret(New Password())
            Assert.That(champ.Force(""), [Is].EqualTo(PasswordStrength.None))
            Assert.That(champ.Force("abc"), [Is].EqualTo(PasswordStrength.Weak))
            Assert.That(champ.Force("abcd12"), [Is].EqualTo(PasswordStrength.Medium), "6 caractères, minuscules et chiffres")
            Assert.That(champ.Force("Abcdef12"), [Is].EqualTo(PasswordStrength.Strong), "8 caractères, les trois genres")
        End Sub

        <Test>
        Public Sub Password_LeMotDePasseVaDUnChampALAutre()
            Dim champ = Pret(New Password With {.WeakLabel = "Trop simple"})
            Dim changements = 0
            AddHandler champ.PasswordChanged, Sub(s, e) changements += 1
            Dim masque = Partie(Of PasswordBox)(champ, "PART_PasswordBox")
            Dim clair = Partie(Of TextBox)(champ, "PART_TextBox")

            masque.Password = "abc"
            Assert.That(champ.Password, [Is].EqualTo("abc"))
            Assert.That(clair.Text, [Is].EqualTo("abc"), "le champ en clair suit")
            Assert.That(champ.HasPassword, [Is].True)
            Assert.That(champ.Strength, [Is].EqualTo(PasswordStrength.Weak))
            Assert.That(champ.StrengthText, [Is].EqualTo("Trop simple"))

            clair.Text = "Abcdef12"
            Assert.That(masque.Password, [Is].EqualTo("Abcdef12"))
            Assert.That(champ.Strength, [Is].EqualTo(PasswordStrength.Strong))
            champ.Password = ""
            Assert.That(champ.HasPassword, [Is].False)
            Assert.That(changements, [Is].EqualTo(3))
        End Sub

        <Test>
        Public Sub Password_LOeilMontreLeChampEnClair()
            Dim champ = Pret(New Password With {.ToggleMask = True})
            Dim oeil = Partie(Of Primitives.ButtonBase)(champ, "PART_Toggle")
            Assert.That(oeil.Visibility, [Is].EqualTo(Visibility.Visible))
            oeil.RaiseEvent(New RoutedEventArgs(Primitives.ButtonBase.ClickEvent, oeil))
            champ.UpdateLayout()
            Assert.That(champ.IsUnmasked, [Is].True)
            Assert.That(Partie(Of TextBox)(champ, "PART_TextBox").Visibility, [Is].EqualTo(Visibility.Visible))
            Assert.That(Partie(Of PasswordBox)(champ, "PART_PasswordBox").Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub Password_LesChampsPortentLeTexteDAide()
            Dim champ = Pret(New Password With {.Placeholder = "Mot de passe"})
            Dim masque = Partie(Of PasswordBox)(champ, "PART_PasswordBox")
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(masque).GetName(), [Is].EqualTo("Mot de passe"))
        End Sub

        ' ---- InputMask ----

        Private Shared Function Masque(modele As String) As InputMask
            Return Pret(New InputMask With {.Mask = modele})
        End Function

        <Test>
        Public Sub InputMask_LaFrappeRemplitLesCasesEtSauteLesSeparateurs()
            Dim champ = Masque("99-999")
            For Each c In "12x345"
                champ.Taper(c)
            Next
            Assert.That(champ.Text, [Is].EqualTo("12-345"), "« x » refusé, le tiret sauté")
            Assert.That(champ.UnmaskedValue, [Is].EqualTo("12345"))
            Assert.That(champ.IsComplete, [Is].True)

            champ.Effacer(False)
            Assert.That(champ.Text, [Is].EqualTo("12-34_"))
            Assert.That(champ.IsComplete, [Is].False)
        End Sub

        <Test>
        Public Sub InputMask_UnTexteDonneSeRangeDansLeModele()
            Dim champ = Masque("HS-99999")
            champ.Text = "42"
            Assert.That(champ.Text, [Is].Empty, "incomplet, hors du clavier : vidé (AutoClear)")
            champ.Text = "00042"
            Assert.That(champ.Text, [Is].EqualTo("HS-00042"))
            Assert.That(champ.UnmaskedValue, [Is].EqualTo("00042"))

            Dim garde = Masque("aaa-999")
            garde.AutoClear = False
            garde.Text = "ABC1"
            Assert.That(garde.Text, [Is].EqualTo("ABC-1"), "AutoClear=False : coupé après le dernier caractère")
        End Sub

        <Test>
        Public Sub InputMask_LaPartieFacultative()
            Dim champ = Masque("99?-99")
            champ.Text = "12"
            Assert.That(champ.Text, [Is].EqualTo("12"))
            Assert.That(champ.IsComplete, [Is].True)
        End Sub

        ' ---- InputOtp ----

        <Test>
        Public Sub InputOtp_UnCodeColleRemplitLesCases()
            Dim code = Pret(New InputOtp())
            Assert.That(code.Cases.Count, [Is].EqualTo(4))
            code.Coller("123456")
            Assert.That(code.Value, [Is].EqualTo("1234"), "coupé à la longueur")
            Assert.That(code.Cases.Select(Function(c) DirectCast(c, TextBox).Text), [Is].EqualTo({"1", "2", "3", "4"}))

            code.IntegerOnly = True
            code.Coller("98a7")
            Assert.That(code.Value, [Is].EqualTo("1234"), "des chiffres seulement")
        End Sub

        <Test>
        Public Sub InputOtp_LaValeurSeRepartitEtLeMasqueChangeLesCases()
            Dim code = Pret(New InputOtp With {.Length = 6, .Value = "42"})
            Assert.That(code.Cases.Count, [Is].EqualTo(6))
            Assert.That(DirectCast(code.Cases(1), TextBox).Text, [Is].EqualTo("2"))
            code.Mask = True
            Assert.That(code.Cases.All(Function(c) TypeOf c Is PasswordBox), [Is].True)
            Assert.That(DirectCast(code.Cases(0), PasswordBox).Password, [Is].EqualTo("4"))
            Assert.That(UIElementAutomationPeer.CreatePeerForElement(code).GetAutomationControlType(), [Is].EqualTo(AutomationControlType.Group))
        End Sub

        ' ---- KeyFilter ----

        <Test>
        Public Sub KeyFilter_LesFiltresDeMkmEtLesExpressions()
            Assert.That(KeyFilter.Accepte("pint", "123"), [Is].True)
            Assert.That(KeyFilter.Accepte("pint", "12-3"), [Is].False)
            Assert.That(KeyFilter.Accepte("int", "-12"), [Is].True)
            Assert.That(KeyFilter.Accepte("hex", "0aF"), [Is].True)
            Assert.That(KeyFilter.Accepte("alpha", "abc1"), [Is].False)
            Assert.That(KeyFilter.Accepte("[A-Z]", "ABC"), [Is].True)
            Assert.That(KeyFilter.Accepte("[A-Z]", "AbC"), [Is].False)
            Assert.That(KeyFilter.Accepte("[", "tout"), [Is].True, "une expression fausse ne filtre rien")
        End Sub

        <Test>
        Public Sub KeyFilter_SePoseEtSeRetire()
            Dim champ As New InputText()
            KeyFilter.SetPattern(champ, "pint")
            Assert.That(KeyFilter.GetPattern(champ), [Is].EqualTo("pint"))
            KeyFilter.SetPattern(champ, Nothing)
            Assert.That(KeyFilter.GetPattern(champ), [Is].Null)
        End Sub

    End Class

End Namespace
