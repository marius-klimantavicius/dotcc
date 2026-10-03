"""Reject old or malformed fpos layouts before managed output."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_fpos_abi(root,out):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 cases=[('old-long','long'),('wrong-tag','struct wrong {long offset;unsigned state,value;}'),('short-offset','struct __edg_fpos {int offset;unsigned state,value;}'),('signed-state','struct __edg_fpos {long offset;int state;unsigned value;}'),('reordered-fields','struct __edg_fpos {unsigned state,value;long offset;}'),('wrong-names','struct __edg_fpos {long position;unsigned state,value;}'),('extra-field','struct __edg_fpos {long offset;unsigned state,value;long extra;}'),('incomplete','struct __edg_fpos')]
 for name,shape in cases:
  for symbol in ['fgetpos','fsetpos']:
   label=name+'-'+symbol;source=out/(label+'.c');source.write_text('typedef struct __edg_FILE FILE;\ntypedef '+shape+' position;\nint '+symbol+'(FILE*,position*);\nvoid *address(void){return (void*)'+symbol+';}\n')
   command=list(map(str,[sys.executable,root/'dotnet.py',source,'--language','c','--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--output-dir',out/label]));r=subprocess.run(command,capture_output=True,text=True);(out/(label+'.command.json')).write_text(json.dumps(command,indent=2));(out/(label+'.log')).write_text(r.stdout+r.stderr)
   assert r.returncode!=0 and 'libc import ABI' in r.stderr and symbol in r.stderr,(label,r.stdout,r.stderr);assert not (out/label/'Translated.cs').exists();checks.append(label+': rejected')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[]
 try:checks=verify_libc_fpos_abi(Path(__file__).resolve().parents[2],out)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
