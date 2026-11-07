Imports System.Globalization
Imports System.Resources
Imports System.Windows
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' Application de la vitrine : ses textes, sa langue de départ, puis <see cref="MainWindow"/>.
    ''' </summary>
    Partial Public Class Application

        ''' <summary>Avant la première fenêtre : les textes de la vitrine et sa langue de départ.</summary>
        Protected Overrides Sub OnStartup(e As StartupEventArgs)
            ' Textes/Vitrine.resx (anglais) et Vitrine.fr.resx ; VB nomme la ressource par son fichier.
            Dim textes As New ResourceManager("Vitrine", GetType(Application).Assembly)
            Localizer.Current.AddSource(New ResxTextSource(textes, "en", "fr"))

            ' La langue d'affichage de Windows, ou la plus proche des langues de la vitrine.
            Localizer.Current.Language = Localizer.Current.BestLanguage(CultureInfo.CurrentUICulture)

            MyBase.OnStartup(e)
        End Sub

    End Class

End Namespace
