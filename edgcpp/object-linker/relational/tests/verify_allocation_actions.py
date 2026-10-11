#!/usr/bin/env python3
"""Recorded runtime byte-zero initialization and nonthrowing object destruction."""
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
        zero=db.execute('''SELECT a.*,n.type_id,n.file_id,n.line,n.column,s.name AS function_name,
            init.id AS action_node,init.kind AS action_kind,init.type_id AS action_type,
            init.source_node_id,init.initialization_id,i.kind AS initialization_kind,i.type_id AS initialization_type,
            i.constructor_symbol_id,i.destructor_symbol_id,bytes.child_id AS byte_count_node,
            t.kind AS source_kind,t.size AS source_size,t.element_type_id
            FROM nodes n JOIN allocations a ON a.node_id=n.id JOIN symbols s ON s.id=n.function_id
            JOIN edges e ON e.parent_id=n.id AND e.role='initialize' AND e.ordinal=0
            JOIN nodes init ON init.id=e.child_id JOIN initializations i ON i.id=init.initialization_id
            JOIN edges bytes ON bytes.parent_id=init.id AND bytes.role='byte-count' AND bytes.ordinal=0
            JOIN types t ON t.id=init.type_id WHERE n.kind='new' AND n.operator='zero-initialized' ''').fetchall()
        for action in zero:
            assert action['action_kind']=='allocation-initialize' and action['initialization_kind']=='zero', dict(action)
            assert action['source_node_id']==action['node_id'] and action['action_type']==action['allocated_type_id']==action['initialization_type']
            assert action['byte_count_node']==action['size_node_id'], dict(action)
            assert action['count_symbol_id'] is not None and action['count_value_node_id'] is not None
            assert action['source_kind']=='array' and action['source_size']==0
            assert action['constructor_symbol_id'] is None and action['destructor_symbol_id'] is None
            assert action['result_byte_offset']==0 and action['construction_null_check'] in (0,1)
            assert action['file_id'] is not None and action['line']>0 and action['column']>0
        deletions=db.execute('''SELECT n.id,n.file_id,n.line,n.column,s.name AS function_name,
            operand.child_id AS pointer_node,action.id AS destruction_node,action.kind AS destruction_kind,
            action.source_node_id,i.id AS initialization_id,i.kind AS initialization_kind,i.destructor_symbol_id,
            policy.constant_text AS null_check,policy.type_id AS policy_type,execution.child_id AS execution_node,
            abi.id AS abi_node,abi.kind AS abi_kind,abi.operator AS abi_kind_name,abi.domain AS abi_domain,
            abi.function_id AS abi_owner,abi.source_node_id AS abi_source,abi.symbol_id AS abi_symbol,
            entry.link_name AS abi_link_name,entry.type_id AS abi_type
            FROM nodes n JOIN symbols s ON s.id=n.function_id
            JOIN edges operand ON operand.parent_id=n.id AND operand.role='operand' AND operand.ordinal=0
            JOIN edges d ON d.parent_id=n.id AND d.role='destroy' AND d.ordinal=0
            JOIN nodes action ON action.id=d.child_id JOIN initializations i ON i.id=action.initialization_id
            JOIN edges p ON p.parent_id=n.id AND p.role='null-check' AND p.ordinal=0
            JOIN nodes policy ON policy.id=p.child_id
            JOIN edges execution ON execution.parent_id=n.id AND execution.role='execution' AND execution.ordinal=0
            JOIN edges a ON a.parent_id=action.id AND a.role='selected-abi-entry' AND a.ordinal=0
            JOIN nodes abi ON abi.id=a.child_id JOIN symbols entry ON entry.id=abi.symbol_id
            WHERE n.kind='delete' AND n.operator='destroy' ''').fetchall()
        for deletion in deletions:
            assert deletion['destruction_kind']=='destroy' and deletion['initialization_kind']=='none', dict(deletion)
            assert deletion['destructor_symbol_id'] is not None and deletion['source_node_id']==deletion['id']
            assert deletion['null_check']=='true' and db.execute('SELECT kind FROM types WHERE id=?',(deletion['policy_type'],)).fetchone()[0]=='bool'
            assert deletion['abi_kind']=='function' and deletion['abi_kind_name']=='complete-destructor'
            assert deletion['abi_domain']=='template-recipe' and deletion['abi_owner'] is None
            assert deletion['abi_source']==deletion['destruction_node'] and deletion['abi_link_name']
            assert not db.execute('SELECT 1 FROM lifetime_destructions WHERE initialization_id=?',
                                  (deletion['initialization_id'],)).fetchone(), dict(deletion)
            assert deletion['file_id'] is not None and deletion['line']>0 and deletion['column']>0
            reused=db.execute('''WITH RECURSIVE graph(id) AS (
                SELECT ? UNION SELECT e.child_id FROM graph g JOIN edges e ON e.parent_id=g.id
                WHERE e.role='operand') SELECT 1 FROM graph WHERE id=?''',
                (deletion['execution_node'],deletion['pointer_node'])).fetchone()
            assert reused, 'Selected deallocator lost saved source pointer alias: '+str(dict(deletion))
            assert db.execute('''SELECT 1 FROM edges e JOIN nodes value ON value.id=e.child_id
                JOIN types t ON t.id=value.type_id WHERE e.parent_id=? AND e.role='operand'
                AND e.ordinal=2 AND value.kind='constant' ''',(deletion['execution_node'],)).fetchone(), 'Missing selected sized delete argument'
        if path.stem=='provider':
            assert len(zero)==1 and zero[0]['function_name']=='allocate_zero_words' and zero[0]['construction_null_check']==1, [dict(row) for row in zero]
            assert len(deletions)==1 and deletions[0]['function_name']=='release_value', [dict(row) for row in deletions]
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='template-recipe' AND function_id IS NOT NULL").fetchone()
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', '', 'selected runtime byte zero',
                          *(str(dict(row)) for row in zero), '', 'selected object delete', *(str(dict(row)) for row in deletions)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/allocation_actions',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    log=work/'unsupported-throwing-compile.log'
    run([sys.executable,RELATIONAL/'compile.py',source/'unsupported_throwing_delete.cpp',
         '--output',work/'unsupported-throwing.dotobj','--std','c++17',
         '--work-dir',work/'unsupported-throwing-work','--compiler',args.compiler.resolve()],log,expected=1)
    diagnostic=(work/'unsupported-throwing-work/stderr.txt').read_text()
    assert 'new/delete expression' in diagnostic and 'destructor-nonthrowing=0' in diagnostic, diagnostic
    assert not (work/'unsupported-throwing.dotobj').exists(), 'Unsupported throwing delete published an object'
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(path) for path in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*objects],work/'link.log')
    code=(generated/'Translated.cs').read_text()
    assert 'NativeMemory.Clear' in code and '__allocation_bytes_' in code and '__allocation_result_' in code
    assert '__delete_object_' in code and '__deleted_pointer_' in code
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: exact saved byte-zero/count/null guard, selected complete destruction/member cleanup/sized deallocator/pointer once, native/source-hidden cross-TU C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
