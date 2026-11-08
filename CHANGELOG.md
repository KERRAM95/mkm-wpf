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
- Le lot 10 (`docs/composants/bases-et-defilement.md`) : les barres de
  défilement de mkm pour toute l'application (9 px, sans flèches ni gouttière),
  `ScrollPanel` (ses barres posées sur le contenu, au survol), `Divider`,
  `ToggleSwitch`, `Skeleton` ; leurs jetons importés de mkm, leurs pages de
  documentation, leurs tests et leurs scénarios d'interaction.
- Le lot 11 (`docs/composants/pastilles-et-personnes.md`) : `Badge` et
  `OverlayBadge`, `Avatar` et `AvatarGroup`, `Chip` (retirable, au clavier
  aussi) ; leurs jetons, leurs pages, leurs tests et leurs scénarios.
- Le lot 12 (`docs/composants/panneaux-et-accordeon.md`) : `Panel` (repliable,
  ses actions et son pied), `Card`, `Accordion` et `AccordionPanel` (une seule
  section ouverte ou plusieurs, ↑ ↓ Début Fin) ; leurs jetons, leurs pages,
  leurs tests et leurs scénarios.
- Le lot 13 (`docs/composants/chemin-bouton-partage-et-popover.md`) :
  `Breadcrumb` et `BreadcrumbItem`, `SplitButton` (son menu au clavier aussi),
  `Popover` (dessous ou dessus, sa flèche vers son élément, Échap) ; `Button`
  gagne `CornerRadius` ; leurs jetons, leurs pages, leurs tests et leurs
  scénarios.
