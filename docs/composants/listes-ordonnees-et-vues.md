# Listes ordonnées et vues : OrderList, PickList, DataView

> Fiche du lot 23, priorité 3.

**État :** livrée dans la version en préparation.

## Le besoin

- **Ordonner** les étapes d'une diffusion, les colonnes d'un export
  (OrderList).
- **Choisir dans une liste et ordonner le choix** : les destinataires parmi
  les services (PickList).
- **Présenter des données** autrement qu'en tableau : des fiches en liste ou
  en grille, page par page (DataView).

## Ce que fait mkm

| Composant | Dans mkm                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------- |
| OrderList | une Listbox et, à sa gauche, quatre boutons (monter, tout en haut, descendre, tout en bas) à 0,5 rem l'un de l'autre, à 1,125 rem de la liste |
| PickList  | deux OrderList (ses boutons à gauche de la source, à droite de la cible) et, entre elles, quatre boutons de passage ; un double clic fait passer un élément |
| DataView  | un en-tête bordé en bas, les éléments par le gabarit `list` ou `grid`, un paginateur, un pied bordé en haut |

## Le choix

- **OrderList** hérite de `Control` et porte un `Listbox` de mkm-wpf :
  `ItemsSource` (une liste modifiable, ordonnée en place : `Move` d'une
  `ObservableCollection`, sinon retirer et insérer), `ItemTemplate`,
  `DisplayMemberPath`, `SelectionMode` (plusieurs par défaut), `Header`,
  `ShowControls`, `ControlsPosition` ; `MoveUp`, `MoveTop`, `MoveDown`,
  `MoveBottom` ; l'évènement `Reorder`.
- **PickList** hérite de `Control` et porte deux OrderList : `Source`,
  `Target`, leurs en-têtes et leurs boutons ; `MoveToTarget`,
  `MoveAllToTarget`, `MoveToSource`, `MoveAllToSource` ; les évènements
  `MovedToTarget`, `MovedToSource` (les éléments passés). Les deux listes ont
  la même largeur (une rangée symétrique, interne), étirées ou non.
- **DataView** hérite de `Control` : `ItemsSource`, `ItemTemplate`,
  `GridItemTemplate`, `Layout` (List, Grid), `GridColumns`, `Paginator`,
  `Rows`, `First`, `Header`, `Footer` ; `PageItems`, `TotalRecords`,
  `IsEmpty`. Le paginateur est celui de mkm-wpf.

## Écarts avec mkm

- DataView : pas de tri (`sortField`, `sortOrder`) ; le paginateur est
  toujours dessous.
- OrderList et PickList : pas de glisser-déposer entre les éléments.

## Clavier, accessibilité, performance

- OrderList : un groupe nommé par son en-tête ; ses boutons nommés ; la liste
  du Listbox (flèches, Maj, Ctrl).
- PickList : un groupe ; ses boutons nommés ; ses deux listes.
- DataView : une liste nommée par son en-tête.

## La documentation et les tests

Les pages OrderList, PickList et DataView ; `Lot23Tests` ; `DonneesTests`
(tout en haut ; un double clic et tout vers la source ; la page suivante).
