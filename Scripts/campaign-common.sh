#!/usr/bin/env bash
# Sourceable support for every campaign command. No build/fetch side effects.
set -euo pipefail
CAMPAIGN_REPO="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
PYTHON_CMD=python3
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
    PYTHON_CMD=python
fi
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
    echo "Python 3 is required" >&2
    exit 1
fi
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info < (3, 11))' >/dev/null 2>&1; then
    echo "Python 3.11 or newer is required" >&2
    exit 1
fi
export PYTHON_CMD
export PYTHONPATH="$CAMPAIGN_REPO/Scripts${PYTHONPATH:+:$PYTHONPATH}"
campaign_exec() {
    local action="$1"
    shift
    exec "$PYTHON_CMD" "$CAMPAIGN_REPO/Scripts/campaign.py" "$action" "${CAMPAIGN_PROJECT:?Project binding missing}" "$@"
}
