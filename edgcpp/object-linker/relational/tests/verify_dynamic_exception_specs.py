#!/usr/bin/env python3
"""Selected legacy exception policies/lists without invented runtime boundaries."""
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
        assert db.execute('PRAGMA user_version').fetchone()[0]==1022
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert len(tables)==44, len(tables)
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        types={row['id']:row for row in db.execute('SELECT * FROM types')}

        def plain(identity):
            selected=types[identity]
            while selected['kind'] in ('typedef','qualified'):
                selected=types[selected['underlying_type_id']]
            return selected

        def selected_specification(name,policy,allowed,defined=False):
            symbol=db.execute('SELECT * FROM symbols WHERE name=?',(name,)).fetchone()
            assert symbol and bool(symbol['is_definition'])==defined, name
            assert symbol['linkage']=='external' and symbol['link_name'], dict(symbol)
            assert symbol['file_id'] is not None and symbol['line']>0 and symbol['column']>0, dict(symbol)
            signature=plain(symbol['type_id'])
            assert signature['kind']=='function' and signature['exception_policy']==policy, dict(signature)
            selected=db.execute('''SELECT ordinal,allowed_type_id FROM exception_specification_types
                WHERE function_type_id=? ORDER BY ordinal''',(signature['id'],)).fetchall()
            assert [row['ordinal'] for row in selected]==list(range(len(allowed))), [dict(row) for row in selected]
            assert [plain(row['allowed_type_id'])['builtin_name'] for row in selected]==allowed
            body=db.execute('SELECT exception_policy FROM functions WHERE symbol_id=?',(symbol['id'],)).fetchone()
            if defined:
                assert body and body['exception_policy']==policy, body
            else:
                assert body is None, body
            return f'{name}: {policy}, allowed={allowed}, definition={defined}, link={symbol["link_name"]}'

        if path.stem=='declarations':
            rows=[selected_specification('imported_empty','dynamic',[]),
                  selected_specification('imported_types','dynamic',['int','float']),
                  selected_specification('terminate_decl','terminate',[]),
                  selected_specification('unwind_decl','unwind',[]),
                  selected_specification('false_noexcept_decl','unwind',[])]
            for name,member_kind in (('data_member_decl','int'),('function_member_decl','function')):
                symbol=db.execute('SELECT * FROM symbols WHERE name=?',(name,)).fetchone()
                assert symbol and not symbol['is_definition'], name
                signature=plain(symbol['type_id'])
                parameter=db.execute('SELECT parameter_type_id FROM type_parameters WHERE type_id=? AND ordinal=0',
                                     (signature['id'],)).fetchone()
                member=plain(parameter['parameter_type_id'])
                assert member['kind']=='member-pointer' and member['size']>0 and member['alignment']>0, dict(member)
                source_owner=types[member['object_type_id']]
                assert source_owner['kind']=='typedef' and source_owner['name']=='MemberAlias', dict(source_owner)
                owner=plain(member['object_type_id'])
                assert owner['kind']=='record' and owner['name']=='MemberOwner', dict(owner)
                selected_member=plain(member['pointee_type_id'])
                assert (selected_member['builtin_name']=='int' if member_kind=='int'
                        else selected_member['kind']=='function'), dict(selected_member)
                assert not db.execute('SELECT 1 FROM functions WHERE symbol_id=?',(symbol['id'],)).fetchone()
                rows.append(f'{name}: member-pointer owner={owner["name"]}, member={member_kind}, '
                            f'size={member["size"]}, alignment={member["alignment"]}')
        else:
            rows=[selected_specification('dynamic_body','dynamic',['int','float'],defined=True)]
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',*rows])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/dynamic_exception_specs',source)
    objects=[]
    for name in ('declarations','body'):
        native=work/(name+'-native')
        run(['g++','-std=c++14',source/(name+'.cpp'),'-o',native],work/(name+'-native-build.log'))
        run([native],work/(name+'-native-run.log'))
        obj=work/(name+'.dotobj')
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--output',obj,'--std','c++14',
             '--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve()],work/(name+'-compile.log'))
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(path) for path in objects)+'\n')
    source.rename(work/'original-source')
    generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',objects[0]],work/'declarations-link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    run(['dotnet',args.linker.resolve(),'link','--out',work/'body-generated','--entry','main',objects[1]],
        work/'body-link.log',expected=1)
    diagnostic=(work/'body-link.log').read_text()
    assert 'dynamic exception specification runtime boundary emission is not yet implemented' in diagnostic, diagnostic
    assert 'body.cpp:' in diagnostic and 'exporter defect' not in diagnostic, diagnostic
    print('PASS: exact dynamic/noexcept/unwind policies and ordered allowed types, external declarations without providers, '
          'native/source-hidden C#'+(', NativeAOT' if args.nativeaot else '')+'; explicit unsupported dynamic body boundary')
    print(work)


if __name__=='__main__':main()
