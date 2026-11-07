# Les boutons : Button et Checkbox

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`src/Mkm.Wpf/Controls/Button`,
`src/Mkm.Wpf/Controls/Checkbox`).

## Le besoin

Les commandes d'une fenêtre (Enregistrer, Annuler, Supprimer…) et les choix
oui/non d'un formulaire. Des boutons de plusieurs gravités, avec ou sans
icône, et une case à cocher qui peut être indéterminée.

## Ce que fait mkm

- **Button**, le bouton de 3DEXPERIENCE (presets `button`) :
  - **la taille :** 24 px de haut, coins de 4 px, 65 px de large au moins avec un libellé, 8 px entre l'icône et le libellé ;
  - **les gravités :** `primary`, `secondary`, `success`, `info`, `warn`, `help`, `danger`, `contrast` ;
  - **les formes :** plein, `outlined`, `text`, `link` ;
  - **les états :** survol, appui et focus clavier changent le fond et la bordure ; le principal de 3DEXPERIENCE a ses bleus à lui au clavier et enfoncé ;
  - **le bouton « lite » :** d'icône seule et en texte, sans fond ni bordure, 16 px de large, gris, du gris du texte au survol, bleu clair enfoncé ;
  - **grisé :** il garde ses couleurs, à moitié transparent.
- **Checkbox**, la case de 3DEXPERIENCE (presets `checkbox`) :
  - **la case :** 14 px, coins de 2 px, gris clair bordée de gris ;
  - **cochée :** la couleur principale, et une coche blanche qui la remplit, d'un trait de 2 px ;
  - **les états :** survolée ou au clavier, elle fonce d'un pas, enfoncée d'un pas de plus ;
  - **indéterminée :** un trait ;
  - **invalide :** bordée de la couleur d'erreur ;
  - **grisée :** à moitié transparente.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `Button` et `CheckBox` de WPF | tout le comportement : `Click`, `Command` (grisé quand elle ne peut pas s'exécuter), `IsDefault` et `IsCancel` (Entrée et Échap d'un dialogue), touches d'accès, trois états, accessibilité | l'apparence de mkm |
| MahApps.Metro, MaterialDesignInXamlToolkit, WPF UI, HandyControl (MIT) | des boutons restylés, des « variantes » par style nommé ou propriété | leur apparence |

Les bibliothèques restylent les contrôles de WPF ; aucune ne réécrit le clic.

## Le choix (technique)

- **`Button` hérite du `Button` de WPF, `Checkbox` de `CheckBox`** : tout leur
  comportement est gardé.
- **La gravité et la forme** sont deux propriétés (`Severity`, `Variant`), comme
  les props de mkm. Les couleurs de chaque état viennent des jetons de mkm
  (`mkm.button.<gravité>.…`, `mkm.button.outlined.<gravité>.…`) par
  `SetResourceReference` : elles suivent le thème à chaud.
- **Ce que la feuille CSS de mkm dit sans jeton** (hauteur, largeur, bleus du
  principal, couleurs du bouton « lite », fonds de la case survolée et enfoncée,
  écart du libellé) est écrit dans `Theming/Tokens/<thème>.css.tokens`.
- **Le nom `Checkbox`** est celui de mkm. VB ne distingue pas la casse :
  `Checkbox` et `CheckBox` (WPF) se confondent quand les deux espaces de noms
  sont importés ; on écrit alors `Mkm.Wpf.Controls.Checkbox`. En XAML, le
  préfixe (`mkm:Checkbox`) suffit.

## L'API

- **Button :** `Severity` (`ButtonSeverity`, `Primary`), `Variant`
  (`ButtonVariant` : `Filled`, `Outlined`, `Text`, `Link`), `Icon` ; en lecture
  seule `HasIcon`, `IsIconOnly`, `IsFocusVisible` ; les pinceaux de chaque état
  (`HoverBackground`, `PressedBackground`, `FocusBackground`… posés par la
  gravité). Le reste est celui de WPF.
- **Checkbox :** `Invalid`. Le libellé est `Content`, à 6 px de la case (la case
  de mkm n'a pas de libellé à elle : écart assumé).

## Clavier, accessibilité, performance

Ceux de WPF : Espace et Entrée, `IsDefault`/`IsCancel`, touches d'accès. Le
focus clavier se voit ; le focus à la souris non (`IsFocusVisible`). Un bouton
d'icône seule se nomme par son `ToolTip` ou `AutomationProperties.Name`.

## Écarts avec mkm

- Un lien n'est pas souligné au survol ; les couleurs changent sans transition.
- Le libellé de la case fait partie d'elle (le survoler allume la case).

## La vitrine et les tests

La page « Boutons » montre chaque gravité dans chaque forme, avec icône,
d'icône seule, « lite » et grisé, puis la case dans ses états. Les tests
vérifient la hauteur du thème, `IsIconOnly`, les couleurs de la gravité et de la
forme, la case indéterminée ; les règles communes vérifient que chaque gabarit
s'applique et que chaque ressource qu'il nomme existe dans chaque thème.
