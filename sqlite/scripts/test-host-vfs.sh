#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"
# Linux campaign entry; the dedicated CI workflow also runs Windows/macOS.
source "$(dirname "$0")/legacy-common.sh"
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
if [[ $# != 0 ]]; then
  echo "Usage: $0" >&2
  exit 2
fi

"$SQLITE_ROOT/scripts/emit-engine.sh" > "$SQLITE_ROOT/artifacts/host-vfs-emission.log" 2>&1
project="$SQLITE_ROOT/tests/HostVfsTests/HostVfsTests.csproj"
dotnet run --project "$SQLITE_ROOT/tests/HostVfsPlatformTests/HostVfsPlatformTests.csproj" \
  -c Release --no-launch-profile
dotnet build "$project" -c Release --nologo > "$SQLITE_ROOT/artifacts/host-vfs-build.log" 2>&1
run_sqlite_process dotnet "$SQLITE_ROOT/tests/HostVfsTests/bin/Release/net10.0/HostVfsTests.dll"

# This separate native oracle uses SQLite's real default OS VFS. It never becomes
# a dependency of the translated library, and does not use SQLITE_OS_OTHER.
gcc -std=c17 -O1 -DSQLITE_THREADSAFE=0 -DSQLITE_OMIT_LOAD_EXTENSION \
  -DSQLITE_ENABLE_FTS5 -I "$SQLITE_AMALGAMATION" \
  "$SQLITE_AMALGAMATION/sqlite3.c" "$SQLITE_ROOT/tests/host_vfs_native.c" \
  -lm -o "$SQLITE_ROOT/build/host-vfs-native"
"$PYTHON_CMD" "$SQLITE_ROOT/scripts/test-host-vfs-processes.py" \
  --managed dotnet "$SQLITE_ROOT/tests/HostVfsTests/bin/Release/net10.0/HostVfsTests.dll" \
  --native "$SQLITE_ROOT/build/host-vfs-native"

if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true \
    -o "$SQLITE_ROOT/build/host-vfs-aot" --nologo \
    > "$SQLITE_ROOT/artifacts/host-vfs-aot-build.log" 2>&1
  run_sqlite_process "$SQLITE_ROOT/build/host-vfs-aot/HostVfsTests"
  "$PYTHON_CMD" "$SQLITE_ROOT/scripts/test-host-vfs-processes.py" \
    --managed "$SQLITE_ROOT/build/host-vfs-aot/HostVfsTests" \
    --native "$SQLITE_ROOT/build/host-vfs-native"
fi
