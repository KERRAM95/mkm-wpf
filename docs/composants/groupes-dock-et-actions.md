# Groupes, dock et actions : ButtonGroup, Dock, SpeedDial

> Fiche du lot 22, priorité 3.

**État :** livrée dans la version en préparation.

## Le besoin

- **Des boutons liés** : Enregistrer, Supprimer, Annuler, d'un seul bloc
  (ButtonGroup).
- **Un lanceur d'applications** sur un bord d'un tableau de bord (Dock).
- **Quelques actions derrière un bouton** rond, flottant sur une page
  (SpeedDial).

## Ce que fait mkm

| Composant   | Dans mkm                                                                                          |
| ----------- | ------------------------------------------------------------------------------------------------- |
| ButtonGroup | des boutons collés : le premier garde ses coins à gauche, le dernier à droite, ceux du milieu aucun ; sans bordure à droite, sauf le dernier |
| Dock        | sur un bord de son conteneur, un cadre translucide (voile blanc à 10 %, bordure à 20 %), aux grands coins, 6 px de retrait ; des applications de 36 px, leur nom en info-bulle ; `position` |
| SpeedDial   | un bouton rond, son plus qui tourne en croix ; ses actions, des boutons ronds secondaires d'icône, à 6 px, en ligne (`direction`) ou en cercle, demi-cercle, quart de cercle (`type`, `radius`) ; une action choisie, un clic ailleurs ou Échap les replie |

## Le choix

- **ButtonGroup** hérite d'`ItemsControl` : il pose sur chaque bouton sa place
  (`ButtonGroup.Position`, attachée), et le style de Button en tire ses coins
  et sa bordure ; seul, un bouton reprend sa forme.
- **Dock** hérite d'`ItemsControl` : `Position` ; ses `DockItem` (des
  `ButtonBase`) : `Icon` (une icône de mkm ou une image), `Label`, `Click`,
  `Command`. Le nom est aussi l'énumération `Dock` de WPF : l'écrire en entier
  dans le code VB (comme Image et Slider).
- **SpeedDial** hérite d'`ItemsControl` : `IsOpen`, `Direction`, `Type`,
  `Radius`, `ShowIcon`, `HideIcon` ; ses `SpeedDialItem` sont des Button de
  mkm (`Label` en info-bulle). Les actions se posent autour du centre du
  bouton par le calcul de mkm, sans pousser la page.

## Écarts avec mkm

- Dock : pas de version mobile (défilement).
- SpeedDial : pas de voile (`mask`) ; les actions paraissent sans animation.

## Clavier, accessibilité, performance

- ButtonGroup : un groupe, ses boutons ; Tab.
- Dock : un menu, ses applications des éléments de menu nommés ; Tab y entre
  une fois, les flèches passent de l'une à l'autre.
- SpeedDial : le bouton se dit développé ou réduit ; ouvert au clavier, le
  clavier va à la première action ; les flèches, Échap.

## La documentation et les tests

Les pages ButtonGroup, Dock et SpeedDial ; `Lot22Tests` ; `MenuTests` (une
application lancée ; les actions déployées, une action choisie).
