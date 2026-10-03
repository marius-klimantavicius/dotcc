"""Reject malformed private C11 thread import ABIs before C# emission."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_c11_threads_abi(root,out):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 cases={
  'create-token-width':('int __edg_c11_thread_create(unsigned*,int(*)(void*),void*);','unsigned token;return __edg_c11_thread_create(&token,0,0);'),
  'create-callback-return':('int __edg_c11_thread_create(unsigned long*,void*(*)(void*),void*);','unsigned long token;return __edg_c11_thread_create(&token,0,0);'),
  'create-callback-argument':('int __edg_c11_thread_create(unsigned long*,int(*)(int),void*);','unsigned long token;return __edg_c11_thread_create(&token,0,0);'),
  'create-result':('long __edg_c11_thread_create(unsigned long*,int(*)(void*),void*);','unsigned long token;return (int)__edg_c11_thread_create(&token,0,0);'),
  'exit-result':('int __edg_c11_thread_exit(int);','return __edg_c11_thread_exit(1);'),
  'exit-argument':('void __edg_c11_thread_exit(long);','__edg_c11_thread_exit(1);return 0;'),
  'join-output':('int __edg_c11_thread_join(unsigned long,long*);','long result;return __edg_c11_thread_join(1,&result);'),
  'mutex-token-signed':('int __edg_c11_mutex_lock(long*);','long token=0;return __edg_c11_mutex_lock(&token);'),
  'once-callback':('void __edg_c11_call_once(unsigned long*,int(*)(void));','unsigned long token=0;__edg_c11_call_once(&token,0);return 0;'),
  'tss-destructor':('int __edg_c11_tss_create(unsigned long*,void(*)(int));','unsigned long token;return __edg_c11_tss_create(&token,0);'),
  'timespec-tag':('struct wrong{long tv_sec,tv_nsec;};int __edg_c11_thread_sleep(const struct wrong*,struct wrong*);','struct wrong t={0,0};return __edg_c11_thread_sleep(&t,0);'),
  'timespec-field':('struct timespec{long tv_sec;int tv_nsec;};int __edg_c11_thread_sleep(const struct timespec*,struct timespec*);','struct timespec t={0,0};return __edg_c11_thread_sleep(&t,0);'),
  'variadic':('int __edg_c11_mutex_init(unsigned long*,...);','unsigned long token=0;return __edg_c11_mutex_init(&token,0);'),
 }
 for label,(declarations,body) in cases.items():
  source=out/(label+'.c');source.write_text(declarations+'\nint main(void){'+body+'}\n');directory=out/label
  command=[sys.executable,str(root/'dotnet.py'),str(source),'--language','c','--libc','dotcc','--target','portable-lp64-binary80-v1','--output-dir',str(directory)]
  r=subprocess.run(command,capture_output=True,text=True,timeout=60);(out/(label+'.command.json')).write_text(json.dumps(command));(out/(label+'.stderr')).write_text(r.stderr);(out/(label+'.stdout')).write_text(r.stdout)
  assert r.returncode!=0 and 'thread import ABI' in r.stderr,(label,r.returncode,r.stderr)
  assert not (directory/'Translated.cs').exists(),label
  checks.append(label+': strict thread ABI rejected')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();checks=[]
 try:checks=verify_libc_c11_threads_abi(root,out)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
