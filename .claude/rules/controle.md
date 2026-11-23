---
paths:
  - "src/Mkm.Wpf/Controls/**"
  - "src/Mkm.Wpf/Themes/**"
  - "apps/Mkm.Wpf.Showcase/Documentation/**"
---

# Écrire un contrôle

On écrit comme WPF écrit : un contrôle sans apparence propre (*lookless*),
dont le gabarit vit dans un style par défaut.

- `src/Mkm.Wpf/Controls/<Nom>/` : la classe (`<Nom>.vb`, espace de noms
  `Mkm.Wpf.Controls`) et son style par défaut (`<Nom>.xaml`), fusionné dans
  `Themes/Generic.xaml` (une ligne par contrôle, ordre alphabétique).
- Le constructeur partagé appelle
  `DefaultStyleKeyProperty.OverrideMetadata(GetType(<Nom>), New FrameworkPropertyMetadata(GetType(<Nom>)))`.
- L'API passe par des `DependencyProperty`, des événements routés et des
  commandes (`ICommand`). Les parties du gabarit que le code emploie sont
  déclarées par `<TemplatePart(Name:="PART_…", Type:=…)>` et prises dans
  `OnApplyTemplate` ; le contrôle reste sûr si une partie manque.
- Les couleurs, tailles, rayons et polices viennent des ressources du thème
  (`DynamicResource`), jamais en dur dans un gabarit. La convention des jetons
  se fixe avec le premier thème.
- Clavier et UI Automation : focus visible, navigation au clavier, un
  `AutomationPeer` quand celui du contrôle de base ne suffit pas.
- Performance : animations par `Storyboard`, jamais de boucle avec
  `Thread.Sleep` ; listes virtualisées.
- Avec le contrôle : sa page de documentation, sa fiche, ses tests de
  comportement, ses scénarios d'interaction, une ligne dans `CHANGELOG.md`. Les
  règles communes (`ConventionsTests`) s'appliquent d'elles-mêmes.

### Sa page de documentation

La vitrine est la documentation de mkm-wpf, sur le modèle d'ui-kit : une page
par composant, `apps/Mkm.Wpf.Showcase/Documentation/Pages/<id>.xml`.

- Ses attributs : `titre`, `categorie` (celle d'ui-kit : Formulaire, Bouton,
  Données, Panneau, Superposition, Menu, Messages, Divers), `types` (les types
  dont l'onglet API donne les membres) et `jetons` (les préfixes des jetons de
  l'onglet Thème).
- Ses sections, dans l'ordre d'ui-kit : Import, Basique, une par usage
  (Gravité, Tailles, Invalide, Grisé…), Accessibilité (lecteur d'écran,
  clavier). Chacune : un texte court (`<p>`, `<c>` pour le code,
  `<lien page="…">`), puis l'exemple vivant (`<exemple>`), dont le XAML est
  aussi le code montré : on ne les écrit qu'une fois.
- Un exemple qui a besoin de code (données, évènements) nomme son comportement
  (`comportement="<Nom>"`) : `Documentation/Exemples/<Nom>.vb`, dont la partie
  entre `#Region "Exemple"` et `#End Region` est montrée sous le XAML.
- Les onglets API et Thème s'écrivent seuls, depuis le code et ses
  commentaires : un commentaire `''' <summary>` manquant y fait un trou.
- `DocumentationTests` ouvre chaque page : un exemple qui ne se lit pas, un
  onglet qui plante ou une commande sans nom fait échouer la CI.

Les noms de l'API (classes, propriétés, événements) sont en anglais, comme
WPF et comme mkm. Les commentaires, la documentation et la vitrine sont en
français. Un texte affiché par un contrôle (« Fermer »…) est une propriété,
dont la valeur par défaut vient des textes traduits de la bibliothèque,
l'anglais en repli (voir « Thème et langue »).
