# Tile

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm n'a
> pas ce composant : son apparence est celle de la tuile de résultat de
> 3DEXPERIENCE, relevée au pixel ; Mourad l'a validée sur maquette.

**État :** validée le 15/11/2026 (maquette, variante avec liseré) · faite le
15/11/2026 (`Controls/Tile`).

## Le besoin

Montrer un objet dans une liste, d'un coup d'œil, comme la recherche de
3DEXPERIENCE montre ses résultats : sa vignette, l'icône de son type, son titre,
une ligne de ce qui le situe (type, révision, état, propriétaire, date) et son
identifiant ; un menu pour agir dessus. La liste peut être longue (défilement
virtuel) et étroite (un panneau latéral) : la tuile garde sa hauteur, seul le
texte se coupe.

`IdCard` ne convient pas : c'est l'en-tête d'un objet ouvert (vignette de 72 px,
titre de 16 px, boutons de repli et de fermeture, facettes), pas une ligne de
liste ; sa hauteur suit ses détails.

## Ce que fait 3DEXPERIENCE

La tuile « large » de la recherche (3DSearch, la tuile responsive de WebUX),
relevée sur une capture du panneau de recherche et sur son DOM (`picture`,
`icon`, `title`, `subtitle`, `description`, `menu`, `color`) :

- **la tuile :** 70 px de haut, fond blanc, bord gris de 1 px (`#E2E4E3`),
  10 px entre deux tuiles, sur le fond gris de la liste (`#F1F1F1`) ;
- **la vignette :** une zone grise (`#F4F5F6`) de 79 × 58, à 5 px du bord ; les
  vignettes de la plateforme (144 × 108) la remplissent ;
- **le titre :** l'icône du type en 16 px, 4 px, puis le titre en 14 px bleu
  (`#368EC4`), sur une ligne, coupé de points de suspension ;
- **le sous-titre :** 11 px gris (`#77797C`), les éléments séparés de « | »,
  le propriétaire en lien bleu ;
- **la description :** 11 px gras, gris foncé (`#3D3D3D`) ;
- **le menu :** un chevron gris de 14 × 7 au bord droit, centré ;
- **la couleur :** un liseré à gauche, quand l'objet en a une.

Les six couleurs sont, à l'identique, celles de la palette du tableau de bord de
mkm : `dsblue.500`, `dsgray.50`, `100`, `200`, `500`, `700`.

## L'état de l'art

| Candidat | Licence | net48 | Vivant | Ce qu'il fait bien | Ce qui manque |
| -------- | ------- | ----- | ------ | ------------------ | ------------- |
| MahApps.Metro `Tile` | MIT | oui | oui | une tuile cliquable (style Metro) | ni vignette, ni sous-titre, ni menu |
| HandyControl `Card` | MIT | oui | oui | une carte à en-tête et pied | sa mise en page, pas celle d'une ligne de résultat |
| mkm-wpf `IdCard` | — | oui | — | vignette, titre, détails, boutons | l'en-tête d'un objet, non une ligne (plus haut) |

**Dans WPF :** un `Control` à gabarit suffit ; la liste qui porte les tuiles
(`Listbox`, `ItemsControl` virtualisé) fait la sélection et le clavier.

## Le choix (technique)

Un composant à nous, `Tile`, qui hérite de `Control` : aucun code repris, la
mise en page est celle de 3DEXPERIENCE, chaque mesure et chaque couleur un jeton.

- **Les jetons** (`tile.*`) s'écrivent à la main dans `Dashboard.css.tokens` et
  `Ara.css.tokens`, mkm n'ayant pas de preset `tile`. Ils renvoient aux couleurs
  sémantiques (`content.background`, `surface.50`, `surface.200`,
  `surface.500`, `surface.700`, `primary.500`) : dans Dashboard, ce sont
  exactement celles de 3DEXPERIENCE ; dans ARA, celles d'ARA.
- **Le titre et le sous-titre sous 4,5:1, comme 3DEXPERIENCE :** le bleu du
  titre et le gris du sous-titre sur le blanc font 3,6:1 et 4,4:1 dans
  Dashboard ; ils restent ainsi, au-dessus de 3:1, comme le bouton principal de
  3DX (`ThemesTests`, paires permises).
- **La vignette :** une image tenue dans sa zone sans être rognée ni agrandie
  (une icône de 32 px reste nette, au centre) ; une géométrie, une icône de
  32 px grise. Sans vignette, la zone garde son fond gris.
- **Le sous-titre est un contenu :** un texte, ou un `TextBlock` dont les `Run`
  colorent un élément (le propriétaire en `tile.link.color`) ; il tient sur une
  ligne, coupé de points de suspension.
- **Le liseré (`Stripe`) est la politique de l'application**, comme la couleur
  de maturité d'`IdCard` : un pinceau, aucun par défaut.
- **Le menu :** le chevron (`HasMenu`) lève `MenuRequested` ; si personne ne
  le traite, il ouvre le `ContextMenu` de la tuile sous lui. L'application qui
  préfère un `Popover` le traite.
- **Survol et choix :** sous la souris, le bord prend `tile.hover.border.color` ;
  `IsSelected` cerne la tuile de `tile.selected.border.color` (la liste le lie
  au choix de sa ligne).

## L'API

- **Propriétés :** `Thumbnail` (objet : image ou géométrie), `TypeIcon` (objet),
  `Title` (texte), `Subtitle` (objet), `Description` (texte), `Stripe`
  (pinceau, aucun), `HasMenu` (faux), `MenuLabel` (texte, par défaut
  `tile.menu`), `IsSelected` (faux).
- **Événement routé :** `MenuRequested` (le chevron cliqué, à bulles).
- **Partie du gabarit :** `PART_MenuButton` (`ButtonBase`).
- **Jetons :** `tile.height`, `tile.background`, `tile.border.color`,
  `tile.border.width`, `tile.hover.border.color`, `tile.selected.border.color`,
  `tile.selected.border.width`, `tile.thumbnail.margin`, `.width`, `.height`,
  `.background`, `.icon.size`, `tile.stripe.width`, `tile.content.margin`,
  `tile.type.icon.size`, `tile.type.icon.margin`, `tile.title.color`,
  `.font.size`, `.line.height`, `tile.subtitle.color`, `.font.size`,
  `.line.height`, `.margin`, `tile.link.color`, `tile.description.color`,
  `.font.size`, `.font.weight`, `.line.height`, `.margin`, `tile.menu.color`,
  `tile.menu.size`, `tile.menu.icon.size`, `tile.menu.margin`.

## Clavier, accessibilité, performance

La tuile ne prend pas le clavier : la ligne de la liste le prend. Son chevron le
prend (Tab), et Entrée ou Espace l'ouvrent. Pour un lecteur d'écran, la tuile est
un groupe nommé par son titre ; le chevron se nomme « Actions » (`MenuLabel`).
Les vignettes et les icônes sont des dessins, non lus. Aucune animation ; dans
une liste virtualisée, seules les tuiles visibles sont faites.

## La vitrine et les tests

Sa page (`tile.xml`) : Import, Basique, Sans vignette, Liseré, Menu, Liste,
Accessibilité. Les tests : les mesures et couleurs du thème, le liseré absent
puis présent, la zone de la vignette gardée sans image, le chevron absent sans
`HasMenu`, `MenuRequested` puis le `ContextMenu`, le cadre d'`IsSelected`, le nom
du groupe et du chevron.
