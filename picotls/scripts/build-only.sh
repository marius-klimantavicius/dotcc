#!/usr/bin/env bash
# Build an existing translation. No fetching, translation, test or publish steps.
source "$(dirname -- "$0")/common.sh"
project="$PICOTLS_PROJECT"
provider="$PICOTLS_ROOT/src/BclProvider/BclProvider.csproj"
[[ ! -f "$provider" ]] || project="$provider"
case "${1:-}" in
    --raw) project="$PICOTLS_RAW_PROJECT"; shift ;;
    --core) project="$PICOTLS_PROJECT"; shift ;;
    --help|-h) echo "Usage: $0 [--core|--raw] (default: owning BclProvider project when present)"; exit 0 ;;
esac
if (( $# )); then echo "Usage: $0 [--core|--raw]" >&2; exit 1; fi
if [[ "$project" == "$provider" ]]; then require_picotls_project "$PICOTLS_PROJECT"; fi
require_picotls_project "$project"
exec dotnet build "$project" -c Release --nologo
