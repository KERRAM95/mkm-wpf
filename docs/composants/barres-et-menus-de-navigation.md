# Barres et menus de navigation : Menubar, MegaMenu, PanelMenu

> Fiche du lot 21, priorité 3.

**État :** livrée dans la version en préparation.

## Le besoin

- **La barre de menus d'une application** : Fichier, Édition, Aide
  (Menubar).
- **Une navigation riche** : un élément qui ouvre un grand panneau de liens
  rangés en colonnes (MegaMenu).
- **Une navigation latérale** : des sections qui se déplient, en arbre
  (PanelMenu).

## Ce que fait mkm

| Composant | Dans mkm                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------- |
| Menubar   | le fond et la bordure du contenu, 6 px sur 9 de retrait ; ses éléments (icône, libellé, chevron) à 6 px l'un de l'autre ; les sous-menus en cascade ; `start`, `end` ; un bouton « hamburger » sur petit écran |
| MegaMenu  | la même barre (ou en colonne) ; chaque élément ouvre un panneau ombré de colonnes de groupes (libellé en gras, liens) |
| PanelMenu | des panneaux collés, bordés (le premier et le dernier arrondis), 3 px de retrait ; chaque élément, son chevron, son icône, son libellé ; les éléments d'un élément décalés de 12 px ; un seul panneau déplié (`multiple`) |

## Le choix

- **Menubar** hérite du `Menu` de WPF : Alt, F10, les touches d'accès, les
  flèches, les commandes ; ses éléments sont des `MenuItem`, leurs
  sous-menus le menu de mkm (Themes/Natifs.xaml). `Start` et `End`.
- **MegaMenu** hérite du `Menu` de WPF : des `MegaMenuItem` (des
  `MenuItem`), leur panneau de `MegaMenuColumn`, chacune de `MenuGroup` (ceux
  du Menu) ; `Orientation` (en colonne, les panneaux à droite), `Start`,
  `End`. Un lien choisi referme le panneau.
- **PanelMenu** hérite d'`ItemsControl` : des `PanelMenuItem` (`Header`,
  `Icon`, `IsExpanded`, `Command`, l'évènement `Click`), à tout niveau ;
  `Multiple`. L'en-tête de chaque élément est un bouton, dépliable s'il a des
  éléments.

## Écarts avec mkm

- Pas de bouton « hamburger » sur petit écran (Menubar, MegaMenu).
- Menubar : les sous-menus sont le menu de 3DEXPERIENCE de mkm, sans coins
  arrondis.
- PanelMenu : les éléments se déplient sans transition.

## Clavier, accessibilité, performance

- Menubar, MegaMenu : une barre de menus ; Alt, F10, les flèches, Entrée,
  Échap (ceux de WPF).
- PanelMenu : chaque en-tête est un bouton, développé ou réduit s'il a des
  éléments ; ↑ ↓, → déplie, ← replie ou remonte, Entrée, Espace.

## La documentation et les tests

Les pages Menubar, MegaMenu et PanelMenu ; `Lot21Tests` ; `MenuTests` (un
sous-menu ; un panneau et un lien ; un panneau déplié, un élément lancé).
