---
name: verifier
description: Compiler et vérifier mkm-wpf (Windows, Linux), suivre la CI, regarder les captures de la vitrine. À utiliser avant chaque poussée et après, jusqu'à la CI verte.
---

# Vérifier mkm-wpf

**Sous Windows (référence)** : ouvrir `Mkm.Wpf.sln` dans Visual Studio 2022,
compiler, lancer la vitrine et les tests ; ou, à la racine, `dotnet build` puis
`dotnet test`.

**La CI** (`.github/workflows/ci.yml`) compile et lance les tests sous
Windows à chaque poussée sur `main`. Un runner Windows consomme deux fois plus
de minutes GitHub Actions qu'un runner Linux sur un dépôt privé.

**Les captures** : la CI lance aussi `Mkm.Wpf.Showcase.exe --captures <dossier>`
(`apps/Mkm.Wpf.Showcase/Captures.vb`) et publie l'artefact `captures` : chaque page
de la documentation dans chaque thème, la fenêtre, des notifications et des dialogues,
en PNG à 96 ppp. Une session sans Windows les télécharge
(`https://api.github.com/repos/KERRAM95/mkm-wpf/actions/artifacts/<id>/zip`) pour
regarder le rendu ; les états de survol n'y sont pas.

**Sous Linux** (une session sans Windows) : `outils/linux/verifier.sh`
compile toute la solution, code VB et XAML compris. Il lui faut le SDK .NET
de Microsoft — celui d'Ubuntu n'a pas la partie WPF ; le script l'installe
dans un cache s'il manque. Il n'exécute rien : la vitrine et les tests ne
tournent que sous Windows.

**Après la poussée** : suivre l'exécution de `ci.yml` (dépôt `KERRAM95/mkm-wpf`, branche `main`) jusqu'à sa fin ; en
échec, lire le journal du job, corriger, repousser. Seuls les artefacts de la dernière exécution restent (le stockage
GitHub est limité) : télécharger les captures avant l'exécution suivante si on veut les garder.