- Le lot 14 (`docs/composants/listes-et-champs-composes.md`) : `Listbox`
  (coche, lignes alternées, filtre), `MultiSelect` (virgules ou puces, « tout
  sélectionner », filtre), `AutoComplete` (suggestions demandées à
  l'application, bouton accolé), `IconField`, `InputGroup` et
  `InputGroupAddon` ; `Chip` gagne `CornerRadius` ; leurs jetons, leurs pages,
  leurs tests et leurs scénarios. La priorité 1 est faite.
- Le lot 15 (`docs/composants/etapes-frise-et-pagination.md`) : `Stepper` et
  `StepperPanel` (en ligne ou en colonne, linéaire), `Timeline` et
  `TimelineEvent` (verticale ou horizontale, alternée, rond personnalisé),
  `Paginator` (flèches, liens de page, lignes par page, « 3 sur 12 ») ; leurs
  jetons, leurs pages, leurs tests et leurs scénarios.
- Le lot 16 (`docs/composants/tiroir-sur-place-et-images.md`) : `Drawer`
  (tiroir sur un voile, de chaque bord ou plein écran, pied, le clavier entre
  et revient), `Inplace` (modifier sur place), `Image` (aperçu en grand :
  tourner, agrandir, réduire), `ImageCompare` (deux images à une poignée) ;
  leurs jetons, leurs pages, leurs tests et leurs scénarios.
- Le lot 17 (`docs/composants/listes-en-arbre-et-en-cascade.md`) :
  `TreeSelect` (un arbre sous un champ, croix qui retire le choix) et
  `CascadeSelect` avec `CascadeSelectItem` (des listes qui s'ouvrent l'une à
  droite de l'autre) ; liés à des données ou écrits dans le XAML ; leurs
  jetons, leurs pages, leurs tests et leurs scénarios. La priorité 2 est faite.
- Le lot 18 (`docs/composants/note-curseur-et-bouton-rotatif.md`) : `Rating`
  (des étoiles, un clic sur la note la retire), `Slider` (le curseur de WPF
  habillé comme mkm), `Knob` (un arc qu'on tourne, la valeur au milieu) ;
  `Icon.Filled` (une icône pleine) ; leurs jetons, leurs pages, leurs tests et
  leurs scénarios.
- Le lot 19 (`docs/composants/saisies-protegees-et-filtrees.md`) : `Password`
  (masqué, l'œil, la jauge de force), `InputMask` (un champ qui suit un
  modèle), `InputOtp` (une case par caractère), `KeyFilter` (le filtre de
  touches, propriété attachée) ; leurs jetons, leurs pages, leurs tests et
  leurs scénarios.
- Image : les boutons de l'aperçu prennent enfin leur style (un type à eux,
  au lieu d'une clé que l'application ne voyait pas).
- Le lot 20 (`docs/composants/puces-libelles-et-couleur.md`) : `InputChips`
  (des valeurs en puces), `FloatLabel` (le libellé qui s'envole : au-dessus,
  dedans, sur la bordure) et `IftaLabel` (toujours dans le champ),
  `ColorPicker` (nuances et teintes, en place ou dans un panneau) ; leurs
  jetons, leurs pages, leurs tests et leurs scénarios. L'import des jetons
  suit aussi les renvois écrits en camelCase.
- Le lot 21 (`docs/composants/barres-et-menus-de-navigation.md`) : `Menubar`
  (la barre de menus, sur le Menu de WPF), `MegaMenu` avec `MegaMenuItem` et
  `MegaMenuColumn` (des panneaux en colonnes de liens), `PanelMenu` et
  `PanelMenuItem` (des panneaux qui se déplient en arbre) ; leurs jetons,
  leurs pages, leurs tests et leurs scénarios.
- Le lot 22 (`docs/composants/groupes-dock-et-actions.md`) : `ButtonGroup`
  (des boutons collés, `ButtonGroup.Position`), `Dock` et `DockItem` (un
  lanceur d'applications translucide), `SpeedDial` et `SpeedDialItem` (des
  actions en ligne ou en arc autour d'un bouton rond) ; leurs jetons, leurs
  pages, leurs tests et leurs scénarios. `Dock` étant aussi une énumération
  de WPF, le code de mkm-wpf l'écrit en entier.
- Le lot 23 (`docs/composants/listes-ordonnees-et-vues.md`) : `OrderList` (une
  liste qu'on ordonne en place), `PickList` (deux listes, des éléments qui
  passent de l'une à l'autre), `DataView` (des éléments par un gabarit, en
  liste ou en grille, page par page) ; leurs jetons, leurs pages, leurs tests
  et leurs scénarios.
- Jetons écrits à la main : les marges et retraits sont dans l'ordre de CSS
  (haut, droite, bas, gauche) ; plusieurs, depuis le lot 18, étaient dans
  celui de WPF (FloatLabel, Slider, CascadeSelect, MegaMenu, PanelMenu,
  OrderList, PickList).
- Le lot 24 (`docs/composants/organigramme-et-jauges.md`) :
  `OrganizationChart` et `OrganizationChartNode` (un organigramme, ses traits,
  ses nœuds qui se replient et se choisissent), `MeterGroup` et `MeterItem`
  (des parts dans une barre, et leur légende) ; leurs jetons, leurs pages,
  leurs tests et leurs scénarios.
- Le lot 25 (`docs/composants/carrousel-galerie-et-terminal.md`) : `Carousel`
  (des éléments page par page, flèches, indicateurs, lecture automatique),
  `Galleria` (une image en grand et ses vignettes), `Terminal` (des commandes
  et leurs réponses) ; leurs jetons, leurs pages, leurs tests et leurs
  scénarios. La priorité 3 est faite.
- PickList : un double clic rend la souris avant de faire passer l'élément
  (le clic suivant se perdait).
- Contrôle qualité, comparaison à ui-kit composant par composant :
  - Inplace : le texte suit la couleur de la page (il était noir, du style de
    Windows) ; un contenu sans rien de focalisable garde le clavier, pour
    qu'Échap rende le texte.
  - OverlayBadge : le contour de 2 px est au-dehors de la pastille, comme
    mkm (la pastille faisait 14 px au lieu de 18, le point 2 au lieu de 6).
  - EmptyContent : une vraie région vivante (titre et message annoncés quand
    ils changent) ; l'exemple espace ses actions de 6 px, comme mkm.
  - Terminal : chaque réponse est annoncée aux lecteurs d'écran ; la police
    du thème, comme mkm.
  - Button : l'anneau du clavier de mkm (1 px, à 1 px au-dehors, de la
    couleur de la gravité ; le focus d'un lien était invisible) ; un lien se
    souligne au survol.
  - Message : la croix prend la couleur du message (elle était noire) ;
    remontré, il est réannoncé et sa vie repart.
  - ButtonGroup : un bouton caché ou montré replace les autres (coins et
    bordures) ; un groupe lié par ItemsSource est rangé lui aussi.
  - AutoComplete : Alt+↓ demande enfin toutes les suggestions (WPF donne
    Key.System avec Alt) ; Alt+↑ choisit et referme ; ↑ sans suggestion
    surlignée va à la dernière ; retaper défait le choix ; une liste de
    suggestions remplie sur place est suivie.
  - Checkbox : indéterminée, elle n'est pas cochée (un trait de la couleur
    du texte sur la case grise, comme mkm) ; l'anneau du clavier de mkm.
  - FloatLabel et IftaLabel : le libellé se place par rapport au champ, même
    dans une rangée plus haute ; il suit le champ d'un IconField (et se
    décale après son icône) et d'un DatePicker ; une hauteur ou un retrait
    posés par l'application ne sont plus effacés.
  - Menu, TieredMenu : leurs traits ont le style de mkm (ils gardaient le
    gris de WPF, avec 2 px de marge) ; le menu n'est qu'un arrêt de la
    touche Tab, sans cadre pointillé ; idem pour la colonne d'un MegaMenu et
    la barre d'un Menubar.
  - Tous les menus : Espace ouvre le sous-menu ou choisit l'élément, comme
    Entrée (mkm) ; un élément qui se coche n'a que sa coche, sans icône ;
    le menu contextuel fait 90 px au moins, ombre non comprise.
  - PanelMenu : les éléments d'un panneau gardent leur retrait de 12 px.
  - Knob : le pas par défaut est 1 (il était de 0,1) ; Slider : une valeur
    entière à la souris aussi.
  - KeyFilter : l'Espace passe enfin au filtre (WPF l'écrit sans
    PreviewTextInput).
  - SelectButton : l'anneau du focus ne paraît qu'au clavier (il paraissait
    après un clic) ; grisé, le choix paraît au repos, comme mkm ; une option
    grisée seule est pâlie ; refusé, le trait rouge est au-dehors.
  - ToggleButton : grisé et enfoncé, il paraît au repos ; ToggleSwitch :
    grisé et activé, il garde sa bordure.
  - Select, MultiSelect, TreeSelect : grisés, leur texte (et le texte
    d'aide) prend la couleur grisée de mkm.
  - MultiSelect : ↑ ouvre la liste ; la case de tête n'est cochée que si
    toutes les options montrées sont choisies (elle l'était à tort avec un
    filtre). Rating : l'anneau du clavier à 1 px de l'étoile.
  - Paginator : moins de lignes qu'avant, la dernière page (First restait
    au-delà) ; un autre nombre de lignes n'annonce la page qu'une fois ; la
    page montrée le dit aux lecteurs d'écran ; la liste des lignes par page
    n'est plus élargie.
  - DataView : une liste qui rétrécit ramène à la dernière page (la vue
    paraissait vide) ; abonnement faible à la liste.
  - OrderList, PickList : un bouton qui n'aurait rien à faire passer est
    grisé (sans choix, ou une liste vide), comme mkm ; boutons à droite, la
    touche Tab passe par la liste d'abord.
  - OrganizationChart : le chevron dans le sens de mkm (vers le bas,
    déplié) ; un choix posé avant la création des nœuds les marque ; un
    nœud à l'en-tête composé se nomme par ses textes.
  - Timeline : la documentation de TimelineAlign Top et Bottom remise à
    l'endroit.
  - Carousel, Galleria : les indicateurs ne sont plus recréés à chaque page
    (le clavier quittait le carrousel) ; la page montrée le dit aux lecteurs
    d'écran ; le carrousel glisse vraiment d'une page à l'autre ; ses
    indicateurs sont un seul arrêt de Tab, parcourus aux flèches.
  - InputOtp, ColorPicker, Terminal, Password : la langue n'est suivie que
    tant que le contrôle est chargé (l'abonnement à vie le gardait en
    mémoire).
