"""Receipt-retention controls only; no interpreter or upstream case executes."""
import importlib.util
import json
from pathlib import Path
import tempfile
import types
import unittest
from unittest import mock
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[3]/'Scripts'))

spec = importlib.util.spec_from_file_location('upstream_receipt_control', Path(__file__).with_name('campaign_hooks.py'))
hooks = importlib.util.module_from_spec(spec)
spec.loader.exec_module(hooks)


class FailedReportRetention(unittest.TestCase):
    def test_missing_profile_proof_fails_before_consumer_build_or_execution(self):
        context = types.SimpleNamespace(root=Path(__file__).resolve().parents[2],
            profile='upstream-tests-static',options=types.SimpleNamespace(rid='linux-x64',form='processed'),
            run=mock.Mock(side_effect=AssertionError('build/execution must not be reached')))
        with mock.patch('campaigns.translation.check_product',return_value=Path('unbuilt.csproj')):
            with mock.patch.dict(hooks.os.environ,{},clear=True):
                with self.assertRaisesRegex(RuntimeError,'Explicit test-profile compiled/lifetime evidence'):
                    hooks.run_suite(context,types.SimpleNamespace(timeout=900))
        context.run.assert_not_called()

    def test_required_nonpass_and_duplicate_occurrences_are_preserved(self):
        with tempfile.TemporaryDirectory() as temporary:
            path = Path(temporary)/'cases.json'
            document = {'counts': {'executed': 5, 'executed_unique': 4, 'passed': 3,
                                  'required-skip': 1, 'required-expected-failure': 1},
                        'required_unexecuted': ['required.missing'], 'cases': [
                            {'case': 'required.a', 'status': 'passed'},
                            {'case': 'required.skipped', 'status': 'required-skip', 'reason': 'original reason'},
                            {'case': 'required.xfail', 'status': 'required-expected-failure'}]}
            path.write_text(json.dumps(document))
            cell = {'status': 'blocked-or-failed', 'error': 'interpreter exited 1'}
            hooks.retain_report(cell, path)
            self.assertEqual(cell['counts'], document['counts'])
            self.assertEqual(cell['status'], 'blocked-or-failed')
            self.assertEqual(cell['first_required_nonpass'], document['cases'][1])
            self.assertEqual(cell['required_unexecuted'], ['required.missing'])
            self.assertEqual(len(cell['report_sha256']), 64)

    def test_missing_report_does_not_invent_results(self):
        with tempfile.TemporaryDirectory() as temporary:
            cell = {'status': 'blocked-or-failed'}
            self.assertIsNone(hooks.retain_report(cell, Path(temporary)/'missing.json'))
            self.assertNotIn('counts', cell)

    def test_damaged_report_is_not_accepted(self):
        with tempfile.TemporaryDirectory() as temporary:
            path = Path(temporary)/'cases.json'
            path.write_text('{')
            with self.assertRaises(json.JSONDecodeError):
                hooks.retain_report({'status': 'blocked-or-failed'}, path)


if __name__ == '__main__':
    unittest.main(verbosity=2)
