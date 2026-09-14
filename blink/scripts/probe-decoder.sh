#!/usr/bin/env bash
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$BLINK_ROOT/.." && pwd)"
UPSTREAM="$BLINK_ROOT/ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580"
mkdir -p "$BLINK_ROOT/generated/decoder-profile" "$BLINK_ROOT/artifacts/decoder" "$BLINK_ROOT/build"
python3 "$BLINK_ROOT/scripts/stage-decoder.py"
args=(-std=c17 -DNDEBUG -I "$BLINK_ROOT/generated/decoder-profile" -I "$UPSTREAM")
inputs=("$UPSTREAM/blink/x86.c" "$UPSTREAM/blink/bitscan.c" "$BLINK_ROOT/tests/Decoder/probe.c")
cc -D_GNU_SOURCE "${args[@]}" "${inputs[@]}" -o "$BLINK_ROOT/build/decoder-native"
timeout 30 "$BLINK_ROOT/build/decoder-native" > "$BLINK_ROOT/artifacts/decoder/native.txt"
inputs[0]="$BLINK_ROOT/generated/decoder-profile/x86.c"
cc -D_GNU_SOURCE "${args[@]}" "${inputs[@]}" -o "$BLINK_ROOT/build/decoder-staged-native"
timeout 30 "$BLINK_ROOT/build/decoder-staged-native" > "$BLINK_ROOT/artifacts/decoder/staged-native.txt"
diff -u "$BLINK_ROOT/artifacts/decoder/native.txt" "$BLINK_ROOT/artifacts/decoder/staged-native.txt"
dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" "${args[@]}" "${inputs[@]}" --runtime=c -o "$BLINK_ROOT/generated/DecoderProbe" > "$BLINK_ROOT/artifacts/decoder/translate.log" 2>&1
dotnet build "$BLINK_ROOT/generated/DecoderProbe" -c Release > "$BLINK_ROOT/artifacts/decoder/build.log" 2>&1
timeout 30 dotnet "$BLINK_ROOT/generated/DecoderProbe/bin/Release/net10.0/DecoderProbe.dll" > "$BLINK_ROOT/artifacts/decoder/managed.txt"
diff -u "$BLINK_ROOT/artifacts/decoder/native.txt" "$BLINK_ROOT/artifacts/decoder/managed.txt"
