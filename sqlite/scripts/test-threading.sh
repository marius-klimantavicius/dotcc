#!/usr/bin/env bash
source "$(dirname "$0")/legacy-common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
if [[ $# != 0 ]]; then
  echo "Usage: $0" >&2
  exit 2
fi
project="$SQLITE_ROOT/tests/ThreadingTests/ThreadingTests.csproj"
prefix="$SQLITE_ROOT/artifacts/threading"
"$SQLITE_ROOT/scripts/emit-engine.sh" > "$prefix-emission.log" 2>&1
dotnet build "$project" -c Release --nologo > "$prefix-build.log" 2>&1
run_sqlite_process dotnet "$SQLITE_ROOT/tests/ThreadingTests/bin/Release/net10.0/ThreadingTests.dll" > "$prefix-jit.out"
cat "$prefix-jit.out"
if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  publish="$SQLITE_ROOT/build/threading-aot"
  dotnet publish "$project" -c Release -r "${SQLITE_RUNTIME_ID:-linux-x64}" -p:PublishAot=true \
    -o "$publish" --nologo > "$prefix-aot-build.log" 2>&1
  run_sqlite_process "$publish/ThreadingTests" > "$prefix-aot.out"
  cat "$prefix-aot.out"
fi
