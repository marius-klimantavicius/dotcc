#!/usr/bin/env python3
"""Browse dependent explicit template references without executing source recipes."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory = sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone()) == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        references = db.execute('''SELECT n.id,n.domain,n.function_id,n.file_id,n.line,n.column,
            target.kind AS target_kind,r.name FROM nodes n
            JOIN edges e ON e.parent_id=n.id AND e.role='template' AND e.ordinal=0
            JOIN nodes target ON target.id=e.child_id
            JOIN template_value_references r ON r.node_id=target.id
            WHERE n.kind='dependent-template-reference' ORDER BY n.id''').fetchall()
        assert references, 'Missing explicit dependent template reference'
        for reference in references:
            assert reference['domain'] == 'template-recipe' and reference['function_id'] is None
            assert reference['target_kind'] == 'dependent-qualified-function-reference'
            assert reference['name'] == 'Recipes::choose', dict(reference)
            assert reference['file_id'] is not None and reference['line'] > 0
            arguments = db.execute('''SELECT e.ordinal,n.kind,n.domain,n.function_id,n.constant_text,
                t.kind AS type_kind,p.kind AS parameter_kind FROM edges e
                JOIN nodes n ON n.id=e.child_id LEFT JOIN types t ON t.id=n.type_id
                LEFT JOIN template_type_parameters p ON p.type_id=t.id
                WHERE e.parent_id=? AND e.role='template-argument' ORDER BY e.ordinal''',
                (reference['id'],)).fetchall()
            assert len(arguments) == 2 and [row['ordinal'] for row in arguments] == [0, 1]
            assert all(row['domain'] == 'template-recipe' and row['function_id'] is None for row in arguments)
            assert (arguments[0]['kind'],arguments[0]['type_kind'],arguments[0]['parameter_kind']) == (
                'type-reference','template-parameter','parameter')
            assert (arguments[1]['kind'],arguments[1]['constant_text']) == ('constant','3')
        assert not list(db.execute("SELECT id FROM nodes WHERE kind LIKE 'dependent-%' AND domain='program'"))
        assert not list(db.execute("SELECT id FROM nodes WHERE domain='template-recipe' AND function_id IS NOT NULL"))
        assert db.execute('''SELECT count(*) FROM template_formals f JOIN nodes n
            ON n.id=f.default_expression_id WHERE n.domain='template-recipe' ''').fetchone()[0] > 0
        tables = db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        output = [f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{row["name"]}: {db.execute("SELECT count(*) FROM " + row["name"]).fetchone()[0]} rows'
                   for row in tables]
        output += ['', 'dependent explicit template references', *(str(dict(row)) for row in references)]
        return '\n'.join(output)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    args = parser.parse_args()
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work/'source.cpp'
    shutil.copy2(RELATIONAL/'tests/dependent_template_reference.cpp', source)
    run(['g++', '-std=c++17', source, '-o', work/'native'], work/'native-build.log')
    run([work/'native'], work/'native-run.log')
    obj = work/'template-reference.dotobj'
    run([sys.executable, RELATIONAL/'compile.py', source, '--output', obj,
         '--std', 'c++17', '--work-dir', work/'compile-work', '--compiler', args.compiler.resolve()],
        work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    source.rename(work/'original-source.cpp')
    generated = work/'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', obj], work/'link.log')
    assert 'global nodes 0' in (work/'link.log').read_text(), 'Source recipes entered executable global storage'
    run(['dotnet', 'run', '--project', generated/'Translated.csproj', '-c', 'Release'], work/'managed-run.log')
    print('PASS: exact dependent template names/type/value arguments, recipe isolation and source-hidden concrete execution')
    print(work)


if __name__ == '__main__':
    main()
