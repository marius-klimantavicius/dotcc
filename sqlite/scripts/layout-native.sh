#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
SQLITE_LAYOUT_FLAGS=("${SQLITE_NATIVE_FLAGS[@]}")
if [[ "${SQLITE_MS_BITFIELDS:-0}" == 1 ]]; then
  SQLITE_LAYOUT_FLAGS=()
  for flag in "${SQLITE_NATIVE_FLAGS[@]}"; do
    [[ "$flag" == -mno-ms-bitfields ]] || SQLITE_LAYOUT_FLAGS+=("$flag")
  done
  SQLITE_LAYOUT_FLAGS+=(-mms-bitfields)
fi
"$SQLITE_ROOT/scripts/preprocess.sh" > "$SQLITE_ROOT/artifacts/layout-sqlite3.i"
python3 "$SQLITE_ROOT/scripts/generate-layout-requests.py" \
  "$SQLITE_ROOT/artifacts/layout-sqlite3.i" "$SQLITE_ROOT/generated/layout_requests.h" >&2
gcc -std=c17 -O0 "${SQLITE_LAYOUT_FLAGS[@]}" "${SQLITE_DEFINES[@]}" \
  -DDOTCC_LAYOUT_REQUESTS -I "$SQLITE_ROOT/generated" \
  -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/tests" "$SQLITE_ROOT/tests/layout_probe.c" "$SQLITE_ROOT/tests/native/memory_vfs.c" \
  -lm -o "$SQLITE_ROOT/build/layout-native"
run_sqlite_process "$SQLITE_ROOT/build/layout-native"
