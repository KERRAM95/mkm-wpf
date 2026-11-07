# Thème et langue

> Fiche d'une fondation : elle règle ce dont tous les composants dépendent.
> Écrite avant le code (`CLAUDE.md`, « L'open source d'abord ») ; Mourad la
> valide avant le développement.

**État :** à l'étude — proposition du 07/11/2026.

## Le besoin

Une application qui emploie mkm-wpf choisit **son thème et sa langue par une
variable chacun**, au démarrage, et peut les changer pendant qu'elle tourne :
comme le fait mkm. Pour la refonte de HS CART, la langue est une exigence au
même titre que le thème : sans elle, la refonte n'a pas de sens (Mourad, le
07/11/2026).

Tous les composants en dépendent — leurs couleurs, leurs textes, la largeur de
leurs boutons — : la décision précède le premier composant.

## Ce que fait mkm

| Sujet  | Au démarrage                                   | Pendant l'exécution                                     |
| ------ | ---------------------------------------------- | ------------------------------------------------------- |
| Thème  | `app.use(Mkm, { theme: { preset: Preset } })`  | `usePreset(…)` : toute l'interface change aussitôt      |
| Langue | `useI18n(messages)` : une table par langue     | la langue de la personne est suivie (`onLanguageChange`) |

Les textes de mkm (`@mkm/platform`, `i18n`) : une table par langue, l'anglais en
repli ; `t(clé, paramètres)` remplace `{count}` ou `{0}` ; `formatNumber` et
`formatDate` écrivent dans la langue de la personne ; une clé absente partout
s'affiche telle quelle. Les textes propres aux composants viennent de la
configuration (`locale`), en anglais par défaut, et l'application les remplace.

## L'état de l'art

### Le thème

| Bibliothèque                        | Poser le thème                                         | Le changer pendant l'exécution                   | Licence | net48 |
| ----------------------------------- | ------------------------------------------------------ | ------------------------------------------------ | ------- | ----- |
| MaterialDesignThemes 5.4.0 (10/2026) | `<materialDesign:BundledTheme BaseTheme="…" PrimaryColor="…"/>` dans `App.xaml` | `PaletteHelper` : `GetTheme`, puis `SetTheme`     | MIT     | oui (net462) |
| WPF UI 4.3.0 (05/2026)               | `<ui:ThemesDictionary Theme="Light"/>` dans `App.xaml` | `ApplicationThemeManager.Apply(…)`               | MIT     | oui (net462, net472) |
| ControlzEx 7.0.4 (04/2026), MahApps  | un dictionnaire par thème                              | `ThemeManager.Current.ChangeTheme(app, "Light.Blue")` | MIT | oui (net462) |
| HandyControl 3.5.1 (03/2024)         | un dictionnaire par thème                              | remplacement du dictionnaire                     | MIT     | oui   |

**Toutes emploient la même technique, celle de WPF lui-même** : les gabarits
lisent leurs couleurs et leurs mesures par `DynamicResource` ; le thème est un
dictionnaire de ressources fusionné dans `Application.Resources` ; le
remplacer met à jour toute l'interface, sans redémarrer.

**Ce qu'on retient :** cette technique, sans paquet ; et la forme de
MaterialDesign et de WPF UI — un dictionnaire à propriétés posé dans
`App.xaml`, qui est notre « simple variable ».

### La langue

| Solution                                   | Principe                                                                                       | Changement à chaud | Licence | Ce qui manque                                        |
| ------------------------------------------ | ---------------------------------------------------------------------------------------------- | ------------------ | ------- | ---------------------------------------------------- |
| `.resx` de .NET                            | un fichier de textes par langue (`Strings.resx`, `Strings.fr.resx`), repli d'une langue à l'autre par .NET | non, seul          | —       | `{x:Static}` fige le texte au chargement             |
| WPFLocalizeExtension 3.10.0 (02/2023)      | `.resx` et `{lex:Loc Clé}` ; `LocalizeDictionary.Instance.Culture`                             | oui                | MS-PL   | licence à copyleft faible ; aucune version depuis 2023 |
| HandyControl 3.5.1 (03/2024)               | `.resx` et une source de textes liée (`LangProvider`) ; `ConfigHelper.Instance.SetLang("fr")`  | oui                | MIT     | interne à HandyControl : un principe, pas un paquet  |
| Antelcat.I18N.WPF 1.1.0 (03/2025)          | `.resx` et `{I18N {x:Static …}}` ; `I18NExtension.Culture`                                     | oui                | MIT     | clés produites par un générateur de code pour C# ; son emploi depuis VB n'est pas documenté ; petit projet |
| Dictionnaires de textes en XAML            | un dictionnaire de `sys:String` par langue, remplacé comme un thème ; `{DynamicResource}`       | oui                | —       | pas de paramètres, clés non vérifiées, traductions hors `.resx` |

**Le principe commun** des trois bibliothèques : les textes dans des `.resx`,
et une **extension de balisage** liée à une source qui prévient de chaque
changement de langue ; tous les textes liés se mettent à jour.

