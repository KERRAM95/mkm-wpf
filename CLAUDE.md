Contexte de travail sur le dépôt `mkm-wpf`.

## Règles Git

- **Un seul développeur pour l'instant : on travaille directement sur
  `main`.** Pas de branche de fonctionnalité ; ne pas en créer de sa propre
  initiative.
- **Ne jamais réécrire l'historique de `main`** — pas de `--force`.
- L'identité et l'attribution des commits sont fixées dans
  `.claude/settings.json` — `attribution.commit`, `pr` et `sessionUrl` sont à
  `false`. Ne pas les modifier, et **ne pas les contourner** : un pied de page
  d'attribution écrit à la main dans le corps d'un message de commit passe
  outre le réglage, et la forge affiche alors un second contributeur. Le hook
  `outils/hooks/commit-msg` retire ces lignes ; il ne dispense pas de ne pas
  les écrire. C'est la règle de tous les dépôts de Mourad.
- Une fois par clone : `git config core.hooksPath outils/hooks`.
- Les messages de commit sont en français : une première ligne qui dit ce que
  le commit apporte, puis le pourquoi.

## Qui décide

**Mourad n'est pas développeur VB.NET** (le 07/11/2026) : les décisions
techniques — VB, WPF, MSBuild, l'organisation du code — se prennent ici, avec
leur raison en une phrase dans la fiche ou le commit. On ne lui soumet que ce
qui touche au produit : ce que l'utilisateur voit et fait, l'apparence, le
périmètre.

**Ce que mkm (`ui-kit`) a déjà décidé ne se redemande pas** (Mourad, le
07/11/2026) : son comportement, son apparence, ses thèmes, ses icônes se
reprennent tels quels. Une fiche le constate, sans le soumettre ; une
question ne porte que sur ce que mkm ne tranche pas.

## Ce dépôt

**mkm-wpf**, la bibliothèque de composants WPF de Mourad : la déclinaison
bureau de **mkm** (`ui-kit`, le fork de PrimeVue). Elle grandit au fil des
besoins — composants, thèmes, icônes, commandes — avec une vitrine qui les
montre tous.

**Elle est neutre** : rien n'y vient d'une application cliente. Aucune règle
métier, aucun nom de client ni d'application qui l'emploie, aucune dépendance
à un logiciel tiers (CAO, PLM…). Un thème de charte comme ARA est un thème
parmi d'autres. Un besoin d'application devient ici un composant générique,
ou reste dans l'application.

| Dossier                 | Projet             | Rôle                                                                     |
| ----------------------- | ------------------ | ------------------------------------------------------------------------ |
| `src/Mkm.Wpf`           | `Mkm.Wpf`          | les contrôles et leurs styles par défaut (`Themes/Generic.xaml`), la configuration (`MkmResources`) et la langue (`Localization`) |
| `apps/Mkm.Wpf.Showcase` | `Mkm.Wpf.Showcase` | la vitrine : une page par composant                                      |
| `tests/Mkm.Wpf.Tests`   | `Mkm.Wpf.Tests`    | les tests (NUnit), dont les règles communes à tous les contrôles         |
| `docs/fondations`       | aucun              | ce dont tous les composants dépendent : thème et langue                  |
| `docs/composants`       | aucun              | une fiche par composant : l'état de l'art et le choix, avant le code     |
| `outils`                | aucun              | le hook Git, la compilation sous Linux                                   |

D'autres projets viendront avec leur premier contenu, pas avant : les thèmes
et leurs jetons, les icônes, les commandes. Leur découpage se décide avec le
premier d'entre eux, après l'étude de l'open source (plus bas).

## Cible et langage

- **.NET Framework 4.8**, celui des postes des clients : rien de nouveau à
  déployer. Toute dépendance doit tourner sur net48 (cible `net48`, `net462`
  ou plus, ou `netstandard2.0`).
- **VB.NET**, projets au format SDK (`Microsoft.NET.Sdk`, `UseWPF`).
  `Directory.Build.props` porte les propriétés communes, `Directory.Packages.props`
  les versions des paquets (gestion centralisée : un `PackageReference` n'a
  jamais de `Version`).
- `Option Strict On`, `Explicit On`, `Infer On` ; les avertissements sont des
  erreurs.
- L'outil de référence est Visual Studio 2022 sous Windows.

## Thème et langue

