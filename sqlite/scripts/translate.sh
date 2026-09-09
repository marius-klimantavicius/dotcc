#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
# Object emission accepts the complete amalgamation before a main/VFS is ready.
exec dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" -std=c17 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" "$SQLITE_AMALGAMATION/sqlite3.c" --emit=obj -o "$SQLITE_ROOT/generated/sqlite3.cs"
