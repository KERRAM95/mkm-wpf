# La mise en page : Triptych, WelcomePanel, Splitter et EmptyContent

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`Controls/Triptych`, `Controls/WelcomePanel`,
`Controls/Splitter`, `Controls/EmptyContent`).

## Le besoin

Donner à une application complète (hscart-next) la mise en page des applications
3DEXPERIENCE : un panneau d'accueil à gauche pour naviguer, le travail au milieu,
les propriétés à droite. Partager la place entre deux vues. Dire ce qu'il faut
faire quand un panneau n'a rien à montrer.

## Ce que fait mkm

- **Triptych** (preset `triptych` et sa feuille, relevés sur Product Structure
  Editor et Document Management) :
  - **les panneaux :** gauche (300 px), principal, droite (300 px, 250 au moins) ;
    ceux de côté s'ouvrent et se ferment en glissant (0,4 s) et poussent le principal ;
  - **la bande :** le panneau de gauche se replie en une bande de 40 px ; une languette
    (25 × 45 px, en haut à gauche du principal) l'ouvre, le replie et le déplie ;
  - **les poignées :** une bande invisible de 10 px sur le bord d'un panneau ouvert ;
    au survol, pendant qu'on la tire ou au clavier, une pilule grise de 4 × 40 px ;
    les flèches par pas de 10 px, Début et Fin jusqu'aux bornes ;
    le principal garde toujours 100 px (150 entre deux panneaux) ;
  - **la superposition :** sous 550 px, un panneau de côté (90 % de la largeur)
    recouvre le principal, un voile dessous ; un seul à la fois ; le voile et Échap le ferment ;
    le panneau de droite que la place a fermé revient avec elle ;
  - **le modal :** un panneau de droite modal voile le reste.
