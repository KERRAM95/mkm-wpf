# Les champs : InputText et Select

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`src/Mkm.Wpf/Controls/InputText`,
`src/Mkm.Wpf/Controls/Select`). Le libellé, le message sous un champ et la
marque d'un champ obligatoire viendront avec la fiche de propriétés de mkm
(`PropertyPanel`).

## Le besoin

Les champs de saisie d'une application : la zone de texte d'une ligne et la
liste déroulante, éditable ou non, avec des listes parfois longues de
plusieurs milliers de valeurs. Ils doivent être rapides : une liste qui
s'ouvre aussitôt, aucune animation qui bloque.

## Ce que fait mkm

- **InputText**, le champ de 3DEXPERIENCE : 24 px de haut, coins de 4 px,
  bordé de gris, plus foncé au survol ; au focus, la bordure prend le bleu
  du thème, sans anneau ; grisé, un fond gris clair et le texte au gris de
  la bordure ; un texte d'aide (`placeholder`) ; `invalid` le borde de la
  couleur d'erreur.
- **Select**, la liste déroulante de 3DEXPERIENCE :
  - **le champ et son bouton :** un champ de 24 px, au bord gauche arrondi (5 px), et à sa droite un bouton gris de 22 px qui porte la flèche ;
  - **au survol :** tout fonce d'un pas ;
  - **au focus :** les bordures prennent le bleu, le bouton un bleu pâle, la flèche un bleu foncé ;
  - **la liste :** elle s'ouvre 4 px sous le champ, ses choix de 22 px séparés d'un trait, le choix actuel cerné de bleu ;
  - **éditable :** la valeur peut sortir de la liste (`editable`).
- Une partie de ces mesures n'a pas de jeton dans mkm : elle est écrite dans
  la feuille CSS de chaque preset (`css`).

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `TextBox` et `ComboBox` de WPF | tout le comportement : saisie, annulation, `MaxLength`, majuscules (`CharacterCasing`), lecture seule, liste éditable, recherche au clavier, F4 et Alt+Bas, accessibilité ; virtualisation de la liste (`VirtualizingStackPanel`) | l'apparence de mkm ; pas de texte d'aide |
| MahApps.Metro (MIT) | des `TextBox` et `ComboBox` restylés, un texte d'aide par propriété jointe (`TextBoxHelper.Watermark`) | l'apparence de Metro |
| MaterialDesignInXamlToolkit (MIT) | idem, un texte d'aide qui flotte (`HintAssist`) | l'apparence de Material |
| WPF UI (MIT) | idem, `PlaceholderText`, une icône dans le champ | l'apparence de Fluent |
| HandyControl (MIT) | idem, un titre au-dessus du champ (`InfoElement.Title`) | son apparence |

Toutes ces bibliothèques font la même chose : elles **restylent les contrôles
de WPF** et leur ajoutent un texte d'aide. Aucune ne réécrit la saisie.

## Le choix (technique)

- **`InputText` hérite de `TextBox`, `Select` de `ComboBox`** : tout le
  comportement de WPF est gardé, y compris ce dont les applications ont besoin
  (`MaxLength`, majuscules, lecture seule, liste éditable). On n'y ajoute que
  ce que mkm a en plus : `Placeholder` et `Invalid`.
- **Les gabarits** sont ceux de mkm, aux jetons du thème
  (`mkm.inputtext.…`, `mkm.select.…`).
- **Ce que la feuille CSS de mkm dit sans jeton** devient des jetons de
  mkm-wpf, écrits à la main par thème dans `Theming/Tokens/<thème>.css.tokens` :
  hauteur de 24 px, retraits, bouton de la flèche, couleurs du bouton au
  survol, au focus et enfoncé, choix de 22 px, trait du choix actuel. Ces
  valeurs renvoient à des teintes différentes selon le thème (`dsblue.100`
  dans Dashboard, `primary.100` dans ARA), comme dans mkm.
- **La liste est virtualisée** (`VirtualizingStackPanel`, recyclage) : seuls
  les choix visibles existent, si bien qu'une liste de milliers de valeurs
  s'ouvre aussitôt. Aucune animation : la liste paraît d'un coup, comme dans
  mkm.
- **« Select » est un mot réservé de VB** : la classe s'écrit `[Select]` en
  VB ; en XAML, `<mkm:Select>`.

## Écarts avec mkm

- Le dernier choix de la liste garde son trait (mkm l'enlève).
- La liste ne se recale pas sur le bord droit de la fenêtre : WPF la garde à
  l'écran à sa façon.
- Dans Dashboard, le choix surligné (texte blanc sur le bleu de
  3DEXPERIENCE) est à 3,6:1, sous les 4,5:1 : c'est la couleur de la
  plateforme, que mkm garde aussi. ARA est à 9,4:1. `ThemesTests` ne mesure
  donc pas cette paire.

## La vitrine et les tests

**La vitrine (page Champs) :**
- `InputText` : texte d'aide, majuscules et 15 caractères, lecture seule, grisé, invalide ;
- `Select` : texte d'aide, liste éditable, liste de 5 000 valeurs, grisée, invalide.

**Les tests :**
- les gabarits et la hauteur du thème ;
- le texte d'aide qui paraît et disparaît ;
- la liste éditable ;
- la virtualisation ;
- le style des choix ;
- les paires texte/fond dans `ThemesTests`.

## Sources

- mkm : `ui-kit`, `packages/themes/src/presets/dashboard/inputtext`,
  `…/select`, `…/ara/inputtext`, `…/ara/select` (jetons et feuilles).
- MahApps.Metro, MaterialDesignInXamlToolkit, WPF UI, HandyControl : leurs
  `TextBox` et `ComboBox` restylés (voir `docs/fondations/theme.md` pour leurs
  versions et licences).
