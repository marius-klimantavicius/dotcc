#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
SQLITE_SANITIZE_FLAGS=()
if [[ "${SQLITE_SANITIZE:-0}" == 1 ]]; then
  SQLITE_SANITIZE_FLAGS=(-fsanitize=address,undefined -fno-omit-frame-pointer)
fi
gcc -std=c17 "${SQLITE_NATIVE_FLAGS[@]}" -O1 -g -Wall -Wextra -Werror "${SQLITE_SANITIZE_FLAGS[@]}" \
  "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/tests" \
  "$SQLITE_AMALGAMATION/sqlite3.c" "$SQLITE_ROOT/tests/memory_vfs.c" \
  "$SQLITE_ROOT/tests/fts5_native.c" -lm -o "$SQLITE_ROOT/build/fts5-native"
run_sqlite_process "$SQLITE_ROOT/build/fts5-native"
