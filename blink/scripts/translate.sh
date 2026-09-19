#!/usr/bin/env bash
# Deliver the complete selected core and normal semantic post-processing.
set -euo pipefail
campaign=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
exec python3 "$campaign/scripts/translate.py" "$@"
