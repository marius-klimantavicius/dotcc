"""Synchronous source signal profile: native observations, source algorithms and generated execution."""
import argparse,json,resource,subprocess,sys
from pathlib import Path

PROFILE={'version':1,'delivery':'source-raise-abort','state':'owner','reset':'before-call','defaultAction':'process-failfast','osDelivery':False}

def verify_libc_signals(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[];source=root/'backend/runtime/libc/signals';headers=root/'backend/runtime/libc/include'
 native=out/'native';native.mkdir();(native/'include').mkdir()
 for name in ['signal.h','assert.h']:(native/'include'/name).write_bytes((headers/name).read_bytes())
 (native/'bridge.c').write_text('''#include <stdlib.h>
#include <stdio.h>
#ifdef __cplusplus
extern "C" {
#endif
void __edg_signal_terminate(int sig){_Exit(128+sig);}
void __edg_assert_report(const char*e,const char*f,int l,const char*n){fprintf(stderr,"Assertion failed: %s (%s:%d %s)\\n",e,f,l,n);}
int signals_contract(void);int signals_abort_escape(void);void signals_fatal(int);
#ifdef __cplusplus
}
#endif
int main(int argc,char**argv){if(argc>1){signals_fatal(atoi(argv[1]));return 90;}int r=signals_contract();if(r)return r;
#ifndef __cplusplus
if(signals_abort_escape())return 20;
#endif
return 0;}
''')
 def child(command,label,expected=None,diagnostic=None):
  def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
  command=list(map(str,command));r=subprocess.run(command,capture_output=True,text=True,timeout=45,preexec_fn=no_core)
  (out/(label+'.json')).write_text(json.dumps({'command':command,'returncode':r.returncode,'stdout':r.stdout,'stderr':r.stderr},indent=2))
  assert (r.returncode==expected if expected is not None else r.returncode!=0),(label,r.returncode,r.stdout,r.stderr)
  assert not any(m in r.stdout+r.stderr for m in ['source-finalizer','source-unwound','HOST_CAUGHT','HOST_FINALLY']),(label,r.stdout,r.stderr)
  if diagnostic:assert diagnostic in r.stderr,(label,r.stderr)
 for compiler,standard in [('gcc','c11'),('g++','c++17')]:
  binary=native/compiler;run([compiler,'-std='+standard,'-O2','-fno-builtin','-fsanitize=undefined','-fno-sanitize-recover=undefined','-I',native/'include',tests/'libc_signals.c',source/'signals.c',source/'assert_fail.c',native/'bridge.c','-o',binary]);run([binary])
  for mode in range(7):child([binary,str(mode)],compiler+'-'+str(mode),143 if mode==0 else 130 if mode==6 else 73 if mode in [4,5] else 134,'Assertion failed' if mode==5 else None)
  checks.append(compiler+': UBSan signal profile, identities, sentinels, errno, six signals, fatal/abort/assert (C includes longjmp escape)')
 # The host may retain or reset installed handlers. Only shared C contracts are
 # compared here; authored algorithm tests above choose reset-before-call.
 (native/'host.c').write_text('''#include <signal.h>
#include <stdlib.h>
#include <setjmp.h>
static volatile sig_atomic_t seen;
static jmp_buf target;
static void handler(int s){seen=s;}
static void escape(int s){longjmp(target,s);}
int main(void){int values[]={SIGABRT,SIGFPE,SIGILL,SIGINT,SIGSEGV,SIGTERM};
for(int i=0;i<6;++i){if(signal(values[i],SIG_IGN)==SIG_ERR||raise(values[i]))return 1;signal(values[i],SIG_DFL);}
if(signal(SIGINT,handler)==SIG_ERR||raise(SIGINT)||seen!=SIGINT)return 2;
if(setjmp(target)==0){signal(SIGABRT,escape);abort();}return 0;}
''')
 run(['gcc','-std=c11','-O2',native/'host.c','-o',native/'host']);run([native/'host']);checks.append('native host: six ignore dispositions, synchronous delivery, abort nonlocal escape; no assumed reset policy')
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   provider=json.loads((root/'backend/runtime/musl/provider.json').read_text());registered=any(c.get('sourceBase')=='libc' and c['directory']=='signals' for c in provider['components'])
   extra=[] if registered else [source/'signals.c',source/'assert_fail.c']
   run([sys.executable,root/'dotnet.py',tests/'libc_signals.c',*extra,'--language',language,'--target','portable-lp64-binary80-v1','--libc','musl','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','40000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());assert manifest['signals']==PROFILE,manifest.get('signals');methods={x['symbol']:x['method'] for x in manifest['exports']};host=directory/'host';host.mkdir();receiver='owner' if instance else 'Core'
   def call(n):return receiver+'.'+methods[n]
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   isolation=('''using var other=new Core();
'''+call('signals_owner_install')+'''(0);other.'''+methods['signals_owner_install']+'''(1);
if('''+call('signals_handler_address')+'''()!=other.'''+methods['signals_handler_address']+'''())throw new Exception("canonical handler identity");
if('''+call('signals_owner_raise')+'''()!=1||other.'''+methods['signals_owner_count']+'''()!=0||other.'''+methods['signals_owner_raise']+'''()!=10||'''+call('signals_owner_count')+'''()!=1)throw new Exception("owner signal state");
other.Shutdown();if('''+call('signals_owner_raise')+'''()!=1)throw new Exception("disposed other owner affected signal state");''') if instance else ''
   (host/'Program.cs').write_text('''using System;using System.Threading;unsafe class Program{
 static void Main(string[]args){'''+('using var owner=new Core();' if instance else '')+'''
 if(args.Length!=0){try{'''+call('signals_fatal')+'''(int.Parse(args[0]));}catch{Console.WriteLine("HOST_CAUGHT");}finally{Console.WriteLine("HOST_FINALLY");}throw new Exception("fatal returned");}
 int result='''+call('signals_contract')+'''();if(result!=0)throw new Exception("signal contract "+result);
'''+('if('+call('signals_abort_escape')+'()!=0)throw new Exception("abort nonlocal escape");' if language=='c' else '')+isolation+'''
 int failure=0;
 Thread a=new Thread(()=>{try{if('''+call('signals_registration_stress')+'''(0)!=0)Interlocked.Exchange(ref failure,1);}finally{'''+receiver+'''.ReleaseThread();}});
 Thread b=new Thread(()=>{try{if('''+call('signals_registration_stress')+'''(1)!=0)Interlocked.Exchange(ref failure,1);}finally{'''+receiver+'''.ReleaseThread();}});
 a.Start();b.Start();a.Join();b.Join();if(failure!=0)throw new Exception("concurrent registration");
 '''+call('signals_registration_stress')+'''(0);
 long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<8;++i)if('''+call('signals_registration_stress')+'''(i%2)!=0)throw new Exception("registration");long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
 if(bytes!=0)throw new Exception("steady-state registration allocation "+bytes);
 '''+call('signals_stress_reset')+'''();'''+receiver+'''.Shutdown();Console.WriteLine("source signal identity, owner state, atomic registration and zero-allocation passed");}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
   for prefix,label in [(['dotnet',host/'bin/Release/net10.0/Host.dll'],'jit')]+([([host/'aot/Host'],'aot')] if nativeaot else []):
    if label=='aot':run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
    run(prefix)
    for mode in range(7):child(prefix+[str(mode)],directory.name+'-'+label+'-'+str(mode),73 if mode in [4,5] else None,'Assertion failed' if mode==5 else None if mode==4 else 'Translated default signal action')
    checks.append(directory.name+'-'+label+': source identity/state/atomic/zero-allocation and seven fatal abort/assert/default scenarios')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_signals(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
