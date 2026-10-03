"""Replay retained source adaptations for selected wide, multibyte and numeric components."""
import argparse,hashlib,json,shutil,subprocess
from pathlib import Path

def verify_musl_patches(root,out,components=('wide','multibyte','wide_numeric','stdlib_algorithms','scanf','wscanf','c_headers','string_case','strptime','prng','math_parts')):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 for component in components:
  source=root/'backend/runtime/musl'/component;destination=out/component
  shutil.copytree(source,destination/'snapshot');replay=destination/'replay';shutil.copytree(source/'originals',replay)
  manifest=json.loads((source/'source.json').read_text())
  for name,metadata in manifest['sources'].items():
   assert hashlib.sha256((replay/name).read_bytes()).hexdigest()==metadata['originalSha256']
   patch=destination/'snapshot'/metadata.get('patch',manifest.get('patch',f'patches/{name}.patch'))
   patch_digest=metadata.get('patchSha256',manifest.get('patchSha256'))
   if patch_digest:assert hashlib.sha256(patch.read_bytes()).hexdigest()==patch_digest
   if patch.stat().st_size:
    command=['patch','--batch','--fuzz=0','-p1','-d',str(replay),'-i',str(patch)]
    result=subprocess.run(command,capture_output=True,text=True)
    (destination/(name+'.command.json')).write_text(json.dumps(command,indent=2));(destination/(name+'.log')).write_text(result.stdout+result.stderr);result.check_returncode()
   assert hashlib.sha256((replay/name).read_bytes()).hexdigest()==metadata['adaptedSha256'],(component,name)
   assert (replay/name).read_bytes()==(source/name).read_bytes()
   if component=='c_headers':assert (root/'backend/runtime/libc/include'/name).read_bytes()==(source/name).read_bytes()
  for name,item in manifest.get('referenceOnly',{}).items():assert hashlib.sha256((source/'reference'/name).read_bytes()).hexdigest()==item['sha256']
  for name,digest in manifest['adapters'].items():assert hashlib.sha256((source/name).read_bytes()).hexdigest()==digest
  checks.append(f'{component}: {len(manifest["sources"])} original hashes and patch replays plus adapter hashes matched')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',required=True,type=Path);a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[]
 try:checks=verify_musl_patches(root,out)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
