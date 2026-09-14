#!/usr/bin/env python3
"""Copy immutable core/test closure, then apply exact before/patch/after hashes."""
import hashlib,json,pathlib,shutil,subprocess,sys,tempfile
ROOT=pathlib.Path(__file__).resolve().parents[1]
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def main():
    subprocess.run([sys.executable,str(ROOT/'scripts/fetch.py'),'--offline'],check=True)
    dest=ROOT/'generated/native-input';dest.parent.mkdir(exist_ok=True)
    with tempfile.TemporaryDirectory(dir=dest.parent) as tmp:
        temp=pathlib.Path(tmp)
        shutil.copytree(ROOT/'ref/upstream/Marius.Pinta',temp/'Marius.Pinta')
        for item in json.loads((ROOT/'config/patches.json').read_text()):
            source=temp/item['path'];patch=ROOT/'config'/item['patch']
            if sha(source)!=item['before_sha256'] or sha(patch)!=item['patch_sha256']:raise SystemExit('Patch input hash mismatch: '+item['path'])
            subprocess.run(['patch','--batch','--forward','-p1','-i',str(patch)],cwd=temp,check=True)
            if sha(source)!=item['after_sha256']:raise SystemExit('Patch output hash mismatch: '+item['path'])
        if dest.exists():shutil.rmtree(dest)
        temp.rename(dest)
    print(dest)
if __name__=='__main__':main()
