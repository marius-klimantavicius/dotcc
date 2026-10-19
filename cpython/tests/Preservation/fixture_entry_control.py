"""Native control: embedding fixtures do not depend on CLI argument parsing."""
import builtins
import importlib.util
import json
from pathlib import Path
import sys

repo = Path(__file__).resolve().parents[3]
source = repo / 'cpython/tests/fixtures/run_required.py'
artifacts = repo / 'cpython/artifacts/owner-b-r7-20261017'
work = repo / 'cpython/build/owner-b-r7-20261017/native-fixture-entry'
original = builtins.__import__

def importing(name, *args, **kwargs):
    if name == 'argparse':
        raise AssertionError('Explicit embedding entry imported the CLI parser')
    return original(name, *args, **kwargs)

builtins.__import__ = importing
try:
    sys.path.insert(0, str(source.parent))
    spec = importlib.util.spec_from_file_location('fixture_entry_control', source)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    report = artifacts / 'native-fixture-entry.json'
    assert module.run('behavior', work, report) == 0
    observed = json.loads(report.read_text())
    expected = json.loads((artifacts/'native-fixture-cli.json').read_text())
    assert observed['cases'] == expected['cases']
    assert sum(case['status'] == 'passed' for case in observed['cases']) == 5
    assert len(observed['cases']) == 9
    for invalid in ('', 'missing', 'behavior --skip'):
        try:
            module.run(invalid, work, report)
        except ValueError:
            pass
        else:
            raise AssertionError('Unknown suite accepted')
finally:
    builtins.__import__ = original
print('fixture embedding/native CLI equivalence passed: all9 groups preserved, behavior5pass4unselected; no argparse')
