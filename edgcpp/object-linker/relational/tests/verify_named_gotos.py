#!/usr/bin/env python3
"""Selected lexical labels and forward source jumps preserve recorded cleanup."""
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
        columns={row['name'] for row in db.execute("PRAGMA table_info('nodes')")}
        assert {'common_lifetime_id','containing_lifetime_id'}<=columns
        jumps=db.execute('''SELECT n.id,n.function_id,n.lifetime_id,n.common_lifetime_id,
            n.containing_lifetime_id,n.file_id,n.line,n.column,e.ordinal,label.id AS label_id,
            label.kind AS label_kind,label.function_id AS label_function,label.constant_text AS label_name,
            label.lifetime_id AS label_owned_lifetime,label.common_lifetime_id AS label_common_lifetime,
            label.containing_lifetime_id AS label_containing_lifetime,s.name AS function_name,
            f.body_node_id,l.owner_node_id AS common_owner
            FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='target'
            JOIN nodes label ON label.id=e.child_id JOIN symbols s ON s.id=n.function_id
            JOIN functions f ON f.symbol_id=n.function_id LEFT JOIN lifetimes l ON l.id=n.common_lifetime_id
            WHERE n.kind='goto' ORDER BY n.id''').fetchall()
        for jump in jumps:
            assert jump['ordinal']==0 and jump['label_kind']=='label', dict(jump)
            assert jump['function_id']==jump['label_function'], dict(jump)
            assert jump['lifetime_id'] is None and jump['containing_lifetime_id'] is None, dict(jump)
            assert jump['label_owned_lifetime'] is None and jump['label_common_lifetime'] is None, dict(jump)
            assert jump['file_id'] is not None and jump['line']>0 and jump['column']>0
            lexical=db.execute('''SELECT e.parent_id,e.ordinal FROM edges e JOIN nodes n ON n.id=e.parent_id
                WHERE e.child_id=? AND e.role='statement' AND n.kind='block' ''',(jump['label_id'],)).fetchall()
            assert len(lexical)==1, [dict(row) for row in lexical]
            assert db.execute("SELECT count(*) FROM nodes WHERE kind='label' AND function_id=? AND constant_text=?",
                              (jump['function_id'],jump['label_name'])).fetchone()[0]==1, dict(jump)
        if path.stem=='provider':
            assert {(row['function_name'],row['label_name']) for row in jumps}=={('lookup','__done'),('same_scope','finished')}, [dict(row) for row in jumps]
            for jump in jumps:
                assert jump['common_lifetime_id'] is not None, dict(jump)
                assert jump['common_lifetime_id']==jump['label_containing_lifetime'], dict(jump)
                assert jump['common_owner']==jump['body_node_id'], 'Control reference changed cleanup ownership: '+str(dict(jump))
                assert db.execute('SELECT 1 FROM lifetime_destructions WHERE lifetime_id=?',
                                  (jump['common_lifetime_id'],)).fetchone(), dict(jump)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', '', 'selected goto/label aliases',
                          *(str(dict(row)) for row in jumps)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/named_gotos',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects={}
    for name in ('provider','main','unsupported_target_scope'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects[name]=obj
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(path) for path in objects.values())+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',objects['provider'],objects['main']],work/'link.log')
    code=(generated/'Translated.cs').read_text()
    assert 'goto __done;' in code and '__done: ;' in code
    assert 'goto finished;' in code and 'finished: ;' in code and 'finally' in code
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    log=work/'unsupported-target-link.log'
    run(['dotnet',args.linker.resolve(),'link','--out',work/'unsupported-target-generated',
         objects['unsupported_target_scope']],log,expected=1)
    assert 'named goto emission' in log.read_text(), log.read_text()
    print('PASS: exact label aliases/control lifetimes, loop/if scope-exit cleanup, skipped construction/failure, native/source-hidden cross-TU C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
