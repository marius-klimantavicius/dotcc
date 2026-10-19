"""Pure source policy controls: no build or native execution."""
import copy
import importlib.util
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
spec = importlib.util.spec_from_file_location('r37_pyconfig', ROOT / 'scripts/pyconfig.py')
policy = importlib.util.module_from_spec(spec)
spec.loader.exec_module(policy)
DOCUMENT = json.loads((ROOT / 'config/pyconfig-policy.json').read_text())


class FeaturePolicyTests(unittest.TestCase):
    def test_existing_profiles_resolve(self):
        for profile in ('embedded-gil', 'upstream-tests-static'):
            resolved = policy.resolve(DOCUMENT, profile)
            self.assertIn('HAVE_PIPE', policy.UNIFORM_SERVICE_FEATURES)
            self.assertIn('HAVE_SYMLINK', policy.UNIFORM_SERVICE_FEATURES)
            self.assertEqual(resolved['values']['HAVE_LSTAT'], '1')
            self.assertEqual(resolved['values']['HAVE_READLINK'], '1')

    def test_defined_zero_is_not_absence(self):
        for profile in ('embedded-gil', 'upstream-tests-static'):
            for name in ('HAVE_PIPE', 'HAVE_SYMLINK', 'HAVE_READLINK', 'HAVE_LSTAT'):
                document = copy.deepcopy(DOCUMENT)
                document['values'][name] = '0'
                with self.subTest(profile=profile, feature=name), self.assertRaisesRegex(RuntimeError, 'must be 1'):
                    policy.resolve(document, profile)

    def test_symlink_requires_readlink_and_lstat(self):
        for missing in ('HAVE_READLINK', 'HAVE_LSTAT'):
            document = copy.deepcopy(DOCUMENT)
            document['values']['HAVE_SYMLINK'] = '1'
            document['values'].pop(missing, None)
            with self.subTest(missing=missing), self.assertRaisesRegex(RuntimeError, 'Incomplete symlink'):
                policy.resolve(document, 'embedded-gil')

    def test_every_common_and_unit_option_spelling_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            header = Path(directory) / 'pyconfig.h'
            for profile in ('embedded-gil', 'upstream-tests-static'):
                resolved = policy.resolve(DOCUMENT, profile)
                header.write_text(policy.render(resolved))
                ctx = SimpleNamespace(root=ROOT, profile=profile, receipt={})
                for name in ('HAVE_PIPE', 'HAVE_SYMLINK', 'HAVE_READLINK', 'HAVE_LSTAT'):
                    for options in (['-D'+name], ['-D'+name+'=0'], ['-D', name+'=1'], ['-U'+name], ['-U', name]):
                        for common in (True, False):
                            unit = SimpleNamespace(path=Path('real-unit.c'), flags=() if common else tuple(options))
                            with self.subTest(profile=profile, name=name, options=options, common=common):
                                with self.assertRaisesRegex(RuntimeError, 'override of profile feature'):
                                    policy.bind(ctx, resolved, header, [unit], options if common else [])

    def test_wrong_header_rejected_before_unit_binding(self):
        with tempfile.TemporaryDirectory() as directory:
            header = Path(directory) / 'pyconfig.h'
            resolved = policy.resolve(DOCUMENT, 'embedded-gil')
            header.write_text(policy.render(resolved) + '#define HAVE_PIPE 1\n')
            ctx = SimpleNamespace(root=ROOT, profile='embedded-gil', receipt={})
            with self.assertRaisesRegex(RuntimeError, 'header differs'):
                policy.bind(ctx, resolved, header, [], [])


if __name__ == '__main__':
    unittest.main()
