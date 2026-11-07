# Le thème

> Fiche d'une fondation, écrite avant le code (`CLAUDE.md`, « L'open source
> d'abord »). Mourad valide ce qui touche à l'apparence (« À valider ») ; la
> technique se décide ici (`CLAUDE.md`, « Qui décide »).

**État :** à l'étude — proposition du 07/11/2026.

## Le besoin

Une application choisit son thème par **une variable**, posée dans
`App.xaml` à côté de la langue, et le change pendant qu'elle tourne :

```xml
<mkm:MkmResources Theme="Ara" Language="fr" />
```

Le principe est validé avec la langue (`docs/fondations/theme-et-langue.md`) ;
cette fiche dit ce qu'est un thème, ce qu'il contient, et lesquels on fournit.

## Ce que fait mkm

- **Un thème est un preset** : des objets de jetons, à trois niveaux.
  - **Primitifs** : les palettes (onze teintes par couleur, de 50 à 950), les
    rayons d'angle (`xs` à `xl`), les couleurs d'état et de maturité.
  - **Sémantiques** : la couleur principale, les surfaces, le texte (normal,
    survolé, atténué), le contenu, les champs de saisie (marges, taille du
    texte, petite et grande tailles), les listes, la navigation, les
    superpositions (menus, popovers, fenêtres modales), l'anneau de focus,
    l'opacité d'un élément désactivé, la typographie.
  - **Par composant** : les jetons d'un composant, qui renvoient aux
    sémantiques.
- **Dashboard** porte les mesures relevées sur le tableau de bord
  3DEXPERIENCE, en pixels : Arial 12 px, une ligne de 14 px, des rayons de 2 et
  4 px, des champs à 7 × 6 px de marge, aucune transition.
- **ARA** reprend les mesures de Dashboard ; seules les couleurs passent à
  celles d'une charte (un marine `#014491`, des gris froids, onze couleurs
  secondaires). `ara.spec.ts` le vérifie : un écart avec Dashboard qui n'est
  pas une couleur le fait échouer, comme un texte qui se lit sous 4,5:1 sur son
  fond.
- **Un thème dérive d'un autre** (`definePreset`) : le gabarit de widget pose
  sa couleur de marque dans `semantic.primary`, rien d'autre.
- **Le mode sombre ne suit pas le système.**

## L'état de l'art

| Bibliothèque             | Ce qu'est un thème                                                                 | Les clés                                   | Construit                                     |
| ------------------------ | ---------------------------------------------------------------------------------- | ------------------------------------------ | --------------------------------------------- |
| MaterialDesignThemes 5.4 | un objet `Theme` (couleurs principale, secondaire, fond, texte) ; `PaletteHelper`  | `MaterialDesign.Brush.…`                   | **en code** : les pinceaux naissent de l'objet |
| MahApps.Metro 2.4 / ControlzEx 7 | un dictionnaire par couple fond × couleur d'accent (« Light.Blue »), généré | `MahApps.Colors.…` et `MahApps.Brushes.…`  | par un générateur, à partir de gabarits        |
| WPF UI 4.3               | un dictionnaire XAML par fond (clair, sombre), l'accent posé en code               | les noms de WinUI (`TextFillColorPrimaryBrush`…) | XAML, plus l'accent en code              |
| HandyControl 3.5         | un dictionnaire de couleurs et de pinceaux par habillage                            | `PrimaryBrush`, `RegionBrush`…             | XAML                                          |

Ce qu'elles ont toutes en commun : les gabarits lisent leurs ressources par
`DynamicResource`, et le thème se remplace en bloc dans
`Application.Resources`. Ce qui les sépare : un thème écrit à la main en XAML
(un fichier par thème, qui recopie tout) ou construit en code depuis une
définition (MaterialDesign).

## Le choix (technique)

1. **Un thème est une définition en VB**, comme un preset de mkm, aux mêmes
   trois niveaux et avec les mêmes noms de jetons. **ARA dérive de Dashboard**
   en ne changeant que ses couleurs ; une application dérive le sien de même,
   pour sa couleur de marque. *Pourquoi :* ARA ne recopie pas les mesures de
   Dashboard, et un thème de mkm se transpose jeton par jeton.
2. **`Theme` construit, depuis la définition, un dictionnaire de ressources**
   — couleurs, pinceaux figés, rayons, marges, tailles, police — et le
   remplace à chaud ; les gabarits ne lisent que ces ressources, par
   `DynamicResource`. *Pourquoi :* c'est la technique de toutes les
   bibliothèques étudiées ; construire en code, comme MaterialDesign, évite un
   fichier XAML par thème.
3. **Les clés** : `Mkm.` puis le chemin du jeton — `Mkm.Color.Navy.500` pour
   une couleur de palette, `Mkm.Brush.Text`, `Mkm.Brush.Primary` pour les
   pinceaux sémantiques, `Mkm.Radius.Xs`, `Mkm.FontSize`… Les jetons d'un
   composant (`Mkm.Button.Padding`) arrivent avec lui, dans chaque thème.
   *Pourquoi :* un préfixe évite toute collision avec les ressources de
   l'application, comme `MahApps.` et `MaterialDesign.`.
4. **Les valeurs sont celles de mkm, aux mêmes nombres** : un pixel CSS vaut
   une unité WPF (1/96 de pouce), si bien que 12 px restent 12.
5. **Le texte** : Arial 12, une ligne de 14 ; les fenêtres en
   `TextOptions.TextFormattingMode="Display"`. *Pourquoi :* WPF rend les
   petits textes flous par défaut ; le mode `Display` les dessine nets, comme
   Windows.
6. **Les vérifications, comme mkm** : un test compare ARA à Dashboard (seules
   les couleurs diffèrent) ; un autre mesure chaque texte sur son fond
   (4,5:1 au moins).
7. **L'origine** : la structure des jetons vient de PrimeUIX (MIT), par mkm ;
   les valeurs de Dashboard et d'ARA viennent de mkm. `ORIGINES.md` le dira
   avec le code.

## À valider

1. **Les thèmes fournis** : Dashboard, l'apparence de 3DEXPERIENCE (celle des
   widgets mkm), par défaut ; et ARA, ses mesures aux couleurs de la charte.
2. **Pas de mode sombre pour l'instant**, comme mkm : il viendrait plus tard
   comme un thème de plus.
3. **La barre de titre de Windows reste** : une barre de titre dessinée par
   nous, à la manière de 3DEXPERIENCE, serait un composant à part, plus tard.
4. **Les couleurs de maturité de mkm** (Private, In Work, Frozen, Released,
   Obsolete) entrent dans les thèmes.

## Sources

- mkm : `ui-kit`, `packages/themes/src/presets/dashboard` et `ara` (`base`),
  `src/ara.spec.ts`, `templates/widget/src/theme.ts`.
- [MaterialDesignInXamlToolkit, *Advanced Theming*](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Advanced-Theming)
- [WPF UI (lepoco/wpfui)](https://github.com/lepoco/wpfui)
- Versions : les métadonnées des paquets sur nuget.org, relevées le
  07/11/2026.
