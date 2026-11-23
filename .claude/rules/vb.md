---
paths:
  - "**/*.vb"
  - "**/*.vbproj"
  - "**/*.props"
---

# Conventions VB

- **`RootNamespace` est vide dans chaque projet** : chaque fichier écrit son
  espace de noms en entier (`Namespace Mkm.Wpf.Controls`), comme en C#. Un
  `x:Class` donne ainsi le nom complet de sa classe, et un code porté depuis
  C# garde ses espaces de noms. (Le SDK pose sa valeur par défaut après
  `Directory.Build.props` : c'est pourquoi la ligne est dans chaque projet.)
- Chaque espace de noms public de `Mkm.Wpf` est associé à
  `xmlns:mkm="urn:mkm-wpf"` par un `XmlnsDefinition` dans
  `src/Mkm.Wpf/AssemblyInfo.vb`, **dès qu'il a un type** : le compilateur XAML
  d'une application refuse un espace de noms vide.
- VB nomme une ressource `.resx` par le seul nom de son fichier, sans son
  dossier (`Textes/Vitrine.resx` → `"Vitrine"`) : deux `.resx` d'un même projet
  ne portent jamais le même nom.
- Les `Imports` sont écrits en tête de chaque fichier ; on n'ajoute pas
  d'imports au niveau du projet.
- Champs privés en `_camelCase` ; types, membres et constantes en PascalCase.
- NUnit : `Is` est un mot-clé de VB, on écrit `[Is]`
  (`Assert.That(x, [Is].EqualTo(1))`) ; une exception s'attend par
  `Assert.Throws(Of T)(New Action(Sub() …))` (sans `New Action`, VB ne choisit
  pas entre les surcharges).
- Une variable locale ne porte pas le nom d'un type (`Dim traducteur As New
  Localizer`, pas `localizer`) : VB ne distingue pas la casse.
- **Les noms de mkm qui sont aussi ceux de WPF** (`Button`, `Checkbox` et
  `CheckBox`, `Select`, `Tab`, `Menu`, `Toolbar`, `ToggleButton`,
  `RadioButton`, `Panel`, `Listbox` et `ListBox`…) : `Panel` surtout, que les panneaux de mise en page
  de la bibliothèque héritent de WPF (`Inherits System.Windows.Controls.Panel`,
  `System.Windows.Controls.Panel.SetZIndex`). Un contrôle écrit son type de base en entier
  (`Inherits System.Windows.Controls.Button`), une partie de gabarit aussi
  (`Inherits System.Windows.Controls.Primitives.ToggleButton` : sinon elle
  hérite du contrôle de mkm et de son style, ce que refuse le test
  `ChaquePartieInterne_NHeriteDUnControleDeMkmQueVoulu`) ; un fichier qui importe les deux
  espaces de noms écrit `Mkm.Wpf.Controls.Button` (la casse ne les distingue
  pas). Les mots-clés de VB se mettent entre crochets (`[Select]`, `[Variant]`,
  `[Error]`, `[Structure]`). En XAML, le préfixe suffit (`mkm:Button`).
- Un `Byte` multiplié par un `Byte` reste un `Byte` en VB et déborde : on
  convertit d'abord (`CDbl(a) * b`).
