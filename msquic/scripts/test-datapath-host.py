#!/usr/bin/env python3
"""Qualify the typed BCL UDP host against raw/optimized frozen MsQuic workers."""
import argparse,hashlib,json,resource,subprocess,sys
from pathlib import Path
from xml.sax.saxutils import escape
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'tests/PlatformHost'))
from bootstrap import write_test_host
parser=argparse.ArgumentParser();parser.add_argument('--variants',nargs='+',choices=['raw','optimized'],default=['raw','optimized']);parser.add_argument('--jit-only',action='store_true');args=parser.parse_args()
BUILD=ROOT/'build/datapath-host';LOGS=ROOT/'artifacts/datapath-host';BUILD.mkdir(parents=True,exist_ok=True);LOGS.mkdir(parents=True,exist_ok=True)
resource.setrlimit(resource.RLIMIT_CORE,(0,0))
receipt=dict(passed=False,commands=[],variants=[],deferred_send_method='Real Socket.SendToAsync submitted; test-only partial hook delays its completion delivery to prove ownership, GC and close drain. Production hook is omitted by the compiler.')
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def run(command,name,negative=False):
 command=[str(x) for x in command];receipt['commands'].append(dict(name=name,arguments=command))
 result=subprocess.run(command,text=True,capture_output=True,timeout=600)
 (LOGS/(name+'.log')).write_text(result.stdout+result.stderr)
 if negative:
  if result.returncode==0 or 'Socket deletion must be deferred outside its callback.' not in result.stdout+result.stderr:raise RuntimeError(name+' did not reject callback-local socket deletion')
 elif result.returncode:raise RuntimeError(name+' failed; see '+str(LOGS/(name+'.log')))
 return result.stdout
try:
 receipt['closure_sha256']=sha(ROOT/'config/product-closure.json')
 receipt['test_driver_sha256']=sha(Path(__file__))
 receipt['bootstrap_sha256']=sha(ROOT/'tests/PlatformHost/bootstrap.py')
 sourcefiles=[ROOT/'src/BclHost'/name for name in ['MsQuicHost.Resources.cs','MsQuicHost.Platform.cs','MsQuicHost.Queue.cs','Status.cs']]+sorted((ROOT/'src/BclHost').glob('MsQuicHost.Datapath*.cs'))+sorted((ROOT/'tests/DatapathHost').glob('*.cs'))
 receipt['source_hashes']={str(p.relative_to(ROOT)):sha(p) for p in sourcefiles}
 for variant in args.variants:
  project=BUILD/variant;project.mkdir(exist_ok=True)
  generated=ROOT/'generated'/('raw/TranslatedMsQuic' if variant == 'raw' else 'TranslatedMsQuic');library=generated/'TranslatedMsQuic.csproj'
  generated_hashes={p.name:sha(p) for p in generated.glob('*.cs')}
  write_test_host(generated,project,register_calls='RegisterPlatform(ref table);RegisterDatapath(ref table);',method_name='CreateDatapathTable')
  (project/'DatapathHost.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible><NoWarn>CS0162</NoWarn></PropertyGroup><ItemGroup><ProjectReference Include="'+escape(str(library))+'"/>'+''.join('<Compile Include="'+escape(str(p))+'"/>' for p in sourcefiles)+'<TrimmerRootAssembly Include="TranslatedMsQuic"/></ItemGroup></Project>\n')
  run(['dotnet','build',project/'DatapathHost.csproj','-c','Release','--nologo'],variant+'-build')
  commands=[('jit',['dotnet',project/'bin/Release/net10.0/DatapathHost.dll'])]
  if not args.jit_only:
   run(['dotnet','publish',project/'DatapathHost.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-o',project/'aot','--nologo'],variant+'-aot-build')
   commands.append(('aot',[project/'aot/DatapathHost']))
  for runtime,command in commands:
   output=run(command,variant+'-'+runtime)
   expected='datapath typed buffers, dual-stack source selection, scopes, truncation, unreachable and drain passed; AOT='+str(runtime=='aot')
   if output.strip()!=expected:raise RuntimeError('Incorrect datapath receipt')
   run([*command,'callback-delete'],variant+'-'+runtime+'-callback-delete',negative=True)
  if generated_hashes!={p.name:sha(p) for p in generated.glob('*.cs')}:raise RuntimeError('Generated library changed during validation')
  receipt['variants'].append(dict(name=variant,passed=True,runtimes=[name for name,_ in commands],generated_hashes=generated_hashes))
  print(variant+': typed buffers, worker completions, UDP addresses/lifetimes PASS',flush=True)
 if receipt['source_hashes']!={str(p.relative_to(ROOT)):sha(p) for p in sourcefiles}:raise RuntimeError('Host sources changed during validation')
 receipt['passed']=True
finally:(LOGS/'results.json').write_text(json.dumps(receipt,indent=2)+'\n')
