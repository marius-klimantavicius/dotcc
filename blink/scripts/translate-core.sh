#!/usr/bin/env bash
# Preserve initial complete-core diagnostics; this is not a successful port.
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$BLINK_ROOT/.." && pwd)"
UPSTREAM="$BLINK_ROOT/ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580"
mkdir -p "$BLINK_ROOT/generated/decoder-profile" "$BLINK_ROOT/artifacts"
cp "$BLINK_ROOT/config/decoder-config.h" "$BLINK_ROOT/generated/decoder-profile/config.h"
exec dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" -std=c17 -DNDEBUG \
  -I "$BLINK_ROOT/generated/decoder-profile" -I "$UPSTREAM" "$UPSTREAM/blink/machine.c" \
  --emit=managedlib --nest-types --runtime=c --split=size --split-size=102400 \
  --class-name Blink --namespace Managed.Emulation -o "$BLINK_ROOT/generated/TranslatedBlink"
