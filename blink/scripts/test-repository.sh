#!/usr/bin/env bash
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$BLINK_ROOT/.." && pwd)"
export TMPDIR="$BLINK_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR" "$BLINK_ROOT/artifacts"
dotnet build "$DOTCC_ROOT/dotcc.sln" -c Release -p:UseLocalLalrCc=false
dotnet test "$DOTCC_ROOT/DotCC.Tests/DotCC.Tests.csproj" -c Release --no-build --blame-hang-timeout 300s
dotnet test "$DOTCC_ROOT/DotCC.FunctionalTests/DotCC.FunctionalTests.csproj" -c Release --no-build --blame-hang-timeout 300s
