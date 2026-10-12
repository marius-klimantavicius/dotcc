#!/usr/bin/env python3
"""Selected static-member operations retain evaluated receivers and exact members."""
import argparse
import hashlib
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro',uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone())==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows=db.execute('''SELECT n.*,s.name AS owner FROM nodes n JOIN symbols s ON s.id=n.function_id
            WHERE n.operator IN ('dot-static','points-to-static') ORDER BY n.id''').fetchall()
        assert {row['operator'] for row in rows}=={'dot-static','points-to-static'}
        assert {'pointer_call','reference_call','pointer_value','reference_value','assign_value'} <= {row['owner'] for row in rows}
        for row in rows:
            assert row['kind']=='operation' and row['domain']=='program' and row['type_id'] is not None
            assert row['file_id'] is not None and row['line']>0 and row['column']>0
            operands=db.execute('''SELECT n.*,e.ordinal FROM edges e JOIN nodes n ON n.id=e.child_id
                WHERE e.parent_id=? AND e.role='operand' ORDER BY e.ordinal''',(row['id'],)).fetchall()
            assert len(operands)==2 and [operand['ordinal'] for operand in operands]==[0,1]
            assert operands[0]['type_id'] is not None
            assert db.execute('''WITH RECURSIVE receiver(id) AS (SELECT ? UNION
                SELECT e.child_id FROM edges e JOIN receiver r ON r.id=e.parent_id WHERE e.role='operand')
                SELECT 1 FROM receiver r JOIN nodes n ON n.id=r.id WHERE n.operator='call' ''',
                (operands[0]['id'],)).fetchone()
            selected=operands[1]
            assert selected['symbol_id'] is not None and selected['kind'] in ('variable','function')
            symbol=db.execute('SELECT * FROM symbols WHERE id=?',(selected['symbol_id'],)).fetchone()
            assert symbol['name'] in ('value','event')
            if selected['kind']=='variable':assert row['value_category']=='lvalue'
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',*(str(dict(row)) for row in rows)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    if args.nativeaot and not args.linker:parser.error('--nativeaot requires --linker')
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/static_member_selections.cpp',source)
    compiler=args.compiler.resolve()
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(path.read_bytes()).hexdigest()+'  '+str(path)
        for path in (compiler,source,RELATIONAL/'compile.py',Path(__file__).resolve()))+'\n')
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'static-members.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--std','c++17','--output',obj,
         '--work-dir',work/'compile-work','--compiler',compiler],work/'compile.log')
    source.rename(work/'hidden-source.cpp')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    if args.linker:
        generated=work/'generated'
        run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',obj],work/'link.log')
        run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
        if args.nativeaot:
            run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
                 '-o',work/'nativeaot'],work/'nativeaot-build.log')
            apps=[path for path in (work/'nativeaot').iterdir()
                  if path.is_file() and path.suffix=='' and path.stat().st_mode & 0o111]
            if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
            run(apps,work/'nativeaot-run.log')
    print('PASS: native oracle and source-hidden exact static-member operands, effect order and lvalue facts'+
          (', C#'+(', NativeAOT' if args.nativeaot else '') if args.linker else ''))
    print(work)


if __name__=='__main__':main()
