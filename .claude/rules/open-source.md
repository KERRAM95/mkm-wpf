---
paths:
  - "docs/composants/**"
  - "src/Mkm.Wpf/Controls/**"
  - "ORIGINES.md"
  - "THIRD-PARTY-NOTICES.md"
  - "Directory.Packages.props"
---

# L'open source d'abord

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
