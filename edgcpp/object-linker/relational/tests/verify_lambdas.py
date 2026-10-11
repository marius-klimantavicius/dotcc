#!/usr/bin/env python3
"""EDG-selected lambda closure initialization, call identity and reference storage."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def underlying(db, identity):
    while identity is not None:
        row=db.execute('SELECT kind,underlying_type_id FROM types WHERE id=?',(identity,)).fetchone()
        if row['kind'] not in ('typedef','qualified'):
            return identity
        identity=row['underlying_type_id']
    return None


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone())==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        closures=db.execute('''SELECT n.*,s.name AS owner_name,call.name AS call_name,
            call.type_id AS callable_type,call.link_name AS callable_link,
            t.kind AS closure_kind,i.id AS selected_initialization_id,i.kind AS action_kind,i.type_id AS action_type,
            i.destructor_symbol_id,i.value_node_id,i.destination_initialization_id
            FROM nodes n JOIN symbols s ON s.id=n.function_id
            JOIN types t ON t.id=n.type_id JOIN symbols call ON call.id=n.symbol_id
            JOIN initializations i ON i.id=n.initialization_id OR
                (n.kind='record-initializer' AND n.operator='lambda' AND i.value_node_id=n.id)
            WHERE n.kind='lambda' OR (n.kind='record-initializer' AND n.operator='lambda'
                AND NOT EXISTS(SELECT 1 FROM nodes root WHERE root.kind='lambda'
                    AND root.initialization_id=i.id)) ORDER BY n.id''').fetchall()
        for closure in closures:
            assert closure['domain']=='program' and closure['closure_kind']=='record', dict(closure)
            assert closure['call_name']=='operator()' and closure['callable_link'], dict(closure)
            assert closure['value_category'] in ('lvalue','xvalue','prvalue')
            assert underlying(db,closure['action_type'])==underlying(db,closure['type_id'])
            assert closure['destructor_symbol_id'] is None
            assert closure['file_id'] is not None and closure['line']>0 and closure['column']>0
            assert db.execute('SELECT 1 FROM functions WHERE symbol_id=?',(closure['symbol_id'],)).fetchone(), dict(closure)
            if closure['action_kind']=='aggregate':
                assert db.execute('SELECT 1 FROM record_initializers WHERE node_id=?',
                                  (closure['value_node_id'],)).fetchone(), dict(closure)
                fields=db.execute('''SELECT e.field_id,e.base_subobject_id,f.owner_type_id,f.type_id
                    FROM record_initializer_elements e JOIN fields f ON f.id=e.field_id
                    WHERE e.initializer_node_id=? ORDER BY e.ordinal''',
                    (closure['value_node_id'],)).fetchall()
                assert fields and all(field['owner_type_id']==closure['type_id'] and
                                      field['base_subobject_id'] is None for field in fields), [dict(row) for row in fields]
            else:
                assert closure['action_kind']=='none', dict(closure)
        if path.stem=='provider':
            by_owner={row['owner_name']:row for row in closures}
            assert set(by_owner)=={'captureless_call','captureless_reference','captured_values',
                                   'captured_reference','capture_once'}, list(by_owner)
            assert len(closures)==5, [dict(row) for row in closures]
            assert all(by_owner[name]['action_kind']=='none' for name in
                       ('captureless_call','captureless_reference'))
            assert all(by_owner[name]['action_kind']=='aggregate' for name in
                       ('captured_values','captured_reference','capture_once'))
            captured=by_owner['captured_reference']
            assert db.execute('''SELECT 1 FROM fields f JOIN types t ON t.id=f.type_id
                WHERE f.owner_type_id=? AND t.kind='lvalue-reference' ''',(captured['type_id'],)).fetchone(), dict(captured)
            initialized=by_owner['capture_once']
            assert db.execute('''WITH RECURSIVE graph(id) AS (
                SELECT ? UNION SELECT e.value_node_id FROM record_initializer_elements e
                    JOIN graph g ON g.id=e.initializer_node_id
                UNION SELECT i.value_node_id FROM nodes n JOIN initializations i ON i.id=n.initialization_id
                    JOIN graph g ON g.id=n.id WHERE i.value_node_id IS NOT NULL
                UNION SELECT e.child_id FROM edges e JOIN graph g ON g.id=e.parent_id
                    WHERE e.role='operand')
                SELECT 1 FROM graph g JOIN nodes n ON n.id=g.id JOIN symbols s ON s.id=n.symbol_id
                WHERE n.operator='call' AND s.name='selected_value' ''',
                (initialized['value_node_id'],)).fetchone(), dict(initialized)
            helpers=db.execute('''SELECT s.id,s.name,s.link_name,s.is_definition,fun.body_node_id
                FROM symbols s JOIN functions fun ON fun.symbol_id=s.id WHERE s.name='_FUN' ''').fetchall()
            assert helpers and any('converted_call' in helper['link_name'] for helper in helpers), [dict(row) for row in helpers]
            for helper in helpers:
                assert helper['is_definition']==1
                assert db.execute('''SELECT 1 FROM nodes body JOIN nodes source ON source.id=body.source_node_id
                    JOIN symbols selected ON selected.id=source.symbol_id
                    WHERE body.id=? AND body.is_synthetic=1 AND source.domain='template-recipe'
                        AND source.function_id IS NULL AND source.operator='lambda-call-operator'
                        AND selected.name='operator()' ''',(helper['body_node_id'],)).fetchone(), dict(helper)
                assert db.execute('''SELECT 1 FROM nodes call JOIN edges e ON e.parent_id=call.id AND e.role='operand' AND e.ordinal=1
                    JOIN nodes receiver ON receiver.id=e.child_id WHERE call.function_id=? AND call.operator='call'
                        AND receiver.constant_text='0' ''',(helper['id'],)).fetchone(), dict(helper)
                storage=db.execute('''SELECT n.* FROM nodes n JOIN symbols s ON s.id=n.symbol_id
                    WHERE n.function_id=? AND n.kind='initialize' AND s.kind='local' ''',
                    (helper['id'],)).fetchall()
                assert storage and all(row['initialization_id'] is None and row['is_synthetic']==1
                                       and row['source_node_id'] is not None for row in storage), dict(helper)
                assert all(not db.execute('SELECT 1 FROM edges WHERE parent_id=?',(row['id'],)).fetchone()
                           for row in storage), [dict(row) for row in storage]
                assert not db.execute('''SELECT 1 FROM symbols s WHERE s.parent_symbol_id=? AND s.kind='local'
                    AND NOT EXISTS(SELECT 1 FROM nodes n WHERE n.symbol_id=s.id AND n.kind='initialize')''',
                    (helper['id'],)).fetchone(), dict(helper)
            assert db.execute('''SELECT 1 FROM nodes root JOIN symbols owner ON owner.id=root.function_id
                JOIN types result ON result.id=root.type_id JOIN symbols selected ON selected.id=root.symbol_id
                JOIN functions body ON body.symbol_id=selected.id
                WHERE owner.name='converted_call' AND root.kind='routine-address' AND root.value_category='prvalue'
                    AND result.kind='pointer' AND root.file_id IS NOT NULL AND root.line>0 AND root.column>0
                    AND selected.name='_FUN' AND selected.is_definition=1
                    AND NOT EXISTS(SELECT 1 FROM edges e WHERE e.parent_id=root.id)''').fetchone()
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', '', 'selected lambda closures',
                          *(str(dict(row)) for row in closures)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/lambdas',source)
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
    assert '__temporary_' in code and 'selected_value' in code
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected lambda closure/call identities, captureless/reference return, value/reference/initialized capture actions once, native/source-hidden cross-TU C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
