#!/usr/bin/env bash
set -euo pipefail
SQLITE_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$SQLITE_ROOT/.." && pwd)"
SQLITE_AMALGAMATION="$SQLITE_ROOT/ref/sqlite-amalgamation-3500400"
SQLITE_DEFINES=()
while IFS= read -r definition; do
  [[ -z "$definition" || "$definition" == \#* ]] || SQLITE_DEFINES+=("-D$definition")
done < "$SQLITE_ROOT/config/defines.txt"
mkdir -p "$SQLITE_ROOT/build" "$SQLITE_ROOT/generated" "$SQLITE_ROOT/artifacts"
