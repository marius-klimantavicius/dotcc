"""Class payload ownership against EDG C, including failure and termination."""
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys


def verify_class_exceptions(root, tests, out, nativeaot, run):
    cases=[(str(i),i,0,0) for i in range(32)]
    cases.extend((f'{i}-fail-{f}-{o}',i,f,o) for i,f,o in
                 [(0,1,1),(16,5,1),(16,1,1),(16,7,1),(17,4,1),(18,4,1),(6,1,2)])
    body=''.join(f'{{int r=run_case({i},{f},{o},hook);printf("{label}:%d:%llu:%d:%d:%d\\n",r,read_trace(),read_live(),read_bad(),read_alignment_bad());}}\n'
                 for label,i,f,o in cases)
    source=out/'class-exceptions-oracle.cpp'
    source.write_text('''#include <class_exceptions.cpp>
extern "C" int printf(const char*,...);
extern "C" int atoi(const char*);
extern "C" int fflush(void*);
int report_fatal;
void hook(int n){if(report_fatal){printf("%d\\n",n);fflush(0);}}
int main(int argc,char** argv){if(argc>1){report_fatal=1;terminate_case(atoi(argv[1]),hook);return 90;}
'''+body+'return 0;}\n')
    c_source=out/'class-exceptions-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'class-exceptions-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang: raise RuntimeError('Clang required for the C oracle')
    def terminated(command,argument,message):
        result=subprocess.run([*map(str,command),str(argument)],capture_output=True,text=True,
             timeout=15,preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
        assert result.returncode not in (0,90,91) and message in result.stderr,(argument,result.returncode,result.stdout,result.stderr)
        return result.stdout
    fatal={0:'Exception escaped catch parameter initialization',1:'Exception escaped EDG throw copy',
           2:'Destructor threw during unwinding',4:'Destructor threw during unwinding',
           5:'Exception escaped catch parameter initialization',6:'Exception escaped EDG throw copy'}
    traces=[]
    fatal_traces=[]
    alignment_observations={}
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'class-exceptions-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        native=run([exe]).stdout
        alignment_observations[label]={s.split(':')[0]:int(s.rsplit(':',1)[1]) for s in native.splitlines()}
        trace=''.join(s.rsplit(':',1)[0]+'\n' for s in native.splitlines())
        (out/f'class-exceptions-oracle-{label}.txt').write_text(trace)
        traces.append(trace)
        fatal_traces.append({str(argument):terminated([exe],argument,'terminate() called') for argument in fatal})
    assert traces[0]==traces[1],traces
    assert fatal_traces[0]==fatal_traces[1],fatal_traces
    # EDG libC eh_alloc_on_stack uses MOST_STRICT_ALIGNMENT, not an alignment
    # argument from __throw_setup_dtor. C# checks its explicit alignment separately.
    (out/'class-exceptions-oracle-alignment.json').write_text(json.dumps(alignment_observations,indent=2)+'\n')
    (out/'class-exceptions-termination-traces.json').write_text(json.dumps(fatal_traces[0],indent=2)+'\n')
    expected=traces[0]
    assert len(expected.splitlines())==len(cases)
    assert all(s.endswith(':0:0') for s in expected.splitlines()),expected
    checks=[f'class exceptions: {len(cases)} EDG C traces and {len(fatal)} termination cases agree under GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/f'class-exceptions-{mode}'
        result=run([sys.executable,root/'dotnet.py',tests/'class_exceptions.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size',
             '--split-size','2400',*(['--instance-methods'] if instance else ['--no-nest-types'])],
             env=dict(os.environ,EDG_DOT_TRACE='1'))
        (directory/'lowered-il.txt').write_text(result.stderr)
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function, receiver='owner'):
            prefix=f'_Z{len(function)}{function}'
            names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith(prefix)]
            assert len(names)==1,(function,names)
            return (receiver+'.' if instance else 'Core.')+names[0]
        body=''.join(f'{{int r={method("run_case")}({i},{f},{o},&Observe);if({method("read_alignment_bad")}()!=0)return 98;Console.WriteLine($"{label}:{{r}}:{{{method("read_trace")}()}}:{{{method("read_live")}()}}:{{{method("read_bad")}()}}");}}\n'
                     for label,i,f,o in cases)
        owner_test=''
        if instance:
            owner_test=f'''{method('reset')}(0,0,&Observe); {method('reset','second')}(0,0,&Observe);
if({method('export_catch')}(&RaiseOther)!=31 || {method('read_trace')}()!=0 || {method('read_trace','second')}()!=14 ||
   {method('read_live','second')}()!=0 || {method('read_bad','second')}()!=0)return 95;'''
        host=directory/'host';host.mkdir(exist_ok=True)
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core(), second = new Core();' if instance else '')}
  static readonly Exception injected=new InvalidOperationException("foreign");
  static bool reportFatal;
  static void Observe({'Core current, ' if instance else ''}int n) {{
    if(reportFatal)Console.WriteLine(n);
    GC.Collect(2, GCCollectionMode.Forced, true, true); GC.WaitForPendingFinalizers();
  }}
  static void Inject({'Core current' if instance else ''}) {{throw injected;}}
  static void InjectOperand({'Core current, ' if instance else ''}int n) {{throw injected;}}
  static void RaiseOther({'Core current' if instance else ''}) {{{method('export_raise','second')}();}}
  static int Main(string[] args) {{
    if(args.Length!=0) {{reportFatal=true;try {{{method('terminate_case')}(int.Parse(args[0]),&Observe);}} catch(Exception) {{return 91;}}return 90;}}
    {body}
    {method('reset')}(0,0,&Observe);
    try {{{method('through_foreign')}(&Inject);return 92;}} catch(Exception ex) {{
      if(!Object.ReferenceEquals(ex,injected) || {method('read_live')}()!=0 || {method('read_bad')}()!=0 || {method('read_trace')}()!=14)return 93;
    }}
    try {{{method('run_case')}(0,0,0,&InjectOperand);return 96;}} catch(Exception ex) {{
      if(!Object.ReferenceEquals(ex,injected) || {method('read_live')}()!=0 || {method('read_trace')}()!=1)return 97;
    }}
    {owner_test}
    return 0;
  }}
}}''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        commands=[('JIT',['dotnet',host/'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            commands.append(('NativeAOT',[host/'aot/Host']))
        for runtime,command in commands:
            actual=run(command,timeout=90).stdout
            (host/f'{runtime}-trace.txt').write_text(actual)
            assert actual==expected,(mode,runtime,expected,actual)
            checks.append(f'class exceptions/{mode}/{runtime}: {len(cases)} C traces, final destruction, references, alignment, forced GC and foreign/owner cleanup')
            for argument,message in fatal.items():
                actual_fatal=terminated(command,argument,message)
                assert actual_fatal==fatal_traces[0][str(argument)],(mode,runtime,argument,actual_fatal,fatal_traces[0])
            checks.append(f'class exceptions/{mode}/{runtime}: {len(fatal)} EDG-matched copy/cleanup termination cases')
    return checks
