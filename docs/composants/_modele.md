# <Nom du composant>

> Fiche écrite **avant** le code (`CLAUDE.md`, « L'open source d'abord »).
> Mourad la valide avant le développement. Copier ce modèle en
> `docs/composants/<nom>.md`.

**État :** à l'étude · validée le JJ/MM/AAAA · livrée en X.Y.Z

## Le besoin

Ce que le composant doit faire, et dans quelles situations. Sans nom
d'application cliente : un besoin d'application devient un besoin générique.
À quoi il ressemble ailleurs (mkm, d'autres logiciels), avec une capture si
elle aide.

## L'état de l'art

| Candidat | Licence | net48 | Vivant | Ce qu'il fait bien | Ce qui manque |
| -------- | ------- | ----- | ------ | ------------------ | ------------- |
|          |         |       |        |                    |               |

**Dans WPF :** le contrôle le plus proche (`ToolBar`, `TabControl`,
`ItemsControl`…), ce qu'on peut en hériter, et comment `dotnet/wpf` le code.

## Le choix

Le paquet tel quel, le code repris et porté, ou un composant à nous — et
pourquoi. Ce qu'on reprend, et ce qu'on laisse.

## L'API prévue

- **Propriétés** (`DependencyProperty`) : nom, type, valeur par défaut.
- **Événements routés** et **commandes**.
- **Parties du gabarit** (`PART_…`).
- **Ressources du thème** que le style emploie.

## Clavier, accessibilité, performance

Navigation au clavier, focus visible, UI Automation, virtualisation,
animations.

## La documentation et les tests

Les sections de sa page de documentation (`apps/Mkm.Wpf.Showcase/Documentation/Pages/<id>.xml`) ;
les comportements que les tests vérifient, et ses scénarios d'interaction.
