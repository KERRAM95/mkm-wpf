---
paths:
  - "**/*.xaml"
  - "**/*.vb"
---

# Les pièges de WPF déjà rencontrés

- **Pas de chemin de liaison avec un préfixe** (`{Binding (local:Type.Prop)}`)
  dans la bibliothèque : WPF n'en trouvait pas le préfixe à l'exécution, et la
  liaison échouait sans bruit (les icônes de l'ActionBar). On écrit
  `{TemplateBinding local:Type.Prop}`, ou la liaison en code
  (`New PropertyPath(laPropriete)`). Un test le vérifie.
- **Un `TextBox` pose lui-même son `Padding`** dans sa zone de texte : un gabarit
  ne le pose pas une seconde fois.
- **Le `TabPanel` retranche la marge d'un onglet** de la place qu'il lui donne :
  la marge d'un onglet va dans son gabarit.
- **Une couleur posée sur un `TabItem` passe à son contenu** : elle se pose sur
  le titre, dans le gabarit.
- **Un rayon plus grand que la moitié du côté** (999 px chez mkm) n'est pas
  borné par WPF comme par CSS : il dessine un fuseau ; on le borne en code.
- **`ToolTip` et `ContextMenu` ne se dessinent qu'ouverts** ; `StartupUri` ne
  s'efface pas (`Nothing` lève une exception).
- **Les styles implicites des contrôles de WPF eux-mêmes** (info-bulle, menu)
  vont dans `Themes/Natifs.xaml`, que `MkmResources` fusionne ; ceux des
  contrôles de la bibliothèque, dans `Themes/Generic.xaml`.
- **Un nom de mkm cache celui de WPF** dans l'espace de noms des contrôles :
  `ToggleButton`, `RadioButton`, `Menu`, `Button` y désignent ceux de mkm, même
  là où l'on voulait celui de WPF (un `TryCast(…, ToggleButton)` sur une partie
  de gabarit rend alors `Nothing`, sans bruit). Écrire le nom complet
  (`System.Windows.Controls.Primitives.ToggleButton`) ; `ConventionsTests`
  vérifie le type de chaque partie annoncée d'un gabarit.
- **Une propriété héritée suit le parent logique** avant le parent visuel : posée
  sur le cadre d'une fenêtre flottante ou d'un sous-menu, elle n'atteint pas les
  éléments qui s'y dessinent mais appartiennent à la liste (la commande d'une
  liste de l'ActionBar, l'élément d'un sous-menu). La poser sur chaque élément
  (`PrepareContainerForItemOverride`).
- **Une valeur locale bat les déclencheurs d'un gabarit** qui visent le contrôle
  lui-même (sans `TargetName`) : une couleur posée en code (`SetResourceReference`)
  les rend muets. La faire porter par un élément du gabarit, que les déclencheurs
  visent ; et ne pas écraser une valeur que l'application a posée (`Habillage`).
- **`IsKeyboardFocused` est vrai aussi après un clic** : pour un anneau du focus
  au clavier seulement (`:focus-visible`), `FocusVisible.IsVisible`.
- **Un `MenuItem` survolé prend le focus**, et un `MenuBase` où le focus entre
  passe en mode menu et capture la souris : bon pour un menu qui s'ouvre, mauvais
  pour un menu posé dans la page (un champ perdrait le focus au survol). `Menu` et
  `TieredMenu` refusent le focus venu du survol ; `Menu` ne passe pas en mode menu.
- **Une ligne recyclée du `DataGrid` garde ses cellules** : `GenerateElement`
  n'est pas rappelé pour le nouvel objet, seul le `DataContext` change. Ce que
  la cellule tient de son objet suit `DataContextChanged`.
- **Pas de `MinWidth` lié au `ViewportWidth` d'un `ScrollViewer`** qui le
  contient : c'est un cliquet, le contenu ne rétrécit plus jamais (l'ActionBar
  gardait la largeur de sa plus large section). Le `ScrollViewer` étire déjà son
  contenu à sa largeur visible ; un `HorizontalAlignment="Center"` suffit à
  centrer.

**L'accessibilité (UI Automation)**, relevée par les tests d'interaction :

- **Un `Control`, un `ContentControl` ou un `Panel` n'a pas de pair
  d'automatisation** : invisible aux lecteurs d'écran (et aux tests), son
  `x:Name` et son `AutomationProperties.LiveSetting` avec lui. Tout contrôle de
  mkm-wpf en a un (`PairDeConteneur` et ses rôles).
- **Le pair d'un `ItemsControl` ne montre que ses éléments**, pas le reste de
  son gabarit (les boutons de l'ActionBar, le « … » de la Toolbar) :
  `PairDeConteneur` montre tout l'arbre visuel, et traverse les `ItemsControl`
  simples d'un gabarit.
- **Le pair du `DataGrid` est scellé** (`DataGridAutomationPeer`) : rien du
  gabarit d'un DataTable hors titres et lignes n'y paraît (Header, Footer,
  voile). Ce qui doit être lu passe par une propriété du tableau lui-même
  (`ItemStatus` pendant un chargement).
- **Une ligne de données se nomme par `ToString`** : sans lui, Narrateur lit le
  nom de la classe.
- **Un bouton sans libellé se nomme** : par `AutomationProperties.Name`, ou son
  info-bulle (`PairDeBouton`). Un nom qui dépend d'un état se lie en code
  (`MultiBinding`) : posé par un déclencheur du gabarit, il restait vide quand
  l'état était vrai dès le départ.
- **Une bascule qui replie se dit « développé / réduit »** (`ExpandCollapse`),
  non « activée » : la légende du Fieldset.
