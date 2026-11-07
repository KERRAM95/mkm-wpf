#!/bin/bash
# Compilation de toute la solution sous Linux, pour une session de développement
# sans Windows : code VB et XAML (styles, gabarits, fenêtres x:Class), contre les
# assemblies de référence de .NET Framework 4.8.
#
# Ce que le script ne vérifie pas : l'exécution. Ni la vitrine ni les tests ne
# tournent hors de Windows ; la CI (et Visual Studio 2022) font foi.
#
# Il faut le SDK .NET de Microsoft : celui des distributions (paquet dotnet-sdk-8.0
# d'Ubuntu, construit depuis les sources) n'a pas la partie WPF
# (Sdks/Microsoft.NET.Sdk.WindowsDesktop). Si le dotnet du PATH ne l'a pas, le script
# installe le SDK de Microsoft dans un cache hors du dépôt, après contrôle de son
# SHA-512 par les métadonnées de publication de Microsoft.
#
# Usage : outils/linux/verifier.sh
set -euo pipefail

VERSION_SDK=8.0.427
racine=$(cd "$(dirname "$0")/../.." && pwd)

a_wpf() { # $1 : dossier d'un dotnet
  [ -d "$1/sdk/$VERSION_SDK/Sdks/Microsoft.NET.Sdk.WindowsDesktop/tools" ] ||
    compgen -G "$1/sdk/*/Sdks/Microsoft.NET.Sdk.WindowsDesktop/tools" >/dev/null
}

dotnet_actuel=$(dirname "$(readlink -f "$(command -v dotnet 2>/dev/null || echo /nonexistent)")")
if a_wpf "$dotnet_actuel"; then
  export DOTNET_ROOT=$dotnet_actuel
else
  cache=${XDG_CACHE_HOME:-$HOME/.cache}/mkm-wpf/dotnet-$VERSION_SDK
  if ! a_wpf "$cache"; then
    echo "Installation du SDK .NET $VERSION_SDK de Microsoft dans $cache"
    temp=$(mktemp -d)
    trap 'rm -rf "$temp"' EXIT
    archive=dotnet-sdk-$VERSION_SDK-linux-x64.tar.gz
    curl -sSfL -o "$temp/$archive" "https://builds.dotnet.microsoft.com/dotnet/Sdk/$VERSION_SDK/$archive"
    curl -sSfL -o "$temp/releases.json" "https://builds.dotnet.microsoft.com/dotnet/release-metadata/${VERSION_SDK%.*}/releases.json"
    attendu=$(python3 -I - "$temp/releases.json" "$VERSION_SDK" "$archive" <<'EOF'
import json, sys
chemin, version, archive = sys.argv[1:]
for publication in json.load(open(chemin))["releases"]:
    for sdk in [publication.get("sdk", {})] + publication.get("sdks", []):
        if sdk.get("version") == version:
            for fichier in sdk["files"]:
                if fichier["url"].endswith("/" + archive):
                    print(fichier["hash"])
                    sys.exit()
EOF
)
    obtenu=$(sha512sum "$temp/$archive" | cut -d' ' -f1)
    if [ -z "$attendu" ] || [ "$attendu" != "$obtenu" ]; then
      echo "SHA-512 du SDK non conforme : installation abandonnée." >&2
      exit 1
    fi
    mkdir -p "$cache"
    tar -xzf "$temp/$archive" -C "$cache"
  fi
  export DOTNET_ROOT=$cache
fi

export PATH=$DOTNET_ROOT:$PATH DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
echo "SDK .NET $(dotnet --version) ($DOTNET_ROOT)"
dotnet build "$racine/Mkm.Wpf.sln" --configuration Release -nologo -v q
echo "Non vérifié sous Linux : l'exécution de la vitrine et des tests (voir la CI)."
