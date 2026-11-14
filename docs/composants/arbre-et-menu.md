# L'arbre et le menu contextuel : Tree et ContextMenu

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`Controls/Tree`, `Themes/Natifs.xaml`,
`Controls/ContextMenu/MenuIcons.vb`).

## Le besoin

Ranger des éléments en arbre (les notes d'un plan, le `CustomTreeView` de
l'ancien HS CART) et proposer leurs commandes au clic droit.

## Ce que fait mkm

- **Tree** (jetons de `tree`, ceux de Nora) : 12 px de retrait ; chaque nœud de 3 px sur 6, coins de 2 px ; sa flèche dans un rond de 21 px ; une icône grise ; le nœud choisi surligné ; les enfants décalés de 12 px ; au clavier, un anneau 2 px à l'intérieur.
- **ContextMenu**, le menu de 3DEXPERIENCE (presets `contextmenu`) :
  - **le menu :** blanc, bordé de gris clair, sans arrondi, ombre courte, 90 px au moins ; il s'ouvre au point du clic, sans animation ;
  - **les éléments :** 24 px ; l'icône de 16 px dans une colonne de 44 px où commence le libellé (sans colonne si aucun élément n'a d'icône) ; le raccourci à droite en 11 px gris ;
  - **les états :** une commande grisée dans un gris à elle ; au clavier, cernée de 2 px de gris ; le surlignage et l'anneau sans arrondi (mkm-wpf : `contextmenu.item.border.radius` les arrondit) ;
  - **le reste :** le trait entre deux groupes, les sous-menus.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `TreeView` et `TreeViewItem` de WPF | le clavier (flèches, + et -, *), la sélection, les données hiérarchiques, la virtualisation, l'accessibilité | l'apparence |
| `ContextMenu`, `MenuItem` de WPF | l'ouverture au clic droit et au clavier, les sous-menus, `InputGestureText`, `Command`, l'accessibilité | l'apparence |
| MahApps.Metro, MaterialDesignInXamlToolkit (MIT) | des arbres et des menus restylés | leur apparence |

## Le choix (technique)

- **`Tree` hérite de `TreeView`, `TreeNode` de `TreeViewItem`** : tout le
  comportement de WPF est gardé ; `TreeNode` ajoute `Icon`.
- **Le menu de mkm sur tout menu contextuel de WPF** : des styles implicites de
  `ContextMenu`, `MenuItem` et du trait (`MenuItem.SeparatorStyleKey`) dans
  `Themes/Natifs.xaml`. Une application écrit ses menus comme d'habitude.
  `MenuIcons` pose, à l'ouverture de chaque menu et sous-menu, s'il garde la
  colonne d'icône.

## Écarts avec mkm

- L'arbre n'a ni cases à cocher (`selectionMode="checkbox"`) ni filtre ; les
  nœuds s'ouvrent sans transition.
- Le premier et le dernier élément d'un menu gardent 24 px (mkm leur retire le
  pixel du bord) ; une icône grisée pâlit sans passer en niveaux de gris.

## La vitrine et les tests

La page « Arbre et menu » montre un arbre de notes et son menu (icônes,
raccourcis, trait, sous-menu, commande grisée), et un menu sans icônes ; les
captures montrent les deux menus ouverts. Les tests vérifient les nœuds créés
pour des données, les styles natifs et la colonne d'icône.
