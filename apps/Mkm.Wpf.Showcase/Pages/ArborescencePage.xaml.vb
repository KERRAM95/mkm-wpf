Imports System.Collections.ObjectModel
Imports System.Windows.Media
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>La page de l'arborescence et du cycle de vie : la structure d'un produit, le cycle de vie d'un plan.</summary>
    Partial Public Class ArborescencePage

        ''' <summary>Un nœud de la structure.</summary>
        Public Class Element
            Public Property Nom As String
            Public Property Revision As String
            Public Property Maturite As String
            Public Property Proprietaire As String
            Public Property Icone As Object
            Public Property Enfants As New ObservableCollection(Of Element)

            Public Sub New(nom As String, icone As Object, revision As String, maturite As String, ParamArray enfants As Element())
                Me.Nom = nom
                Me.Icone = icone
                Me.Revision = revision
                Me.Maturite = maturite
                Proprietaire = "M. Kerram"
                For Each enfant In enfants
                    Me.Enfants.Add(enfant)
                Next
            End Sub
        End Class

        Public Sub New()
            InitializeComponent()
            Dim t = Function(cle As String) Localizer.Current.T(cle)
            Dim enCours = t("showcase.identity.inWork")
            Dim valide = t("showcase.identity.released")
            Dim sousEnsemble = New Element("HS-SE-010", Icons.Documents, "B", enCours,
                New Element("HS-P-011 Carter", Icons.Document, "A", valide),
                New Element("HS-P-012 Couvercle", Icons.Document, "C", enCours),
                New Element("HS-P-013 Joint", Icons.Document, "A", valide))
            Dim racine = New Element("HS-ASM-001", Icons.Documents, "A", enCours,
                sousEnsemble,
                New Element("HS-SE-020", Icons.Documents, "A", valide,
                    New Element("HS-P-021 Axe", Icons.Document, "A", valide)),
                New Element("HS-PL-001 Plan d'ensemble", Icons.Drawings, "B", enCours))
            Dim racines As New ObservableCollection(Of Element) From {racine, New Element("HS-ASM-002", Icons.Documents, "A", valide)}
            Produit.Nodes = racines
            Produit.Expand(racine)
            Produit.Expand(sousEnsemble)
            Produit.SelectedItem = sousEnsemble.Enfants(1)

            ' Le cycle de vie de 3DEXPERIENCE ; les couleurs de maturité sont celles de l'application (sa politique).
            Dim couleur = Function(texte As String) New SolidColorBrush(DirectCast(ColorConverter.ConvertFromString(texte), Color))
            Dim etats = DirectCast(Cycle.States, ObservableCollection(Of StateGraphState))
            etats.Add(New StateGraphState With {.Id = "private", .Label = t("showcase.structure.private"), .Color = couleur("#77797C"), .PaleColor = couleur("#BABABA")})
            etats.Add(New StateGraphState With {.Id = "inwork", .Label = enCours, .Color = couleur("#007DA3"), .PaleColor = couleur("#7FBED1")})
            etats.Add(New StateGraphState With {.Id = "frozen", .Label = t("showcase.structure.frozen"), .Color = couleur("#42A2DA"), .PaleColor = couleur("#A0D0EC")})
            etats.Add(New StateGraphState With {.Id = "released", .Label = valide, .Color = couleur("#018308"), .PaleColor = couleur("#80C183")})
            etats.Add(New StateGraphState With {.Id = "obsolete", .Label = t("showcase.structure.obsolete"), .Color = couleur("#FFFFFF")})
            Dim transitions = DirectCast(Cycle.Transitions, ObservableCollection(Of StateGraphTransition))
            For Each paire In {({"private", "inwork"}), ({"inwork", "frozen"}), ({"frozen", "released"}), ({"released", "obsolete"}), ({"frozen", "inwork"})}
                transitions.Add(New StateGraphTransition With {.From = paire(0), .To = paire(1)})
            Next
            Dim actions = DirectCast(Cycle.Actions, ObservableCollection(Of StateGraphAction))
            actions.Add(New StateGraphAction With {.To = "frozen", .Label = t("showcase.structure.freeze")})
            actions.Add(New StateGraphAction With {.To = "released", .Label = t("showcase.structure.release")})
            actions.Add(New StateGraphAction With {.To = "private", .Label = t("showcase.structure.demote")})
            AddHandler Cycle.ActionClick, Sub(s, e) Journal.Text = e.Action.Label
        End Sub

    End Class

End Namespace
