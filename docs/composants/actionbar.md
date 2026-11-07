# ActionBar

> Fiche écrite **avant** le code (`CLAUDE.md`, « L'open source d'abord »).
> Mourad valide ce qui touche au produit (« À valider ») ; la technique se
> décide ici.

**État :** faite le 07/11/2026 (`src/Mkm.Wpf/Controls/ActionBar`), comme l'ActionBar de mkm. Composant prioritaire.

## Le besoin

Une barre de commandes à la manière de 3DEXPERIENCE : des sections choisies par
des onglets, des commandes en icônes, qui se replie pour libérer la place. La
référence est l'**ActionBar de mkm** (`ui-kit`), écrite par Mourad sur le modèle
de celle de 3DEXPERIENCE : mkm-wpf en est la déclinaison bureau.

## Ce que fait mkm

- **Des sections**, une par onglet ; les onglets ne paraissent que s'il y a
  plusieurs sections, et barre ouverte.
- **Des commandes fixes**, montrées en tête de chaque section.
- **Trois sortes de commandes** :
  - la commande simple ;
  - la **bascule** (enfoncée ou non) ;
  - la **liste** : une commande et sa flèche ; la flèche ouvre la liste de
    ses commandes, et la barre montre la commande cochée, sinon la dernière
    lancée, sinon la première ;
  - et le **séparateur** entre deux groupes.
- **Le repli** : un bouton replie la barre, seul son en-tête reste.
- **Les libellés** sous les icônes, montrés ou cachés par un clic droit sur la
  barre ouverte.
- **Le défilement** : des flèches aux bords dès que la section ne tient plus
  dans la largeur.
- **L'info-bulle** d'une commande : son libellé, et un indice (`hint`), par
  exemple ce qu'il faut pour qu'elle soit disponible.
- **Le clavier** : un seul arrêt de tabulation pour toute la barre (la
  dernière commande qui a eu le focus) ; flèches gauche et droite, Début, Fin ;
  flèche haut ou bas sur une liste pour l'ouvrir ; Entrée ou Espace pour lancer.
  L'info-bulle suit le focus.
- **La place** : en bas de son conteneur, comme dans 3DEXPERIENCE ; les listes
  s'ouvrent au-dessus quand la place le permet.
- **Les mesures** (thème ARA) : onglets de 40 px, texte de 14 px en
  semi-gras ; commandes de 40 px, icônes de 20 px ; avec libellés, des cases
  de 80 × 72 px ; bouton de repli et flèches de 32 px ; panneau à coins de
  6 px ; la rangée apparaît et défile en 200 ms.

## L'état de l'art

