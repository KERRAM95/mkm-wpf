# Saisies protégées et filtrées : Password, InputMask, InputOtp, KeyFilter

> Fiche du lot 19, priorité 3.

**État :** livrée dans la version en préparation.

## Le besoin

- **Un mot de passe** : masqué, montrable, avec sa force (Password).
- **Une saisie au format** : un téléphone, une date, une référence
  « HS-00042 » (InputMask).
- **Un code reçu** : de vérification, à quelques caractères (InputOtp).
- **Refuser des caractères** : des chiffres seulement dans une quantité
  (KeyFilter).

## Ce que fait mkm

| Composant | Dans mkm                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------- |
| Password  | le champ d'InputText masqué ; `toggleMask` (l'œil) ; `feedback` : sous le champ, une jauge de 9 px, rouge au tiers, jaune aux deux tiers, verte en entier, et son texte ; `mediumRegex`, `strongRegex`, les quatre textes |
| InputMask | un champ qui suit `mask` (`9`, `a`, `*`, `?`), `slotChar`, `autoClear`, `unmask` |
| InputOtp  | des cases de 30 px à 6 px l'une de l'autre ; `length`, `mask`, `integerOnly`, `readonly` |
| KeyFilter | la directive `v-keyfilter` : `pint`, `int`, `pnum`, `money`, `num`, `hex`, `email`, `alpha`, `alphanum`, ou une expression |

## Le choix

- **Password** hérite de `Control` : un `PasswordBox` habillé comme
  InputText, et une InputText pour le texte en clair. Comme le `PasswordBox`
  de WPF, le mot de passe (`Password`) n'est pas liable ; `PasswordChanged`
  dit qu'il change. `Strength` et `StrengthText` disent la force.
- **InputMask** hérite d'InputText, avec l'algorithme de mkm (jQuery Masked
  Input) : cases, décalages, collage, coupe. `Text` est le texte montré,
  `UnmaskedValue` les seuls caractères saisis, `IsComplete` dit si tout est
  rempli.
- **InputOtp** hérite de `Control` : une InputText (ou un champ masqué) par
  case ; `Value` (liée dans les deux sens), `Length`, `Mask`, `IntegerOnly`,
  `IsReadOnly`, `Invalid`.
- **KeyFilter** est une propriété attachée (`KeyFilter.Pattern`), qui se pose
  sur tout champ.

## Écarts avec mkm

- Password : le mot de passe n'est pas une propriété liable (la règle de
  WPF) ; pas de croix `showClear`.
- InputMask : pas de `unmask` (la valeur sans masque est `UnmaskedValue`).
- KeyFilter : pas de `validateOnly`.

## Clavier, accessibilité, performance

- Password : un champ « mot de passe » nommé par son texte d'aide ; la jauge
  s'annonce ; l'œil se nomme « Afficher le mot de passe » ou « Masquer le mot
  de passe » ; Échap cache la jauge.
- InputMask : Retour arrière et Suppr vident une case et ramènent la suite.
- InputOtp : un groupe ; chaque case, « Caractère 2 sur 4 » ; ← →, Retour
  arrière, Suppr.

## La documentation et les tests

Les pages Password, InputMask, InputOtp et KeyFilter ; `Lot19Tests` ;
`FormulaireTests` (la jauge et Échap ; la frappe suit le modèle ; la frappe
passe de case en case ; les caractères refusés).
