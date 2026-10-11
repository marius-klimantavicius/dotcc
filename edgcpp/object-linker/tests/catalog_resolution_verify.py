#!/usr/bin/env python3
"""Compare disk catalog selection with resident linking of the same input target."""
import argparse
import json
import os
from pathlib import Path
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--driver', type=Path, required=True)
    parser.add_argument('--inputs', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--resident', type=Path, help='Existing successful resident generation for these exact inputs')
    parser.add_argument('--expect-rejection', action='store_true')
    args = parser.parse_args()
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    inputs = json.loads(args.inputs.read_text())
    checks, commands = [], []
    error = None

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command):
        env = dict(os.environ, DOTNET_GCHeapHardLimit='0x80000000')
        result = subprocess.run(command, capture_output=True, timeout=1800, env=env)
        (output / (name + '.stdout')).write_bytes(result.stdout)
        (output / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        return result

    try:
        check('nonempty-target', bool(inputs))
        operands = []
        for item in inputs:
            item['path'] = str(Path(item['path']).resolve())
            if item['provider']:
                operands.append('--provider')
            operands.append(item['path'])
        catalog = output / 'selection.sqlite'
        selected = run('catalog', ['dotnet', str(args.driver.resolve()), 'catalog', '--resolve', '--allow-externs', '--audit-object-lifetimes', '--out', str(catalog), *operands])
        resident = args.resident.resolve() if args.resident else output / 'resident'
        generated = None if args.resident else run('resident', ['dotnet', str(args.driver.resolve()), 'link', '--allow-externs', '--out', str(resident), *operands])
        if args.expect_rejection:
            check('both-paths-reject', selected.returncode != 0 and generated is not None and generated.returncode != 0)
            with sqlite3.connect(catalog) as db:
                check('failed-resolution-not-published', db.execute('SELECT status FROM catalog_info').fetchone() == ('imported-not-resolved',))
                check('failed-resolution-transaction-rolled-back', not db.execute("SELECT name FROM sqlite_schema WHERE name='winners'").fetchall())
        else:
            check('catalog-selection-succeeds', selected.returncode == 0)
            check('resident-generation-succeeds', generated is None or generated.returncode == 0)
            report = json.loads((resident / 'link-report.json').read_text())
            selection = json.loads((resident / 'link-selection.json').read_text())
            check('same-input-receipts', {item['path'] for item in report['objects']} == {item['path'] for item in inputs})
            with sqlite3.connect(catalog) as db:
                check('explicit-not-generated-status', db.execute('SELECT status FROM catalog_info').fetchone() == ('selected-not-generated',))
                check('catalog-integrity', db.execute('PRAGMA integrity_check').fetchone() == ('ok',))
                check('catalog-references', not db.execute('PRAGMA foreign_key_check').fetchall())
                actual = set(db.execute("SELECT u.unit_id,w.local_id FROM winners w JOIN units u ON u.id=w.unit_id WHERE w.kind='function' AND w.selected=1"))
                expected = {(item['unitId'], item['entityId']) for item in report['symbols']}
                check('same-selected-function-locators', actual == expected)
                check('same-selected-storage-count', db.execute("SELECT count(*) FROM winners WHERE kind='storage' AND selected=1").fetchone()[0] == selection['selectedStorages'])
                actual = set(db.execute('SELECT u.unit_id,r.local_id FROM selected_records r JOIN units u ON u.id=r.unit_id'))
                expected = {(item['unitId'], item['typeId']) for item in report['records']}
                check('same-selected-record-locators', actual == expected)
                actual = {row[0] for row in db.execute('SELECT u.unit_id FROM selected_units s JOIN units u ON u.id=s.unit_id')}
                expected = {item['unitId'] for item in selection['objects'] if item['selected']}
                check('same-provider-extraction', actual == expected)
                # Name parity is available in builds with disk-backed allocation.
                if db.execute("SELECT name FROM sqlite_schema WHERE name='names'").fetchone():
                    actual = set(db.execute("SELECT u.unit_id,w.local_id,n.name FROM winners w JOIN units u ON u.id=w.unit_id JOIN names n ON n.kind=w.kind AND n.symbol_key=w.symbol_key WHERE w.kind='function' AND w.selected=1"))
                    expected = {(item['unitId'], item['entityId'], item['generatedName']) for item in report['symbols']}
                    check('same-generated-function-names', actual == expected)
                check('all-unit-graphs-released', json.loads(selected.stdout.splitlines()[-2])['auditedReleasedObjects'] == len(inputs))
    except Exception as exc:
        error = repr(exc)
    (output / 'results.json').write_text(json.dumps(dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands), indent=2) + '\n')
    if error:
        raise RuntimeError(error)
    print(f'PASS {len(checks)} catalog resolution checks')


if __name__ == '__main__':
    main()
