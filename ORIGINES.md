# Origines

Ce que mkm-wpf doit à d'autres projets : chaque dépendance et chaque morceau de
code repris d'un projet open source. Une ligne entre ici dans le même commit que
l'emprunt (voir `CLAUDE.md`, « L'open source d'abord »).

## Dépendances

| Paquet                                     | Version | Licence | Employé par                     | Pourquoi                                                       |
| ------------------------------------------ | ------- | ------- | ------------------------------- | -------------------------------------------------------------- |
| Microsoft.NETFramework.ReferenceAssemblies | 1.0.3   | MIT     | tous (compilation seulement)    | compiler pour .NET Framework 4.8 sans Developer Pack installé  |
| NUnit                                      | 4.6.1   | MIT     | `Mkm.Wpf.Tests`                 | les tests ; `[Apartment(STA)]` pour WPF                        |
| NUnit3TestAdapter                          | 6.3.1   | MIT     | `Mkm.Wpf.Tests`                 | lancer les tests NUnit depuis Visual Studio et `dotnet test`   |
| Microsoft.NET.Test.Sdk                     | 18.11.0 | MIT     | `Mkm.Wpf.Tests`                 | l'hôte de test de `dotnet test`                                |

La bibliothèque `Mkm.Wpf` elle-même n'a encore aucune dépendance.

## Jetons de thème

| Élément de mkm-wpf | Source | Licence | Ce qui est repris |
| ------------------ | ------ | ------- | ----------------- |
| `src/Mkm.Wpf/Theming/Tokens/Dashboard.tokens` | mkm (`ui-kit`), `packages/themes/src/presets/dashboard`, sur `nora` en pixels | MIT (PrimeUIX, par mkm) | les jetons de la base et des composants importés, valeurs comprises |
| `src/Mkm.Wpf/Theming/Tokens/Ara.tokens` | mkm (`ui-kit`), `packages/themes/src/presets/ara` | MIT (PrimeUIX, par mkm) ; couleurs de la charte ARA | idem |
| la structure des jetons (primitifs, sémantiques, par composant) et leurs chemins | PrimeUIX, par mkm | MIT | les noms, tels que `dt()` les lit |
| `src/Mkm.Wpf/Theming/Tokens/*.css.tokens` | mkm (`ui-kit`), la feuille CSS des presets `inputtext`, `select`, `button`, `checkbox`, `tabs`, `message`, `toast`, `dialog`, `contextmenu`, `idcard`, `togglebutton`, `datatable`, `toolbar` de Dashboard et d'ARA, et les feuilles de PrimeVue par mkm (`tooltip`, `progressspinner`, `textarea`, `datepicker`, `confirmdialog`, `propertypanel`, `tree`, `inputnumber`, `selectbutton`, `radiobutton`) | MIT (mkm) | leurs mesures et leurs couleurs, réécrites en jetons à la main |
| `src/Mkm.Wpf/Controls/Toast/ToastMessage.vb` (la vie d'une notification) | mkm (`ui-kit`), `packages/mkm/src/toast/ToastMessage.vue` | MIT (mkm, sur PrimeVue) | la règle de vie, de relief, de fondu et de repli, portée en VB |
| `src/Mkm.Wpf/Controls/ProgressBar/ProgressBar.vb`, `ProgressSpinner.vb` (les animations) | PrimeVue par mkm (`packages/styles/src/progressbar`, `progressspinner`) | MIT | les images clés, leurs durées et leurs courbes (`cubic-bezier`), portées en animations WPF |
| `src/Mkm.Wpf/Controls/Toolbar/ToolbarPanel.vb` (le débordement) | mkm (`ui-kit`), `packages/mkm/src/toolbar/Toolbar.vue` (`updateOverflow`, `isItemShown`, `overflowItems`) | MIT (mkm, sur PrimeVue) | la répartition des commandes entre la barre et le menu « … », la règle des traits, portées en VB |
| `src/Mkm.Wpf/Theming/ThemeResources.vb` (`color-mix`) | CSS Color 5, « color-mix() » (W3C) | spécification | le mélange pondéré de l'opacité, comme le calcule `contrast.ts` de mkm |

Importés par `outils/themes/importer-mkm.mjs` ; les couleurs de maturité (celles
d'`idcard` comprises) et le mode sombre ne le sont pas.

## Icônes

| Élément de mkm-wpf | Source | Version | Licence | Ce qui est repris |
| ------------------ | ------ | ------- | ------- | ----------------- |
| `src/Mkm.Wpf/Controls/Icon/Icons.g.vb` | Tabler Icons (`@tabler/icons`), les noms du manifeste de mkm (`ui-kit`, `packages/icons/tabler/manifest.json`) | 3.48.0 | MIT (licence dans `THIRD-PARTY-NOTICES.md`) | les formes des 95 icônes du manifeste, style outline, en géométries WPF |
| `src/Mkm.Wpf/Controls/Icon/InternalIcons.g.vb` | Tabler Icons, la clé `internal` du même manifeste (les icônes des composants de mkm) | 3.48.0 | MIT | les formes de 45 icônes, style outline (les deux pleines ne sont pas reprises) |

Importées par `outils/icones/importer-tabler.mjs`, qui contrôle l'empreinte du
paquet donnée par le manifeste.

## Code repris

Aucun pour l'instant.

| Élément de mkm-wpf | Source | Version ou commit | Licence | Ce qui est repris | Écarts |
| ------------------ | ------ | ----------------- | ------- | ----------------- | ------ |
