#!/usr/bin/env python3
"""Selected placement allocation/constructor ordering and exception-only cleanup."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    cleanup_ids=[]
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone())==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        assert 'construction_null_check' in {row['name'] for row in db.execute("PRAGMA table_info('allocations')")}
        allocations=db.execute('''SELECT a.*,n.kind,n.operator,n.type_id,n.value_category,n.constant_text,
            n.file_id,n.line,n.column,s.name AS function_name,t.kind AS allocated_kind,t.size AS allocated_size
            FROM allocations a JOIN nodes n ON n.id=a.node_id
            JOIN symbols s ON s.id=n.function_id JOIN types t ON t.id=a.allocated_type_id
            WHERE n.operator='placement' ORDER BY a.node_id''').fetchall()
        for allocation in allocations:
            assert allocation['kind']=='new' and allocation['value_category']=='prvalue', dict(allocation)
            assert allocation['constant_text'] is None and allocation['construction_null_check'] in (0,1), dict(allocation)
            assert allocation['allocated_kind']=='record' and allocation['allocated_size']>0, dict(allocation)
            assert allocation['result_byte_offset']==0 and allocation['count_symbol_id'] is None
            assert allocation['file_id'] is not None and allocation['line']>0 and allocation['column']>0
            edges=db.execute('''SELECT e.role,e.ordinal,n.id,n.initialization_id,n.source_node_id
                FROM edges e JOIN nodes n ON n.id=e.child_id WHERE e.parent_id=? ORDER BY e.role,e.ordinal''',
                (allocation['node_id'],)).fetchall()
            by_role={role:[row for row in edges if row['role']==role]
                     for role in ('placement-argument','exception-argument','initialize','exception-protected','allocation-cleanup')}
            assert all(len(rows)==1 and rows[0]['ordinal']==0 for rows in by_role.values()), [dict(row) for row in edges]
            assert by_role['placement-argument'][0]['id']==by_role['exception-argument'][0]['id']
            assert by_role['initialize'][0]['id']==by_role['exception-protected'][0]['id']
            for role in ('initialize','allocation-cleanup'):
                assert by_role[role][0]['source_node_id']==allocation['node_id'], dict(by_role[role][0])
            initialization=db.execute('SELECT * FROM initializations WHERE id=?',
                                     (by_role['exception-protected'][0]['initialization_id'],)).fetchone()
            cleanup=db.execute('SELECT * FROM initializations WHERE id=?',
                               (by_role['allocation-cleanup'][0]['initialization_id'],)).fetchone()
            if allocation['function_name']=='construct_copy':
                assert initialization['kind']=='expression' and initialization['constructor_symbol_id'] is None, dict(initialization)
                assert initialization['value_node_id'] is not None
                value=db.execute('SELECT type_id FROM nodes WHERE id=?',(initialization['value_node_id'],)).fetchone()
                assert value and value['type_id'] is not None
            else:
                assert initialization['kind']=='constructor' and initialization['constructor_symbol_id'] is not None
            assert initialization['destructor_symbol_id'] is None
            assert cleanup['kind']=='none' and cleanup['free_on_exception']==1 and cleanup['destructor_symbol_id'] is not None
            assert cleanup['temporary_lifetime']==1
            assert db.execute('SELECT count(*) FROM lifetime_destructions WHERE initialization_id=?',
                              (cleanup['id'],)).fetchone()[0]==1, dict(cleanup)
            cleanup_ids.append(cleanup['id'])
            size=db.execute('SELECT type_id FROM nodes WHERE id=?',(allocation['size_node_id'],)).fetchone()
            assert size and size['type_id'] is not None
        if path.stem=='provider':
            guards={row['function_name']:row['construction_null_check'] for row in allocations}
            assert guards=={'construct_pointer':0,'construct_tag':1,'construct_throwing':0,'construct_discard':0,'construct_copy':0}, guards
            discarded=next(row for row in allocations if row['function_name']=='construct_discard')
            assert db.execute('''SELECT 1 FROM edges e JOIN nodes n ON n.id=e.parent_id
                JOIN types t ON t.id=n.type_id WHERE e.child_id=? AND e.role='operand'
                AND n.kind='operation' AND n.operator='cast' AND t.kind='void' ''',
                (discarded['node_id'],)).fetchone(), 'Missing actual allocator-style void wrapper'
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{row["name"]}: {db.execute("SELECT count(*) FROM " + row["name"]).fetchone()[0]} rows' for row in tables]
        output += ['', 'selected placement allocations', *(str(dict(row)) for row in allocations)]
        return '\n'.join(output),cleanup_ids


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/placement_allocations',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    names=('provider','main','unsupported_conditional','unsupported_short_circuit')
    objects={}
    summaries=[];cleanup_ids=[]
    for name in names:
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects[name]=obj
        summary,owned=inspect(obj)
        summaries.append(summary)
        if name=='provider':cleanup_ids.extend(owned)
    (work/'database-summary.txt').write_text('\n\n'.join(summaries)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.PlacementAllocations',
         '--class','Core','--entry','main',objects['provider'],objects['main']],work/'link.log')
    code='\n'.join(path.read_text() for path in generated.glob('*.cs'))
    assert '__placement_argument_' in code and '__placement_result_' in code
    assert all(f'__constructed_{owned}' not in code for owned in cleanup_ids), 'Allocation cleanup entered ordinary lifetime flags'
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    for name in names[2:]:
        log=work/f'{name}-link.log'
        run(['dotnet',args.linker.resolve(),'link','--out',work/f'{name}-generated',
             objects['provider'],objects[name]],log,expected=1)
        assert 'placement construction requires statement expansion' in log.read_text(), log.read_text()
    print('PASS: exact placement guards/protected action/cleanup ownership, saved arguments, self identity, ctor/argument/allocator failure, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
