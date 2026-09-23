#!/usr/bin/env bash
set -euo pipefail
# Use Libc sockets/poll and upstream integer descriptors throughout translation.
exec "$(dirname -- "$0")/translate.sh" "$@" --profile legacy
