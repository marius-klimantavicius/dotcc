#!/usr/bin/env bash
set -euo pipefail
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/production-common.sh"
SQLITE_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$SQLITE_ROOT/.." && pwd)"
DOTCC_COMPILER="${DOTCC_COMPILER:-$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll}"
SQLITE_AMALGAMATION="$SQLITE_ROOT/ref/$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["product"]["directory"])' "$SQLITE_ROOT/config/sources.json")"
SQLITE_DEFINES=()
while IFS= read -r definition; do
  [[ -z "$definition" || "$definition" == \#* ]] || SQLITE_DEFINES+=("-D$definition")
done < "$SQLITE_ROOT/config/defines.txt"
mkdir -p "$SQLITE_ROOT/build" "$SQLITE_ROOT/generated" "$SQLITE_ROOT/artifacts"
SQLITE_NATIVE_FLAGS=()
while IFS= read -r native_flag; do
  [[ -z "$native_flag" || "$native_flag" == \#* ]] || SQLITE_NATIVE_FLAGS+=("$native_flag")
done < "$SQLITE_ROOT/config/native-flags.txt"

# A generated runtime defect must fail a corpus run instead of hanging CI.
# Compilation/publishing keeps its separate build budget.
run_sqlite_process() {
  timeout --kill-after=10s "${SQLITE_EXECUTION_TIMEOUT:-120}s" "$@"
}
