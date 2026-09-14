#!/usr/bin/env bash
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTCC_ROOT="$(cd "$BLINK_ROOT/.." && pwd)"
UPSTREAM="$BLINK_ROOT/ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580"
mkdir -p "$BLINK_ROOT/generated/decoder-profile" "$BLINK_ROOT/artifacts/decoder" "$BLINK_ROOT/build"
rm -f "$BLINK_ROOT/artifacts/decoder/receipt.json"
python3 "$BLINK_ROOT/scripts/stage-decoder.py"
args=(-std=c17 -DNDEBUG -I "$BLINK_ROOT/generated/decoder-profile" -I "$UPSTREAM")
inputs=("$UPSTREAM/blink/x86.c" "$UPSTREAM/blink/bitscan.c" "$BLINK_ROOT/tests/Decoder/probe.c")
cc -D_GNU_SOURCE "${args[@]}" "${inputs[@]}" -o "$BLINK_ROOT/build/decoder-native"
timeout 30 "$BLINK_ROOT/build/decoder-native" > "$BLINK_ROOT/artifacts/decoder/native.txt"
inputs[0]="$BLINK_ROOT/generated/decoder-profile/x86.c"
inputs[1]="$BLINK_ROOT/generated/decoder-profile/bitscan.c"
cc -D_GNU_SOURCE "${args[@]}" "${inputs[@]}" -o "$BLINK_ROOT/build/decoder-staged-native"
timeout 30 "$BLINK_ROOT/build/decoder-staged-native" > "$BLINK_ROOT/artifacts/decoder/staged-native.txt"
diff -u "$BLINK_ROOT/artifacts/decoder/native.txt" "$BLINK_ROOT/artifacts/decoder/staged-native.txt"
dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll" "${args[@]}" "${inputs[@]}" --runtime=c -o "$BLINK_ROOT/generated/DecoderProbe" > "$BLINK_ROOT/artifacts/decoder/translate.log" 2>&1
dotnet build "$BLINK_ROOT/generated/DecoderProbe" -c Release > "$BLINK_ROOT/artifacts/decoder/build.log" 2>&1
timeout 30 dotnet "$BLINK_ROOT/generated/DecoderProbe/bin/Release/net10.0/DecoderProbe.dll" > "$BLINK_ROOT/artifacts/decoder/managed.txt"
diff -u "$BLINK_ROOT/artifacts/decoder/native.txt" "$BLINK_ROOT/artifacts/decoder/managed.txt"
# Preserve an immutable-by-convention raw generation snapshot before optimization.
python3 - "$BLINK_ROOT" <<'PY'
import pathlib, shutil, sys
root = pathlib.Path(sys.argv[1]); generated = root/'generated'
for name in ['DecoderRaw', 'DecoderOptimized']:
    target = generated/name
    if target.exists(): shutil.rmtree(target)
    shutil.copytree(generated/'DecoderProbe', target, ignore=shutil.ignore_patterns('bin', 'obj'))
PY
dotnet publish "$BLINK_ROOT/generated/DecoderRaw/DecoderProbe.csproj" -c Release -r linux-x64 -p:PublishAot=true -o "$BLINK_ROOT/build/decoder-raw-aot" > "$BLINK_ROOT/artifacts/decoder/raw-aot-build.log" 2>&1
timeout 30 "$BLINK_ROOT/build/decoder-raw-aot/DecoderProbe" > "$BLINK_ROOT/artifacts/decoder/raw-aot.txt"
diff -u "$BLINK_ROOT/artifacts/decoder/native.txt" "$BLINK_ROOT/artifacts/decoder/raw-aot.txt"
dotnet restore "$BLINK_ROOT/generated/DecoderOptimized/DecoderProbe.csproj" > "$BLINK_ROOT/artifacts/decoder/optimized-restore.log" 2>&1
dotnet "$DOTCC_ROOT/DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll" "$BLINK_ROOT/generated/DecoderOptimized/DecoderProbe.csproj" --in-place > "$BLINK_ROOT/artifacts/decoder/postprocess.log" 2>&1
dotnet build "$BLINK_ROOT/generated/DecoderOptimized/DecoderProbe.csproj" -c Release > "$BLINK_ROOT/artifacts/decoder/optimized-build.log" 2>&1
timeout 30 dotnet "$BLINK_ROOT/generated/DecoderOptimized/bin/Release/net10.0/DecoderProbe.dll" > "$BLINK_ROOT/artifacts/decoder/optimized-jit.txt"
diff -u "$BLINK_ROOT/artifacts/decoder/native.txt" "$BLINK_ROOT/artifacts/decoder/optimized-jit.txt"
dotnet publish "$BLINK_ROOT/generated/DecoderOptimized/DecoderProbe.csproj" -c Release -r linux-x64 -p:PublishAot=true -o "$BLINK_ROOT/build/decoder-optimized-aot" > "$BLINK_ROOT/artifacts/decoder/optimized-aot-build.log" 2>&1
timeout 30 "$BLINK_ROOT/build/decoder-optimized-aot/DecoderProbe" > "$BLINK_ROOT/artifacts/decoder/optimized-aot.txt"
diff -u "$BLINK_ROOT/artifacts/decoder/native.txt" "$BLINK_ROOT/artifacts/decoder/optimized-aot.txt"
python3 - "$BLINK_ROOT" "$DOTCC_ROOT" <<'PY'
import hashlib, json, pathlib, sys
root, repo = map(pathlib.Path, sys.argv[1:])
hashfile = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
receipt = {'scope':'decoder-only; no guest instructions executed', 'host':'linux-x64',
           'compiler_sha256':hashfile(repo/'DotCC/bin/Release/net10.0/DotCC.Lib.dll'),
           'config_sha256':hashfile(root/'config/decoder-config.h'),
           'harness_sha256':hashfile(root/'tests/Decoder/probe.c'),
           'adaptation':json.loads((root/'generated/decoder-profile/adaptation.json').read_text()),
           'results':{name:hashfile(root/'artifacts/decoder'/name) for name in ['native.txt','staged-native.txt','managed.txt','raw-aot.txt','optimized-jit.txt','optimized-aot.txt']},
           'generated':{str(p.relative_to(root/'generated')):hashfile(p) for name in ['DecoderRaw','DecoderOptimized'] for p in (root/'generated'/name).glob('*.cs')}}
(root/'artifacts/decoder/receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
print('decoder native/raw/optimized JIT/AOT outputs match')
PY
