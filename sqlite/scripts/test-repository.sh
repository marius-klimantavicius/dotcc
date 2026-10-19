#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/production-common.sh"
source "$(dirname "$0")/legacy-common.sh"
# Unit fixtures create C files in the temporary directory. Include discovery
# recursively scans that directory; isolate it from unrelated host temporary data.
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
"${PRODUCTION_COMMAND[@]}" dotnet build "$DOTCC_ROOT/dotcc.sln" -c Release -p:UseLocalLalrCc=false
"${PRODUCTION_COMMAND[@]}" dotnet test "$DOTCC_ROOT/DotCC.Tests/DotCC.Tests.csproj" -c Release --no-build --blame-hang-timeout 300s
"${PRODUCTION_COMMAND[@]}" dotnet test "$DOTCC_ROOT/DotCC.FunctionalTests/DotCC.FunctionalTests.csproj" -c Release --no-build --blame-hang-timeout 300s
