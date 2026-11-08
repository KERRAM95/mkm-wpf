Imports System.Windows
Imports System.Windows.Controls
Imports Mkm.Wpf.Commands
Imports Mkm.Wpf.Controls
Imports BarreDeMkm = Mkm.Wpf.Controls.Toolbar
Imports MenuDeMkm = Mkm.Wpf.Controls.Menu

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>
    ''' Le registre de l'exemple : quatre commandes déclarées une fois, d'où sont fabriqués un menu et une barre, et leurs
    ''' raccourcis ; chaque commande lancée s'écrit dans le journal.
    ''' </summary>
    Friend NotInheritable Class CommandesRegistre

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            Dim journal = DirectCast(exemple.FindName("Journal"), TextBlock)
            Remplir(DirectCast(exemple.FindName("MenuDuRegistre"), MenuDeMkm),
                    DirectCast(exemple.FindName("BarreDuRegistre"), BarreDeMkm),
                    exemple, Sub(texte) journal.Text = texte)
        End Sub

#Region "Exemple"
        Private Shared Sub Remplir(leMenu As MenuDeMkm, laBarre As BarreDeMkm, page As UIElement, noter As System.Action(Of String))
            ' Chaque commande, déclarée une fois : son libellé, son icône, son raccourci, son action.
            Dim lancee = Sub(e As CommandExecutedEventArgs) noter(e.Id & If(e.Checked.HasValue, " : " & e.Checked.Value.ToString(), ""))
            Dim registre As New CommandRegistry(
                New CommandDefinition With {.Id = "ajouter", .Label = "Ajouter", .Icon = Icons.Add, .Shortcut = "Ctrl+N", .Execute = lancee},
                New CommandDefinition With {.Id = "copier", .Label = "Copier", .Icon = Icons.Copy, .Shortcut = "Ctrl+Shift+C", .Execute = lancee},
                New CommandDefinition With {.Id = "supprimer", .Label = "Supprimer", .Icon = Icons.Delete,
                                            .IsEnabled = Function(contexte) False, .Execute = lancee},
                New CommandDefinition With {.Id = "verrouiller", .Kind = CommandKind.Check, .Label = "Verrouiller",
                                            .CheckedLabel = "Verrouillé", .Icon = Icons.Unlocked, .CheckedIcon = Icons.Locked,
                                            .Shortcut = "Ctrl+L", .Execute = lancee})

            ' Un menu et une barre fabriqués des mêmes commandes ; « - » (CommandRegistry.Separator) pose un trait.
            For Each element In registre.CreateMenuItems({"ajouter", "copier", "supprimer", CommandRegistry.Separator, "verrouiller"})
                leMenu.Items.Add(element)
            Next
            For Each element In registre.CreateToolbarItems({"ajouter", "copier", CommandRegistry.Separator, "verrouiller"})
                laBarre.Items.Add(element)
            Next

            ' Les raccourcis, actifs quand le clavier est dans la page.
            registre.BindShortcuts(page)
        End Sub
#End Region

    End Class

End Namespace
