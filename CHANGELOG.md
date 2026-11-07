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
- L'ActionBar, comme celle de mkm (`docs/composants/actionbar.md`) : `ActionBar`
  (sections en onglets, commandes fixes, repli, libellés au clic droit, défilement,
  Début et Fin), `ActionBarSection`, `ActionBarButton`, `ActionBarToggleButton`,
  `ActionBarDropDown` (la commande enfoncée, sinon la dernière lancée, sinon la
  première), `ActionBarSeparator` ; info-bulles « libellé — indice », aussi au
  clavier ; jetons `mkm.actionbar.*`, `mkm.menu.*`, `mkm.contextmenu.*`,
  `mkm.tooltip.*`. Les textes de la bibliothèque (`Localization/Strings.resx`,
  anglais et français) et leur garde.
- La page ActionBar de la vitrine, avec le journal des commandes lancées.
- Les champs, comme ceux de mkm (`docs/composants/champs.md`) : `InputText` (un
  `TextBox`) et `Select` (un `ComboBox`, éditable, liste virtualisée), avec
  `Placeholder` et `Invalid`, au rendu de 3DEXPERIENCE ; les jetons de la feuille
  CSS de mkm, écrits par thème (`Theming/Tokens/<thème>.css.tokens`).
- La vitrine : la liste des composants à gauche, et la page Champs.
- Les boutons, comme ceux de mkm (`docs/composants/boutons.md`) : `Button` (huit
  gravités, quatre formes, icône, icône seule, bouton « lite ») et `Checkbox`
  (trois états, `Invalid`).
- Les conteneurs (`docs/composants/conteneurs.md`) : `Tabs` et `Tab` (le groupe
  de 3DEXPERIENCE et le style « strip » des facettes, `Variant`), `Fieldset`
  (légende sur le cadre, repliable : `Toggleable`, `IsCollapsed`).
- Les retours (`docs/composants/retours.md`) : `Message` (gravités, formes,
  tailles, croix, `Life`), `Toast` et `ToastMessage` (les notifications de
  3DEXPERIENCE, avec le relief, le fondu et la vie de mkm), `Dialog` (une vraie
  fenêtre modale au dessin de mkm, pied, poignée).
- La vitrine : les pages Boutons, Onglets et groupes, Messages et dialogues, et
  les notifications au-dessus de la fenêtre.
- Les tests communs : chaque contrôle applique son gabarit ; chaque ressource
  que nomme un gabarit existe dans chaque thème ; les contrastes des boutons,
  des onglets, des messages et des notifications, comme mkm les vérifie.

- Les captures de la vitrine : `Mkm.Wpf.Showcase --captures <dossier>` rend chaque
  page dans chaque thème en PNG, et des scènes (calendrier, menus, info-bulle,
  dialogues) ; les erreurs de liaison de WPF vont à son journal ; la CI les publie
  (artefact `captures`).
- Le lot 2, dix composants de plus :
  - l'attente et l'aide (`docs/composants/attente.md`) : l'info-bulle de mkm sur
    toute info-bulle de WPF (`Themes/Natifs.xaml`, `TooltipPlacement`),
    `ProgressBar`, `ProgressSpinner`, `BlockUI` ;
  - le texte, la date, la confirmation (`docs/composants/saisie.md`) :
    `Textarea`, `DatePicker` (et le calendrier de mkm), `ConfirmDialog` ;
  - la fiche de propriétés (`docs/composants/fiche-de-proprietes.md`) :
    `PropertyPanel`, `PropertySection`, `PropertyRow` ;
  - l'arbre et le menu (`docs/composants/arbre-et-menu.md`) : `Tree`, `TreeNode`,
    le menu contextuel de mkm sur tout `ContextMenu` de WPF (`MenuIcons`).
- Le lot 3 (`docs/composants/identite-et-choix.md`) : `IdCard` (la couleur de
  maturité donnée par l'application), `Tag`, `SelectButton`, `InputNumber` ; la
  page Identité et choix de la vitrine ; `InputText.CornerRadius`.
- `InternalIcons` : les 45 icônes que les composants de mkm dessinent eux-mêmes
  (clé `internal` du manifeste).
- La vitrine : les pages Attente et aide, Texte date et confirmation, Fiche de
  propriétés, Arbre et menu.

### Corrigé

- ActionBar : les commandes n'avaient ni icône ni libellé (un chemin de liaison
  `(local:…)` dont WPF ne trouvait pas le préfixe à l'exécution).
- InputText : le retrait était posé deux fois (le texte décalé et rogné).
- Tabs : le contenu d'un onglet prenait la couleur de son titre ; les titres des
  onglets « strip » étaient rognés.
- ProgressBar : ses bouts ronds (le rayon de 999 px de mkm) sont bornés comme en CSS.
- `color-mix(…)` mêle les couleurs comme CSS : pondérées de leur opacité, avec
  des parts facultatives ; une couleur mêlée au transparent ne fonce plus
  (l'ombre des onglets, celle des messages contrastés).
