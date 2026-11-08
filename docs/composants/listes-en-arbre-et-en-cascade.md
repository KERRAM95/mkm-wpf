# Listes en arbre et en cascade : TreeSelect, CascadeSelect

> Fiche du lot 17, priorité 2.

**État :** livrée dans la version en préparation.

## Le besoin

- **Choisir un dossier, une catégorie** dans une hiérarchie, depuis un champ
  de formulaire (TreeSelect).
- **Choisir pas à pas** : un site, puis un atelier, puis une machine, chaque
  niveau ouvrant le suivant (CascadeSelect).

## Ce que fait mkm

| Composant     | Dans mkm                                                                                          |
| ------------- | ------------------------------------------------------------------------------------------------- |
| TreeSelect    | le champ des listes déroulantes ; ouvert, un arbre (Tree) dans un cadre ombré, aussi large que le champ au moins, 20 rem au plus ; `selectionMode` single, multiple ou checkbox ; `showClear` ; `display` comma ou chip ; filtre |
| CascadeSelect | le même champ ; ouvert, une liste d'options ; une option de groupe (flèche à droite) ouvre la liste suivante à sa droite ; l'option choisie surlignée ; `optionLabel`, `optionGroupChildren` |

## Le choix

- **TreeSelect** hérite de `Control` et porte un `Tree` de mkm-wpf, son enfant
  logique (il hérite du contexte de données du champ) : `Items` (des
  `TreeNode`), `ItemsSource` et `ItemTemplate` (un `HierarchicalDataTemplate`),
  `SelectedItem` (lié dans les deux sens), l'évènement `SelectedItemChanged`,
  `Placeholder`, `DisplayMemberPath`, `ShowClear`, `IsDropDownOpen`, `Invalid`.
  Ouvert, le clavier entre dans l'arbre, sur le nœud choisi ; les flèches de
  l'arbre le parcourent sans changer le choix ; Entrée, Espace ou un clic le
  font.
- **CascadeSelect** hérite de `Control` et porte un menu de WPF (comme
  SplitButton) : ses options, des `CascadeSelectItem` (des `MenuItem`), ouvrent
  leur suite au survol ou à →, se referment à ← ou Échap ; une option sans
  suite se choisit. `Items`, `ItemsSource`, `ItemTemplate`, `SelectedItem`,
  `SelectedItemChanged`, `Placeholder`, `DisplayMemberPath`,
  `IsDropDownOpen`, `Invalid` ; `CascadeSelectItem.IsChosen` marque l'option
  choisie.
- Les deux champs sont des « zones de liste déroulante » pour les lecteurs
  d'écran : leur valeur est le texte du choix, ils se disent développés ou
  réduits.

## Écarts avec mkm

- TreeSelect : un seul nœud choisi (ni `selectionMode` multiple ou checkbox,
  ni puces), pas de filtre.
- CascadeSelect : pas de croix `showClear` ; pas de mode mobile (listes
  empilées) ; seule l'option choisie est marquée, pas son chemin.

## Clavier, accessibilité, performance

- TreeSelect : ↓, Entrée, Espace, F4 ouvrent ; dans l'arbre, les flèches, + et
  - ; Entrée ou Espace choisissent ; Échap referme sans choisir ; Tab referme et
  passe à la suite. La croix se nomme « Effacer ».
- CascadeSelect : ↓, Entrée, Espace, F4 ouvrent, sur la première option ;
  ↑ ↓ → ← dans les listes ; Entrée choisit ; Échap referme.
- La liste et l'arbre se referment quand la page défile.

## La documentation et les tests

Les pages TreeSelect et CascadeSelect ; `Lot17Tests` ; `FormulaireTests`
(un clic puis le clavier choisissent un nœud ; la souris puis le clavier
choisissent une machine).
