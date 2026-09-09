#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"

# Each executable uses the same unchanged engine and exact C oracle harness.
# Compiler/build logs stay separate from the deterministic program transcript.
suite="${1:-core}"
if [[ $# -gt 2 || ( -n "${2:-}" && "$2" != --emit-only ) ]]; then
  echo "Usage: $0 [core|api|vfs|vtable|allocation|upstream] [--emit-only]" >&2
  exit 2
fi
case "$suite" in
  core) harness="$SQLITE_ROOT/src/probe.c"; expected=native-corpus.expected ;;
  api) harness="$SQLITE_ROOT/tests/api_native.c"; expected=native-api.expected ;;
  vfs) harness="$SQLITE_ROOT/tests/vfs_native.c"; expected=native-vfs.expected ;;
  vtable) harness="$SQLITE_ROOT/tests/vtable_native.c"; expected=native-vtable.expected ;;
  allocation) harness="$SQLITE_ROOT/tests/allocation_native.c"; expected=native-allocation.expected ;;
  upstream)
    python3 "$SQLITE_ROOT/scripts/generate-upstream-jsonb.py" >&2
    harness="$SQLITE_ROOT/generated/upstream-jsonb.c"; expected=upstream-jsonb.expected ;;
  *) echo "Usage: $0 [core|api|vfs|vtable|allocation|upstream] [--emit-only]" >&2; exit 2 ;;
esac
translation_unit="$SQLITE_ROOT/generated/test-$suite.c"
python3 - "$translation_unit" "$harness" <<'PY'
import json, pathlib, sys
pathlib.Path(sys.argv[1]).write_text('#include "engine.c"\n#include ' + json.dumps(pathlib.Path(sys.argv[2]).name) + '\n')
PY
output="$SQLITE_ROOT/generated/Test-$suite"
dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" \
  -std=c17 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/src" -I "$(dirname "$harness")" \
  "$translation_unit" --emit=csproj -o "$output" \
  --offset-generator "$SQLITE_ROOT/generators/DotCC.OffsetGenerator/bin/Release/netstandard2.0/DotCC.OffsetGenerator.dll" \
  > "$SQLITE_ROOT/artifacts/translated-$suite-emission.log" 2>&1
if [[ "${2:-}" == --emit-only ]]; then
  echo "Emitted $output"
  exit 0
fi
dotnet build "$output/Test-$suite.csproj" -c Release --nologo \
  > "$SQLITE_ROOT/artifacts/translated-$suite-build.log" 2>&1
dotnet "$output/bin/Release/net10.0/Test-$suite.dll" \
  > "$SQLITE_ROOT/artifacts/translated-$suite.out"
diff -u "$SQLITE_ROOT/tests/$expected" "$SQLITE_ROOT/artifacts/translated-$suite.out"
cat "$SQLITE_ROOT/artifacts/translated-$suite.out"
