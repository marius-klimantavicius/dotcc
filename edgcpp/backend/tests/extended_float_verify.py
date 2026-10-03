"""GNU binary128 and half storage/arithmetic/conversions versus native EDG C."""
import argparse,json,os,shutil,subprocess,sys
from pathlib import Path

def verify_extended_float(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True)
 generator=out/'oracle';run(['gcc','-std=gnu17','-O2',tests/'extended_float_oracle.c','-o',generator])
 data=run([generator]).stdout;(out/'extended_float_data.h').write_text(data)
 source=out/'input.c';shutil.copy2(tests/'extended_float.c',source);checks=[]
 for language in ['c','c++']:
  label=language.replace('+','x');lowered=out/(label+'.c');oracle=out/(label+'-oracle')
  run([root/'build/bin/cpfe','--c11' if language=='c' else '--c++17','--gnu_version','130300','-I',out,'--gen_c_file_name',lowered,'-o',out/(label+'.il'),source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
  run(['gcc','-std=gnu11',lowered,root/'build/lib/libC.a','-lm','-o',oracle]);run([oracle]);checks.append(label+': original EDG C,2048 IEEE rows plus65536 half representations')
  for instance in [False,True]:
   directory=out/(label+('-instance' if instance else '-static'))
   run([sys.executable,root/'dotnet.py',source,'-I',out,'--language',language,'--output-dir',directory,*(['--instance-methods'] if instance else []),*(['--runtime','edg-exceptions'] if language=='c++' else [])])
   if instance:
    (directory/'EntryPoint.cs').write_text('using System; static class EntryPoint { static int Main() { using var a=new Edg.Generated.TranslatedProgram();using var b=new Edg.Generated.TranslatedProgram();int x=a.Run();GC.Collect();int y=b.Run();return x!=0?x:y; }}')
   manifest=json.loads((directory/'dot-gen.json').read_text());assert manifest['extendedFloating']=={'binary128':'software-ieee-v1','binary16':'system-half-v1'}
   project=directory/'Generated.csproj';run(['dotnet','build',project,'-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',directory/'bin/Release/net10.0/Generated.dll']);checks.append(directory.name+': JIT binary128/half operations, exact constants/conversions and exception identity')
   if nativeaot:
    run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);run([directory/'aot/Generated']);checks.append(directory.name+': NativeAOT IEEE arithmetic and storage')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);commands=[]
 def run(command,**kwargs):
  command=list(map(str,command));r=subprocess.run(command,capture_output=True,text=True,timeout=300,**kwargs);i=len(commands);(out/f'{i:03}.stdout').write_text(r.stdout);(out/f'{i:03}.stderr').write_text(r.stderr);commands.append({'command':command,'exit':r.returncode});(out/'commands.json').write_text(json.dumps(commands,indent=2)+'\n');r.check_returncode();return r
 root=Path(__file__).resolve().parents[2];checks=verify_extended_float(root,root/'backend/tests',out,a.nativeaot,run);(out/'results.json').write_text(json.dumps({'checks':checks},indent=2)+'\n');print(len(checks),'extended floating checks passed')
