#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"
set -euo pipefail
root="$(cd -- "$(dirname -- "$0")/.." && pwd)"
exec "$PYTHON_CMD" "$root/scripts/test-managed-peer.py" "$@"