**Une application choisit son thème et sa langue par une variable chacun, et
les change pendant qu'elle tourne**, comme avec mkm. La langue est une exigence
au même titre que le thème (Mourad, le 07/11/2026) : tout composant est
traduisible. Le pourquoi est dans `docs/fondations/theme-et-langue.md`
(validée le 07/11/2026) ; le thème a sa fiche, `docs/fondations/theme.md`
(validée le 07/11/2026). Une application fixe son thème : elle n'offre pas
d'en changer ; le changement à chaud sert la vitrine. Les couleurs de
maturité ne sont pas des jetons de thème : ce sont des *policies* de
l'application.

La langue, telle qu'elle est faite (`src/Mkm.Wpf/Localization`) :

- `<mkm:MkmResources Language="fr" />` dans `App.xaml` : la configuration ;
  sans `Language`, la langue d'affichage de Windows. Elle donne aussi aux
  éléments WPF le **format régional de Windows** pour les dates et les nombres
  (WPF écrirait sinon à l'américaine) ; changer de langue ne change pas ce
  format, pour qu'une valeur saisie (`2,5` kg) ne change pas de sens.
- `Localizer.Current` : la langue (`Language`, à chaud), les langues proposées
  (`Languages`), les sources (`AddSource`), et `T(clé, paramètres)` en code.
- `{mkm:T clé}` en XAML : un texte qui suit la langue.
- Les sources : `ResxTextSource` (fichiers `.resx`) et `TextTable` (une table
  que l'application remplit depuis ses propres fichiers). Celles de
  l'application passent avant celles de la bibliothèque.
- Un texte absent : sa langue parente, puis l'anglais, puis sa clé.

Le thème, tel qu'il est fait (`src/Mkm.Wpf/Theming`) :

- `<mkm:MkmResources Theme="Ara" />` ; sans `Theme`, Dashboard. `Themes`
  connaît Dashboard, ARA et les thèmes qu'une application enregistre
  (`Themes.Register`, souvent par `Themes.Dashboard.Derive(…)`).
- Une ressource par jeton, sous `mkm.<chemin de dt() dans mkm>` :
  `{DynamicResource mkm.actionbar.item.size}`. Un gabarit ne lit **que** ces
  ressources, et **toujours par `DynamicResource`** : jamais une couleur ni une
  mesure en dur.
- Les jetons viennent de mkm : `node outils/themes/importer-mkm.mjs <ui-kit>
  [composant…]` régénère `src/Mkm.Wpf/Theming/Tokens/*.tokens` (base, et les
  composants de sa liste). Un composant nouveau ajoute son nom à la liste de
  l'outil et relance l'import ; on ne retouche jamais un `.tokens` importé à la
  main.
- Ce que la feuille CSS d'un preset de mkm (`css`) dit sans jeton s'écrit à la
  main, par thème, dans `Theming/Tokens/<thème>.css.tokens`, lu après les
  jetons importés : aux valeurs de mkm, avec la teinte que chaque thème y met
  (`dsblue.100` dans Dashboard, `primary.100` dans ARA).
- `ThemesTests` vérifie, comme mkm : chaque renvoi résolu, ARA qui ne diffère
  de Dashboard que par ses couleurs, chaque texte lisible sur son fond
  (4,5:1) ; un composant ajoute ses paires texte/fond à la liste du test.

Les icônes (`src/Mkm.Wpf/Controls/Icon`) : celles de mkm, nommées par leur
sens (`{x:Static mkm:Icons.Add}`), dessinées par Tabler. `<mkm:Icon
Source="…" />` trace une géométrie à la couleur du texte, au trait fin de mkm
(1,5 sur la grille de 24, 1 px à 16 px), ou dessine une image de
l'application. Une icône de plus : l'ajouter au manifeste de mkm, puis
`node outils/icones/importer-tabler.mjs <ui-kit>` ; on ne retouche jamais
`Icons.g.vb` à la main.

Les règles pour chaque composant :

- aucun texte en dur : une clé dans `src/Mkm.Wpf/Localization/Strings.resx`
  (anglais) et sa traduction dans `Strings.fr.resx` — le premier composant qui
  affiche un texte les crée, avec la source de la bibliothèque
  (`AddLibrarySource`) et le test qui compare leurs clés
  (`ResxTextSourceTests.ClesSansTraduction`) ;
- les clés sont en anglais, en `camelCase` séparé par des points, préfixées
  par le composant : `actionBar.more` ;
- un texte que l'application voudrait changer est aussi une propriété du
  contrôle, qui l'emporte sur la ressource ;
- aucune largeur fixe pour un élément qui porte un texte : le français est
  20 à 30 % plus long que l'anglais ;
- aucune phrase construite par morceaux : un texte à paramètres (`{count}`) ;
- la vitrine a le choix de la langue dans sa barre (et aura celui du thème) :
  chaque composant s'y voit dans les deux langues ;
- l'écriture de droite à gauche n'est pas visée.

## L'open source d'abord

**Avant de développer un composant, regarder ce qui se fait de mieux en open
source** (Mourad, le 07/11/2026). Ce que d'autres ont déjà écrit, testé et
corrigé ne se réécrit pas : on ne perd pas des heures à refaire ce qui existe.

1. **Chercher** le composant, ou le plus proche, dans les bibliothèques de
   référence (liste plus bas), puis sur GitHub et NuGet. Regarder aussi
   comment WPF lui-même fait pour le contrôle le plus proche (`dotnet/wpf`).
2. **Juger chaque candidat** :
   - la licence : MIT, Apache-2.0 ou BSD. MS-PL : à discuter avec Mourad.
     GPL, LGPL ou licence réservée à un usage non commercial : exclues ;
   - la cible : compatible avec net48 ;
   - la vie du projet : versions récentes, issues traitées ;
   - la qualité : tests, clavier et UI Automation, performance.
3. **Choisir**, dans cet ordre :
   - le paquet tel quel, en dépendance NuGet, s'il fait le travail : un projet
     VB emploie un assembly C# sans difficulté ;
   - le code repris et porté en VB, avec son auteur et sa licence ;
   - un composant à nous, inspiré de ce qu'on a vu, quand rien ne convient —
     en disant pourquoi.
4. **Écrire la fiche** `docs/composants/<nom>.md` (modèle :
   `docs/composants/_modele.md`) : les candidats, ce qu'on retient, le choix
   et sa raison. **La fiche précède le code, et Mourad la valide.**
5. **Tracer** : une dépendance ou un code repris entre dans `ORIGINES.md` dans
   le même commit. Un code repris garde la mention de copyright de son auteur
   dans ses fichiers, et sa licence entre dans `THIRD-PARTY-NOTICES.md`.

Bibliothèques de référence, point de départ de la recherche (licence et cible
à revérifier à chaque fois) :

| Bibliothèque                | Licence | À regarder pour                                     |
| --------------------------- | ------- | --------------------------------------------------- |
| `dotnet/wpf`                | MIT     | le code de WPF : la façon de faire de référence      |
| MahApps.Metro               | MIT     | contrôles et thèmes                                 |
| MaterialDesignInXamlToolkit | MIT     | contrôles, thèmes, palettes                         |
| WPF UI (`lepoco/wpfui`)     | MIT     | contrôles modernes, un dossier par contrôle         |
| HandyControl                | MIT     | catalogue très large de contrôles                   |
| Fluent.Ribbon               | MIT     | ruban, barres de commandes                          |
| AvalonDock (Dirkster99)     | MS-PL   | fenêtres ancrables                                  |
| CommunityToolkit.Mvvm       | MIT     | commandes, MVVM                                     |
| Microsoft.Xaml.Behaviors    | MIT     | comportements, déclencheurs                         |

Exclu : Extended WPF Toolkit (Xceed), dont la licence interdit l'usage
commercial.

## Écrire un contrôle

On écrit comme WPF écrit : un contrôle sans apparence propre (*lookless*),
dont le gabarit vit dans un style par défaut.

- `src/Mkm.Wpf/Controls/<Nom>/` : la classe (`<Nom>.vb`, espace de noms
  `Mkm.Wpf.Controls`) et son style par défaut (`<Nom>.xaml`), fusionné dans
  `Themes/Generic.xaml` (une ligne par contrôle, ordre alphabétique).
- Le constructeur partagé appelle
  `DefaultStyleKeyProperty.OverrideMetadata(GetType(<Nom>), New FrameworkPropertyMetadata(GetType(<Nom>)))`.
- L'API passe par des `DependencyProperty`, des événements routés et des
  commandes (`ICommand`). Les parties du gabarit que le code emploie sont
  déclarées par `<TemplatePart(Name:="PART_…", Type:=…)>` et prises dans
  `OnApplyTemplate` ; le contrôle reste sûr si une partie manque.
- Les couleurs, tailles, rayons et polices viennent des ressources du thème
  (`DynamicResource`), jamais en dur dans un gabarit. La convention des jetons
  se fixe avec le premier thème.
- Clavier et UI Automation : focus visible, navigation au clavier, un
  `AutomationPeer` quand celui du contrôle de base ne suffit pas.
- Performance : animations par `Storyboard`, jamais de boucle avec
  `Thread.Sleep` ; listes virtualisées.
- Avec le contrôle : sa page dans la vitrine
  (`apps/Mkm.Wpf.Showcase/Pages/<Nom>Page.xaml`), sa fiche, ses tests de
  comportement, une ligne dans `CHANGELOG.md`. Les règles communes
  (`ConventionsTests`) s'appliquent d'elles-mêmes.

Les noms de l'API (classes, propriétés, événements) sont en anglais, comme
WPF et comme mkm. Les commentaires, la documentation et la vitrine sont en
français. Un texte affiché par un contrôle (« Fermer »…) est une propriété,
dont la valeur par défaut vient des textes traduits de la bibliothèque,
l'anglais en repli (voir « Thème et langue »).

## Conventions VB

- **`RootNamespace` est vide dans chaque projet** : chaque fichier écrit son
  espace de noms en entier (`Namespace Mkm.Wpf.Controls`), comme en C#. Un
  `x:Class` donne ainsi le nom complet de sa classe, et un code porté depuis
  C# garde ses espaces de noms. (Le SDK pose sa valeur par défaut après
  `Directory.Build.props` : c'est pourquoi la ligne est dans chaque projet.)
- Chaque espace de noms public de `Mkm.Wpf` est associé à
  `xmlns:mkm="urn:mkm-wpf"` par un `XmlnsDefinition` dans
  `src/Mkm.Wpf/AssemblyInfo.vb`, **dès qu'il a un type** : le compilateur XAML
  d'une application refuse un espace de noms vide.
- VB nomme une ressource `.resx` par le seul nom de son fichier, sans son
  dossier (`Textes/Vitrine.resx` → `"Vitrine"`) : deux `.resx` d'un même projet
  ne portent jamais le même nom.
- Les `Imports` sont écrits en tête de chaque fichier ; on n'ajoute pas
  d'imports au niveau du projet.
- Champs privés en `_camelCase` ; types, membres et constantes en PascalCase.
- NUnit : `Is` est un mot-clé de VB, on écrit `[Is]`
  (`Assert.That(x, [Is].EqualTo(1))`) ; une exception s'attend par
  `Assert.Throws(Of T)(New Action(Sub() …))` (sans `New Action`, VB ne choisit
  pas entre les surcharges).
- Une variable locale ne porte pas le nom d'un type (`Dim traducteur As New
  Localizer`, pas `localizer`) : VB ne distingue pas la casse.
- **Les noms de mkm qui sont aussi ceux de WPF** (`Button`, `Checkbox` et
  `CheckBox`, `Select`, `Tab`…) : un contrôle écrit son type de base en entier
  (`Inherits System.Windows.Controls.Button`) ; un fichier qui importe les deux
  espaces de noms écrit `Mkm.Wpf.Controls.Button` (la casse ne les distingue
  pas). Les mots-clés de VB se mettent entre crochets (`[Select]`, `[Variant]`,
  `[Error]`, `[Structure]`). En XAML, le préfixe suffit (`mkm:Button`).
- Un `Byte` multiplié par un `Byte` reste un `Byte` en VB et déborde : on
  convertit d'abord (`CDbl(a) * b`).

## Les pièges de WPF déjà rencontrés

- **Pas de chemin de liaison avec un préfixe** (`{Binding (local:Type.Prop)}`)
  dans la bibliothèque : WPF n'en trouvait pas le préfixe à l'exécution, et la
  liaison échouait sans bruit (les icônes de l'ActionBar). On écrit
  `{TemplateBinding local:Type.Prop}`, ou la liaison en code
  (`New PropertyPath(laPropriete)`). Un test le vérifie.
- **Un `TextBox` pose lui-même son `Padding`** dans sa zone de texte : un gabarit
  ne le pose pas une seconde fois.
- **Le `TabPanel` retranche la marge d'un onglet** de la place qu'il lui donne :
  la marge d'un onglet va dans son gabarit.
- **Une couleur posée sur un `TabItem` passe à son contenu** : elle se pose sur
  le titre, dans le gabarit.
- **Un rayon plus grand que la moitié du côté** (999 px chez mkm) n'est pas
  borné par WPF comme par CSS : il dessine un fuseau ; on le borne en code.
- **`ToolTip` et `ContextMenu` ne se dessinent qu'ouverts** ; `StartupUri` ne
  s'efface pas (`Nothing` lève une exception).
- **Les styles implicites des contrôles de WPF eux-mêmes** (info-bulle, menu)
  vont dans `Themes/Natifs.xaml`, que `MkmResources` fusionne ; ceux des
  contrôles de la bibliothèque, dans `Themes/Generic.xaml`.
- **Un nom de mkm cache celui de WPF** dans l'espace de noms des contrôles :
  `ToggleButton`, `RadioButton`, `Menu`, `Button` y désignent ceux de mkm, même
  là où l'on voulait celui de WPF (un `TryCast(…, ToggleButton)` sur une partie
  de gabarit rend alors `Nothing`, sans bruit). Écrire le nom complet
  (`System.Windows.Controls.Primitives.ToggleButton`) ; `ConventionsTests`
  vérifie le type de chaque partie annoncée d'un gabarit.
- **Une propriété héritée suit le parent logique** avant le parent visuel : posée
  sur le cadre d'une fenêtre flottante ou d'un sous-menu, elle n'atteint pas les
  éléments qui s'y dessinent mais appartiennent à la liste (la commande d'une
  liste de l'ActionBar, l'élément d'un sous-menu). La poser sur chaque élément
  (`PrepareContainerForItemOverride`).
- **Une valeur locale bat les déclencheurs d'un gabarit** qui visent le contrôle
  lui-même (sans `TargetName`) : une couleur posée en code (`SetResourceReference`)
  les rend muets. La faire porter par un élément du gabarit, que les déclencheurs
  visent ; et ne pas écraser une valeur que l'application a posée (`Habillage`).
- **`IsKeyboardFocused` est vrai aussi après un clic** : pour un anneau du focus
  au clavier seulement (`:focus-visible`), `FocusVisible.IsVisible`.
- **Un `MenuItem` survolé prend le focus**, et un `MenuBase` où le focus entre
  passe en mode menu et capture la souris : bon pour un menu qui s'ouvre, mauvais
  pour un menu posé dans la page (un champ perdrait le focus au survol). `Menu` et
  `TieredMenu` refusent le focus venu du survol ; `Menu` ne passe pas en mode menu.
- **Une ligne recyclée du `DataGrid` garde ses cellules** : `GenerateElement`
  n'est pas rappelé pour le nouvel objet, seul le `DataContext` change. Ce que
  la cellule tient de son objet suit `DataContextChanged`.
- **Pas de `MinWidth` lié au `ViewportWidth` d'un `ScrollViewer`** qui le
  contient : c'est un cliquet, le contenu ne rétrécit plus jamais (l'ActionBar
  gardait la largeur de sa plus large section). Le `ScrollViewer` étire déjà son
  contenu à sa largeur visible ; un `HorizontalAlignment="Center"` suffit à
  centrer.

## Vérifier

**Sous Windows (référence)** : ouvrir `Mkm.Wpf.sln` dans Visual Studio 2022,
compiler, lancer la vitrine et les tests ; ou, à la racine, `dotnet build` puis
`dotnet test`.

**La CI** (`.github/workflows/ci.yml`) compile et lance les tests sous
Windows à chaque poussée sur `main`. Un runner Windows consomme deux fois plus
de minutes GitHub Actions qu'un runner Linux sur un dépôt privé.

**Les captures** : la CI lance aussi `Mkm.Wpf.Showcase.exe --captures <dossier>`
(`apps/Mkm.Wpf.Showcase/Captures.vb`) et publie l'artefact `captures` : chaque page
de la vitrine dans chaque thème, la fenêtre, des notifications et des dialogues,
en PNG à 96 ppp. Une session sans Windows les télécharge
(`https://api.github.com/repos/KERRAM95/mkm-wpf/actions/artifacts/<id>/zip`) pour
regarder le rendu ; les états de survol n'y sont pas.

**Sous Linux** (une session sans Windows) : `outils/linux/verifier.sh`
compile toute la solution, code VB et XAML compris. Il lui faut le SDK .NET
de Microsoft — celui d'Ubuntu n'a pas la partie WPF ; le script l'installe
dans un cache s'il manque. Il n'exécute rien : la vitrine et les tests ne
tournent que sous Windows.

## Les versions

Une seule version pour tout le dépôt (`VersionPrefix` dans
`Directory.Build.props`), comme mkm :

- **Mineure** (`0.2.0`) : un composant, une propriété facultative, un thème en
  plus.
- **Correctif** (`0.1.1`) : un défaut corrigé sans changer l'API.
- **Majeure** : une rupture. Tant qu'on est en `0.x`, une rupture passe à la
  mineure suivante, et `CHANGELOG.md` la nomme.

Publier : la version et sa section de `CHANGELOG.md` dans un même commit, puis
un tag `vX.Y.Z`. La forme de la livraison (paquet NuGet, DLL) se décide avec
la première version.
