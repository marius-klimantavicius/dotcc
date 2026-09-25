#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
"$SQLITE_ROOT/scripts/preprocess.sh" > "$SQLITE_ROOT/artifacts/layout-sqlite3.i"
python3 "$SQLITE_ROOT/scripts/generate-layout-requests.py" \
  "$SQLITE_ROOT/artifacts/layout-sqlite3.i" "$SQLITE_ROOT/generated/layout_requests.h" >&2
output="$SQLITE_ROOT/generated/Test-layout"
dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" \
  -std=c17 "${SQLITE_DEFINES[@]}" -DDOTCC_LAYOUT_REQUESTS \
  -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/tests" -I "$SQLITE_ROOT/generated" \
  "$SQLITE_ROOT/tests/layout_probe.c" --overrides-file "$SQLITE_ROOT/config/corpus-overrides.json" --emit=csproj -o "$output" \
  > "$SQLITE_ROOT/artifacts/translated-layout-emission.log" 2>&1
python3 "$SQLITE_ROOT/scripts/check-layout-metadata.py" "$output/DotCcProgram.cs"
python3 "$SQLITE_ROOT/scripts/generate-layout-storage-checks.py" \
  "$SQLITE_ROOT/tests/layout-native.expected" "$output/LayoutStorageChecks.cs"
dotnet build "$output/Test-layout.csproj" -c Release --nologo \
  > "$SQLITE_ROOT/artifacts/translated-layout-build.log" 2>&1
run_sqlite_process dotnet "$output/bin/Release/net10.0/Test-layout.dll" > "$SQLITE_ROOT/artifacts/translated-layout.out"
diff -u "$SQLITE_ROOT/tests/layout-native.expected" "$SQLITE_ROOT/artifacts/translated-layout.out"
cat "$SQLITE_ROOT/artifacts/translated-layout.out"
if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  dotnet publish "$output/Test-layout.csproj" -c Release -r linux-x64 \
    -p:PublishAot=true -o "$SQLITE_ROOT/build/layout-aot" --nologo \
    > "$SQLITE_ROOT/artifacts/translated-layout-aot-build.log" 2>&1
  run_sqlite_process "$SQLITE_ROOT/build/layout-aot/Test-layout" > "$SQLITE_ROOT/artifacts/translated-layout-aot.out"
  diff -u "$SQLITE_ROOT/tests/layout-native.expected" "$SQLITE_ROOT/artifacts/translated-layout-aot.out"
  printf '%s\n' 'PASS NativeAOT layout constants, aggregate alignment, pointer-array storage and addresses'
fi
