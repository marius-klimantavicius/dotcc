#!/usr/bin/env python3
"""Verify exact delayed generated exception policy provenance in a retained object."""
import argparse
import hashlib
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
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    obj = out / 'unit.dotobj'; shutil.copy2(args.object, obj)
    checks, commands, transitions = [], [], []

    def check(name, passed):
        if not passed: raise AssertionError(name)
        checks.append(name)

    def verify(name, path, success=True):
        argv = ['dotnet', str(args.linker.resolve()), 'verify', str(path)]
        result = subprocess.run(argv, cwd=out, capture_output=True, timeout=300)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=argv, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)

    error = None
    try:
        verify('verify-original', obj)
        with sqlite3.connect(obj) as db:
            db.row_factory = sqlite3.Row
            transitions = [dict(row) for row in db.execute('SELECT * FROM source_routine_exception_resolutions ORDER BY source_routine_type_id')]
            check('observed-resolutions', len(transitions) > 0)
            matches = [r for r in transitions if r['initial_present'] == 0 and r['initial_policy'] == 'unwind' and r['resolved_policy'] == 'terminate' and r['final_is_noexcept'] == 1 and r['is_defaulted'] == 1]
            check('defaulted-absent-to-noexcept', len(matches) > 0)
            row = next((r for r in matches if r['source_function_entity_id'] is not None), matches[0])
            selected = row['source_routine_type_id']
            check('semantic-final-policy', db.execute('SELECT exception_policy FROM source_routine_types WHERE type_id=?', (selected,)).fetchone()[0] == 'terminate')
            check('owned-policy-origin', db.execute('SELECT exception_policy_origin FROM source_routine_types WHERE type_id=?', (selected,)).fetchone()[0] == 'generated-resolution')
            check('exact-stock-proof', all(r['capture_proof'] == 'observed-generated-exception-specification' for r in transitions))
        mutations = {
            'missing-resolution': ('DELETE FROM source_routine_exception_resolutions WHERE source_routine_type_id=?', (selected,)),
            'missing-origin': ("UPDATE source_routine_types SET exception_policy_origin='captured' WHERE type_id=?", (selected,)),
            'invalid-proof': ("UPDATE source_routine_exception_resolutions SET capture_proof='unobserved' WHERE source_routine_type_id=?", (selected,)),
            'wrong-initial-policy': ("UPDATE source_routine_exception_resolutions SET initial_policy='terminate' WHERE source_routine_type_id=?", (selected,)),
            'absent-spec-with-flags': ('UPDATE source_routine_exception_resolutions SET initial_is_noexcept=1 WHERE source_routine_type_id=?', (selected,)),
            'unresolved-final-policy': ('UPDATE source_routine_exception_resolutions SET final_indeterminate=1 WHERE source_routine_type_id=?', (selected,)),
            'wrong-final-policy': ("UPDATE source_routine_exception_resolutions SET resolved_policy='unwind' WHERE source_routine_type_id=?", (selected,)),
            'missing-member-origin': ('UPDATE source_routine_exception_resolutions SET is_defaulted=0,is_compiler_generated=0,is_inheriting_constructor=0 WHERE source_routine_type_id=?', (selected,)),
        }
        if row['source_function_entity_id'] is not None:
            mutations['wrong-member-kind'] = ("UPDATE source_routine_exception_resolutions SET special_member_kind='destructor' WHERE source_routine_type_id=?", (selected,))
        for name, (sql, parameters) in mutations.items():
            bad = out / (name + '.dotobj'); shutil.copy2(obj, bad)
            with sqlite3.connect(bad) as db:
                db.execute('PRAGMA foreign_keys=OFF'); db.execute('PRAGMA ignore_check_constraints=ON')
                db.execute(sql, parameters)
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            verify('reject-' + name, bad, False)
    except Exception as exc:
        error = str(exc)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        transitions=transitions, objectSha256=hashlib.sha256(args.object.read_bytes()).hexdigest(),
        linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
