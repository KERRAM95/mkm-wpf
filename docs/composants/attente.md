# L'attente et l'aide : Tooltip, ProgressBar, ProgressSpinner et BlockUI

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`Themes/Natifs.xaml`, `Controls/Tooltip`,
`Controls/ProgressBar`, `Controls/ProgressSpinner`, `Controls/BlockUI`).

## Le besoin

Dire ce que fait une commande (l'info-bulle), montrer qu'un traitement avance
(la barre, le chargeur) et empêcher d'agir sur une partie de la fenêtre pendant
qu'elle se met à jour (le voile) : ce que faisaient le `LoadPanel` et le
`TransparentPanel` de l'ancien HS CART.

## Ce que fait mkm

- **Tooltip** (presets `tooltip`) :
  - **l'apparence :** un gris foncé, le texte blanc, 4 px sur 8 de retrait, coins de 2 px, 150 px de large au plus, le texte à la ligne ;
  - **la place :** à droite de l'élément, une flèche de 3 px vers lui.
- **ProgressBar** (presets `progressbar`) :
  - **l'apparence :** un trait de 6 px aux bouts ronds, la couleur principale sur un fond pâle ;
  - **la valeur :** elle avance en une seconde ; trop fine pour porter son pourcentage, elle le laisse au texte voisin ;
  - **indéterminée :** deux traits la parcourent.
- **ProgressSpinner** (presets `progressspinner`) : un arc de la couleur principale qui tourne et s'allonge (le SVG et les animations de PrimeVue), sans changer de couleur ; 100 px par défaut.
- **BlockUI** : un voile noir à moitié transparent, aux coins du contenu, qui paraît en fondu ; au milieu, ce qu'on veut (un chargeur, un texte).

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `ToolTip`, `ToolTipService` de WPF | l'ouverture au survol, les délais, l'accessibilité, `CustomPopupPlacementCallback` | l'apparence et la place de mkm |
| `ProgressBar` de WPF | `Minimum`, `Maximum`, `Value`, `IsIndeterminate`, l'accessibilité | l'apparence, l'avance animée |
| MahApps.Metro, MaterialDesignInXamlToolkit, HandyControl (MIT) | des `ProgressRing` (arcs animés), des barres restylées, un `Busy indicator` | leur apparence, pas l'arc de PrimeVue |
| Extended WPF Toolkit, `BusyIndicator` (Ms-PL) | un voile et un message | son apparence ; une licence de plus |

## Le choix (technique)

- **L'info-bulle de mkm sur toute info-bulle de WPF** : un style implicite dans
  `Themes/Natifs.xaml`, que `MkmResources` fusionne. Une application n'a rien à
  écrire : `ToolTip="…"` suffit. Sa place, à droite et centrée, est
  `TooltipPlacement.Right`.
- **`ProgressBar` hérite du `ProgressBar` de WPF** ; la valeur avance par une
  animation d'une seconde qui rend ensuite la main à WPF ; les deux traits de
  la barre indéterminée suivent les images clés de PrimeVue (2,1 s, leurs
  courbes de Bézier).
- **`ProgressSpinner` dessine son arc en code** (`OnRender`) : le début et la fin
  de l'arc et sa rotation s'animent comme le `stroke-dasharray`, le
  `stroke-dashoffset` et la rotation de PrimeVue. Il ne tourne que visible.
- **`BlockUI` hérite de `ContentControl`** : `Blocked`, `Overlay`. Le voile
  arrête la souris, et aussi le clavier, ce que PrimeVue ne fait pas : une
  application de bureau ne doit pas laisser saisir sous un voile.

## L'API

- **Tooltip :** `ToolTip="…"` de WPF ; `TooltipPlacement.Right`.
- **ProgressBar :** celle du `ProgressBar` de WPF.
- **ProgressSpinner :** `Stroke`, `StrokeWidth` (sur 50, 2 par défaut), `AnimationDuration` (2 s).
- **BlockUI :** `Blocked`, `Overlay` ; partie `PART_Mask`.

## Écarts avec mkm

- L'info-bulle paraît après le délai de Windows, pas aussitôt ; passée à gauche
  faute de place, elle garde sa flèche à gauche.
- Le pourcentage écrit dans la barre (`showValue`) n'existe pas : mkm ne le
  montre pas à 6 px.

## La vitrine et les tests

La page « Attente et aide » montre la barre qui avance, la barre indéterminée,
trois chargeurs, un voile de trois secondes sur un groupe de champs et des
info-bulles. Les tests vérifient la place de l'info-bulle, la barre
indéterminée, la taille et la couleur du chargeur, le voile et le clavier
arrêté.
