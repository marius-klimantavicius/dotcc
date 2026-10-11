#!/usr/bin/env python3
"""Source compound statements where recorded types and constant effects permit."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone()==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows=db.execute('''SELECT n.operator,n.evaluation_order,t.kind,l.kind,r.kind,s.name
            FROM nodes n JOIN types t ON t.id=n.operation_type_id
            JOIN edges le ON le.parent_id=n.id AND le.role='operand' AND le.ordinal=0
            JOIN nodes l ON l.id=le.child_id LEFT JOIN symbols s ON s.id=l.symbol_id
            JOIN edges re ON re.parent_id=n.id AND re.role='operand' AND re.ordinal=1
            JOIN nodes r ON r.id=re.child_id
            WHERE n.operator IN ('bit-and-assign','bit-or-assign','bit-xor-assign') ORDER BY n.id''').fetchall()
        if path.stem=='provider':
            assert len(rows)>=7 and all(order=='right-to-left' for _,order,*_ in rows), rows
        return path.name+': '+str(rows)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/source_compounds',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/(name+'.dotobj')
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--output',obj,
             '--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve()],work/(name+'-compile.log'))
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n'.join(inspect(obj) for obj in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*objects],work/'link.log')
    code=(generated/'Translated.cs').read_text()
    for statement in ('c &= 7;','c |= 16;','c ^= 3;'):
        assert statement in code, statement
    assert 'Apply((&c))' in code and 'Apply((&narrow))' in code, code
    # The same int &= helper remains needed by expression/effectful sites,
    # while the only int |= and ^= sites are emitted directly.
    assert '__bit_and_assign_int' in code, code
    assert '__bit_or_assign_int' not in code and '__bit_xor_assign_int' not in code, code
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true','-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: ordinary named-local bitwise compound statements; helpers retain selected glvalue, conversion, RHS effects/order and complex targets; '
          'native/source-hidden C#'+('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