| Candidat | Licence | net48 | Vivant | Ce qu'il fait bien | Ce qui manque |
| -------- | ------- | ----- | ------ | ------------------ | ------------- |
| Fluent.Ribbon 11.0.2 (01/2026) | MIT | oui (net462) | oui | onglets, groupes, boutons simples, à bascule, à liste et scindés ; repli du ruban ; raccourcis clavier (KeyTips) ; groupes qui se réduisent avec la largeur | le modèle et l'apparence d'Office (groupes à tailles, Backstage, barre d'accès rapide, fenêtre `RibbonWindow`) : la plier à la barre de 3DEXPERIENCE, c'est lutter contre ses gabarits ; une grosse dépendance pour une barre |
| Ribbon de .NET Framework (`System.Windows.Controls.Ribbon`) | livré avec .NET | oui | figé depuis 2012 | un ruban complet, sans dépendance | l'apparence d'Office 2010, difficile à reprendre ; plus maintenu |
| `ToolBar` et `ToolBarTray` de WPF | livré avec .NET | oui | — | le clavier d'une barre, le débordement, le style des boutons dans une barre | ni onglets, ni repli, ni listes qui retiennent la dernière commande |
| `CommandBar` de WinUI | MIT | non (WinUI) | oui | les libellés sous, à côté ou sans (`DefaultLabelPosition`) : une référence de comportement | pas pour WPF |
| Telerik, DevExpress, Syncfusion | commerciales | — | — | — | exclues |

**Dans WPF :** `Selector` (la section active, comme `TabControl`), `ToolBar` (un
seul arrêt de tabulation, `KeyboardNavigation`), `Button` et `ToggleButton`
avec `ICommand` (une commande indisponible se grise d'elle-même par
`CanExecute`), `ContextMenu` (les libellés), `Popup` (les listes),
`ScrollViewer` (le défilement).

## Le choix (technique)

**Écrire la nôtre, sur le modèle de l'ActionBar de mkm**, avec les briques de
WPF. Fluent.Ribbon est le plus proche, mais son modèle est celui d'Office :
on y regarde comment il fait ses boutons à liste et son repli, sans le prendre
en dépendance. Ce qui en serait repris entrerait dans `ORIGINES.md`.

## L'API prévue

En XAML, comme en WPF : la barre contient ses sections, qui contiennent leurs
commandes. Une application MVVM peut aussi les lier (`ItemsSource`).

```xml
<mkm:ActionBar ActiveSection="edition" ShowLabels="{Binding Libelles}">
    <mkm:ActionBar.FixedItems>
        <mkm:ActionBarButton Label="{mkm:T app.save}" Icon="{StaticResource Enregistrer}"
                             Command="{Binding Enregistrer}" />
    </mkm:ActionBar.FixedItems>
    <mkm:ActionBarSection Key="edition" Header="{mkm:T app.edit}">
        <mkm:ActionBarButton Label="…" Icon="…" Hint="…" Command="…" />
        <mkm:ActionBarToggleButton Label="…" Icon="…" IsChecked="{Binding …}" />
        <mkm:ActionBarSeparator />
        <mkm:ActionBarDropDown>
            <mkm:ActionBarButton … />
            <mkm:ActionBarButton … />
        </mkm:ActionBarDropDown>
    </mkm:ActionBarSection>
</mkm:ActionBar>
```

- **`ActionBar`** (un `Selector`) : `ActiveSection` (la clé de la section
  montrée), `FixedItems`, `IsCollapsed`, `ShowLabels` — toutes liables dans les
  deux sens, si bien que l'application garde l'état si elle le veut.
- **`ActionBarSection`** : `Key`, `Header` (le texte de l'onglet), ses
  commandes.
- **Les commandes** : `ActionBarButton` (`Label`, `Icon`, `Hint`, `Command`,
  `CommandParameter`), `ActionBarToggleButton` (`IsChecked` en plus),
  `ActionBarDropDown` (ses commandes), `ActionBarSeparator`. `Icon` est une
  `ImageSource` : une icône du jeu de mkm-wpf, ou une image de l'application.
- **Parties du gabarit** : les onglets, le bouton de repli, la rangée qui
  défile et ses deux flèches.
- **Ressources du thème** : `Mkm.ActionBar.…`, aux valeurs de l'ActionBar de
  mkm dans Dashboard et ARA.
- **Textes** (en anglais et en français) : replier, déplier, précédent,
  suivant, montrer et cacher les libellés.

## Clavier, accessibilité, performance

Le clavier de mkm (plus haut). Une commande indisponible se grise par son
`ICommand`. Les info-bulles s'ouvrent aussi au focus du clavier (.NET 4.8 le
fait). UI Automation : la barre se présente comme une barre d'outils, une
bascule comme un bouton enfoncé ou non. Les animations (200 ms) passent par
`Storyboard`.

## La vitrine et les tests

**La vitrine :** une barre de démonstration, avec deux sections, des commandes
fixes, une bascule, une liste et un séparateur, et un journal des commandes
lancées. Elle est visible dans les deux thèmes et les deux langues.

**Les tests :**
- le changement de section ;
- la liste qui montre sa dernière commande ;
- `CanExecute` qui grise une commande ;
- le clavier ;
- le repli ;
- les libellés.

## Avant l'ActionBar

Elle demande deux fondations :
- **le thème**, déjà validé : il reste à le coder ;
- **des icônes** : mkm a le jeu Tabler (MIT), voir « À valider ».

## Décidé

mkm a tout tranché (`CLAUDE.md`, « Qui décide ») : l'ActionBar de mkm-wpf
reprend **son comportement au complet**, **son apparence dans chaque thème**,
**sa place en bas de son conteneur** (les listes s'ouvrent au-dessus quand la
place le permet) et **le jeu d'icônes Tabler** (MIT) ; une application peut
aussi donner ses propres images.

## Écarts avec mkm

- Les couleurs changent sans transition (mkm : 200 ms), et la rangée paraît
  sans fondu quand la barre se rouvre.
- Le panneau n'a pas d'ombre : celle de mkm tombe sous le bord du conteneur,
  hors de la vue.
- Le défilement des flèches est immédiat (mkm : défilement doux).

## Sources

- mkm : `ui-kit`, `packages/mkm/src/actionbar` (`ActionBar.vue`,
  `ActionBar.d.ts`), `packages/themes/src/presets/ara/actionbar`.
- Fluent.Ribbon, Wpf-Ui : métadonnées des paquets sur nuget.org, relevées le
  07/11/2026.
