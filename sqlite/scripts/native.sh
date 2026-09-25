#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
gcc -std=c17 "${SQLITE_NATIVE_FLAGS[@]}" -O2 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/tests" \
  "$SQLITE_AMALGAMATION/sqlite3.c" "$SQLITE_ROOT/tests/native/memory_vfs.c" "$SQLITE_ROOT/tests/probe.c" \
  -lm -o "$SQLITE_ROOT/build/sqlite-native"
run_sqlite_process "$SQLITE_ROOT/build/sqlite-native"
