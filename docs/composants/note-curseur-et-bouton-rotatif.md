# Note, curseur et bouton rotatif : Rating, Slider, Knob

> Fiche du lot 18, priorité 3.

**État :** livrée dans la version en préparation.

## Le besoin

- **Noter** : la qualité d'un plan, la priorité d'une demande (Rating).
- **Régler une valeur** entre deux bornes : une opacité, un seuil (Slider) ;
  la même chose en cadran, la valeur au milieu (Knob).

## Ce que fait mkm

| Composant | Dans mkm                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------- |
| Rating    | des étoiles de 12 px à 3 px l'une de l'autre, grises, pleines et de la couleur du thème jusqu'à la note ; le survol colore l'étoile visée ; un clic sur la note la retire ; `stars`, `readonly`, `onIcon`, `offIcon` |
| Slider    | une piste de 3 px grise, colorée jusqu'à la valeur ; une poignée ronde de 16 px ; `min`, `max`, `step`, `orientation`, `range` (deux poignées) |
| Knob      | sur une grille de 100, un arc de rayon 40 et de 14 d'épaisseur, de 240° à -60° par le haut ; l'arc de la valeur en couleur ; la valeur au milieu ; `size`, `step`, `strokeWidth`, `valueTemplate`, couleurs |

## Le choix

- **Rating** hérite de `Control` : `Value` (0 à `Stars`, liée dans les deux
  sens), `Stars`, `IsReadOnly`, `OnIcon`, `OffIcon`, l'évènement
  `ValueChanged`. L'étoile pleine est l'étoile de mkm remplie : `Icon` gagne
  `Filled`. Pour les lecteurs d'écran, un curseur.
- **Slider** hérite du `Slider` de WPF, habillé : ses propriétés, son clavier
  et son accessibilité. Le `step` de mkm est `TickFrequency` avec
  `IsSnapToTickEnabled` ; un clic sur la piste y amène la poignée
  (`IsMoveToPointEnabled`).
- **Knob** hérite de `RangeBase` : `Minimum`, `Maximum`, `Value`,
  `SmallChange` (le pas), `LargeChange` ; `Size`, `StrokeWidth`, `ShowValue`,
  `ValueFormat` (un format .NET, « {0} % »), `IsReadOnly`, `ValueColor`,
  `RangeColor`, `Foreground` (le texte).

## Écarts avec mkm

- Slider : pas de `range` à deux poignées.
- Rating : un curseur pour les lecteurs d'écran (mkm : des boutons radio) ;
  ← et ↓ ne descendent pas sous une étoile, Suppr retire la note.
- Knob : `ValueFormat` est un format .NET (`{0}`), et non `{value}`.

## Clavier, accessibilité, performance

- Rating : → ↑ ← ↓, Début, Fin, Suppr ; l'anneau du clavier autour de
  l'étoile de la note.
- Slider : flèches, Page haut et bas, Début, Fin (ceux de WPF).
- Knob : flèches, Page haut et bas, Début, Fin.

## La documentation et les tests

Les pages Rating, Slider et Knob ; `Lot18Tests` ; `FormulaireTests` (un clic
et les flèches notent ; le clavier déplace la poignée ; la souris et le
clavier tournent le bouton).
