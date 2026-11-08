Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports Point = System.Drawing.Point
Imports System.Linq
Imports System.Threading
Imports FlaUI.Core.AutomationElements
Imports FlaUI.Core.Definitions
Imports FlaUI.Core.Input
Imports FlaUI.Core.WindowsAPI
Imports NUnit.Framework
Imports NUnit.Framework.Interfaces

Namespace Mkm.Wpf.UiTests

    ''' <summary>
    ''' Un scénario d'interaction : il ouvre une page de la vitrine, puis agit comme un utilisateur (la souris, le clavier)
    ''' et regarde ce que voit un lecteur d'écran. Après chaque scénario : une capture et l'arbre d'automatisation s'il a
    ''' échoué, et l'échec aussi si la vitrine a levé une erreur non gérée pendant ce temps.
    ''' </summary>
    Public MustInherit Class Scenario

        ''' <summary>Le délai d'attente d'un changement à l'écran.</summary>
        Protected Shared ReadOnly Delai As TimeSpan = TimeSpan.FromSeconds(5)

        Private _journalAvant As Long

        ''' <summary>La fenêtre de la vitrine.</summary>
        Protected Shared ReadOnly Property Fenetre As Window
            Get
                Return Vitrine.Fenetre
            End Get
        End Property

        ''' <summary>Le texte français d'une clé (voir <see cref="Textes"/>).</summary>
        Protected Shared Function T(cle As String, ParamArray parametres As Object()) As String
            Return Textes.T(cle, parametres)
        End Function

        <SetUp>
        Public Sub Preparer()
            Dim f = Fenetre
            Vitrine.RemettreEnEtat()
            _journalAvant = Vitrine.TailleDuJournal()
        End Sub

        <TearDown>
        Public Sub Conclure()
            Dim arretee = Vitrine.EstArretee
            If TestContext.CurrentContext.Result.Outcome.Status = TestStatus.Failed AndAlso Not arretee Then
                Vitrine.Photographier(TestContext.CurrentContext.Test.Name)
            End If
            Dim erreurs = Vitrine.ErreursDepuis(_journalAvant)
            Vitrine.RemettreEnEtat()
            If arretee Then Assert.Fail("La vitrine s'est arrêtée pendant le scénario." & If(erreurs = "", "", Environment.NewLine & erreurs))
            If erreurs <> "" Then Assert.Fail("La vitrine a levé une erreur non gérée :" & Environment.NewLine & erreurs)
        End Sub

        ' ---- Les pages ----

        ''' <summary>Ouvre une page neuve de la documentation par son identifiant (« button »), et la rend.</summary>
        Protected Function OuvrirLaDoc(id As String) As AutomationElement
            Dim entree = Trouver("nav-doc-" & id).AsListBoxItem()
            If entree.IsSelected Then
                Trouver("nav-doc-introduction").AsListBoxItem().Select()
                Trouver("page-introduction")
            End If
            entree.Select()
            Dim contenu = Trouver("page-" & id)
            Wait.UntilInputIsProcessed()
            Return contenu
        End Function

        ''' <summary>Le cadre d'un exemple de la page, par le titre de sa section (« exemple-sections »).</summary>
        Protected Function Exemple(page As AutomationElement, section As String) As AutomationElement
            Dim cadre = Trouver("exemple-" & section, page)
            Montrer(cadre)
            Return cadre
        End Function

        ' ---- Chercher ----

        ''' <summary>L'élément d'identifiant <paramref name="id"/> (son <c>x:Name</c>), attendu jusqu'au délai.</summary>
        Protected Function Trouver(id As String, Optional dans As AutomationElement = Nothing) As AutomationElement
            Dim racine = If(dans, Fenetre)
            Return Attendre(Function() racine.FindFirstDescendant(Function(cf) cf.ByAutomationId(id)), $"l'élément « {id} »")
        End Function

        ''' <summary>L'élément de nom <paramref name="nom"/> (ce que lit un lecteur d'écran), du rôle donné s'il l'est.</summary>
        Protected Function TrouverParNom(nom As String, Optional role As ControlType? = Nothing,
                                         Optional dans As AutomationElement = Nothing) As AutomationElement
            Dim racine = If(dans, Fenetre)
            Return Attendre(Function() ChercherParNom(racine, nom, role), $"« {nom} »{If(role.HasValue, $" ({role.Value})", "")}")
        End Function

        Private Shared Function ChercherParNom(racine As AutomationElement, nom As String, role As ControlType?) As AutomationElement
            If role.HasValue Then
                Return racine.FindFirstDescendant(Function(cf) cf.ByName(nom).And(cf.ByControlType(role.Value)))
            End If
            Return racine.FindFirstDescendant(Function(cf) cf.ByName(nom))
        End Function

        ''' <summary>
        ''' Un élément d'une autre fenêtre de la vitrine : un menu ou une liste ouverts (des fenêtres à part), un dialogue.
        ''' Nothing s'il n'y en a pas.
        ''' </summary>
        Protected Function ChercherAilleurs(nom As String, Optional role As ControlType? = Nothing) As AutomationElement
            For Each autre In AutresFenetres()
                If NomDe(autre) = nom AndAlso (Not role.HasValue OrElse role.Value = ControlType.Window) Then Return autre
                Dim trouve = ChercherParNom(autre, nom, role)
                If trouve IsNot Nothing Then Return trouve
            Next
            Return Nothing
        End Function

        ''' <summary>Les fenêtres de la vitrine autres que la principale : ses dialogues, ses menus et listes ouverts.</summary>
        Protected Function AutresFenetres() As List(Of AutomationElement)
            Dim principale = Fenetre.Properties.NativeWindowHandle.ValueOrDefault
            Dim autres = Vitrine.FenetresDuProcessus().
                Where(Function(f) f.Properties.NativeWindowHandle.ValueOrDefault <> principale).ToList()
            autres.AddRange(Fenetre.FindAllChildren(Function(cf) cf.ByControlType(ControlType.Window)))
            Return autres
        End Function

        ''' <summary>Comme <see cref="ChercherAilleurs"/>, attendu jusqu'au délai.</summary>
        Protected Function TrouverAilleurs(nom As String, Optional role As ControlType? = Nothing) As AutomationElement
            Return Attendre(Function() ChercherAilleurs(nom, role), $"« {nom} » dans un menu, une liste ou un dialogue")
        End Function

        ''' <summary>Vrai quand le menu, la liste ou le dialogue qui montrait cet élément s'est refermé.</summary>
        Protected Function Referme(nom As String) As Boolean
            Dim element = ChercherAilleurs(nom)
            Return element Is Nothing OrElse element.IsOffscreen
        End Function

        ''' <summary>Le nom d'un élément, ou une chaîne vide.</summary>
        Protected Shared Function NomDe(element As AutomationElement) As String
            Try
                Return If(element.Properties.Name.ValueOrDefault, String.Empty)
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function

        ''' <summary>Les noms d'un élément et de tous ses descendants, une ligne chacun : le texte qu'il montre.</summary>
        Protected Shared Function TexteDe(element As AutomationElement) As String
            Return String.Join(Environment.NewLine, {NomDe(element)}.Concat(element.FindAllDescendants().Select(Function(e) NomDe(e))))
        End Function

        ''' <summary>Vrai quand l'élément a le clavier.</summary>
        Protected Shared Function AFocus(element As AutomationElement) As Boolean
            Return element.Properties.HasKeyboardFocus.ValueOrDefault
        End Function

        ' ---- Attendre ----

        ''' <summary>Attend qu'un élément paraisse ; l'échec dit lequel.</summary>
        Protected Shared Function Attendre(Of TElement As Class)(chercher As Func(Of TElement), quoi As String,
                                                                 Optional delaiMax As TimeSpan? = Nothing) As TElement
            Dim chrono = Stopwatch.StartNew()
            Dim limite = If(delaiMax, Delai)
            Do
                Dim trouve As TElement = Nothing
                Try
                    trouve = chercher()
                Catch ex As Exception
                    ' L'élément peut disparaître pendant qu'on le lit : on recommence.
                End Try
                If trouve IsNot Nothing Then Return trouve
                If chrono.Elapsed > limite Then Assert.Fail($"Introuvable après {limite.TotalSeconds:0} s : {quoi}.")
                Thread.Sleep(100)
            Loop
        End Function

        ''' <summary>Attend qu'une condition soit vraie ; l'échec dit laquelle.</summary>
        Protected Shared Sub AttendreQue(condition As Func(Of Boolean), quoi As String, Optional delaiMax As TimeSpan? = Nothing)
            Dim chrono = Stopwatch.StartNew()
            Dim limite = If(delaiMax, Delai)
            Do
                Dim vrai = False
                Try
                    vrai = condition()
                Catch ex As Exception
                    ' Un état intermédiaire illisible : on recommence.
                End Try
                If vrai Then Return
                If chrono.Elapsed > limite Then Assert.Fail($"Pas vrai après {limite.TotalSeconds:0} s : {quoi}.")
                Thread.Sleep(100)
            Loop
        End Sub

        ' ---- Agir ----

        ''' <summary>Amène l'élément dans la partie visible de la page qui défile, puis clique au milieu, à la souris.</summary>
        Protected Shared Sub Cliquer(element As AutomationElement, Optional bouton As MouseButton = MouseButton.Left)
            Montrer(element)
            Mouse.Click(Centre(element), bouton)
            Wait.UntilInputIsProcessed()
        End Sub

        ''' <summary>Pose la souris au milieu de l'élément, sans cliquer.</summary>
        Protected Shared Sub Survoler(element As AutomationElement)
            Montrer(element)
            Mouse.MoveTo(Centre(element))
            Wait.UntilInputIsProcessed()
        End Sub

        ''' <summary>Tape des touches, l'une après l'autre.</summary>
        Protected Shared Sub Taper(ParamArray touches As VirtualKeyShort())
            Keyboard.Type(touches)
            Wait.UntilInputIsProcessed()
        End Sub

        ''' <summary>Tape un raccourci : les touches enfoncées ensemble (Ctrl+L).</summary>
        Protected Shared Sub Raccourci(ParamArray touches As VirtualKeyShort())
            Keyboard.TypeSimultaneously(touches)
            Wait.UntilInputIsProcessed()
        End Sub

        ''' <summary>Tape un texte.</summary>
        Protected Shared Sub Ecrire(texte As String)
            Keyboard.Type(texte)
            Wait.UntilInputIsProcessed()
        End Sub

        ''' <summary>Le milieu d'un élément, en pixels de l'écran.</summary>
        Protected Shared Function Centre(element As AutomationElement) As Point
            Dim r = element.BoundingRectangle
            Return New Point(r.Left + r.Width \ 2, r.Top + r.Height \ 2)
        End Function

        ''' <summary>
        ''' Fait défiler la page jusqu'à ce que l'élément y soit entier : la vitrine est plus haute que l'écran de la CI
        ''' (1024 × 768). Sans défilement possible, ne fait rien.
        ''' </summary>
        Protected Shared Sub Montrer(element As AutomationElement)
            ' Le défilement aide le clic ; il ne doit pas le faire échouer : l'automatisation de Windows refuse parfois de
            ' remonter les parents d'un élément d'une liste virtualisée (COMException 0x80040201).
            Try
                Defiler(element)
            Catch ex As Runtime.InteropServices.COMException
                TestContext.Progress.WriteLine($"Défilement impossible : {ex.Message}")
            End Try
        End Sub

        Private Shared Sub Defiler(element As AutomationElement)
            ' Déjà sous la souris (un élément d'une liste ouverte, au-dessus de la page) : faire défiler la page fermerait
            ' la liste, comme mkm.
            If EstCliquable(element) Then Return
            Dim defileur = AncetreQuiDefile(element)
            If defileur Is Nothing Then Return
            Dim defilement = defileur.Patterns.Scroll.Pattern
            For essai = 1 To 200
                Dim zone = defileur.BoundingRectangle
                Dim r = element.BoundingRectangle
                Dim pas As ScrollAmount
                If r.Bottom > zone.Bottom AndAlso r.Top > zone.Top Then
                    pas = If(r.Top > zone.Bottom, ScrollAmount.LargeIncrement, ScrollAmount.SmallIncrement)
                ElseIf r.Top < zone.Top AndAlso r.Bottom < zone.Bottom Then
                    pas = If(r.Bottom < zone.Top, ScrollAmount.LargeDecrement, ScrollAmount.SmallDecrement)
                Else
                    Exit For
                End If
                Dim avant = defilement.VerticalScrollPercent.ValueOrDefault
                defilement.Scroll(ScrollAmount.NoAmount, pas)
                Wait.UntilInputIsProcessed()
                If defilement.VerticalScrollPercent.ValueOrDefault = avant Then Exit For
            Next
        End Sub

        ' Vrai quand le milieu de l'élément est à l'écran et que c'est bien lui (ou un de ses enfants) qui s'y trouve.
        Private Shared Function EstCliquable(element As AutomationElement) As Boolean
            If element.IsOffscreen Then Return False
            Dim touche = Vitrine.Automation.FromPoint(Centre(element))
            Dim marcheur = Vitrine.Automation.TreeWalkerFactory.GetControlViewWalker()
            For i = 1 To 20
                If touche Is Nothing Then Return False
                If touche.Equals(element) Then Return True
                touche = marcheur.GetParent(touche)
            Next
            Return False
        End Function

        ' Le premier ancêtre qui défile verticalement (le ScrollViewer de la page).
        Private Shared Function AncetreQuiDefile(element As AutomationElement) As AutomationElement
            Dim marcheur = Vitrine.Automation.TreeWalkerFactory.GetControlViewWalker()
            Dim parent = marcheur.GetParent(element)
            While parent IsNot Nothing
                If parent.ControlType = ControlType.Window Then Return Nothing
                If parent.Patterns.Scroll.IsSupported AndAlso parent.Patterns.Scroll.Pattern.VerticallyScrollable.ValueOrDefault Then
                    Return parent
                End If
                parent = marcheur.GetParent(parent)
            End While
            Return Nothing
        End Function

    End Class

End Namespace
