// Importe le jeu d'icônes de mkm dans mkm-wpf : les icônes Tabler que nomme le manifeste de ui-kit
// (packages/icons/tabler/manifest.json), à sa version et à son empreinte, deviennent des géométries WPF dans
// src/Mkm.Wpf/Controls/Icon/Icons.g.vb, celles des composants (« internal ») dans InternalIcons.g.vb ; la licence
// de Tabler entre dans THIRD-PARTY-NOTICES.md.
//
//   node outils/icones/importer-tabler.mjs <ui-kit>
//
// Ajouter une icône : l'ajouter au manifeste de mkm (son nom par son sens, le fichier Tabler qui la dessine), puis
// relancer cet outil. Le paquet est téléchargé par npm pack et extrait, jamais installé : aucun de ses scripts ne tourne.
import { execFileSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import { mkdtempSync, readFileSync, readdirSync, rmSync, writeFileSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';

const fail = (message) => {
    console.error(`importer-tabler : ${message}`);
    process.exit(1);
};

const [racine] = process.argv.slice(2);
if (!racine) fail('usage : node outils/icones/importer-tabler.mjs <ui-kit>');

const ici = import.meta.dirname;
const depot = resolve(ici, '../..');
const manifeste = JSON.parse(readFileSync(resolve(racine, 'packages/icons/tabler/manifest.json'), 'utf8'));

function importer(paquet) {
    const proprietes = [];
    for (const [nom, fichier] of Object.entries(manifeste.icons)) {
        if (fichier.includes('/')) fail(`${nom} : seul le style outline est repris (${fichier}).`);
        const chemin = join(paquet, 'icons', manifeste.style, `${fichier}.svg`);
        if (!existsSync(chemin)) fail(`${fichier}.svg n'existe pas (icône « ${nom} »).`);
        proprietes.push({ nom, fichier, donnees: geometrie(readFileSync(chemin, 'utf8'), fichier) });
    }

    const lignes = [
        "' Les icônes de mkm, dessinées par Tabler Icons (MIT, voir THIRD-PARTY-NOTICES.md) :",
        `' ${manifeste.package} ${manifeste.version}, style ${manifeste.style}, sur la grille de 24 de Tabler.`,
        "' Fichier produit par outils/icones/importer-tabler.mjs : ne pas le modifier à la main.",
        'Imports System.Windows.Media',
        '',
        'Namespace Mkm.Wpf.Controls',
        '',
        '    Partial Public NotInheritable Class Icons',
        ''
    ];
    for (const { nom, fichier, donnees } of proprietes) {
        lignes.push(`        ''' <summary>« ${nom} » : ${fichier} (Tabler).</summary>`);
        lignes.push(`        Public Shared ReadOnly Property ${identifiant(nom)} As Geometry = Dessin("${donnees}")`);
        lignes.push('');
    }
    lignes.push('    End Class', '', 'End Namespace', '');
    writeFileSync(join(depot, 'src/Mkm.Wpf/Controls/Icon/Icons.g.vb'), lignes.join('\n'));

    // Les icônes des composants (la clé « internal » du manifeste) : celles que les composants de mkm dessinent
    // eux-mêmes (le calendrier du DatePicker, les flèches d'un mois…), nommées comme leur composant Vue
    // (« CalendarIcon » → InternalIcons.Calendar). Une icône pleine (« filled/… ») ou sans dessin n'est pas reprise :
    // Icon trace des traits.
    const internes = [];
    for (const [dossier, fichier] of Object.entries(manifeste.internal ?? {})) {
        if (!fichier || fichier.includes('/')) continue;
        const vue = readdirSync(resolve(racine, 'packages/icons/src', dossier)).find((f) => f.endsWith('Icon.vue'));
        if (!vue) fail(`packages/icons/src/${dossier} : pas de composant *Icon.vue.`);
        const chemin = join(paquet, 'icons', manifeste.style, `${fichier}.svg`);
        if (!existsSync(chemin)) fail(`${fichier}.svg n'existe pas (icône interne « ${dossier} »).`);
        internes.push({ nom: vue.replace(/Icon\.vue$/, ''), dossier, fichier, donnees: geometrie(readFileSync(chemin, 'utf8'), fichier) });
    }
    const interne = [
        "' Les icônes des composants de mkm (clé « internal » du manifeste), dessinées par Tabler Icons (MIT, voir",
        `' THIRD-PARTY-NOTICES.md) : ${manifeste.package} ${manifeste.version}, style ${manifeste.style}, sur la grille de 24.`,
        "' Fichier produit par outils/icones/importer-tabler.mjs : ne pas le modifier à la main.",
        'Imports System.Windows.Media',
        '',
        'Namespace Mkm.Wpf.Controls',
        '',
        '    Partial Public NotInheritable Class InternalIcons',
        ''
    ];
    for (const { nom, dossier, fichier, donnees } of internes) {
        interne.push(`        ''' <summary>« ${dossier} » (${nom}Icon de mkm) : ${fichier} (Tabler).</summary>`);
        interne.push(`        Public Shared ReadOnly Property ${identifiant(nom)} As Geometry = Icons.Dessin("${donnees}")`);
        interne.push('');
    }
    interne.push('    End Class', '', 'End Namespace', '');
    writeFileSync(join(depot, 'src/Mkm.Wpf/Controls/Icon/InternalIcons.g.vb'), interne.join('\n'));
    console.log(`${internes.length} icônes internes`);

    // Seule la section de Tabler, entre ses deux marqueurs, est réécrite : les avis des autres œuvres reprises (CLAUDE.md,
    // « L'open source d'abord ») restent.
    const licence = readFileSync(join(paquet, 'LICENSE'), 'utf8').trim();
    const section = [
        '<!-- début : Tabler Icons, écrit par outils/icones/importer-tabler.mjs -->',
        `## Tabler Icons (${manifeste.package} ${manifeste.version})`,
        '',
        'Les géométries de `src/Mkm.Wpf/Controls/Icon/Icons.g.vb` et `InternalIcons.g.vb`.',
        '',
        '```text',
        licence,
        '```',
        '<!-- fin : Tabler Icons -->'
    ].join('\n');
    const cheminDesAvis = join(depot, 'THIRD-PARTY-NOTICES.md');
    const entete = '# Avis de tiers\n\nmkm-wpf embarque des œuvres de tiers, chacune sous sa licence, reproduite ici.\n';
    const marquee = /<!-- début : Tabler Icons[\s\S]*?<!-- fin : Tabler Icons -->/;
    let avis = existsSync(cheminDesAvis) ? readFileSync(cheminDesAvis, 'utf8') : entete;
    if (marquee.test(avis)) {
        avis = avis.replace(marquee, () => section);
    } else if (/^## Tabler Icons/m.test(avis)) {
        // Une section d'avant les marqueurs : jusqu'à la section suivante, ou la fin.
        avis = avis.replace(/^## Tabler Icons[\s\S]*?(?=^## |$(?![\s\S]))/m, () => section + '\n');
    } else {
        avis = `${avis.trimEnd()}\n\n${section}\n`;
    }
    writeFileSync(cheminDesAvis, avis);
    console.log(`${proprietes.length} icônes`);
}

// Les formes d'une icône, sans le cadre invisible de Tabler, en une seule donnée de chemin que WPF lit.
function geometrie(svg, fichier) {
    const morceaux = [];
    for (const [, balise, reste] of svg.matchAll(/<([a-zA-Z]+)\b([^>]*?)\/?>/g)) {
        if (balise === 'svg' || balise === 'g') continue;
        const a = Object.fromEntries([...reste.matchAll(/([a-z-]+)="([^"]*)"/g)].map((m) => [m[1], m[2]]));
        if (a.stroke === 'none' && /^M0 0h24v24H0z$/.test(a.d ?? '')) continue;
        if ((a.fill && a.fill !== 'none') || a.transform || a.opacity) fail(`${fichier} : forme pleine, transformée ou transparente (${balise}).`);
        const n = (cle) => Number(a[cle] ?? 0);
        switch (balise) {
            case 'path':
                morceaux.push(normaliser(a.d));
                break;
            case 'circle':
                morceaux.push(ellipse(n('cx'), n('cy'), n('r'), n('r')));
                break;
            case 'ellipse':
                morceaux.push(ellipse(n('cx'), n('cy'), n('rx'), n('ry')));
                break;
            case 'line':
                morceaux.push(`M ${n('x1')} ${n('y1')} L ${n('x2')} ${n('y2')}`);
                break;
            case 'polyline':
            case 'polygon': {
                const p = a.points.trim().split(/[\s,]+/).map(Number);
                let d = `M ${p[0]} ${p[1]}`;
                for (let i = 2; i < p.length; i += 2) d += ` L ${p[i]} ${p[i + 1]}`;
                morceaux.push(balise === 'polygon' ? `${d} Z` : d);
                break;
            }
            case 'rect':
                morceaux.push(rectangle(n('x'), n('y'), n('width'), n('height'), Number(a.rx ?? a.ry ?? 0), Number(a.ry ?? a.rx ?? 0)));
                break;
            default:
                fail(`${fichier} : élément <${balise}> inattendu.`);
        }
    }
    if (!morceaux.length) fail(`${fichier} : aucune forme.`);
    return morceaux.join(' ');
}

const nombre = (v) => String(Math.round(v * 10000) / 10000);
const ellipse = (cx, cy, rx, ry) => `M ${nombre(cx - rx)} ${nombre(cy)} A ${nombre(rx)} ${nombre(ry)} 0 1 0 ${nombre(cx + rx)} ${nombre(cy)} A ${nombre(rx)} ${nombre(ry)} 0 1 0 ${nombre(cx - rx)} ${nombre(cy)} Z`;

function rectangle(x, y, w, h, rx, ry) {
    if (!rx && !ry) return `M ${x} ${y} H ${x + w} V ${y + h} H ${x} Z`;
    return [
        `M ${nombre(x + rx)} ${nombre(y)}`, `H ${nombre(x + w - rx)}`, `A ${nombre(rx)} ${nombre(ry)} 0 0 1 ${nombre(x + w)} ${nombre(y + ry)}`,
        `V ${nombre(y + h - ry)}`, `A ${nombre(rx)} ${nombre(ry)} 0 0 1 ${nombre(x + w - rx)} ${nombre(y + h)}`,
        `H ${nombre(x + rx)}`, `A ${nombre(rx)} ${nombre(ry)} 0 0 1 ${nombre(x)} ${nombre(y + h - ry)}`,
        `V ${nombre(y + ry)}`, `A ${nombre(rx)} ${nombre(ry)} 0 0 1 ${nombre(x + rx)} ${nombre(y)}`, 'Z'
    ].join(' ');
}

// Les données de chemin de SVG, réécrites avec un espace entre chaque nombre : SVG permet « 1-2 », « .5.5 » ou des
// drapeaux d'arc collés (« 0 011 1 »), que WPF ne lit pas toujours.
function normaliser(d) {
    const sortie = [];
    let i = 0;
    let commande = null;
    let rang = 0;
    const blanc = /[\s,]/;
    const lireNombre = () => {
        const m = /^[-+]?(\d*\.\d+|\d+\.?)([eE][-+]?\d+)?/.exec(d.slice(i));
        if (!m) fail(`chemin illisible près de « ${d.slice(i, i + 12)} »`);
        i += m[0].length;
        return nombre(Number(m[0]));
    };
    while (i < d.length) {
        const c = d[i];
        if (blanc.test(c)) {
            i++;
            continue;
        }
        if (/[a-zA-Z]/.test(c)) {
            commande = c;
            rang = 0;
            sortie.push(c);
            i++;
            continue;
        }
        // Les 4e et 5e paramètres d'un arc sont des drapeaux d'un chiffre, parfois collés au suivant.
        if ((commande === 'a' || commande === 'A') && (rang % 7 === 3 || rang % 7 === 4)) {
            if (c !== '0' && c !== '1') fail(`drapeau d'arc « ${c} » inattendu`);
            sortie.push(c);
            i++;
        } else {
            sortie.push(lireNombre());
        }
        rang++;
    }
    return sortie.join(' ');
}

// Un nom de mkm (« caret-down ») en identifiant VB (« CaretDown ») ; un mot réservé de VB entre crochets.
const RESERVES = new Set(['Error', 'Stop', 'Structure', 'Select', 'Option', 'Property', 'Next', 'End', 'Loop', 'Step', 'Call', 'Class', 'Module', 'Namespace', 'Event', 'Operator', 'Resume', 'Return', 'Set', 'Get', 'Finally', 'Imports', 'Is', 'Like', 'Not', 'Or', 'And', 'Date', 'Default', 'Delegate', 'Dim', 'Do', 'Each', 'Else', 'Enum', 'Exit', 'For', 'Function', 'Global', 'GoTo', 'If', 'In', 'Interface', 'Let', 'Me', 'New', 'Nothing', 'Of', 'On', 'Out', 'Shared', 'Static', 'Sub', 'Then', 'Throw', 'To', 'Try', 'Type', 'Until', 'Using', 'When', 'While', 'With']);
function identifiant(nom) {
    const pascal = nom.split('-').map((m) => m[0].toUpperCase() + m.slice(1)).join('');
    return RESERVES.has(pascal) ? `[${pascal}]` : pascal;
}

// Le paquet, à la version du manifeste, et son empreinte.
const temp = mkdtempSync(join(tmpdir(), 'tabler-'));
try {
    execFileSync('npm', ['pack', '--silent', '--ignore-scripts', `${manifeste.package}@${manifeste.version}`], { cwd: temp, stdio: ['ignore', 'ignore', 'inherit'] });
    const archive = join(temp, readdirSync(temp).find((f) => f.endsWith('.tgz')));
    const empreinte = `sha512-${createHash('sha512').update(readFileSync(archive)).digest('base64')}`;
    if (empreinte !== manifeste.integrity) fail(`empreinte ${empreinte}, le manifeste attend ${manifeste.integrity}.`);
    execFileSync('tar', ['-xzf', archive, '-C', temp]);
    importer(join(temp, 'package'));
} finally {
    rmSync(temp, { recursive: true, force: true });
}
