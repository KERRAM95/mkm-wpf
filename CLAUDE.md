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
- Une fois par clone : `git config core.hooksPath outils/hooks` ; une session Claude Code ouverte
  sur ce dépôt le fait d'elle-même (`.claude/settings.json`).
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
| `apps/Mkm.Wpf.Showcase` | `Mkm.Wpf.Showcase` | la documentation : une page par composant, comme ui-kit                  |
| `tests/Mkm.Wpf.Tests`   | `Mkm.Wpf.Tests`    | les tests (NUnit), dont les règles communes à tous les contrôles         |
| `tests/Mkm.Wpf.UiTests` | `Mkm.Wpf.UiTests` | les tests d'interaction (FlaUI) : la vitrine pilotée comme par un utilisateur |
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

Comment c'est fait (langue, thème, icônes, règles de chaque composant) : `.claude/rules/theme-et-langue.md`.

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

## Vérifier

Le skill `/verifier` (`.claude/skills/verifier/`) : compiler sous Windows ou sous Linux, la CI, les captures de la
vitrine. Une poussée n'est finie que CI verte.

## Organisation des consignes

Ce fichier donne le cadre, chargé à chaque session. Le détail par sujet est dans `.claude/`, chargé seulement quand on
touche les fichiers concernés :

| Fichier | Sujet | S'applique à |
|---|---|---|
| `rules/open-source.md` | chercher l'existant avant d'écrire un composant, la fiche, la traçabilité | `docs/composants/`, `src/Mkm.Wpf/Controls/`, `ORIGINES.md`… |
| `rules/controle.md` | écrire un contrôle, sa page de documentation | `src/Mkm.Wpf/Controls/`, `Themes/`, la documentation de la vitrine |
| `rules/theme-et-langue.md` | langue, thème, jetons, icônes, règles de chaque composant | `src/`, `apps/`, `tests/`, les outils d'import |
| `rules/vb.md` | conventions VB | `*.vb`, `*.vbproj`, `*.props` |
| `rules/pieges-wpf.md` | les pièges de WPF déjà rencontrés, l'accessibilité | `*.xaml`, `*.vb` |
| `skills/verifier/` | compiler, la CI, les captures (`/verifier`) | sur demande ou avant une poussée |
| `settings.json` | identité des commits, sans attribution ; active `outils/hooks/` à l'ouverture | toujours |

Une préférence propre à un poste va dans `CLAUDE.local.md` ou `.claude/settings.local.json` (non versionnés).
