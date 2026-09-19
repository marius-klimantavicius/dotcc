#!/usr/bin/env bash
set -euo pipefail
libsmb2_root=$(cd -- "$(dirname -- "$0")/.." && pwd)
exec python3 "$libsmb2_root/scripts/test.py" "$@"