- **WelcomePanel** (preset `welcomepanel` et sa feuille) : le panneau d'accueil
  de 3DX, sur un fond `#f9f9f9`.
  - **l'en-tête :** un titre de 22 px et une description de 13 px, gras, gris ;
  - **les rubriques :** leur libellé de 13 px gras ;
  - **les entrées :** 13 px grasses, grises, une icône de 18 px ;
    la choisie bleue, marquée d'un trait de 2 px à gauche ; des entrées d'action qu'on ne choisit pas ;
  - **le repli :** les icônes seules (24 px), leur libellé en info-bulle ;
  - **le clavier :** un seul arrêt de tabulation (l'entrée choisie), les flèches, Début et Fin.
- **Splitter** (styles de PrimeVue, jetons de Nora et de mkm) :
  - **les panneaux :** en pourcentages, avec une taille minimale ;
  - **les gouttières :** gris clair, 4 px ; une pilule grise au survol, pendant qu'on tire ou au clavier ;
    le clavier par pas de 5 px.
- **EmptyContent** (preset `emptycontent`) : l'écran vide.
  - **le bloc :** centré, 420 px au plus ; une icône grise de 32 px, un titre de 16 px semi-gras, un message gris ;
  - **les possibilités :** chacune son icône dans une pastille ronde de 48 px de la couleur principale pâlie ;
    entre deux, « ou » entre deux traits courts ;
  - **les actions :** dessous ; la taille `small` pour un panneau étroit.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `GridSplitter` de WPF | tirer une frontière de `Grid` | les pourcentages, les tailles minimales relatives, l'apparence |
| AvalonDock (Ms-PL), Dirkster AvalonDock | des panneaux qu'on ancre et détache | bien plus que la mise en page de 3DX, une autre apparence, une dépendance lourde |
| MahApps `SplitView`, `HamburgerMenu` (MIT) | un volet qui recouvre ou pousse le contenu | un seul volet, une autre apparence |

## Le choix (technique)

- **`Triptych` est un `ContentControl`** (le contenu est le panneau principal ;
  `Left` et `Right`, les panneaux de côté). Sa géométrie (largeurs, place du
  principal, poignées, voile) est posée en code, comme le fait mkm par ses
  styles en ligne ; elle glisse par des animations WPF (`CubicEase`), sauf
  pendant qu'on tire une poignée et quand Windows coupe les animations. Un
  panneau fermé, ou le principal sous un voile, ne prend ni la souris ni la
  tabulation ; en superposition, la tabulation tourne dans le panneau ouvert.
- **`WelcomePanel` est un `ItemsControl`** de rubriques
  (`WelcomePanelSection`, un `HeaderedItemsControl`) et de traits ; ses entrées
  (`WelcomePanelItem`) sont des `ButtonBase` : `Command` et clic. La sélection
  est une clé (`Selection`), comme dans mkm.
- **`Splitter` est un `Control`** dont les panneaux (`Panels`, ses enfants
  logiques) sont placés par sa disposition (`SplitterLayout`) avec une
  gouttière (`SplitterGutter`, un `Thumb`) entre deux.
- **`EmptyContent` est un `ItemsControl`** de possibilités
  (`EmptyContentItem`), annoncées aux lecteurs d'écran (`LiveSetting`) ; les
  actions ne le sont pas.
- **Deux briques pour toute la bibliothèque :** `GapStackPanel`, la pile avec
  écart (le `gap` de CSS), et `Pill`, un cadre aux bouts ronds à toute taille
  (le rayon de 999 px de mkm, que WPF ne borne pas).
- **ARA prend les mesures de Dashboard**, comme le veut mkm (seules les
  couleurs changent) : ce que la feuille de Dashboard ajoute (la bordure du
  panneau de gauche sous sa languette seulement, les lignes de 1,43 fois le
  texte du panneau d'accueil) vaut aussi pour lui, bien que le preset `ara` de
  mkm n'ait pas cette feuille.

## L'API

- **Triptych :** `Left`, `Right`, `LeftVisible`, `RightVisible`,
  `LeftCollapsed`, `LeftSize`, `LeftCollapsedSize`, `LeftMinSize`, `RightSize`,
  `RightMinSize`, `LeftResizable`, `RightResizable`, `Step`,
  `ShowLeftCloseIcon`, `ShowRightCloseIcon`, `ShowLeftToggle`, `Breakpoint`,
  `OverlaySize`, `Dismissable`, `CloseOnEscape`, `Modal`, `LeftAutomationName`,
  `RightAutomationName`, `IsOverlay`,
  `IsLeftExpanded`, `LeftPixels`, `RightPixels` ; `Show`, `Hide`, `Toggle`
  (`TriptychSide`), `GetMaxSize` ; les événements `ModeChange`, `ResizeEnd`.
- **WelcomePanel :** `Title`, `Description`, `Header`, `Footer`, `Selection`,
  `IsCollapsed`, `Toggle`, `GetItems` ; l'événement `ItemClick` ;
  **WelcomePanelSection** (`Header`) ; **WelcomePanelItem** (`Key`, `Icon`,
  `Selectable`, `IsSelected`) ; **WelcomePanelSeparator**.
- **Splitter :** `Panels`, `Orientation`, `GutterSize`, `Step`, `GetSizes` ;
  l'événement `ResizeEnd` ; **SplitterPanel** (`Size`, `MinSize`).
- **EmptyContent :** `Icon`, `Title`, `Message`, `Actions`, `Size`
  (`EmptyContentSize`) ; **EmptyContentItem** (`Icon`, le contenu).

## Écarts avec mkm

- Triptych : pas de mémoire d'état (`stateKey`) ni de `beforeHide` ; l'info-bulle
  des poignées n'est pas reprise ; un panneau ouvert coupe ce qui dépasse de lui.
- WelcomePanel : l'info-bulle d'une entrée repliée est celle de la bibliothèque,
  non l'info-bulle claire des panneaux de 3DX ; pas de variante sombre
  (`darkVariant`).
- Splitter : pas de mémoire d'état ; `ResizeEnd` ne porte pas les tailles
  (`GetSizes` les donne).
- EmptyContent : les actions sont un seul contenu (un `StackPanel` de boutons).

## La vitrine et les tests

La page « Mise en page » montre un Triptych complet (le panneau d'accueil et sa
languette, un séparateur et un écran vide au milieu, des propriétés à droite),
un écran vide seul, deux séparateurs, le panneau d'accueil déplié et replié ; les
captures montrent aussi la page étroite, en superposition. Les tests vérifient
l'écart de la pile, le « ou » des possibilités et la taille héritée, les parts et
les gouttières d'un séparateur, ses tailles minimales, le sens vertical, la
sélection et l'arrêt de tabulation du panneau d'accueil, l'entrée d'action, le
repli, les panneaux qui poussent le principal, la poignée qui lui garde sa
place, la superposition et le retour du panneau de droite, la languette.
