#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
exec "$PYTHON_CMD" "$VALKEY_ROOT/scripts/translate.py" "$@"
