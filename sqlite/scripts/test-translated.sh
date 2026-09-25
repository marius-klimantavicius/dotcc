#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"

# Each executable uses the same unchanged engine and exact C oracle harness.
# Compiler/build logs stay separate from the deterministic program transcript.
suite="${1:-core}"
if [[ $# -gt 2 || ( -n "${2:-}" && "$2" != --emit-only ) ]]; then
  echo "Usage: $0 [core|api|vfs|vtable|allocation|upstream|fts5] [--emit-only]" >&2
  exit 2
fi
case "$suite" in
  core) harness="$SQLITE_ROOT/tests/probe.c"; expected=native-corpus.expected ;;
  api) harness="$SQLITE_ROOT/tests/api_native.c"; expected=native-api.expected ;;
  vfs) harness="$SQLITE_ROOT/tests/vfs_native.c"; expected=native-vfs.expected ;;
  vtable) harness="$SQLITE_ROOT/tests/vtable_native.c"; expected=native-vtable.expected ;;
  fts5) harness="$SQLITE_ROOT/tests/fts5_native.c"; expected=native-fts5.expected ;;
  allocation) harness="$SQLITE_ROOT/tests/allocation_native.c"; expected=native-allocation.expected ;;
  upstream)
    python3 "$SQLITE_ROOT/scripts/generate-upstream-jsonb.py" >&2
    harness="$SQLITE_ROOT/generated/upstream-jsonb.c"; expected=upstream-jsonb.expected ;;
  *) echo "Usage: $0 [core|api|vfs|vtable|allocation|upstream|fts5] [--emit-only]" >&2; exit 2 ;;
esac
output="$SQLITE_ROOT/generated/Test-$suite"
dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" \
  -std=c17 "${SQLITE_DEFINES[@]}" -I "$SQLITE_AMALGAMATION" -I "$SQLITE_ROOT/tests" -I "$(dirname "$harness")" \
  "$SQLITE_AMALGAMATION/sqlite3.c" "$harness" \
  --overrides-file "$SQLITE_ROOT/config/corpus-overrides.json" --emit=csproj -o "$output" \
  > "$SQLITE_ROOT/artifacts/translated-$suite-emission.log" 2>&1
if [[ "${2:-}" == --emit-only ]]; then
  echo "Emitted $output"
  exit 0
fi
dotnet build "$output/Test-$suite.csproj" -c Release --nologo \
  > "$SQLITE_ROOT/artifacts/translated-$suite-build.log" 2>&1
run_sqlite_process dotnet "$output/bin/Release/net10.0/Test-$suite.dll" \
  > "$SQLITE_ROOT/artifacts/translated-$suite.out"
diff -u "$SQLITE_ROOT/tests/$expected" "$SQLITE_ROOT/artifacts/translated-$suite.out"
cat "$SQLITE_ROOT/artifacts/translated-$suite.out"

# The API corpus covers managed-boundary contracts and optional math/percentile/
# metadata behavior beyond the separate consumer's smoke checks. The VFS corpus
# checks the authored managed adapter against the full native contract in AOT too.
if [[ "${SQLITE_AOT:-0}" == 1 && ( "$suite" == api || "$suite" == vfs ) ]]; then
  publish="$SQLITE_ROOT/build/$suite-aot"
  dotnet publish "$output/Test-$suite.csproj" -c Release -r linux-x64 \
    -p:PublishAot=true -o "$publish" --nologo \
    > "$SQLITE_ROOT/artifacts/translated-$suite-aot-build.log" 2>&1
  run_sqlite_process "$publish/Test-$suite" > "$SQLITE_ROOT/artifacts/translated-$suite-aot.out"
  diff -u "$SQLITE_ROOT/tests/$expected" "$SQLITE_ROOT/artifacts/translated-$suite-aot.out"
  echo "PASS NativeAOT $suite corpus"
fi
