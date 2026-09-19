#!/usr/bin/env bash
set -euo pipefail
libsmb2_root=$(cd -- "$(dirname -- "$0")/.." && pwd)
project="$libsmb2_root/generated/TranslatedLibsmb2/TranslatedLibsmb2.csproj"
if [[ ! -f "$project" ]]; then
    echo 'No generated library; run libsmb2/scripts/translate.sh first.' >&2
    exit 1
fi
exec dotnet build "$project" -c Release --nologo "$@"
