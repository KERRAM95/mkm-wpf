# mkm-wpf

Bibliothèque de composants WPF en VB.NET, déclinaison bureau de **mkm**
(`ui-kit`) : contrôles, thèmes, icônes et commandes réutilisables, avec une
vitrine. Neutre, indépendante de toute application.

**État :** les fondations (langue et thème, changeables à chaud), les icônes
de mkm et le premier composant, l'ActionBar. La version 0.1.0 est en
préparation.

## Prérequis

- Windows, Visual Studio 2022 avec la charge de travail « Développement .NET
  Desktop ».
- Rien d'autre : les assemblies de référence de .NET Framework 4.8 viennent
  d'un paquet NuGet, sans Developer Pack à installer.

## Démarrer

    git clone https://github.com/KERRAM95/mkm-wpf.git
    cd mkm-wpf
    git config core.hooksPath outils/hooks

Ouvrir `Mkm.Wpf.sln`, puis F5 : la vitrine `Mkm.Wpf.Showcase` est le projet
de démarrage (le premier de la solution). Si Visual Studio en garde un autre,
clic droit sur `Mkm.Wpf.Showcase`, « Définir en tant que projet de démarrage ».
En ligne de commande :

    dotnet build
    dotnet test
    dotnet run --project apps/Mkm.Wpf.Showcase

(Des `/` dans les chemins : ils marchent dans tous les terminaux, bash compris.)

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
La configuration se pose une fois dans `App.xaml` :

```xml
<Application xmlns:mkm="urn:mkm-wpf" …>
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <mkm:MkmResources Language="fr" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Les textes de l'application viennent de ses sources, ajoutées au démarrage
(`Localizer.Current.AddSource(…)`), et s'écrivent `{mkm:T clé}` en XAML ;
`Localizer.Current.Language = …` change la langue pendant l'exécution. La
vitrine en est l'exemple (`apps/Mkm.Wpf.Showcase`).

## Choix techniques

- **.NET Framework 4.8**, celui des postes : rien de nouveau à déployer.
- **VB.NET**, projets au format SDK : lisibles, et prêts pour un paquet NuGet.
- **Contrôles sans apparence propre**, stylés par thème : l'apparence change
  sans toucher au code, comme dans mkm.
- **L'open source d'abord** : avant d'écrire un composant, on étudie ce qui
  existe (voir `CLAUDE.md`).

## Feuille de route

1. ~~La langue~~ : faite — une variable, changeable pendant l'exécution
   (`docs/fondations/theme-et-langue.md`).
2. ~~Les thèmes~~ : faits — Dashboard et ARA, les jetons de mkm, par la même
   configuration (`docs/fondations/theme.md`).
3. ~~Les icônes~~ : faites — les 95 de mkm, dessinées par Tabler.
4. ~~L'ActionBar~~ : faite, comme celle de mkm (`docs/composants/actionbar.md`).
5. ~~Les champs~~ : faits — `InputText` et `Select` (`docs/composants/champs.md`).
6. La fiche de propriétés de mkm (`PropertyPanel`) : le libellé, le message et
   la marque d'un champ obligatoire ; puis les onglets et les groupes.
7. Le registre des commandes.

## Règles

Les règles de travail sont dans `CLAUDE.md`, les emprunts à l'open source
dans `ORIGINES.md`, les évolutions dans `CHANGELOG.md`.