- Avatar et Chip : leur image est peinte par le code ; liée dans le gabarit,
  elle faisait écrire à WPF une erreur de liaison (sans effet visible).
- Galleria : une vignette pâlie s'éclaire sous la souris, comme mkm.
- OrganizationChart : le bouton qui replie un nœud passe au-dessus du trait
  qui descend vers les enfants (le trait le barrait et prenait son clic).
- PickList : ses deux listes ont toujours la même largeur, même quand elle
  prend sa taille naturelle (la grille en étoile les laissait à leur taille).
- Info-bulle et Popover : la page qui défile les referme, comme mkm.
- Le zoom de toute l'interface (`docs/fondations/zoom.md`) :
  `MkmResources.Zoom` (80 à 200 %), `ZoomIn`, `ZoomOut`, `ResetZoom`,
  `ZoomChanged` ; chaque fenêtre, ses dialogues et ses fenêtres flottantes
  suivent, nets ; Ctrl +, Ctrl −, Ctrl + 0, Ctrl+molette et une bulle annoncée ;
  `WindowsTextScale` pour partir du réglage de Windows. La vitrine a son choix
  du zoom, une page Zoom, et déclare l'échelle de chaque écran (PerMonitorV2).
- Corrigé : la légende repliable du `Fieldset` héritait du ToggleButton de mkm
  (12 px, non grasse, hauteur coupée) ; un test de convention l'empêche
  désormais.
- La documentation, sur le modèle d'ui-kit : la vitrine devient une page par
  composant, rangée par catégorie (Pour commencer, Formulaire, Bouton, Données,
  Panneau, Superposition, Menu, Messages, Divers), avec une recherche. Chaque
  page a trois onglets : Fonctionnalités (une section par usage, l'exemple
  vivant et le XAML qui le produit, coloré et à copier, le code VB de son
  comportement ; l'accessibilité), API (propriétés, valeurs par défaut,
  évènements, méthodes, tirés du code et de ses commentaires) et Thème (les
  jetons du composant dans chaque thème). Les pages générales : Introduction,
  Installation, Thème, Langue, Icônes (la galerie de toutes les icônes). Les
  pages denses de la vitrine, plusieurs composants chacune, disparaissent.
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
- Select, DatePicker, la liste de l'ActionBar : ouverts, ils se referment quand
  la page défile, comme dans mkm ; la liste restait à sa place sur l'écran,
  détachée de son champ (vu par Mourad, à la molette).
- TieredMenu : un élément choisi dans un étage referme les étages ouverts ; ils
  restaient ouverts (trouvé par les tests d'interaction).
- ActionBar : repliée dès le départ, son bouton de repli n'avait pas de nom.
- Fieldset : sa légende repliable se dit « développé » ou « réduit »
  (ExpandCollapse), comme `aria-expanded` dans mkm ; elle se disait « activée »
  quand le groupe était replié.
- DataTable : pendant un chargement, il se dit « Chargement en cours » (son
  état) ; le voile, dans son gabarit, échappait aux lecteurs d'écran.
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
