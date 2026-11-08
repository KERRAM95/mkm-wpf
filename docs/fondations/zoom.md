# Le zoom de toute l'interface : MkmResources.Zoom

> Fiche de fondation, au même titre que le thème et la langue. Demandée pour
> HS Cart : qu'un utilisateur à la vue fatiguée agrandisse toute l'interface.

**État :** fait le 08/11/2026 (`src/Mkm.Wpf/MkmResources.Zoom.vb`).

## Le besoin

Une personne malvoyante agrandit **toute** l'interface, nette, sans perdre de
fonction : texte, icônes, bordures et marges ensemble, comme le zoom d'un
navigateur. Grossir le seul texte ferait déborder les champs de 24 px de
3DEXPERIENCE.

## Ce que fait mkm

Le navigateur s'en charge (Ctrl +, Ctrl −, Ctrl + 0, Ctrl+molette) : ui-kit n'a
rien à faire. Une application de bureau n'a pas de navigateur : la bibliothèque
le fait.

## Le choix

- **`MkmResources.Current.Zoom`** : 1 pour 100 %, de 0,8 à 2 ; `ZoomIn()`,
  `ZoomOut()` (les paliers 80, 90, 100, 110, 125, 150, 175, 200 %),
  `ResetZoom()`, l'évènement `ZoomChanged`. Changeable à chaud, posable dans
  `App.xaml` (`Zoom="1.25"`).
- **Toutes les fenêtres suivent** : la bibliothèque met à l'échelle la racine de
  chaque fenêtre qui s'ouvre (`LayoutTransform`), dialogues de mkm compris (leur
  barre de titre est dans leur gabarit). Les fenêtres flottantes (listes,
  menus, info-bulles, Popover) suivent l'échelle de leur élément.
- **Net** : WPF dessine en vecteurs. Agrandi, le texte passe en mode `Ideal` :
  le mode `Display` que pose la bibliothèque, le plus net à 100 %, devient flou
  une fois mis à l'échelle. Les menus et info-bulles sans parent le prennent à
  leur ouverture.
- **Les raccourcis** dans chaque fenêtre : Ctrl +, Ctrl − (la touche 6 d'un
  clavier AZERTY : la touche est lue dans la disposition du clavier), Ctrl + 0,
  Ctrl+molette ; `ZoomShortcuts = False` les retire ; un élément qui garde
  Ctrl+molette (une visionneuse) pose `MkmResources.HandlesZoomWheel`.
- **La bulle** « 125 % », une seconde au milieu de la fenêtre, annoncée par
  Narrateur (région vivante) ; `ZoomIndicator = False` la retire.
- **Le réglage de Windows** : `MkmResources.WindowsTextScale` (Accessibilité ›
  Taille du texte), d'où l'application part au premier lancement. La
  bibliothèque ne mémorise rien : l'application garde le choix de
  l'utilisateur (`ZoomChanged`).
- **Chaque écran** : le manifeste de l'application déclare `PerMonitorV2` ; la
  vitrine le fait, HS Cart le fera.

- **Rien de caché** : agrandie, une fenêtre de taille fixe garde au moins
  `ZoomMinimumSize` (800 × 600 par défaut, jamais plus que la fenêtre) pour
  mettre son contenu en page, et défile pour montrer le reste, comme une page
  agrandie dans un navigateur (une zone qui défile autour de la présentation
  de son contenu : `Window.Content` reste le même). La molette la défile quand
  rien d'autre ne défile, Maj + molette de côté ; les flèches restent à la
  navigation au clavier. Les notifications (`Toast`) restent dans la partie
  visible.

## Écarts et limites

- Le zoom ne change pas la taille des fenêtres : à 200 %, une fenêtre montre
  moitié moins, et défile. Une application dont les écrans se replient bien
  (Toolbar, ActionBar, Triptych) baisse `ZoomMinimumSize`.
- Les images bitmap (photos) s'adoucissent agrandies ; les icônes, non.

## La documentation et les tests

La page Zoom (« Pour commencer ») et le choix du zoom dans la barre de la
vitrine ; `ZoomTests` (paliers, bornes, évènement, touches, fenêtres qui
suivent, mode du texte) ; `UiTests/ZoomTests` (Ctrl + agrandit la fenêtre et
une liste ouverte, Ctrl + 0 ramène à 100 %, la liste du zoom) ; les captures de
la fenêtre sur chaque page et de deux dialogues à 150 et 200 %
(`captures/Dashboard-150`, `Dashboard-200`).
