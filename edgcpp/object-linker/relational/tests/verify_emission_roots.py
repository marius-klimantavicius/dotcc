#!/usr/bin/env python3
"""Final native roots retain unused source definitions and selected startup."""
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
        assert not db.execute('SELECT 1 FROM symbols WHERE is_emission_root IS NULL').fetchone()
        expected={'active_helper':1,'dormant_static':0,'dormant_inline':0,
                  'load_dynamic':1,'selected_entry':1,'main':1,
                  'used_data':1,'dormant_data':0,'dynamic_value':1}
        rows=[]
        for name,needed in expected.items():
            symbol=db.execute('SELECT * FROM symbols WHERE name=?',(name,)).fetchone()
            assert symbol and symbol['is_definition'] and symbol['is_emission_root']==needed, dict(symbol) if symbol else name
            rows.append(symbol)
            if symbol['kind']=='function':
                body=db.execute('SELECT body_node_id FROM functions WHERE symbol_id=?',(symbol['id'],)).fetchone()
                assert body and db.execute('SELECT 1 FROM nodes WHERE id=?',(body['body_node_id'],)).fetchone()
            else:
                assert symbol['kind']=='global'
                if name!='dynamic_value':
                    assert db.execute('SELECT 1 FROM global_initializers WHERE symbol_id=?',(symbol['id'],)).fetchone()
        groups=db.execute('SELECT g.*,s.name,s.is_definition,s.is_emission_root,f.body_node_id '
            'FROM startup_groups g JOIN symbols s ON s.id=g.function_symbol_id '
            'JOIN functions f ON f.symbol_id=s.id ORDER BY g.ordinal').fetchall()
        assert groups and all(row['is_definition'] and row['is_emission_root']==1 for row in groups)
        dynamic=next(row for row in rows if row['name']=='dynamic_value')
        assert any(db.execute("SELECT 1 FROM edges e JOIN nodes n ON n.id=e.child_id "
            "WHERE e.parent_id=? AND e.role='statement' AND n.kind='initialize' AND n.symbol_id=?",
            (group['body_node_id'],dynamic['id'])).fetchone() for group in groups)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',
                         *(str(dict(row)) for row in rows),*(str(dict(row)) for row in groups)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/emission_roots.cpp',source)
    compiler=args.compiler.resolve()
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(p.read_bytes()).hexdigest()+'  '+str(p) for p in
        (compiler,source,RELATIONAL/'compile.py',Path(__file__).resolve()))+'\n')
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'emission-roots.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--std','c++17','--output',obj,'--work-dir',work/'compile-work',
         '--compiler',compiler],work/'compile.log')
    source.rename(work/'hidden-source.cpp')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    print('PASS: native oracle and source-hidden final roots preserve all captured definitions and startup actions')
    print(work)


if __name__=='__main__':main()
