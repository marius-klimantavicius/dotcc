#!/usr/bin/env python3
"""Dependent noexcept and typeid-address queries retain selected source operands."""
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
        assert db.execute('PRAGMA user_version').fetchone()[0]>=1025
        target=db.execute('SELECT * FROM object_info').fetchone()
        assert target['char_bit']==8 and target['little_endian']==1, dict(target)
        types={row['id']:row for row in db.execute('SELECT * FROM types')}

        def plain(type_id):
            value=types[type_id]
            while value['kind'] in ('qualified','typedef'):
                value=types[value['underlying_type_id']]
            return value

        defaults=db.execute('''SELECT t.name AS template_name,f.* FROM templates t JOIN template_formals f
            ON f.template_id=t.id WHERE t.name IN('NoexceptRecipe','DestructorRecipe','BracedRecipe')
            AND f.kind='constant' ORDER BY t.name,f.ordinal''').fetchall()
        assert {row['template_name'] for row in defaults}=={'NoexceptRecipe','DestructorRecipe','BracedRecipe'}, [dict(row) for row in defaults]
        roots=[]
        for default in defaults:
            assert default['default_value_text'] is None and default['default_expression_id'] is not None, dict(default)
            roots.append(('aggregate-initializer' if default['template_name']=='BracedRecipe' else 'noexcept',
                          default['default_expression_id']))
        addresses=db.execute('''SELECT e.child_id FROM symbols s JOIN nodes n ON n.symbol_id=s.id
            JOIN edges e ON e.parent_id=n.id AND e.role='initializer'
            WHERE s.name='source_typeid_address' AND s.kind='source-variable'
            AND s.is_definition=1 AND n.kind='variable-declaration' ''').fetchall()
        assert addresses, 'Selected dependent typeid address initializer missing'
        roots.extend(('typeid-address',row[0]) for row in addresses)
        queries=[]
        for expected,root in roots:
            pending=[root];seen=set();selected=[]
            while pending:
                node_id=pending.pop()
                if node_id in seen:continue
                seen.add(node_id)
                node=db.execute('SELECT * FROM nodes WHERE id=?',(node_id,)).fetchone()
                assert node['domain']=='template-recipe' and node['function_id'] is None, dict(node)
                assert node['initialization_id'] is None and node['lifetime_id'] is None, dict(node)
                if node['operator']==expected or node['kind']==expected or (expected=='typeid-address' and node['operator']=='typeid'):
                    selected.append(node)
                pending.extend(row[0] for row in db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role IN('operand','element')",
                                                            (node_id,)))
            assert selected, (expected,root,seen)
            if expected=='aggregate-initializer':continue
            for query in selected:
                assert query['kind']=='operation' and query['constant_text'] is None and query['symbol_id'] is None, dict(query)
                assert query['value_category']==('lvalue' if query['operator']=='typeid' else 'prvalue'), dict(query)
                operands=db.execute('''SELECT e.ordinal,n.* FROM edges e JOIN nodes n ON n.id=e.child_id
                    WHERE e.parent_id=? AND e.role='operand' ORDER BY e.ordinal''',(query['id'],)).fetchall()
                assert len(operands)==1 and operands[0]['ordinal']==0, [dict(row) for row in operands]
                assert operands[0]['domain']=='template-recipe' and operands[0]['function_id'] is None
                if expected=='noexcept':
                    assert plain(query['type_id'])['kind']=='bool', dict(query)
                    assert operands[0]['kind']!='type-reference', dict(operands[0])
                else:
                    assert plain(query['type_id'])['kind']==('record' if query['operator']=='typeid' else 'pointer'), dict(query)
                    assert operands[0]['kind']=='type-reference', dict(operands[0])
                    assert plain(operands[0]['type_id'])['is_dependent'], dict(operands[0])
                    assert operands[0]['source_node_id']==query['id'], dict(operands[0])
                queries.append(query)
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='program' AND operator IN('noexcept','typeid-address')").fetchone()
        assert not db.execute("SELECT 1 FROM symbols WHERE kind='rtti-object'").fetchone()
        destructors=db.execute('''SELECT n.*,r.name AS reference_name,r.qualifier_type_id
            FROM nodes n JOIN template_value_references r ON r.node_id=n.id
            WHERE n.kind='dependent-destructor-reference' ''').fetchall()
        assert destructors, 'Selected dependent destructor reference missing'
        for destructor in destructors:
            assert destructor['domain']=='template-recipe' and destructor['function_id'] is None
            assert destructor['symbol_id'] is None and destructor['qualifier_type_id'] is not None, dict(destructor)
            assert destructor['operator']=='unqualified', dict(destructor)
            assert types[destructor['qualifier_type_id']]['is_dependent'], dict(destructor)
        assert db.execute("SELECT 1 FROM nodes WHERE domain='template-recipe' AND operator='dot-static'").fetchone()
        aggregates=db.execute("SELECT * FROM nodes WHERE kind='aggregate-initializer'").fetchall()
        assert aggregates, 'Selected dependent braced aggregate missing'
        for aggregate in aggregates:
            assert aggregate['domain']=='template-recipe' and aggregate['function_id'] is None
            assert aggregate['operator']=='braces' and types[aggregate['type_id']]['is_dependent'], dict(aggregate)
            assert not db.execute('SELECT 1 FROM array_initializers WHERE node_id=?',(aggregate['id'],)).fetchone()
            assert not db.execute('SELECT 1 FROM record_initializers WHERE node_id=?',(aggregate['id'],)).fetchone()
            elements=db.execute('''SELECT e.ordinal,n.* FROM edges e JOIN nodes n ON n.id=e.child_id
                WHERE e.parent_id=? AND e.role='element' ORDER BY e.ordinal''',(aggregate['id'],)).fetchall()
            assert len(elements)==1 and elements[0]['ordinal']==0 and elements[0]['constant_text']=='1', [dict(row) for row in elements]
            assert elements[0]['domain']=='template-recipe' and elements[0]['function_id'] is None
        folded=db.execute('''SELECT n.constant_text FROM symbols s JOIN global_initializers g ON g.symbol_id=s.id
            JOIN nodes n ON n.id=g.initializer_node_id WHERE s.name='folded_noexcept' ''').fetchone()
        assert folded and folded['constant_text']=='1'
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert len(tables)==44 and not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',str(dict(target)), *(str(dict(row)) for row in queries)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/source_query_recipes.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'source-query-recipes.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--output',obj,'--work-dir',work/'compile-work',
         '--compiler',args.compiler.resolve(),'-I',EDG/'backend/runtime/include'],work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    source.rename(work/'original-source.cpp');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',obj],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: exact selected source query operands/result categories, metadata isolation, '
          'folded concrete result and native/source-hidden C#'+(', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
