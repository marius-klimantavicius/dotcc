"""Return ownership follows executable EDG C-backend output, including EH."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_return_objects(root, tests, out, nativeaot, run):
    cases=json.loads((tests/'return_objects_cases.json').read_text())
    body=''
    for label,function,fail,occurrence,*args in cases:
        call=f'{function}(hook'+''.join(f', {a}' for a in args)+')'
        body+=f'fail={fail};remaining={occurrence}; {{int caught=0,value=0;try {{value={call};}}catch(int n){{caught=n;}}printf("{label}:%d:%d:%llu\\n",caught,value,read_trace());}}\n'
    source=out/'return-objects-oracle.cpp'
    source.write_text('''#include "return_objects.cpp"
extern "C" int printf(const char*,...);
int fail,remaining;
void hook(int n){if(n==fail && --remaining==0)throw n;}
int main(){
'''+body+'}\n')
    env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc'))
    c_source=out/'return-objects-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'return-objects-oracle.il',source],env=env)
    run(['gcc','-w','-std=gnu11',c_source,root/'build/lib/libC.a','-lm','-o',out/'return-objects-edg'])
    expected=run([out/'return-objects-edg']).stdout
    (out/'edg-return-object-trace.txt').write_text(expected)
    assert len(expected.splitlines())==len(cases)
    # Other compilers are informative comparisons, not substitutes for EDG's
    # own portable-EH lowering. Preserve all differences for source auditing.
    comparisons={}
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    for label,compiler in [('gcc','g++'),*([('clang',clang)] if clang else [])]:
        executable=out/f'return-objects-{label}'
        run([compiler,'-std=c++17','-O0','-I',tests,source,'-o',executable])
        trace=run([executable]).stdout
        assert len(trace.splitlines())==len(cases),(label,trace)
        (out/f'{label}-return-object-trace.txt').write_text(trace)
        comparisons[label]={'version':run([compiler,'--version']).stdout.splitlines()[0],
            'differences':[{'edg':a,label:b} for a,b in zip(expected.splitlines(),trace.splitlines()) if a!=b]}
    (out/'return-native-oracles.json').write_text(json.dumps(comparisons,indent=2)+'\n')
    checks=['return objects: executable EDG C-backend/libC oracle; GCC/Clang comparisons recorded']
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/f'return-objects-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'return_objects.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size',
             '--split-size','2400',*(['--instance-methods'] if instance else [])])
        exports=json.loads((directory/'dot-gen.json').read_text())['exports']
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
        if(n==fail && --remaining==0)throw injected;
    }}
    static int Main() {{
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
            checks.append(f'return objects/{mode}/{runtime}: {len(cases)} EDG C traces, values, object addresses and exception identity')
    return checks
