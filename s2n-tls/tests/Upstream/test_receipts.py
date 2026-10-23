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
    def direct_c_fixture(self, root):
        """A harmless process fixture exercises caller acceptance without a build."""
        case = 'tests/unit/receipt_gate.c'
        original = root / 'ref/s2n-tls' / case
        original.parent.mkdir(parents=True)
        original.write_bytes(b'/* Native-report caller regression fixture. */\n')
        staged = root / 'compiled.c'
        staged.write_bytes(b'#include "utils/s2n_prelude.h"\n#include "dotcc_s2n_host.h"\n' + original.read_bytes())
        sentinel = root / 'executed'
        process = root / 'harness_fixture.py'
        process.write_text('from pathlib import Path\nimport sys\n'
                           'Path(sys.argv[1]).write_text("executed")\nprint("PASSED 17 tests")\n')
        (root / 'config').mkdir()
        (root / 'config/upstream-tests.json').write_text(json.dumps({'cases': [
            dict(path=case, status='required', category='unit', reason='', assertion_sites=1,
                 test_functions=[], conditional_or_skip_sites=0)]}))
        manifest = root / 'executables.json'
        manifest.write_text(json.dumps({'executables': [dict(case=case, form='raw', mode='jit', status='built',
            compiled_source=str(staged), original_sha256=runner.hashlib.sha256(original.read_bytes()).hexdigest(),
            compiled_sha256=runner.hashlib.sha256(staged.read_bytes()).hexdigest(),
            execution_sha256={str(process): runner.hashlib.sha256(process.read_bytes()).hexdigest()},
            command=[sys.executable, str(process), str(sentinel)])]}))
        return case, manifest, sentinel

    def test_direct_c_rejects_incomplete_native_before_harness_execution(self):
        for status in ('failed', 'running', None):
            for native_key in ('case', 'name'):
                with self.subTest(status=status, native_key=native_key), tempfile.TemporaryDirectory() as directory:
                    root = Path(directory)
                    case, manifest, sentinel = self.direct_c_fixture(root)
                    native = root / 'native.json'
                    reference = {'upstream': [{native_key: case if native_key == 'case' else Path(case).stem,
                                              'status': 'passed', 'assertions': 17}]}
                    if status is not None:
                        reference['status'] = status
                    native.write_text(json.dumps(reference))
                    output = root / 'result.json'
                    argv = ['run.py', '--c-only', '--executables', str(manifest), '--native-report',
                            str(native), '--output', str(output)]
                    with patch.object(runner, 'ROOT', root), patch.object(sys, 'argv', argv), \
                         patch.object(sys, 'path', [str(HERE.parent / 'Provider'), *sys.path]), \
                         patch.object(runner.subprocess, 'run', wraps=subprocess.run) as execute:
                        self.assertEqual(runner.main(), 1)
                    execute.assert_not_called()
                    self.assertFalse(sentinel.exists())
                    result = json.loads(output.read_text())
                    self.assertEqual(result['status'], 'failed')
                    self.assertEqual(result['counts']['failed'], 1)
                    self.assertEqual(result['cases'][0]['assertions'], 0)
                    self.assertIn("top-level status 'passed'", result['cases'][0]['error'])

    def test_direct_c_accepts_passed_native_with_exact_assertion_comparison(self):
        for native_key in ('case', 'name'):
            with self.subTest(native_key=native_key), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                case, manifest, sentinel = self.direct_c_fixture(root)
                native = root / 'native.json'
                native.write_text(json.dumps({'status': 'passed', 'upstream': [
                    {native_key: case if native_key == 'case' else Path(case).stem,
                     'status': 'passed', 'assertions': 17}]}))
                output = root / 'result.json'
                argv = ['run.py', '--c-only', '--executables', str(manifest), '--native-report',
                        str(native), '--output', str(output)]
                with patch.object(runner, 'ROOT', root), patch.object(sys, 'argv', argv), \
                     patch.object(sys, 'path', [str(HERE.parent / 'Provider'), *sys.path]):
                    self.assertEqual(runner.main(), 0)
                self.assertEqual(sentinel.read_text(), 'executed')
                result = json.loads(output.read_text())
                self.assertEqual(result['counts']['passed'], 1)
                self.assertEqual(result['cases'][0]['assertions'], 17)
                self.assertEqual(result['cases'][0]['native_assertions'], 17)

    def test_managed_forwarding_consumes_aggregate_output_not_last_python_input(self):
        specification = importlib.util.spec_from_file_location('managed_qualification_forwarding',
            HERE.parents[1] / 'scripts/managed-qualification.py')
        managed = importlib.util.module_from_spec(specification)
        specification.loader.exec_module(managed)
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            fixtures = root / 'fixtures'
            fixtures.mkdir()
            for name in ('rsa.pem', 'rsa.key', 'ecdsa.pem', 'ecdsa.key'):
                (fixtures / name).write_text('forwarding fixture')
            source = root / 'source'
            (source / 'tests/pems').mkdir(parents=True)
            (source / 'tests/pems/dhparams_2048.pem').write_text('forwarding fixture')
            translated = [root / 'translated-first.json', root / 'translated-second.json']
            native = [root / 'native-first.json', root / 'native-second.json']
            for path in (*translated, *native):
                path.write_text(json.dumps({'cases': [{'case': 'input-only', 'status': 'failed'}]}))
            product = root / 'product.csproj'
            product.write_text('<Project/>')
            output = root / 'output'
            output.mkdir()
            qualification = managed.Qualification.__new__(managed.Qualification)
            qualification.args = types.SimpleNamespace(suite=['upstream'], form='raw', mode='jit',
                product=product, rid='linux-x64', upstream_executables=root / 'executables.json',
                native_report=None, native_equivalence=None, c_report=None, campaign_run=None,
                python_executables=root / 'endpoints.json', python=sys.executable, python_workers=1,
                peer_commands=None, python_report=translated, native_python_report=native)
            qualification.output, qualification.fixtures, qualification.source = output, fixtures, source
            qualification.report = {'status': 'failed', 'rows': [], 'errors': []}
            def consume(command, **_kwargs):
                self.assertEqual([command[i + 1] for i, value in enumerate(command)
                                  if value == '--python-report'], translated)
                self.assertEqual([command[i + 1] for i, value in enumerate(command)
                                  if value == '--native-python-report'], native)
                destination = command[command.index('--output') + 1]
                destination.write_text(json.dumps({'cases': [{'case': 'actual-aggregate', 'status': 'passed'}]}))
            with patch.object(qualification, 'snapshot_product', return_value=product), \
                 patch.object(qualification, 'command', side_effect=consume):
                self.assertEqual(qualification.run(), 0)
            result = json.loads((output / 'report.json').read_text())
            self.assertEqual(result['rows'], [{'case': 'actual-aggregate', 'status': 'passed'}])
            self.assertEqual(result['counts']['passed'], 1)

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