**Ce qu'on retient :** ce principe, écrit chez nous en quelques dizaines de
lignes VB. Aucune des trois ne convient en dépendance : licence et maintenance
pour WPFLocalizeExtension, code interne pour HandyControl, générateur pour C#
pour Antelcat. Les `.resx` gardent le format standard de .NET, ses outils
(l'éditeur de Visual Studio, ResXManager pour éditer les langues côte à côte)
et son repli d'une langue à l'autre.

## Le choix proposé

### Une configuration, deux variables

Comme `app.use(Mkm, …)` dans mkm, une seule entrée, dans `App.xaml` :

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <mkm:MkmResources Theme="Ara" Language="fr" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Et pendant l'exécution, une ligne de code change l'une ou l'autre ; toute
l'interface suit, sans redémarrer. Les noms (`MkmResources`, et celui de
l'objet de configuration en code) se fixent avec le code.

### Le thème

- Le thème est un dictionnaire de **jetons** (couleurs, mesures, polices) que
  `Theme` remplace ; les gabarits ne lisent que ces jetons, par
  `DynamicResource`.
- La structure des jetons (primitifs, sémantiques, par composant, comme mkm)
  et les thèmes eux-mêmes (un thème par défaut, ARA) : la fiche du thème.

### La langue

- Les textes sont dans des `.resx` : `Strings.resx` en anglais, la langue de
  repli, et `Strings.fr.resx`. La bibliothèque a les siens ; l'application,
  les siens.
- En XAML : `Text="{mkm:T Close}"`. Le texte suit la langue.
- En code : `T(clé, paramètres)`, avec des paramètres nommés (`{count}`), comme
  mkm.
- **L'application passe avant la bibliothèque** : un texte de l'application
  remplace celui d'un composant de même clé, comme le `locale` de mkm.
- **Une clé absente s'affiche telle quelle**, repère visible d'un oubli ; et
  un test compare les clés de chaque langue, si bien qu'une traduction oubliée
  fait échouer la compilation en CI.
- **Dates et nombres** : changer de langue change aussi la culture de
  l'application et la langue (`Language`) de chaque fenêtre. Sans cela, WPF
  écrit les dates et les nombres à l'américaine, même sur un Windows français.
- **Les langues** : l'anglais et le français.

### Qui choisit la langue

L'application, jamais la bibliothèque. Pour HS CART, la proposition :
- un réglage de l'utilisateur ;
- par défaut, la langue d'affichage de Windows ; l'anglais si elle n'est pas
  prise en charge.

mkm suit la langue de la personne dans 3DEXPERIENCE. Pour HS CART, suivre sa
langue 3DEXPERIENCE ou celle de CATIA serait cohérent : c'est à étudier avec
`hscart-next`, selon ce que ces logiciels permettent de lire.

## Les règles pour chaque composant

Une fois cette fiche validée, elles entrent dans `CLAUDE.md` :

- aucun texte en dur : une clé dans `Strings.resx`, sa traduction dans
  `Strings.fr.resx` ;
- un texte que l'application voudrait changer est aussi une propriété du
  contrôle, qui l'emporte sur la ressource ;
- aucune largeur fixe pour un élément qui porte un texte : le français est
  20 à 30 % plus long que l'anglais ;
- aucune phrase construite par morceaux : un texte à paramètres (l'ordre des
  mots change d'une langue à l'autre) ;
- la vitrine a un choix du thème et un choix de la langue dans sa barre : chaque
  composant s'y voit dans tous les thèmes et dans les deux langues ;
- l'écriture de droite à gauche n'est pas visée.

## Pour HS CART

- Les quelque 380 textes écrits aujourd'hui dans le code, en français mêlé
  d'anglais, deviennent des clés : l'inventaire se fait au début de la refonte.
- Les journaux restent dans une seule langue : ils servent au support, pas à
  l'utilisateur.
- Les données qui viennent de 3DEXPERIENCE ou de CATIA (noms d'attributs,
  états de maturité) s'affichent telles que la plateforme les donne.

## À valider

1. Le principe : une configuration, deux variables, changeables à chaud.
2. La langue : `.resx` et notre extension `{mkm:T}`, plutôt qu'un paquet.
3. Les langues : anglais et français.
4. Pour HS CART : la langue de Windows par défaut, modifiable dans les
   réglages — ou celle de 3DEXPERIENCE, après étude.

## Sources

- [MaterialDesignInXamlToolkit, *Advanced Theming*](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki/Advanced-Theming)
- [WPF UI (lepoco/wpfui)](https://github.com/lepoco/wpfui)
- [HandyControl, localisation](https://deepwiki.com/HandyOrg/HandyControl/4.2-localization) ;
  [changement de langue](https://ghost1372.github.io/handycontrol/langs)
- [Antelcat.I18N](https://awesome.ecosyste.ms/projects/github.com%2FAntelcat%2FI18N) ;
  [Antelcat.I18N.SourceGenerators sur NuGet](https://www.nuget.org/packages/Antelcat.I18N.SourceGenerators)
- [WPFLocalizeExtension](https://kandi.openweaver.com/csharp/XAMLMarkupExtensions/WPFLocalizeExtension)
- Versions, dates et licences : les métadonnées des paquets sur nuget.org,
  relevées le 07/11/2026.
- mkm : `ui-kit`, `packages/platform/src/i18n`, `packages/styled` (`usePreset`),
  `templates/widget/src` (`main.ts`, `i18n.ts`, `theme.ts`).
