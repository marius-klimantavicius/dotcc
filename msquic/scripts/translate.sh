#!/usr/bin/env bash
# Regenerate the selected C core, nest its API types, and postprocess the product.
set -euo pipefail
msquic_root=$(cd -- "$(dirname -- "$0")/.." && pwd)
repo_root=$(dirname -- "$msquic_root")
build_tools=true
case "${1:-}" in
  --no-build-tools) build_tools=false; shift ;;
  --help|-h) echo "Usage: $0 [--no-build-tools]"; exit 0 ;;
esac
if (( $# )); then echo "Usage: $0 [--no-build-tools]" >&2; exit 1; fi

# Preserve the old closure before replacing cached objects or generated files.
python3 "$msquic_root/scripts/freeze-product.py" --archive-current
if "$build_tools"; then
  dotnet build "$repo_root/DotCC/DotCC.csproj" -c Release --nologo
  dotnet build "$repo_root/DotCC.PostProcess/DotCC.PostProcess.csproj" -c Release --nologo
fi
python3 "$msquic_root/scripts/fetch.py"
python3 "$msquic_root/scripts/test-host-contract.py"
python3 "$msquic_root/scripts/test-abi.py" --groups public \
  --compiler "$msquic_root/build/host-contract/compiler/dotcc.dll"
python3 "$msquic_root/scripts/build-product.py"
# This regeneration does not rerun or relabel historical SQLite/runtime evidence.
python3 "$msquic_root/scripts/freeze-product.py" --without-sqlite
