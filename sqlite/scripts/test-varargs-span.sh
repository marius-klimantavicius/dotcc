#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
if [[ $# != 0 ]]; then
  echo "Usage: $0" >&2
  exit 2
fi

compiler="$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll"
fixture="$SQLITE_ROOT/tests/VarargsSpanConsumer/fixture.c"
generated="$SQLITE_ROOT/generated/VarargsSpanFixture"
project="$SQLITE_ROOT/tests/VarargsSpanConsumer/VarargsSpanConsumer.csproj"
prefix="$SQLITE_ROOT/artifacts/varargs-span"
dotnet "$compiler" -std=c17 "$fixture" --emit=managedlib -o "$generated" \
  > "$prefix-emission.log" 2>&1
dotnet build "$project" -c Release -warnaserror:CS9080 --nologo > "$prefix-build.log" 2>&1
run_sqlite_process dotnet "$SQLITE_ROOT/tests/VarargsSpanConsumer/bin/Release/net10.0/VarargsSpanConsumer.dll" \
  > "$prefix-jit.out"
cat "$prefix-jit.out"

# Elapsed times are reported, never used as correctness thresholds. Each runtime
# independently checks results and warmed per-thread allocation deltas.
if [[ "${SQLITE_AOT:-1}" == 1 ]]; then
publish="$SQLITE_ROOT/build/varargs-span-aot"
dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true \
  -o "$publish" -warnaserror:CS9080 --nologo > "$prefix-aot-build.log" 2>&1
run_sqlite_process "$publish/VarargsSpanConsumer" > "$prefix-aot.out"
cat "$prefix-aot.out"
fi
