#!/usr/bin/env bash
set -euo pipefail
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
mkdir -p "$root/artifacts/instance-io"
rm -f "$root/artifacts/instance-io/receipt.json"
project="$root/tests/InstanceIo/InstanceIo.csproj"
dotnet build "$project" -c Release > "$root/artifacts/instance-io/build.log" 2>&1
timeout --kill-after=5s 30 dotnet "$root/tests/InstanceIo/bin/Release/net10.0/InstanceIo.dll" > "$root/artifacts/instance-io/jit.txt"
dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true -o "$root/build/instance-io-aot" > "$root/artifacts/instance-io/aot-build.log" 2>&1
timeout --kill-after=5s 30 "$root/build/instance-io-aot/InstanceIo" > "$root/artifacts/instance-io/aot.txt"
diff -u "$root/artifacts/instance-io/jit.txt" "$root/artifacts/instance-io/aot.txt"
cat "$root/artifacts/instance-io/jit.txt"

python3 - "$root" <<'PYRECEIPT'
import hashlib,json,pathlib,subprocess,sys
root=pathlib.Path(sys.argv[1]); out=root/'artifacts/instance-io'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
inputs=list((root/'src/Managed.Emulation.Host').glob('*.cs'))+list((root/'tests/InstanceIo').glob('*.cs'))
r={'scope':'typed instance I/O; guest callback integration pending','host':'linux-x64',
'sdk':subprocess.check_output(['dotnet','--version'],text=True).strip(),
'inputs':{str(p.relative_to(root)):sha(p) for p in sorted(inputs)},
'jitAssembly':sha(root/'tests/InstanceIo/bin/Release/net10.0/InstanceIo.dll'),
'hostAssembly':sha(root/'tests/InstanceIo/bin/Release/net10.0/Managed.Emulation.Host.dll'),
'nativeaotExecutable':sha(root/'build/instance-io-aot/InstanceIo'),
'outputs':{name:sha(out/name) for name in ['jit.txt','aot.txt']}}
(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
PYRECEIPT
