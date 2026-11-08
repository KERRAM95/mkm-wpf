// Importe les jetons d'un thème de mkm (ui-kit) dans mkm-wpf : un fichier « chemin = valeur » par thème, dans
// src/Mkm.Wpf/Theming/Tokens/<Thème>.tokens, que la bibliothèque embarque et lit à l'exécution.
//
//   node outils/themes/importer-mkm.mjs <ui-kit> [composant…]
//
// <ui-kit> : la racine d'un clone de ui-kit. Les composants (actionbar, tooltip…) s'ajoutent à la base ; sans eux,
// ceux de la liste COMPOSANTS. Node 22.18 ou plus : il lit les fichiers TypeScript des presets en retirant leurs types.
//
// Les chemins sont ceux de dt() dans mkm : les clés en camelCase deviennent des mots séparés par des points
// (paddingX → padding.x), la section « root » d'un composant et le schéma « colorScheme.light » disparaissent, la
// section « extend.mkm » perd son préfixe. Ne sont pas repris : le mode sombre (colorScheme.dark), les couleurs de
// maturité (des policies de l'application, pas des jetons de thème) et la feuille CSS d'un composant (« css »).
import { existsSync, mkdirSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

// Dashboard étend Nora, ses tailles passées en pixels (1rem = 12 px, le texte de 3DEXPERIENCE), comme dans mkm
// (presets/dashboard/index.ts : definePreset(inPixels(Nora, 12), …)) ; ARA a toutes ses valeurs.
const THEMES = {
    Dashboard: { dossier: 'dashboard', etend: 'nora', rem: 12 },
    Ara: { dossier: 'ara' }
};
const COMPOSANTS = ['accordion', 'actionbar', 'autocomplete', 'avatar', 'badge', 'blockui', 'breadcrumb', 'button', 'card', 'carousel', 'cascadeselect', 'checkbox', 'chip', 'colorpicker', 'confirmdialog', 'contextmenu', 'datatable', 'dataview', 'datepicker', 'dialog', 'divider', 'dock', 'drawer', 'emptycontent', 'fieldset', 'floatlabel', 'galleria', 'iconfield', 'idcard', 'image', 'imagecompare', 'inplace', 'inputchips', 'inputgroup', 'inputnumber', 'inputotp', 'inputtext', 'knob', 'listbox', 'menu', 'megamenu', 'menubar', 'message', 'metergroup', 'multiselect', 'orderlist', 'organizationchart', 'overlaybadge', 'paginator', 'panel', 'panelmenu', 'password', 'picklist', 'popover', 'progressbar', 'progressspinner', 'propertypanel', 'radiobutton', 'rating', 'scrollpanel', 'select', 'selectbutton', 'skeleton', 'slider', 'speeddial', 'splitbutton', 'splitter', 'stategraph', 'stepper', 'tabs', 'tag', 'terminal', 'textarea', 'tieredmenu', 'timeline', 'toast', 'togglebutton', 'toggleswitch', 'toolbar', 'tooltip', 'tree', 'treegrid', 'treeselect', 'triptych', 'welcomepanel'];

const [racine, ...demandes] = process.argv.slice(2);
if (!racine) {
    console.error('usage : node outils/themes/importer-mkm.mjs <ui-kit> [composant…]');
    process.exit(1);
}
const composants = demandes.length ? demandes : COMPOSANTS;
const presets = resolve(racine, 'packages/themes/src/presets');
const sortie = resolve(import.meta.dirname, '../../src/Mkm.Wpf/Theming/Tokens');

const mots = (cle) => cle.replace(/([a-z0-9])([A-Z])/g, '$1.$2').toLowerCase();

function aplatir(noeud, chemin, lignes) {
    if (typeof noeud === 'string' || typeof noeud === 'number') {
        // Un renvoi écrit en camelCase dans mkm (« {overlay.popover.borderRadius} ») suit la même règle que les clés.
        const valeur = String(noeud).replace(/\{([^}]+)\}/g, (_, renvoi) => `{${renvoi.split('.').map(mots).join('.')}}`);
        if (/[\r\n]/.test(valeur)) throw new Error(`${chemin.join('.')} : une valeur sur plusieurs lignes n'est pas un jeton`);
        lignes.push([chemin.join('.'), valeur]);
        return;
    }
    for (const [cle, enfant] of Object.entries(noeud)) {
        // « css » : la feuille propre à mkm, pas un jeton.
        if (cle === 'dark' || cle === 'maturity' || cle === 'css') continue;
        if (cle === 'colorScheme') {
            if (enfant.light) aplatir(enfant.light, chemin, lignes);
            continue;
        }
        if (cle === 'root' || cle === 'light') {
            aplatir(enfant, chemin, lignes);
            continue;
        }
        aplatir(enfant, [...chemin, mots(cle)], lignes);
    }
}

