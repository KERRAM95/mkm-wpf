Imports System
Imports System.Globalization
Imports System.Resources
Imports System.Windows
Imports System.Windows.Threading
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' Application de la vitrine : ses textes, sa langue de départ, puis <see cref="MainWindow"/>.
    ''' </summary>
    Partial Public Class Application

        ' En mode captures, une erreur non gérée, même avant OnStartup, s'écrit au journal.
        Shared Sub New()
            Dim arguments = Environment.GetCommandLineArgs()
            Dim i = Array.IndexOf(arguments, "--captures")
            If i < 0 OrElse i + 1 >= arguments.Length Then Return
            Dim dossier = arguments(i + 1)
            AddHandler AppDomain.CurrentDomain.UnhandledException,
                Sub(s, ev) Captures.Journal(dossier, $"ERREUR fatale : {ev.ExceptionObject}")
        End Sub

        ''' <summary>Avant la première fenêtre : les textes de la vitrine et sa langue de départ.</summary>
        Protected Overrides Sub OnStartup(e As StartupEventArgs)
            ' Textes/Vitrine.resx (anglais) et Vitrine.fr.resx ; VB nomme la ressource par son fichier.
            Dim textes As New ResourceManager("Vitrine", GetType(Application).Assembly)
            Localizer.Current.AddSource(New ResxTextSource(textes, "en", "fr"))

            ' La langue d'affichage de Windows, ou la plus proche des langues de la vitrine.
            Localizer.Current.Language = Localizer.Current.BestLanguage(CultureInfo.CurrentUICulture)

            ' --captures <dossier> : les captures de la vitrine, sans la fenêtre de la vitrine (voir Captures).
            Dim i = Array.IndexOf(e.Args, "--captures")
            If i >= 0 AndAlso i + 1 < e.Args.Length Then
                ShutdownMode = ShutdownMode.OnExplicitShutdown
                Dim dossier = e.Args(i + 1)
                Captures.Journal(dossier, "démarrage")
                ' Une erreur non gérée s'écrit au journal et arrête les captures, au lieu d'un arrêt muet.
                AddHandler DispatcherUnhandledException, Sub(s, ev)
                                                            Captures.Journal(dossier, $"ERREUR non gérée : {ev.Exception}")
                                                            ev.Handled = True
                                                            Shutdown(99)
                                                        End Sub
                Dispatcher.BeginInvoke(New Action(Sub() Shutdown(Captures.Executer(dossier))), DispatcherPriority.ApplicationIdle)
            ElseIf Array.IndexOf(e.Args, "--tests") >= 0 Then
                ' --tests [dossier] : la vitrine pilotée par les tests d'interaction (tests/Mkm.Wpf.UiTests).
                Dim j = Array.IndexOf(e.Args, "--tests")
                ModeTests.Demarrer(Me, If(j + 1 < e.Args.Length, e.Args(j + 1), Nothing))
            Else
                ' Le zoom part de la taille du texte réglée dans Windows (Accessibilité) : une application le ferait au
                ' premier lancement, puis garderait le choix de l'utilisateur. Les tests et les captures restent à 100 %.
                MkmResources.Current.Zoom = MkmResources.WindowsTextScale
                ' La fenêtre de la vitrine (pas de StartupUri : le mode captures n'en ouvre pas).
                Dim fenetre As New MainWindow()
                fenetre.Show()
            End If

            MyBase.OnStartup(e)
        End Sub

    End Class

End Namespace
