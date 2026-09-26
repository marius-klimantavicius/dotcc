#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"
set -euo pipefail
libsmb2_root=$(cd -- "$(dirname -- "$0")/.." && pwd)
exec "$PYTHON_CMD" "$libsmb2_root/scripts/crypto-abi.py" "$@"
