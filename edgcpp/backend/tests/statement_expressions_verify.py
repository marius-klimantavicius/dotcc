"""Contained GNU statement expressions against original EDG C output."""
import argparse,json,os,subprocess,sys
from pathlib import Path

def verify_statement_expressions(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);source=out/'input.c';source.write_text((tests/'statement_expressions.c').read_text()+'\nint main(void){return statement_expression_cases();}\n');checks=[]
 for language in ['c','c++']:
  mode=language.replace('+','x');lowered=out/f'oracle-{mode}.c';native=out/f'oracle-{mode}'
  run([root/'build/bin/cpfe','--c11' if language=='c' else '--c++17','--gnu_version','130300','--gen_c_file_name',lowered,'-o',out/f'oracle-{mode}.il',source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
  run(['gcc','-std=gnu11',lowered,root/'build/lib/libC.a','-lm','-o',native]);run([native]);checks.append(mode+': original EDG C statement expressions')
  if not translate:continue
  for instance in [False,True]:
   directory=out/f'{mode}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',tests/'statement_expressions.c','--language',language,'--emit','managedlib','--namespace','','--class-name','Core','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());method=next(x['method'] for x in manifest['exports'] if x['symbol']=='statement_expression_cases');host=directory/'host';host.mkdir()
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   body=f'using var a=new Core();using var b=new Core();return a.{method}()==0 && b.{method}()==0?0:99;' if instance else f'return Core.{method}();'
   (host/'Program.cs').write_text('unsafe class Program { static int Main() {'+body+'} }')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT statement-expression captures/loops/nesting/aggregate and conditional evaluation')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT statement expression compatibility')
 if not translate:return checks
 for name,body,diagnostic in [
  ('outer-return','int main(void){return ({return 3;4;});}','GNU statement expression control flow'),
  ('outer-goto','int main(void){int x=({goto done;2;});return x;done:return 0;}','GNU statement expression outgoing jump'),
  ('vla','int main(void){volatile int n=3;return ({int a[n];a[0]=0;a[0];});}','variable-length array is not allowed'),
  ('raii','static int calls;struct Guard{~Guard(){++calls;}};int main(){return ({Guard g;1;});}','GNU statement expression'),
 ]:
  source=out/(name+'.c');source.write_text(body+'\n')
  command=[sys.executable,root/'dotnet.py',source,'--language','c++' if name=='raii' else 'c','--output-dir',out/(name+'-generated')]
  completed=subprocess.run(list(map(str,command)),capture_output=True,text=True)
  (out/(name+'.negative.log')).write_text(completed.stdout+completed.stderr)
  (out/(name+'.negative-command.json')).write_text(json.dumps(list(map(str,command)),indent=2))
  assert completed.returncode!=0 and diagnostic in completed.stderr,completed.stderr
  checks.append(name+': unsupported nonlocal/lifetime scope rejected explicitly')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command,**kwargs):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300,**kwargs)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_statement_expressions(root,root/'backend/tests',out,a.nativeaot,run,translate=not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
