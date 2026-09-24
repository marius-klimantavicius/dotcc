#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/common.sh"
fetch_args=()
translate_args=()
while (($#)); do
  case "$1" in
    --no-fetch) fetch_args+=(--no-fetch); translate_args+=(--no-fetch); shift ;;
    --no-build-tools) translate_args+=(--no-build-tools); shift ;;
    --jobs)
      if (($# < 2)); then echo "--jobs requires a value" >&2; exit 2; fi
      translate_args+=(--jobs "$2"); shift 2 ;;
    *) echo "Usage: $0 [--no-fetch] [--no-build-tools] [--jobs N]" >&2; exit 2 ;;
  esac
done
"$PYTHON_CMD" -m unittest discover -s "$VALKEY_ROOT/tests"
"$PYTHON_CMD" "$VALKEY_ROOT/scripts/oracle.py" "${fetch_args[@]}"
"$PYTHON_CMD" "$VALKEY_ROOT/scripts/translate.py" "${translate_args[@]}"
"$VALKEY_ROOT/scripts/build.sh"
for variant in raw processed; do
  "$PYTHON_CMD" "$VALKEY_ROOT/scripts/validate_managed.py" \
    --variant "$variant" --native-compare --persistence-exchange --upstream-protocol --aot
done
