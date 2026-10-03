"""GNU builtin calls bind exact translated C definitions before BCL fallback."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_source_builtins(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[];fixture=tests/'libc_source_builtins.c'
 run(['gcc','-std=c11','-O0',fixture,'-lm','-o',out/'native']);run([out/'native']);checks.append('Native GNU builtin scalar math oracle')
 symbols={'fmod','fmodf','fmodl','modf','modff','modfl','abs'}
 for language in ('c','c++'):
  for instance in (False,True):
   label=language.replace('+','x')+('-instance' if instance else '-static');directory=out/label
   run([sys.executable,root/'dotnet.py',fixture,'--libc','musl','--language',language,'--split','function','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   source=''.join(p.read_text() for p in directory.glob('Translated*.cs'));manifest=json.loads((directory/'dot-gen.json').read_text())
   for symbol in symbols:assert '// dot-gen:source-builtin '+symbol+'\n' in source,symbol
   assert not symbols.intersection(manifest['imports']);assert '__builtin_frexp' in manifest['imports']
   run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',directory/'bin/Release/net10.0/Generated.dll']);checks.append(label+': JIT exact source definitions, output pointers/domain errno, audited BCL fallback')
   if nativeaot:
    run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);run([directory/'aot/Generated']);checks.append(label+': NativeAOT source builtin resolution')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0
 def run(command,**kwargs):
  global index
  r=subprocess.run(list(map(str,command)),capture_output=True,text=True,**kwargs);prefix=out/f'command-{index:04d}';index+=1;prefix.with_suffix('.json').write_text(json.dumps(list(map(str,command))));prefix.with_suffix('.stdout').write_text(r.stdout);prefix.with_suffix('.stderr').write_text(r.stderr)
  if r.returncode:raise RuntimeError(f'{command}: {r.returncode}\n{r.stdout}\n{r.stderr}')
  return r
 root=Path(__file__).resolve().parents[2];checks=verify_libc_source_builtins(root,root/'backend/tests',out,a.nativeaot,run);(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n');print(json.dumps(checks,indent=2))
