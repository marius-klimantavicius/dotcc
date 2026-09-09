#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
SQLITE_LAYOUT_FLAGS=("${SQLITE_NATIVE_FLAGS[@]}")
if [[ "${SQLITE_MS_BITFIELDS:-1}" == 0 ]]; then
  SQLITE_LAYOUT_FLAGS=()
  for flag in "${SQLITE_NATIVE_FLAGS[@]}"; do
    [[ "$flag" == -mms-bitfields ]] || SQLITE_LAYOUT_FLAGS+=("$flag")
  done
  SQLITE_LAYOUT_FLAGS+=(-mno-ms-bitfields)
fi
"$SQLITE_ROOT/scripts/preprocess.sh" > "$SQLITE_ROOT/artifacts/layout-sqlite3.i"
python3 "$SQLITE_ROOT/scripts/generate-layout-requests.py" \
  "$SQLITE_ROOT/artifacts/layout-sqlite3.i" "$SQLITE_ROOT/generated/layout_requests.inc" >&2
gcc -std=c17 -O0 "${SQLITE_LAYOUT_FLAGS[@]}" "${SQLITE_DEFINES[@]}" \
  -DDOTCC_LAYOUT_REQUESTS -I "$SQLITE_ROOT/generated" \
  -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/src" "$SQLITE_ROOT/src/layout_probe.c" \
  -lm -o "$SQLITE_ROOT/build/layout-native"
exec "$SQLITE_ROOT/build/layout-native"
