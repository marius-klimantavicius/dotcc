#!/usr/bin/env python3
"""Selected vacuous destructor expressions evaluate their operands only."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone())==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        operations=db.execute('''SELECT n.*,s.name AS function_name,t.kind AS result_kind
            FROM nodes n JOIN symbols s ON s.id=n.function_id JOIN types t ON t.id=n.type_id
            WHERE n.operator IN ('dot-vacuous-destructor-call','points-to-vacuous-destructor-call')
            ORDER BY n.id''').fetchall()
        for operation in operations:
            assert operation['kind']=='operation' and operation['domain']=='program', dict(operation)
            assert operation['result_kind']=='void' and operation['symbol_id'] is None, dict(operation)
            assert operation['file_id'] is not None and operation['line']>0 and operation['column']>0
            operands=db.execute('''SELECT e.ordinal,n.* FROM edges e JOIN nodes n ON n.id=e.child_id
                WHERE e.parent_id=? AND e.role='operand' ORDER BY e.ordinal''',(operation['id'],)).fetchall()
            assert len(operands)==1 and operands[0]['ordinal']==0, [dict(row) for row in operands]
            assert operands[0]['domain']=='program' and operands[0]['function_id']==operation['function_id']
            names=db.execute('''SELECT e.ordinal,n.* FROM edges e JOIN nodes n ON n.id=e.child_id
                WHERE e.parent_id=? AND e.role='source-name' ''',(operation['id'],)).fetchall()
            assert len(names)<=1, [dict(row) for row in names]
            for name in names:
                assert name['ordinal']==0 and name['kind']=='reference-name' and name['domain']=='template-recipe'
                assert name['function_id'] is None and name['symbol_id'] is None and name['source_node_id']==operation['id']
        if path.stem=='provider':
            selected={row['function_name']:row['operator'] for row in operations}
            assert selected=={
                'points_scalar':'points-to-vacuous-destructor-call',
                'points_record':'points-to-vacuous-destructor-call',
                'dot_scalar':'dot-vacuous-destructor-call',
                'dot_record':'dot-vacuous-destructor-call',
                'dot_field':'dot-vacuous-destructor-call',
                'dot_value':'dot-vacuous-destructor-call'}, selected
            assert not db.execute("SELECT 1 FROM functions WHERE kind='destructor'").fetchone(), 'Vacuous operation acquired a destructor provider'
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='template-recipe' AND function_id IS NOT NULL").fetchone()
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', '', 'selected vacuous operations',
                          *(str(dict(row)) for row in operations)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/vacuous_destructors',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(path) for path in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*objects],work/'link.log')
    code=(generated/'Translated.cs').read_text()
    assert '__dtor_' not in code, 'Vacuous expression emitted a destructor ABI'
    assert '_ = scalar_pointer();' in code and '_ = object_pointer();' in code
    assert '_ = scalar_reference();' in code and '_ = object_reference();' in code
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected vacuous pointer/location/value effects once, no destructor provider, native/source-hidden cross-TU C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