async function charger(theme, partie) {
    const fichier = join(presets, theme, partie, 'index.ts');
    if (!existsSync(fichier)) return undefined;
    return (await import(pathToFileURL(fichier).href)).default;
}

// Comme inPixels de mkm (packages/themes/src/pixels.ts).
function enPixels(valeur, rem) {
    if (typeof valeur === 'string') return valeur.replace(/(\d*\.?\d+)rem\b/g, (_, taille) => `${Number((parseFloat(taille) * rem).toFixed(3))}px`);
    if (valeur && typeof valeur === 'object') return Object.fromEntries(Object.entries(valeur).map(([cle, jeton]) => [cle, enPixels(jeton, rem)]));
    return valeur;
}

// Comme definePreset de mkm (deepMerge) : les objets se fondent, une valeur remplace l'autre.
function fondre(...objets) {
    const resultat = {};
    for (const objet of objets.filter(Boolean)) {
        for (const [cle, valeur] of Object.entries(objet)) {
            resultat[cle] = valeur && typeof valeur === 'object' && resultat[cle] && typeof resultat[cle] === 'object' ? fondre(resultat[cle], valeur) : valeur;
        }
    }
    return resultat;
}

// Une partie du thème (base ou composant), fondue sur celle du thème qu'il étend.
async function partie(theme, nom) {
    const propre = await charger(theme.dossier, nom);
    if (!theme.etend) return propre;
    const heritee = await charger(theme.etend, nom);
    return fondre(heritee && enPixels(heritee, theme.rem), propre);
}

mkdirSync(sortie, { recursive: true });
for (const [nom, theme] of Object.entries(THEMES)) {
    const dossier = theme.dossier;
    const base = await partie(theme, 'base');
    const blocs = [];

    const lignesBase = [];
    aplatir(base.primitive ?? {}, [], lignesBase);
    aplatir(base.semantic ?? {}, [], lignesBase);
    aplatir(base.extend?.mkm ?? {}, [], lignesBase);
    blocs.push(['base', lignesBase]);

    for (const composant of composants) {
        const lignes = [];
        const jetons = await partie(theme, composant);
        if (!jetons) throw new Error(`${nom} : pas de jetons pour ${composant}`);
        // Le schéma clair d'un composant l'emporte sur ses valeurs communes, comme les variables CSS de mkm : il s'y fond
        // (Nora porte des valeurs dans son schéma que le tableau de bord remplace aussi, treegrid par exemple).
        const { colorScheme, ...communs } = jetons;
        aplatir(colorScheme?.light ? fondre(communs, colorScheme.light) : jetons, [composant], lignes);
        blocs.push([composant, lignes]);
    }

    const vus = new Set();
    const origine = theme.etend ? `${dossier}, sur ${theme.etend} en pixels` : dossier;
    let texte = `# Jetons du thème ${nom}, importés de mkm (ui-kit, packages/themes/src/presets/${origine}).\n`;
    texte += '# Fichier produit par outils/themes/importer-mkm.mjs : ne pas le modifier à la main.\n';
    for (const [titre, lignes] of blocs) {
        texte += `\n# ${titre}\n`;
        for (const [chemin, valeur] of lignes) {
            if (vus.has(chemin)) throw new Error(`${nom} : jeton en double, ${chemin}`);
            vus.add(chemin);
            texte += `${chemin} = ${valeur}\n`;
        }
    }
    writeFileSync(join(sortie, `${nom}.tokens`), texte);
    console.log(`${nom} : ${vus.size} jetons`);
}
