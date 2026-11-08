Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>La galerie des icônes : une tuile par icône de <c>Icons</c>, filtrée par la recherche.</summary>
    Friend NotInheritable Class GalerieDIcones

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim galerie = DirectCast(exemple.FindName("Galerie"), WrapPanel)
            Dim filtre = DirectCast(exemple.FindName("Filtre"), InputText)
            Dim icones = GetType(Icons).GetProperties(BindingFlags.Public Or BindingFlags.Static).
                Where(Function(p) GetType(Geometry).IsAssignableFrom(p.PropertyType)).OrderBy(Function(p) p.Name).ToList()
            For Each icone In icones
                galerie.Children.Add(Tuile(icone.Name, DirectCast(icone.GetValue(Nothing), Geometry)))
            Next
            AddHandler filtre.TextChanged, Sub()
                                               Dim cherche = filtre.Text.Trim()
                                               For Each tuile As FrameworkElement In galerie.Children
                                                   Dim visible = cherche.Length = 0 OrElse
                                                       CultureInfo.InvariantCulture.CompareInfo.IndexOf(CStr(tuile.Tag), cherche, CompareOptions.IgnoreCase) >= 0
                                                   tuile.Visibility = If(visible, Visibility.Visible, Visibility.Collapsed)
                                               Next
                                           End Sub
        End Sub

        ' Une tuile : l'icône, son nom dessous ; son info-bulle donne ce qu'on écrit en XAML.
        Private Shared Function Tuile(nom As String, dessin As Geometry) As FrameworkElement
            Dim pile As New StackPanel With {.HorizontalAlignment = HorizontalAlignment.Center}
            pile.Children.Add(New Icon With {.Source = dessin, .Width = 24, .Height = 24, .Margin = New Thickness(0, 4, 0, 8)})
            pile.Children.Add(New TextBlock With {.Text = nom, .FontSize = 11, .TextAlignment = TextAlignment.Center, .TextTrimming = TextTrimming.CharacterEllipsis})
            Dim cadre As New Border With {.Width = 112, .Height = 80, .Margin = New Thickness(0, 0, 8, 8), .Padding = New Thickness(6),
                                          .CornerRadius = New CornerRadius(6), .BorderThickness = New Thickness(1), .Child = pile,
                                          .Tag = nom, .ToolTip = $"{{x:Static mkm:Icons.{nom}}}"}
            cadre.SetResourceReference(Border.BorderBrushProperty, ThemeResources.Prefix & "content.border.color")
            Automation.AutomationProperties.SetName(cadre, nom)
            Return cadre
        End Function

    End Class

End Namespace
