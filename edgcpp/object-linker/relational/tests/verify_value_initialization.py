#!/usr/bin/env python3
"""Exact selected constructor value-initialization zeroing decisions."""
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
        assert db.execute('PRAGMA user_version').fetchone()[0]==1028
        actions=db.execute('SELECT i.*,t.name AS target_name,s.name AS variable_name '
            'FROM initializations i LEFT JOIN types t ON t.id=i.type_id '
            'LEFT JOIN symbols s ON s.id=i.variable_symbol_id ORDER BY i.id').fetchall()
        locals_by_name={row['variable_name']:row for row in actions if row['variable_name']}
        for name, target, value_init, zeroing in (
                ('user_provided','NoZero',1,0), ('ordinary','NoZero',0,None),
                ('implicit','NeedsZero',1,1)):
            action=locals_by_name[name]
            assert action['kind']=='constructor' and action['target_name']==target, dict(action)
            assert action['constructor_symbol_id'] is not None
            assert action['value_initialization']==value_init, dict(action)
            assert action['value_initialization_needs_zeroing']==zeroing, dict(action)
        copied=db.execute('SELECT i.* FROM constructor_initializers c '
            'JOIN fields f ON f.id=c.field_id JOIN initializations i ON i.id=c.initialization_id '
            'JOIN types t ON t.id=i.type_id WHERE f.name=? AND t.name=?',('value','NeedsZero')).fetchall()
        assert copied and all(row['kind']=='constructor' and row['value_initialization']==1 and
            row['value_initialization_needs_zeroing']==1 for row in copied), [dict(row) for row in copied]
        for action in actions:
            if action['kind']!='constructor' or not action['value_initialization']:
                assert action['value_initialization_needs_zeroing'] is None, dict(action)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes',
                          *(str(dict(row)) for row in actions)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/value_initialization.cpp',source)
    compiler=args.compiler.resolve()
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(path.read_bytes()).hexdigest()+'  '+str(path) for path in
        (compiler,source,RELATIONAL/'compile.py',Path(__file__).resolve()))+'\n')
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'value-initialization.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--std','c++17','--output',obj,'--work-dir',work/'compile-work',
         '--compiler',compiler],work/'compile.log')
    source.rename(work/'hidden-source.cpp')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    print('PASS: native values and source-hidden selected zeroing true/false/NULL, including copied DMI')
    print(work)


if __name__=='__main__':main()
