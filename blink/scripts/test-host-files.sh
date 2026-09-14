#!/usr/bin/env bash
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mkdir -p "$BLINK_ROOT/artifacts/host-files"
rm -f "$BLINK_ROOT/artifacts/host-files/receipt.json"
project="$BLINK_ROOT/tests/HostFiles/HostFiles.csproj"
dotnet build "$project" -c Release > "$BLINK_ROOT/artifacts/host-files/build.log" 2>&1
timeout 30 dotnet "$BLINK_ROOT/tests/HostFiles/bin/Release/net10.0/HostFiles.dll" > "$BLINK_ROOT/artifacts/host-files/jit.txt"
dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true -o "$BLINK_ROOT/build/host-files-aot" > "$BLINK_ROOT/artifacts/host-files/aot-build.log" 2>&1
timeout 30 "$BLINK_ROOT/build/host-files-aot/HostFiles" > "$BLINK_ROOT/artifacts/host-files/aot.txt"
diff -u "$BLINK_ROOT/artifacts/host-files/jit.txt" "$BLINK_ROOT/artifacts/host-files/aot.txt"
cat "$BLINK_ROOT/artifacts/host-files/jit.txt"

python3 - "$BLINK_ROOT" <<'PYRECEIPT'
import hashlib, json, pathlib, subprocess, sys
root = pathlib.Path(sys.argv[1]); artifact = root/'artifacts/host-files'
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
paths = list((root/'src/Managed.Emulation.Host').glob('*.cs')) + list((root/'tests/HostFiles').glob('*.cs'))
receipt = {'host':'linux-x64', 'scope':'independent host contract; guest callback integration pending',
 'sdk':subprocess.check_output(['dotnet','--version'],text=True).strip(),
 'sources':{str(p.relative_to(root)):sha(p) for p in sorted(paths)},
 'jit_assembly':sha(root/'tests/HostFiles/bin/Release/net10.0/HostFiles.dll'),
 'host_assembly':sha(root/'tests/HostFiles/bin/Release/net10.0/Managed.Emulation.Host.dll'),
 'nativeaot_executable':sha(root/'build/host-files-aot/HostFiles'),
 'outputs':{p:sha(artifact/p) for p in ['jit.txt','aot.txt']}}
(artifact/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
PYRECEIPT
