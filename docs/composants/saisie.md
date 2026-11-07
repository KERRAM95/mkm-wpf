# Le texte, la date et la confirmation : Textarea, DatePicker et ConfirmDialog

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`Controls/Textarea`, `Controls/DatePicker`,
`Controls/Dialog/ConfirmDialog.vb`, `Controls/Icon/InternalIcons.vb`).

## Le besoin

Saisir des notes de plusieurs lignes, choisir une date (le `DateTimePicker` de
l'ancien HS CART) et demander une confirmation avant d'agir (son
`ConfirmCancel`).

## Ce que fait mkm

- **Textarea** (jetons de `textarea`, ceux des champs) : bordée comme un champ,
  le retrait des champs (6 px sur 7), la bordure au bleu du thème au focus ;
  `autoResize`, `rows`.
- **DatePicker** :
  - **le champ :** celui d'InputText, accolé au bouton du calendrier (30 px, bordé sauf à gauche) ;
  - **le panneau :** 2 px sous le champ, bordé, ombré ; l'en-tête, le mois et l'année entre deux flèches rondes ;
  - **les jours :** des cercles de 24 px, gris clair au survol, aujourd'hui gris, le jour choisi de la couleur principale ;
  - **les autres vues :** l'année et la décennie.
- **ConfirmDialog** : un dialogue, une icône de 24 px devant la question, 12 px
  entre elles ; Non puis Oui, principaux tous deux (les props de mkm
  `rejectProps`, `acceptProps` les distinguent).

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `TextBox` de WPF | `AcceptsReturn`, `TextWrapping`, `MinLines`, `MaxLines`, la barre de défilement | l'apparence, `rows` |
| `DatePicker` et `Calendar` de WPF | la saisie, le format régional, Alt+Bas, les flèches, `BlackoutDates`, `DisplayDateStart`, l'accessibilité | l'apparence de mkm, le texte d'aide (WPF écrit « Select a date ») |
| MahApps.Metro, MaterialDesignInXamlToolkit, HandyControl (MIT) | des `DatePicker` restylés | leur apparence |
| `MessageBox` de WPF | la question oui/non | l'apparence, l'icône de mkm |

## Le choix (technique)

- **`Textarea` hérite de `TextBox`** : `Rows` (2, comme HTML) fixe ses lignes,
  `AutoResize` la fait grandir. Le retrait est posé par le `TextBox` lui-même :
  le gabarit ne le pose pas une seconde fois (la leçon d'InputText).
- **`DatePicker` hérite du `DatePicker` de WPF** ; son `CalendarStyle` est le
  calendrier de mkm, en gabarits du `Calendar`, du `CalendarItem`, des jours
  (`CalendarDayButton`) et des mois (`CalendarButton`).
- **Les icônes des composants de mkm** : le calendrier, les flèches et le crayon
  viennent de la clé `internal` du manifeste de mkm, importée comme les icônes
  publiques (`InternalIcons`).
- **`ConfirmDialog` hérite de `Dialog`** : `Message`, `MessageIcon` (`Icon` est
  celle de la fenêtre), les libellés et les gravités des boutons ;
  `ConfirmDialog.Confirm(fenêtre, question, titre, icône)` l'ouvre et rend vrai
  ou faux. Les textes Oui et Non sont ceux de la bibliothèque (`locale.accept`,
  `locale.reject`), dans la langue choisie.

## Écarts avec mkm

- Le calendrier montre un titre seul (« novembre 2026 ») plutôt que le mois et
  l'année cliquables à part ; les jours des autres mois sont gris pâle.
- Ni heure (`showTime`) ni plage de dates (`selectionMode="range"`) : le
  `DatePicker` de WPF ne les a pas ; à ajouter si un besoin vient.

## La vitrine et les tests

La page « Texte, date, confirmation » montre les zones de texte, les dates et la
demande de confirmation ; les captures montrent le calendrier ouvert et la
demande. Les tests vérifient les lignes de la zone de texte, les parties et le
calendrier du DatePicker, les boutons de la confirmation.
