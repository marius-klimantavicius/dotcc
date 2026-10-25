"""Exercise maintained campaign selection and same-producer runtime dispatch."""
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / 'Scripts'))
from campaigns.cli import options_for, parser
from campaigns.identity import digest
from campaigns.recipes import load
from campaigns.testing import select

REPO = Path(__file__).resolve().parents[2]
recipe = load(REPO, 'blink')
campaign = sys.modules[recipe.translate.__module__]


class CampaignEntrypointsTests(unittest.TestCase):
    def test_default_verify_executes_guests_and_public_machine(self):
        options = options_for(parser().parse_args(['verify', 'blink']), recipe)
        suites = {suite.name for suite in select(recipe, options)}
        self.assertTrue({'guest-threads', 'machine-api'} <= suites)
        self.assertNotIn('consumer-delivery', suites)  # Docker service remains explicit.

    def test_supported_concurrency_reaches_assembler(self):
        for jobs, fresh in ((1, False), (2, False), (4, False), (4, True)):
            with self.subTest(jobs=jobs, fresh=fresh), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                profile = root / 'generated/core-profile/fresh'
                profile.mkdir(parents=True)
                (profile / 'inputs.json').write_text('{}')
                calls = []
                def script(name, *arguments, **kwargs):
                    calls.append((name, arguments))
                    return str(profile) + '\n' if name == 'probe-core.sh' else ''
                ctx = SimpleNamespace(root=root, receipt={}, env={}, compiler=root / 'dotcc.dll',
                    profile='single-thread', options=SimpleNamespace(fast=True, action='probe', unit=[],
                    jobs=jobs, timeout=12), script=script)
                with patch('production_support.production_sources', return_value={}), patch.object(
                        campaign, 'helper', return_value=SimpleNamespace(profile_sources=lambda *args: [])), patch.dict(
                        campaign.os.environ, {'DOTCC_BLINK_FRESH_OBJECTS': '1' if fresh else '0'}):
                    campaign.translate(ctx)
                arguments = next(args for name, args in calls if name == 'assemble-core.py')
                self.assertEqual(arguments[arguments.index('--jobs') + 1], str(jobs))
                self.assertEqual('--fresh' in arguments, fresh)
        with self.assertRaisesRegex(ValueError, '1..4'):
            options_for(parser().parse_args(['translate', 'blink', '--jobs', '8']), recipe)

    def test_thread_gate_runs_native_then_managed_with_pinned_producer(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            artifacts = root / 'artifacts/campaign/run'
            artifacts.mkdir(parents=True)
            producer = root / 'producer.json'
            producer.write_text('{}')
            (artifacts.parent / 'current-threaded.json').write_text(json.dumps({'receipt': str(producer)}))
            native, managed = root / 'artifacts/native', root / 'artifacts/managed'
            for path in (native, managed):
                path.mkdir()
                (path / 'receipt.json').write_text('{"passed":true}')
            calls = []
            def run(command, label, **kwargs):
                calls.append((command, label))
                return str(native if label == 'guest-threads' else managed) + '\n'
            ctx = SimpleNamespace(root=root, artifacts=artifacts, receipt={}, save=lambda: None, run=run)
            with patch.object(campaign, 'helper', return_value=SimpleNamespace(load_delivery=lambda path: {})):
                campaign.run_product_gate(ctx, SimpleNamespace(name='guest-threads', timeout=30))
            self.assertEqual([label for _, label in calls], ['guest-threads', 'guest-threads-managed'])
            self.assertEqual(len(calls[0][0]), 2)
            managed_command = calls[1][0]
            self.assertEqual(managed_command[managed_command.index('--delivery-receipt') + 1], producer)
            self.assertEqual(managed_command[managed_command.index('--delivery-sha256') + 1], digest(producer))
            self.assertEqual(ctx.receipt['product_gate_reports']['guest-threads'], str(managed / 'receipt.json'))


if __name__ == '__main__':
    unittest.main(verbosity=2)
