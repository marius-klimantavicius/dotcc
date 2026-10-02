"""Owned physical layouts: recursive references, validation, serialization, and raw emission parity."""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path

def verify_physical_types(root,tests,out,nativeaot,run):
 native=out/'native';run(['g++','-std=c++17',tests/'physical_types.cpp','-o',native]);run([native])
 checks=['owned physical types: native C++ oracle']
 validator=out/'validator'
 run(['g++','-std=c++17','-Wall','-Wextra','-Werror',tests/'physical_type_graph.cpp','-o',validator]);run([validator])
 checks.append('owned physical types: dangling edges, invalid extents/alignment and embedded cycles rejected')
 for instance in (False,True):
  mode='instance' if instance else 'static';common=['--instance-methods'] if instance else []
  baseline=out/(mode+'-direct');generated=out/mode
  for directory,options in [(baseline,[]),(generated,['--type-ir'])]:
   run([sys.executable,root/'dotnet.py',tests/'physical_types.cpp','--output-dir',directory,*common,*options])
  assert (baseline/'Translated.cs').read_bytes()==(generated/'Translated.cs').read_bytes()
  document=json.loads((generated/'dot-gen.types.json').read_text());manifest=json.loads((generated/'dot-gen.json').read_text())
  assert document['schema']==1 and document['scope']=='physical-types-only' and document['target']==manifest['target']['id']
  assert manifest['physicalTypeIR']['sha256']==hashlib.sha256((generated/'dot-gen.types.json').read_bytes()).hexdigest()
  types={t['id']:t for t in document['types']}
  node=next(t for t in types.values() if t['name']=='Node' and t['kind']=='record')
  fields={f['name']:f for f in node['fields']}
  assert types[fields['next']['type']]['element']==node['id']
  callback=types[types[fields['callback']['type']]['element']]
  assert callback['kind']=='function' and len(callback['parameters'])==1 and types[callback['result']]['size']==4
  assert fields['flags']['bitWidth']==3 and fields['delta']['bitWidth']==5
  assert any(t['kind']=='array' and t['count']==3 and t['element']==node['id'] for t in types.values())
  assert any(t['kind']=='record' and t['name']=='Opaque' and not t['complete'] for t in types.values())
  assert any(t['qualifiers']==3 for t in types.values())
  assert 'dot-gen.types.json' not in (generated/'Generated.csproj').read_text()
  project=generated/'Generated.csproj';run(['dotnet','build',project,'-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',generated/'bin/Release/net10.0/Generated.dll'])
  checks.append(mode+'/JIT: byte-identical C# and verified pointer-free recursive physical type graph')
  if nativeaot:
   run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',generated/'aot']);run([generated/'aot/Generated']);checks.append(mode+'/NativeAOT: physical layout ingestion parity')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);commands=[]
 def run(command,**kwargs):
  command=list(map(str,command));r=subprocess.run(command,text=True,capture_output=True,**kwargs);i=len(commands);(out/f'{i:03d}.stdout').write_text(r.stdout);(out/f'{i:03d}.stderr').write_text(r.stderr);commands.append({'command':command,'exit':r.returncode});(out/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
  if r.returncode:raise RuntimeError(f'{command}: {r.returncode}\n{r.stdout}\n{r.stderr}')
  return r
 root=Path(__file__).resolve().parents[2];checks=verify_physical_types(root,Path(__file__).resolve().parent,out,a.nativeaot,run);(out/'results.json').write_text(json.dumps({'checks':checks},indent=2)+'\n');print(len(checks),'owned physical type checks passed')
