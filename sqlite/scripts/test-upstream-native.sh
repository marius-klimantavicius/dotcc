#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
python3 "$SQLITE_ROOT/scripts/generate-upstream-jsonb.py" >&2
gcc -std=c17 "${SQLITE_NATIVE_FLAGS[@]}" -O2 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/src" \
  "$SQLITE_AMALGAMATION/sqlite3.c" "$SQLITE_ROOT/src/memory_vfs.c" \
  "$SQLITE_ROOT/generated/upstream-jsonb.c" -lm -o "$SQLITE_ROOT/build/upstream-jsonb-native"
exec "$SQLITE_ROOT/build/upstream-jsonb-native"
