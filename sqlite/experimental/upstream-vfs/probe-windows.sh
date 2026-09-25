#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/../../scripts/common.sh"
experiment="$SQLITE_ROOT/experimental/upstream-vfs"
python3 "$SQLITE_ROOT/scripts/prepare-host-source.py"
args=()
if [[ -n "${SQLITE_WINDOWS_HEADERS:-}" ]]; then args+=(-I "$SQLITE_WINDOWS_HEADERS"); fi
dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" "$experiment/windows.c" \
  -I "$SQLITE_ROOT/generated/sqlite-port" -I "$SQLITE_ROOT/src" "${args[@]}" \
  "${SQLITE_DEFINES[@]}" -DSQLITE_OS_OTHER=0 -DSQLITE_OS_WIN=1 -D_WIN32=1 \
  -DSQLITE_THREADSAFE=1 -DSQLITE_MUTEX_APPDEF=1 -DDOTCC_HOST_VFS=1 \
  -DSQLITE_MAX_WORKER_THREADS=0 -DSQLITE_WIN32_NO_ANSI=1 -DSQLITE_WIN32_USE_UUID=0 \
  -DSQLITE_DEFAULT_MMAP_SIZE=67108864 -DSQLITE_MAX_MMAP_SIZE=268435456 \
  --overrides-file "$experiment/windows-overrides.json" --emit=managedlib --class-name Sqlite --namespace Managed.Database.UpstreamWindows --split=function \
  -o "$SQLITE_ROOT/generated/UpstreamWindowsSqlite"
