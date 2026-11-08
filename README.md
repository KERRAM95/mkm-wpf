# mkm-wpf

Bibliothèque de composants WPF en VB.NET, déclinaison bureau de **mkm**
(`ui-kit`) : contrôles, thèmes, icônes et commandes réutilisables, avec leur
documentation. Neutre, indépendante de toute application.

**État :** les fondations (langue, thème et zoom, changeables à chaud), les icônes
de mkm et 94 composants, chacun avec sa page de documentation, ses tests et
ses scénarios d'interaction. La version 0.1.0 est en préparation.

**La documentation** est l'application `Mkm.Wpf.Showcase`, sur le modèle de
celle d'ui-kit : une page par composant, rangée par catégorie. Chaque page
donne les usages du composant (un exemple vivant, et le XAML qui le produit, à
copier), son accessibilité, son API (tirée du code) et ses jetons de thème.

## Prérequis

- Windows, Visual Studio 2022 avec la charge de travail « Développement .NET
  Desktop ».
- Rien d'autre : les assemblies de référence de .NET Framework 4.8 viennent
  d'un paquet NuGet, sans Developer Pack à installer.

## Démarrer

    git clone https://github.com/KERRAM95/mkm-wpf.git
    cd mkm-wpf
    git config core.hooksPath outils/hooks

Ouvrir `Mkm.Wpf.sln`, puis F5 : la documentation `Mkm.Wpf.Showcase` est le projet
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
    ├── apps/Mkm.Wpf.Showcase/      la documentation : une page par composant (Documentation/Pages)
    ├── tests/Mkm.Wpf.Tests/        les tests (NUnit)
    ├── tests/Mkm.Wpf.UiTests/      les tests d'interaction (FlaUI) : la documentation pilotée comme par un utilisateur
    ├── docs/composants/            une fiche par composant, écrite avant le code
    ├── docs/fondations/            le thème, la langue, le zoom, le registre des commandes
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
documentation en est l'exemple (`apps/Mkm.Wpf.Showcase`).

Les commandes de l'application se déclarent une fois dans un `CommandRegistry`
(`Mkm.Wpf.Commands`) ; ses menus, ses barres et ses raccourcis en sont fabriqués
(`docs/fondations/commandes.md`).

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
6. ~~Les boutons, les onglets, les groupes, les messages, les notifications et
   le dialogue~~ : faits — `Button`, `Checkbox`, `Tabs`, `Fieldset`, `Message`,
   `Toast`, `Dialog` (`docs/composants/boutons.md`, `conteneurs.md`,
   `retours.md`).
7. ~~Le lot 2~~ : fait — l'info-bulle, `ProgressBar`, `ProgressSpinner`,
   `BlockUI`, `Textarea`, `DatePicker`, `ConfirmDialog`, `PropertyPanel`, `Tree`,
   le menu contextuel (`docs/composants/attente.md`, `saisie.md`,
   `fiche-de-proprietes.md`, `arbre-et-menu.md`).
8. ~~Le lot 3~~ : fait — `IdCard`, `Tag`, `SelectButton`, `InputNumber`
   (`docs/composants/identite-et-choix.md`).
9. ~~Le tableau de données~~ : fait — `DataTable` (`docs/composants/tableau.md`).
10. ~~Le lot 5~~ : fait — `RadioButton`, `ToggleButton`, `Toolbar`
   (`docs/composants/choix-et-barre.md`).
11. ~~Le lot 6~~ : fait — la mise en page d'une application : `Triptych`,
   `WelcomePanel`, `Splitter`, `EmptyContent` (`docs/composants/mise-en-page.md`).
12. ~~Le lot 7~~ : fait — les menus posés dans la page, `Menu` et `TieredMenu`
   (`docs/composants/menus.md`).
13. ~~Le lot 8~~ : fait — la grille arborescente (`TreeGrid`) et le graphe du
   cycle de vie (`StateGraph`) (`docs/composants/arborescence-et-cycle-de-vie.md`).
14. ~~Le registre des commandes~~ : fait — `CommandRegistry`
   (`docs/fondations/commandes.md`).
15. ~~Le lot 10~~ : fait — les barres de défilement de mkm partout, `ScrollPanel`,
   `Divider`, `ToggleSwitch`, `Skeleton` (`docs/composants/bases-et-defilement.md`).
16. ~~Le lot 11~~ : fait — `Badge`, `OverlayBadge`, `Avatar`, `AvatarGroup`, `Chip`
   (`docs/composants/pastilles-et-personnes.md`).
17. ~~Le lot 12~~ : fait — `Panel`, `Card`, `Accordion`, `AccordionPanel`
   (`docs/composants/panneaux-et-accordeon.md`).
18. ~~Le lot 13~~ : fait — `Breadcrumb`, `SplitButton`, `Popover`
   (`docs/composants/chemin-bouton-partage-et-popover.md`).
19. ~~Le lot 14~~ : fait — `Listbox`, `MultiSelect`, `AutoComplete`,
   `IconField`, `InputGroup` (`docs/composants/listes-et-champs-composes.md`).
   La priorité 1 est faite.
20. ~~Le lot 15~~ : fait — `Stepper`, `Timeline`, `Paginator`
   (`docs/composants/etapes-frise-et-pagination.md`).
21. ~~Le lot 16~~ : fait — `Drawer`, `Inplace`, `Image`, `ImageCompare`
   (`docs/composants/tiroir-sur-place-et-images.md`).
22. ~~Le lot 17~~ : fait — `TreeSelect`, `CascadeSelect`
   (`docs/composants/listes-en-arbre-et-en-cascade.md`). La priorité 2 est faite.
23. ~~Le lot 18~~ : fait — `Rating`, `Slider`, `Knob`
   (`docs/composants/note-curseur-et-bouton-rotatif.md`).
24. ~~Le lot 19~~ : fait — `Password`, `InputMask`, `InputOtp`, `KeyFilter`
   (`docs/composants/saisies-protegees-et-filtrees.md`).
25. ~~Le lot 20~~ : fait — `InputChips`, `FloatLabel`, `IftaLabel`,
   `ColorPicker` (`docs/composants/puces-libelles-et-couleur.md`). Les champs
   sont faits.
26. ~~Le lot 21~~ : fait — `Menubar`, `MegaMenu`, `PanelMenu`
   (`docs/composants/barres-et-menus-de-navigation.md`).
27. ~~Le lot 22~~ : fait — `ButtonGroup`, `Dock`, `SpeedDial`
   (`docs/composants/groupes-dock-et-actions.md`).
28. ~~Le lot 23~~ : fait — `OrderList`, `PickList`, `DataView`
   (`docs/composants/listes-ordonnees-et-vues.md`).
29. ~~Le lot 24~~ : fait — `OrganizationChart`, `MeterGroup`
   (`docs/composants/organigramme-et-jauges.md`).
30. ~~Le lot 25~~ : fait — `Carousel`, `Galleria`, `Terminal`
   (`docs/composants/carrousel-galerie-et-terminal.md`). La priorité 3 est
   faite : tous les composants de la liste sont là.
31. La suite : le paquet NuGet ; hscart-next sur la bibliothèque.

## Règles

Les règles de travail sont dans `CLAUDE.md`, les emprunts à l'open source
dans `ORIGINES.md`, les évolutions dans `CHANGELOG.md`.
