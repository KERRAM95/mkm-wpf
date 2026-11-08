# Organigramme et jauges : OrganizationChart, MeterGroup

> Fiche du lot 24, priorité 3.

**État :** livrée dans la version en préparation.

## Le besoin

- **Montrer une hiérarchie** : une organisation, une structure de produit
  vue de haut (OrganizationChart).
- **Montrer des parts** d'un tout : l'espace occupé par type de document, l'état
  d'avancement par lot (MeterGroup).

## Ce que fait mkm

| Composant         | Dans mkm                                                                                          |
| ----------------- | ------------------------------------------------------------------------------------------------- |
| OrganizationChart | chaque nœud dans un cadre bordé, 0,75 rem sur 1 rem de retrait ; sous un nœud, un trait de 24 px, puis le trait qui relie ses enfants ; un bouton rond de 1,5 rem pour replier (`collapsible`) ; le choix (`selectionMode`) |
| MeterGroup        | une barre de 0,5 rem, grise, aux coins du thème, les jauges côte à côte ; la légende : un repère rond (ou une icône), le libellé et la part ; en ligne ou en colonne, la légende avant ou après |

## Le choix

- **OrganizationChart** hérite d'`ItemsControl` : des
  `OrganizationChartNode` (`Header`, `IsExpanded`, `IsSelected`), à tout
  niveau, écrits dans le XAML ou liés à des données (`ItemsSource`,
  `HierarchicalDataTemplate`) ; `Collapsible`, `Selectable`, `SelectedItem`,
  l'évènement `SelectedItemChanged`. Une rangée interne pose les enfants et
  trace leurs traits.
- **MeterGroup** hérite d'`ItemsControl` : des `MeterItem` (`Label`,
  `Value`, `Color`, `Icon` ; `Percent` et `Legend` calculés), `Minimum`,
  `Maximum`, `Orientation`, `LabelPosition`, `LabelOrientation`. La barre se
  redessine quand une valeur change.

## Écarts avec mkm

- OrganizationChart : un seul nœud choisi (pas de choix multiple) ; les
  traits n'ont pas de coins arrondis.
- MeterGroup : pas de gabarit de légende (`label` slot) ; la légende est
  celle de mkm.

## Clavier, accessibilité, performance

- OrganizationChart : un arbre ; chaque nœud, un élément d'arborescence nommé
  par son en-tête, développé ou réduit, sélectionné ou non ; Entrée et Espace
  choisissent ; le bouton se nomme « Replier » ou « Déplier ».
- MeterGroup : un groupe nommé, sa légende lisible.

## La documentation et les tests

Les pages OrganizationChart et MeterGroup ; `Lot24Tests` ; `DonneesTests`
(un nœud choisi, puis replié).
