#!/usr/bin/env python3
"""Exercise the actual native SQLite adapter's transactional reference checks."""
import argparse
from pathlib import Path
import sqlite3
import subprocess
import time


RELATIONAL = Path(__file__).resolve().parents[1]
EDG = RELATIONAL.parents[1]

SOURCE = r'''
#include "dot_gen_semantic_sqlite.h"
#include <cstring>
int main(int argc, char **argv) {
  if(argc!=3)return 2;
  dot_semantic_sqlite::Database db(argv[1]);
  const char *mode=argv[2];
  if(!std::strcmp(mode,"valid")) {
    // Forward references followed by a cycle; IDs need not arrive in order.
    for(int id=12000;id>0;--id)
      db.insert("types",{{"id",id},{"kind","pointer"},{"pointee_type_id",id==1?12000:id-1}});
    db.insert("type_parameters",{{"type_id",1},{"ordinal",0},{"parameter_type_id",12000}});
  } else if(!std::strcmp(mode,"missing-parent")) {
    db.insert("types",{{"id",1},{"kind","pointer"},{"pointee_type_id",2}});
  } else if(!std::strcmp(mode,"without-rowid")) {
    db.insert("type_parameters",{{"type_id",1},{"ordinal",0},{"parameter_type_id",2}});
  } else if(!std::strcmp(mode,"updated-parent")) {
    db.insert("types",{{"id",1},{"kind","i32"}});
    db.update("types","id",1,{{"pointee_type_id",99}});
  } else if(!std::strcmp(mode,"check-constraint")) {
    db.insert("types",{{"id",1},{"kind","i32"},{"is_const",2}});
  } else if(!std::strcmp(mode,"unique-constraint")) {
    db.insert("types",{{"id",1},{"kind","i32"}});
    db.insert("types",{{"id",1},{"kind","i64"}});
  } else if(!std::strcmp(mode,"unfinished")) {
    db.insert("types",{{"id",1},{"kind","i32"}});
    return 0;
  } else return 2;
  db.finish();
}
'''


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    args = parser.parse_args()
    out = args.out.resolve()
    out.mkdir(parents=True, exist_ok=False)
    schema = (RELATIONAL / 'schema.sql').read_text()
    (out / 'dot_gen_semantic_schema.h').write_text(
        '#pragma once\nstatic const char dot_semantic_schema[] = R"DOTSCHEMA(' + schema + ')DOTSCHEMA";\n')
    source = out / 'writer.cpp'
    source.write_text(SOURCE)
    build = subprocess.run(['g++', '-std=c++17', '-O2', '-I', str(EDG / 'backend'),
                            '-I', str(out), str(source), '-lsqlite3', '-o', str(out / 'writer')],
                           text=True, capture_output=True)
    (out / 'build.log').write_text(build.stdout + build.stderr)
    build.check_returncode()
    summaries = []
    for mode in ('valid', 'missing-parent', 'without-rowid', 'updated-parent',
                 'check-constraint', 'unique-constraint', 'unfinished'):
        path = out / (mode + '.dotobj')
        start = time.perf_counter()
        result = subprocess.run([out / 'writer', path, mode], text=True, capture_output=True)
        elapsed = time.perf_counter() - start
        (out / (mode + '.log')).write_text(result.stdout + result.stderr)
        expected = 0 if mode in ('valid', 'unfinished') else 1
        assert result.returncode == expected, (mode, result.returncode, result.stderr)
        with sqlite3.connect(path) as db:
            if mode == 'valid':
                assert db.execute('SELECT count(*) FROM types').fetchone() == (12000,)
                assert not db.execute('PRAGMA foreign_key_check').fetchall()
                assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
            else:
                assert not db.execute("SELECT name FROM sqlite_master WHERE type='table'").fetchall(), mode
        if mode in ('missing-parent', 'without-rowid', 'updated-parent'):
            assert 'unresolved foreign key:' in result.stderr, result.stderr
        summaries.append(f'{mode}: PASS ({elapsed:.4f} s)')
    (out / 'result.txt').write_text('\n'.join(summaries) + '\n')
    print('\n'.join(summaries))
    print(out)


if __name__ == '__main__':
    main()
