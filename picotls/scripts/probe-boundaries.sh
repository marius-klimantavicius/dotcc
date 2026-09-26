#!/usr/bin/env bash
set -euo pipefail
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"
campaign=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
source_dir=$("$PYTHON_CMD" "$CAMPAIGN_REPO/Scripts/campaign-reference.py" picotls)
artifacts="$campaign/artifacts/p1/boundaries"
export TMPDIR="$campaign/artifacts/tmp/p1"
mkdir -p "$artifacts" "$TMPDIR"
timeout --kill-after=10s 120s cc -std=c11 -DPTLS_HAVE_LOG=0 -DPICOTLS_USE_DTRACE=0 -I"$source_dir/include" "$campaign/tests/native-layout.c" -o "$artifacts/native-layout"
timeout --kill-after=10s 30s "$artifacts/native-layout" > "$artifacts/native-layout.txt"
timeout --kill-after=10s 180s dotnet run --project "$campaign/tests/BoundaryProbes/BoundaryProbes.csproj" -c Release -- "$artifacts/native-layout.txt" 2>&1 | tee "$artifacts/managed.log"
