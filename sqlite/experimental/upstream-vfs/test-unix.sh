#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/../../scripts/common.sh"
experiment="$SQLITE_ROOT/experimental/upstream-vfs"
"$experiment/emit-unix.sh"
dotnet build "$experiment/tests/UpstreamVfsTests.csproj" -c Release --nologo
run_sqlite_process dotnet "$experiment/tests/bin/Release/net10.0/UpstreamVfsTests.dll"
if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  dotnet publish "$experiment/tests/UpstreamVfsTests.csproj" -c Release -r linux-x64 \
    -p:PublishAot=true -o "$SQLITE_ROOT/build/upstream-unix-vfs-aot" --nologo
  run_sqlite_process "$SQLITE_ROOT/build/upstream-unix-vfs-aot/UpstreamVfsTests"
fi
