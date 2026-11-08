# Les tests d'interaction

Les tests `Mkm.Wpf.Tests` vérifient les contrôles un par un, dans le code. Les
tests d'interaction (`tests/Mkm.Wpf.UiTests`) font ce que fait un utilisateur :
ils lancent la vitrine, cliquent, survolent, tapent au clavier, puis regardent
ce qui a changé à l'écran. Ils trouvent ce qu'un test de code ne voit pas : un
clic qui ne fait rien, un menu qui ne s'ouvre pas, une touche sans effet, une
erreur à l'affichage.

Ils passent par l'automatisation de l'interface de Windows (UI Automation), la
même que Narrateur et les autres lecteurs d'écran. Un élément qu'un test ne
trouve pas est aussi invisible pour un utilisateur malvoyant. Les tests
vérifient donc aussi l'accessibilité.

La bibliothèque qui pilote la vitrine est [FlaUI](https://github.com/FlaUI/FlaUI)
(licence MIT, voir `ORIGINES.md`).

## Ce qui est vérifié

Les tests pilotent la documentation (`Mkm.Wpf.Showcase`) : chaque composant y a
sa page, et ses exemples vivants sont ce qu'ils manipulent.

- **Chaque page** s'ouvre, avec ses onglets API et Thème ; un exemple qui ne se
  lit pas est une erreur.
- **Chaque commande** d'une page (bouton, case, bouton radio, onglet, lien,
  entrée de menu ou de liste) a un nom pour les lecteurs d'écran.
- **Des scénarios par composant**, rangés comme le menu (`FormulaireTests`,
  `PanneauTests`, `DonneesTests`, `SuperpositionTests`, `MenuTests`,
  `MessagesTests`), par exemple :
  - une commande de l'ActionBar cliquée se lance ; la barre reprend sa largeur
    en revenant à une section courte ; Début et Fin mènent aux bouts ;
  - le bouton « … » d'une Toolbar étroite ouvre les commandes qui débordent ;
  - Ctrl+L lance la commande du registre ;
  - les flèches choisissent le bouton radio voisin ;
  - un clic sur l'en-tête du DataTable trie dans les deux sens ;
  - le bouton d'un nœud de la TreeGrid le replie et le déplie ;
  - le voile du BlockUI refuse le clic ;
  - Entrée accepte le dialogue de confirmation, Échap le refuse ;
  - les boutons d'un InputNumber s'arrêtent à sa borne.
- **Aucune erreur non gérée** : la documentation les écrit dans `erreurs.txt`,
  et un scénario échoue si elle en a écrit une pendant qu'il tournait.

## Les lancer sur un poste Windows

1. Compiler la solution : `dotnet build Mkm.Wpf.sln -c Release`.
2. Lancer les tests :
   `dotnet test tests/Mkm.Wpf.UiTests/Mkm.Wpf.UiTests.vbproj -c Release --no-build`.
3. **Ne plus toucher la souris ni le clavier** jusqu'à la fin, quelques minutes :
   les tests s'en servent. La vitrine s'ouvre en haut à gauche de l'écran, au
   premier plan, en français.

Visual Studio les lance aussi, depuis l'Explorateur de tests, avec les autres
tests de la solution.

## Lire un échec

Pour chaque scénario en échec, le dossier `TestResults/ui` reçoit :

- `<scénario>.png`, une capture de l'écran au moment de l'échec ;
- `<scénario>.txt`, l'arbre d'automatisation de chaque fenêtre de la vitrine,
  une ligne par élément (rôle, nom, identifiant, classe, place) : on y voit ce
  que voyait le test ;
- `erreurs.txt`, les erreurs non gérées de la vitrine, avec leur pile.

Sur la CI, ce dossier est dans l'artefact `resultats-tests`.

## Écrire un scénario

Un scénario hérite de `Scenario`. Il ouvre une page de la documentation par son
identifiant (`OuvrirLaDoc("actionbar")`), puis un exemple par le titre de sa
section (`Exemple(page, "commandes-fixes")`). Il y trouve un élément par son
`x:Name` (`Trouver("Barre", cadre)`) ou par son nom, ce que lit un lecteur
d'écran (`TrouverParNom("Ajouter", ControlType.Button, cadre)`). Les textes de
mkm-wpf (« Fermer », « Plus de commandes ») viennent de ses fichiers `.resx`
français, par leur clé : `T("aria.close")`. Puis il agit :

- `Cliquer`, `Survoler` : à la souris ; la page défile d'abord jusqu'à l'élément ;
- `Taper`, `Raccourci`, `Ecrire` : au clavier.

Il vérifie avec `AttendreQue(condition, "ce qu'on attend")` : l'écran change
après l'action, pas pendant.

Ce qu'un menu, une liste ou un dialogue ouvrent est dans une autre fenêtre :
`TrouverAilleurs`. Un élément qu'un scénario doit retrouver reçoit un `x:Name`
dans l'exemple de la page (`Documentation/Pages/<id>.xml`).
