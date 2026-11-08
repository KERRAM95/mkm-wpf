# Fonctions d'ui-kit absentes de mkm-wpf

Le contrôle qualité de novembre 2026 a comparé chaque composant à ui-kit (rendu, jetons, API, clavier, accessibilité,
pages, tests). Les défauts trouvés sont corrigés (voir le CHANGELOG). Restent les fonctions qu'ui-kit documente et que
mkm-wpf n'a pas encore : chacune demande un vrai travail, et une décision (utile ou non dans les applications de bureau).
Elles sont listées ici, composant par composant ; les écarts voulus (pas de mode sombre, pas de pass-through ni de
Tailwind, pas d'animation d'apparition, gabarits WPF à la place des slots) n'y sont pas.

## Champs

| Composant | Absent |
|---|---|
| InputText, Textarea, Password, InputMask, InputOtp | tailles `small` et `large` (jetons `*.sm.*`, `*.lg.*`), variante `filled` |
| Checkbox, RadioButton | tailles, variante `filled` |
| Select | croix `showClear`, filtre, coche du choix, groupes d'options, tailles, `loading`, `filled` |
| MultiSelect | croix `showClear`, groupes d'options, `selectionLimit`, tailles, `loading`, `filled` |
| AutoComplete | `multiple` (InputChips sans suggestions en tient lieu), `forceSelection`, groupes, gabarit de suggestion, `showClear`, `dropdownMode="current"`, `loading` |
| CascadeSelect, TreeSelect | `showClear` (CascadeSelect), sélection multiple et cases (TreeSelect), filtre, `loading`, tailles |
| DatePicker | plage et dates multiples, heure (`showTime`), barre Aujourd'hui/Effacer, vues mois et année, plusieurs mois, numéros de semaine, calendrier posé dans la page ; le Calendar de WPF montre toujours six semaines |
| InputNumber | boutons verticaux, mode monnaie, icônes des boutons, `showClear` |
| Slider | plage à deux poignées (`range`) |
| IconField | une icône de chaque côté |
| InputChips | ← → entre les puces |
| KeyFilter | `validateOnly` (le texte entier) |

## Boutons et superpositions

| Composant | Absent |
|---|---|
| Button | tailles `small` et `large`, `loading`, badge, position de l'icône, `rounded`, `raised`, gravité `plain` |
| SplitButton | tailles, `rounded`, `raised` |
| SpeedDial | voile (`mask`), `hideOnClickOutside` |
| Dialog | `maximizable`, `position` |
| Drawer | `modal`, `closeOnEscape`, `blockScroll` |
| Popover | `closeOnEscape`, `dismissable` |
| Tooltip | places `left` et `bottom`, délai d'apparition nul ; à gauche, la flèche ne s'inverse pas |
| Message | `Closed` ne dit pas la raison (croix ou fin de vie) |

## Données et panneaux

| Composant | Absent |
|---|---|
| DataTable | filtres, pagination intégrée, pied de colonne, lignes dépliables, colonne de cases « tout choisir », groupement de lignes |
| TreeGrid | cases, menu d'en-tête (épingler, masquer), filtre, glisser-déposer, colonnes typées, état mémorisé, tri sur plusieurs colonnes |
| Tree | sélection multiple, cases, filtre, chargement à la demande, glisser-déposer |
| DataView | paginateur en haut, tri, message vide réglable |
| Paginator | gabarit (ordre des éléments, saut de page), contenus de début et de fin |
| OrganizationChart | sélection multiple, évènements de dépliage |
| Tabs | défilement des onglets (`scrollable`) : en trop, le TabPanel de WPF passe à la ligne |
| IdCard | `loading` (squelettes) ; ses facettes se dessinent en Tabs « Strip » |
| PropertyPanel | `loading` |
| Galleria | plein écran, place des vignettes et des indicateurs, flèches au survol |
| ProgressBar | la valeur écrite dans la barre (`showValue`) : la barre de 6 px est trop fine |

## Hauteur de ligne

ui-kit pose une hauteur de ligne de 1,1667 (14 px pour 12 px) ; WPF garde celle de la police (15 px) : une option de
liste fait 25 px au lieu de 24, un champ à plusieurs lignes un peu plus. À reprendre ensemble, avec une décision sur
`TextBlock.LineHeight` pour toute la bibliothèque.
