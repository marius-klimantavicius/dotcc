#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
gcc -std=c17 "${SQLITE_NATIVE_FLAGS[@]}" -O2 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/src" \
  "$SQLITE_AMALGAMATION/sqlite3.c" "$SQLITE_ROOT/src/memory_vfs.c" "$SQLITE_ROOT/src/probe.c" \
  -lm -o "$SQLITE_ROOT/build/sqlite-native"
exec "$SQLITE_ROOT/build/sqlite-native"
