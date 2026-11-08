# Carrousel, galerie et terminal : Carousel, Galleria, Terminal

> Fiche du lot 25, priorité 3 (le dernier).

**État :** livrée dans la version en préparation.

## Le besoin

- **Faire défiler des fiches** : les pièces du mois, des annonces
  (Carousel).
- **Montrer des photos** : une pièce sous plusieurs angles, des photos d'un
  site (Galleria).
- **Une console** : des commandes de maintenance, un journal interactif
  (Terminal).

## Ce que fait mkm

| Composant | Dans mkm                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------- |
| Carousel  | les éléments page par page (`numVisible`, `numScroll`), des flèches (boutons secondaires de texte, ronds), des indicateurs (barres de 2 × 0,5 rem) ; `circular`, `autoplayInterval`, `orientation` |
| Galleria  | l'élément en grand, ses vignettes (qui défilent, la montrée entière, les autres pâlies), des flèches rondes translucides, des indicateurs, une légende sur un voile ; `circular`, `autoPlay`, plein écran |
| Terminal  | le cadre des champs, 18 rem de haut ; l'accueil, les commandes et leurs réponses ; l'invite et la saisie ; `TerminalService` pour répondre |

## Le choix

- **Carousel** hérite d'`ItemsControl` : `Page`, `NumVisible`, `NumScroll`,
  `Circular`, `AutoplayInterval` (le survol l'arrête), `Orientation`,
  `ShowNavigators`, `ShowIndicators` ; `PageCount`, `Pages`, `Previous()`,
  `Next()`. Une piste interne pose les éléments et glisse jusqu'à la page.
- **Galleria** hérite de `Control` : `ItemsSource`, `ItemTemplate`,
  `ThumbnailTemplate`, `CaptionTemplate`, `ActiveIndex`, `NumVisible`,
  `ShowThumbnails`, `ShowItemNavigators`, `ShowIndicators`, `Circular`,
  `AutoPlay`, `TransitionInterval`. Ses vignettes sont un Carousel ; chacune,
  un bouton qui se dit sélectionné quand son élément est montré.
- **Terminal** hérite de `Control` : `WelcomeMessage`, `Prompt`, `Entries`
  (les commandes et leurs réponses) ; l'évènement `CommandEntered` (la
  réponse dans `e.Response`), `Respond()` pour une réponse qui vient plus
  tard, `Send()`, `Clear()`. À la place du `TerminalService` de mkm, un
  évènement.

## Écarts avec mkm

- Galleria : pas de plein écran.
- Carousel : pas de points de rupture (`responsiveOptions`).

## Clavier, accessibilité, performance

- Carousel : un groupe ; les flèches « Page précédente », « Page suivante » ;
  les indicateurs « Page 2 ».
- Galleria : un groupe ; chaque vignette, un bouton nommé par son élément,
  sélectionné quand il est montré ; ← et → dans la galerie.
- Terminal : un groupe ; la saisie nommée « Commande » ; Entrée envoie.

## La documentation et les tests

Les pages Carousel, Galleria et Terminal ; `Lot25Tests` ; `DiversTests` (les
flèches et un indicateur ; une vignette montrée ; une commande et sa réponse).
