"""Process callback and status ABI rejection, including routine addresses."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_process_exit_abi(root,out):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 cases={
  'atexit-result':('void atexit(void(*)(void));','atexit(0);return 0;'),
  'atexit-callback-result':('int atexit(int(*)(void));','return atexit(0);'),
  'atexit-callback-argument':('int atexit(void(*)(void*));','return atexit(0);'),
  'atexit-address':('int atexit(void(*)(int));','int(*volatile p)(void(*)(int))=atexit;return p(0);'),
  'quick-registration-variadic':('int at_quick_exit(void(*)(int,...));','return at_quick_exit(0);'),
  'exit-result':('int exit(int);','return exit(7);'),
  'exit-status':('void exit(long);','exit(7);return 0;'),
  'quick-status':('void quick_exit(unsigned);','quick_exit(7);return 0;'),
  'immediate-address':('void _Exit(long);','void(*volatile p)(long)=_Exit;p(7);return 0;'),
 }
 for label,(declarations,body) in cases.items():
  source=out/(label+'.c');source.write_text(declarations+'\nint main(void){'+body+'}\n');directory=out/label;command=[sys.executable,str(root/'dotnet.py'),str(source),'--language','c','--libc','dotcc','--target','portable-lp64-binary80-v1','--output-dir',str(directory)]
  r=subprocess.run(command,capture_output=True,text=True,timeout=60);(out/(label+'.command.json')).write_text(json.dumps(command));(out/(label+'.stdout')).write_text(r.stdout);(out/(label+'.stderr')).write_text(r.stderr)
  assert r.returncode!=0 and 'process import ABI' in r.stderr,(label,r.returncode,r.stderr)
  assert not (directory/'Translated.cs').exists(),label
  checks.append(label+': rejected before emission')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();checks=[]
 try:checks=verify_libc_process_exit_abi(root,out)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
