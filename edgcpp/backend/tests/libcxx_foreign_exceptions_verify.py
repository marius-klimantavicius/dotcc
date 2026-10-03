"""Opaque CLR exceptions across translated handlers, replacement and termination."""
import json
from pathlib import Path
import resource
import subprocess
import sys
from libcxx_exception_ptr_verify import write_exception_fixture


def verify_libcxx_foreign_exceptions(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    checks=[]
    def fatal(command,directory,label,which):
        def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
        command=list(map(str,command))
        try:result=subprocess.run(command,text=True,capture_output=True,preexec_fn=no_core,timeout=60)
        except subprocess.TimeoutExpired as error:
            (directory/(label+'.stdout')).write_bytes(error.stdout or b'');(directory/(label+'.stderr')).write_bytes(error.stderr or b'')
            (directory/(label+'.json')).write_text(json.dumps({'command':command,'timeout':60},indent=2));raise
        (directory/(label+'.stdout')).write_text(result.stdout);(directory/(label+'.stderr')).write_text(result.stderr)
        (directory/(label+'.json')).write_text(json.dumps({'command':command,'exit':result.returncode},indent=2))
        expected={0:['20/0','-20/0','900/0'],1:['30/10','900/0'],2:['40/0','900/0'],3:['12/0','-12/0','100/0','900/0']}[which]
        actual=[line for line in result.stdout.splitlines() if '/' in line]
        assert result.returncode!=0 and actual==expected,(label,result.returncode,expected,actual,result.stderr[:1000])
        reason={0:'Exception escaped noexcept function',1:'Destructor threw during unwinding',2:'C++ terminate',3:'Unexpected handler threw a forbidden exception'}[which]
        assert reason in result.stderr,(label,reason,result.stderr[:1000])
        checks.append(label+': owner terminate, opaque foreign identity policy and first-pass cleanup')
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/mode;combined=out/(mode+'.cpp')
        write_exception_fixture(root,tests/'libcxx/foreign_exceptions.cpp',library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++14','--emit','managedlib',
             '-I',acquisition/'config','-I',library/'include','-I',library/'src','--output-dir',directory,
             '--namespace','','--class-name','Core','--split','size','--split-size','3200',*(['--instance-methods'] if instance else [])])
        exports={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']}
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        receiver='owner' if instance else 'Core'
        def method(symbol):return receiver+'.'+exports[symbol]
        (host/'Program.cs').write_text(f'''using System;
using System.Collections.Generic;
unsafe class Program {{
  static readonly Exception foreign=new InvalidOperationException("foreign identity");
  static readonly List<string> events=new();
  {('static Core? expected;' if instance else '')}
  static void Inject({('Core callbackOwner' if instance else '')}) {{
    {('if(!ReferenceEquals(expected,callbackOwner))throw new Exception("wrong callback owner");' if instance else '')}
    throw foreign;
  }}
  static void Report({('Core callbackOwner,' if instance else '')}int what,int state) {{
    {('if(!ReferenceEquals(expected,callbackOwner))what=-996;' if instance else '')}
    string value=what+"/"+state;events.Add(value);Console.WriteLine(value);Console.Out.Flush();
  }}
  static void Check(bool ok,string what) {{if(!ok)throw new Exception(what);}}
  static void Trace(params string[] values) {{Check(string.Join(",",events)==string.Join(",",values),"trace "+string.Join(",",events));events.Clear();}}
  static void Main(string[] args) {{
    {('var owner=new Core();expected=owner;' if instance else '')}
    {method('foreign_install')}(&Report,&Inject);
    {('var other=new Core();other.'+exports['foreign_install']+'(&Report,&Inject);' if instance else '')}
    if(args.Length!=0){{{method('foreign_fatal')}(int.Parse(args[0]));throw new Exception("fatal returned");}}
    for(int i=0;i<3;++i) {{
      try {{{method('foreign_escape')}();throw new Exception("missing foreign throw");}}
      catch(Exception error) {{Check(ReferenceEquals(error,foreign),"opaque CLR identity changed");}}
      Trace("11/0","101/0","-11/0");Check({method('foreign_state')}()==0,"foreign escape state");
      Check({method('foreign_nested')}()==0,"nested handler restoration");Trace();
      Check({method('foreign_replacement')}(0)==0,"foreign to permitted C++ replacement");Trace("12/0","-12/0","100/0","200/1");
      Check({method('foreign_replacement')}(1)==0,"foreign replacement to bad_exception");Trace("13/0","-13/10","100/1","201/1");
      Check({method('foreign_state')}()==0,"foreign replacement state");
    }}
    {receiver}.ReleaseThread();{receiver}.Shutdown();{('other.Shutdown();' if instance else '')}
    Console.WriteLine("foreign CLR boundary checks passed");
  }}
}}''')
        project=host/'Host.csproj';run(['dotnet','build',project,'-c','Release'])
        runtime=['dotnet',host/'bin/Release/net10.0/Host.dll'];run(runtime)
        checks.append(f'foreign CLR boundaries: {mode} JIT identity, nested handlers, current_exception and dynamic replacements')
        for which in range(4):fatal([*runtime,str(which)],directory,f'{mode}-jit-{which}',which)
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            runtime=[host/'aot/Host'];run(runtime)
            checks.append(f'foreign CLR boundaries: {mode} NativeAOT identity, nested handlers, current_exception and dynamic replacements')
            for which in range(4):fatal([*runtime,str(which)],directory,f'{mode}-aot-{which}',which)
    return checks
