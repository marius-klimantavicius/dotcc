#!/usr/bin/env bash
set -euo pipefail
SQLITE_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$SQLITE_ROOT/.." && pwd)"
PYTHON_CMD=python3
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then PYTHON_CMD=python; fi
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
  echo "Python 3 is required" >&2
  exit 1
fi
SQLITE_AMALGAMATION="$SQLITE_ROOT/ref/sqlite-amalgamation-3530400"
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
