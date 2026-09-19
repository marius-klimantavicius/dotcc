#!/usr/bin/env python3
"""Native common permission invariants and private owner four-mode qualification."""
import hashlib,json,os,shutil,subprocess,tempfile
from pathlib import Path
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2];REPO=ROOT.parent
base=ROOT/'generated/host-permissions';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base));out=ROOT/'artifacts/host-permissions'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'passed':False,'scope':'private modes, immutable ownership and creation mask; no host metadata mutation by managed product','results':{}}
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,label,cwd=a,timeout=180):
 with(out/(label+'.log')).open('wb')as log:code=subprocess.run(list(map(str,cmd)),cwd=cwd,env=dict(os.environ,LC_ALL='C'),stdout=log,stderr=subprocess.STDOUT,timeout=timeout).returncode
 r['results'][label]={'command':list(map(str,cmd)),'exit':code};save()
 if code:raise RuntimeError(label+' failed: '+str(out/(label+'.log')))
 return(out/(label+'.log')).read_bytes()
try:
 for name in ['probe.c','Program.cs']:shutil.copyfile(ROOT/'tests/HostPermissions'/name,a/name)
 shutil.copytree(ROOT/'config/managed-host',a/'profile')
 shutil.copytree(ROOT/'src/Managed.Emulation.Host',a/'host',ignore=shutil.ignore_patterns('bin','obj'))
 bridges=['HostIo','HostFileMetadata','HostIdentity','HostAccess','HostPaths','HostNamespace','HostPermissions']
 for name in bridges:
  shutil.copyfile(ROOT/'src'/name/(name+'Bridge.cs'),a/(name+'Bridge.cs'))
 for module,name in [('HostIo','host-io.h'),('HostIdentity','host-identity.h'),('HostAccess','host-access.h'),('HostPaths','host-paths.h'),('HostNamespace','host-namespace.h'),('HostPermissions','host-permissions.h')]:shutil.copyfile(ROOT/'src'/module/name,a/'profile'/name)
 shutil.copyfile(ROOT/'src/Host/HostFileControl.c',a/'HostFileControl.c')
 r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
 identity=lambda:{str(p.relative_to(REPO)):sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]}
 r['compiler']=identity();r['head']=subprocess.check_output(['git','-C',str(REPO),'rev-parse','HEAD'],text=True).strip();save()
 run(['cc','-std=c17','-Wall','-Wextra','-Werror',a/'probe.c','-o',a/'native'],'native-build')
 (a/'native-root').mkdir();expected=run([a/'native'],'native',cwd=a/'native-root',timeout=30)
 raw=a/'raw';optimized=a/'optimized'
 run(['dotnet',cli,'-std=c17','-DBLINK_MANAGED_PERMISSIONS','-I',a/'profile',a/'probe.c',a/'HostFileControl.c','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
 if identity()!=r['compiler']:raise RuntimeError('compiler changed during emission')
 r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,optimized)
 for label,generated in [('raw',raw),('optimized',optimized)]:
  consumer=a/(label+'-consumer');consumer.mkdir()
  for name in ['Program.cs']+[n+'Bridge.cs'for n in bridges]:shutil.copyfile(a/name,consumer/name)
  xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
  for key,val in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','HostPermissionsProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=val
  items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'));ET.SubElement(items,'ProjectReference',Include=str(a/'host/Managed.Emulation.Host.csproj'));ET.SubElement(items,'TrimmerRootAssembly',Include='HostPermissionsProbe')
  project=consumer/'HostPermissionsProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
  if label=='optimized':run(['dotnet','restore',project],'optimized-restore');run(['dotnet',post,project,'--in-place'],'postprocess')
  run(['dotnet','build',project,'-c','Release'],label+'-build')
  if run(['dotnet',consumer/'bin/Release/net10.0/HostPermissionsProbe.dll'],label+'-jit',timeout=30)!=expected:raise RuntimeError(label+' JIT mismatch')
  publish=a/(label+'-aot');run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',timeout=300)
  if run([publish/'HostPermissionsProbe'],label+'-aot',timeout=30)!=expected:raise RuntimeError(label+' AOT mismatch')
 for name in ['HostFiles','InstanceIo']:
  model=a/('regression-'+name);model.mkdir();shutil.copyfile(ROOT/'tests'/name/'Program.cs',model/'Program.cs')
  project=model/'Regression.csproj';project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../host/Managed.Emulation.Host.csproj" /></ItemGroup></Project>')
  r['inputs']['regression-'+name+'/Program.cs']=sha(model/'Program.cs');run(['dotnet','run','--project',project,'-c','Release'],'regression-'+name,timeout=120)
 if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw sources changed')
 r['optimizedGenerated']={p.name:sha(p)for p in optimized.glob('*.cs')};r['passed']=True;save();print('native plus four-mode private permissions and HostFiles/InstanceIo regressions PASS: '+str(out/'receipt.json'))
finally:save()
