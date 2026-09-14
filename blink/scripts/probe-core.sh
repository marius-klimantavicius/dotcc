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
import hashlib, json, pathlib, shutil, sys, tempfile
p=pathlib.Path(sys.argv[1])
stage=pathlib.Path(tempfile.mkdtemp(prefix='attempt-',dir=p/'generated/core-profile'))
shutil.copyfile(p/'config/core-config.h',stage/'config.h')
shutil.copyfile(p/'config/core-overrides.json',stage/'overrides.json')
host=p/'config/managed-host'
if host.is_dir():
    # The core owns feature selection; the host profile supplies declarations.
    shutil.copytree(host,stage/'host',ignore=shutil.ignore_patterns('config.h'))
authored=stage/'authored'
authored.mkdir()
for original in [p/'src/core-probe/probe.c', p/'src/HostSignals/HostSignals.c', p/'src/HostSignals/HostSignals.h']:
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
files={str(f.relative_to(stage)):hashlib.sha256(f.read_bytes()).hexdigest() for f in stage.rglob('*') if f.is_file()}
compiler=p.parent/'DotCC/bin/Release/net10.0'
manifest={'staged_headers':files,'compiler':{f.name:hashlib.sha256(f.read_bytes()).hexdigest() for f in compiler.glob('DotCC*.dll')}}
manifest['compiler']['dotcc.dll']=hashlib.sha256((compiler/'dotcc.dll').read_bytes()).hexdigest()
(stage/'inputs.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(stage)
PY
)
printf '%s\n' "$attempt" > "$out/latest-profile.txt"
if [[ ${1:-} == --stage-only ]]; then
  printf '%s\n' "$attempt"
  exit 0
fi
mapfile -t sources < "$out/source-paths.txt"
mapfile -t additions < "$attempt/managed-source-paths.txt"
sources+=("${additions[@]}")
# dotcc's current include overlay uses last-wins resolution. Keep authored host
# declarations last; unimplemented operations remain unresolved imports.
includes=(-I "$attempt" -I "$upstream")
if [[ -d "$attempt/host" ]]; then
  includes+=(-I "$attempt/host")
fi
set +e
timeout "${CORE_TRANSLATION_TIMEOUT:-1800}" dotnet "$repo/DotCC/bin/Release/net10.0/dotcc.dll" -std=c17 -D_GNU_SOURCE -DNDEBUG -DNOLINEAR \
  "${includes[@]}" "${sources[@]}" \
  "$attempt/authored/probe.c" "$attempt/authored/HostSignals.c" --overrides-file "$attempt/overrides.json" \
  --override-report "$attempt/override-report.jsonl" --runtime=c \
  --emit=managedlib --nest-types --class-name Blink --namespace Managed.Emulation \
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
