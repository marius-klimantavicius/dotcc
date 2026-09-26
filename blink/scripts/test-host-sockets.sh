#!/usr/bin/env bash
source "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"
set -euo pipefail
BLINK_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mkdir -p "$BLINK_ROOT/artifacts/host-sockets"
rm -f "$BLINK_ROOT/artifacts/host-sockets/receipt.json"
project="$BLINK_ROOT/tests/HostSockets/HostSockets.csproj"
dotnet build "$project" -c Release > "$BLINK_ROOT/artifacts/host-sockets/build.log" 2>&1
timeout --kill-after=5s 30 dotnet "$BLINK_ROOT/tests/HostSockets/bin/Release/net10.0/HostSockets.dll" > "$BLINK_ROOT/artifacts/host-sockets/jit.txt"
dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true -o "$BLINK_ROOT/build/host-sockets-aot" > "$BLINK_ROOT/artifacts/host-sockets/aot-build.log" 2>&1
timeout --kill-after=5s 30 "$BLINK_ROOT/build/host-sockets-aot/HostSockets" > "$BLINK_ROOT/artifacts/host-sockets/aot.txt"
diff -u "$BLINK_ROOT/artifacts/host-sockets/jit.txt" "$BLINK_ROOT/artifacts/host-sockets/aot.txt"
cat "$BLINK_ROOT/artifacts/host-sockets/jit.txt"

"$PYTHON_CMD" - "$BLINK_ROOT" <<'PYRECEIPT'
import hashlib, json, pathlib, subprocess, sys
root = pathlib.Path(sys.argv[1]); artifact = root/'artifacts/host-sockets'
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
paths = list((root/'src/Managed.Emulation.Host').glob('*.cs')) + list((root/'tests/HostSockets').glob('*.cs'))
receipt = {'host':'linux-x64', 'scope':'independent host contract; guest callback integration pending',
 'sdk':subprocess.check_output(['dotnet','--version'],text=True).strip(),
 'sources':{str(p.relative_to(root)):sha(p) for p in sorted(paths)},
 'jit_assembly':sha(root/'tests/HostSockets/bin/Release/net10.0/HostSockets.dll'),
 'host_assembly':sha(root/'tests/HostSockets/bin/Release/net10.0/Managed.Emulation.Host.dll'),
 'nativeaot_executable':sha(root/'build/host-sockets-aot/HostSockets'),
 'outputs':{p:sha(artifact/p) for p in ['jit.txt','aot.txt']}}
(artifact/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
PYRECEIPT
