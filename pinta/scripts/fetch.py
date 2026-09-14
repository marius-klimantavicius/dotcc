#!/usr/bin/env python3
"""Fetch the pinned archive once; verify every build input and fixture offline."""
import argparse, hashlib, json, pathlib, tarfile, tempfile, urllib.request, shutil
ROOT = pathlib.Path(__file__).resolve().parents[1]
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    parser=argparse.ArgumentParser(); parser.add_argument('--offline', action='store_true'); args=parser.parse_args()
    lock=json.loads((ROOT/'config/source-lock.json').read_text()); ref=ROOT/'ref'; ref.mkdir(exist_ok=True)
    archive=ref/'upstream.tar.gz'; target=ref/'upstream'
    if not archive.exists():
        if args.offline: raise SystemExit('Pinned archive missing; rerun without --offline')
        with tempfile.NamedTemporaryFile(dir=ref, delete=False) as tmp:
            pending=pathlib.Path(tmp.name)
            with urllib.request.urlopen(lock['archive_url']) as response: shutil.copyfileobj(response,tmp)
        if digest(pending)!=lock['archive_sha256']:
            pending.unlink(); raise SystemExit('Archive SHA-256 mismatch')
        pending.replace(archive)
    if digest(archive)!=lock['archive_sha256']: raise SystemExit('Archive SHA-256 mismatch')
    if not target.exists():
        with tempfile.TemporaryDirectory(dir=ref) as temp:
            with tarfile.open(archive) as tar: tar.extractall(temp,filter='data')
            (pathlib.Path(temp)/('pinta-'+lock['commit'])).rename(target)
    for entry in lock['files']+lock['fixtures']:
        path=target/entry['path']
        if not path.is_file() or digest(path)!=entry['sha256']: raise SystemExit('Input hash mismatch: '+entry['path'])
    print(f"Verified {len(lock['files'])} source/license files and {len(lock['fixtures'])} fixtures at {lock['commit']}")
if __name__=='__main__': main()
