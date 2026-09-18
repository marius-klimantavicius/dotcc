#!/usr/bin/env bash
set -euo pipefail
PICOTLS_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd -- "$PICOTLS_ROOT/.." && pwd)"
PYTHON_CMD=python3
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then PYTHON_CMD=python; fi
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
    echo "Python 3 is required" >&2
    exit 1
fi
PICOTLS_PROJECT="$PICOTLS_ROOT/generated/TranslatedPicotls/TranslatedPicotls.csproj"
PICOTLS_RAW_PROJECT="$PICOTLS_ROOT/generated/TranslatedPicotlsRaw/TranslatedPicotls.csproj"
PICOTLS_DEFINES=()
while IFS= read -r definition; do
    [[ -z "$definition" || "$definition" == \#* ]] || PICOTLS_DEFINES+=("-D$definition")
done < "$PICOTLS_ROOT/config/core-defines.txt"
mkdir -p "$PICOTLS_ROOT/generated" "$PICOTLS_ROOT/build" "$PICOTLS_ROOT/artifacts/tmp"
export TMPDIR="$PICOTLS_ROOT/artifacts/tmp"

require_picotls_project() {
    if [[ ! -f "$1" ]]; then
        echo "Missing translated project: $1. Run scripts/translate.sh first; compiler blockers are recorded in docs/blockers.md." >&2
        exit 1
    fi
}
