# L'arborescence et le cycle de vie : TreeGrid et StateGraph

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`Controls/TreeGrid`, `Controls/StateGraph`).

## Le besoin

Montrer une structure (un assemblage, ses sous-ensembles, ses pièces, ses plans)
en colonnes, la déplier, la trier sans la défaire. Montrer où en est un objet dans
son cycle de vie et lancer la transition voulue d'un clic.

## Ce que fait mkm

- **TreeGrid** (preset `treegrid`, la DataGridView de 3DX, relevée sur le banc) :
  - **la grille :** un en-tête gris de 27 px, des lignes de 32 px alternées, la choisie bleu pâle ;
  - **l'arbre :** les colonnes de l'arbre à 19 px + 25 px par niveau ; des traits gris clair (ceux des
    ancêtres qui ont encore des frères, celui du nœud jusqu'à son milieu s'il est le dernier, un trait
    court vers la droite pour une feuille) ; la case de dépliage (15 px, + ou −) posée sur le trait du
    parent ; l'icône du nœud (22 px) sur la colonne suivante, d'où pend le trait des enfants ;
  - **le clavier :** le motif « treegrid » du W3C ;
  - **au-delà :** la bande des cases, le menu d'en-tête (épingler, cacher, ajuster), le déplacement et
    le redimensionnement des colonnes, la virtualisation, des cellules typées (étiquette, personne,
    image, état).
- **StateGraph** (preset `stategraph`, le cycle de vie de 3DX) :
  - **les états :** des boîtes de 90 × 40 px sur une ligne, 30 px entre deux, coins de 4 px, texte de 12 px ;
    l'état courant cerné de 2 px de noir, ceux à venir pâlis ; un état blanc cerné de gris ;
  - **les transitions :** d'un état au suivant, une flèche gris pâle ;
  - **les actions :** depuis l'état courant, des flèches gris foncé — droites vers un voisin, par-dessus
    vers un état plus loin, par-dessous pour un retour, chacune dans son couloir — et leur nom bleu qui
    se clique, précédé ou suivi d'un guillemet de la couleur de l'état visé ; au survol, la flèche
    s'épaissit et défile ;
  - **l'attente :** pendant une action, l'état visé attend sous un voile à trois barres.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `DataGrid` de WPF (déjà le `DataTable`) | colonnes, tri, sélection, virtualisation | l'arbre |
| `TreeListView` (exemples MSDN, divers projets MIT) | un `TreeView` en colonnes | l'apparence de 3DX ; ni le tri ni les colonnes du `DataGrid` |
| Syncfusion, Telerik, DevExpress `TreeGrid` | tout | propriétaires, payants |
| Graphes (MSAGL, MIT ; GraphSharp) | la disposition de graphes quelconques | une autre apparence ; le cycle de vie est une ligne simple |

## Le choix (technique)

- **`TreeGrid` hérite du `DataTable`** (donc du `DataGrid`) : en-tête, lignes,
  tri, sélection, virtualisation et colonnes de WPF sont gardés. Ses lignes sont
  les nœuds dépliés, aplatis dans sa propre collection ; les liaisons des colonnes
  restent celles des objets de l'application (`{Binding Nom}`). Une collection qui
  change (`INotifyCollectionChanged`) refait les lignes sans toucher à celles qui
  restent : la sélection est gardée.
- **La colonne de l'arbre** (`TreeGridColumn`) fabrique une `TreeGridCell` : les
  traits sont dessinés (`OnRender`) depuis la place du nœud (`TreeGridRowInfo`),
  la case et l'icône sont dans son gabarit.
- **Le tri** d'une colonne trie les frères à chaque niveau ; l'arbre reste.
- **`StateGraph` dessine en code** (un `Canvas`) la géométrie de mkm, portée telle
  quelle : tailles, couloirs, départs partagés sur le bord de l'état courant,
  places des noms. Les couleurs des états sont données par l'application (les
  maturités sont sa politique, comme pour `IdCard`) ; le reste vient du thème.

## L'API

- **TreeGrid :** `Nodes`, `ChildrenPath`, `GetRowInfo`, `IsExpanded`, `Expand`,
  `Collapse`, `Toggle`, `ExpandAll`, `CollapseAll`, et tout `DataTable` (sans
  `ItemsSource`) ; **TreeGridColumn** (`Binding`, `IconBinding`,
  `SortMemberPath`) ; **TreeGridRowInfo** (`Level`, `HasChildren`, `IsExpanded`,
  `IsLast`, `Guides`).
- **StateGraph :** `States` (`StateGraphState` : `Id`, `Label`, `Color`,
  `PaleColor`, `ToolTip`), `Transitions` (`StateGraphTransition` : `From`, `To`),
  `Actions` (`StateGraphAction` : `To`, `Label`), `Current`, `Pending`,
  `InvokeAction` ; l'événement `ActionClick`.

## Écarts avec mkm

- TreeGrid : pas encore la bande des cases (sélection à trois états), ni le menu
  d'en-tête (épingler, cacher, ajuster), ni les cellules typées ; le
  redimensionnement et le déplacement des colonnes sont ceux du `DataGrid`. Les
  couleurs de la grille sont celles du `DataTable`.
- StateGraph : le guillemet est plein, de la couleur de l'état visé (mkm en
  dessine le contour).

## La vitrine et les tests

La page « Arborescence et cycle de vie » montre la structure d'un produit (deux
niveaux dépliés, triable) et le cycle de vie d'un plan en cours. Les tests
vérifient le dépliage et le repli, les niveaux, les traits et le dernier frère,
le tri qui garde l'arbre, l'enfant ajouté et la sélection gardée, la cellule de
l'arbre (retrait, case d'une feuille), les états et les actions du graphe, sa
taille, l'action cliquée et celle refusée pendant une autre.
