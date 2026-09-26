#!/usr/bin/env bash
# Regenerate the managed SQLite library using the shared translation pipeline.
# Forward options such as --no-postprocess and --help unchanged.
exec bash "$(dirname "$0")/emit-engine.sh" "$@"
