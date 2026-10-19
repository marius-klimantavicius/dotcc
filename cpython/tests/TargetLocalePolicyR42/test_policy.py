"""Whole-target locale policy guards; pure controls, no build or locks."""
import copy
import importlib.util
import json
import os
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest

CPY = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(CPY.parent / 'Scripts'))
spec = importlib.util.spec_from_file_location('target_locale_policy', CPY / 'scripts/pyconfig.py')
policy = importlib.util.module_from_spec(spec)
spec.loader.exec_module(policy)
DOCUMENT = json.loads(Path(os.environ.get('DOTCC_R42_POLICY', CPY / 'config/pyconfig-policy.json')).read_text())
FEATURES = ('_Py_FORCE_UTF8_LOCALE', '_Py_FORCE_UTF8_FS_ENCODING')
PROFILES = ('embedded-gil', 'upstream-tests-static')


class TargetLocalePolicyTests(unittest.TestCase):
    def test_whole_contract_and_upstream_filesystem_derivation(self):
        for profile in PROFILES:
            resolved = policy.resolve(DOCUMENT, profile)
            self.assertEqual(resolved['values']['SIZEOF_WCHAR_T'], '2')
            self.assertEqual(resolved['values'][FEATURES[0]], '1')
            self.assertNotIn(FEATURES[1], resolved['values'])
            self.assertNotIn(FEATURES[1], resolved['undefined'])
            self.assertTrue(resolved['target_locale_contract'])

    def test_no_filesystem_only_or_disabled_contract(self):
        for profile in PROFILES:
            for value in (None, '0', ''):
                document = copy.deepcopy(DOCUMENT)
                if value is None:
                    document['values'].pop(FEATURES[0])
                else:
                    document['values'][FEATURES[0]] = value
                with self.subTest(profile=profile, value=value), self.assertRaises(RuntimeError):
                    policy.resolve(document, profile)

    def test_no_profile_can_disable_common_contract(self):
        for profile in PROFILES:
            for section, value in (('values', '0'), ('undefined', 'disabled')):
                document = copy.deepcopy(DOCUMENT)
                document.setdefault('profiles', {}).setdefault(profile, {}).setdefault(section, {})[FEATURES[0]] = value
                with self.subTest(profile=profile, section=section), self.assertRaises(RuntimeError):
                    policy.resolve(document, profile)

    def test_no_direct_filesystem_macro_even_if_one(self):
        for name in ('values', 'undefined'):
            for value in ('0', '1', ''):
                document = copy.deepcopy(DOCUMENT)
                document[name][FEATURES[1]] = value
                with self.subTest(section=name, value=value), self.assertRaisesRegex(RuntimeError, 'derived'):
                    policy.resolve(document, 'embedded-gil')

    def test_no_wchar_abi_change_hidden_in_policy(self):
        document = copy.deepcopy(DOCUMENT)
        document['values']['SIZEOF_WCHAR_T'] = '4'
        with self.assertRaisesRegex(RuntimeError, 'wchar16'):
            policy.resolve(document, 'embedded-gil')

    def test_every_unit_common_and_per_unit_ordered_override(self):
        # All actual 193/286 units, including limited/core helpers, use the
        # production binder. Exercise a later cancelling -D/-U as well.
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / 'config').mkdir()
            (root / 'scripts').mkdir()
            (root / 'config/pyconfig-policy.json').write_text(json.dumps(DOCUMENT))
            (root / 'scripts/pyconfig.py').write_bytes((CPY / 'scripts/pyconfig.py').read_bytes())
            header = root / 'pyconfig.h'
            for profile in PROFILES:
                publication = json.loads((CPY / ('artifacts/owner-a-r32-20261018/final-' + profile + '-publication.json')).read_text())
                receipt = json.loads(Path(publication['receipt']['path']).read_text())
                rows = receipt['profile_feature_policy']['units']
                self.assertEqual(len(rows), 193 if profile == 'embedded-gil' else 286)
                resolved = policy.resolve(DOCUMENT, profile)
                header.write_text(policy.render(resolved))
                ctx = SimpleNamespace(root=root, profile=profile, receipt={})
                units = [SimpleNamespace(path=Path(row['path']), flags=tuple(row['ordered_options'])) for row in rows]
                policy.bind(ctx, resolved, header, units, [])
                self.assertEqual(ctx.receipt['profile_feature_policy']['derived_features'][FEATURES[1]]['source'], 'Include/pyport.h')
                for unit in units:
                    for name in FEATURES:
                        for options in (['-D'+name], ['-D'+name+'=0'], ['-D', name+'=1'],
                                        ['-U'+name], ['-U', name], ['-D'+name+'=1', '-U'+name]):
                            for common in (True, False):
                                changed = SimpleNamespace(path=unit.path, flags=unit.flags + (() if common else tuple(options)))
                                with self.subTest(profile=profile, path=str(unit.path), name=name, options=options, common=common):
                                    with self.assertRaisesRegex(RuntimeError, 'override of profile feature'):
                                        policy.bind(ctx, resolved, header, [changed], options if common else [])
                header.write_text(policy.render(resolved) + '#undef _Py_FORCE_UTF8_LOCALE\n')
                with self.assertRaisesRegex(RuntimeError, 'header differs'):
                    policy.bind(ctx, resolved, header, units, [])


if __name__ == '__main__':
    unittest.main()
