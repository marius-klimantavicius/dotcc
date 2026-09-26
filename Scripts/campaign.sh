#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/campaign-common.sh"
exec "$PYTHON_CMD" "$CAMPAIGN_REPO/Scripts/campaign.py" "$@"
