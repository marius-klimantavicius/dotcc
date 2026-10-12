#!/usr/bin/env python3
"""EDG-folded void literals carry their selected type and no scalar payload."""
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
        main=db.execute("SELECT * FROM symbols WHERE name='main'").fetchone()
        assert main and main['is_definition'] and main['is_emission_root']==1
        values=db.execute("SELECT n.*,t.kind AS selected_type FROM nodes n JOIN types t ON t.id=n.type_id "
            "WHERE n.kind='void-constant' AND n.function_id=? ORDER BY n.id",(main['id'],)).fetchall()
        assert values
        for value in values:
            assert value['selected_type']=='void' and value['value_category']=='prvalue'
            assert value['constant_text'] is None and value['floating_value_id'] is None
            assert value['literal_object_id'] is None and value['symbol_id'] is None
            assert value['domain']=='program' and value['file_id'] is not None
            assert value['line'] and value['column']
            assert db.execute("SELECT 1 FROM edges e JOIN nodes s ON s.id=e.parent_id "
                "WHERE e.child_id=? AND e.role='value' AND s.kind='expr-stmt'",(value['id'],)).fetchone()
        folded=db.execute("SELECT n.* FROM nodes n WHERE n.function_id=? AND n.kind='void-constant' AND n.line=6",
            (main['id'],)).fetchone()
        assert folded and folded['column']==3
        helper=db.execute("SELECT s.*,f.body_node_id FROM symbols s JOIN functions f ON f.symbol_id=s.id "
            "WHERE s.name='no_value'").fetchone()
        assert helper and helper['is_definition'] and helper['is_emission_root']==0
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',str(dict(helper)),
                         *(str(dict(row)) for row in values)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/void_constants.cpp',source)
    compiler=args.compiler.resolve()
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(p.read_bytes()).hexdigest()+'  '+str(p) for p in
        (compiler,source,RELATIONAL/'compile.py',Path(__file__).resolve()))+'\n')
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'void-constants.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--std','c++17','--output',obj,'--work-dir',work/'compile-work',
         '--compiler',compiler],work/'compile.log')
    source.rename(work/'hidden-source.cpp')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    print('PASS: native oracle and source-hidden typed void constants have no invented scalar value')
    print(work)


if __name__=='__main__':main()
