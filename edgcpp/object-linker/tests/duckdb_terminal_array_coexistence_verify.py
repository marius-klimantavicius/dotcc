#!/usr/bin/env python3
"""Check terminal elision coexists only with owned array construction rollback."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('object', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    original = out / 'original.dotobj'; shutil.copy2(args.object, original)
    spec = importlib.util.spec_from_file_location('packager', args.packager)
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks = []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def verify(name, obj, succeeds):
        result = subprocess.run(['dotnet', str(args.linker.resolve()), 'verify', str(obj)],
                                capture_output=True, timeout=120)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        check(name, (result.returncode == 0) == succeeds)

    error = None
    try:
        with sqlite3.connect(original) as db:
            rows = list(db.execute('''SELECT e.source_obligation_id,e.function_entity_id,a.node_id
                FROM destruction_elisions e JOIN array_runtime_operations a
                  ON a.source_obligation_id=e.source_obligation_id
                JOIN nodes n ON n.id=a.node_id AND n.function_id=e.function_entity_id
                WHERE e.kind='terminal-no-throw' AND a.operation_kind='construct' '''))
            check('exact-construction-and-terminal-elision', bool(rows))
        verify('original-verified', original, True)
        for name, sql, values in (
            ('reject-destruction-coexistence',
             "UPDATE array_runtime_operations SET operation_kind='destroy' WHERE node_id=?", (rows[0][2],)),
            ('reject-no-effect-construction-coexistence',
             "UPDATE destruction_elisions SET kind='stock-no-effect',capture_proof='observed-no-effect-destruction-removal' WHERE source_obligation_id=? AND function_entity_id=?",
             rows[0][:2]),
        ):
            damaged = out / (name + '.dotobj'); shutil.copy2(original, damaged)
            with sqlite3.connect(damaged) as db:
                check(name + '-changed', db.execute(sql, values).rowcount == 1)
                check(name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            verify(name, damaged, False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
