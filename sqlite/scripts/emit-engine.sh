#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/common.sh"
# Product tests consume the selected existing delivery; they do not regenerate it.
if [[ "${DOTCC_CAMPAIGN_EXISTING:-0}" == 1 ]]; then
    [[ -f "$CAMPAIGN_ROOT/generated/TranslatedSqlite/TranslatedSqlite.csproj" ]] || {
        echo "Missing generated SQLite library; run translate.sh first" >&2
        exit 1
    }
    exit 0
fi
campaign_exec translate "$@"
