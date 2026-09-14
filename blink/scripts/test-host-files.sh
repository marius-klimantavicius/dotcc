#!/usr/bin/env bash
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mkdir -p "$BLINK_ROOT/artifacts/host-files"
project="$BLINK_ROOT/tests/HostFiles/HostFiles.csproj"
dotnet build "$project" -c Release > "$BLINK_ROOT/artifacts/host-files/build.log" 2>&1
timeout 30 dotnet "$BLINK_ROOT/tests/HostFiles/bin/Release/net10.0/HostFiles.dll" > "$BLINK_ROOT/artifacts/host-files/jit.txt"
dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true -o "$BLINK_ROOT/build/host-files-aot" > "$BLINK_ROOT/artifacts/host-files/aot-build.log" 2>&1
timeout 30 "$BLINK_ROOT/build/host-files-aot/HostFiles" > "$BLINK_ROOT/artifacts/host-files/aot.txt"
diff -u "$BLINK_ROOT/artifacts/host-files/jit.txt" "$BLINK_ROOT/artifacts/host-files/aot.txt"
cat "$BLINK_ROOT/artifacts/host-files/jit.txt"
