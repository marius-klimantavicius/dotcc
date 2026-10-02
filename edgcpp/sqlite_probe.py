#!/usr/bin/env python3
"""Run the pinned SQLite amalgamation through original EDG C and dot_gen_be.

Uses the repository's deterministic SQLite definitions and explicit local GCC
header directories as a diagnostic profile, not the final portable header ABI.
Keeps diagnostics, timing, hashes and the original C output for inspection.
"""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys
import time

ROOT=Path(__file__).resolve().parent
REPO=ROOT.parent
OUT=ROOT/'artifacts/sqlite-edg'


def main():
    OUT.mkdir(parents=True,exist_ok=True)
    config=json.loads((REPO/'sqlite/config/sources.json').read_text())['product']
    source=REPO/'sqlite/ref'/config['directory']/'sqlite3.c'
    if not source.is_file():raise SystemExit(f'Missing pinned source: {source}')
    archive=source.parent.with_suffix('.zip')
    if archive.is_file() and hashlib.sha256(archive.read_bytes()).hexdigest()!=config['sha256']:
        raise SystemExit('SQLite archive hash does not match repository manifest')
    definitions=[s.strip() for s in (REPO/'sqlite/config/defines.txt').read_text().splitlines() if s.strip() and not s.startswith('#')]
    includes=subprocess.run(['gcc','-E','-x','c','-','-v'],input='',text=True,capture_output=True,check=True).stderr
    include_dirs=[line.strip() for line in includes.split('#include <...> search starts here:')[1].split('End of search list.')[0].splitlines() if line.strip()]
    env=dict(os.environ,EDG_BASE=str(ROOT/'upstream/bases/docker/dev-env/gcc'))
    report={'source':str(source),'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),
            'archiveSha256':config['sha256'],'defines':definitions,'includeDirectories':include_dirs,
            'headerProfile':'diagnostic host GCC LP64; not a portable target header contract','runs':{}}
    def execute(label,command):
        start=time.monotonic()
        result=subprocess.run(list(map(str,command)),env=env,text=True,capture_output=True,timeout=300)
        (OUT/f'{label}.stdout.log').write_text(result.stdout)
        (OUT/f'{label}.stderr.log').write_text(result.stderr)
        report['runs'][label]={'command':list(map(str,command)),'exitCode':result.returncode,'seconds':round(time.monotonic()-start,3)}
        return result
    options=[arg for directory in include_dirs for arg in ('-I',directory)]+['-D'+d for d in definitions]
    c_file=OUT/'sqlite3.edg.c'
    result=execute('original-c',[ROOT/'build/bin/cpfe','--c11','--gnu_version','130300',*options,
            '--gen_c_file_name',c_file,'-o',OUT/'sqlite3.il',source])
    if result.returncode==0:
        report['generatedC']={'path':str(c_file),'bytes':c_file.stat().st_size,'sha256':hashlib.sha256(c_file.read_bytes()).hexdigest()}
        execute('original-c-syntax',['gcc','-x','c','-std=gnu11','-w','-fsyntax-only',c_file])
        obj=OUT/'sqlite3.edg.o'
        compiled=execute('original-c-object',['gcc','-x','c','-std=gnu11','-w','-O0','-fPIC','-c',c_file,'-o',obj])
        if compiled.returncode==0:
            symbols=execute('native-imports',['nm','-u',obj])
            report['nativeImports']=[line.split()[-1] for line in symbols.stdout.splitlines() if line.split()]
            (OUT/'native-imports.json').write_text(json.dumps(report['nativeImports'],indent=2)+'\n')
    managed=OUT/'managed'
    result=execute('csharp',[sys.executable,ROOT/'dotnet.py',source,'--language','c',*options,
        '--emit','managedlib','--namespace','Edg.SQLite','--class-name','SqliteCore',
        '--instance-methods','--split','size','--output-dir',managed])
    if result.returncode==0:
        built=execute('csharp-build',['dotnet','build',managed/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        report['csharpBuildSucceeded']=built.returncode==0
    else:
        report['csharpBuildSucceeded']=False
        report['firstBlocker']=result.stderr.strip().splitlines()[-1] if result.stderr.strip() else 'See csharp.stderr.log'
    (OUT/'results.json').write_text(json.dumps(report,indent=2)+'\n')
    print(json.dumps(report,indent=2))
    return 0


if __name__=='__main__':raise SystemExit(main())
