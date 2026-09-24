#!/usr/bin/env bash
set -euo pipefail
VALKEY_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$VALKEY_ROOT/.." && pwd)"
PYTHON_CMD=python3
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then PYTHON_CMD=python; fi
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
  echo "Python 3 is required" >&2
  exit 1
fi
