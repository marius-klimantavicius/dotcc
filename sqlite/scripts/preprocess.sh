#!/usr/bin/env bash
source "$(dirname "$0")/legacy-common.sh"
exec dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" -std=c17 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -E "$SQLITE_AMALGAMATION/sqlite3.c"
