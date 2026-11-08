# Panneaux et accordéon : Panel, Card, Accordion

> Fiche du lot 12, priorité 1.

**État :** livrée dans la version en préparation.

## Le besoin

- **Un bloc titré** dans une fiche ou un tableau de bord, qu'on replie pour
  gagner de la place, avec des actions dans son en-tête (Panel).
- **Une carte** pour un document, une pièce, un résultat de recherche : un
  aperçu, un titre, quelques lignes, des boutons (Card).
- **Des sections** dont on n'ouvre qu'une à la fois : les rubriques d'une
  fiche longue, une aide (Accordion).

## Ce que fait mkm

| Composant | Dans mkm                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------- |
| Panel     | bordé d'un pixel, aux coins du thème ; l'en-tête en gras sous un trait, 13,5 px de retrait ; repliable, un bouton de texte moins / plus à droite |
| Card      | blanche et ombrée, sans bordure ; un haut bord à bord, puis à 15 px du bord le titre, le sous-titre gris, le contenu, le pied |
| Accordion | des en-têtes en gras de 13,5 px de retrait, bordés sur les côtés et en bas (la première tout autour), un chevron ; une seule ouverte, sauf `multiple` ; ↑ ↓ Début Fin passent d'un en-tête à l'autre |

## Le choix

Des composants à nous, sur les jetons de mkm (`panel`, `card`, `accordion`,
importés de ui-kit) :

- **Panel** hérite de `HeaderedContentControl` : `Toggleable`, `IsCollapsed`
  (lié dans les deux sens), `Actions`, `Footer`. Son bouton est un `Button` de
  mkm nommé « Replier » ou « Déplier ».
- **Card** hérite de `ContentControl` : `Header`, `Title`, `Subtitle`,
  `Footer` ; chaque partie vide disparaît. L'ombre est posée sur un fond à part,
  derrière la carte, pour ne pas flouter son texte.
- **Accordion** est un `ItemsControl` d'**AccordionPanel**
  (`HeaderedContentControl` : `IsExpanded`, lié dans les deux sens ; `IsFirst`
  et `IsLast` en lecture seule) ; `Multiple` laisse plusieurs sections ouvertes.
  L'en-tête est un bouton à bascule du gabarit.

Les coins de l'accordéon (ceux du haut sur la première section, ceux du bas sur
la dernière) et la bordure de la première section deviennent des jetons dans
les feuilles `.css.tokens` : mkm les écrit côté par côté.

« Panel » est aussi une classe de WPF (`System.Windows.Controls.Panel`) : le
code de la bibliothèque l'écrit en entier ; la règle est dans `CLAUDE.md`.

## Écarts avec mkm

- L'accordéon s'ouvre et se referme sans animation.
- Deux sections dites ouvertes au départ sans `Multiple` : seule la première
  le reste (mkm n'a qu'une valeur ouverte).

## Clavier, accessibilité, performance

- Panel et Card : un groupe nommé par leur titre (un texte).
- Accordion : un groupe ; chaque section, un groupe nommé par son titre ; son
  en-tête, un bouton qui se dit développé ou réduit (ExpandCollapse), comme
  `aria-expanded`. Tab passe d'un en-tête à l'autre ; ↑ ↓ (en boucle), Début et
  Fin aussi, en sautant les sections désactivées ; Entrée et Espace ouvrent et
  referment.

## La documentation et les tests

Les pages Panel, Card et Accordion ; `Lot12Tests` ; `PanneauTests` (le repli
d'un Panel et son bouton renommé, la carte nommée, une section ouverte au clic
et au clavier qui referme l'autre).
