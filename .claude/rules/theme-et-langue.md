---
paths:
  - "src/**"
  - "apps/**"
  - "tests/**"
  - "outils/themes/**"
  - "outils/icones/**"
  - "docs/fondations/**"
---

# Thème, langue et icônes : comment c'est fait

La langue, telle qu'elle est faite (`src/Mkm.Wpf/Localization`) :

- `<mkm:MkmResources Language="fr" />` dans `App.xaml` : la configuration ;
  sans `Language`, la langue d'affichage de Windows. Elle donne aussi aux
  éléments WPF le **format régional de Windows** pour les dates et les nombres
  (WPF écrirait sinon à l'américaine) ; changer de langue ne change pas ce
  format, pour qu'une valeur saisie (`2,5` kg) ne change pas de sens.
- `Localizer.Current` : la langue (`Language`, à chaud), les langues proposées
  (`Languages`), les sources (`AddSource`), et `T(clé, paramètres)` en code.
- `{mkm:T clé}` en XAML : un texte qui suit la langue.
- Les sources : `ResxTextSource` (fichiers `.resx`) et `TextTable` (une table
  que l'application remplit depuis ses propres fichiers). Celles de
  l'application passent avant celles de la bibliothèque.
- Un texte absent : sa langue parente, puis l'anglais, puis sa clé.

Le thème, tel qu'il est fait (`src/Mkm.Wpf/Theming`) :

- `<mkm:MkmResources Theme="Ara" />` ; sans `Theme`, Dashboard. `Themes`
  connaît Dashboard, ARA et les thèmes qu'une application enregistre
  (`Themes.Register`, souvent par `Themes.Dashboard.Derive(…)`).
- Une ressource par jeton, sous `mkm.<chemin de dt() dans mkm>` :
  `{DynamicResource mkm.actionbar.item.size}`. Un gabarit ne lit **que** ces
  ressources, et **toujours par `DynamicResource`** : jamais une couleur ni une
  mesure en dur.
- Les jetons viennent de mkm : `node outils/themes/importer-mkm.mjs <ui-kit>
  [composant…]` régénère `src/Mkm.Wpf/Theming/Tokens/*.tokens` (base, et les
  composants de sa liste). Un composant nouveau ajoute son nom à la liste de
  l'outil et relance l'import ; on ne retouche jamais un `.tokens` importé à la
  main.
- Ce que la feuille CSS d'un preset de mkm (`css`) dit sans jeton s'écrit à la
  main, par thème, dans `Theming/Tokens/<thème>.css.tokens`, lu après les
  jetons importés : aux valeurs de mkm, avec la teinte que chaque thème y met
  (`dsblue.100` dans Dashboard, `primary.100` dans ARA).
- `ThemesTests` vérifie, comme mkm : chaque renvoi résolu, ARA qui ne diffère
  de Dashboard que par ses couleurs, chaque texte lisible sur son fond
  (4,5:1) ; un composant ajoute ses paires texte/fond à la liste du test.

Les icônes (`src/Mkm.Wpf/Controls/Icon`) : celles de mkm, nommées par leur
sens (`{x:Static mkm:Icons.Add}`), dessinées par Tabler. `<mkm:Icon
Source="…" />` trace une géométrie à la couleur du texte, au trait fin de mkm
(1,5 sur la grille de 24, 1 px à 16 px), ou dessine une image de
l'application. Une icône de plus : l'ajouter au manifeste de mkm, puis
`node outils/icones/importer-tabler.mjs <ui-kit>` ; on ne retouche jamais
`Icons.g.vb` à la main.

Les règles pour chaque composant :

- aucun texte en dur : une clé dans `src/Mkm.Wpf/Localization/Strings.resx`
  (anglais) et sa traduction dans `Strings.fr.resx` — le premier composant qui
  affiche un texte les crée, avec la source de la bibliothèque
  (`AddLibrarySource`) et le test qui compare leurs clés
  (`ResxTextSourceTests.ClesSansTraduction`) ;
- les clés sont en anglais, en `camelCase` séparé par des points, préfixées
  par le composant : `actionBar.more` ;
- un texte que l'application voudrait changer est aussi une propriété du
  contrôle, qui l'emporte sur la ressource ;
- aucune largeur fixe pour un élément qui porte un texte : le français est
  20 à 30 % plus long que l'anglais ;
- aucune phrase construite par morceaux : un texte à paramètres (`{count}`) ;
- la vitrine a le choix de la langue dans sa barre (et aura celui du thème) :
  chaque composant s'y voit dans les deux langues ;
- l'écriture de droite à gauche n'est pas visée.
