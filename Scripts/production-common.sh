#!/usr/bin/env bash
# Individual legacy production commands only; never wrap an entire suite.
source "$(dirname -- "${BASH_SOURCE[0]}")/campaign-common.sh"
PRODUCTION_COMMAND=("$PYTHON_CMD" "$CAMPAIGN_REPO/Scripts/production_support.py")
"${PRODUCTION_COMMAND[@]}" --preflight
