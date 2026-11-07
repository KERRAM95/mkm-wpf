# Les retours : Message, Toast et Dialog

> Fiche écrite avant le code (`CLAUDE.md`, « L'open source d'abord »). mkm a
> tranché l'apparence et le comportement : la fiche le constate.

**État :** faits le 07/11/2026 (`src/Mkm.Wpf/Controls/Message`,
`src/Mkm.Wpf/Controls/Toast`, `src/Mkm.Wpf/Controls/Dialog`).

## Le besoin

Dire à l'utilisateur ce qui se passe : un message dans la page (un plan en
lecture seule, une erreur de saisie), une notification qui passe (« Enregistré »),
et une fenêtre qui pose une question ou demande une saisie.

## Ce que fait mkm

- **Message** (presets `message`) :
  - **l'apparence :** un fond pâle de la couleur de sa gravité (7 %), un filet un peu plus soutenu (35 %), des coins de 8 px ; le texte reste gris foncé ;
  - **l'icône :** elle prend la couleur de la gravité assombrie (80 % et du noir), lisible à plus de 3:1 ;
  - **la mise en page :** l'icône (16 px), le texte (lignes de 18 px) et la croix (24 px) à 10 px les uns des autres, en haut ;
  - **les gravités :** `info`, `success`, `warn`, `error`, `secondary`, `contrast` ;
  - **les formes :** `outlined`, `simple` ;
  - **les tailles :** `small`, `large` ;
  - **la fermeture :** `closable`, et `life` qui le ferme seul.
- **Toast**, les notifications de 3DEXPERIENCE (presets `toast`) :
  - **la place :** en haut à droite, des cartes de 250 px à 7 px les unes des autres, 200 px de haut au plus (au-delà, le message défile) ;
  - **la carte :** sans coins, à l'ombre courte, le fond pâle de leur gravité et une bande de 5 px de sa couleur à gauche ; l'icône de 22 px de cette couleur ;
  - **le texte :** le titre en 14 px gras, le message en 12 px ;
  - **la croix :** de 12 px, grise, sans anneau ;
  - **ce que mkm ajoute à PrimeVue :** `highlightDuration` (la carte en relief à son apparition et sous le pointeur), `fadeOutDuration` (elle s'efface à la fin de sa vie, puis se replie), `lifeOnLeave` (la part de sa vie qu'elle retrouve quand le pointeur la quitte), et `prepend` (une carte devant les autres).
- **Dialog**, la fenêtre de 3DEXPERIENCE (presets `dialog`) :
  - **la fenêtre :** des coins de 6 px sous une ombre courte ;
  - **la barre de titre :** 26 px, le titre de 14 px gras coupé et fondu au blanc ; la croix, un carré de 24 px sans fond ;
  - **le contenu :** 10 px de retrait ;
  - **le pied :** gris, sous son trait, les boutons à droite, 10 px entre eux ;
  - **la poignée :** une équerre au coin.

## L'état de l'art

| Candidat | Ce qu'il fait bien | Ce qui manque |
| -------- | ------------------ | ------------- |
| `Window` de WPF | une vraie fenêtre : `ShowDialog` modal, `DialogResult`, `IsDefault`/`IsCancel`, propriétaire désactivé, déplacement (`DragMove`) | l'apparence de mkm (une fenêtre sans bord se dessine elle-même) |
| `MessageBox` de WPF | la question simple | l'apparence, rien d'autre que du texte |
| MahApps.Metro (MIT) | `MetroWindow` (fenêtre sans bord, `WindowChrome`), des dialogues dans la fenêtre (`DialogCoordinator`) | leur apparence ; un dialogue dans la fenêtre ne rend pas la main comme `ShowDialog` |
| MaterialDesignInXamlToolkit (MIT) | `DialogHost` (un dialogue posé sur la fenêtre), `Snackbar` (une notification en bas) | idem |
| HandyControl (MIT) | `Growl` (des notifications empilées, une durée), `Dialog` | leur apparence ; ni relief ni fondu de mkm |
| Notification.Wpf, ToastNotifications (MIT) | des notifications empilées, des positions, une durée | leur apparence, les règles de vie de mkm |

## Le choix (technique)

- **`Message` hérite de `ContentControl`** : son texte est son contenu. Les
  couleurs, les mesures et la taille viennent des jetons de mkm selon la gravité,
  la forme et la taille (`SetResourceReference`, à chaud).
