"""Shared case table drives native and managed exact-trace tests."""
import json
import os
from pathlib import Path
import shutil
import resource
import subprocess
import sys


def verify_array_temps(root, tests, out, nativeaot, run):
    checks=[]
    cases=json.loads((tests/'array_temps_cases.json').read_text())
    value_functions={'returning','comma_lvalue'}
    native='''#include <cstdio>
#include "array_temps.cpp"
int fail;
void hook(int n){if(n==fail)throw n;}
int main(){
'''
    for label,function,fail,*args in cases:
        call=f'{function}(hook'+''.join(f', {a}' for a in args)+')'
        native+=f'fail={fail}; {{int caught=0,value=0;try {{'+('value=' if function in value_functions else '')+call+f';}}catch(int n){{caught=n;}}std::printf("{label}:%d:%d:%llu\\n",caught,value,read_trace());}}\n'
    native+='}\n'
    source=out/'array-temps-oracle.cpp'
    source.write_text(native)
    run(['g++','-std=c++17','-O0','-I',tests,source,'-o',out/'native-array-temps'])
    gcc_trace=run([out/'native-array-temps']).stdout
    (out/'gcc-array-temp-trace.txt').write_text(gcc_trace)
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:
        raise RuntimeError('Array trace tests require Clang; set EDG_TEST_CLANGXX to clang++.')
    run([clang,'-std=c++17','-O0','-I',tests,source,'-o',out/'native-array-temps-clang'])
    expected=run([out/'native-array-temps-clang']).stdout
    (out/'native-array-temp-trace.txt').write_text(expected)
    # GCC 13.3 duplicates prior-row destruction in these two fixtures. Keep
    # both traces and allow only these exact discrepancies; all others fail.
    known={
        ('multi-brace-third:3:0:1237676','multi-brace-third:3:0:12376'),
        ('multi-brace-fourth:4:0:123487676','multi-brace-fourth:4:0:1234876'),
    }
    assert len(gcc_trace.splitlines())==len(expected.splitlines())==len(cases)
    differences=[]
    for gcc_line,clang_line in zip(gcc_trace.splitlines(),expected.splitlines()):
        if gcc_line!=clang_line:
            assert (gcc_line,clang_line) in known,(gcc_line,clang_line)
            differences.append({'gcc':gcc_line,'clang':clang_line})
    (out/'array-native-oracles.json').write_text(json.dumps({
        'gccVersion':run(['g++','--version']).stdout.splitlines()[0],
        'clangVersion':run([clang,'--version']).stdout.splitlines()[0],
        'differences':differences},indent=2)+'\n')
    checks.append('array native oracles: GCC/Clang comparison with two explicit GCC 13 trace discrepancies')
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/f'array-temps-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'array_temps.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size',
             '--split-size','2400',*(['--instance-methods'] if instance else [])])
        exports=json.loads((directory/'dot-gen.json').read_text())['exports']
        def method(function):
            prefix=f'_Z{len(function)}{function}'
            names=[e['method'] for e in exports if e['symbol'].startswith(prefix)]
            assert len(names)==1,(function,exports)
            return ('owner.' if instance else 'Core.')+names[0]
        body=''
        for label,function,fail,*args in cases:
            call=method(function)+'(&Hook'+''.join(f', {a}' for a in args)+')'
            body+=f'fail={fail}; {{int caught=0,value=0;try {{'+('value=' if function in value_functions else '')+call+f';}}catch(Exception ex){{if(!Object.ReferenceEquals(ex,injected))throw;caught=fail;}}Console.WriteLine($"{label}:{{caught}}:{{value}}:{{{method("read_trace")}()}}");}}\n'
        host=directory/'host';host.mkdir(exist_ok=True)
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
    static int fail;
    static readonly Exception injected=new InvalidOperationException("injected");
    {('static readonly Core owner=new Core();' if instance else '')}
    static void Hook({'Core current, ' if instance else ''}int n) {{
        if(n==fail || (fail==98 && (n==9 || n==8)) || (fail==26 && (n==2 || n==6)))throw injected;
    }}
    static int Main(string[] args) {{
        if(args.Length!=0) {{
            fail=int.Parse(args[0]);
            try {{{method('defaults')}(&Hook);}} catch(Exception) {{return 91;}}
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
            assert actual==expected,f'{mode}/{runtime}\nexpected:\n{expected}\nactual:\n{actual}'
            checks.append(f'arrays/temporaries/{mode}/{runtime}: {len(cases)} exact native traces, return values and exception identity')
            for argument in ('98','26'):
                result=subprocess.run([*map(str,command),argument],capture_output=True,text=True,
                    preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
                assert result.returncode not in (0,90,91) and 'Array destructor threw during unwinding' in result.stderr,result.stderr
            checks.append(f'arrays/{mode}/{runtime}: partial/full array double-exception termination')
    return checks
