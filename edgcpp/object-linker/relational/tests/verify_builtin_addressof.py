#!/usr/bin/env python3
"""Selected builtin addresses bypass overloads and preserve source storage."""
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
        addresses=db.execute("""
            SELECT n.id,n.kind,n.domain,n.function_id,n.type_id,n.value_category,n.operator,
                   n.symbol_id,n.file_id,n.line,n.column,s.name AS function_name,
                   meta.id AS name_node,meta.domain AS name_domain,meta.function_id AS name_function,
                   meta.type_id AS name_type,meta.value_category AS name_category,meta.source_node_id
            FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='source-name' AND e.ordinal=0
            JOIN nodes meta ON meta.id=e.child_id LEFT JOIN symbols s ON s.id=n.function_id
            WHERE meta.kind='reference-name' AND meta.constant_text='__builtin_addressof' ORDER BY n.id
        """).fetchall()
        assert addresses, 'Missing selected builtin address operation'
        for address in addresses:
            assert (address['kind'],address['operator'],address['domain'],address['value_category'])==('operation','address-of','program','prvalue'), dict(address)
            assert address['function_id'] is not None and address['symbol_id'] is None
            assert address['file_id'] is not None and address['line']>0 and address['column']>0
            assert address['name_domain']=='template-recipe' and address['name_function'] is None
            assert address['name_type'] is None and address['name_category'] is None and address['source_node_id']==address['id']
            operands=db.execute("""
                SELECT e.ordinal,n.id,n.kind,n.operator,n.type_id,n.value_category
                FROM edges e JOIN nodes n ON n.id=e.child_id WHERE e.parent_id=? AND e.role='operand'
            """,(address['id'],)).fetchall()
            assert len(operands)==1 and operands[0]['ordinal']==0 and operands[0]['value_category']=='lvalue', [dict(row) for row in operands]
            result=db.execute('SELECT kind,pointee_type_id FROM types WHERE id=?',(address['type_id'],)).fetchone()
            assert result['kind']=='pointer' and result['pointee_type_id']==operands[0]['type_id'], (dict(address),dict(result),dict(operands[0]))
            if path.stem=='provider' and address['function_name'] in ('record_address','scalar_address'):
                assert operands[0]['operator']=='ref-indirect', dict(operands[0])
                assert db.execute("SELECT 1 FROM edges e JOIN nodes n ON n.id=e.child_id WHERE e.parent_id=? AND e.role='operand' AND n.operator='call'",(operands[0]['id'],)).fetchone(), dict(operands[0])
        if path.stem=='provider':
            names={row['function_name'] for row in addresses}
            assert {'record_address','field_address','scalar_address','array_element_address'}<=names, [dict(row) for row in addresses]
            assert names<={'record_address','field_address','scalar_address','array_element_address','static_address'}, [dict(row) for row in addresses]
        assert not db.execute("SELECT 1 FROM symbols WHERE name='__builtin_addressof'").fetchone(), 'Builtin address acquired an overload/callable provider'
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='template-recipe' AND function_id IS NOT NULL").fetchone()
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{row["name"]}: {db.execute("SELECT count(*) FROM " + row["name"]).fetchone()[0]} rows' for row in tables]
        output += ['', 'selected builtin addresses', *(str(dict(row)) for row in addresses)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/builtin_addressof',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*objects],work/'link.log')
    code='\n'.join(path.read_text() for path in generated.glob('*.cs'))
    assert '__builtin_addressof' not in code, 'Builtin address emitted a callable ABI/provider'
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: builtin overload bypass, selected references once, scalar/field/array/static/local storage, native/source-hidden cross-TU C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