- **`Toast` hérite d'`ItemsControl`** : ses cartes, des `ToastMessage`, sont
  ses éléments. On le pose une fois au-dessus de la fenêtre ; `Add`, `Remove`,
  `RemoveAll` et l'événement `MessageClosed` (avec la raison : croix ou fin de
  vie) remplacent le service de PrimeVue. La vie, le relief, le fondu et le
  repli suivent le code de mkm (`ToastMessage.vue`) : le pointeur arrête la vie,
  et la rend en le quittant.
- **`Dialog` hérite de `Window`** : une vraie fenêtre, pas un calque dans la
  fenêtre. `ShowDialog` la rend modale et rend `DialogResult` ; Échap la ferme
  (`CloseOnEscape`), comme un bouton `IsCancel` ; elle s'ouvre au milieu de la
  fenêtre principale, hors de la barre des tâches, à la taille de son contenu.
  Sans bord (`WindowStyle.None`) et transparente autour de son dessin
  (`AllowsTransparency`), elle dessine ses coins et son ombre comme mkm. On la
  déplace par sa barre ; `ResizeMode="CanResize"` lui donne la poignée du coin.
- **La gravité est commune** au message et à la notification
  (`MessageSeverity`).
- **Les icônes par défaut de la notification** sont celles de mkm : info,
  coche, triangle d'avertissement, cercle barré (`Icons.Info`, `Check`,
  `Warning`, `Error`).
- **Ce que la feuille CSS de mkm dit sans jeton** (lignes de 18 px, place de la
  croix, couleurs de l'icône, bande, marges, relief, croix grise, barre de
  titre, pied, poignée) est écrit dans `Theming/Tokens/<thème>.css.tokens`.
  `dialog.window.padding` est une mesure de mkm-wpf : la place de l'ombre autour
  de la fenêtre.

## L'API

- **Message :** `Severity` (`MessageSeverity`), `Variant` (`MessageVariant` :
  `Filled`, `Outlined`, `Simple`), `Size` (`MessageSize`), `Icon`, `Closable`,
  `Life` ; `Close()` et l'événement routé `Closed`. Partie `PART_CloseButton`.
- **Toast :** `Position` (`ToastPosition`, `TopRight`), `HighlightDuration`,
  `FadeOutDuration`, `LifeOnLeave` ; `Add(message, prepend)`,
  `Add(gravité, titre, message, vie)`, `Remove`, `RemoveAll` ; `MessageClosed`.
- **ToastMessage :** `Severity`, `Summary`, `Detail`, `Icon`, `Life`,
  `Closable` ; en lecture seule `DisplayIcon`, `IsHighlighted` ; `Close()`.
- **Dialog :** `Title`, `Content`, `Footer`, `Closable`, `CloseOnEscape`,
  `Draggable`, et tout `Window`. Parties `PART_Header`, `PART_CloseButton`,
  `PART_ResizeGrip`.

## Clavier, accessibilité, performance

- Le message et la notification sont annoncés dès qu'ils paraissent
  (`AutomationProperties.LiveSetting` assertif, comme le rôle « alert » de mkm).
- Leur croix et celle du dialogue se nomment « Fermer » (texte de la
  bibliothèque, `aria.close`).
- Le dialogue : Entrée pour le bouton `IsDefault`, Échap pour fermer, Tab qui
  reste dans la fenêtre.
- Les minuteries de vie s'arrêtent quand la carte se ferme.

## Écarts avec mkm

- Le message se ferme sans transition ; une notification paraît et se ferme
  sans animation (comme chez mkm), mais son fondu et son repli suivent
  `FadeOutDuration` plutôt qu'une seconde fixe.
- Le dialogue n'a pas de masque sur la fenêtre propriétaire : Windows la
  désactive déjà.
- Le relief d'une notification n'a pas le filet intérieur de mkm en ombre
  (`inset`) : c'est une bordure d'un pixel.

## La vitrine et les tests

La page « Messages et dialogues » montre chaque gravité, chaque forme et chaque
taille du message, un message qui se ferme seul, des notifications de chaque
gravité (5 s, relief et fondu d'une seconde), et deux dialogues (un renommage
modal qui rend sa réponse, un dialogue étirable au titre trop long). Les tests
vérifient les couleurs et les mesures du message, sa fermeture, l'ajout devant
et derrière des notifications et leur fermeture, l'icône de la gravité, et les
parties du dialogue avec et sans pied.
