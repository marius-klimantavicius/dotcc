#!/usr/bin/env bash
set -euo pipefail
campaign=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
repo=$(cd "$campaign/.." && pwd)
native="$campaign/build/native/source"
upstream="$campaign/ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580"
out="$campaign/artifacts/core"
mkdir -p "$out" "$campaign/generated/core-profile"
[[ -f "$native/o/blink/blink.a" ]] || { echo 'Run scripts/native-oracle.sh first' >&2; exit 1; }
timeout 60 cc -D_GNU_SOURCE -D_DEFAULT_SOURCE -DNOLINEAR -I "$native" \
  "$campaign/src/core-probe/probe.c" "$native/o/blink/blink.a" -lz -lrt -lm -pthread \
  -Wl,-Map="$out/native-link.map" -o "$campaign/build/core-native" > "$out/native-build.log" 2>&1
timeout 15 "$campaign/build/core-native" > "$out/native.txt" 2> "$out/native.stderr"
# Linker extraction establishes an observed native dependency closure; it is
# not yet the final managed profile and does not qualify host dependencies.
python3 - "$campaign" <<'PY'
import hashlib, json, pathlib, re, sys
p = pathlib.Path(sys.argv[1]); out = p/'artifacts/core'
objects = sorted(set(re.findall(r'blink\.a\(([^()]+)\.o\)', (out/'native-link.map').read_text())))
upstream = p/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
sources = []
inventory = json.loads((p/'config/source-inventory.json').read_text())
pins = {row['path']: row['sha256'] for row in inventory['files']}
native_config=(p/'build/native/source/config.h').read_text()
core_config=(p/'config/core-config.h').read_text()
exclusions=lambda content:set(re.findall(r'^#define\s+(DISABLE_[A-Z0-9_]+)\b',content,re.M))
if exclusions(native_config) != exclusions(core_config):
    raise SystemExit('core/native feature exclusions differ; review both profiles')
if re.search(r'^#define\s+HAVE_FORK\b',native_config+'\n'+core_config,re.M):
    raise SystemExit('guest fork must remain excluded')
stage = p/'generated/core-profile/source'
stage.mkdir(parents=True, exist_ok=True)
for obj in objects:
    source = upstream/'blink'/(pathlib.Path(obj).name+'.c')
    if not source.exists(): raise SystemExit('missing source for native archive member '+obj)
    name = str(source.relative_to(upstream))
    content = source.read_bytes()
    digest = hashlib.sha256(content).hexdigest()
    if digest != pins[name]: raise SystemExit('upstream source checksum mismatch: '+name)
    target = stage/source.name
    target.write_bytes(content)
    if hashlib.sha256(target.read_bytes()).hexdigest() != digest:
        raise SystemExit('staged source checksum mismatch: '+name)
    sources.append({'path': name, 'sha256': digest, 'staged_path': str(target.relative_to(p))})
manifest = {'kind': 'observed native archive extraction; unresolved managed host closure', 'sources': sources,
            'adapter_sha256': hashlib.sha256((p/'src/core-probe/probe.c').read_bytes()).hexdigest(),
            'native_config_sha256': hashlib.sha256((p/'build/native/source/config.h').read_bytes()).hexdigest(),
            'native_archive_sha256': hashlib.sha256((p/'build/native/source/o/blink/blink.a').read_bytes()).hexdigest(),
            'native_binary_sha256': hashlib.sha256((p/'build/core-native').read_bytes()).hexdigest(),
            'native_passed': True}
