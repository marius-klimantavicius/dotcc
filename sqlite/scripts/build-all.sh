#!/usr/bin/env bash
# Compatibility name for SQLite's original fetch/build/translate composite.
source "$(dirname -- "${BASH_SOURCE[0]}")/common.sh"
campaign_exec translate "$@"
