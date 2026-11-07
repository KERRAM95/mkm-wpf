# Changements

Les évolutions notables de mkm-wpf, version par version. Le format suit
[Keep a Changelog](https://keepachangelog.com/fr/1.1.0/), les versions
[SemVer](https://semver.org/lang/fr/).

## [Non publié]

### Ajouté

- Le squelette du dépôt : la solution `Mkm.Wpf.sln`, la bibliothèque
  `Mkm.Wpf` (aucun contrôle encore), la vitrine `Mkm.Wpf.Showcase`, les
  tests `Mkm.Wpf.Tests` avec les règles communes à tous les contrôles.
- Les règles de travail (`CLAUDE.md`), dont l'open source d'abord, le modèle de
  fiche d'un composant et le registre des origines (`ORIGINES.md`).
- La CI sous Windows (compilation et tests) et la compilation sous Linux
  (`outils/linux/verifier.sh`).
- La fiche des fondations, thème et langue (`docs/fondations/theme-et-langue.md`) :
  une variable chacun, changeables pendant l'exécution, comme mkm.
- La langue : `MkmResources` (la configuration de `App.xaml`, `Language`),
  `Localizer` (langue à chaud, langues proposées, sources, `T(clé, paramètres)`,
  repli langue parente → anglais → clé), `{mkm:T clé}` en XAML, les sources
  `ResxTextSource` et `TextTable`. Les éléments WPF prennent le format régional
  de Windows pour les dates et les nombres.
- La vitrine en anglais et en français, avec le choix de la langue dans sa barre.
- La fiche du thème (`docs/fondations/theme.md`) : Dashboard et ARA, sans mode
  sombre ni couleurs de maturité.
- Le thème : `MkmResources.Theme` (Dashboard par défaut, ARA), changeable à chaud ;
  `ThemeDefinition` (jetons, renvois, dérivation) et `Themes` ; une ressource par
  jeton, `mkm.<chemin>`, typée (pinceau, taille, marge, rayon, ombre, police,
  graisse, durée) ; les jetons importés de mkm (`outils/themes/importer-mkm.mjs`).
  Les textes se dessinent nets (mode `Display`).
- La vitrine habillée par le thème, avec le choix du thème dans sa barre.
- Les icônes : les 95 de mkm (`Icons`, dessinées par Tabler Icons 3.48.0, MIT) et
  l'élément `Icon`, qui les trace à la couleur du texte ; `THIRD-PARTY-NOTICES.md`.
- La fiche de l'ActionBar (`docs/composants/actionbar.md`) : comme celle de mkm.
