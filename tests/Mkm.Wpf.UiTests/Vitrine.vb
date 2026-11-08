Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports FlaUI.Core
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Capturing
Imports FlaUI.Core.Input
Imports FlaUI.Core.WindowsAPI
Imports FlaUI.UIA3
Imports NUnit.Framework

Namespace Mkm.Wpf.UiTests

    ''' <summary>
    ''' La vitrine sous test : lancée une fois pour tous les scénarios (<c>Mkm.Wpf.Showcase --tests</c>), relancée si elle
    ''' s'arrête. Elle se trouve à côté des tests dans le dépôt (<c>apps/Mkm.Wpf.Showcase/bin/&lt;configuration&gt;</c>),
    ''' ou là où le dit la variable d'environnement <c>MKM_VITRINE</c>.
    ''' </summary>
    Public NotInheritable Class Vitrine

        Private Shared _application As Application
        Private Shared _automation As UIA3Automation
        Private Shared _fenetre As Window
        Private Shared _racine As String

        Private Sub New()
        End Sub

        ''' <summary>Le dossier du dépôt (celui de Mkm.Wpf.sln).</summary>
        Public Shared ReadOnly Property Racine As String
            Get
                If _racine Is Nothing Then
                    Dim dossier = New DirectoryInfo(TestContext.CurrentContext.TestDirectory)
                    While dossier IsNot Nothing AndAlso Not File.Exists(Path.Combine(dossier.FullName, "Mkm.Wpf.sln"))
                        dossier = dossier.Parent
                    End While
                    If dossier Is Nothing Then Throw New InvalidOperationException("Mkm.Wpf.sln introuvable au-dessus des tests.")
                    _racine = dossier.FullName
                End If
                Return _racine
            End Get
        End Property

        ''' <summary>Les captures et les arbres d'automatisation des scénarios en échec, le journal de la vitrine.</summary>
        Public Shared ReadOnly Property Resultats As String
            Get
                Dim dossier = Environment.GetEnvironmentVariable("MKM_RESULTATS_UI")
                If String.IsNullOrEmpty(dossier) Then dossier = Path.Combine(Racine, "TestResults", "ui")
                Directory.CreateDirectory(dossier)
                Return dossier
            End Get
        End Property

        ''' <summary>Le fichier où la vitrine écrit ses erreurs non gérées.</summary>
        Public Shared ReadOnly Property Journal As String
            Get
                Return Path.Combine(Resultats, "erreurs.txt")
            End Get
        End Property

        ''' <summary>L'exécutable de la vitrine, compilé dans la même configuration que les tests.</summary>
        Public Shared ReadOnly Property Executable As String
            Get
                Dim chemin = Environment.GetEnvironmentVariable("MKM_VITRINE")
                If String.IsNullOrEmpty(chemin) Then
                    ' tests/Mkm.Wpf.UiTests/bin/<configuration>/net48
                    Dim configuration = Path.GetFileName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory.TrimEnd(Path.DirectorySeparatorChar)))
                    chemin = Path.Combine(Racine, "apps", "Mkm.Wpf.Showcase", "bin", configuration, "net48", "Mkm.Wpf.Showcase.exe")
                End If
                Return chemin
            End Get
        End Property

        ''' <summary>L'automatisation de l'interface (UIA3).</summary>
        Public Shared ReadOnly Property Automation As UIA3Automation
            Get
                If _automation Is Nothing Then _automation = New UIA3Automation()
                Return _automation
            End Get
        End Property

        ''' <summary>Vrai quand la vitrine ne tourne plus (arrêtée, ou jamais lancée).</summary>
        Public Shared ReadOnly Property EstArretee As Boolean
            Get
                Return _application Is Nothing OrElse _application.HasExited
            End Get
        End Property

        ''' <summary>Le processus de la vitrine.</summary>
        Public Shared ReadOnly Property Processus As Integer
            Get
                Return _application.ProcessId
            End Get
        End Property

        ''' <summary>La fenêtre de la vitrine ; la vitrine se relance si elle s'est arrêtée.</summary>
        Public Shared ReadOnly Property Fenetre As Window
            Get
                If EstArretee Then Lancer()
                Return _fenetre
            End Get
        End Property

        ''' <summary>Lance la vitrine et attend sa fenêtre.</summary>
        Public Shared Sub Lancer()
            Arreter()
            Dim exe = Executable
            If Not File.Exists(exe) Then
                Throw New FileNotFoundException($"La vitrine n'est pas compilée : {exe}. Compiler la solution dans la même configuration que les tests.", exe)
            End If
            Dim depart As New ProcessStartInfo(exe, $"--tests ""{Resultats}""") With {
                .WorkingDirectory = Path.GetDirectoryName(exe),
                .UseShellExecute = False
            }
            _application = Application.Launch(depart)
            _application.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(60))
            _fenetre = _application.GetMainWindow(Automation, TimeSpan.FromSeconds(60))
            If _fenetre Is Nothing Then Throw New InvalidOperationException("La fenêtre de la vitrine ne s'est pas ouverte.")
            _fenetre.SetForeground()
            Wait.UntilInputIsProcessed()
        End Sub

        ''' <summary>Ferme la vitrine (de force si elle ne se ferme pas).</summary>
        Public Shared Sub Arreter()
            If _application Is Nothing Then Return
            Try
                If Not _application.HasExited Then _application.Close(True)
            Catch ex As Exception
                TestContext.Progress.WriteLine($"Fermeture de la vitrine : {ex.Message}")
            End Try
            _application.Dispose()
            _application = Nothing
            _fenetre = Nothing
        End Sub

        ''' <summary>Les fenêtres de la vitrine posées sur le bureau : la fenêtre, ses dialogues, ses menus et listes ouverts.</summary>
        Public Shared Function FenetresDuProcessus() As IReadOnlyList(Of AutomationElement)
            If EstArretee Then Return Array.Empty(Of AutomationElement)()
            Return Automation.GetDesktop().FindAllChildren(Function(cf) cf.ByProcessId(Processus))
        End Function

        ''' <summary>
        ''' Referme ce qu'un scénario a laissé ouvert (un menu, une liste, un dialogue) : Échap, tant qu'il reste autre chose
        ''' que la fenêtre principale.
        ''' </summary>
        Public Shared Sub RemettreEnEtat()
            If EstArretee Then Return
            For essai = 1 To 5
                Dim autres = FenetresDuProcessus().Where(Function(f) Not EstLaFenetre(f)).ToList()
                autres.AddRange(_fenetre.FindAllChildren(Function(cf) cf.ByControlType(Definitions.ControlType.Window)))
                If autres.Count = 0 Then Exit For
                Keyboard.Type(VirtualKeyShort.ESCAPE)
                Wait.UntilInputIsProcessed()
                Threading.Thread.Sleep(150)
            Next
            Try
                _fenetre.SetForeground()
            Catch ex As Exception
                TestContext.Progress.WriteLine($"Premier plan : {ex.Message}")
            End Try
        End Sub

        Private Shared Function EstLaFenetre(element As AutomationElement) As Boolean
            Try
                Return element.Properties.NativeWindowHandle.ValueOrDefault = _fenetre.Properties.NativeWindowHandle.ValueOrDefault
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' <summary>La taille du journal des erreurs, pour lire ensuite ce qu'un scénario y a ajouté.</summary>
        Public Shared Function TailleDuJournal() As Long
            Return If(File.Exists(Journal), New FileInfo(Journal).Length, 0L)
        End Function

        ''' <summary>Les erreurs non gérées écrites par la vitrine depuis <paramref name="taille"/>.</summary>
        Public Shared Function ErreursDepuis(taille As Long) As String
            If Not File.Exists(Journal) Then Return String.Empty
            Using flux As New FileStream(Journal, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                If flux.Length <= taille Then Return String.Empty
                flux.Seek(taille, SeekOrigin.Begin)
                Using lecteur As New StreamReader(flux, Encoding.UTF8)
                    Return lecteur.ReadToEnd().Trim()
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Ce qu'il faut pour comprendre un échec sans poste Windows : une capture de l'écran, et l'arbre d'automatisation
        ''' de chaque fenêtre de la vitrine, dans <see cref="Resultats"/>.
        ''' </summary>
        Public Shared Sub Photographier(nom As String)
            Dim base = Path.Combine(Resultats, Nettoyer(nom))
            Try
                Using image = Capture.Screen()
                    image.ToFile(base & ".png")
                End Using
                TestContext.AddTestAttachment(base & ".png")
            Catch ex As Exception
                TestContext.Progress.WriteLine($"Capture impossible : {ex.Message}")
            End Try
            Try
                Dim texte As New StringBuilder()
                For Each ouverte In FenetresDuProcessus()
                    Decrire(ouverte, 0, texte)
                    texte.AppendLine()
                Next
                File.WriteAllText(base & ".txt", texte.ToString(), Encoding.UTF8)
                TestContext.AddTestAttachment(base & ".txt")
            Catch ex As Exception
                TestContext.Progress.WriteLine($"Arbre d'automatisation impossible : {ex.Message}")
            End Try
        End Sub

        ''' <summary>L'arbre d'automatisation sous un élément, une ligne par élément.</summary>
        Public Shared Function Arbre(element As AutomationElement) As String
            Dim texte As New StringBuilder()
            Decrire(element, 0, texte)
            Return texte.ToString()
        End Function

        Private Shared Sub Decrire(element As AutomationElement, profondeur As Integer, texte As StringBuilder)
            If profondeur > 60 OrElse texte.Length > 2000000 Then Return
            texte.Append(New String(" "c, profondeur * 2)).AppendLine(Description(element))
            Dim enfants As AutomationElement()
            Try
                enfants = element.FindAllChildren()
            Catch ex As Exception
                Return
            End Try
            For Each enfant In enfants
                Decrire(enfant, profondeur + 1, texte)
            Next
        End Sub

        ''' <summary>Un élément en une ligne : son rôle, son nom, son identifiant, sa classe, sa place.</summary>
        Public Shared Function Description(element As AutomationElement) As String
            Try
                Dim p = element.Properties
                Dim hors = If(p.IsOffscreen.ValueOrDefault, " (hors écran)", "")
                Dim grise = If(p.IsEnabled.ValueOrDefault, "", " (grisé)")
                Return $"{p.ControlType.ValueOrDefault} « {p.Name.ValueOrDefault} » id={p.AutomationId.ValueOrDefault} classe={p.ClassName.ValueOrDefault} {p.BoundingRectangle.ValueOrDefault}{hors}{grise}"
            Catch ex As Exception
                Return $"(illisible : {ex.Message})"
            End Try
        End Function

        Private Shared Function Nettoyer(nom As String) As String
            Dim invalides = Path.GetInvalidFileNameChars()
            Return New String(nom.Select(Function(c) If(invalides.Contains(c) OrElse c = """"c, "_"c, c)).ToArray())
        End Function

    End Class

End Namespace
