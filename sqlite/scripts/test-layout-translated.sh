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
  -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/src" -I "$SQLITE_ROOT/generated" \
  "$SQLITE_ROOT/src/layout_probe.c" --emit=csproj -o "$output" \
  --offset-generator "$SQLITE_ROOT/generators/DotCC.OffsetGenerator/bin/Release/netstandard2.0/DotCC.OffsetGenerator.dll" \
  > "$SQLITE_ROOT/artifacts/translated-layout-emission.log" 2>&1
python3 "$SQLITE_ROOT/scripts/check-layout-metadata.py" "$output/Program.cs"
dotnet build "$output/Test-layout.csproj" -c Release --nologo \
  > "$SQLITE_ROOT/artifacts/translated-layout-build.log" 2>&1
dotnet "$output/bin/Release/net10.0/Test-layout.dll" > "$SQLITE_ROOT/artifacts/translated-layout.out"
diff -u "$SQLITE_ROOT/tests/layout-native.expected" "$SQLITE_ROOT/artifacts/translated-layout.out"
cat "$SQLITE_ROOT/artifacts/translated-layout.out"
