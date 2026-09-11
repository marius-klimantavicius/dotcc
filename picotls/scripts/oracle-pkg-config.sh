#!/usr/bin/env bash
# Upstream has no WITH_BROTLI switch: decline exactly its two optional probes.
set -euo pipefail
for argument in "$@"; do
    case "$argument" in
        libbrotlienc|libbrotlidec) exit 1 ;;
    esac
done
exec /usr/bin/pkg-config "$@"
