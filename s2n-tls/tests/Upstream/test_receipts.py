"""Controller-consumption regressions; these do not simulate a TLS engine."""
import importlib.util
import json
from pathlib import Path
import sys
import subprocess
import tempfile
import types
import unittest
from unittest.mock import patch

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
specification = importlib.util.spec_from_file_location('upstream_receipt_runner', HERE / 'run.py')
runner = importlib.util.module_from_spec(specification)
specification.loader.exec_module(runner)
from python_receipt import Validator


class ReceiptTests(unittest.TestCase):
    def test_provider_policy_cannot_mix_with_explicit_provider_before_any_build(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / 'not-created.json'
            result = subprocess.run([sys.executable, '-B', str(HERE / 'build.py'), '--campaign-run', directory,
                '--provider-policy', str(Path(directory) / 'policy.json'), '--provider-project',
                str(Path(directory) / 'provider.csproj'), '--output', str(output)], capture_output=True, text=True)
            self.assertEqual(result.returncode, 2)
            self.assertIn('cannot be combined', result.stderr)
            self.assertFalse(output.exists())

    def test_completed_c_aggregate_uses_case_paths_without_launching_a_controller(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / 'config').mkdir()
            cases = ['tests/unit/a.c', 'tests/unit/b.c']
            inventory = {'cases': [dict(path=case, status='required', category='unit', reason='',
                assertion_sites=1, test_functions=[], conditional_or_skip_sites=0) for case in cases]}
            (root / 'config/upstream-tests.json').write_text(json.dumps(inventory))
            native = root / 'native.json'
            native.write_text(json.dumps({'upstream': [{'case': case, 'status': 'passed'} for case in cases]}))
            output = root / 'result.json'
            helper = types.ModuleType('required_c_receipt')
            helper.validate_completed_report = lambda *_args, **_kwargs: {'status': 'passed', 'cases': [
                dict(case=case, form=form, mode=mode, status='passed', assertions=17, native_assertions=17)
                for case in cases for form in ('raw', 'processed') for mode in ('jit', 'aot')]}
            argv = ['run.py', '--c-only', '--c-report', str(root / 'retained.json'), '--native-report', str(native),
                    '--output', str(output)]
            with patch.object(runner, 'ROOT', root), patch.object(sys, 'argv', argv), \
                 patch.dict(sys.modules, {'required_c_receipt': helper}), \
                 patch.object(runner.subprocess, 'run', side_effect=AssertionError('Duplicate execution launched')):
                self.assertEqual(runner.main(), 0)
            result = json.loads(output.read_text())
            self.assertEqual(result['counts']['passed'], 2)
            self.assertEqual({row['case'] for row in result['cases']}, set(cases))
            self.assertEqual({row['assertions'] for row in result['cases']}, {17})

    def test_incomplete_c_aggregate_keeps_gate_failed_without_execution(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / 'config').mkdir()
            inventory = {'cases': [dict(path=case, status='required', category='unit', reason='',
                assertion_sites=1, test_functions=[], conditional_or_skip_sites=0)
                for case in ('tests/unit/a.c', 'tests/integrationv2/a.py')]}
            (root / 'config/upstream-tests.json').write_text(json.dumps(inventory))
            native = root / 'native.json'
            native.write_text('{"upstream": []}')
            helper = types.ModuleType('required_c_receipt')
            def reject(*_args, **_kwargs):
                raise RuntimeError('Incomplete original four-variant execution')
            helper.validate_completed_report = reject
            output = root / 'result.json'
            argv = ['run.py', '--c-report', str(root / 'retained.json'), '--native-report', str(native),
                    '--native-equivalence', str(root / 'proof.json'), '--output', str(output)]
            with patch.object(runner, 'ROOT', root), patch.object(sys, 'argv', argv), \
                 patch.dict(sys.modules, {'required_c_receipt': helper}), \
                 patch.object(runner.subprocess, 'run', side_effect=AssertionError('Duplicate execution launched')):
                self.assertEqual(runner.main(), 1)
            self.assertEqual(json.loads(output.read_text())['counts']['failed'], 2)

    def test_matrix_selects_exact_variant_and_rejects_changed_leaf(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            matrix = {'evidence_kind': 'original-python-matrix', 'endpoint_kind': 'translated', 'cases': []}
            for form in ('raw', 'processed'):
                for mode in ('jit', 'aot'):
                    leaf = root / (form + '-' + mode + '.json')
                    leaf.write_text(json.dumps({'evidence_kind': 'original-python-integration', 'endpoint_kind': 'translated',
                        'form': form, 'mode': mode, 'cases': [{'case': 'original.py', 'status': 'passed'}]}))
                    matrix['cases'].append(dict(case='original.py', status='passed', form=form, mode=mode,
                        report=str(leaf), report_sha256=runner.hashlib.sha256(leaf.read_bytes()).hexdigest()))
            path = root / 'matrix.json'
            path.write_text(json.dumps(matrix))
            selected = runner.python_reports([path], 'translated', 'raw', 'jit')
            self.assertEqual(len(selected), 1)
            self.assertEqual(selected['original.py'][1]['mode'], 'jit')
            (root / 'raw-jit.json').write_text('{}')
            with self.assertRaisesRegex(RuntimeError, 'identity changed'):
                runner.python_reports([path], 'translated', 'raw', 'jit')

    def test_a_rerun_cannot_be_adopted_as_a_success(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'parameters.json'
            path.write_text(json.dumps({'status': 'passed', 'exit_code': 0, 'failed': 0, 'skipped': 0,
                'collection_errors': [], 'collected': [{'nodeid': 'a'}], 'deselected': [], 'passed': 1,
                'outcomes': [{'nodeid': 'a', 'phase': 'call', 'outcome': 'rerun'},
                             {'nodeid': 'a', 'phase': 'call', 'outcome': 'passed'}]}))
            row = dict(status='passed', exit_code=0, parameters=str(path), passed=1, parameter_cases=1,
                process=dict(exit_code=0, timed_out=False, drained=True, remaining_endpoints=False))
            with self.assertRaisesRegex(RuntimeError, 'retried'):
                Validator().family({}, row)


if __name__ == '__main__':
    unittest.main()
