#!/usr/bin/env python3
"""BCL platform callbacks + unchanged generated worker, against the frozen closure."""
import argparse,hashlib,json,re,resource,subprocess,sys
from pathlib import Path
from xml.sax.saxutils import escape
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'tests/PlatformHost'))
from bootstrap import write_test_host
parser=argparse.ArgumentParser();parser.add_argument('--variants',nargs='+',choices=['raw','optimized'],default=['raw','optimized']);parser.add_argument('--jit-only',action='store_true');args=parser.parse_args()
BUILD=ROOT/'build/platform-host';LOGS=ROOT/'artifacts/platform-host';BUILD.mkdir(parents=True,exist_ok=True);LOGS.mkdir(parents=True,exist_ok=True)
receipt=dict(passed=False,commands=[],variants=[])
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def run(command,name,negative=False):
 command=[str(x) for x in command];receipt['commands'].append(dict(name=name,arguments=command))
 result=subprocess.run(command,text=True,capture_output=True,timeout=600)
 (LOGS/(name+'.log')).write_text(result.stdout+result.stderr)
 if negative:
  if result.returncode==0 or 'MsQuic managed host invariant:' not in result.stdout+result.stderr:raise RuntimeError(name+' did not fail at the host invariant boundary')
 elif result.returncode:raise RuntimeError(name+' failed; see '+str(LOGS/(name+'.log')))
 return result.stdout
resource.setrlimit(resource.RLIMIT_CORE,(0,0))
status=(ROOT/'src/BclHost/Status.cs').read_text()
status_names=[]
for declaration in re.findall(r'internal const uint ([^;]+);',status):status_names+=re.findall(r'\b(\w+)\s*=',declaration)
overrides={'VersionNegotiationError':'QUIC_STATUS_VER_NEG_ERROR','AlpnNegotiationFailure':'QUIC_STATUS_ALPN_NEG_FAILURE','CertificateExpired':'QUIC_STATUS_CERT_EXPIRED','CertificateUntrustedRoot':'QUIC_STATUS_CERT_UNTRUSTED_ROOT','CertificateMissing':'QUIC_STATUS_CERT_NO_CERT','TlsErrorBase':'TLS_ERROR_BASE','CertificateErrorBase':'CERT_ERROR_BASE'}
def macro(name):return overrides.get(name,'QUIC_STATUS_'+re.sub(r'(?<!^)([A-Z])',r'_\1',name).upper())

try:
 closure=ROOT/'config/product-closure.json';receipt['closure_sha256']=sha(closure)
 closure_data=json.loads(closure.read_text())
 sourcefiles=[ROOT/'src/BclHost'/name for name in ['MsQuicHost.Resources.cs','MsQuicHost.Platform.cs','MsQuicHost.Queue.cs','Status.cs']]+[ROOT/'tests/PlatformHost/Program.cs']
 receipt['source_hashes']={str(p.relative_to(ROOT)):sha(p) for p in sourcefiles}
 stage=ROOT/'build/product-source';manifest=json.loads((stage/'manifest.json').read_text())
 native=BUILD/'status.c'
 native.write_text('#include "msquic.h"\n#include <stdio.h>\nint main(void){\n'+''.join(f'printf("{n}=%u\\n", (unsigned)({macro(n)}));\n' for n in status_names)+'printf("TlsAlert42=%u\\n",(unsigned)QUIC_STATUS_TLS_ALERT(42));\nprintf("FailedPending=%u\\n",(unsigned)QUIC_FAILED(QUIC_STATUS_PENDING));\nprintf("FailedInvalid=%u\\n",(unsigned)QUIC_FAILED(QUIC_STATUS_INVALID_PARAMETER));\n}\n')
 run(['gcc','-std=gnu17','-fms-extensions',*['-D'+x for x in manifest['defines']],*['-I'+str(stage/x) for x in ['system','src/inc','host']],native,'-o',BUILD/'status-native'],'status-native-build')
 expected=run([BUILD/'status-native'],'status-native')
 for variant in args.variants:
  project=BUILD/variant;project.mkdir(exist_ok=True)
  generated=ROOT/'generated'/('raw/TranslatedMsQuic' if variant == 'raw' else 'TranslatedMsQuic')
  library=generated/'TranslatedMsQuic.csproj'
  generated_hashes={p.name:sha(p) for p in generated.glob('*.cs')}
  if any(closure_data['generated'][variant].get(name)!=digest for name,digest in generated_hashes.items()):raise RuntimeError('Generated library does not match frozen product closure')
  write_test_host(generated,project)
  (project/'StatusProbe.cs').write_text('using System;\nusing Managed.Transport.Hosting;\ninternal static unsafe partial class Program { private static void PrintStatus(){\n'+''.join(f'Console.WriteLine("{n}="+Status.{n});\n' for n in status_names)+'Console.WriteLine("TlsAlert42="+Status.TlsAlert(42));\nConsole.WriteLine("FailedPending="+(Status.Failed(Status.Pending)?1:0));\nConsole.WriteLine("FailedInvalid="+(Status.Failed(Status.InvalidParameter)?1:0));\n}}\n')
  (project/'PlatformHost.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible><NoWarn>CS0162</NoWarn></PropertyGroup><ItemGroup><ProjectReference Include="'+escape(str(library))+'"/>'+''.join('<Compile Include="'+escape(str(p))+'"/>' for p in sourcefiles)+'<TrimmerRootAssembly Include="TranslatedMsQuic"/></ItemGroup></Project>\n')
  run(['dotnet','build',project/'PlatformHost.csproj','-c','Release','--nologo'],variant+'-build')
  commands=[('jit',['dotnet',project/'bin/Release/net10.0/PlatformHost.dll'])]
  if not args.jit_only:
   run(['dotnet','publish',project/'PlatformHost.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-o',project/'aot','--nologo'],variant+'-aot-build')
   commands.append(('aot',[project/'aot/PlatformHost']))
  for runtime,command in commands:
   output=run(command,variant+'-'+runtime)
   if output.strip()!='platform services and translated workers passed':raise RuntimeError('Incorrect platform receipt')
   actual=run([*command,'status'],variant+'-'+runtime+'-status')
   if actual!=expected:raise RuntimeError('Native status contract mismatch')
   for failure in ['self-join','bad-return']:run([*command,failure],variant+'-'+runtime+'-'+failure,negative=True)
  if generated_hashes!={p.name:sha(p) for p in generated.glob('*.cs')}:raise RuntimeError('Generated library changed during validation')
  receipt['variants'].append(dict(name=variant,passed=True,runtimes=[name for name,_ in commands],native_status_records=len(expected.splitlines()),generated_hashes=generated_hashes))
  print(variant+': platform services, actual workers, native status mapping PASS',flush=True)
 if receipt['source_hashes']!={str(p.relative_to(ROOT)):sha(p) for p in sourcefiles}:raise RuntimeError('Platform sources changed during validation')
 receipt['passed']=True
finally:(LOGS/'results.json').write_text(json.dumps(receipt,indent=2)+'\n')
