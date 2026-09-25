#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
gcc -std=c17 "${SQLITE_NATIVE_FLAGS[@]}" -O1 -g -Wall -Wextra -Werror \
  "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/tests" \
  "$SQLITE_AMALGAMATION/sqlite3.c" "$SQLITE_ROOT/tests/native/memory_vfs.c" \
  "$SQLITE_ROOT/tests/image_native.c" -lm -o "$SQLITE_ROOT/build/image-native"
run_sqlite_process "$SQLITE_ROOT/build/image-native" write "$SQLITE_ROOT/artifacts/native-exchange.db"
run_sqlite_process "$SQLITE_ROOT/build/image-native" read "$SQLITE_ROOT/artifacts/native-exchange.db"
