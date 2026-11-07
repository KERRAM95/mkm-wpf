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
| `src/Mkm.Wpf`           | `Mkm.Wpf`          | les contrôles et leurs styles par défaut (`Themes/Generic.xaml`)         |
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
traduisible. Le mécanisme est dans `docs/fondations/theme-et-langue.md`, à
valider avant le premier composant ; ses règles entrent ici une fois validé.

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
  `src/Mkm.Wpf/AssemblyInfo.vb`.
- Les `Imports` sont écrits en tête de chaque fichier ; on n'ajoute pas
  d'imports au niveau du projet.
- Champs privés en `_camelCase` ; types, membres et constantes en PascalCase.
- NUnit : `Is` est un mot-clé de VB, on écrit `[Is]`
  (`Assert.That(x, [Is].EqualTo(1))`).

## Vérifier

**Sous Windows (référence)** : ouvrir `Mkm.Wpf.sln` dans Visual Studio 2022,
compiler, lancer la vitrine et les tests ; ou, à la racine, `dotnet build` puis
`dotnet test`.

**La CI** (`.github/workflows/ci.yml`) compile et lance les tests sous
Windows à chaque poussée sur `main`. Un runner Windows consomme deux fois plus
de minutes GitHub Actions qu'un runner Linux sur un dépôt privé.

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
