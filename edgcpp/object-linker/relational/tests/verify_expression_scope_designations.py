#!/usr/bin/env python3
"""Recorded void throws and record lvalue scopes retain cleanup and identity."""
import argparse
from pathlib import Path
import re
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,required=True)
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/expression_scope_designations',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/(name+'.dotobj')
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--output',obj,
             '--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve()],work/(name+'-compile.log'))
        objects.append(obj)
    with sqlite3.connect(objects[0]) as db:
        rows=db.execute('''SELECT s.name,n.id,t.kind,n.value_category,n.lifetime_id,c.kind,c.operator,
            (SELECT COUNT(*) FROM lifetime_destructions d WHERE d.lifetime_id=n.lifetime_id)
            FROM nodes n JOIN symbols s ON s.id=n.function_id JOIN types t ON t.id=n.type_id
            JOIN edges e ON e.parent_id=n.id AND e.role='operand' JOIN nodes c ON c.id=e.child_id
            WHERE n.kind='lifetime-scope' ORDER BY n.id''').fetchall()
        assert any(name=='discard_record' and kind=='record' and category=='lvalue' and life and count
            for name,_,kind,category,life,*_,count in rows),rows
        assert any(name=='throw_scoped' and kind=='void' and child=='throw' and life and count
            for name,_,kind,_,life,child,_,count in rows),rows
        (work/'database-summary.txt').write_text(str(rows)+'\n')
    source.rename(work/'original-source')
    generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*objects],work/'link.log')
    code=(generated/'Translated.cs').read_text()
    assert re.search(r'Accumulator\* __scope_address_\d+ = default;',code),code
    assert re.search(r'_ = \(&\(\*__scope_address_\d+\)\);',code),code
    assert not re.search(r'Accumulator __scope_value_|_ = \(\*__scope_address_',code),code
    assert 'void __scope_value_' not in code,code
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        output=work/'nativeaot'
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true','-o',output],work/'nativeaot-build.log')
        run([output/'Translated'],work/'nativeaot-run.log')
    print('PASS: exact void throw cleanup and record lvalue location, with no result copy; receiver/argument/payload/call failure cleanup, '
          'reference identity after cleanup; native/source-hidden C#'+('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
