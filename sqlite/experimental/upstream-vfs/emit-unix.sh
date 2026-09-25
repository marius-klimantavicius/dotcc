#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$0")/../../scripts/common.sh"
experiment="$SQLITE_ROOT/experimental/upstream-vfs"
out="$SQLITE_ROOT/generated/UpstreamUnixSqlite"
if [[ "$(uname -s)" != Linux || "$(uname -m)" != x86_64 ]]; then
  echo "The experimental Unix OS bindings currently require Linux x64." >&2
  exit 1
fi
python3 "$SQLITE_ROOT/scripts/prepare-host-source.py"
dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" "$experiment/unix.c" \
  -I "$experiment/include/unix" -I "$SQLITE_ROOT/generated/sqlite-port" -I "$SQLITE_ROOT/src" \
  "${SQLITE_DEFINES[@]}" -DSQLITE_OS_OTHER=0 -DSQLITE_OS_UNIX=1 \
  -DSQLITE_THREADSAFE=1 -DSQLITE_MUTEX_APPDEF=1 -DDOTCC_HOST_VFS=1 \
  -DSQLITE_DEFAULT_MMAP_SIZE=67108864 -DSQLITE_MAX_MMAP_SIZE=268435456 \
  -DHAVE_MREMAP=0 -DHAVE_READLINK=1 -DHAVE_LSTAT=1 -DHAVE_NANOSLEEP=0 -DHAVE_USLEEP=1 \
  --overrides-file "$experiment/unix-overrides.json" --emit=managedlib --class-name Sqlite --namespace Managed.Database.UpstreamUnix --split=function -o "$out"
cp "$experiment/managed/UnixNative.cs" "$out/UnixNative.cs"
sed 's/Managed.Database/Managed.Database.UpstreamUnix/g' "$SQLITE_ROOT/src/HostVfs.Mutex.cs" > "$out/HostMutex.cs"
cc -std=c11 -Wall -Wextra -Werror -shared -fPIC "$experiment/native/unix.c" -o "$out/libdotcc_sqlite_os.so"
python3 - "$out/UpstreamUnixSqlite.csproj" <<'PYPROJECT'
from pathlib import Path
import sys
project = Path(sys.argv[1])
project.write_text(project.read_text().replace('</Project>',
    '<ItemGroup><None Update="libdotcc_sqlite_os.so" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" /></ItemGroup>\n</Project>'))
PYPROJECT
dotnet build "$out/UpstreamUnixSqlite.csproj" -c Release --nologo
