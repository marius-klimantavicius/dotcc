#!/usr/bin/env bash
# Regenerate the selected C core, nest its API types, and postprocess the product.
set -euo pipefail
msquic_root=$(cd -- "$(dirname -- "$0")/.." && pwd)
repo_root=$(dirname -- "$msquic_root")
python_cmd=python3
if ! "$python_cmd" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then python_cmd=python; fi
if ! "$python_cmd" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
  echo "$0: Python 3 is required" >&2
  exit 1
fi
build_tools=true
fast=false
jobs=()
while (( $# )); do
  case "$1" in
    --fast) fast=true; shift ;;
    --no-build-tools) build_tools=false; shift ;;
    --jobs)
      if (( $# < 2 )); then echo "Usage: $0 [--fast] [--no-build-tools] [--jobs count]" >&2; exit 1; fi
      jobs=(--jobs "$2"); shift 2 ;;
    --help|-h) echo "Usage: $0 [--fast] [--no-build-tools] [--jobs count]"; exit 0 ;;
    *) echo "Usage: $0 [--fast] [--no-build-tools] [--jobs count]" >&2; exit 1 ;;
  esac
done

if ! "$fast" && (( ${#jobs[@]} )); then
  echo "$0: --jobs requires --fast" >&2
  exit 1
fi

if "$fast"; then
  if "$build_tools"; then
    dotnet build "$repo_root/DotCC/DotCC.csproj" -c Release --nologo
    dotnet build "$repo_root/DotCC.PostProcess/DotCC.PostProcess.csproj" -c Release --nologo
  fi
  exec "$python_cmd" "$msquic_root/scripts/translate-fast.py" "${jobs[@]}"
fi

# Preserve the old closure before replacing cached objects or generated files.
"$python_cmd" "$msquic_root/scripts/freeze-product.py" --archive-current
if "$build_tools"; then
  dotnet build "$repo_root/DotCC/DotCC.csproj" -c Release --nologo
  dotnet build "$repo_root/DotCC.PostProcess/DotCC.PostProcess.csproj" -c Release --nologo
fi
"$python_cmd" "$msquic_root/scripts/fetch.py"
"$python_cmd" "$msquic_root/scripts/test-host-contract.py"
"$python_cmd" "$msquic_root/scripts/test-abi.py" --groups public
"$python_cmd" "$msquic_root/scripts/build-product.py"
# This regeneration does not rerun or relabel historical SQLite/runtime evidence.
"$python_cmd" "$msquic_root/scripts/freeze-product.py" --without-sqlite
