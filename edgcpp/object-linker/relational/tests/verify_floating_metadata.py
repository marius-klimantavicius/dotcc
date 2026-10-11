#!/usr/bin/env python3
"""Preserve source floating type identity and explicit target format metadata."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    args = parser.parse_args()
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work/'source.cpp'
    shutil.copy2(RELATIONAL/'tests/floating_metadata.cpp', source)
    run(['g++', '-std=c++20', source, '-o', work/'native'], work/'native-build.log')
    run([work/'native'], work/'native-run.log')
    obj = work/'floating.dotobj'
    run([sys.executable, RELATIONAL/'compile.py', source, '--output', obj,
         '--work-dir', work/'compile-work', '--compiler', args.compiler.resolve()], work/'compile.log')
    with sqlite3.connect(obj.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows = list(db.execute('''SELECT t.builtin_name,t.kind,t.size,t.alignment,
            f.format,f.precision_bits,f.min_exponent,f.max_exponent FROM floating_types f
            JOIN types t ON t.id=f.type_id ORDER BY t.id'''))
        formats = {row[0]: (row[4],row[5]) for row in rows}
        assert formats['float'] == ('binary32', 24), rows
        assert formats['double'] == ('binary64', 53), rows
        assert formats['long double'] == ('x87-binary80', 64), rows
        assert formats['__float128'] == ('binary128', 113), rows
        (work/'database-summary.txt').write_text('\n'.join('\t'.join(map(str,row)) for row in rows)+'\n')
    source.rename(work/'original-source.cpp')
    generated = work/'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', obj], work/'link.log')
    run(['dotnet', 'run', '--project', generated/'Translated.csproj', '-c', 'Release'], work/'managed-run.log')
    print('PASS: floating source/format metadata and source-hidden folded sizeof; no floating arithmetic claimed')
    print(work)


if __name__ == '__main__':
    main()
