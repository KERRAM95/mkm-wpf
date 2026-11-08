# Bases et défilement : ScrollPanel, Divider, ToggleSwitch, Skeleton

> Fiche du lot 10, le premier de la priorité 1 (les composants d'ui-kit qui
> manquaient et qui servent à toute application de bureau).

**État :** livrée dans la version en préparation.

## Le besoin

- **Les barres de défilement** : on voyait partout celles de Windows, grises et
  larges, à côté des composants de mkm. C'était l'écart le plus visible de la
  bibliothèque.
- **Un séparateur** entre deux blocs, avec un titre au besoin.
- **Un interrupteur** pour un réglage qui agit tout de suite.
- **Des silhouettes** à la place d'un contenu qui charge.

## Ce que fait mkm

| Composant    | Dans mkm                                                                                       |
| ------------ | ---------------------------------------------------------------------------------------------- |
| ScrollPanel  | une zone dont les barres (9 px, coins du thème, gris pâle) sont posées sur le contenu, au survol |
| Divider      | un trait d'un pixel, plein, en tirets ou en pointillés, horizontal ou vertical, un contenu dessus |
| ToggleSwitch | une piste de 30 × 18 px, sa poignée de 12 px, qui passe à droite activée                         |
| Skeleton     | un bloc gris pâle traversé d'une vague claire en 1,2 s, rectangle ou rond                        |

mkm n'habille pas les barres du navigateur : sous Windows 11, celles d'Edge sont
déjà fines. Ce sont celles de WPF qui détonnent.

## L'état de l'art

**Dans WPF :** `ScrollBar` (son `Track`, sa `Thumb`, ses `RepeatButton`) se
restyle par un style implicite ; `ScrollViewer` porte des barres dans son
gabarit (`PART_VerticalScrollBar`, `PART_HorizontalScrollBar`) ;
`ToggleButton` donne l'état, Espace et l'accessibilité d'un interrupteur ;
`Path` étiré (`Stretch="Fill"`) garde le pas de ses tirets ; une vague est un
masque d'opacité dont on anime la transformation relative.

## Le choix

Des composants à nous, sur les jetons de mkm (`divider`, `scrollpanel`,
`skeleton`, `toggleswitch`, importés de ui-kit) :

- **Les barres de toute l'application**, dans `Themes/Natifs.xaml` que
  `MkmResources` fusionne : 9 px, sans flèches ni gouttière, la poignée seule ;
  la gouttière reste cliquable (une page). La poignée fonce au survol et quand on
  la tire (`scrollpanel.bar.hover.background`, une addition de mkm-wpf : une
  barre de bureau reste visible, elle doit se repérer sous la souris).
- **ScrollPanel** hérite de `ScrollViewer` ; son gabarit pose les barres sur le
  contenu et ne les montre qu'au survol, comme mkm.
- **Divider** hérite de `ContentControl` : `Layout`, `Type`, `Align`, comme les
  propriétés de mkm (`layout`, `type`, `align`).
- **ToggleSwitch** hérite de `ToggleButton` : `Invalid`, `IsFocusVisible`.
- **Skeleton** hérite de `Control` : `Shape`, `Animation`, `CornerRadius`.

## Écarts avec mkm

- La poignée du ToggleSwitch change de côté sans glisser (0,1 s chez mkm).
- Le libellé du ToggleSwitch fait partie de lui (`Content`), comme la case à
  cocher ; mkm le pose à part.
- Le coin entre deux barres d'un `ScrollViewer` de WPF garde la couleur de
  Windows.

## Clavier, accessibilité, performance

- ToggleSwitch : Tab, Espace ; l'anneau du focus au clavier seulement ; un
  bouton bascule pour Narrateur.
- Divider : un séparateur (rôle `separator` de mkm), nommé par son contenu ; il
  ne prend pas le clavier.
- Skeleton : un dessin, invisible aux lecteurs d'écran ; sa vague ne tourne que
  quand il est visible.
- ScrollPanel : le clavier et l'accessibilité du `ScrollViewer` (Scroll).

## La documentation et les tests

Les pages ScrollPanel, Divider, ToggleSwitch et Skeleton de la documentation ;
`Lot10Tests` (la poignée, le contenu sur le trait, le rôle de séparateur, la
forme, les barres de 9 px partout) ; les scénarios d'interaction de
l'interrupteur (clic, Espace), du séparateur nommé et de la molette d'un
ScrollPanel.
