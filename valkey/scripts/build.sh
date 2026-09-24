#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/common.sh"
exec dotnet build "$VALKEY_ROOT/ManagedConsumer.slnx" -c Release --nologo "$@"
