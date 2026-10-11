#!/usr/bin/env python3
"""Named bitfields consume exact signedness and occupied-byte EDG layouts."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone()==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        assert db.execute('SELECT char_bit,little_endian FROM object_info').fetchone()==(8,1)
        rows=db.execute('''SELECT t.name,t.size,f.name,f.byte_offset,f.bit_offset,f.bit_width,f.bit_is_signed
            FROM fields f JOIN types t ON t.id=f.owner_type_id
            WHERE f.bit_width IS NOT NULL ORDER BY t.name,f.ordinal''').fetchall()
        by_name={name:(record,size,offset,bit,width,signed) for record,size,name,offset,bit,width,signed in rows}
        if path.stem!='unsupported_result':
            assert by_name['__cap_'][3:]==(1,63,0), rows
            assert by_name['__size_'][3:]==(1,7,0), rows
        assert by_name['signed_value'][2:]==(1,0,5,1), rows
        assert by_name['low'][2:]==(1,5,3,0), rows
        assert by_name['crossing'][2:]==(2,0,9,0), rows
        assert by_name['enabled'][2:]==(3,1,1,0), rows
        for _,size,_,offset,bit,width,signed in rows:
            assert signed in (0,1) and offset+(bit+width+7)//8<=size, rows
        return path.name+': '+str(rows)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/bitfields',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main','unsupported_result'):
        obj=work/(name+'.dotobj')
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--output',obj,
             '--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve()],work/(name+'-compile.log'))
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n'.join(inspect(obj) for obj in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*objects[:2]],work/'link.log')
    code=(generated/'Translated.cs').read_text()
    for declaration in ('public ulong __cap_', 'public byte __size_', 'public int signed_value', 'public bool enabled'):
        assert declaration in code, declaration
    assert 'private byte __bits_' in code
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true','-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    negative=subprocess.run(['dotnet',str(args.linker.resolve()),'link','--out',str(work/'unsupported-generated'),
                             '--entry','main',str(objects[2])],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
    (work/'unsupported-link.log').write_text(negative.stdout)
    assert negative.returncode!=0 and 'Value-producing bitfield assignment emission is not yet implemented' in negative.stdout, negative.stdout
    assert 'unsupported_result.cpp:' in negative.stdout, negative.stdout
    print('PASS: exact byte/in-byte layouts and EDG signedness; string-shaped and packed neighbour-preserving properties, '
          'signed/unsigned truncation, bool, cross-byte fields and once-only selected store order; native/source-hidden C#'+('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
