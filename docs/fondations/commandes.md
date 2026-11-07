# Le registre des commandes : CommandRegistry

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché le comportement : la fiche le constate.

**État :** fait le 07/11/2026 (`src/Mkm.Wpf/Commands`).

## Le besoin

Une même commande (« Enregistrer », « Verrouiller ») paraît à plusieurs endroits :
une barre, un menu, un menu contextuel, un raccourci. Elle doit y avoir partout le
même libellé, la même icône, le même état grisé ou coché, et lancer la même
action, sans que l'application le répète à chaque endroit.

## Ce que fait mkm

`createCommands` (`packages/mkm/src/commands`) :

- **la déclaration :** une fois, par clé ; libellé, icône, aide courte et longue,
  raccourci (« Ctrl+Shift+E »), grisée et visible selon le contexte, vues où elle
  paraît, case (`check`) ou choix d'un groupe (`radio`) avec l'allure de son état
  coché, action ;
- **les dispositions :** des clés, « - » pour un trait, des sous-menus (et leur
  commande par défaut), que le registre change en modèle d'une `Toolbar`, d'une
  `ActionBar` ou d'un menu ; une commande cachée ne laisse pas de trait seul ;
- **le lancement :** `execute` (seulement visible et non grisée ; une case
  s'inverse d'abord, un choix décoche les autres de son groupe, sauf ceux dont
  l'état vient de l'application), `set`, `state` ;
- **les raccourcis :** `bindShortcuts`, les champs de saisie gardant leurs touches ;
- **la barre que l'on compose :** `customizable` (les commandes choisies parmi
  des permises, gardées d'une session à l'autre).

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `RoutedCommand` et `CommandBinding` de WPF | une commande nommée, ses raccourcis, son état grisé | ni libellé ni icône, ni case ni choix, ni dispositions |
| `RelayCommand` (CommunityToolkit.Mvvm, MIT) | un `ICommand` d'une action et d'un prédicat | le reste |

## Le choix (technique)

- **Une commande déclarée (`CommandDefinition`) devient un `ICommand`**
  (`RegisteredCommand`) : WPF le grise de lui-même (son `CanExecute` suit
  `CommandManager.RequerySuggested`), et ses propriétés d'allure (`Label`,
  `Icon`, `ToolTip`, `IsChecked`, `Visibility`) se lient.
- **Le contexte** est le `CommandParameter` d'un élément, ou celui du registre
  (`Context`), lu au lancement ; changer la vue (`View`) ou le contexte, ou
  appeler `Refresh`, remet à jour tous les éléments.
- **Les dispositions** fabriquent des éléments WPF liés à leur commande :
  `CreateMenuItems` (des `MenuItem`, cases et choix compris), `CreateToolbarItems`
  (des `ToolbarButton`), `CreateActionBarItems` (des `ActionBarButton`, et des
  `ActionBarToggleButton` pour une case ou un choix).
- **Les raccourcis** sont des `KeyBinding` de WPF (`BindShortcuts`) : un champ de
  saisie qui traite une touche la garde.

## L'API

- **CommandRegistry :** `Add`, `Ids`, `Item(id)`, `View`, `Context`, `Execute`,
  `CanExecute`, `SetChecked`, `Refresh`, `CreateMenuItems`, `CreateToolbarItems`,
  `CreateActionBarItems`, `BindShortcuts`, `Separator` (« - »).
- **CommandDefinition :** `Id`, `Label`, `LabelFor`, `Icon`, `ToolTip`, `Help`,
  `Shortcut`, `Kind` (`CommandKind` : `Normal`, `Check`, `Radio`), `Group`,
  `Checked`, `IsChecked`, `CheckedLabel`, `CheckedIcon`, `CheckedToolTip`,
  `IsEnabled`, `IsVisible`, `Views`, `Execute` (`CommandExecutedEventArgs` :
  `Id`, `Context`, `Checked`).
- **RegisteredCommand :** `Id`, `Definition`, `Label`, `Icon`, `ToolTip`,
  `IsChecked`, `IsVisible`, `Visibility`, `Gesture`, et `ICommand`.
- **CommandSubmenu :** `Label`, `Icon`, `Items`, `DefaultCommand`.

## Écarts avec mkm

- Pas encore la barre que l'on compose (`customizable`).
- Une commande cachée après la fabrication d'un menu se cache, mais son trait
  voisin reste : les traits se décident à la fabrication.
- Dans une `Toolbar`, une case garde un bouton simple (son allure cochée suit).

## Les tests

Les tests vérifient le lancement et l'état grisé selon le contexte, la case qui
s'inverse et prend son allure cochée, le choix qui décoche son groupe, la vue qui
cache, le menu sans trait seul et lié à la commande, les barres et les
sous-menus, les raccourcis, et les erreurs de déclaration.
