Imports System
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Windows
Imports System.Windows.Threading
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' La vitrine pilotée par les tests d'interaction (<c>Mkm.Wpf.Showcase --tests [dossier]</c>, voir
    ''' tests/Mkm.Wpf.UiTests) : en français, dates et nombres compris, la fenêtre en haut à gauche de l'écran, au premier
    ''' plan. Une erreur non gérée ne ferme pas la vitrine : elle s'écrit dans <c>erreurs.txt</c> du dossier donné, que les
    ''' tests relisent après chaque scénario.
    ''' </summary>
    Friend Module ModeTests

        ''' <summary>Le fichier des erreurs non gérées, ou Nothing sans dossier.</summary>
        Private _journal As String

        ''' <summary>Ouvre la fenêtre de la vitrine pour les tests.</summary>
        Friend Sub Demarrer(application As Application, dossier As String)
            Dim francais = CultureInfo.GetCultureInfo("fr-FR")
            Thread.CurrentThread.CurrentCulture = francais
            Thread.CurrentThread.CurrentUICulture = francais
            CultureInfo.DefaultThreadCurrentCulture = francais
            CultureInfo.DefaultThreadCurrentUICulture = francais
            Localizer.Current.Language = Localizer.Current.BestLanguage(francais)

            If Not String.IsNullOrEmpty(dossier) Then
                Directory.CreateDirectory(dossier)
                _journal = Path.Combine(dossier, "erreurs.txt")
                If File.Exists(_journal) Then File.Delete(_journal)
            End If
            AddHandler application.DispatcherUnhandledException, AddressOf ErreurNonGeree
            ' Un exemple de la documentation qui ne se lit pas fait échouer le scénario en cours, comme une erreur.
            Documentation.Signalement.Sortie = Sub(texte) Ecrire(texte)

            ' Toute la fenêtre à l'écran, même petit (la CI a un écran de 1024 × 768).
            Dim ecran = SystemParameters.WorkArea
            Dim fenetre As New MainWindow With {
                .WindowStartupLocation = WindowStartupLocation.Manual,
                .Left = ecran.Left,
                .Top = ecran.Top,
                .Topmost = True
            }
            fenetre.Width = Math.Min(fenetre.Width, ecran.Width)
            fenetre.Height = Math.Min(fenetre.Height, ecran.Height)
            fenetre.Show()
            fenetre.Activate()
        End Sub

        ' L'erreur s'écrit, la vitrine continue : le scénario en cours échoue, les suivants s'exécutent.
        Private Sub ErreurNonGeree(sender As Object, e As DispatcherUnhandledExceptionEventArgs)
            Ecrire(e.Exception.ToString())
            e.Handled = True
        End Sub

        Private Sub Ecrire(texte As String)
            If _journal IsNot Nothing Then
                File.AppendAllText(_journal, $"{Date.Now:HH:mm:ss.fff} {texte}{Environment.NewLine}{Environment.NewLine}")
            End If
        End Sub

    End Module

End Namespace
