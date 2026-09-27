"""Framework provenance routing tests; no compiler, links, or elevated setup."""
import hashlib
import json
import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
from product_evidence import product_closure


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


class ProductEvidenceTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name).resolve() / 'msquic'
        self.owner = self.root / 'artifacts/campaign/attempt'
        self.owner.mkdir(parents=True)
        self.receipt_path = self.owner / 'receipt.json'
        self.closure_path = self.owner / 'qualified-closure.json'
        self.current_path = self.owner.parent / 'current-default.json'
        self.closure = dict(gates=dict(raw_optimized_jit_nativeaot=True,
                                      entire_generated_assembly_rooted_for_aot=True), generated={})
        self.receipt = dict(project='msquic', profile='default', run_id='attempt', action='verify', status='running',
                            tasks=[dict(name='translation', status='passed')], outputs={},
                            qualification=str(self.closure_path), commands=[
                                dict(label='product-boundary', status='passed', command=[]),
                                dict(label='freeze-closure', status='passed', command=['freeze', '--output', str(self.closure_path)])])
        for form, variant, directory in [('raw', 'raw', 'TranslatedMsQuic.Raw'), ('processed', 'optimized', 'TranslatedMsQuic')]:
            product = self.root / 'generated' / directory
            product.mkdir(parents=True)
            project = product / 'TranslatedMsQuic.csproj'
            project.write_text('<Project/>')
            (product / 'Engine.cs').write_text('class Engine {}')
            hashes = {name: digest(product / name) for name in ('Engine.cs', project.name)}
            self.closure['generated'][variant] = hashes.copy()
            self.receipt['outputs'][form] = dict(project=str(project), hashes=hashes.copy())
        environment = patch.dict(os.environ)
        environment.start()
        self.addCleanup(environment.stop)
        os.environ.pop('DOTCC_MSQUIC_QUALIFIED_RECEIPT', None)
        self.save()

    def save(self):
        self.closure_path.write_text(json.dumps(self.closure))
        self.receipt['qualification_sha256'] = digest(self.closure_path)
        self.receipt_path.write_text(json.dumps(self.receipt))
        self.current_path.write_text(json.dumps(dict(receipt=str(self.receipt_path), outputs=self.receipt['outputs'])))

    def test_current_in_progress_verification_has_completed_delivery(self):
        self.assertEqual(product_closure(self.root), self.closure_path)

    def test_explicit_successful_framework_receipt(self):
        self.receipt['status'] = 'passed'
        self.save()
        self.current_path.unlink()
        os.environ['DOTCC_MSQUIC_QUALIFIED_RECEIPT'] = str(self.receipt_path)
        self.assertEqual(product_closure(self.root), self.closure_path)

    def test_no_fallback_to_historical_checkpoint(self):
        (self.root / 'config').mkdir()
        (self.root / 'config/product-closure.json').write_text(json.dumps(self.closure))
        self.current_path.unlink()
        with self.assertRaisesRegex(RuntimeError, 'Cannot read'):
            product_closure(self.root)

    def test_rejects_receipt_outside_campaign(self):
        foreign = self.root / 'receipt.json'
        foreign.write_text(json.dumps(self.receipt))
        os.environ['DOTCC_MSQUIC_QUALIFIED_RECEIPT'] = str(foreign)
        with self.assertRaisesRegex(RuntimeError, 'not owned'):
            product_closure(self.root)

    def test_rejects_wrong_identity(self):
        self.receipt['project'] = 'other'
        self.save()
        with self.assertRaisesRegex(RuntimeError, 'identity'):
            product_closure(self.root)

    def test_rejects_failed_receipt(self):
        self.receipt['status'] = 'failed'
        self.save()
        with self.assertRaisesRegex(RuntimeError, 'active or successful'):
            product_closure(self.root)

    def test_rejects_incomplete_translation(self):
        self.receipt['tasks'][0]['status'] = 'running'
        self.save()
        with self.assertRaisesRegex(RuntimeError, 'translation'):
            product_closure(self.root)

    def test_rejects_failed_freeze(self):
        self.receipt['commands'][1]['status'] = 'failed'
        self.save()
        with self.assertRaisesRegex(RuntimeError, 'freeze-closure gate'):
            product_closure(self.root)

    def test_rejects_unowned_qualification(self):
        self.receipt['qualification'] = str(self.root / 'elsewhere.json')
        self.save()
        with self.assertRaisesRegex(RuntimeError, 'not owned'):
            product_closure(self.root)

    def test_rejects_changed_qualification(self):
        self.closure_path.write_text('{}')
        with self.assertRaisesRegex(RuntimeError, 'missing or changed'):
            product_closure(self.root)

    def test_rejects_missing_required_gates(self):
        self.closure['gates']['entire_generated_assembly_rooted_for_aot'] = False
        self.save()
        with self.assertRaisesRegex(RuntimeError, 'boundary gates'):
            product_closure(self.root)

    def test_rejects_changed_generated_product(self):
        (self.root / 'generated/TranslatedMsQuic/Engine.cs').write_text('changed')
        with self.assertRaisesRegex(RuntimeError, 'does not match qualification'):
            product_closure(self.root)

    def test_rejects_changed_delivery_pointer_outputs(self):
        self.current_path.write_text(json.dumps(dict(receipt=str(self.receipt_path), outputs={})))
        with self.assertRaisesRegex(RuntimeError, 'canonical delivery'):
            product_closure(self.root)

    def test_rejects_inventory_escape(self):
        self.closure['generated']['raw']['../outside'] = 'bad'
        self.save()
        with self.assertRaisesRegex(RuntimeError, 'inventory escapes'):
            product_closure(self.root)

    def test_off_policy_omits_optional_framework_hash_manifest(self):
        self.receipt['hash_policy'] = 'off'
        for output in self.receipt['outputs'].values():
            output['hashes'] = {}
        self.save()
        self.assertEqual(product_closure(self.root), self.closure_path)
        (self.root / 'generated/TranslatedMsQuic/Engine.cs').write_text('changed')
        with self.assertRaisesRegex(RuntimeError, 'does not match qualification'):
            product_closure(self.root)

    def select_failed_verification_explicitly(self):
        self.receipt['status'] = 'failed'
        self.receipt['tasks'].append(dict(name='test:recovery', status='failed'))
        os.environ['DOTCC_MSQUIC_QUALIFIED_RECEIPT'] = str(self.receipt_path)

    def test_explicit_failed_test_is_diagnostic_only(self):
        self.select_failed_verification_explicitly()
        self.save()
        self.assertEqual(product_closure(self.root), self.closure_path)
        self.assertEqual(json.loads(self.receipt_path.read_text())['status'], 'failed')
        os.environ.pop('DOTCC_MSQUIC_QUALIFIED_RECEIPT')
        with self.assertRaisesRegex(RuntimeError, 'active or successful'):
            product_closure(self.root)

    def test_explicit_failed_translation_is_rejected(self):
        self.select_failed_verification_explicitly()
        self.receipt['tasks'][0]['status'] = 'failed'
        self.save()
        with self.assertRaises(RuntimeError):
            product_closure(self.root)

    def test_explicit_cancelled_test_is_diagnostic_only(self):
        self.select_failed_verification_explicitly()
        self.receipt['status'] = self.receipt['tasks'][-1]['status'] = 'cancelled'
        self.save()
        self.assertEqual(product_closure(self.root), self.closure_path)
        self.assertEqual(json.loads(self.receipt_path.read_text())['status'], 'cancelled')
        os.environ.pop('DOTCC_MSQUIC_QUALIFIED_RECEIPT')
        with self.assertRaisesRegex(RuntimeError, 'active or successful'):
            product_closure(self.root)

    def test_cancelled_translation_or_boundary_never_qualifies(self):
        self.select_failed_verification_explicitly()
        self.receipt['status'] = self.receipt['tasks'][-1]['status'] = 'cancelled'
        self.receipt['tasks'][0]['status'] = 'cancelled'
        self.save()
        with self.assertRaises(RuntimeError):
            product_closure(self.root)
        self.receipt['tasks'][0]['status'] = 'passed'
        for command in self.receipt['commands']:
            with self.subTest(gate=command['label']):
                command['status'] = 'cancelled'
                self.save()
                with self.assertRaisesRegex(RuntimeError, 'gate is not complete'):
                    product_closure(self.root)
                command['status'] = 'passed'

    def test_explicit_diagnostic_still_requires_boundary_and_freeze(self):
        self.select_failed_verification_explicitly()
        for command in self.receipt['commands']:
            with self.subTest(gate=command['label']):
                command['status'] = 'failed'
                self.save()
                with self.assertRaisesRegex(RuntimeError, 'gate is not complete'):
                    product_closure(self.root)
                command['status'] = 'passed'

    def test_explicit_diagnostic_still_requires_artifact_hash(self):
        self.select_failed_verification_explicitly()
        self.save()
        self.closure_path.write_text('{}')
        with self.assertRaisesRegex(RuntimeError, 'missing or changed'):
            product_closure(self.root)


if __name__ == '__main__':
    unittest.main()
