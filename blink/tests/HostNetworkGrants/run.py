#!/usr/bin/env python3
"""Normal BCL network grant boundary checks; no translated execution claim."""
import hashlib,json,os,shutil,signal,subprocess,tempfile,time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
base=ROOT/'artifacts/host-network-grants';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base)); (a/'tmp').mkdir()
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def tree(p):return {str(f.relative_to(p)):sha(f) for f in sorted(p.rglob('*')) if f.is_file() and not {'bin','obj'}.intersection(f.relative_to(p).parts)}
r={'passed':False,'kind':'normal-host-network-grants','scope':'BCL grant enforcement and real loopback peers, JIT and NativeAOT; no guest or native-Linux policy equivalence claimed','commands':[]}
def save():
 p=a/'receipt.json.tmp';p.write_text(json.dumps(r,indent=2)+'\n');p.replace(a/'receipt.json')
def interrupted(sig,frame):raise InterruptedError('signal '+str(sig))
signal.signal(signal.SIGTERM,interrupted)
def run(command,label,execute=False,binaries=()):
 before={str(p):sha(p) for p in binaries}; started=time.monotonic()
 with (a/(label+'.stdout')).open('wb') as out,(a/(label+'.stderr')).open('wb') as err:
  p=subprocess.Popen(list(map(str,command)),cwd=a,stdout=out,stderr=err,start_new_session=True,env=dict(os.environ,TMPDIR=str(a/'tmp')))
  try:code=p.wait(timeout=240)
  except BaseException:
   try:os.killpg(p.pid,signal.SIGTERM)
   except ProcessLookupError:pass
   try:p.wait(timeout=10)
   except subprocess.TimeoutExpired:
    try:os.killpg(p.pid,signal.SIGKILL)
    except ProcessLookupError:pass
    p.wait()
   raise
 after={str(p):sha(p) for p in binaries}
 r['commands'].append({'command':list(map(str,command)),'label':label,'exit_code':code,'seconds':time.monotonic()-started,'stdout_sha256':sha(a/(label+'.stdout')),'stderr_sha256':sha(a/(label+'.stderr')),'before':before,'after':after});save()
 if code or before!=after:raise RuntimeError(label+' failed')
 if execute and ((a/(label+'.stderr')).stat().st_size or (a/(label+'.stdout')).read_text()!='network-grants isolated publication outbound bytes cleanup PASS\n'):raise RuntimeError(label+' transcript')
try:
 host=ROOT/'src/Managed.Emulation.Host';source=Path(__file__).resolve().parent
 r['host']=tree(host);r['fixture']=tree(source);r['dotnet']=sha(Path(shutil.which('dotnet')).resolve())
 shutil.copytree(host,a/'Host',ignore=shutil.ignore_patterns('bin','obj'));shutil.copyfile(source/'Program.cs',a/'Program.cs')
 (a/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><EnableDefaultCompileItems>false</EnableDefaultCompileItems><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><Compile Include="Program.cs"/><ProjectReference Include="Host/Managed.Emulation.Host.csproj"/></ItemGroup></Project>')
 run(['dotnet','build','Probe.csproj','-c','Release'],'build')
 dll=a/'bin/Release/net10.0/Probe.dll';run(['dotnet',dll],'jit',True,[dll,a/'bin/Release/net10.0/Managed.Emulation.Host.dll'])
 run(['dotnet','publish','Probe.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-o',a/'aot'],'publish')
 exe=a/'aot/Probe';run([exe],'aot',True,[exe])
 if tree(host)!=r['host'] or tree(source)!=r['fixture'] or sha(Path(shutil.which('dotnet')).resolve())!=r['dotnet']:raise RuntimeError('input identity changed')
 r['passed']=True
except BaseException as error:r['failure']={'type':type(error).__name__,'message':str(error)};raise
finally:save();print(a/'receipt.json',flush=True)
