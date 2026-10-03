"""Build a deterministic pinned TZif bundle with IANA's pinned native zic.
This is dependency-data generation, not translated real-world acceptance.
All downloads, source, compiler output and zone files remain in output-dir.
"""
import argparse,hashlib,json,os,shutil,struct,subprocess,tarfile,urllib.request
from pathlib import Path

ROOT=Path(__file__).resolve().parent

def acquire(output,cache):
 spec=json.loads((ROOT/'source.json').read_text());output.mkdir(parents=True,exist_ok=False);source=output/'source';source.mkdir();commands=[]
 def run(command,**kwargs):
  command=list(map(str,command));r=subprocess.run(command,capture_output=True,text=True,**kwargs);i=len(commands);(output/f'{i:03}.log').write_text(r.stdout+r.stderr);commands.append({'command':command,'exit':r.returncode});(output/'commands.json').write_text(json.dumps(commands,indent=2)+'\n');r.check_returncode()
 for archive in spec['archives']:
  target=output/archive['file'];cached=cache/archive['file'] if cache else None
  if cached and cached.is_file():shutil.copyfile(cached,target)
  else:
   with urllib.request.urlopen(archive['url'],timeout=60) as response:target.write_bytes(response.read())
  if hashlib.sha256(target.read_bytes()).hexdigest()!=archive['sha256']:raise RuntimeError('tzdb archive hash mismatch: '+archive['file'])
  with tarfile.open(target) as data:data.extractall(source,filter='data')
 configuration=spec['configuration']
 if configuration['leapSeconds']:raise ValueError('portable POSIX time bundle excludes leap-second corrections')
 run(['make','-j2','zic','tzdata.zi','CFLAGS=-O2 -DHAVE_GETTEXT=0',*[key+'='+configuration[key] for key in ['DATAFORM','BACKWARD','PACKRATDATA','PACKRATLIST']]],cwd=source,env=dict(os.environ,LC_ALL='C'))
 zones=output/'zones';zones.mkdir();run([source/'zic','-b',configuration['zicBloat'],'-d',zones,source/'tzdata.zi'],env=dict(os.environ,LC_ALL='C'))
 entries=[];unique={};payload=bytearray()
 for path in sorted(p for p in zones.rglob('*') if p.is_file()):
  name=path.relative_to(zones).as_posix();data=path.read_bytes();digest=hashlib.sha256(data).hexdigest()
  if not data.startswith(b'TZif'):raise RuntimeError('non-TZif zic output: '+name)
  if digest not in unique:unique[digest]=(len(payload),len(data));payload.extend(data)
  offset,length=unique[digest];entries.append({'name':name,'offset':offset,'length':length,'sha256':digest})
 index_size=12+sum(10+len(e['name'].encode('ascii')) for e in entries)
 index=bytearray(b'EDGTZ01\0')+struct.pack('<I',len(entries))
 for e in entries:
  name=e['name'].encode('ascii');index+=struct.pack('<H',len(name))+name+struct.pack('<II',index_size+e['offset'],e['length'])
 bundle=bytes(index+payload);(output/'tzdb.bin').write_bytes(bundle)
 result={'version':spec['version'],'sourceManifestSha256':hashlib.sha256((ROOT/'source.json').read_bytes()).hexdigest(),'bundleSha256':hashlib.sha256(bundle).hexdigest(),'bundleBytes':len(bundle),'zones':len(entries),'uniquePayloads':len(unique),'entries':entries}
 (output/'bundle.json').write_text(json.dumps(result,indent=2)+'\n');shutil.copyfile(ROOT/'source.json',output/'source.json');shutil.copyfile(ROOT/'LICENSE',output/'LICENSE');print(json.dumps({k:v for k,v in result.items() if k!='entries'},indent=2))

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--cache',type=Path);a=p.parse_args();acquire(a.output_dir.resolve(),a.cache.resolve() if a.cache else None)
