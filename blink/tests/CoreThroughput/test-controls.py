#!/usr/bin/env python3
"""Reject incomplete/drifted preparation receipts without executing benchmark code."""
import argparse, copy, hashlib, json, subprocess, sys, tempfile
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--prepared', type=Path, required=True)
args = p.parse_args()
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
original = args.prepared.resolve(); original_hash = sha(original)
r = json.loads(original.read_text()); build = Path(r['build'])
if not r.get('ready_for_measurement') or r.get('passed'): raise SystemExit('Requires fresh prepared receipt')
base = ROOT/'artifacts/core-throughput-controls'; base.mkdir(parents=True,exist_ok=True)
out = Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
script = ROOT/'tests/CoreThroughput/run.py'
result = {'passed':False,'prepared':str(original),'prepared_sha256':original_hash,
          'runner_sha256':sha(script),'test_sha256':sha(Path(__file__)),'cases':[]}
try:
    for case in ['missing-mode','missing-preflight','core-identity','assembly-identity','profile-identity',
                 'runner-identity','executable-path','artifact-set','native-only-preparation']:
        value = copy.deepcopy(r)
        scratch = out/(out.name+'-'+case+'-build'); scratch.mkdir()
        value['build'] = str(scratch)
        value['executables'] = {'native':[str(scratch/'native')]}
        for variant in ['raw','optimized']:
            value['executables'][variant+'-jit']=['dotnet',str(scratch/variant/'consumer/bin/Release/net10.0/CoreThroughput.dll')]
            value['executables'][variant+'-aot']=[str(scratch/variant/'publish/CoreThroughput')]
        expected = ''
        if case=='missing-mode': value['executables'].pop('optimized-aot'); expected='exactly all five'
        if case=='missing-preflight': value['semantic_preflight'].pop('raw-aot'); expected='exactly all five'
        if case=='core-identity': value['core_sha256']='0'*64; expected='provenance chain changed: core_sha256'
        if case=='assembly-identity': value['assembly_sha256']='0'*64; expected='provenance chain changed: assembly_sha256'
        if case=='profile-identity': value['profile_inputs_sha256']='0'*64; expected='provenance chain changed: profile_inputs_sha256'
        if case=='runner-identity': value['runner_sha256']='0'*64; expected='tool/runner identity changed'
        if case=='executable-path': value['executables']['native']=['/must/not/execute']; expected='executable paths differ'
        if case=='artifact-set': expected='Prepared artifact set changed'
        receipt=out/(case+'.json');receipt.write_text(json.dumps(value,indent=2)+'\n')
        command=[sys.executable,str(script),'--measure-existing',str(receipt)]
        if case=='native-only-preparation':
            command=[sys.executable,str(script),'--core-receipt',r['core_receipt'],'--native-only','--prepare-only']
            expected='Native-only preparation cannot qualify'
        process=subprocess.run(command,text=True,capture_output=True,timeout=30)
        (out/(case+'.stdout')).write_text(process.stdout)
        (out/(case+'.stderr')).write_text(process.stderr)
        if process.returncode==0 or expected not in process.stderr: raise RuntimeError(case+' did not reject at intended gate')
        result['cases'].append({'name':case,'exit':process.returncode,'expected_error':expected})
    if sha(original)!=original_hash: raise RuntimeError('Original preparation receipt changed')
    for name,digest in r['frozen_artifacts'].items():
        if sha(build/name)!=digest: raise RuntimeError('Original prepared artifact changed: '+name)
    result['passed']=True
finally:
    (out/'receipt.json').write_text(json.dumps(result,indent=2)+'\n')
print(out/'receipt.json')
