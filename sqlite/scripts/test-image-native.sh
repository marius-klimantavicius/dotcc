#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
gcc -std=c17 "${SQLITE_NATIVE_FLAGS[@]}" -O1 -g -Wall -Wextra -Werror \
  "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/src" \
  "$SQLITE_AMALGAMATION/sqlite3.c" "$SQLITE_ROOT/src/memory_vfs.c" \
  "$SQLITE_ROOT/tests/image_native.c" -lm -o "$SQLITE_ROOT/build/image-native"
"$SQLITE_ROOT/build/image-native" write "$SQLITE_ROOT/artifacts/native-exchange.db"
"$SQLITE_ROOT/build/image-native" read "$SQLITE_ROOT/artifacts/native-exchange.db"
