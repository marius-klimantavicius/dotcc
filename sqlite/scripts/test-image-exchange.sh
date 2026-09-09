#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
if [[ $# -gt 1 || ( -n "${1:-}" && "$1" != --emit-only ) ]]; then
  echo "Usage: $0 [--emit-only]" >&2
  exit 2
fi

translation_unit="$SQLITE_ROOT/generated/test-image.c"
cat > "$translation_unit" <<'C'
#include "engine.c"
#include "image_native.c"
C
output="$SQLITE_ROOT/generated/Test-image"
dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" \
  -std=c17 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" \
  -I "$SQLITE_ROOT/src" -I "$SQLITE_ROOT/tests" \
  "$translation_unit" --emit=csproj -o "$output" \
  --offset-generator "$SQLITE_ROOT/generators/DotCC.OffsetGenerator/bin/Release/netstandard2.0/DotCC.OffsetGenerator.dll" \
  > "$SQLITE_ROOT/artifacts/translated-image-emission.log" 2>&1
if [[ "${1:-}" == --emit-only ]]; then
  echo "Emitted $output"
  exit 0
fi
dotnet build "$output/Test-image.csproj" -c Release --nologo \
  > "$SQLITE_ROOT/artifacts/translated-image-build.log" 2>&1
"$SQLITE_ROOT/scripts/test-image-native.sh" \
  > "$SQLITE_ROOT/artifacts/native-image-roundtrip.out" \
  2> "$SQLITE_ROOT/artifacts/native-image-build.log"
diff -u "$SQLITE_ROOT/tests/native-image.expected" "$SQLITE_ROOT/artifacts/native-image-roundtrip.out"

native="$SQLITE_ROOT/build/image-native"
managed=(dotnet "$output/bin/Release/net10.0/Test-image.dll")
for direction in native-to-managed managed-to-native managed-to-managed; do
  database="$SQLITE_ROOT/artifacts/exchange-$direction.db"
  transcript="$SQLITE_ROOT/artifacts/exchange-$direction.out"
  case "$direction" in
    native-to-managed)
      run_sqlite_process "$native" write "$database" > "$transcript"
      run_sqlite_process "${managed[@]}" read "$database" >> "$transcript" ;;
    managed-to-native)
      run_sqlite_process "${managed[@]}" write "$database" > "$transcript"
      run_sqlite_process "$native" read "$database" >> "$transcript" ;;
    managed-to-managed)
      run_sqlite_process "${managed[@]}" write "$database" > "$transcript"
      run_sqlite_process "${managed[@]}" read "$database" >> "$transcript" ;;
  esac
  diff -u "$SQLITE_ROOT/tests/native-image.expected" "$transcript"
  echo "PASS $direction: independent-process image exchange"
done
