#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
SQLITE_LAYOUT_FLAGS=("${SQLITE_NATIVE_FLAGS[@]}")
if [[ "${SQLITE_MS_BITFIELDS:-1}" == 0 ]]; then SQLITE_LAYOUT_FLAGS=(-mno-ms-bitfields); fi
gcc -std=c17 -O0 "${SQLITE_LAYOUT_FLAGS[@]}" "${SQLITE_DEFINES[@]}" \
  -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/src" "$SQLITE_ROOT/src/layout_probe.c" \
  -lm -o "$SQLITE_ROOT/build/layout-native"
exec "$SQLITE_ROOT/build/layout-native"
