Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>
    ''' Les graphes de l'exemple reçoivent le cycle de vie d'un plan dans 3DEXPERIENCE ; un graphe marqué « actions »
    ''' reçoit aussi ses actions, et l'action choisie s'écrit dans le texte « Journal ».
    ''' </summary>
    Friend NotInheritable Class CycleDeVie

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim journal = TryCast(exemple.FindName("Journal"), TextBlock)
            For Each graphe In Tous(Of StateGraph)(exemple)
                Remplir(graphe, Equals(graphe.Tag, "actions"), journal)
            Next
        End Sub

#Region "Exemple"
        ' Les états, dans l'ordre du cycle, et leurs couleurs de maturité : celles de l'application, pas du thème.
        Friend Shared Sub Remplir(graphe As StateGraph, avecActions As Boolean, journal As TextBlock)
            Dim etats = DirectCast(graphe.States, ObservableCollection(Of StateGraphState))
            etats.Add(New StateGraphState With {.Id = "private", .Label = "Privé", .Color = Couleur("#77797C"), .PaleColor = Couleur("#BABABA")})
            etats.Add(New StateGraphState With {.Id = "inwork", .Label = "En cours", .Color = Couleur("#007DA3"), .PaleColor = Couleur("#7FBED1"),
                                                .ToolTip = "En cours : le plan se modifie"})
            etats.Add(New StateGraphState With {.Id = "frozen", .Label = "Gelé", .Color = Couleur("#42A2DA"), .PaleColor = Couleur("#A0D0EC")})
            etats.Add(New StateGraphState With {.Id = "released", .Label = "Validé", .Color = Couleur("#018308"), .PaleColor = Couleur("#80C183")})
            etats.Add(New StateGraphState With {.Id = "obsolete", .Label = "Obsolète", .Color = Couleur("#FFFFFF")})

            ' Les transitions possibles : une flèche de chaque état vers le suivant, et le retour de Gelé à En cours.
            Dim fleches = DirectCast(graphe.Transitions, ObservableCollection(Of StateGraphTransition))
            For Each paire In {({"private", "inwork"}), ({"inwork", "frozen"}), ({"frozen", "released"}), ({"released", "obsolete"}), ({"frozen", "inwork"})}
                fleches.Add(New StateGraphTransition With {.From = paire(0), .To = paire(1)})
            Next
            If Not avecActions Then Return

            ' Les actions offertes depuis l'état courant ; ActionClick dit laquelle l'utilisateur choisit.
            Dim actions = DirectCast(graphe.Actions, ObservableCollection(Of StateGraphAction))
            actions.Add(New StateGraphAction With {.To = "frozen", .Label = "Geler"})
            actions.Add(New StateGraphAction With {.To = "released", .Label = "Valider"})
            actions.Add(New StateGraphAction With {.To = "private", .Label = "Rétrograder"})
            AddHandler graphe.ActionClick, Sub(s, e)
                                               If journal IsNot Nothing Then journal.Text = $"Action choisie : {e.Action.Label}"
                                           End Sub
        End Sub

        Private Shared Function Couleur(texte As String) As Brush
            Return New SolidColorBrush(DirectCast(ColorConverter.ConvertFromString(texte), Color))
        End Function
#End Region

    End Class

End Namespace
