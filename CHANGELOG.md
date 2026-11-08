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
- `DataTable` (`docs/composants/tableau.md`) : la grille de 3DEXPERIENCE sur le
  `DataGrid` de WPF, zébrée, quadrillée, trois tailles, message vide, voile de
  chargement, en-tête et pied, la case de mkm dans les colonnes de cases ; la page
  Tableau de données de la vitrine.
- Le lot 5 (`docs/composants/choix-et-barre.md`) : `RadioButton`, `ToggleButton`
  (libellés et icônes de son état), `Toolbar` (ses zones, ses commandes
  `ToolbarButton` et `ToolbarSeparator`, le débordement dans le menu « … ») ; la
  page Radios, bascules, barre de la vitrine.
- Le lot 6 (`docs/composants/mise-en-page.md`) : `Triptych` (trois panneaux,
  le glissement, la bande, la languette, les poignées, la superposition),
  `WelcomePanel` (le panneau d'accueil de 3DEXPERIENCE), `Splitter`,
  `EmptyContent` ; `GapStackPanel` et `Pill` ; la page Mise en page de la
  vitrine.
- Le lot 7 (`docs/composants/menus.md`) : `Menu` (et ses groupes, `MenuGroup`)
  et `TieredMenu`, posés dans la page ; sur la page Arbre et menu de la vitrine.
- Le lot 8 (`docs/composants/arborescence-et-cycle-de-vie.md`) : `TreeGrid` (la
  DataGridView de 3DEXPERIENCE sur le `DataTable`, sa colonne d'arbre) et
  `StateGraph` (le graphe du cycle de vie) ; la page Arborescence et cycle de vie
  de la vitrine.
- Le registre des commandes (`docs/fondations/commandes.md`) : `CommandRegistry`,
  `CommandDefinition`, `RegisteredCommand`, `CommandSubmenu` ; les menus, les
  `Toolbar` et les sections de l'`ActionBar` fabriqués d'une disposition ; les
  raccourcis.
- L'importeur de jetons fond le schéma clair d'un composant dans ses valeurs
  communes, comme mkm.
- Les thèmes : une largeur de bordure côté par côté (`0 1px 1px 0`) devient
  une épaisseur.
- `InternalIcons` : les 45 icônes que les composants de mkm dessinent eux-mêmes
  (clé `internal` du manifeste).
- La vitrine : les pages Attente et aide, Texte date et confirmation, Fiche de
  propriétés, Arbre et menu.
- Les tests d'interaction `Mkm.Wpf.UiTests` (FlaUI) : la vitrine lancée en mode
  `--tests`, puis pilotée à la souris et au clavier comme par un utilisateur ;
  chaque page s'ouvre sans erreur, chaque commande a un nom pour les lecteurs
  d'écran, et des scénarios par composant. Une capture et l'arbre
  d'automatisation de chaque échec (`docs/fondations/tests-d-interaction.md`).

### Corrigé

- L'accessibilité, relevée en écrivant les tests d'interaction :
  - les contrôles qui contiennent des commandes (ActionBar et ses sections et
    listes, Toolbar, Toast, EmptyContent, PropertyPanel, WelcomePanel, Menu,
    MenuGroup, TieredMenu) cachaient leur gabarit aux lecteurs d'écran : les
    commandes de l'ActionBar, le bouton « … » de la Toolbar, les actions d'un
    écran vide n'y paraissaient pas ;
  - une section de l'ActionBar se dit onglet, choisi ou non ;
  - un Button ou un ToggleButton d'icône seule prend le nom de son info-bulle ;
    un ToggleButton, celui de son libellé d'état (OnLabel, OffLabel) ;
  - le bouton plier/déplier d'un nœud de la TreeGrid a un nom ;
  - les actions du StateGraph sont des boutons, qu'un lecteur d'écran lance ;
  - une entrée du WelcomePanel se dit choisie ou non ;
  - Message, ToastMessage, ProgressSpinner, IdCard, PropertyRow, StateGraph,
    BlockUI, Splitter et Triptych n'avaient pas de pair d'automatisation :
    invisibles aux lecteurs d'écran, et l'annonce d'un message ou d'une
    notification (LiveSetting) ne partait pas ;
  - InputNumber est une toupie (Spinner) qui dit sa valeur et ses bornes ;
  - un InputText, un Textarea ou un Select sans libellé prend le nom de son
    texte indicatif, comme sur le web ;
  - les boutons plier/déplier du Tree et les cases d'une colonne du DataTable
    ont un nom.
- RadioButton : les flèches choisissent le bouton voisin du groupe, comme mkm.
- La relecture des lots 1 à 4 :
  - ActionBar : les commandes d'une liste ouverte n'avaient pas la forme de lignes
    de menu ; Début et Fin faisaient défiler la rangée sans déplacer le focus ;
    une section cachée restait montrée ; un repli réécrivait la section voulue ;
    la tabulation passait par le bouton de repli avant les onglets ;
  - Button : les couleurs du texte au survol et enfoncé, et tout le gris du bouton
    « lite », étaient sans effet ; une valeur posée par l'application (couleur,
    retrait) est désormais gardée, comme pour Message et Toast (`Habillage`) ;
  - Checkbox, Tree, PropertyPanel : l'anneau du focus ne paraît qu'au clavier
    (`FocusVisible`) ; le titre d'une section fermée de la fiche était noir ;
  - ConfirmDialog : `Confirm` sans titre levait une exception ; une longue question
    ne passait pas à la ligne ;
  - MenuIcons : la colonne d'icône d'un sous-menu ne suivait pas ses éléments ;
  - BlockUI : le focus ne quittait pas le contenu voilé, et le voile avalait toutes
    les touches (Tab, Alt+F4) ;
  - info-bulle : le décalage était compté deux fois ; Toast : une notification fermée
    sous le pointeur revivait ; Fieldset : replié sans bascule, le contenu restait ;
  - InputNumber : le point du pavé numérique en français, le curseur gardé après une
    flèche, l'arrondi comme l'affichage ; SelectButton : `AllowEmpty` tenu en choix
    multiple et au Ctrl+clic ; IdCard : une chaîne vide ne s'affiche plus ; Tree :
    les enfants d'un nœud sont virtualisés ;
  - ToggleButton : arriver au clavier sur lui faisait tomber l'application (un jeton
    « …radius », lu comme un CornerRadius, sur le RadiusX de son anneau) ; un test
    vérifie désormais le type de chaque ressource des gabarits ;
  - langue : les textes enrichis (`Run`, `Span`, `FlowDocument`) suivent le format
    de Windows ; `InputNumber` suit ses réglages personnalisés ;
  - Tag : le texte lisible est celui de mkm (blanc ou #111111, sa luminance) ;
    Icon : sans taille, 16 px même étirée ; DatePicker : le survol du jour
    d'aujourd'hui, et pas des jours bloqués ; l'importeur d'icônes ne réécrit que
    sa section de `THIRD-PARTY-NOTICES.md`.

- Le contrôle qualité des lots 5 à 9 :
  - `Menu` et `TieredMenu` : le survol prenait le focus (un champ le perdait) et
    le `Menu` capturait la souris en mode menu ;
  - `TreeGrid` : Suppr retirait une ligne de la vue ; replier un parent faisait
    oublier le dépliage de ses descendants ;
  - `Triptych` : un panneau ouvert par liaison en superposition laissait l'autre
    ouvert ; fermé, il gardait le focus au lieu de le rendre à qui l'avait ouvert ;
  - `TreeGrid` et `StateGraph` : leurs abonnements aux collections du modèle sont
    faibles (un modèle qui survit à sa vue ne la retient plus) ;
  - `Splitter` : le pas du clavier est en pixels, comme mkm ;
  - `StateGraph` : les noms d'action mesurés dans leur police, la flèche survolée
    remise à zéro quand une action commence ou finit, le focus gardé au redessin ;
  - `WelcomePanel` : un `Header` donné remplace bien le titre et la description.

- ActionBar : la liste d'une commande ne trouvait plus sa flèche depuis l'arrivée
  du `ToggleButton` de mkm (le nom cachait celui de WPF) ; `ActionBarToggleButton`
  hérite de nouveau du `ToggleButton` de WPF.

- ActionBar : les commandes n'avaient ni icône ni libellé (un chemin de liaison
  `(local:…)` dont WPF ne trouvait pas le préfixe à l'exécution).
- ActionBar : revenue d'une section large à une section étroite, la barre gardait
  la largeur de la large ; elle suit désormais la section montrée.
- DataTable : la case d'une colonne de cases, en lecture, levait une exception (le
  style visait la `Checkbox` de mkm et non la `CheckBox` de WPF).
- InputText : le retrait était posé deux fois (le texte décalé et rogné).
- Tabs : le contenu d'un onglet prenait la couleur de son titre ; les titres des
  onglets « strip » étaient rognés.
- ProgressBar : ses bouts ronds (le rayon de 999 px de mkm) sont bornés comme en CSS.
- `color-mix(…)` mêle les couleurs comme CSS : pondérées de leur opacité, avec
  des parts facultatives ; une couleur mêlée au transparent ne fonce plus
  (l'ombre des onglets, celle des messages contrastés).
