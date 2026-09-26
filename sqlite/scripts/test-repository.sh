#!/usr/bin/env bash
source "$(dirname "$0")/legacy-common.sh"
# Unit fixtures create C files in the temporary directory. Include discovery
# recursively scans that directory; isolate it from unrelated host temporary data.
export TMPDIR="$SQLITE_ROOT/artifacts/tmp"
mkdir -p "$TMPDIR"
dotnet build "$DOTCC_ROOT/dotcc.sln" -c Release -p:UseLocalLalrCc=false
dotnet test "$DOTCC_ROOT/DotCC.Tests/DotCC.Tests.csproj" -c Release --no-build --blame-hang-timeout 300s
dotnet test "$DOTCC_ROOT/DotCC.FunctionalTests/DotCC.FunctionalTests.csproj" -c Release --no-build --blame-hang-timeout 300s
