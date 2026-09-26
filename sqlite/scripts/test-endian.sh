#!/usr/bin/env bash
# Compare endian-sensitive C APIs and files with the unmodified native engine.
source "$(dirname "$0")/legacy-common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
"$SQLITE_ROOT/scripts/emit-engine.sh" > "$SQLITE_ROOT/artifacts/endian-emission.log" 2>&1
project="$SQLITE_ROOT/samples/ManagedConsumer/ManagedConsumer.csproj"
dotnet build "$project" -c Release --nologo > "$SQLITE_ROOT/artifacts/endian-build.log" 2>&1
gcc -std=c17 -O1 -DSQLITE_THREADSAFE=0 -DSQLITE_OMIT_LOAD_EXTENSION \
  -I "$SQLITE_AMALGAMATION" "$SQLITE_AMALGAMATION/sqlite3.c" \
  "$SQLITE_ROOT/tests/endian_native.c" -lm -o "$SQLITE_ROOT/build/endian-native"

check_exchange() {
  local label="$1"; shift
  local root="$SQLITE_ROOT/artifacts/endian-$label"
  mkdir -p "$root/native" "$root/managed"
  "$SQLITE_ROOT/build/endian-native" write "$root/native" > "$root/expected.out"
  run_sqlite_process "$@" endian read "$root/native" > "$root/native-to-managed.out"
  run_sqlite_process "$@" endian write "$root/managed" > "$root/managed.out"
  "$SQLITE_ROOT/build/endian-native" read "$root/managed" > "$root/managed-to-native.out"
  diff -u "$root/expected.out" "$root/native-to-managed.out"
  diff -u "$root/expected.out" "$root/managed.out"
  diff -u "$root/expected.out" "$root/managed-to-native.out"
  cat "$root/expected.out"
  echo "PASS $label: native/managed UTF16LE and UTF16BE database exchange"
}
check_exchange jit dotnet "$SQLITE_ROOT/samples/ManagedConsumer/bin/Release/net10.0/ManagedConsumer.dll"
if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true \
    -o "$SQLITE_ROOT/build/managed-consumer-aot" --nologo \
    > "$SQLITE_ROOT/artifacts/managed-consumer-aot-build.log" 2>&1
  check_exchange aot "$SQLITE_ROOT/build/managed-consumer-aot/ManagedConsumer"
fi
