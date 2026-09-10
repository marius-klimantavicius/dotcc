#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
python3 "$SQLITE_ROOT/scripts/prepare-host-source.py" >&2
SQLITE_HOST_DEFINES=()
while IFS= read -r definition; do
  [[ -z "$definition" || "$definition" == \#* ]] || SQLITE_HOST_DEFINES+=("-D$definition")
done < "$SQLITE_ROOT/config/host-defines.txt"
exec dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" \
  -std=c17 "${SQLITE_DEFINES[@]}" "${SQLITE_HOST_DEFINES[@]}" -I "$SQLITE_ROOT/generated/sqlite-port" \
  -I "$SQLITE_ROOT/src" "$SQLITE_ROOT/src/engine.c" \
  --emit=managedlib --class-name Sqlite -o "$SQLITE_ROOT/generated/TranslatedSqlite"
