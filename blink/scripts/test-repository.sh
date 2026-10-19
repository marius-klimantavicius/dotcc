#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/production-common.sh"
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$BLINK_ROOT/.." && pwd)"
export TMPDIR="$BLINK_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR" "$BLINK_ROOT/artifacts"
"${PRODUCTION_COMMAND[@]}" dotnet build "$DOTCC_ROOT/dotcc.sln" -c Release -p:UseLocalLalrCc=false
"${PRODUCTION_COMMAND[@]}" dotnet test "$DOTCC_ROOT/DotCC.Tests/DotCC.Tests.csproj" -c Release --no-build --blame-hang-timeout 300s
"${PRODUCTION_COMMAND[@]}" dotnet test "$DOTCC_ROOT/DotCC.FunctionalTests/DotCC.FunctionalTests.csproj" -c Release --no-build --blame-hang-timeout 300s
