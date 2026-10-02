"""Unordered temporary groups, checked against EDG's C backend."""
import json
import os
from pathlib import Path
import re
import resource
import shutil
import subprocess
import sys


def verify_unordered(root, tests, out, nativeaot, run):
    oracle=root/'build/bin/cpfe'
    if not oracle.is_file():
        raise RuntimeError('Build the C oracle and runtime: edgcpp/build.sh')
    cases=json.loads((tests/'unordered_cases.json').read_text())
    body=''
    for label,function,fail,occurrence,*args in cases:
        call=f'{function}(hook'+''.join(f', {a}' for a in args)+')'
        body+=f'fail={fail};remaining={occurrence}; {{int caught=0,value=0;try {{value={call};}}catch(int n){{caught=n;}}printf("{label}:%d:%d:%llu\\n",caught,value,read_trace());}}\n'
    source=out/'unordered-oracle.cpp'
    source.write_text('''#include "unordered.cpp"
extern "C" int printf(const char*,...);
int fail,remaining;
void hook(int n){if(n==fail && --remaining==0)throw n;}
int main(){
'''+body+'}\n')
    env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc'))
    traces={}
    for label,compiler in [('portable',oracle)]:
        c_source=out/f'unordered-{label}.c'
        run([compiler,'--c++17','--gnu_version','130300','-I',tests,
             '--gen_c_file_name',c_source,'-o',out/f'unordered-{label}.il',source],env=env)
        executable=out/f'unordered-{label}'
        run(['gcc','-w','-std=gnu11',c_source,root/'build/lib/libC.a','-lm','-o',executable])
        traces[label]=run([executable]).stdout
        (out/f'unordered-{label}-trace.txt').write_text(traces[label])
        assert len(traces[label].splitlines())==len(cases)
    expected=traces['portable']
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clangxx=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clangxx:
        raise RuntimeError('EDG C oracle requires Clang; set EDG_TEST_CLANGXX.')
    # Force C mode even when the discovered executable is named clang++.
    run([clangxx,'-x','c','-w','-std=gnu11',out/'unordered-portable.c','-x','none',
         root/'build/lib/libC.a','-lm','-o',out/'unordered-portable-clang'])
    clang_trace=run([out/'unordered-portable-clang']).stdout
    (out/'unordered-portable-clang-trace.txt').write_text(clang_trace)
    assert clang_trace==expected,'EDG C evaluation differs between GCC and Clang; audit operand order.'
    comparison={
        'primary':'cpfe: original EDG C backend, DO_UNORDERED_EH_PROCESSING=1',
        'gccVersion':run(['gcc','--version']).stdout.splitlines()[0],
        'clangVersion':run([clangxx,'--version']).stdout.splitlines()[0],
        'portableCCompilersAgree':True}
    (out/'unordered-native-oracles.json').write_text(json.dumps(comparison,indent=2)+'\n')
    checks=['unordered temporaries: EDG C oracle agrees under GCC/Clang with whole-group liveness']
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/f'unordered-{mode}'
        result=run([sys.executable,root/'dotnet.py',tests/'unordered.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size',
             '--split-size','2400',*(['--instance-methods'] if instance else [])],
             env=dict(os.environ,EDG_DOT_TRACE='1'))
        (directory/'lowered-il.txt').write_text(result.stderr)
        assert re.search(r'flags=0,0,0,1\b',result.stderr),'Fixture must exercise unordered destruction metadata.'
        manifest=json.loads((directory/'dot-gen.json').read_text())
        assert manifest['cleanupModel']=='edg-region-tables'
        exports=manifest['exports']
        def method(function):
            prefix=f'_Z{len(function)}{function}'
            names=[e['method'] for e in exports if e['symbol'].startswith(prefix)]
            assert len(names)==1,(function,exports)
            return ('owner.' if instance else 'Core.')+names[0]
        body=''
        for label,function,fail,occurrence,*args in cases:
            call=method(function)+'(&Hook'+''.join(f', {a}' for a in args)+')'
            body+=f'fail={fail};remaining={occurrence}; {{int caught=0,value=0;try {{value={call};}}catch(Exception ex){{if(!Object.ReferenceEquals(ex,injected))throw;caught=fail;}}Console.WriteLine($"{label}:{{caught}}:{{value}}:{{{method("read_trace")}()}}");}}\n'
        host=directory/'host';host.mkdir(exist_ok=True)
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
    static int fail,remaining;
    static readonly Exception injected=new InvalidOperationException("injected");
    {('static readonly Core owner=new Core();' if instance else '')}
    static void Hook({'Core current, ' if instance else ''}int n) {{
        if((n==fail && --remaining==0) || (fail==-1 && (n==9 || n==7)) ||
           (fail==-2 && (n==2 || n==6)) || (fail==-3 && (n==7 || n==6)) ||
           (fail==-4 && (n==1 || n==7)) || (fail==-5 && (n==9 || n==6)))throw injected;
    }}
    static int Main(string[] args) {{
        if(args.Length!=0) {{
            fail=int.Parse(args[0]);
            try {{if(fail<=-4){{{method('run_receiver')}(&Hook);}}else{{{method('run_call')}(&Hook);}}}} catch(Exception) {{return 91;}}
            return 90;
        }}
        {body}
        return 0;
    }}
}}''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo'])
        commands=[('JIT',['dotnet',host/'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            commands.append(('NativeAOT',[host/'aot/Host']))
        for runtime,command in commands:
            actual=run(command).stdout
            (host/f'{runtime}-trace.txt').write_text(actual)
            if actual!=expected:
                differences=[(a,b) for a,b in zip(expected.splitlines(),actual.splitlines()) if a!=b]
                raise AssertionError(f'{mode}/{runtime}: EDG C / C# differences: {differences}; '
                                     f'line counts {len(expected.splitlines())}/{len(actual.splitlines())}')
            checks.append(f'unordered temporaries/{mode}/{runtime}: {len(cases)} EDG C traces, values, object addresses and exception identity')
            for argument in ('-1','-2','-3','-4','-5'):
                result=subprocess.run([*map(str,command),argument],capture_output=True,text=True,
                    preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
                assert result.returncode not in (0,90,91) and 'Destructor threw during unwinding' in result.stderr,result.stderr
            checks.append(f'unordered temporaries/{mode}/{runtime}: constructor/body/destructor and temporary-receiver double-exception termination')
    return checks
