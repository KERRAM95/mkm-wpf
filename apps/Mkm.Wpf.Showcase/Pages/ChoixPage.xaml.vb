Imports Mkm.Wpf.Commands
Imports Mkm.Wpf.Controls
Imports Mkm.Wpf.Localization

Namespace Mkm.Wpf.Showcase

    ''' <summary>
    ''' La page des choix et de la barre de commandes ; en bas, un registre des commandes : un menu et une barre fabriqués
    ''' des mêmes commandes, dont l'état coché se suit de l'un à l'autre.
    ''' </summary>
    Partial Public Class ChoixPage

        Public Sub New()
            InitializeComponent()
            Dim t = Function(cle As String) Localizer.Current.T(cle)
            Dim noter = Sub(e As CommandExecutedEventArgs) JournalDuRegistre.Text = e.Id & If(e.Checked.HasValue, " : " & e.Checked.Value.ToString(), "")
            Dim registre As New CommandRegistry(
                New CommandDefinition With {.Id = "ajouter", .Label = t("showcase.actionbar.add"), .Icon = Icons.Add, .Shortcut = "Ctrl+N", .Execute = noter},
                New CommandDefinition With {.Id = "copier", .Label = t("showcase.actionbar.copy"), .Icon = Icons.Copy, .Shortcut = "Ctrl+Shift+C", .Execute = noter},
                New CommandDefinition With {.Id = "supprimer", .Label = t("showcase.actionbar.delete"), .Icon = Icons.Delete,
                                            .IsEnabled = Function(contexte) False, .Execute = noter},
                New CommandDefinition With {.Id = "verrouiller", .Kind = CommandKind.Check, .Label = t("showcase.actionbar.lock"),
                                            .CheckedLabel = t("showcase.choices.locked"), .Icon = Icons.Unlocked, .CheckedIcon = Icons.Locked,
                                            .Shortcut = "Ctrl+L", .Execute = noter})
            For Each element In registre.CreateMenuItems({"ajouter", "copier", "supprimer", CommandRegistry.Separator, "verrouiller"})
                MenuDuRegistre.Items.Add(element)
            Next
            For Each element In registre.CreateToolbarItems({"ajouter", "copier", CommandRegistry.Separator, "verrouiller"})
                BarreDuRegistre.Items.Add(element)
            Next
            registre.BindShortcuts(Me)
        End Sub

    End Class

End Namespace
