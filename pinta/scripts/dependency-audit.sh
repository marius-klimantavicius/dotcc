#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"
set -euo pipefail
exec "$PYTHON_CMD" "$(dirname -- "$0")/dependency-audit.py" "$@"
