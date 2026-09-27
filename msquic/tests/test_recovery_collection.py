"""Diagnostic collection must retain failures and cannot grant qualification."""
import importlib.util
from pathlib import Path
from types import SimpleNamespace
import sys
import unittest
from unittest.mock import patch

scripts = Path(__file__).resolve().parents[1] / 'scripts'
sys.path.insert(0, str(scripts))
spec = importlib.util.spec_from_file_location('recovery_collection', scripts / 'test-recovery.py')
recovery = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recovery)


class RecoveryCollectionTests(unittest.TestCase):
    def receipt(self):
        return dict(cases=[], passed=False, targeted_passed=False, verification_passed=False)

    def failed(self, receipt, error='peer failed'):
        receipt['cases'].append(dict(name='case', passed=False, error=error, elapsed_seconds=15.0))
        raise RuntimeError(error)

    def test_default_still_stops(self):
        receipt = self.receipt()
        with patch.object(recovery, 'run_case', side_effect=lambda *a: self.failed(receipt)):
            with self.assertRaisesRegex(RuntimeError, 'peer failed'):
                recovery.collect_case(SimpleNamespace(keep_going=False), receipt)
        self.assertNotIn('unexpected_failures', receipt)

    def test_diagnostic_retains_original_failure(self):
        receipt = self.receipt()
        with patch.object(recovery, 'run_case', side_effect=lambda *a: self.failed(receipt)):
            recovery.collect_case(SimpleNamespace(keep_going=True), receipt)
        self.assertFalse(receipt['cases'][0]['passed'])
        self.assertEqual(receipt['unexpected_failures'], [dict(name='case', error='peer failed', exception='RuntimeError')])
        with self.assertRaisesRegex(RuntimeError, '1 unexpected'):
            recovery.complete_collection(receipt, True, 1)
        self.assertTrue(receipt['matrix_complete'])
        self.assertEqual(receipt['cases_passed'], 0)
        self.assertEqual(receipt['cases_unexpected'], 1)
        self.assertFalse(receipt['verification_passed'])
        self.assertFalse(receipt['passed'])

    def test_missing_or_inconsistent_case_stops(self):
        for case in (None, dict(passed=True, error='bad', elapsed_seconds=1),
                     dict(passed=False, error='different', elapsed_seconds=1),
                     dict(passed=False, error='bad')):
            with self.subTest(case=case):
                receipt = self.receipt()
                def fail(*args):
                    if case is not None: receipt['cases'].append(case)
                    raise RuntimeError('bad')
                with patch.object(recovery, 'run_case', side_effect=fail):
                    with self.assertRaisesRegex(RuntimeError, 'bad'):
                        recovery.collect_case(SimpleNamespace(keep_going=True), receipt)

    def test_interrupts_are_not_swallowed(self):
        with patch.object(recovery, 'run_case', side_effect=KeyboardInterrupt):
            with self.assertRaises(KeyboardInterrupt):
                recovery.collect_case(SimpleNamespace(keep_going=True), self.receipt())

    def test_existing_warning_retains_separate_success_counts(self):
        receipt = self.receipt()
        receipt['cases'] = [dict(passed=True), dict(passed=False, warning='known')]
        recovery.complete_collection(receipt, True, 2)
        self.assertTrue(receipt['verification_passed'])
        self.assertFalse(receipt['passed'])
        self.assertEqual((receipt['cases_passed'], receipt['cases_warned']), (1, 1))

    def test_incomplete_matrix_rejects(self):
        receipt = self.receipt()
        with self.assertRaisesRegex(RuntimeError, 'incomplete'):
            recovery.complete_collection(receipt, True, 1)
        self.assertFalse(receipt['verification_passed'])


if __name__ == '__main__':
    unittest.main()
