"""ISO C byte-counted wide destinations: pinned native, host differences and accepted generated binaries."""
import argparse,hashlib,json,subprocess
from pathlib import Path
import libc_scanf_verify as scanner

from libc_scanf_verify import wide_field_corpus

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--reuse-generated',type=Path,required=True);a=p.parse_args();root=Path(__file__).resolve().parents[2];tests=root/'backend/tests';out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);reuse=a.reuse_generated.resolve();index=0;checks=[]
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=180);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:
  rows=wide_field_corpus();scanner.corpus=wide_field_corpus
  checks+=scanner.verify_libc_scanf(root,tests,out,False,run,translate=False)
  run(['gcc','-D_GNU_SOURCE','-std=c11','-O2','-fno-builtin',tests/'libc_scanf.c',tests/'libc_scanf_native.c','-o',out/'host-native']);run([out/'host-native',out/'cases.txt',out/'host.bin'])
  original=(out/'original.bin').read_bytes();host=(out/'host.bin').read_bytes();differences=[]
  for i,row in enumerate(rows):
   if original[i*512:(i+1)*512]!=host[i*512:(i+1)*512]:differences.append({'index':i,'format':row[1],'textHex':row[2].hex(),'musl':original[i*512:(i+1)*512].hex(),'host':host[i*512:(i+1)*512].hex()})
  (out/'host-differences.json').write_text(json.dumps(differences,indent=2));checks.append(f'{len(differences)} host-libc observational differences retained; host is not the byte-field oracle')
  def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
  assert digest(tests/'libc_scanf.c')==digest(reuse/'fixture.c')
  evidence={'fixture':digest(tests/'libc_scanf.c'),'provider':{},'executables':{}}
  for component in ['scanf','strto','multibyte']:
   metadata=json.loads((out/'provider'/component/'source.json').read_text());old_metadata=json.loads((reuse/'provider'/component/'source.json').read_text())
   assert metadata['sources']==old_metadata['sources'] and metadata['adapters']==old_metadata['adapters'],component
   for path in (out/'provider'/component).glob('*'):
    if path.is_file() and path.suffix in ['.c','.h']:
     relative=path.relative_to(out/'provider');old=reuse/'provider'/relative
     assert old.exists() and digest(path)==digest(old),relative
     evidence['provider'][str(relative)]=digest(path)
  for mode in ['c-static','c-instance','cxx-static','cxx-instance']:
   for runtime in ['JIT','NativeAOT']:
    binary=reuse/mode/'host'/('bin/Release/net10.0/Host.dll' if runtime=='JIT' else 'aot/Host')
    command=['dotnet',binary] if runtime=='JIT' else [binary]
    evidence['executables'][str(binary)]=digest(binary)
    if runtime=='JIT':evidence['executables'][str(binary.parent/'Generated.dll')]=digest(binary.parent/'Generated.dll')
    run([*command,out/'cases.txt',out/'golden.bin',out/(mode+'-'+runtime+'-file.bin')]);checks.append(f'{mode}/{runtime}: {len(rows)} byte-field cases on unchanged accepted generated binary')
  (out/'reused-inputs.json').write_text(json.dumps(evidence,indent=2))
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
