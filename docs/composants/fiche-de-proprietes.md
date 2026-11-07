# La fiche de propriétés : PropertyPanel

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faite le 07/11/2026 (`Controls/PropertyPanel`).

## Le besoin

Montrer et modifier les propriétés d'un objet (le cartouche d'un plan), par
sections : libellé et valeur, la marque des propriétés obligatoires, une note,
un message sous un champ refusé. C'est ce que faisaient, dans l'ancien HS CART,
les `LabeledTextBox`, `LabeledComboBox` et `GroupPanel`.

## Ce que fait mkm

Le `PropertyPanel` de mkm (composant propre à mkm, preset `propertypanel`) :
- **les sections :** repliables, le titre de 13 px en demi-gras sur une ligne de 32 px, la flèche à gauche, de la couleur principale foncée quand la section est ouverte ; la première s'ouvre d'elle-même ;
- **les propriétés :** une ligne de 3 px sur 8 de retrait, le libellé gris de 12 px au-dessus de la valeur (`stacked`) ou devant elle sur 40 % (`inline`) ; la valeur de 13 px, sur des lignes de 20 px ; une icône devant la valeur ; une note grise de 11 px ;
- **en modification (`editing`) :** les éditeurs, la marque rouge des obligatoires (une étoile de trois traits, 7 px), le message sous l'éditeur, le libellé de la couleur de sa gravité ;
- **les boutons (`editable`) :** Modifier (le crayon), puis Enregistrer et Annuler ; `saving`, `canSave`.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `PropertyGrid` de Windows Forms, `xctk:PropertyGrid` (Ms-PL) | les propriétés d'un objet par réflexion, par catégories | l'apparence ; la réflexion ne choisit pas les libellés, l'ordre ni les éditeurs d'une fiche métier |
| `HeaderedItemsControl`, `Expander` de WPF | les sections repliables | la fiche entière |

## Le choix (technique)

- **En XAML, par composition** : `PropertyPanel` (un `ItemsControl`) contient
  des `PropertySection` (des `HeaderedItemsControl`), qui contiennent des
  `PropertyRow` (des `ContentControl`). Le contenu d'une ligne est son éditeur,
  lié à l'objet ; `Value` est sa valeur en lecture. Le brouillon de mkm
  (`modelValue`) est ici la liaison des éditeurs : l'application enregistre à
  `Save`, revient aux valeurs enregistrées à `Cancel`.
- **`Editing` et `Layout` sont héritées** par les sections et les lignes : une
  ligne montre son éditeur en modification (`IsEditorShown`), sauf `ReadOnly`.

## L'API

- **PropertyPanel :** `Editing`, `Editable`, `Saving`, `CanSave`, `Layout`
  (`Stacked`, `Inline`), `Header`, `Footer` ; `ExpandAll`, `CollapseAll` ;
  événements `Edit`, `Save`, `Cancel`.
- **PropertySection :** `Header`, `IsExpanded`.
- **PropertyRow :** `Label`, `Value`, `Icon`, `Required`, `ReadOnly`, `Note`,
  `Message`, `MessageSeverity` (`Error`, `Warn`, `Info`) ; le contenu est
  l'éditeur.

## Écarts avec mkm

- Pas de squelettes de chargement (`loading`) ; les sections s'ouvrent sans
  transition.
- Pas de fiche décrite par des données (`sections` et `fields`) : en WPF, la
  fiche s'écrit en XAML ; un modèle de données pourra s'y ajouter.

## La vitrine et les tests

La page « Fiche de propriétés » montre la fiche d'un plan (Cartouche, Notes,
Options) ; Modifier montre les éditeurs, les marques et un avertissement ; une
case met les libellés devant les valeurs. Les tests vérifient l'ouverture de la
première section, les éditeurs en modification, la lecture seule, les boutons
et leurs événements, le message.
