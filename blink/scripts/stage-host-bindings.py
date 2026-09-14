#!/usr/bin/env python3
"""Snapshot authored bindings and prefix verified upstream copies with their header.

Runs only while constructing a new profile, before inputs.json is finalized.
It never modifies the immutable upstream tree or rewrites generated C#.
"""
import argparse,hashlib,json,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--profile',required=True,type=Path)
p.add_argument('--overrides',required=True,type=Path,help='temporary original/staged source override map')
a=p.parse_args(); profile=a.profile.resolve()
sha=lambda path:hashlib.sha256(path.read_bytes()).hexdigest()
manifest_path=ROOT/'config/host-bindings.json'
manifest=json.loads(manifest_path.read_text())
if manifest['version']!=1:raise SystemExit('unknown host binding schema')
if (profile/'host-bindings.json').exists():raise SystemExit('host bindings already staged; use a fresh profile')
shutil.copyfile(manifest_path,profile/'host-bindings.json')
shutil.copyfile(Path(__file__),profile/'stage-host-bindings.py')
authored=profile/'authored';authored.mkdir(exist_ok=True)
managed=profile/'managed';managed.mkdir(exist_ok=True)
inputs={}
for category,target in [('headers',authored),('cSources',authored),('managedSources',managed)]:
    for relative in manifest[category]:
        original=ROOT/relative; destination=target/original.name
        if destination.exists() and destination.read_bytes()!=original.read_bytes():
            raise SystemExit('binding snapshot basename collision: '+str(destination))
        shutil.copyfile(original,destination); inputs[relative]=sha(original)
        if sha(destination)!=inputs[relative]:raise SystemExit('binding source changed during copy: '+relative)
host=ROOT/'src/Managed.Emulation.Host'
shutil.copytree(host,profile/'host-project',ignore=shutil.ignore_patterns('bin','obj'))
for original in host.glob('*.cs'):
    inputs[str(original.relative_to(ROOT))]=sha(original)
    if sha(profile/'host-project'/original.name)!=sha(original):raise SystemExit('Host source changed during copy')
for original in host.rglob('*'):
    if not original.is_file() or 'bin' in original.relative_to(host).parts or 'obj' in original.relative_to(host).parts:continue
    relative=original.relative_to(host)
    inputs[str(original.relative_to(ROOT))]=sha(original)
    if sha(profile/'host-project'/relative)!=inputs[str(original.relative_to(ROOT))]:
        raise SystemExit('Host project input changed during copy: '+str(relative))
lines=['#ifndef BLINK_FULL_HOST_BINDINGS_H','#define BLINK_FULL_HOST_BINDINGS_H',
       '#include "config.h"','#undef BLINK_MANAGED_HOST_DECLARATIONS_ONLY','#define BLINK_MANAGED_HOST_BINDINGS 1']
lines += ['#define '+name+' '+value for name,value in manifest['capabilities'].items()]
lines += ['#define '+name+' blink_host_'+name for name in manifest['isolate']]
lines += ['#include "'+name+'"' for name in manifest['includeHeaders']]
lines += ['#endif','']
for name in manifest['includeHeaders']:
    candidates=[profile/name,profile/'authored'/name,profile/'host'/name]
    found=[path for path in candidates if path.is_file()]
    if len(found)!=1:raise SystemExit('missing or ambiguous binding include: '+name)
(profile/'host-bindings.h').write_text('\n'.join(lines))
# All original upstream TUs receive this preamble, including utility files that
# never include builtin.h/config.h themselves. Authored C wrappers keep their
# own qualified headers and definitions, avoiding recursive open remapping.
closure=json.loads((profile/'closure.json').read_text())
overrides=json.loads(a.overrides.read_text())
rows=[]
entries=list(closure['sources'])
for entry in json.loads((profile/'managed-additions.json').read_text())['sources']:
    entries.append(dict(entry,staged_path=str(profile/'additional'/Path(entry['path']).name)))
for entry in entries:
    before=overrides.get(entry['path'])
    source=ROOT/(before['staged_path'] if before else entry['staged_path'])
    expected=before['sha256'] if before else entry['sha256']
    if sha(source)!=expected:raise SystemExit('pre-binding source changed: '+entry['path'])
    content=source.read_bytes(); target=profile/'bound'/Path(entry['path']).name
    target.parent.mkdir(exist_ok=True)
    target.write_bytes(b'#include "host-bindings.h"\n'+content)
    overrides[entry['path']]=dict(staged_path=str(target.relative_to(ROOT)),sha256=sha(target),original_sha256=entry['sha256'])
    rows.append(dict(source=entry['path'],original_sha256=entry['sha256'],pre_binding_sha256=expected,
                     previous_adaptation=before,staged_sha256=sha(target)))
a.overrides.write_text(json.dumps(overrides,indent=2)+'\n')
(profile/'binding-sources.json').write_text(json.dumps(dict(
    authored_c=['authored/managed-driver.c','authored/HostSignals.c','authored/HostMemory.c']
        + ['authored/'+Path(name).name for name in manifest['cSources']],
    authored_managed=['managed/'+Path(name).name for name in manifest['managedSources']],
    host_project='host-project/Managed.Emulation.Host.csproj'),indent=2)+'\n')
(profile/'host-binding-receipt.json').write_text(json.dumps(dict(
    scope=manifest['scope'],manifest_sha256=sha(manifest_path),header_sha256=sha(profile/'host-bindings.h'),
    source_inputs=inputs,upstream_preambles=rows),indent=2)+'\n')
print('staged explicit host bindings for '+str(len(rows))+' verified upstream translation units')
