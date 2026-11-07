# L'identité et les choix : IdCard, Tag, SelectButton et InputNumber

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`Controls/IdCard`, `Controls/Tag`,
`Controls/SelectButton`, `Controls/InputNumber`).

## Le besoin

Montrer quel objet on modifie (le plan et l'article ouverts, leur révision, leur
maturité), étiqueter un état, choisir entre quelques options d'un clic, saisir un
nombre (une masse, un nombre de feuilles).

## Ce que fait mkm

- **IdCard** (preset `idcard`, redessiné pour 3DEXPERIENCE) :
  - **la vignette :** dans une tuile de 72 px, sur un fond pâle, sans être rognée ni agrandie ;
  - **le titre :** 16 px semi-gras (14 px en petit), la révision dans une pastille grise qui reste visible quand il se coupe ;
  - **les boutons :** replier, fermer, et ceux qu'on ajoute ;
  - **les détails :** 12 px gris, le propriétaire, le type et son icône, la maturité dans une étiquette, le reste ;
  - **les facettes :** des onglets sous la carte.

  Une maturité nommée prend sa couleur de mkm ; une couleur donnée (`maturity.color`) l'emporte, le texte prenant la couleur la plus lisible.
- **Tag** (preset `tag`) : l'étiquette de la grille de 3DX, 18 px, à angles
  droits, 12 px ; le texte blanc sur la couleur de la gravité assombrie de 30 %.
- **SelectButton** (presets `selectbutton` et `togglebutton`) :
  - **les boutons :** à bascule, collés, 24 px, coins de 4 px aux bouts ;
  - **le bouton choisi :** la sélection de 3DX, bordé de la couleur principale, qui recouvre la bordure de son voisin ;
  - **le choix :** un ou plusieurs ; `allowEmpty`.
- **InputNumber** (styles de PrimeVue, jetons de Nora) :
  - **la saisie :** chiffres seuls, format régional, bornes, pas, décimales, préfixe et suffixe ;
  - **le clavier :** les flèches haut et bas changent le nombre ;
  - **les boutons :** empilés dans le champ ou à l'horizontale, qui répètent tant qu'on les tient.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `ListBox` de WPF | la sélection simple et multiple, les flèches, les données | l'apparence de boutons collés |
| `ToggleButton` de WPF | la bascule | le groupe |
| Extended WPF Toolkit `DecimalUpDown` (Ms-PL), MahApps `NumericUpDown` (MIT) | la saisie d'un nombre, ses bornes, ses boutons | leur apparence ; une licence ou une dépendance de plus |

Une carte d'identité ou une étiquette n'existent dans aucune bibliothèque WPF :
ce sont des composants de mkm.

## Le choix (technique)

- **Les couleurs de maturité restent hors de la bibliothèque** (décision de
  Mourad) : `IdCard.MaturityColor` et `Tag.Color` reçoivent la couleur de la
  politique de l'application ; le texte prend le noir ou le blanc, le plus
  lisible (`Tag.ReadableForeground`, le `textOnColor` de mkm). Sans couleur, la
  maturité est une étiquette grise. Les couleurs d'état de mkm ne sont pas
  importées.
- **`IdCard` hérite de `ContentControl`** : son contenu vient sous elle (les
  facettes : un `Tabs` « strip ») ; `Details` est une liste de textes ou
  d'éléments.
- **`SelectButton` hérite de `ListBox`**, ses éléments (`SelectButtonItem`) de
  `ListBoxItem` : toute la sélection de WPF est gardée.
- **`InputNumber` est un `Control`** autour d'un `InputText` et de deux
  `RepeatButton` : le nombre (`Value`, un `Double?`) se lit à chaque frappe et
  s'écrit au format de la langue de l'élément en quittant le champ.
  `InputText` y gagne une propriété `CornerRadius`.

## L'API

- **IdCard :** `Title`, `Revision`, `Type`, `TypeIcon`, `Thumbnail`, `Owner`,
  `Maturity`, `MaturityColor`, `Details`, `Actions`, `Collapsible`,
  `IsCollapsed`, `Closable`, `Size` ; l'événement `Close`.
- **Tag :** `Severity` (`TagSeverity`), `Icon`, `Rounded`, `Color` ; le contenu
  est le texte.
- **SelectButton :** `SelectionMode`, `AllowEmpty`, `Invalid`, et tout
  `ListBox` ; **SelectButtonItem :** `Icon`, `IsFirst`, `IsLast`.
- **InputNumber :** `Value`, `Minimum`, `Maximum`, `Step`, `MinFractionDigits`,
  `MaxFractionDigits`, `UseGrouping`, `Prefix`, `Suffix`, `ShowButtons`,
  `ButtonLayout` (`Stacked`, `Horizontal`), `Placeholder`, `Invalid`,
  `IsReadOnly` ; `Format`, `Parse`, `Spin`.

## Écarts avec mkm

- IdCard : pas de squelettes de chargement ; les facettes sont son contenu.
- InputNumber : pas de disposition verticale ni de mode monétaire
  (`mode="currency"`) ; un préfixe ou un suffixe en tient lieu.

## La vitrine et les tests

La page « Identité et choix » montre trois cartes (complète avec la couleur de
maturité de l'application, petite sans couleur, repliée), les étiquettes, les
choix par boutons et les saisies de nombres. Les tests vérifient la couleur et
le texte lisible des étiquettes, les détails et le repli de la carte, la couleur
de maturité, le choix qu'un clic enlève, l'écriture et la lecture d'un nombre,
ses bornes et ses boutons.
