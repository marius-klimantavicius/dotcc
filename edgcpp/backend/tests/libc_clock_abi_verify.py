"""Clock and narrow locale imports reject incompatible public declarations."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_clock_abi(root,tests,out):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 cases=[
 ('short-timespec','struct timespec { int tv_sec,tv_nsec; };int clock_gettime(int,struct timespec*);','clock_gettime'),
 ('unsigned-timespec','struct timespec { unsigned long tv_sec;long tv_nsec; };int clock_gettime(int,struct timespec*);','clock_gettime'),
 ('misnamed-timespec-fields','struct timespec {long seconds,nanoseconds;};int clock_gettime(int,struct timespec*);','clock_gettime'),
 ('wrong-timespec-tag','struct other {long tv_sec,tv_nsec;};int clock_gettime(int,struct other*);','clock_gettime'),
 ('wrong-clockid','struct timespec {long tv_sec,tv_nsec;};int clock_gettime(long,struct timespec*);','clock_gettime'),
 ('wrong-time-result','int time(long*);','time'),
 ('wrong-difftime-width','double difftime(int,int);','difftime'),
 ('wrong-ctype-width','int isalpha(long);','isalpha'),
 ('wrong-ctype-locale','int isalpha_l(int,void*);','isalpha_l'),
 ('wrong-transform-count','unsigned long strxfrm(char*,const char*,unsigned int);','strxfrm'),
 ('wrong-collation-locale','struct wrong;int strcoll_l(const char*,const char*,struct wrong*);','strcoll_l')]
 for name,source,symbol in cases:
  path=out/f'{name}.c';path.write_text(source+f'\nvoid *address(void){{return (void*){symbol};}}\n')
  command=list(map(str,[sys.executable,root/'dotnet.py',path,'--language','c','--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--output-dir',out/name]))
  r=subprocess.run(command,capture_output=True,text=True);(out/f'{name}.command.json').write_text(json.dumps(command,indent=2));(out/f'{name}.log').write_text(r.stdout+r.stderr)
  assert r.returncode!=0 and 'libc import ABI' in r.stderr and symbol in r.stderr,(name,r.stderr)
  assert not (out/name/'Translated.cs').exists();checks.append(name+': rejected before managed emission')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[]
 try:checks=verify_libc_clock_abi(root,root/'backend/tests',out)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
