# Puces, libellés et couleur : InputChips, FloatLabel, IftaLabel, ColorPicker

> Fiche du lot 20, priorité 3.

**État :** livrée dans la version en préparation.

## Le besoin

- **Plusieurs valeurs libres** dans un champ : des mots-clés, des matières
  (InputChips).
- **Des formulaires compacts** : le libellé dans le champ, qui s'envole à la
  saisie (FloatLabel), ou toujours en haut du champ (IftaLabel).
- **Choisir une couleur** : un repère, une catégorie (ColorPicker).

## Ce que fait mkm

| Composant  | Dans mkm                                                                                          |
| ---------- | ------------------------------------------------------------------------------------------------- |
| InputChips | le cadre des champs ; les puces (coins de 2 px) puis la saisie, à padding.y / 2 l'une de l'autre ; Entrée ajoute, Retour arrière retire ; `separator`, `max`, `allowDuplicate`, `addOnBlur` |
| FloatLabel | le libellé dans le champ, en demi-gras gris ; envolé (clavier, valeur, texte d'aide), en 0,75 rem : au-dessus (`over`), dedans (`in`), sur la bordure (`on`) ; sa couleur au clavier, rempli, refusé |
| IftaLabel  | le libellé toujours en haut dans le champ                                                        |
| ColorPicker | un carré de 1,5 rem ; un panneau sombre de 193 × 166 : le carré des nuances (150 px) et la bande des teintes ; `inline`, `format` (hex) |

## Le choix

- **InputChips** hérite de `Control` : `Value` (une `ObservableCollection(Of
  String)`, liée dans les deux sens), `Separator`, `Max`, `AllowDuplicate`,
  `AddOnBlur`, `Placeholder`, `Invalid` ; les évènements `Add` et `Remove`.
  Les puces sont les `Chip` de mkm-wpf ; une rangée qui passe à la ligne les
  pose, la saisie au bout.
- **FloatLabel** hérite de `ContentControl` : le champ est son contenu, quel
  qu'il soit ; il suit son texte, son choix, sa valeur, son texte d'aide et
  son refus (`Invalid`). `Label`, `Variant` (Over, In, On). Le libellé nomme
  le champ (son texte devient son `AutomationProperties.Name`, sauf un nom
  donné).
- **IftaLabel** est un FloatLabel toujours envolé, dans le champ.
- **ColorPicker** hérite de `Control` : `Value` (« ff0000 », comme mkm),
  `Color` (la `Color` de WPF), `Inline`, `IsDropDownOpen`, l'évènement
  `ColorChanged`. La teinte se garde sur les gris.

## Écarts avec mkm

- FloatLabel : le libellé s'envole sans transition.
- ColorPicker : seul le format hexadécimal (pas `rgb` ni `hsb`) ; le panneau
  se pilote aussi au clavier (mkm : à la souris seulement).
- Import des jetons : un renvoi écrit en camelCase dans mkm
  (`{overlay.popover.borderRadius}`) suit désormais la règle des clés.

## Clavier, accessibilité, performance

- InputChips : un groupe ; ses puces (leur croix, « Retirer ») et la saisie,
  nommée comme le champ ; Entrée, Retour arrière.
- ColorPicker : le carré, un bouton « Choisir une couleur » qui dit la
  couleur ; dans le panneau, « Nuance » (flèches) et « Teinte » (↑ ↓) ;
  Entrée et Échap referment.

## La documentation et les tests

Les pages InputChips, FloatLabel, IftaLabel et ColorPicker ; `Lot20Tests` ;
`FormulaireTests` (puces au clavier ; le libellé nomme le champ et s'envole ;
le panneau change la couleur).
