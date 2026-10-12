#!/usr/bin/env python3
"""Exercise bounded point/stream catalogs using distinct existing real objects."""
import argparse
import hashlib
from pathlib import Path
import shutil
import subprocess
import json


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('objects',nargs='+',type=Path)
    args=parser.parse_args()
    objects=[p.resolve() for p in args.objects]
    if len(set(objects))!=len(objects):
        parser.error('Use distinct object paths, not duplicated-object scaling')
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=Path(__file__).resolve().parent/'catalog_queries'
    shutil.copytree(source,work/'probe')
    linker=args.linker.resolve()
    commands=[(['dotnet','build',str(work/'probe/Probe.csproj'),'-c','Release','-o',str(work/'tool')],'build.log'),
              (['dotnet',str(work/'tool/Probe.dll'),str(linker),*[str(p) for p in objects]],'query-stress.log')]
    for argv,name in commands:
        with (work/name).open('w') as log:
            subprocess.run(argv,stdout=log,stderr=subprocess.STDOUT,check=True)
    hashes={str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in [linker,*objects,Path(__file__).resolve(),*source.iterdir()] if p.is_file()}
    (work/'hashes.json').write_text(json.dumps(hashes,indent=2)+'\n')
    print((work/'query-stress.log').read_text(),end='')
    print(work)


if __name__=='__main__':main()
