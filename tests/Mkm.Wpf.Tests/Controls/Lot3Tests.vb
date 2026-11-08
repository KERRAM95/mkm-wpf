Imports System.Globalization
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Markup
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Theming
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests.Controls

    ''' <summary>Les composants du troisième lot : Tag, IdCard, SelectButton et InputNumber.</summary>
    <TestFixture>
    Public Class Lot3Tests

        Private Shared Function Pret(Of T As FrameworkElement)(element As T) As T
            element.BeginInit()
            element.EndInit()
            TryCast(element, Control)?.ApplyTemplate()
            element.Measure(New Size(600, 300))
            element.Arrange(New Rect(0, 0, 600, 300))
            element.UpdateLayout()
            Return element
        End Function

        Private Shared Function Partie(Of T As DependencyObject)(controle As Control, nom As String) As T
            Return DirectCast(controle.Template.FindName(nom, controle), T)
        End Function

        <Test>
        Public Sub Tag_LaGraviteEtLaCouleurVenueDAilleurs()
            Dim etiquette = Pret(New Tag With {.Content = "À jour", .Severity = TagSeverity.Success})
            Assert.That(etiquette.Background, [Is].SameAs(Application.Current.TryFindResource(ThemeResources.Prefix & "tag.success.background")))

            etiquette.Color = Brushes.White

            Assert.That(etiquette.Background, [Is].SameAs(Brushes.White))
            Assert.That(DirectCast(etiquette.Foreground, SolidColorBrush).Color, [Is].EqualTo(Color.FromRgb(&H11, &H11, &H11)), "le presque noir de mkm, lisible sur le blanc")
        End Sub

        <TestCase("#007DA3", "White")>
        <TestCase("#FFFFFF", "Black")>
        <TestCase("#F2C94C", "Black")>
        Public Sub Tag_LeTexteLePlusLisible(fond As String, attendu As String)
            Dim pinceau = New SolidColorBrush(DirectCast(ColorConverter.ConvertFromString(fond), Color))

            Assert.That(DirectCast(Tag.ReadableForeground(pinceau), SolidColorBrush).Color, [Is].EqualTo(If(attendu = "White", Colors.White, Color.FromRgb(&H11, &H11, &H11))))
        End Sub

        <Test>
        Public Sub IdCard_SesDetailsEtSonRepli()
            Dim carte = Pret(New IdCard With {.Title = "Carter", .Revision = "A", .Owner = "M. Kerram", .Maturity = "In Work"})
            carte.Details.Add("HS-00042")
            ' Un détail élément est son propre conteneur : le style des détails doit aussi s'y appliquer.
            carte.Details.Add(New TextBlock With {.Text = "Modifié le 07/11/2026"})
            carte.UpdateLayout()
            Dim details = Partie(Of UIElement)(carte, "Details")
            Assert.That(Partie(Of UIElement)(carte, "Vignette").Visibility, [Is].EqualTo(Visibility.Collapsed), "sans vignette")
            Assert.That(Partie(Of UIElement)(carte, "Maturite").Visibility, [Is].EqualTo(Visibility.Visible))

            Partie(Of ButtonBase)(carte, IdCard.PartCollapseButton).RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))

            Assert.That(carte.IsCollapsed, [Is].True)
            Assert.That(details.Visibility, [Is].EqualTo(Visibility.Collapsed))
        End Sub

        <Test>
        Public Sub IdCard_LaCouleurDeMaturiteDeLApplication()
            Dim carte = Pret(New IdCard With {.Title = "Carter", .Maturity = "Released", .MaturityColor = Brushes.DarkGreen})

            Dim etiquette = Partie(Of Tag)(carte, "Maturite")

            Assert.That(etiquette.Background, [Is].SameAs(Brushes.DarkGreen))
            Assert.That(etiquette.Foreground, [Is].SameAs(Brushes.White))
        End Sub

        <Test>
        Public Sub SelectButton_UnNouveauClicEnleveLeChoix()
            Dim groupe As New SelectButton()
            Dim plan As New SelectButtonItem With {.Content = "Plan"}
            Dim article As New SelectButtonItem With {.Content = "Article"}
            groupe.Items.Add(plan)
            groupe.Items.Add(article)
            Pret(groupe)
            Assert.That({plan.IsFirst, plan.IsLast, article.IsFirst, article.IsLast}, [Is].EqualTo({True, False, False, True}))

            plan.IsSelected = True
            plan.RaiseEvent(New Input.MouseButtonEventArgs(Input.Mouse.PrimaryDevice, 0, Input.MouseButton.Left) With {.RoutedEvent = UIElement.MouseLeftButtonDownEvent})

            Assert.That(groupe.SelectedItem, [Is].Null)
        End Sub

        <Test>
        Public Sub InputNumber_EcritEtLitLeNombre()
            Dim saisie = Pret(New InputNumber With {.Language = XmlLanguage.GetLanguage("fr-FR"), .MinFractionDigits = 2, .MaxFractionDigits = 2, .Suffix = " kg"})

            Assert.That(saisie.Format(1234.5), [Is].EqualTo("1" & CultureInfo.GetCultureInfo("fr-FR").NumberFormat.NumberGroupSeparator & "234,50 kg"))
            Assert.That(saisie.Parse("1 234,50 kg"), [Is].EqualTo(1234.5))
            Assert.That(saisie.Parse(""), [Is].Null)
        End Sub

        <Test>
        Public Sub InputNumber_LePasDansSesBornes()
            Dim saisie = Pret(New InputNumber With {.Value = 9, .Minimum = 1, .Maximum = 10, .MaxFractionDigits = 0, .ShowButtons = True})

            saisie.Spin(1)
            saisie.Spin(1)
            Assert.That(saisie.Value, [Is].EqualTo(10.0))

            Partie(Of ButtonBase)(saisie, InputNumber.PartDecrement).RaiseEvent(New RoutedEventArgs(ButtonBase.ClickEvent))

            Assert.That(saisie.Value, [Is].EqualTo(9.0))
            Assert.That(Partie(Of TextBox)(saisie, InputNumber.PartTextBox).Text, [Is].EqualTo("9"))
        End Sub

        <Test>
        Public Sub InputNumber_AlHorizontale()
            Dim saisie = Pret(New InputNumber With {.ShowButtons = True, .ButtonLayout = InputNumberButtonLayout.Horizontal})

            Assert.That(Partie(Of InputText)(saisie, InputNumber.PartTextBox).CornerRadius, [Is].EqualTo(New CornerRadius(0)))
        End Sub

    End Class

End Namespace
