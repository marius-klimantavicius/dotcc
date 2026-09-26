#!/usr/bin/env bash
set -euo pipefail
VALKEY_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$VALKEY_ROOT/.." && pwd)"
