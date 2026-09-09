#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
"$SQLITE_ROOT/scripts/emit-engine.sh" > "$SQLITE_ROOT/artifacts/engine-emission.log" 2>&1
project="$SQLITE_ROOT/tests/ManagedConsumer/ManagedConsumer.csproj"
dotnet build "$project" -c Release --nologo > "$SQLITE_ROOT/artifacts/managed-consumer-build.log" 2>&1
dotnet "$SQLITE_ROOT/tests/ManagedConsumer/bin/Release/net10.0/ManagedConsumer.dll"
if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true \
    -o "$SQLITE_ROOT/build/managed-consumer-aot" --nologo \
    > "$SQLITE_ROOT/artifacts/managed-consumer-aot-build.log" 2>&1
  "$SQLITE_ROOT/build/managed-consumer-aot/ManagedConsumer"
fi
