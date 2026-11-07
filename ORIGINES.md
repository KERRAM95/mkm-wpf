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

Importés par `outils/themes/importer-mkm.mjs` ; les couleurs de maturité et le
mode sombre ne le sont pas.

## Code repris

Aucun pour l'instant.

| Élément de mkm-wpf | Source | Version ou commit | Licence | Ce qui est repris | Écarts |
| ------------------ | ------ | ----------------- | ------- | ----------------- | ------ |
