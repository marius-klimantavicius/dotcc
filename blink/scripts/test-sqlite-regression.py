#!/usr/bin/env python3
"""Regenerate SQLite's owning consumer with the campaign compiler and test all4.
Uses existing SQLite adapters/tests; no historical generated assembly is reused.
"""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import time
BLINK = Path(__file__).resolve().parents[1]
ROOT = BLINK.parent
SQLITE = ROOT / 'sqlite'
base = BLINK / 'artifacts/sqlite-regression'
base.mkdir(parents=True, exist_ok=True)
out = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
work = BLINK / 'generated/sqlite-regression' / out.name
work.mkdir(parents=True)
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
cli = ROOT / 'DotCC/bin/Release/net10.0/dotcc.dll'
post = ROOT / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
receipt = dict(kind='fresh-sqlite-managed-consumer-regression',passed=False,results={},
    compiler={p.name:sha(p) for p in cli.parent.glob('*.dll')},
    postprocessor={p.name:sha(p) for p in post.parent.glob('*.dll')},
    runner_sha256=sha(Path(__file__)))
environment = dict(os.environ, LC_ALL='C', TMPDIR=str(out / 'tmp'), SQLITE_SOURCE_SPLIT='none')
Path(environment['TMPDIR']).mkdir()
def save(): (out / 'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
def run(command,label,timeout=600):
    began=time.monotonic()
    with (out / (label+'.log')).open('wb') as log:
        result=subprocess.run(list(map(str,command)),stdout=log,stderr=subprocess.STDOUT,env=environment,timeout=timeout)
    receipt['results'][label]=dict(command=list(map(str,command)),exit_code=result.returncode,seconds=time.monotonic()-began)
    save()
    if result.returncode: raise RuntimeError(label+' failed: '+str(out))
    return (out / (label+'.log')).read_text()
try:
    receipt['inputs']={str(p.relative_to(ROOT)):sha(p)
        for directory in ['sqlite/src','sqlite/config','sqlite/samples/ManagedConsumer','sqlite/scripts']
        for p in (ROOT / directory).rglob('*') if p.is_file() and not any(part in ('bin','obj','__pycache__') for part in p.parts)}
    run(['bash',SQLITE / 'scripts/translate.sh','--tools','reuse'],'fresh-emission',1800)
    library=SQLITE / 'generated/TranslatedSqlite.Raw'
    receipt['raw_sources']={str(p.relative_to(library)):sha(p) for p in library.rglob('*') if p.is_file()}
    shutil.copytree(library,work / 'raw')
    project=SQLITE / 'samples/ManagedConsumer/ManagedConsumer.csproj'
    expected=None
    for mode in ['raw','optimized']:
        library=SQLITE / 'generated' / ('TranslatedSqlite.Raw' if mode=='raw' else 'TranslatedSqlite')
        property='-p:SqliteProject='+str(library / 'TranslatedSqlite.csproj')
        run(['dotnet','build',project,'-c','Release','-p:WarningsAsErrors=CS8500',property],mode+'-build')
        jit=project.parent / 'bin/Release/net10.0/ManagedConsumer.dll'
        jit_hashes={p.name:sha(p) for p in jit.parent.glob('*.dll')}
        output=run(['dotnet',jit],mode+'-jit',120)
        receipt['results'][mode+'-jit']['managed_binary_hashes']=jit_hashes
        if jit_hashes!={p.name:sha(p) for p in jit.parent.glob('*.dll')}:raise RuntimeError('JIT binary drift')
        if expected is None:expected=output
        if output!=expected or 'GC and cleanup passed' not in output:raise RuntimeError('Managed consumer transcript mismatch')
        publish=work / (mode+'-aot')
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:WarningsAsErrors=CS8500',property,'-o',publish],mode+'-aot-build',900)
        executable=publish / 'ManagedConsumer';digest=sha(executable)
        if run([executable],mode+'-aot',120)!=expected:raise RuntimeError('AOT transcript mismatch')
        if sha(executable)!=digest:raise RuntimeError('AOT binary drift')
        receipt['results'][mode+'-aot']['binary_sha256']=digest
    receipt['optimized_sources']={p.name:sha(p) for p in library.glob('*.cs')}
    if receipt['compiler']!={p.name:sha(p) for p in cli.parent.glob('*.dll')}:raise RuntimeError('Compiler drift')
    if receipt['postprocessor']!={p.name:sha(p) for p in post.parent.glob('*.dll')}:raise RuntimeError('Postprocessor drift')
    for name,digest in receipt['inputs'].items():
        if sha(ROOT / name)!=digest:raise RuntimeError('Authored input drift: '+name)
    receipt['passed']=True
    print(out / 'receipt.json')
finally:save()