(out/'closure.json').write_text(json.dumps(manifest, indent=2)+'\n')
(out/'source-paths.txt').write_text(''.join(str(p/s['staged_path'])+'\n' for s in sources))
print('native core probe passed; extracted', len(sources), 'upstream translation units')
PY
if [[ ${1:-} == --native-only ]]; then exit 0; fi
# Match native core exclusions while keeping host capabilities explicit.
attempt=$(python3 - "$campaign" <<'PY'
import hashlib, json, pathlib, shutil, subprocess, sys, tempfile
p=pathlib.Path(sys.argv[1])
stage=pathlib.Path(tempfile.mkdtemp(prefix='attempt-',dir=p/'generated/core-profile'))
sys.path.insert(0,str(p/'scripts'))
from core_inputs import compiler_identity
shutil.copyfile(p/'scripts/core_inputs.py',stage/'compiler-identity.py')
shutil.copyfile(p/'artifacts/core/closure.json',stage/'closure.json')
shutil.copyfile(p/'config/core-config.h',stage/'config.h')
shutil.copyfile(p/'config/target-storage.h',stage/'target-storage.h')
shutil.copyfile(p/'config/core-overrides.json',stage/'overrides.json')
host=p/'config/managed-host'
if host.is_dir():
    # The core owns feature selection; the host profile supplies declarations.
    shutil.copytree(host,stage/'host',ignore=shutil.ignore_patterns('config.h'))
authored=stage/'authored'
authored.mkdir()
# The normal core-probe frontend belongs to derived test links only. Product
# orchestration is a separate C# consumer over translated upstream exports.
for original in [p/'src/Host/HostSignals.c', p/'src/Host/include/HostSignals.h',
                 p/'src/Host/HostMemory.c', p/'src/Host/include/HostMemory.h']:
    shutil.copyfile(original,authored/original.name)
additions_path=p/'config/core-managed-additions.json'
shutil.copyfile(additions_path,stage/'managed-additions.json')
additions=json.loads(additions_path.read_text())['sources']
pins={row['path']:row['sha256'] for row in json.loads((p/'config/source-inventory.json').read_text())['files']}
upstream=p/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
additional=stage/'additional'
additional.mkdir()
paths=[]
for row in additions:
    original=upstream/row['path']
    digest=hashlib.sha256(original.read_bytes()).hexdigest()
    if digest != row['sha256'] or digest != pins[row['path']]:
        raise SystemExit('managed-only upstream source checksum mismatch: '+row['path'])
    target=additional/original.name
    shutil.copyfile(original,target)
    if hashlib.sha256(target.read_bytes()).hexdigest() != digest:
        raise SystemExit('staged managed-only source checksum mismatch: '+row['path'])
    paths.append(str(target))
(stage/'managed-source-paths.txt').write_text(''.join(path+'\n' for path in paths))
config=(stage/'config.h').read_text()
if '#define NOLINEAR 1' not in config or '#define HAVE_MAP_ANONYMOUS 1' not in config or not (stage/'host/sys/mman.h').is_file():
    raise SystemExit('HostMemory requires NOLINEAR, HAVE_MAP_ANONYMOUS, and the campaign mman header together')
source_overrides={}
for basename in ('map', 'debug', 'cpuid'):
    tool=p/('src/Host/scripts/stage-'+basename+'.py')
    snapshot=stage/tool.name
    shutil.copyfile(tool,snapshot)
    adapted=stage/'upstream'/(basename+'.c')
    receipt=stage/(basename+'-boundary.json')
    subprocess.run([sys.executable,str(tool),'--output',str(adapted),
                    '--receipt',str(receipt)],check=True)
    if tool.read_bytes() != snapshot.read_bytes():
        raise SystemExit(basename+' staging tool changed during profile construction')
    boundary=json.loads(receipt.read_text())
    source_overrides['blink/'+basename+'.c']={'staged_path':str(adapted.relative_to(p)),
        'sha256':boundary['stagedSha256'],'original_sha256':boundary['originalSha256']}
# Apply the reviewed scalar correction before host binding preambles. Preserve
# every staging input in this new profile, including its review diff and pins.
# Existing profiles, cached objects and the immutable reference are untouched.
if not any(line.split()[:2] == ['#define', 'DISABLE_JIT'] for line in config.splitlines()):
    raise SystemExit('reviewed scalar FP correction requires DISABLE_JIT')
scalar=p/'src/UpstreamScalarFp'
scalar_snapshot=stage/'source-adaptations/UpstreamScalarFp'
scalar_inputs={f.name:hashlib.sha256(f.read_bytes()).hexdigest()
               for f in scalar.iterdir() if f.is_file()}
shutil.copytree(scalar,scalar_snapshot,ignore=shutil.ignore_patterns('__pycache__'))
for name,digest in scalar_inputs.items():
    if hashlib.sha256((scalar_snapshot/name).read_bytes()).hexdigest()!=digest:
        raise SystemExit('scalar FP staging snapshot changed: '+name)
scalar_output=stage/'upstream/scalar-fp'
scalar_receipt=stage/'scalar-fp-boundary.json'
subprocess.run([sys.executable,str(scalar/'stage.py'),'--output',str(scalar_output),
                '--receipt',str(scalar_receipt)],check=True)
if scalar_inputs!={f.name:hashlib.sha256(f.read_bytes()).hexdigest()
                   for f in scalar.iterdir() if f.is_file()}:
    raise SystemExit('scalar FP staging inputs changed during profile construction')
boundary=json.loads(scalar_receipt.read_text())
if boundary['patch_sha256']!=scalar_inputs['scalar-fp.patch']:
    raise SystemExit('scalar FP generated diff differs from the reviewed patch')
for filename,row in boundary['sources'].items():
    name='blink/'+filename
    adapted=scalar_output/filename
    if name in source_overrides or pins[name]!=row['source_sha256']:
        raise SystemExit('scalar FP source pin or adaptation collision: '+name)
    if hashlib.sha256(adapted.read_bytes()).hexdigest()!=row['staged_sha256']:
        raise SystemExit('scalar FP staged source changed: '+name)
    source_overrides[name]={'staged_path':str(adapted.relative_to(p)),
        'sha256':row['staged_sha256'],'original_sha256':row['source_sha256']}
# Integer corrections are independently pinned and reviewed against hardware.
# Keep their source derivation separate from scalar FP and host bindings.
integer=p/'src/UpstreamInteger'
integer_snapshot=stage/'source-adaptations/UpstreamInteger'
integer_inputs={f.name:hashlib.sha256(f.read_bytes()).hexdigest()
                for f in integer.iterdir() if f.is_file()}
shutil.copytree(integer,integer_snapshot,ignore=shutil.ignore_patterns('__pycache__'))
for name,digest in integer_inputs.items():
    if hashlib.sha256((integer_snapshot/name).read_bytes()).hexdigest()!=digest:
        raise SystemExit('integer staging snapshot changed: '+name)
integer_output=stage/'upstream/integer'
integer_receipt=stage/'integer-boundary.json'
subprocess.run([sys.executable,str(integer/'stage.py'),'--output',str(integer_output),
                '--receipt',str(integer_receipt)],check=True)
if integer_inputs!={f.name:hashlib.sha256(f.read_bytes()).hexdigest()
                    for f in integer.iterdir() if f.is_file()}:
    raise SystemExit('integer staging inputs changed during profile construction')
boundary=json.loads(integer_receipt.read_text())
if boundary['patch_sha256']!=integer_inputs['integer.patch']:
    raise SystemExit('integer generated diff differs from the reviewed patch')
for filename,row in boundary['sources'].items():
    name='blink/'+filename
    adapted=integer_output/filename
    if name in source_overrides or pins[name]!=row['source_sha256']:
        raise SystemExit('integer source pin or adaptation collision: '+name)
    if hashlib.sha256(adapted.read_bytes()).hexdigest()!=row['staged_sha256']:
        raise SystemExit('integer staged source changed: '+name)
    source_overrides[name]={'staged_path':str(adapted.relative_to(p)),
        'sha256':row['staged_sha256'],'original_sha256':row['source_sha256']}
# Cooperative owner cancellation uses two reviewed syscall safe points. Keep
# this private-host boundary separate from upstream CPU corrections.
stop=p/'src/UpstreamExecutionStop'
stop_snapshot=stage/'source-adaptations/UpstreamExecutionStop'
stop_inputs={f.name:hashlib.sha256(f.read_bytes()).hexdigest()
             for f in stop.iterdir() if f.is_file()}
shutil.copytree(stop,stop_snapshot,ignore=shutil.ignore_patterns('__pycache__'))
for name,digest in stop_inputs.items():
    if hashlib.sha256((stop_snapshot/name).read_bytes()).hexdigest()!=digest:
        raise SystemExit('execution-stop staging snapshot changed: '+name)
stop_output=stage/'upstream/execution-stop'
stop_receipt=stage/'execution-stop-boundary.json'
subprocess.run([sys.executable,str(stop/'stage.py'),'--output',str(stop_output),
                '--receipt',str(stop_receipt)],check=True)
if stop_inputs!={f.name:hashlib.sha256(f.read_bytes()).hexdigest()
                 for f in stop.iterdir() if f.is_file()}:
    raise SystemExit('execution-stop staging inputs changed during profile construction')
boundary=json.loads(stop_receipt.read_text())
if boundary['patch_sha256']!=stop_inputs['execution-stop.patch'] or boundary['stage_sha256']!=stop_inputs['stage.py']:
    raise SystemExit('execution-stop derivation differs from reviewed inputs')
for name,digest in boundary['required_headers'].items():
    if hashlib.sha256((p/name).read_bytes()).hexdigest()!=digest:
        raise SystemExit('execution-stop boundary header changed: '+name)
for filename,row in boundary['sources'].items():
    name='blink/'+filename
    adapted=stop_output/filename
    if name in source_overrides or pins[name]!=row['source_sha256']:
        raise SystemExit('execution-stop source pin or adaptation collision: '+name)
    if hashlib.sha256(adapted.read_bytes()).hexdigest()!=row['staged_sha256']:
        raise SystemExit('execution-stop staged source changed: '+name)
    source_overrides[name]={'staged_path':str(adapted.relative_to(p)),
        'sha256':row['staged_sha256'],'original_sha256':row['source_sha256']}
# NativeAOT guest runtime boundaries compose with the exact stop adaptation;
# retain both receipts and never replace the immutable upstream or old profiles.
runtime=p/'src/UpstreamGuestRuntime'
runtime_snapshot=stage/'source-adaptations/UpstreamGuestRuntime'
runtime_inputs={f.name:hashlib.sha256(f.read_bytes()).hexdigest()
                for f in runtime.iterdir() if f.is_file()}
shutil.copytree(runtime,runtime_snapshot,ignore=shutil.ignore_patterns('__pycache__'))
for name,digest in runtime_inputs.items():
    if hashlib.sha256((runtime_snapshot/name).read_bytes()).hexdigest()!=digest:
        raise SystemExit('guest-runtime staging snapshot changed: '+name)
runtime_output=stage/'upstream/guest-runtime'
runtime_receipt=stage/'guest-runtime-boundary.json'
subprocess.run([sys.executable,str(runtime/'stage.py'),
                '--predecessor',str(stop_output/'syscall.c'),
                '--predecessor-receipt',str(stop_receipt),
                '--output',str(runtime_output),'--receipt',str(runtime_receipt)],check=True)
if runtime_inputs!={f.name:hashlib.sha256(f.read_bytes()).hexdigest()
                    for f in runtime.iterdir() if f.is_file()}:
    raise SystemExit('guest-runtime inputs changed during profile construction')
boundary=json.loads(runtime_receipt.read_text())
if boundary['patch_sha256']!=runtime_inputs['guest-runtime.patch'] or boundary['stage_sha256']!=runtime_inputs['stage.py']:
    raise SystemExit('guest-runtime derivation differs from reviewed inputs')
for name,digest in boundary['required_headers'].items():
    if hashlib.sha256((p/name).read_bytes()).hexdigest()!=digest:
        raise SystemExit('guest-runtime header changed: '+name)
for filename,row in boundary['sources'].items():
    name='blink/'+filename
    previous=source_overrides.get(name)
    adapted=runtime_output/filename
    if previous is None or previous['sha256']!=row['predecessor_sha256'] or pins[name]!=row['source_sha256']:
        raise SystemExit('guest-runtime predecessor chain differs: '+name)
    if hashlib.sha256(adapted.read_bytes()).hexdigest()!=row['staged_sha256']:
        raise SystemExit('guest-runtime staged source changed: '+name)
    source_overrides[name]={'staged_path':str(adapted.relative_to(p)),
        'sha256':row['staged_sha256'],'original_sha256':row['source_sha256']}
binding_overrides=stage/'binding-overrides.json'
binding_overrides.write_text(json.dumps(source_overrides,indent=2)+'\n')
with (stage/'host-binding-stage.log').open('wb') as log:
    subprocess.run([sys.executable,str(p/'scripts/stage-host-bindings.py'),
        '--profile',str(stage),'--overrides',str(binding_overrides)],stdout=log,stderr=subprocess.STDOUT,check=True)
source_overrides=json.loads(binding_overrides.read_text())
bindings=json.loads((stage/'binding-sources.json').read_text())
(stage/'authored-source-paths.txt').write_text(''.join(str(stage/name)+'\n' for name in bindings['authored_c']))
selected_additions=[]
for row in additions:
    override=source_overrides.get(row['path'])
    selected_additions.append(str(p/override['staged_path']) if override else str(additional/pathlib.Path(row['path']).name))
(stage/'managed-source-paths.txt').write_text(''.join(path+'\n' for path in selected_additions))
native_paths=(p/'artifacts/core/source-paths.txt').read_text().splitlines()
selected=[]
for path in native_paths:
    override=source_overrides.get('blink/'+pathlib.Path(path).name)
    selected.append(str(p/override['staged_path']) if override else path)
(stage/'core-source-paths.txt').write_text(''.join(path+'\n' for path in selected))
files={str(f.relative_to(stage)):hashlib.sha256(f.read_bytes()).hexdigest() for f in stage.rglob('*') if f.is_file()}
compiler=p.parent/'DotCC/bin/Release/net10.0'
manifest={'staged_headers':files,'compiler':compiler_identity(compiler)}
manifest['source_overrides']=source_overrides
(stage/'inputs.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(stage)
PY
)
printf '%s\n' "$attempt" > "$out/latest-profile.txt"
if [[ ${1:-} == --stage-only ]]; then
  printf '%s\n' "$attempt"
  exit 0
fi
mapfile -t sources < "$attempt/core-source-paths.txt"
mapfile -t additions < "$attempt/managed-source-paths.txt"
mapfile -t authored_sources < "$attempt/authored-source-paths.txt"
sources+=("${additions[@]}" "${authored_sources[@]}")
# dotcc's current include overlay uses last-wins resolution. Keep authored host
# declarations last; unimplemented operations remain unresolved imports.
includes=(-I "$attempt" -I "$upstream" -I "$attempt/authored")
if [[ -d "$attempt/host" ]]; then
  includes+=(-I "$attempt/host")
fi
set +e
timeout "${CORE_TRANSLATION_TIMEOUT:-1800}" dotnet "$repo/DotCC/bin/Release/net10.0/dotcc.dll" -std=c17 -D_GNU_SOURCE -DNDEBUG -DNOLINEAR \
  "${includes[@]}" "${sources[@]}" \
  --overrides-file "$attempt/overrides.json" \
  --override-report "$attempt/override-report.jsonl" --runtime=c \
  --emit=managedlib --nest-types --class-name BlinkCore --namespace Managed.Emulation \
  -o "$campaign/generated/CoreProbe" > "$out/translate.log" 2>&1
status=$?
set -e
python3 - "$out" "$status" "$attempt" <<'PY'
import json, pathlib, shutil, sys
p=pathlib.Path(sys.argv[1]); status=int(sys.argv[2]); attempt=pathlib.Path(sys.argv[3])
receipt={'exit_code':status,'emitted':status==0,'managed_execution':'not run','log':'translate.log','staged_profile':str(attempt),'inputs':json.loads((attempt/'inputs.json').read_text())}
(p/'translation-status.json').write_text(json.dumps(receipt,indent=2)+'\n')
archive=p/'attempts'/attempt.name
archive.mkdir(parents=True,exist_ok=False)
for name in ('translate.log','translation-status.json','closure.json'):
    shutil.copyfile(p/name,archive/name)
report=attempt/'override-report.jsonl'
if report.exists():
    shutil.copyfile(report,archive/'override-report.jsonl')
    shutil.copyfile(report,p/'override-report.jsonl')
PY
if [[ $status != 0 ]]; then
  echo "Core translation stopped with status $status; see $out/translate.log" >&2
  exit "$status"
fi
echo 'Core emitted; managed compilation/execution still requires explicit qualification.'
