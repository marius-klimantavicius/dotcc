#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"
source "$(dirname "$0")/legacy-common.sh"
exec "$PYTHON_CMD" "$VALKEY_ROOT/scripts/validate_managed.py" "$@"
