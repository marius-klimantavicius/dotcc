#!/usr/bin/env python3
"""EDG-selected constexpr branches retain runtime initializer cleanup only."""
import argparse
from pathlib import Path
import re
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone()) == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        selections=db.execute("SELECT id,constant_text,function_id,file_id,line,column FROM nodes WHERE kind='constexpr-if' ORDER BY id").fetchall()
        if path.stem=='main':
            assert len(selections)==6 and {row['constant_text'] for row in selections}=={'true','false'}, [dict(row) for row in selections]
        wrappers=0
        for selection in selections:
            assert selection['function_id'] is not None and selection['file_id'] is not None and selection['line']>0
            branches={row['role']:row['child_id'] for row in db.execute("SELECT role,child_id FROM edges WHERE parent_id=? AND role IN ('then','else','selected-body')",(selection['id'],))}
            chosen='then' if selection['constant_text']=='true' else 'else'
            assert branches['selected-body']==branches[chosen], branches
            discarded='else' if chosen=='then' else 'then'
            if discarded in branches:
                assert db.execute('SELECT kind FROM nodes WHERE id=?',(branches[discarded],)).fetchone()[0]=='empty', branches
            wrapper=db.execute("SELECT n.id,n.kind,n.domain,n.function_id,n.lifetime_id FROM edges e JOIN nodes n ON n.id=e.child_id WHERE e.parent_id=? AND e.role='condition'",(selection['id'],)).fetchone()
            owner=selection['id']
            if wrapper:
                wrappers+=1;owner=wrapper['id']
                assert wrapper['kind']=='condition' and wrapper['domain']=='program' and wrapper['function_id']==selection['function_id']
                assert wrapper['lifetime_id'] is not None
                assert db.execute("SELECT 1 FROM edges WHERE parent_id=? AND role='initializer'",(owner,)).fetchone()
                assert not db.execute("SELECT 1 FROM edges WHERE parent_id=? AND role='value'",(owner,)).fetchone(), 'Runtime predicate edge retained'
            predicate=db.execute("""
                SELECT n.id,n.domain,n.function_id,n.kind,n.operator,n.constant_text,n.file_id,n.line
                FROM edges e JOIN nodes n ON n.id=e.child_id
                WHERE e.parent_id=? AND e.role='source-condition' AND e.ordinal=0
            """,(owner,)).fetchone()
            assert predicate and predicate['domain']=='template-recipe' and predicate['function_id'] is None
            assert predicate['file_id'] is not None and predicate['line']>0
            if predicate['kind']=='constant':
                assert predicate['constant_text']==('1' if selection['constant_text']=='true' else '0'), dict(predicate)
            else:
                assert predicate['operator']=='call', dict(predicate)
                reference=db.execute("""
                    SELECT n.domain,n.function_id,s.name FROM edges e JOIN nodes n ON n.id=e.child_id
                    JOIN symbols s ON s.id=n.symbol_id WHERE e.parent_id=? AND e.role='operand' AND e.ordinal=0
                """,(predicate['id'],)).fetchone()
                assert tuple(reference)==('template-recipe',None,'predicate'), tuple(reference)
        if path.stem=='main':assert wrappers==4, wrappers
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='template-recipe' AND function_id IS NOT NULL").fetchone()
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{row["name"]}: {db.execute("SELECT count(*) FROM " + row["name"]).fetchone()[0]} rows' for row in tables]
        output += ['', 'selected constexpr branches', *(str(dict(row)) for row in selections)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/constexpr_if',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    unsupported=work/'unsupported.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source/'unsupported_discarded.cpp','--output',unsupported,
         '--std','c++17','--work-dir',work/'unsupported-work','--compiler',args.compiler.resolve()],work/'unsupported-compile.log',expected=1)
    assert 'constexpr-if nonempty discarded source branch navigation' in (work/'unsupported-work/stderr.txt').read_text()
    assert not unsupported.exists(), 'Unsupported discarded source branch must not publish an object'
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.ConstexprIf',
         '--class','Core','--entry','main',*objects],work/'link.log')
    code='\n'.join(path.read_text() for path in generated.glob('*.cs'))
    predicate_uses=re.findall(r'\bpredicate\s*\(',code)
    assert len(predicate_uses)==1 and re.search(r'public static bool predicate\s*\(',code), 'Source constexpr predicate emitted a runtime invocation'
    assert 'runtime_predicate' not in code, 'Unevaluated source call entered executable closure'
    assert 'if constexpr' in code, 'Missing selected source branch comments'
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: EDG-selected constexpr branches, navigation-only predicate and initializer success/throw cleanup, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
