# Les conteneurs : Tabs et Fieldset

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`src/Mkm.Wpf/Controls/Tabs`,
`src/Mkm.Wpf/Controls/Fieldset`).

## Le besoin

Ranger une fenêtre chargée : des onglets pour passer d'une vue à l'autre, et
des groupes titrés, repliables, pour les sections d'un formulaire.

## Ce que fait mkm

- **Tabs**, les onglets de 3DEXPERIENCE (presets `tabs`, réécrits par mkm) :
  - **le groupe :** au milieu d'une barre de 26 px, 2 px sous son haut, dans un cadre de 25 px gris clair bordé de gris, sous une ombre courte ; la barre coupe le bas du cadre ;
  - **les onglets :** 23 px de haut, 65 px de large au moins, 10 px de retrait, un filet blanc en haut et gris en bas ; le premier et le dernier ont leur filet de côté et leurs coins de 2 px ;
  - **l'onglet choisi :** d'un bleu pâle (`dspalette.softBlue` dans Dashboard, `primary.100` dans ARA) ;
  - **le survol et l'appui :** ils ne changent rien ;
  - **au clavier :** un anneau 2 px à l'intérieur de l'onglet ;
  - **le style « strip »**, celui des facettes d'un panneau (`p-tabs-strip`) : des onglets sans cadre, de 26 px, soulignés de 3 px (bleu moyen pour le choisi, gris au survol, à l'appui et au clavier), une icône de 16 px, sur une barre au dernier pixel gris.
- **Fieldset** (preset `fieldset` de Nora, le même dans les deux thèmes) :
  - **le cadre :** d'un pixel, aux coins du contenu ;
  - **le retrait :** 9 px sous la légende, 13,5 px ailleurs ;
  - **la légende :** en gras, dans un cartouche bordé, à cheval sur le bord du haut, au droit du contenu ;
  - **repliable (`toggleable`) :** la légende devient un bouton, un moins devant elle (un plus une fois replié), avec le fond du survol.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `TabControl` et `TabItem` de WPF | la sélection, Ctrl+Tab, Ctrl+Maj+Tab, les flèches, Début et Fin, le contenu de l'onglet choisi seul dans l'arbre, l'accessibilité | l'apparence de mkm, la notion de premier et de dernier onglet |
| `GroupBox` de WPF | la légende (`Header`), qui nomme le groupe pour l'accessibilité | ni repli ni l'apparence de mkm |
| `Expander` de WPF | le repli | la légende sur le cadre |
| MahApps.Metro, MaterialDesignInXamlToolkit, HandyControl (MIT) | des `TabControl` restylés ; HandyControl, des onglets qu'on ferme et qu'on déplace | leur apparence ; rien que mkm demande |

## Le choix (technique)

- **`Tabs` hérite de `TabControl`, `Tab` de `TabItem`** : tout le comportement
  de WPF est gardé. `Tabs` marque le premier et le dernier onglet visibles
  (`IsFirst`, `IsLast`), y compris quand on en cache un.
- **`Variant`** (`Group` par défaut, `Strip`) est héritée par les onglets ; le
  style d'un `Tab` change de gabarit selon elle.
- **Les filets de l'onglet** (blanc en haut et aux bouts, gris en bas) sont trois
  bordures empilées : une bordure WPF n'a qu'une couleur.
- **L'ombre du cadre** est portée par une bordure à elle, derrière : sur le
  cadre, elle ombrerait aussi les libellés.
- **`Fieldset` hérite de `GroupBox`** et ajoute `Toggleable` et `IsCollapsed`
  (liable dans les deux sens). La légende est placée au droit du contenu,
  après la bordure et le retrait de gauche, comme celle d'un `fieldset` HTML.
- **Ce que la feuille CSS de mkm dit sans jeton** (barre, cadre, ombre,
  hauteurs, filets, coins des bouts, style « strip ») est écrit dans
  `Theming/Tokens/<thème>.css.tokens`. L'ombre de mkm,
  `color-mix(in srgb, surface.700 50%, transparent)`, a demandé de mélanger
  les couleurs comme CSS : pondérées de leur opacité, une couleur mêlée au
  transparent ne fonce pas.

## L'API

- **Tabs :** `Variant` (`TabsVariant`), et tout `TabControl`.
- **Tab :** `Header`, `Icon`, `Variant` (héritée), `IsFirst`, `IsLast` (lecture
  seule), et tout `TabItem`.
- **Fieldset :** `Header`, `Toggleable`, `IsCollapsed` ; partie `PART_Legend`.

## Clavier, accessibilité, performance

Ceux de `TabControl` et de `GroupBox`. La légende repliable est un
`ToggleButton` nommé par la légende ; Espace la bascule.

## Écarts avec mkm

- Les onglets ne défilent pas quand ils débordent (les flèches de PrimeVue) :
  à ajouter quand un besoin viendra.
- Le groupe se replie sans transition.
- Sa bascule annonce aux lecteurs d'écran « cochée » quand il est replié (mkm
  annonce « déplié » : `aria-expanded`).

## La vitrine et les tests

La page « Onglets et groupes » montre les deux styles d'onglets et trois
groupes (simple, repliable, replié). Les tests vérifient les bouts du groupe
(y compris quand on cache le dernier), le style passé aux onglets, la place de
la légende et le repli.
