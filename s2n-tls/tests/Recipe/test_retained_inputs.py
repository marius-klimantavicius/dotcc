"""Acceptance selection guards only; these fixtures never qualify TLS cases."""
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
SPEC = importlib.util.spec_from_file_location('s2n_retained_inputs', ROOT / 'scripts/upstream_qualification.py')
runner = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(runner)


def write(path, data):
    path.write_text(json.dumps(data) + '\n')


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


class RetainedInputTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name)
        (self.root / 'config').mkdir()
        self.policy = dict(reuse_evidence_environment='S2N_RETAINED_INPUT_TEST',
                           acceptance_consumers=['consumer.py'])
        write(self.root / 'config/qualification.json', self.policy)
        self.source = self.root / 'consumer.py'
        self.source.write_text('# acceptance consumer fixture\n')
        self.snapshot = self.root / 'preserved-consumer.py'
        self.snapshot.write_bytes(self.source.read_bytes())
        self.native = self.root / 'native.json'
        self.c_report = self.root / 'c.json'
        write(self.native, dict(status='passed', scope='selection fixture only'))
        write(self.c_report, dict(status='passed', scope='selection fixture only'))
        self.campaign = self.root / 'closed-translation'
        self.bundle = self.root / 'bundle.json'
        self.data = dict(schema_version=1, status='passed', campaign_run=str(self.campaign),
                         native_report=str(self.native), c_report=str(self.c_report),
                         input_sha256={str(p): digest(p) for p in (self.native, self.c_report)},
                         consumer_source_sha256={str(self.source): digest(self.source)},
                         consumer_source_snapshots={str(self.source): str(self.snapshot)})
        self.ctx = SimpleNamespace(root=self.root, artifacts=self.root,
                                   options=SimpleNamespace(form='raw', mode='jit'),
                                   receipt={}, save=Mock(), run=Mock())
        self.addCleanup(patch.stopall)
        patch.dict(runner.os.environ, {self.policy['reuse_evidence_environment']: str(self.bundle)}).start()
        patch.object(runner, 'campaign', return_value=self.campaign).start()
        self.prepare = patch.object(runner, 'prepare_c', side_effect=AssertionError('No implicit controller')).start()

    def load(self):
        write(self.bundle, self.data)
        return runner.inputs(self.ctx)

    def test_valid_seal_selects_inputs_without_starting_tests(self):
        selected = self.load()
        self.assertEqual(selected['input_bundle']['sha256'], digest(self.bundle))
        self.prepare.assert_not_called()
        self.ctx.run.assert_not_called()

    def test_pending_bundle_rejected_before_any_command(self):
        for status in ('pending', 'failed', None):
            with self.subTest(status=status):
                self.data['status'] = status
                with self.assertRaisesRegex(RuntimeError, 'not sealed passed'):
                    self.load()
        self.prepare.assert_not_called()
        self.ctx.run.assert_not_called()

    def test_failed_report_rejected_even_with_matching_hash(self):
        write(self.native, dict(status='failed'))
        self.data['input_sha256'][str(self.native)] = digest(self.native)
        with self.assertRaisesRegex(RuntimeError, 'changed or is incomplete'):
            self.load()
        self.ctx.run.assert_not_called()

    def test_report_modified_after_sealing_rejected(self):
        write(self.c_report, dict(status='passed', changed=True))
        with self.assertRaisesRegex(RuntimeError, 'changed or is incomplete'):
            self.load()

    def test_other_translation_rejected_without_preparation(self):
        self.data['campaign_run'] = str(self.root / 'other-translation')
        with self.assertRaisesRegex(RuntimeError, 'another translation'):
            self.load()
        self.prepare.assert_not_called()

    def test_missing_or_extra_selected_receipt_hash_rejected(self):
        del self.data['input_sha256'][str(self.native)]
        with self.assertRaisesRegex(RuntimeError, 'exactly the selected'):
            self.load()
        self.data['input_sha256'][str(self.native)] = digest(self.native)
        self.data['input_sha256']['unselected.json'] = 'unused'
        with self.assertRaisesRegex(RuntimeError, 'exactly the selected'):
            self.load()

    def test_consumer_and_preserved_copy_must_both_match(self):
        for path in (self.source, self.snapshot):
            with self.subTest(path=path):
                original = path.read_bytes()
                path.write_text('# changed consumer\n')
                with self.assertRaisesRegex(RuntimeError, 'consumer or preserved source changed'):
                    self.load()
                path.write_bytes(original)
        self.data['consumer_source_snapshots'][str(self.source)] = str(self.source)
        with self.assertRaisesRegex(RuntimeError, 'must be a preserved copy'):
            self.load()

    def test_cached_selection_is_rechecked(self):
        self.load()
        write(self.c_report, dict(status='running'))
        with self.assertRaisesRegex(RuntimeError, 'changed or is incomplete'):
            runner.inputs(self.ctx)
        self.ctx.run.assert_not_called()

    def test_bundle_replaced_after_selection_is_rejected(self):
        self.load()
        self.bundle.write_text('{}\n')
        with self.assertRaisesRegex(RuntimeError, 'bundle changed after selection'):
            runner.inputs(self.ctx)

    def test_input_changed_during_acceptance_command_rejected(self):
        self.load()
        self.ctx.run.side_effect = lambda *args, **kwargs: write(self.c_report, dict(status='failed'))
        with self.assertRaisesRegex(RuntimeError, 'changed or is incomplete'):
            runner.run(self.ctx, python=False)
        self.ctx.run.assert_called_once()


if __name__ == '__main__':
    unittest.main()
