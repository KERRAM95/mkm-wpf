# Chemin, bouton partagé et fenêtre flottante : Breadcrumb, SplitButton, Popover

> Fiche du lot 13, priorité 1.

**État :** livrée dans la version en préparation.

## Le besoin

- **Savoir où l'on est** dans une arborescence (projet, dossier, document)
  et y remonter d'un clic (Breadcrumb).
- **Une action principale et ses variantes** dans un seul bouton :
  « Enregistrer », et dans son menu « Enregistrer sous… », « Exporter en PDF »
  (SplitButton).
- **Un complément attaché à un élément** : un lien à partager, un réglage, un
  aperçu, sans ouvrir de dialogue (Popover).

## Ce que fait mkm

| Composant   | Dans mkm                                                                                          |
| ----------- | ------------------------------------------------------------------------------------------------- |
| Breadcrumb  | 12 px de retrait, des liens gris (le gris du texte au survol) séparés d'un chevron à 6 px ; l'icône à 8 px du libellé ; un anneau d'un pixel au clavier |
| SplitButton | deux boutons accolés, de même gravité et de même forme ; le principal sans bordure ni coins à droite, le chevron sans coins à gauche ; ↓ et ↑ sur le chevron ouvrent le menu (un TieredMenu), aussi large que le bouton au moins |
| Popover     | un cadre blanc bordé et ombré, 8 px de retrait, à 10 px sous son élément, une flèche vers lui à 15 px du bord ; dessus faute de place ; un clic ailleurs ou Échap le ferment |

## Le choix

Des composants à nous, sur les jetons de mkm (`breadcrumb`, `splitbutton`,
`popover`, importés de ui-kit) :

- **Breadcrumb** est un `ItemsControl` de **BreadcrumbItem** (un `ButtonBase` :
  `Icon`, `Click`, `Command` ; `IsFirst` et `IsLast` en lecture seule). Le
  premier lien sert d'accueil : mkm a une propriété `home` à part, ici c'est le
  premier élément.
- **SplitButton** hérite de `ContentControl` : `Content` et `Icon` du bouton
  principal, `Severity`, `Variant`, `IsDropDownOpen`, l'évènement `Click` ; ses
  éléments sont ceux de son menu (`Items`, des `MenuItem`), un `ContextMenu` au
  style de mkm (`DropDownMenu`). Ses deux boutons sont des `Button` de mkm, qui
  gagnent `CornerRadius` (les coins du thème par défaut).
- **Popover** hérite de `ContentControl` : posé n'importe où, il ne prend pas
  de place ; `Target`, `IsOpen`, `Show()`, `Hide()`, `Toggle()`, `IsFlipped`,
  les évènements `Opened` et `Closed`. Son gabarit est un `Popup` de WPF.

Les coins et la bordure des deux boutons accolés, la place de l'ombre du
Popover, ses marges et sa flèche deviennent des jetons dans les feuilles
`.css.tokens`.

## Écarts avec mkm

- Le fil d'Ariane trop long est coupé : mkm le fait défiler sans barre.
- Le menu du SplitButton est un `ContextMenu` (comme le menu « … » de la
  Toolbar), et non un TieredMenu : mêmes jetons d'overlay, même allure.
- Le Popover ouvert au clavier y amène le clavier (Tab y tourne) : mkm ne le
  fait que pour un élément `autofocus`.

## Clavier, accessibilité, performance

- Breadcrumb : un groupe, nommé par `AutomationProperties.Name` ; chaque lien,
  un lien (Hyperlink, Invoke) nommé par son libellé ou son info-bulle ; Tab,
  Entrée, Espace.
- SplitButton : un groupe nommé par son libellé ; le chevron, « Plus de
  commandes », se dit développé ou réduit (ExpandCollapse), comme
  `aria-expanded` ; ↓ ↑ ouvrent le menu, le clavier sur sa première action ;
  fermé, le clavier revient au chevron.
- Popover : son cadre, un volet nommé par le `AutomationProperties.Name` du
  Popover (`role="dialog"` dans mkm) ; Échap le ferme où que soit le clavier, et
  le rend à l'élément s'il était dans le cadre.

## La documentation et les tests

Les pages Breadcrumb, SplitButton et Popover ; `Lot13Tests` ; `MenuTests` (un
lien suivi au clic et au clavier ; le bouton principal, une action du menu, ↓
et Échap sur le chevron) ; `SuperpositionTests` (le Popover ouvert, fermé par
Échap et par son bouton).
