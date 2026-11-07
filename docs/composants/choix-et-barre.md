# Les choix et la barre de commandes : RadioButton, ToggleButton et Toolbar

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`Controls/RadioButton`, `Controls/ToggleButton`,
`Controls/Toolbar`).

## Le besoin

Choisir une option parmi quelques-unes, toutes visibles (une vue, un mode).
Basculer un état d'un clic (verrouiller, épingler, montrer les libellés).
Poser au-dessus d'une liste ou d'un panneau une barre de commandes qui garde son
titre à gauche et ses commandes à droite, même dans une fenêtre étroite.

## Ce que fait mkm

- **RadioButton** (styles de PrimeVue ; jetons de Nora en pixels pour Dashboard,
  preset `ara`) : mkm ne l'a pas redessiné pour 3DEXPERIENCE.
  - **le rond :** 15 px (12 en petit, 18 en grand), blanc, bordé du gris des champs ;
  - **le choix :** un point de 9 px (6, 12) de la couleur principale ;
  - **les états :** survolé, la bordure du survol ; au clavier, celle du focus et l'anneau (1 px, à 1 px) ;
    refusé, la bordure d'erreur ; grisé, le fond et le point des champs grisés ;
  - **le groupe :** `RadioButtonGroup`, par le nom.
- **ToggleButton** (preset `togglebutton`, redessiné pour 3DEXPERIENCE) :
  - **la mesure :** celle du bouton de 3DX, 24 px de haut, 4 px sur 12 de retrait, coins de 4 px,
    un libellé de 12 px en graisse normale, une icône grise à 6 px ;
  - **les couleurs :** au repos blanc, bordé du gris du bouton secondaire ; survolé, gris clair ;
    enfoncé, la sélection de 3DX (le fond bleu pâle, le texte foncé), bordée de la couleur principale ;
  - **le libellé :** `onLabel` et `offLabel`, `onIcon` et `offIcon`, ou le contenu ;
  - **grisé :** il garde ses couleurs, à moitié transparent.
- **Toolbar** (preset `toolbar` et sa feuille, la barre de commandes de 3DX) :
  - **la barre :** sans fond ni bordure ni retrait, de la hauteur de ses boutons ;
  - **ses zones :** `start`, `center`, les commandes, `end` ;
  - **les commandes :** `model`, à droite, collées, des boutons secondaires gris et bordés,
    coupées aux bords de leur cadre, séparées au besoin d'un trait de 24 px à 4 px de chaque côté ;
  - **le débordement :** avec `overflow`, les commandes qui ne tiennent pas passent, dans l'ordre,
    au menu d'un bouton « … » (le bouton « lite » d'icône seule, 26 px de haut, précédé d'un trait).
    Un trait ne finit pas la barre ; le menu n'a de trait ni en tête, ni en queue, ni deux de suite.
  - **le menu :** ouvert au clavier, il prend le focus sur sa première commande ;
    une commande choisie rend le focus au bouton « … ».

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `RadioButton` de WPF | les groupes (`GroupName`), le clavier, l'accessibilité | l'apparence |
| `ToggleButton` de WPF | la bascule, `Command` | l'apparence, les libellés d'état |
| `ToolBar` de WPF | le débordement dans un menu (`ToolBarOverflowPanel`) | son apparence (poignée, fond, débordement en panneau et non en menu), ses commandes à gauche |
| MahApps, ModernWpf (MIT) | des styles de ces contrôles | d'autres apparences ; une dépendance de plus |

## Le choix (technique)

- **`RadioButton` hérite du `RadioButton` de WPF**, `ToggleButton` du
  `ToggleButton` de WPF : groupes, clavier, `Command` et accessibilité sont gardés.
  `GroupName` (ou un même parent) tient lieu de `RadioButtonGroup`.
- **L'anneau du focus ne paraît qu'au clavier** (`IsFocusVisible`, comme
  `Button`) : mkm le montre sur `:focus-visible`.
- **`ToggleButton` montre `CurrentLabel` et `CurrentIcon`** : le contenu s'il en
  a un, sinon le libellé de son état ; l'icône de son état, celle du bouton
  enfoncé à défaut.
- **`Toolbar` est un `ItemsControl`** dont les éléments sont les commandes
  (`ToolbarButton`, un `Button` de mkm secondaire par défaut, et
  `ToolbarSeparator`). Chaque commande est montrée dans un `ContentPresenter`,
  que la barre cache (`Visibility.Hidden`) quand elle passe au menu : elle reste
  mesurée, prête à revenir. La répartition est celle de mkm (`updateOverflow`),
  portée dans le panneau de la rangée (`ToolbarPanel`) ; le bouton « … » est
  toujours mesuré mais ne prend de place que quand la barre déborde
  (`ToolbarOverflow`). Sans `Overflow`, les commandes passent à la ligne.
- **Le menu « … » est un `ContextMenu`** au style de mkm (`Themes/Natifs.xaml`) :
  chaque commande cachée y garde son libellé (`Content` s'il est un texte, sinon
  son nom d'accessibilité), son icône et son état grisé ; la choisir la lance
  comme un clic (son `Click` et sa `Command`), par l'automatisation de WPF.

## L'API

- **RadioButton :** `Size` (`RadioButtonSize` : `Normal`, `Small`, `Large`),
  `Invalid`, `IsFocusVisible`, et tout `RadioButton` de WPF.
- **ToggleButton :** `OnLabel`, `OffLabel`, `OnIcon`, `OffIcon`, `Size`
  (`ToggleButtonSize`), `Invalid`, `CurrentLabel`, `CurrentIcon`,
  `IsFocusVisible`, et tout `ToggleButton` de WPF.
- **Toolbar :** `Start`, `Center`, `End`, `Overflow`, `IsOverflowing`,
  `OverflowMenu`, et tout `ItemsControl` ; **ToolbarButton** (un `Button`) ;
  **ToolbarSeparator**.

En VB, `RadioButton`, `ToggleButton` et `Toolbar` portent le nom d'un contrôle de
WPF : qualifier (`Mkm.Wpf.Controls.Toolbar`) là où les deux espaces de noms sont
importés.

## Écarts avec mkm

- RadioButton : son libellé (`Content`) fait partie de lui, à 6 px, comme celui
  de la case ; pas de variante `filled`.
- ToggleButton : pas de `fluid` (`HorizontalAlignment="Stretch"` en tient lieu).
- Toolbar : les commandes sont des éléments WPF, non un modèle de `MenuItem`
  (`ItemsSource` reçoit des `ToolbarButton` fabriqués par l'application) ; le
  menu « … » s'ouvre aussi par la flèche bas.

## La vitrine et les tests

La page « Radios, bascules, barre » montre un groupe de boutons radio, leurs états
et leurs tailles ; des bascules avec libellés et icônes d'état, d'icône seule,
leurs états et leurs tailles ; une barre complète, puis une barre étroite dont les
commandes débordent (les captures ouvrent aussi son menu « … »). Les tests
vérifient le choix unique d'un groupe, la taille et le refus, les libellés et les
icônes d'état, la commande secondaire, la répartition des commandes et leur
retour, le trait qui ne finit pas la barre, la commande lancée par le menu, le
passage à la ligne sans débordement.
