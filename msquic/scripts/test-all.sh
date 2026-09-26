#!/usr/bin/env bash
# Compatibility entrypoint for the complete declared verification selection.
source "$(dirname -- "${BASH_SOURCE[0]}")/common.sh"
campaign_exec verify "$@"
