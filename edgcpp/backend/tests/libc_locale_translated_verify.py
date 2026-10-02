"""Locale handles and one-pass allocating printf under translated C/C++ JIT/AOT."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_locale(root,tests,out,nativeaot,run):
    out.mkdir(parents=True,exist_ok=True);printf=out/'printf';numeric=out/'strto'
    shutil.copytree(root/'backend/runtime/musl/printf',printf);shutil.copytree(root/'backend/runtime/musl/strto',numeric)
    sources=[tests/'libc_locale.c',tests/'libc_allocating_printf.c',printf/'vfprintf.c',printf/'format_allocating.c',
             *[numeric/name for name in ['intscan.c','floatscan.c','strtol.c','strtod.c','atoi.c','atol.c','atoll.c','atof.c','locale_wrappers.c']]]
    checks=[]
    native=out/'native';native.mkdir()
    (native/'bridge.c').write_text('#include <math.h>\ndouble __edg_scalbn_nearest(double x,int n){return scalbn(x,n);}\nint allocating_printf_cases(void);int main(void){return allocating_printf_cases();}\n')
    run(['gcc','-D_GNU_SOURCE','-U_FORTIFY_SOURCE','-D_FORTIFY_SOURCE=0','-std=c11','-O2','-fno-inline','-fno-builtin','-I',printf,
         tests/'libc_allocating_printf.c',printf/'vfprintf.c',printf/'format_allocating.c',native/'bridge.c','-lm','-o',native/'formatter'])
    run([native/'formatter']);checks.append('native one-pass asprintf including aliased %n and allocator failure')
    for language in ['c','c++']:
      for instance in [False,True]:
        directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
        run([sys.executable,root/'dotnet.py',*sources,'--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc',
             '-I',printf,'-I',numeric,'--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','30000','--output-dir',directory,
             *(['--instance-methods'] if instance else [])])
        manifest=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
        def call(symbol,owner='owner'):
            method=next(x['method'] for x in manifest['exports'] if x['symbol']==symbol)
            return (owner+'.' if instance else 'Core.')+method
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        owner='owner' if instance else 'Core'
        separate=f'using var other=new Core();if({call("locale_validate_owner","other")}(handle)!=0 || {call("locale_convention","other")}()==convention)throw new Exception("owner locale alias");' if instance else ''
        (host/'Program.cs').write_text(f'''using System;using System.Threading;
unsafe class Program {{static void Main() {{
{('using var owner=new Core();' if instance else '')}
int result={call('locale_cases')}();if(result!=0)throw new Exception($"locale case {{result}}");
result={call('allocating_printf_cases')}();if(result!=0)throw new Exception($"allocating formatter case {{result}}");
void* handle={call('locale_create')}();void* convention={call('locale_convention')}();
{separate}
if({call('locale_validate_owner')}(handle)!=1)throw new Exception("owned handle");
if({call('locale_free')}(handle)!=16)throw new Exception("selected locale lifetime");
{owner}.ReleaseThread();
Exception? failure=null;Thread worker=new(()=>{{try {{if({call('locale_validate_owner')}(handle)!=1)throw new Exception("worker handle");{owner}.ReleaseThread();}}catch(Exception error){{failure=error;}}}});worker.Start();worker.Join();if(failure!=null)throw failure;
if({call('locale_free')}(handle)!=71 || {call('locale_validate_owner')}(handle)!=0)throw new Exception("released/stale handle");
GC.Collect(2,GCCollectionMode.Forced,true,true);if({call('locale_convention')}()!=convention || **(byte**)convention!='.')throw new Exception("lconv pinning");
{owner}.Shutdown();Console.WriteLine("locale handles/_l conversions/owner-thread lifecycle and one-pass asprintf passed");
}}}}
''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(f'{directory.name}: JIT locale and allocating formatter')
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            run([host/'aot/Host']);checks.append(f'{directory.name}: NativeAOT locale and allocating formatter')
    return checks

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args()
    out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
    def run(command):
        nonlocal index
        command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300)
        (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
        if r.returncode:print(r.stdout+r.stderr)
        r.check_returncode()
    try:checks=verify_libc_locale(root,root/'backend/tests',out,a.nativeaot,run)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
