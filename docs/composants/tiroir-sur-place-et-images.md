# Tiroir, sur place et images : Drawer, Inplace, Image, ImageCompare

> Fiche du lot 16, priorité 2.

**État :** livrée dans la version en préparation.

## Le besoin

- **Des filtres ou des détails sans quitter la page** : un panneau qui sort
  d'un bord, par-dessus la page (Drawer).
- **Modifier une valeur là où elle est lue** : la référence d'un plan, une
  note, sans formulaire à part (Inplace).
- **Voir une image en grand** : une vignette de plan, une photo de pièce, à
  tourner et agrandir (Image).
- **Comparer deux révisions** d'un plan, l'une sur l'autre (ImageCompare).

## Ce que fait mkm

| Composant    | Dans mkm                                                                                          |
| ------------ | ------------------------------------------------------------------------------------------------- |
| Drawer       | un panneau de 20 rem (ou toute la hauteur en haut et en bas), sur un voile ; titre, croix, contenu qui défile, pied ; `position` left, right, top, bottom, full ; `dismissableMask`, `showCloseIcon` ; glisse de son bord |
| Inplace      | un texte (`display`) grisé au survol ; un clic montre le contenu ; `closable`                    |
| Image        | une image ; `preview` : un œil au survol, sur un voile ; l'aperçu en grand avec tourner, agrandir, réduire, fermer |
| ImageCompare | deux images l'une sur l'autre, découpées à une poignée ronde qui glisse                          |

## Le choix

- **Drawer** hérite de `HeaderedContentControl` : `Header`, `Content`,
  `Footer`, `Position`, `IsOpen` (lié dans les deux sens), `Show()`, `Hide()`,
  `DismissableMask`, `ShowCloseIcon`. Il couvre son conteneur : posé dans la
  grille racine de la fenêtre, après son contenu, il couvre la fenêtre. Fermé,
  il n'est plus dans la mise en page. À l'ouverture, le clavier y entre ; à la
  fermeture, il revient où il était.
- **Inplace** hérite de `ContentControl` : `Display` (un bouton), `IsActive`,
  `Activate()`, `Deactivate()`. Échap, dans le contenu, rend le texte.
- **Image** hérite de `Control` : `Source`, `Stretch`, `Preview`,
  `ShowPreview()`. L'aperçu est une fenêtre sans bord sur celle de l'image :
  le voile du thème, l'image, la barre en haut à droite. Le nom « Image » est
  aussi celui de WPF : dans un fichier VB qui importe `Mkm.Wpf.Controls`,
  l'écrire en entier.
- **ImageCompare** hérite de `Control` : `LeftImage`, `RightImage`, `Value`
  (0 à 100). Sans hauteur, 16/9 de sa largeur. La poignée est un `Slider` de
  WPF (le clavier, l'accessibilité) habillé en rond.

## Écarts avec mkm

- Drawer : pas de `blockScroll` (la page sous le voile ne reçoit plus la
  souris, ce qui suffit) ; pas de `modal="false"` (toujours un voile).
- Inplace : pas de croix `closable` ; Échap ou `Deactivate()` rendent le texte.
- Image : l'aperçu ne s'anime pas en s'ouvrant.

## Clavier, accessibilité, performance

- Drawer : un volet nommé par son titre ; Tab y tourne, Échap le ferme.
- Inplace : le texte est un bouton (Entrée, Espace) ; Échap le rend.
- Image : une image nommée par `AutomationProperties.Name` ; le bouton
  « Aperçu » ; dans l'aperçu, « Tourner à droite », « Tourner à gauche »,
  « Réduire », « Agrandir », « Fermer » ; Échap le ferme.
- ImageCompare : un groupe, sa poignée un curseur « Comparer » ; flèches, Page
  haut et bas, Début, Fin.

## La documentation et les tests

Les pages Drawer, Inplace, Image et ImageCompare (les images d'ui-kit, voir
`ORIGINES.md`) ; `Lot16Tests` ; `SuperpositionTests` (le tiroir, Échap et le
clavier rendu), `DiversTests` (sur place, l'aperçu, la poignée au clavier).
