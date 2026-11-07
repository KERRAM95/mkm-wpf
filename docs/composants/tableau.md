# Le tableau de données : DataTable

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** fait le 07/11/2026 (`Controls/DataTable`).

## Le besoin

Lister des objets en colonnes, les trier, en choisir un ou plusieurs : des
variables d'environnement, des résultats d'une recherche dans 3DEXPERIENCE, les
attributs d'un plan.

## Ce que fait mkm

Le DataTable de mkm (preset `datatable`, redessiné pour 3DEXPERIENCE) :
- **l'en-tête des colonnes :** 27 px sur gris clair, bordé de gris dessous, les colonnes séparées d'un trait, les titres en graisse normale, 7 px de retrait ;
- **le tri :** l'icône de mkm, grise, la colonne triée surlignée ;
- **les lignes :** 32 px quel que soit leur contenu, sans trait entre elles ; le survol ne change rien, la ligne choisie prend le fond de la surbrillance ;
- **les options :** zébrées (`stripedRows`), quadrillées (`showGridlines`) ; trois tailles ;
- **les états :** un message quand il n'y a rien (`emptymessage`), un voile et un chargeur pendant un chargement (`loading`) ;
- **le cadre :** un en-tête et un pied (`header`, `footer`), bordés de gris ;
- **ce que fait aussi PrimeVue :** pagination, filtres, pied de colonne, lignes déplaçables.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `DataGrid` de WPF | les colonnes (texte, case, liste, gabarit), le tri au clic sur un titre (Maj pour plusieurs), la sélection simple ou multiple, le redimensionnement et le déplacement des colonnes, les colonnes figées, l'édition, le détail d'une ligne, la virtualisation, l'accessibilité | l'apparence de mkm |
| MahApps.Metro, MaterialDesignInXamlToolkit (MIT) | des `DataGrid` restylés | leur apparence |
| DevExpress, Telerik, Syncfusion | des grilles riches (filtres, regroupements) | payantes, et leur apparence |

## Le choix (technique)

- **`DataTable` hérite du `DataGrid` de WPF** : tout son comportement est gardé.
  Son style pose les gabarits du tableau, de la rangée des titres, d'un titre,
  d'une ligne et d'une cellule ; `CanUserAddRows` est faux (pas de ligne vide
  d'ajout, que mkm n'a pas).
- **Le gabarit du défilement est celui du `DataGrid`** (colonnes figées, barre
  horizontale décalée), sans le bouton « tout choisir » des titres de lignes, que
  mkm n'a pas.
- **Les cases d'une colonne de cases prennent la case de mkm**
  (`DataTable.CheckBoxStyleKey`), en lecture comme en modification, si
  l'application n'a pas posé son propre style.
- **La taille** pose la hauteur des lignes et les retraits des cellules et des
  titres (`CellPadding`, `HeaderPadding`).

## L'API

`Striped`, `GridLines`, `Size` (`Normal`, `Small`, `Large`), `EmptyMessage`,
`Loading`, `Header`, `Footer`, et tout le `DataGrid` de WPF (`Columns`,
`ItemsSource`, `SelectedItem`, `SelectionMode`, `CanUserSortColumns`…).

## Écarts avec mkm

- Ni pagination, ni filtres, ni pied de colonne, ni lignes déplaçables : à
  ajouter quand un besoin viendra.
- Une colonne est triable par défaut, comme dans WPF (chez mkm, par `sortable`) :
  `CanUserSort="False"` la fige.

## La vitrine et les tests

La page « Tableau de données » montre des variables d'environnement d'exemple
(en-tête avec une recherche, pied avec le nombre, une colonne d'étiquettes, une
colonne de cases), le même tableau zébré et quadrillé en petit, et un tableau
vide. Les tests vérifient les mesures, les lignes, les tailles, le voile, l'en-tête
et le pied, le message vide et les cases de mkm.
