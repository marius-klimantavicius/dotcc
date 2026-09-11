#!/usr/bin/env bash
set -euo pipefail
campaign=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
exec python3 "$campaign/scripts/test-independent-peer.py" "$@"
