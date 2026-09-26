#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"
set -euo pipefail
source "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/legacy-common.sh"
exec "$PYTHON_CMD" "$VALKEY_ROOT/scripts/oracle.py" "$@"
