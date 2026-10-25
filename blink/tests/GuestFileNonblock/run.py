#!/usr/bin/env python3
"""Isolated ordinary native/guest-VFS nonblock semantics; no product build."""
import hashlib, json, os
from pathlib import Path
import shutil, subprocess, tempfile
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()

def main():
    base = ROOT / 'artifacts/coreclr-coordination'; base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='file-nonblock-', dir=base))
    print(attempt, flush=True)
    receipt = dict(passed=False, scope='native Linux and isolated authored HostIoBridge/InstanceIo; no full translated guest', inputs={}, commands=[])
    def save(): (attempt/'receipt.json').write_text(json.dumps(receipt, indent=2)+'\n')
    def pin(p):
        p=Path(p).resolve(); receipt['inputs'][str(p)] = sha(p); return p
    def run(name, args):
        with (attempt/(name+'.log')).open('wb') as log:
            r=subprocess.run(list(map(str,args)), cwd=attempt, stdout=log, stderr=subprocess.STDOUT,
                timeout=120, env=dict(os.environ, MSBUILDDISABLENODEREUSE='1'))
        receipt['commands'].append(dict(name=name,command=list(map(str,args)),exit=r.returncode)); save()
        if r.returncode: raise RuntimeError(name+': '+str(attempt/(name+'.log')))
        print(name+': passed', flush=True)
        return (attempt/(name+'.log')).read_bytes()
    try:
        pin(__file__)
        for name in ('Program.cs','native.c'): shutil.copyfile(pin(HERE/name),attempt/name)
        shutil.copyfile(pin(ROOT/'src/Host/HostIoBridge.cs'),attempt/'HostIoBridge.cs')
        host=attempt/'host';shutil.copytree(ROOT/'src/Managed.Emulation.Host',host,ignore=shutil.ignore_patterns('bin','obj'))
        for p in host.rglob('*'):
            if p.is_file():pin(p)
        (attempt/'probe.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><EnableDefaultCompileItems>false</EnableDefaultCompileItems>
</PropertyGroup><ItemGroup><Compile Include="Program.cs"/><Compile Include="HostIoBridge.cs"/>
<ProjectReference Include="host/Managed.Emulation.Host.csproj"/></ItemGroup></Project>''')
        run('native-build',['cc','-std=c17','-Wall','-Wextra','-Werror',attempt/'native.c','-o',attempt/'native'])
        (attempt/'seed').write_bytes(b'abc')
        expected=run('native',[attempt/'native'])
        run('managed-build',['dotnet','build',attempt/'probe.csproj','-c','Release','--disable-build-servers','-p:UseSharedCompilation=false'])
        actual=run('managed-host',['dotnet',attempt/'bin/Release/net10.0/probe.dll'])
        if actual!=expected: raise RuntimeError('Native/managed output differs')
        if any(sha(p)!=d for p,d in receipt['inputs'].items()): raise RuntimeError('Input changed')
        receipt['passed']=True
    except BaseException as e: receipt['error']=f'{type(e).__name__}: {e}'
    finally: receipt['completed']=True;save()
    print('receipt='+str(attempt/'receipt.json'),flush=True)
    return 0 if receipt['passed'] else 1
if __name__=='__main__':raise SystemExit(main())
