# Pastilles et personnes : Badge, OverlayBadge, Avatar, Chip

> Fiche du lot 11, priorité 1.

**État :** livrée dans la version en préparation.

## Le besoin

- **Un compteur** sur une icône ou un bouton (des révisions nouvelles, des
  alertes), et des étiquettes courtes de couleur (Badge, OverlayBadge).
- **Les personnes** d'un document ou d'une équipe, en petit (Avatar,
  AvatarGroup).
- **Des filtres** qu'on retire d'un clic, des mots-clés (Chip).

## Ce que fait mkm

| Composant    | Dans mkm                                                                                      |
| ------------ | --------------------------------------------------------------------------------------------- |
| Badge        | 18 px de haut, la valeur en gras de 9 px, aux coins du thème, rond pour un caractère, point sans valeur |
| OverlayBadge | la pastille centrée sur le coin haut-droit de son contenu, cernée de 2 px du fond             |
| Avatar       | 24 px (36, 48), gris pâle, des initiales, une icône ou une image ; carré ou rond ; en groupe, chevauchés |
| Chip         | une pastille de 16 px de rayon, grise ; icône, photo ; retirable, une croix ronde (Entrée, Retour arrière) |

## Le choix

Des composants à nous, sur les jetons de mkm (`avatar`, `badge`, `chip`,
`overlaybadge`, importés de ui-kit) :

- **Badge** hérite de `Control` : `Value`, `Severity`, `Size` ; `IsDot` et
  `IsCircle` (lecture seule) suivent la valeur.
- **OverlayBadge** hérite de `ContentControl` ; un petit panneau
  (`CoinDePastille`) centre la pastille sur le coin sans lui donner de place,
  comme le `translate(50%, -50%)` de mkm.
- **Avatar** hérite de `Control` : `Label`, `Icon`, `Image`, `Size`, `Shape` ;
  **AvatarGroup** est un `ItemsControl` qui fait chevaucher ses avatars
  (`avatar.group.offset`).
- **Chip** hérite de `ContentControl` : `Icon`, `Image`, `Removable`,
  l'évènement routé `Remove` et `Close()` ; retirée, la puce se cache, comme
  mkm.

Les retraits du Chip (`chip.padding.x` et `.y`, des nombres) deviennent des
épaisseurs dans les feuilles `.css.tokens` ; le contour de l'OverlayBadge
aussi (`overlaybadge.border.width`).

## Écarts avec mkm

- La croix du Chip prend le clavier (Tab) : sans elle, une puce retirable ne se
  retirait qu'à la souris.

## Clavier, accessibilité, performance

- Badge : un texte, sa valeur ; un point ne se lit pas, son sens doit être dit.
- Avatar : une image, nommée par `AutomationProperties.Name`, à défaut par son
  texte ; AvatarGroup, un groupe.
- Chip : un groupe nommé par son texte ; sa croix, un bouton « Retirer »
  (Tab, Entrée, Espace, Retour arrière), l'anneau du focus au clavier.

## La documentation et les tests

Les pages Badge (avec OverlayBadge), Avatar (avec AvatarGroup) et Chip ;
`Lot11Tests` ; `DiversTests` (la croix et Retour arrière d'un Chip, la pastille
lue, l'avatar nommé).
