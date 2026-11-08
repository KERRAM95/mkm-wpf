# Étapes, frise et pagination : Stepper, Timeline, Paginator

> Fiche du lot 15, priorité 2.

**État :** livrée dans la version en préparation.

## Le besoin

- **Un assistant en plusieurs temps** : diffuser un plan (choisir, vérifier,
  envoyer), créer une pièce (Stepper).
- **L'histoire d'un document** : ses révisions, ses validations, datées
  (Timeline).
- **Une longue liste par pages** : des milliers de documents (Paginator).

## Ce que fait mkm

| Composant | Dans mkm                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------- |
| Stepper   | des étapes numérotées (rond de 24 px bordé de 2 px, titre gras), l'étape en cours en couleur, un trait de 2 px entre elles, coloré avant l'étape en cours ; en colonne (StepItem), le contenu sous son étape ; `linear` |
| Timeline  | des évènements le long d'une ligne de 2 px, un rond de 13,5 px, le contenu et l'opposé de part et d'autre ; `layout` vertical ou horizontal, `align` left, right, alternate, top, bottom ; `marker` |
| Paginator | première, précédente, liens de page (5, centrés), suivante, dernière ; 30 px, la page montrée sur le fond du thème ; lignes par page ; « 3 sur 12 » |

## Le choix

- **Stepper** hérite du `TabControl` de WPF : la sélection, le clavier et
  l'accessibilité d'onglets ; `StepperPanel` (un `TabItem`) a son numéro,
  `IsLast` et `IsCompleted`. `Orientation` (en colonne, chaque contenu sous son
  étape), `Linear`, `Next()`, `Previous()`.
- **Timeline** est un `ItemsControl` de `TimelineEvent` (`Opposite`,
  `OppositeTemplate`, `Marker`) ; `Layout` (une `Orientation`) et `Align`.
- **Paginator** hérite de `Control` : `First`, `Rows`, `TotalRecords`,
  `PageLinkSize`, `RowsPerPageOptions` (« 10, 20, 50 » en XAML),
  `ShowCurrentPageReport`, `ChangePage()`, l'évènement `PageChange`. Il ne
  montre pas les données : l'application montre ses lignes.
- Une rangée commune (`RangeeEtiree`) répartit les étapes et les évènements
  en ligne : le dernier garde sa largeur, les autres se partagent le reste
  (flex chez mkm).

## Écarts avec mkm

- Le rapport de page dit « 3 sur 12 » (sans gabarit à jetons comme
  `{currentPage}`) ; pas de saut à une page saisie (`JumpToPageInput`).

## Clavier, accessibilité, performance

- Stepper : des onglets nommés par leur titre, l'étape en cours sélectionnée ;
  linéaire, ni clic ni clavier n'y sautent.
- Timeline : une liste d'évènements.
- Paginator : une barre d'outils ; « Première page », « Page précédente »,
  « Page 3 »… ; les flèches grisées aux bouts.

## La documentation et les tests

Les pages Stepper, Timeline et Paginator ; `Lot15Tests` ; `PanneauTests`
(Suivant, un clic, Précédent), `DonneesTests` (flèches et liens de page, la
frise).
