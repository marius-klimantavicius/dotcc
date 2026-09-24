#!/usr/bin/env bash
set -euo pipefail
source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/common.sh"
exec "$PYTHON_CMD" "$VALKEY_ROOT/scripts/host_oracle.py" "$@"
