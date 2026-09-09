#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
exec dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" \
  -std=c17 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" \
  -I "$SQLITE_ROOT/src" "$SQLITE_ROOT/src/engine.c" \
  --emit=managedlib -o "$SQLITE_ROOT/generated/TranslatedSqlite" \
  --offset-generator "$SQLITE_ROOT/generators/DotCC.OffsetGenerator/bin/Release/netstandard2.0/DotCC.OffsetGenerator.dll"
