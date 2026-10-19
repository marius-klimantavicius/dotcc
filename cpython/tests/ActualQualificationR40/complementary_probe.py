"""Two exact unchanged cases under matched native/actual isolated embedding."""
import hashlib
import importlib
import json
import locale
import os
from pathlib import Path
import sys
import unittest

import run_inventory as runner


def snapshot():
    from test.support import os_helper
    return {'locale': locale.setlocale(locale.LC_CTYPE), 'getencoding': locale.getencoding(),
        'getpreferredencodingFalse': locale.getpreferredencoding(False),
        'filesystemencoding': sys.getfilesystemencoding(), 'FS_NONASCII': os_helper.FS_NONASCII,
        'utf8_mode': sys.flags.utf8_mode, 'isolated': sys.flags.isolated,
        'ignore_environment': sys.flags.ignore_environment,
        'environment': {k: os.environ.get(k) for k in ('LC_ALL', 'LANG', 'DOTCC_R40_INITIAL_LOCALE', 'DOTCC_R40_SELECTED_LOCALE')}}


def run_probe(report):
    report = Path(report)
    inputs = json.loads((Path(__file__).parent / 'locale-probe-inputs.json').read_text())
    expected = {r['case']: r for r in inputs['cases']}
    rows = []
    tests = []
    for module_name in sorted({case.rsplit('.', 2)[0] for case in expected}):
        loader = unittest.TestLoader()
        module = importlib.import_module(module_name)
        loaded = list(runner.flatten(loader.loadTestsFromModule(module)))
        assert not loader.errors, loader.errors
        for test in loaded:
            if test.id() not in expected:
                continue
            detail = runner.case_detail(test)
            original = expected[test.id()]
            for key in ('source', 'method', 'declaring_class', 'expected_failure'):
                assert detail[key] == original['detail'][key], (test.id(), key)
            function = runner.source_callable(getattr(type(test), test._testMethodName))
            source = Path(runner.inspect.getsourcefile(function))
            assert hashlib.sha256(source.read_bytes()).hexdigest() == original['sourceSha256']
            rows.append({'case': test.id(), 'sourceSha256': original['sourceSha256'],
                         'methodAndDecoratorUnchanged': True})
            tests.append(test)
    assert {test.id() for test in tests} == set(expected) and len(tests) == 2
    before = snapshot()
    with report.with_suffix('.unittest.log').open('w') as stream, report.with_suffix('.journal.jsonl').open('w') as journal:
        def factory(*args, **kwargs):
            result = runner.CaseResult(*args, **kwargs)
            result.journal = journal
            return result
        result = unittest.TextTestRunner(verbosity=2, resultclass=factory, stream=stream).run(unittest.TestSuite(tests))
    report.write_text(json.dumps({'scope': 'matching isolated embedded locale control; no waiver',
        'beforeTests': before, 'afterTests': snapshot(), 'sourceWitnesses': rows,
        'cases': result.cases, 'executed': result.testsRun}, indent=2) + '\n')
    return 0
