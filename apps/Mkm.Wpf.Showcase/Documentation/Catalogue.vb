Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Xml.Linq

Namespace Mkm.Wpf.Showcase.Documentation

    ''' <summary>
    ''' Une page de la documentation, lue de son fichier <c>Documentation/Pages/&lt;id&gt;.xml</c> : son titre, sa catégorie
    ''' dans le menu, son résumé, ses sections (Import, puis une par usage, puis l'accessibilité), les types dont l'onglet
    ''' API donne les propriétés et les préfixes des jetons que montre l'onglet Thème.
    ''' </summary>
    Friend NotInheritable Class PageDeDoc

        ''' <summary>L'identifiant de la page, celui de son fichier (« button »).</summary>
        Public Property Id As String

        ''' <summary>Le titre (« Button »).</summary>
        Public Property Titre As String

        ''' <summary>La catégorie du menu (« Formulaire »).</summary>
        Public Property Categorie As String

        ''' <summary>Une ou deux phrases : ce qu'est le composant.</summary>
        Public Property Chapeau As XElement

        ''' <summary>Les sections de l'onglet Fonctionnalités, dans l'ordre.</summary>
        Public Property Sections As New List(Of XElement)

        ''' <summary>Les types de mkm-wpf que décrit l'onglet API (« Button », « ButtonSeverity »).</summary>
        Public Property Types As New List(Of String)

        ''' <summary>Les préfixes des jetons de l'onglet Thème (« button »).</summary>
        Public Property Jetons As New List(Of String)

        ''' <summary>Vrai pour une page de composant : elle a ses onglets API et Thème.</summary>
        Public ReadOnly Property EstUnComposant As Boolean
            Get
                Return Types.Count > 0
            End Get
        End Property

        ''' <summary>Lit une page de son XML.</summary>
        Public Shared Function Lire(id As String, document As XDocument) As PageDeDoc
            Dim racine = document.Root
            Dim page As New PageDeDoc With {
                .Id = id,
                .Titre = CStr(racine.Attribute("titre")),
                .Categorie = CStr(racine.Attribute("categorie")),
                .Chapeau = racine.Element("resume")
            }
            page.Types.AddRange(Liste(CStr(racine.Attribute("types"))))
            page.Jetons.AddRange(Liste(CStr(racine.Attribute("jetons"))))
            page.Sections.AddRange(racine.Elements("section"))
            Return page
        End Function

        Private Shared Function Liste(texte As String) As IEnumerable(Of String)
            If String.IsNullOrWhiteSpace(texte) Then Return Enumerable.Empty(Of String)()
            Return texte.Split({" "c, ","c}, StringSplitOptions.RemoveEmptyEntries)
        End Function

    End Class

    ''' <summary>
    ''' Toutes les pages de la documentation, dans l'ordre du menu : les catégories d'ui-kit (Pour commencer, Formulaire,
    ''' Bouton, Données, Panneau, Superposition, Menu, Messages, Divers), et dans chacune l'ordre alphabétique.
    ''' </summary>
    Friend NotInheritable Class Catalogue

        ''' <summary>Les catégories du menu, dans l'ordre.</summary>
        Public Shared ReadOnly Categories As String() = {
            "Pour commencer", "Formulaire", "Bouton", "Données", "Panneau", "Superposition", "Menu", "Messages", "Divers"}

        Private Shared _pages As List(Of PageDeDoc)

        Private Sub New()
        End Sub

        ''' <summary>Les pages, lues une fois des ressources de la vitrine.</summary>
        Public Shared ReadOnly Property Pages As IReadOnlyList(Of PageDeDoc)
            Get
                If _pages Is Nothing Then _pages = Charger()
                Return _pages
            End Get
        End Property

        ''' <summary>La page d'identifiant <paramref name="id"/>, ou Nothing.</summary>
        Public Shared Function Trouver(id As String) As PageDeDoc
            Return Pages.FirstOrDefault(Function(p) String.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase))
        End Function

        Private Shared Function Charger() As List(Of PageDeDoc)
            Dim assemblage = GetType(Catalogue).Assembly
            Dim pages As New List(Of PageDeDoc)()
            For Each nom In assemblage.GetManifestResourceNames().Where(Function(n) n.StartsWith("doc/", StringComparison.Ordinal))
                Using flux = assemblage.GetManifestResourceStream(nom)
                    Dim id = Path.GetFileNameWithoutExtension(nom.Substring(4))
                    pages.Add(PageDeDoc.Lire(id, XDocument.Load(flux, LoadOptions.PreserveWhitespace)))
                End Using
            Next
            ' « Pour commencer » garde l'ordre de lecture (Introduction d'abord) ; les composants, l'ordre alphabétique.
            Dim ordreDeLecture = {"introduction", "installation", "theme", "langue", "zoom", "icones"}
            Return pages.OrderBy(Function(p) Array.IndexOf(Categories, p.Categorie)).
                         ThenBy(Function(p) If(Array.IndexOf(ordreDeLecture, p.Id) < 0, Integer.MaxValue, Array.IndexOf(ordreDeLecture, p.Id))).
                         ThenBy(Function(p) p.Titre, StringComparer.OrdinalIgnoreCase).ToList()
        End Function

        ''' <summary>Le source d'un comportement d'exemple (son fichier .vb), tel qu'on le montre.</summary>
        Public Shared Function SourceDuComportement(nom As String) As String
            Dim assemblage = GetType(Catalogue).Assembly
            Using flux = assemblage.GetManifestResourceStream($"comportement/{nom}.vb")
                If flux Is Nothing Then Return Nothing
                Using lecteur As New StreamReader(flux)
                    ' Ce qui est entre #Region "Exemple" et #End Region, s'il y en a : le code utile, sans l'attache.
                    Dim source = lecteur.ReadToEnd()
                    Dim debut = source.IndexOf("#Region ""Exemple""", StringComparison.Ordinal)
                    Dim fin = source.IndexOf("#End Region", StringComparison.Ordinal)
                    If debut < 0 OrElse fin < debut Then Return source
                    debut = source.IndexOf(ControlChars.Lf, debut) + 1
                    Return source.Substring(debut, fin - debut)
                End Using
            End Using
        End Function

        ''' <summary>Le comportement d'un exemple : la méthode <c>Preparer</c> de sa classe, ou Nothing.</summary>
        Public Shared Function Comportement(nom As String) As MethodInfo
            Dim type = GetType(Catalogue).Assembly.GetType($"Mkm.Wpf.Showcase.Documentation.Exemples.{nom}")
            Return type?.GetMethod("Preparer", BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
        End Function

    End Class

End Namespace
