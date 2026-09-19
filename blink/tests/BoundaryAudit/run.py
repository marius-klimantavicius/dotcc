#!/usr/bin/env python3
"""Qualify boundary inventory without executing target methods/initializers."""
import hashlib,json,shutil,subprocess,tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
base=ROOT/'artifacts/boundary-audit';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='self-check-',dir=base)).resolve()
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def run(cmd,name,ok=0):
    with(a/(name+'.log')).open('wb') as log:
        result=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,timeout=60)
    if ok is None:assert result.returncode!=0,name
    else:assert result.returncode==ok,name
for directory in ['host','main','decoder','missing']: (a/directory).mkdir()
shutil.copyfile(ROOT/'tests/BoundaryAudit/Fixture.cs',a/'main/Fixture.cs')
shutil.copyfile(ROOT/'tests/BoundaryAudit/HostFixture.cs',a/'host/HostFixture.cs')
shutil.copyfile(ROOT/'tests/BoundaryAudit/DecoderTests.cs',a/'decoder/DecoderTests.cs')
shutil.copyfile(ROOT/'tools/BoundaryAudit/IlDecoder.cs',a/'decoder/IlDecoder.cs')
(a/'host/Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AssemblyName>Managed.Emulation.Host</AssemblyName></PropertyGroup></Project>')
(a/'main/Fixture.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks></PropertyGroup><ItemGroup><ProjectReference Include="../host/Host.csproj"/></ItemGroup></Project>')
(a/'decoder/Decoder.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings></PropertyGroup></Project>')
audit=ROOT/'tools/BoundaryAudit/bin/Release/net10.0/BoundaryAudit.dll'
run(['dotnet','build',a/'main/Fixture.csproj','-c','Release'],'build')
run(['dotnet','run','--project',a/'decoder/Decoder.csproj','-c','Release'],'decoder')
binary=a/'main/bin/Release/net10.0/Fixture.dll';host=binary.with_name('Managed.Emulation.Host.dll')
run(['dotnet',audit,binary,a/'report.json'],'audit')
r=json.loads((a/'report.json').read_text())
expected=['audit-must-never-load!forbidden','root-import-must-not-run!RootImport','module-initializer-must-not-run!Native',
          'host-initializer-must-not-run!Native','host-module-must-not-run!Native','async-body-must-not-run!Native','iterator-body-must-not-run!Native']
assert r['complete'] and not r['unresolved'] and not r['errors']
assert len(r['allNativeDeclarations'])==len(expected)==len(r['traversedNativeImports'])
for name in expected:assert any(name in row for row in r['traversedNativeImports']),name
assert len(r['directRuntimeEntries'])==1 and len(r['indirectCallSites'])==1
assert len(r['stateMachineExpansions'])==2 and r['virtualCallSites']
assert len(r['scopedAssemblies'])==2
host_identity=next(x for x in r['scopedAssemblies']if x['identity'].startswith('Managed.Emulation.Host,'))
assert Path(host_identity['path'])==host and host_identity['sha256']==sha(host)
assert len([x for x in r['roots']if '<global>::Void .cctor()' in x])==2
run(['dotnet',audit,binary,a/'budget.json','--max-methods','1'],'budget',None)
budget=json.loads((a/'budget.json').read_text());assert not budget['complete'] and any('budget' in x['error']for x in budget['errors'])
shutil.copyfile(binary,a/'missing/Fixture.dll');(a/'missing/report.json').write_text('stale success')
run(['dotnet',audit,a/'missing/Fixture.dll',a/'missing/report.json'],'missing-host',None)
assert not(a/'missing/report.json').exists()
(a/'receipt.json').write_text(json.dumps({'passed':True,'auditSha256':sha(audit),'sources':{str(p.relative_to(ROOT)):sha(p)for folder in ['tools/BoundaryAudit','tests/BoundaryAudit']for p in (ROOT/folder).glob('*')if p.is_file()},'report':str(a/'report.json'),'scope':'metadata-only throwing initializer, root/module/Host imports, async/iterator expansion, indirect call reporting, decoder bounds, method budget and missing dependency fail-closed'},indent=2)+'\n')
print(a/'receipt.json')
