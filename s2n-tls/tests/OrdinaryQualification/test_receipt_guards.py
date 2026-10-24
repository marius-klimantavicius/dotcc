"""No-workload checks for honest ordinary-control receipts and output admission."""
import importlib.util
from pathlib import Path
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch


HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('ordinary_qualification', HERE / 'qualify.py')
qualification = importlib.util.module_from_spec(spec)
spec.loader.exec_module(qualification)


class ReceiptGuardTests(unittest.TestCase):
    def receipt(self, modes=(), *, audit_only=False, case='ordinary.c'):
        run = object.__new__(qualification.Run)
        run.args = SimpleNamespace(audit_only=audit_only, case=[case] if case else [])
        run.report = {'inputs_sha256': {}, 'selected_cases': run.args.case,
                      'results': [dict(case=case, mode=mode, actual_assertions=3, status='passed')
                                  for mode in modes]}
        run.save = Mock()
        return run

    def test_inventory_audit_never_claims_assertion_execution(self):
        run = self.receipt(audit_only=True)
        run.finish()
        self.assertEqual(run.report['status'], 'inventory-audited')
        self.assertEqual(run.report['execution'], 'unrun-audit-only')
        self.assertEqual(run.report['result_counts'], {})

    def test_empty_execution_cannot_pass(self):
        for case in ('ordinary.c', ''):
            with self.subTest(case=case):
                run = self.receipt(case=case)
                run.finish()
                self.assertEqual(run.report['status'], 'failed')

    def test_missing_managed_variant_cannot_pass(self):
        run = self.receipt(qualification.MODES[:-1])
        run.finish()
        self.assertFalse(run.report['complete_selected_matrix'])
        self.assertEqual(run.report['status'], 'failed')

    def test_duplicate_variant_cannot_replace_a_missing_variant(self):
        run = self.receipt([*qualification.MODES[:-1], qualification.MODES[0]])
        run.finish()
        self.assertEqual(run.report['status'], 'failed')

    def test_exact_selected_matrix_passes_and_failed_assertions_do_not(self):
        run = self.receipt(qualification.MODES)
        run.finish()
        self.assertEqual(run.report['status'], 'passed')
        run.report['results'][0]['status'] = 'failed'
        run.finish()
        self.assertEqual(run.report['status'], 'failed')

    def test_unrequested_case_cannot_be_adopted(self):
        run = self.receipt(qualification.MODES)
        run.report['results'].append(dict(case='unrequested.c', mode='raw-jit',
                                          actual_assertions=3, status='passed'))
        run.finish()
        self.assertEqual(run.report['status'], 'failed')

    def test_administrative_context_drift_does_not_invalidate_actual_results(self):
        with tempfile.TemporaryDirectory() as directory:
            context = Path(directory) / 'PLAN.md'
            context.write_text('old coordination status')
            run = self.receipt(qualification.MODES)
            run.report['context_inputs_sha256'] = {str(context): qualification.sha(context)}
            context.write_text('new coordination status')
            run.finish()
            self.assertEqual(run.report['status'], 'passed')
            self.assertEqual(run.report['context_input_drift'], [str(context)])
            run.report['inputs_sha256'] = {str(context): 'different actual source hash'}
            with self.assertRaisesRegex(RuntimeError, 'Protected inputs changed'):
                run.finish()

    def test_symlink_output_and_low_reserve_fail_before_allocation(self):
        with tempfile.TemporaryDirectory() as directory:
            base = Path(directory).resolve()
            alias = base / 'alias'
            try:
                alias.symlink_to(base, target_is_directory=True)
            except (OSError, NotImplementedError) as error:
                self.skipTest('Directory symlinks unavailable: ' + str(error))
            with self.assertRaisesRegex(RuntimeError, 'symlink'):
                qualification.Run(SimpleNamespace(output=alias / 'new-output'))
            self.assertFalse((base / 'new-output').exists())
            # Admission accepts this lexical external scope; a failed reserve
            # must still prevent its creation and any tool/input reads.
            destination = Path('/mnt/storage') / ('ordinary-guard-never-created-' + base.name)
            self.assertFalse(destination.exists())
            with patch.object(qualification, 'require_storage_reserve', side_effect=RuntimeError('2GiB reserve')):
                with self.assertRaisesRegex(RuntimeError, '2GiB reserve'):
                    qualification.Run(SimpleNamespace(output=destination))
            self.assertFalse(destination.exists())


if __name__ == '__main__':
    unittest.main()
