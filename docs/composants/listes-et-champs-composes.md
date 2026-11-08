# Listes et champs composés : Listbox, MultiSelect, AutoComplete, IconField, InputGroup

> Fiche du lot 14, priorité 1 (le dernier lot de cette priorité).

**État :** livrée dans la version en préparation.

## Le besoin

- **Une liste toujours visible** pour choisir une ou plusieurs valeurs, avec un
  filtre quand elle est longue (Listbox).
- **Plusieurs valeurs dans un champ** : les maturités d'une recherche, les types
  de documents d'un export (MultiSelect).
- **Retrouver une référence parmi des milliers** en tapant son début
  (AutoComplete).
- **Un champ et son contexte** : une loupe, une unité, un préfixe, un bouton
  accolé (IconField, InputGroup).

## Ce que fait mkm

| Composant    | Dans mkm                                                                                          |
| ------------ | ------------------------------------------------------------------------------------------------- |
| Listbox      | un cadre de champ, des options de 5 px sur 8, grisées au survol, choisies sur le fond du thème ; coche, lignes alternées, filtre, groupes, message de liste vide |
| MultiSelect  | un champ (le preset dashboard ne le redessine pas : celui de Nora), les choix séparés de virgules ou en puces, « n éléments sélectionnés » ; la liste dessous, « tout sélectionner », filtre, une case devant chaque option |
| AutoComplete | un champ InputText (celui de 3DEXPERIENCE dans dashboard) ; `complete` demande les suggestions à l'application ; ↓ ↑ Entrée ; un bouton accolé (dropdown) |
| IconField    | l'icône à 7 px du bord, le texte reculé de 30 px                                                  |
| InputGroup   | des champs, des compléments et des boutons accolés, les coins aux deux bouts, les compléments bordés en haut et en bas |

## Le choix

- **Listbox** hérite du `ListBox` de WPF : `Checkmark`, `Striped`, `Filter`,
  `FilterText`, `FilterPlaceholder`, `EmptyMessage`, `Invalid` ; plusieurs
  choix par `SelectionMode="Multiple"`. Les groupes d'une vue groupée ont
  l'en-tête de mkm.
- **MultiSelect** hérite de Listbox : sa liste s'ouvre sous un champ, comme un
  `ComboBox` ; `Placeholder`, `Display` (Comma, Chip), `MaxSelectedLabels`,
  `ShowToggleAll`, `IsDropDownOpen` ; `SelectionText` est le texte du champ.
- **AutoComplete** hérite d'`InputText` : `Suggestions`, `MinLength`, `Delay`,
  `DisplayMemberPath`, `Dropdown`, `SelectedItem`, les évènements `Complete`
  et `ItemSelect`, `Search()` et `SelectItem()`.
- **IconField** hérite de `ContentControl` : `Icon`, `IconPosition` ; il recule
  le texte de son champ, sauf retrait posé par l'application.
- **InputGroup** est un `ItemsControl` et **InputGroupAddon** un
  `ContentControl` ; le groupe pose les coins de ses éléments (`CornerRadius`,
  qu'InputText, Button, Chip et InputGroupAddon ont tous) et les bordures de
  ses compléments.

Les retraits, coins et bordures que mkm calcule (`calc()`) ou écrit côté par
côté deviennent des jetons dans les feuilles `.css.tokens` ; la hauteur des
listes (14rem chez mkm) y est en pixels de Dashboard.

« Listbox » est aussi le nom du `ListBox` de WPF : la règle de `CLAUDE.md`
s'applique (le SelectButton écrit désormais `System.Windows.Controls.ListBox`).

## Écarts avec mkm

- AutoComplete : pas de choix multiples en puces (`multiple`) ; la suggestion
  surlignée aux flèches prend le gris du survol, celle déjà choisie n'est pas
  marquée.
- MultiSelect : les choix se montrent dans l'ordre où ils ont été faits, comme
  la valeur de mkm.

## Clavier, accessibilité, performance

- Listbox : une liste, son champ de filtre exposé devant ses options (le pair
  d'un ListBox de WPF n'expose que ses options) ; ↓ dans le filtre entre dans
  la liste.
- MultiSelect : une liste déroulante (ComboBox) qui dit son texte (Value) et
  se dit développée ou réduite ; ↓, Entrée, Espace ouvrent ; Échap et Tab
  ferment ; la case de tête se nomme « Tout sélectionner ».
- AutoComplete : un champ de texte ; le clavier reste au champ pendant que ↓ ↑
  parcourent les suggestions.
- IconField et InputGroup : l'icône et les compléments ne se lisent pas comme
  des champs ; nommez le champ.

## La documentation et les tests

Les pages Listbox, MultiSelect, AutoComplete, IconField et InputGroup ;
`Lot14Tests` ; `FormulaireTests` (un choix et le filtre du Listbox ; deux choix
du MultiSelect au clic et au clavier ; une suggestion choisie au clavier).
