# Les menus posés dans la page : Menu et TieredMenu

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`Controls/Menu`).

## Le besoin

Une liste de commandes posée dans la page (un panneau de navigation, un menu
d'actions à côté d'un formulaire), groupées sous des libellés ; ou des commandes
en étages, chaque étage s'ouvrant à droite du précédent.

## Ce que fait mkm

- **Menu** (preset `menu`, le menu de 3DX) : blanc, bordé de gris clair, sans
  arrondi, 150 px de large au moins ; des éléments de 24 px, l'icône de 16 px
  dans une colonne de 44 px où commence le libellé, le raccourci à droite en
  11 px ; des groupes sous un libellé gras (`submenuLabel`, 5 px sur 8) ; des
  traits. `start` et `end` reçoivent un contenu au-dessus et au-dessous. Avec
  `popup`, il s'ouvre d'un bouton.
- **TieredMenu** (preset `tieredmenu`) : le même cadre et les mêmes éléments ; un
  élément qui a des éléments ouvre son sous-menu à sa droite.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `Menu` de WPF | la barre de menus, ses sous-menus | une barre horizontale : ses premiers éléments ouvrent vers le bas |
| `ContextMenu` de WPF | le menu qui s'ouvre, au style de mkm (`Themes/Natifs.xaml`) | il ne se pose pas dans la page |

## Le choix (technique)

- **`Menu` et `TieredMenu` héritent de `MenuBase`**, la base des menus de WPF :
  clavier, mode menu, sous-menus. N'étant pas le `Menu` de WPF, leurs éléments
  sont des éléments de sous-menu : les étages s'ouvrent à droite, avec le style
  de mkm déjà posé sur tout `MenuItem`.
- **Un groupe est un `MenuGroup`** (`HeaderedItemsControl`) : son libellé, puis ses
  éléments dans le menu même ; ses traits prennent le style des menus.
- **La colonne d'icône** se décide comme pour un menu contextuel (`MenuIcons`) :
  gardée si un élément du menu ou d'un groupe a une icône.
- **Le survol ne prend pas le focus** : WPF donne le focus à l'élément de menu
  survolé, et celui qui tapait dans un champ le perdrait. Le survol reste
  montré ; un clic et le clavier donnent le focus. Posé dans la page, le `Menu`
  ne passe pas en mode menu (celui d'un menu qui s'ouvre, qui capture la souris).
- **Le menu qui s'ouvre d'un bouton** (`popup`) est un `ContextMenu`, déjà de mkm.

## L'API

- **Menu :** `Start`, `End`, et tout `MenuBase` ; **MenuGroup** (`Header`, ses
  éléments).
- **TieredMenu :** tout `MenuBase`.

En VB, `Menu` porte le nom du menu de WPF : qualifier (`Mkm.Wpf.Controls.Menu`)
là où les deux espaces de noms sont importés.

## Écarts avec mkm

- Pas de mode `popup` sur ces deux contrôles : un `ContextMenu` en tient lieu.
- Les éléments sont des `MenuItem` de WPF, non un modèle (`model`).

## La vitrine et les tests

La page « Arbre et menu » montre un menu à deux groupes et un menu à étages. Les
tests vérifient les groupes, la colonne d'icône, le trait d'un groupe, et le
sous-menu qui s'ouvre à droite.
