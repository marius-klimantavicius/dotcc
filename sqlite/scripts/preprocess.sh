#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/production-common.sh"
source "$(dirname "$0")/legacy-common.sh"
exec "${PRODUCTION_COMMAND[@]}" dotnet "$DOTCC_COMPILER" -std=c17 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -E "$SQLITE_AMALGAMATION/sqlite3.c"
