"""Selected musl formatter translated with real imports, VaList, and FILE locking."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def verify_musl_printf(root, tests, out, nativeaot, run):
    out.mkdir(parents=True,exist_ok=True)
    library=root/'backend/runtime/musl/printf'
    headers=root/'backend/runtime/libc/include'
    sources=[tests/'musl_printf_translated.c',*[library/name for name in ['vfprintf.c','vsnprintf.c','format_wrappers.c']]]
    native=out/'native';native.mkdir(exist_ok=True)
    lowered=[]
    for index,source in enumerate(sources):
        generated=native/f'unit{index}.c';lowered.append(generated)
        run([root/'build/bin/cpfe','--c11','--gnu_version','130300','-I',headers,'-I',library,
             '--gen_c_file_name',generated,'-o',native/f'unit{index}.il',source],
            env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    (native/'bridge.c').write_text('''#include <stdio.h>
#include <stdarg.h>
#include <math.h>
int __edg_signbitf(float x){return !!signbit(x);}int __edg_signbit(double x){return !!signbit(x);}int __edg_signbitl(long double x){return !!signbit(x);}
int __edg_isfinitef(float x){return !!isfinite(x);}int __edg_isfinite(double x){return !!isfinite(x);}int __edg_isfinitel(long double x){return !!isfinite(x);}
double __edg_scalbn_nearest(double x,int n){return scalbn(x,n);}
unsigned long __edg_mb_cur_max(void){return 4;}
int __edg_with_file_lock(FILE*f,int(*body)(void*,const char*,va_list),void*c,const char*fmt,va_list args){flockfile(f);int r=body(c,fmt,args);funlockfile(f);return r;}
''')
    (native/'oracle.c').write_text('''#include <stdio.h>
#include <string.h>
extern int format_case(int,char*,unsigned long);
extern int format_file_case(const char*);
int main(int argc,char**argv){if(argc!=3)return 1;FILE*out=fopen(argv[1],"wb");if(!out)return 2;
for(int i=0;i<25;++i){char buffer[512];memset(buffer,0xa5,sizeof buffer);int result=format_case(i,buffer,sizeof buffer);if(result==-999)return 3;fwrite(&result,sizeof result,1,out);fwrite(buffer,1,sizeof buffer,out);}
if(fclose(out))return 4;return format_file_case(argv[2])==1?0:5;}
''')
    run(['gcc','-std=gnu11','-w','-U_FORTIFY_SOURCE','-D_FORTIFY_SOURCE=0','-frounding-math','-O2',
         *lowered,native/'bridge.c',native/'oracle.c',root/'build/lib/libC.a','-lm','-o',native/'oracle'])
    golden=native/'golden.bin';run([native/'oracle',golden,native/'stdio.bin'])
    assert len(golden.read_bytes())==25*516
    checks=['original EDG C: 25 selected-musl numerical/byte/vararg cases and nested FILE formatting']
    for language in ['c','c++']:
        for instance in [False,True]:
            mode='instance' if instance else 'static';directory=out/f'{language.replace("+","x")}-{mode}'
            run([sys.executable,root/'dotnet.py',*sources,'--language',language,'--target','portable-lp64-binary80-v1',
                 '--libc','dotcc','-I',library,'--emit','managedlib','--namespace','','--class-name','Core','--output-dir',directory,
                 *(['--instance-methods','--split','size','--split-size','20000'] if instance else [])])
            manifest=json.loads((directory/'dot-gen.json').read_text())
            def call(symbol,receiver='owner'):
                exported=[item['method'] for item in manifest['exports'] if item['symbol']==symbol]
                assert len(exported)==1,(symbol,exported)
                return (receiver+'.' if instance else 'Core.')+exported[0]
            host=directory/'host';host.mkdir(exist_ok=True)
            (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
            separate=(f'using var other=new Core();if({call("format_locale_width","other")}()!=1 || (nint){call("format_error_pointer","other")}()==saved)throw new Exception("owner locale/error pool alias");') if instance else ''
            (host/'Program.cs').write_text(f'''using System;using System.IO;using System.Text;using System.Runtime.InteropServices;
unsafe class Program {{ static int Main(string[] args) {{
{('using var owner=new Core();' if instance else '')}
byte[] golden=File.ReadAllBytes(args[0]);byte* buffer=(byte*)NativeMemory.Alloc(512);
try {{
for(int i=0;i<25;++i) {{new Span<byte>(buffer,512).Fill(0xa5);int actual={call('format_case')}(i,(sbyte*)buffer,512);int expected=BitConverter.ToInt32(golden,i*516);
if(actual!=expected || !new ReadOnlySpan<byte>(buffer,512).SequenceEqual(golden.AsSpan(i*516+4,512)))throw new Exception($"case {{i}} count {{actual}} != {{expected}}; actual={{Convert.ToHexString(new ReadOnlySpan<byte>(buffer,Math.Min(128,512)))}} expected={{Convert.ToHexString(golden.AsSpan(i*516+4,128))}}");
}}
nint saved=(nint){call('format_error_pointer')}();GC.Collect(2,GCCollectionMode.Forced,true,true);if(saved!=(nint){call('format_error_pointer')}())throw new Exception("error string moved");
if({call('format_locale_width')}()!=4)throw new Exception("selected locale lost");{separate}
byte[] path=Encoding.UTF8.GetBytes(args[1]+"\\0");fixed(byte* p=path)if({call('format_file_case')}((sbyte*)p)!=1)throw new Exception("FILE formatting/lock callback");
}} finally {{NativeMemory.Free(buffer);{('owner' if instance else 'Core')}.Shutdown();}}
Console.WriteLine("25 native numerical/byte cases, imported addresses, owner locale/pinning and nested FILE locks passed");return 0;
}} }}''')
            run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
            expected_output=run(['dotnet',host/'bin/Release/net10.0/Host.dll',golden,host/'jit-file.bin'],timeout=180).stdout
            (host/'jit.log').write_text(expected_output)
            checks.append(f'{language}/{mode}/JIT: '+expected_output.strip())
            if nativeaot:
                run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                     '-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'],timeout=240)
                output=run([host/'aot/Host',golden,host/'aot-file.bin'],timeout=180).stdout
                (host/'aot.log').write_text(output);assert output==expected_output
                checks.append(f'{language}/{mode}/NativeAOT: '+output.strip())
    return checks


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--output-dir',type=Path,required=True);parser.add_argument('--nativeaot',action='store_true');options=parser.parse_args()
    out=options.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];index=0;checks=[]
    def run(command,**kwargs):
        nonlocal index
        command=list(map(str,command));i=index;index+=1
        result=subprocess.run(command,text=True,capture_output=True,**kwargs)
        (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(result.stdout+result.stderr)
        if result.returncode:print(result.stdout+result.stderr,flush=True)
        result.check_returncode();return result
    try:checks=verify_musl_printf(root,root/'backend/tests',out,options.nativeaot,run)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))


if __name__=='__main__':main()
