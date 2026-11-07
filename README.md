# mkm-wpf

Bibliothèque de composants WPF en VB.NET, déclinaison bureau de **mkm**
(`ui-kit`) : contrôles, thèmes, icônes et commandes réutilisables, avec une
vitrine. Neutre, indépendante de toute application.

**État :** squelette. La structure, les règles, la vitrine et les tests sont en
place ; aucun composant pour l'instant. La version 0.1.0 est en préparation.

## Prérequis

- Windows, Visual Studio 2022 avec la charge de travail « Développement .NET
  Desktop ».
- Rien d'autre : les assemblies de référence de .NET Framework 4.8 viennent
  d'un paquet NuGet, sans Developer Pack à installer.

## Démarrer

    git clone https://github.com/KERRAM95/mkm-wpf.git
    cd mkm-wpf
    git config core.hooksPath outils/hooks

Ouvrir `Mkm.Wpf.sln`, choisir `Mkm.Wpf.Showcase` comme projet de démarrage,
puis F5. En ligne de commande :

    dotnet build
    dotnet test

## Structure

    mkm-wpf/
    ├── src/Mkm.Wpf/                la bibliothèque : contrôles et styles par défaut
    │   ├── AssemblyInfo.vb         ThemeInfo, xmlns XAML (urn:mkm-wpf)
    │   └── Themes/Generic.xaml     la fusion des styles de chaque contrôle
    ├── apps/Mkm.Wpf.Showcase/      la vitrine
    ├── tests/Mkm.Wpf.Tests/        les tests (NUnit)
    ├── docs/composants/            une fiche par composant, écrite avant le code
    ├── outils/                     hook Git, compilation sous Linux
    ├── Directory.Build.props       propriétés communes (net48, Option Strict…)
    ├── Directory.Packages.props    versions des paquets NuGet
    ├── CLAUDE.md                   les règles de travail
    ├── ORIGINES.md                 ce que la bibliothèque doit à l'open source
    └── CHANGELOG.md                les évolutions, version par version

## Employer la bibliothèque

Pour l'instant, par une référence de projet vers `src/Mkm.Wpf/Mkm.Wpf.vbproj`.
Dans le XAML :

```xml
<Window xmlns:mkm="urn:mkm-wpf" …>
```

## Choix techniques

- **.NET Framework 4.8**, celui des postes : rien de nouveau à déployer.
- **VB.NET**, projets au format SDK : lisibles, et prêts pour un paquet NuGet.
- **Contrôles sans apparence propre**, stylés par thème : l'apparence change
  sans toucher au code, comme dans mkm.
- **L'open source d'abord** : avant d'écrire un composant, on étudie ce qui
  existe (voir `CLAUDE.md`).

## Feuille de route

1. Les thèmes : les jetons de conception, un thème par défaut et ARA.
2. L'ActionBar, une barre de commandes en onglets.
3. Le registre des commandes, puis les icônes.

## Règles

Les règles de travail sont dans `CLAUDE.md`, les emprunts à l'open source
dans `ORIGINES.md`, les évolutions dans `CHANGELOG.md`.
