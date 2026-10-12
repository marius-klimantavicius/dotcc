#!/usr/bin/env python3
"""Exact ordinary heap construction, exception cleanup and destructor-free delete."""
import argparse
import hashlib
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
        allocations=db.execute('''SELECT a.*,n.operator,n.value_category,n.file_id,n.line,n.column,
            s.name AS function_name FROM allocations a JOIN nodes n ON n.id=a.node_id
            JOIN symbols s ON s.id=n.function_id WHERE n.operator='construct' ORDER BY n.id''').fetchall()
        for allocation in allocations:
            assert allocation['value_category']=='prvalue' and allocation['file_id'] is not None
            assert allocation['line']>0 and allocation['column']>0
            assert allocation['result_byte_offset']==0 and allocation['construction_null_check'] in (0,1)
            assert all(allocation[key] is None for key in ('count_symbol_id','count_value_node_id',
                       'length_check_node_id','length_failure_symbol_id'))
            edges=db.execute('''SELECT e.role,e.ordinal,n.* FROM edges e JOIN nodes n ON n.id=e.child_id
                WHERE e.parent_id=? ORDER BY e.role,e.ordinal''',(allocation['node_id'],)).fetchall()
            by_role={role:[row for row in edges if row['role']==role] for role in
                     ('initialize','exception-protected','allocation-cleanup','exception-argument','placement-argument')}
            assert all(len(by_role[role])==1 for role in ('initialize','exception-protected','allocation-cleanup'))
            assert not by_role['placement-argument']
            initialize=by_role['initialize'][0];cleanup_node=by_role['allocation-cleanup'][0]
            assert initialize['id']==by_role['exception-protected'][0]['id']
            assert initialize['source_node_id']==cleanup_node['source_node_id']==allocation['node_id']
            action=db.execute('SELECT * FROM initializations WHERE id=?',(initialize['initialization_id'],)).fetchone()
            cleanup=db.execute('SELECT * FROM initializations WHERE id=?',(cleanup_node['initialization_id'],)).fetchone()
            assert action['kind']=='constructor' and action['constructor_symbol_id'] is not None
            assert action['destructor_symbol_id'] is None and action['lifetime_id'] is None
            assert cleanup['kind']=='none' and cleanup['free_on_exception']==1 and cleanup['destructor_symbol_id'] is not None
            assert db.execute('SELECT count(*) FROM lifetime_destructions WHERE initialization_id=?',
                              (cleanup['id'],)).fetchone()[0]==1
            routine=db.execute('SELECT * FROM symbols WHERE id=?',(cleanup['destructor_symbol_id'],)).fetchone()
            assert routine['name']=='operator delete'
            parameters=db.execute('SELECT parameter_type_id FROM type_parameters WHERE type_id=? ORDER BY ordinal',
                                  (routine['type_id'],)).fetchall()
            assert len(parameters)==1+len(by_role['exception-argument'])
            if allocation['function_name']=='construct_value':
                assert allocation['construction_null_check']==0
                assert len(by_role['exception-argument'])==1
                assert by_role['exception-argument'][0]['id']==allocation['size_node_id']
            elif allocation['function_name']=='construct_nullable':
                assert allocation['construction_null_check']==1 and not by_role['exception-argument']
            else:
                raise AssertionError(dict(allocation))
        if path.stem=='provider':
            assert {row['function_name'] for row in allocations}=={'construct_value','construct_nullable'}
        if path.stem=='main':
            deletes=db.execute('''SELECT n.*,s.name AS selected_name FROM nodes n JOIN symbols s ON s.id=n.symbol_id
                WHERE n.kind='delete' AND n.operator='scalar' ''').fetchall()
            assert deletes and all(row['selected_name']=='operator delete' for row in deletes)
            for node in deletes:
                assert not db.execute("SELECT 1 FROM edges WHERE parent_id=? AND role='destroy'",(node['id'],)).fetchone()
                assert db.execute("SELECT 1 FROM edges WHERE parent_id=? AND role='execution'",(node['id'],)).fetchone()
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',*(str(dict(row)) for row in allocations)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    if args.nativeaot and not args.linker:parser.error('--nativeaot requires --linker')
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/heap_constructions',source)
    compiler=args.compiler.resolve()
    paths=[compiler,RELATIONAL/'compile.py',Path(__file__).resolve(),*sorted(source.iterdir())]
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(path.read_bytes()).hexdigest()+'  '+str(path) for path in paths)+'\n')
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',compiler],work/f'{name}-compile.log')
        objects.append(obj)
    source.rename(work/'original-source')
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(path) for path in objects)+'\n')
    if args.linker:
        generated=work/'generated'
        run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*objects],work/'link.log')
        run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
        if args.nativeaot:
            run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
                 '-o',work/'nativeaot'],work/'nativeaot-build.log')
            apps=[path for path in (work/'nativeaot').iterdir()
                  if path.is_file() and path.suffix=='' and path.stat().st_mode & 0o111]
            if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
            run(apps,work/'nativeaot-run.log')
    print('PASS: native oracle and source-hidden SQL, exact heap guard/protected action/failure delete suffix, '+
          'saved result/size, argument/constructor/allocator failure, null skip and destructor-free record delete'+
          (', C#'+(', NativeAOT' if args.nativeaot else '') if args.linker else ''))
    print(work)


if __name__=='__main__':main()
