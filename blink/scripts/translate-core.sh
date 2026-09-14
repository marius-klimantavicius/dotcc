#!/usr/bin/env bash
# Run the native embedding oracle, then translate its verified staged closure.
# A successful emission alone does not qualify managed execution.
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
exec "$BLINK_ROOT/scripts/probe-core.sh" "$@"
