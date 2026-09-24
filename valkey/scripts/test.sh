#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/common.sh"
"$PYTHON_CMD" -m unittest discover -s "$VALKEY_ROOT/tests"
exec "$PYTHON_CMD" "$VALKEY_ROOT/scripts/validate_managed.py" "$@"
