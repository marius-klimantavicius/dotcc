#!/usr/bin/env python3
"""EDG-selected static assertion values remain browsable source metadata."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone()) == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        assertions=db.execute("""
            SELECT n.id,n.function_id,n.domain,n.type_id,n.value_category,n.symbol_id,
                   n.file_id,n.line,n.column,s.name AS function_name
            FROM nodes n JOIN symbols s ON s.id=n.function_id
            WHERE n.kind='static-assertion' ORDER BY n.id
        """).fetchall()
        assert len(assertions)==5, [dict(row) for row in assertions]
        assert {row['function_name'] for row in assertions}=={'main','selected_width'}
        messages=[];terse=0
        for assertion in assertions:
            assert assertion['domain']=='program' and assertion['function_id'] is not None
            assert assertion['type_id'] is None and assertion['value_category'] is None and assertion['symbol_id'] is None
            assert assertion['file_id'] is not None and assertion['line']>0 and assertion['column']>0
            assert db.execute("""
                SELECT count(*) FROM edges e JOIN nodes parent ON parent.id=e.parent_id
                WHERE e.child_id=? AND e.role='declaration' AND parent.kind='decl'
            """,(assertion['id'],)).fetchone()[0]==1, dict(assertion)
            children=db.execute("""
                SELECT e.role,e.ordinal,n.id,n.kind,n.domain,n.function_id,n.constant_text,
                       n.literal_object_id,n.file_id,n.line,n.column
                FROM edges e JOIN nodes n ON n.id=e.child_id
                WHERE e.parent_id=? ORDER BY e.role,e.ordinal
            """,(assertion['id'],)).fetchall()
            assert [row['role'] for row in children] in (['condition'],['condition','message'])
            for child in children:
                assert child['ordinal']==0 and child['domain']=='template-recipe' and child['function_id'] is None
                assert (child['file_id'],child['line'],child['column'])==(assertion['file_id'],assertion['line'],assertion['column'])
            condition=children[0]
            assert condition['kind']=='constant' and condition['constant_text']=='1', dict(condition)
            if len(children)==1:
                terse+=1
            else:
                message=children[1]
                assert message['kind']=='string-literal' and message['literal_object_id'] is not None, dict(message)
                literal=message['literal_object_id']
                size=db.execute('SELECT byte_count FROM literal_objects WHERE id=?',(literal,)).fetchone()[0]
                units=db.execute('SELECT byte_offset,value FROM literal_bytes WHERE literal_id=? ORDER BY byte_offset',(literal,)).fetchall()
                assert [row['byte_offset'] for row in units]==list(range(size))
                payload=bytes(row['value'] for row in units)
                assert payload.endswith(b'\0')
                messages.append(payload[:-1].decode('utf-8'))
                assert not db.execute('SELECT 1 FROM literal_initializers WHERE node_id=?',(message['id'],)).fetchone()
        assert terse==2 and set(messages)=={'templated_selected','local_selected',''}, (terse,messages)
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='program' AND literal_object_id IS NOT NULL").fetchone(), 'Assertion message entered executable graph'
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='template-recipe' AND function_id IS NOT NULL").fetchone()
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{row["name"]}: {db.execute("SELECT count(*) FROM " + row["name"]).fetchone()[0]} rows' for row in tables]
        output += ['', 'selected static assertion declarations', *(str(dict(row)) for row in assertions), f'messages: {messages}']
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/static_assertions.cpp',source)
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'static-assertions.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--output',obj,'--std','c++17',
         '--work-dir',work/'compile-work','--compiler',args.compiler.resolve()],work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    source.rename(work/'original-source.cpp');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',obj],work/'link.log')
    assert 'global nodes 0' in (work/'link.log').read_text(), 'Assertion metadata entered executable global storage'
    code='\n'.join(path.read_text() for path in generated.glob('*.cs'))
    assert code.count('// static_assert (accepted by EDG).')==5, 'Missing source assertion comments'
    assert '__literal_' not in code and 'AllocateArray<byte>' not in code, 'Assertion message allocated runtime literal storage'
    assert 'templated_selected' not in code and 'local_selected' not in code, 'Assertion message emitted as runtime expression'
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected static assertions/optional messages, source metadata and source-hidden native/C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
