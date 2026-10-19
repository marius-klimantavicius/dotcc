#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/common.sh"
# Product tests consume the selected existing delivery; they do not regenerate it.
if [[ "${DOTCC_CAMPAIGN_EXISTING:-0}" == 1 ]]; then
    "$PYTHON_CMD" "$CAMPAIGN_ROOT/scripts/product_identity.py"
    exit 0
fi
campaign_exec translate "$@"
