"""Volatile packed/scalar bitfield values, sequencing, qualifiers and ownership."""
import argparse,json,os,re,subprocess,sys
from pathlib import Path

def verify_volatile_bitfields(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);source=tests/'volatile_bitfields.c';checks=[]
 for language in ['c','c++']:
  label=language.replace('+','x');native_source=out/f'oracle-{label}.{"cpp" if language=="c++" else "c"}'
  native_source.write_text('#include "volatile_bitfields.c"\n'+('extern "C" ' if language=='c++' else '')+'int printf(const char*,...);\nint main(void){unsigned long long value=volatile_bitfield_cases();printf("%llu\\n",value);for(unsigned i=0;i<volatile_bitfield_trace_count();++i)printf("%lld\\n",volatile_bitfield_trace_value(i));return !value;}\n')
  generated_c=out/f'oracle-{label}-lowered.c';native=out/f'oracle-{label}'
  run([root/'build/bin/cpfe','--c++17' if language=='c++' else '--c11','--gnu_version','130300',*(['--instantiate','used'] if language=='c++' else []),'-I',tests,'--gen_c_file_name',generated_c,'-o',out/f'oracle-{label}.il',native_source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')),cwd=out)
  run(['gcc','-std=gnu11','-O2',generated_c,root/'build/lib/libC.a','-lm','-o',native]);result=subprocess.run([str(native)],capture_output=True,text=True,timeout=30);(out/f'oracle-{label}.log').write_text(result.stdout+result.stderr);result.check_returncode();values=[int(value) for value in result.stdout.splitlines()];expected=values[0];trace_values=values[1:];assert expected!=0
  checks.append(label+': original EDG C oracle, values/evaluation and 512 independent sign/layout patterns')
  if not translate:continue
  for instance in [False,True]:
   directory=out/f'{label}-{"instance" if instance else "static"}';run([sys.executable,root/'dotnet.py',source,'--language',language,'--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','6000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
   generated='\n'.join(p.read_text() for p in directory.glob('*.cs'))
   readers=re.findall(r'\[global::System.Runtime.CompilerServices.MethodImpl\(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining\)\]\s*private static \w+ (__edg_volatile_bits\d+)\(byte\* p\) \{([^{}]+)\}',generated)
   assert readers, 'missing non-inlined volatile bitfield readers'
   for name,body in readers:
    assert 'global::System.Threading.Volatile.Read(ref p[' in body and 'Volatile.Write' not in body, name
    assert generated.count(name+'(')>1, 'unreferenced volatile reader '+name
   (directory/'volatile-reader-audit.json').write_text(json.dumps({'readers':len(readers),'volatileByteReads':sum(body.count('Volatile.Read(') for _,body in readers)},indent=2)+'\n')
   def call(name,owner='owner'):return (owner+'.' if instance else 'Core.')+next(e['method'] for e in manifest['exports'] if e['symbol']==name)
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   isolation=f'''using var other=new Core();if({call('volatile_bitfield_set','other')}(19)!=19||{call('volatile_bitfield_get')}()!=7||{call('volatile_bitfield_tls_set','other')}(23)!=23||{call('volatile_bitfield_tls_get')}()!=11)throw new Exception("owner alias");''' if instance else ''
   (host/'Program.cs').write_text(f'''using System;using System.Threading;unsafe class Program{{static void Main(){{
{('using var owner=new Core();' if instance else '')}
ulong result={call('volatile_bitfield_cases')}();if(result!={expected}UL){{long[] traceValues=[{','.join(str(value)+'L' for value in trace_values)}];for(uint i=0;i<traceValues.Length;++i){{long actual={call('volatile_bitfield_trace_value')}(i);if(actual!=traceValues[i])throw new Exception($"volatile trace mismatch index {{i}} actual {{actual}} expected {{traceValues[i]}}");}}throw new Exception($"native volatile digest mismatch {{result}}");}}
if({call('volatile_bitfield_set')}(7)!=7||{call('volatile_bitfield_tls_set')}(11)!=11)throw new Exception("initial fields");{isolation}
GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();if({call('volatile_bitfield_get')}()!=7||{call('volatile_bitfield_tls_get')}()!=11)throw new Exception("GC roots");
Exception? failure=null;Thread thread=new(()=>{{try{{if({call('volatile_bitfield_tls_get')}()!=0||{call('volatile_bitfield_tls_set')}(31)!=31)throw new Exception("new TLS");{('owner' if instance else 'Core')}.ReleaseThread();}}catch(Exception e){{failure=e;}}}});thread.Start();thread.Join();if(failure!=null)throw failure;if({call('volatile_bitfield_tls_get')}()!=11)throw new Exception("TLS alias");
{('owner' if instance else 'Core')}.Shutdown();Console.WriteLine("volatile bitfield native parity, byte/layout model, single evaluation, owner/TLS and GC passed");}}}}''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT volatile bitfields')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT volatile bitfields')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',required=True,type=Path);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];commands=[];checks=[]
 def run(command,**kwargs):
  command=list(map(str,command));r=subprocess.run(command,capture_output=True,text=True,timeout=420,**kwargs);i=len(commands);commands.append({'command':command,'exit':r.returncode});(out/f'{i:03}.stdout').write_text(r.stdout);(out/f'{i:03}.stderr').write_text(r.stderr);(out/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
  if r.returncode:raise RuntimeError(f'command {i} failed: {r.returncode}; see retained logs')
  return r
 try:checks=verify_volatile_bitfields(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
