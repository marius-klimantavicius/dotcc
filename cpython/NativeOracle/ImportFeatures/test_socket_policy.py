"""Pure socket source-policy controls; no compiler/native/product execution."""
import copy
import json
import os
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.model import Unit
from campaigns.recipes import helper

POLICY = helper(Path(os.environ.get('SOCKET_POLICY_RESOLVER', ROOT / 'scripts/pyconfig.py')))
FEATURES = ('HAVE_SOCKET', 'HAVE_BIND', 'HAVE_LISTEN', 'HAVE_CONNECT',
            'HAVE_ACCEPT', 'HAVE_GETSOCKNAME', 'HAVE_GETPEERNAME', 'HAVE_GETADDRINFO',
            'HAVE_NETDB_H', 'HAVE_ADDRINFO', 'HAVE_SOCKADDR_STORAGE', 'HAVE_GETNAMEINFO',
            'HAVE_GETHOSTBYNAME')
PROFILES = ('embedded-gil', 'upstream-tests-static')


class SocketPolicy(unittest.TestCase):
    def setUp(self):
        self.document = json.loads((ROOT / 'config/pyconfig-policy.json').read_text())

    def binding(self, profile, *, common=(), unit=(), edit=None, document=None):
        document = self.document if document is None else document
        effective = POLICY.resolve(document, profile)
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / 'config').mkdir()
            (root / 'config/pyconfig-policy.json').write_text(json.dumps(document))
            header = root / 'pyconfig.h'
            text = POLICY.render(effective)
            header.write_text(text if edit is None else edit(text))
            ctx = SimpleNamespace(root=root, profile=profile, receipt={})
            (root / 'scripts').mkdir()
            (root / 'scripts/pyconfig.py').write_bytes((ROOT / 'scripts/pyconfig.py').read_bytes())
            POLICY.bind(ctx, effective, header, [Unit(Path('selected-source.c'), unit)], common)
            return ctx.receipt['profile_feature_policy']

    def test_common_socket_and_complete_test_only_dependency_family(self):
        before = copy.deepcopy(self.document)
        embedded = POLICY.resolve(self.document, PROFILES[0])
        tests = POLICY.resolve(self.document, PROFILES[1])
        self.assertEqual(embedded['values']['HAVE_SOCKET'], '1')
        for feature in FEATURES[1:]:
            self.assertNotIn(feature, embedded['values'])
        self.assertEqual(tests['values'], {**embedded['values'], 'HAVE_FORK': '1',
                         'HAVE_WAITPID': '1', 'HAVE_SYS_WAIT_H': '1',
                         'HAVE_GETHOSTNAME': '1', **dict.fromkeys(FEATURES[1:], '1')})
        self.assertEqual(self.document, before)
        for policy in (embedded, tests):
            for name in ('HAVE_SOCKETPAIR', 'HAVE_ACCEPT4', 'HAVE_UNAME', 'HAVE_GETPPID',
                         'HAVE_GETGROUPS', 'HAVE_KILL', 'HAVE_SETHOSTNAME',
                         'HAVE_WCSFTIME'):
                self.assertNotIn(name, policy['values'])

    def test_selected_and_unselected_common_and_unit_overrides_reject(self):
        for feature in FEATURES:
            spellings = [('-D' + feature,), ('-D', feature), ('-D' + feature + '=1',),
                         ('-D', feature + '=1'), ('-D' + feature + '=0',),
                         ('-D', feature + '=0'), ('-U' + feature,), ('-U', feature)]
            for profile in PROFILES:
                for options in spellings:
                    for place in ('common', 'unit'):
                        with self.subTest(feature=feature, profile=profile, options=options, place=place):
                            with self.assertRaisesRegex(RuntimeError, 'override of profile feature'):
                                self.binding(profile, **{place: options})

    def test_defined_zero_is_never_an_absent_ifdef_service(self):
        for feature in FEATURES:
            for profile in PROFILES:
                document = copy.deepcopy(self.document)
                values = document['values'] if profile == PROFILES[0] else document['profiles'][profile]['values']
                values[feature] = '0'
                with self.subTest(feature=feature, profile=profile), self.assertRaisesRegex(RuntimeError, 'must be 1'):
                    POLICY.resolve(document, profile)

    def test_partial_fallback_methods_reject(self):
        for feature in FEATURES:
            document = copy.deepcopy(self.document)
            # Synthetic complete policy tests rejection only; no service is claimed.
            document['profiles'][PROFILES[1]]['values'].update(dict.fromkeys(FEATURES[1:], '1'))
            document['values'].pop(feature, None)
            document['profiles'][PROFILES[1]]['values'].pop(feature, None)
            with self.subTest(feature=feature), self.assertRaisesRegex(RuntimeError, 'Incomplete local socket'):
                POLICY.resolve(document, PROFILES[1])

    def test_absent_everywhere_still_cannot_be_selected_in_one_unit(self):
        document = copy.deepcopy(self.document)
        for feature in FEATURES:
            document['values'].pop(feature, None)
            document['profiles'][PROFILES[1]]['values'].pop(feature, None)
        for feature in FEATURES:
            for profile in PROFILES:
                with self.subTest(feature=feature, profile=profile), self.assertRaisesRegex(RuntimeError, 'override of profile feature'):
                    self.binding(profile, document=document, unit=('-D', feature + '=1'))

    def complete_document(self):
        document = copy.deepcopy(self.document)
        document['profiles'][PROFILES[1]]['values'].update(dict.fromkeys(FEATURES[1:], '1'))
        return document

    def test_selected_header_cannot_drop_zero_or_duplicate_each_feature(self):
        for feature in FEATURES:
            for edit in [lambda text: text.replace('#define ' + feature + ' 1\n', ''),
                         lambda text: text.replace('#define ' + feature + ' 1', '#define ' + feature + ' 0'),
                         lambda text: text + '\n#define ' + feature + ' 1\n']:
                with self.subTest(feature=feature, edit=edit), self.assertRaisesRegex(RuntimeError, 'header differs'):
                    self.binding(PROFILES[1], edit=edit, document=self.complete_document())

    def test_unselected_embedded_header_cannot_add_test_only_methods(self):
        for feature in FEATURES[1:]:
            for value in ('0', '1'):
                with self.subTest(feature=feature, value=value), self.assertRaisesRegex(RuntimeError, 'header differs'):
                    self.binding(PROFILES[0], edit=lambda text: text + '\n#define ' + feature + ' ' + value + '\n')

    def test_receipt_records_common_selected_and_unselected_ordered_options(self):
        for profile in PROFILES:
            row = self.binding(profile, common=('-DUSE_COMPUTED_GOTOS=0',),
                               unit=('-UPy_BUILD_CORE', '-DPy_BUILD_CORE_BUILTIN'))
            for feature in FEATURES:
                value = '1' if feature == 'HAVE_SOCKET' or profile == PROFILES[1] else None
                self.assertIn(feature, row['uniform_features'])
                self.assertEqual(row['uniform_features'].get(feature), value)
                if feature == 'HAVE_SOCKET' or value is None:
                    self.assertNotIn(feature, row['selected_features'])
                else:
                    self.assertEqual(row['selected_features'][feature], value)
            self.assertEqual(row['units'][0]['ordered_options'],
                             ['-DUSE_COMPUTED_GOTOS=0', '-UPy_BUILD_CORE', '-DPy_BUILD_CORE_BUILTIN'])

    def test_sysconfig_uses_same_effective_policy_for_both_profiles(self):
        generator = helper(ROOT / 'scripts/test_profile.py')
        for profile in PROFILES:
            with self.subTest(profile=profile), tempfile.TemporaryDirectory() as directory:
                target = Path(directory)
                (target / 'stdlib').mkdir()
                path = generator.sysconfig_data(SimpleNamespace(root=ROOT, profile=profile), target)
                namespace = {'__file__': str(path)}
                exec(compile(path.read_text(), str(path), 'exec'), namespace)
                values = namespace['build_time_vars']
                self.assertEqual(values['HAVE_SOCKET'], 1)
                for feature in FEATURES[1:]:
                    if profile == PROFILES[1]:
                        self.assertEqual(values[feature], 1)
                    else:
                        self.assertNotIn(feature, values)
                for feature in ('HAVE_SOCKETPAIR', 'HAVE_ACCEPT4'):
                    self.assertNotIn(feature, values)

    def test_missing_split_argument_rejects(self):
        for option in ('-D', '-U'):
            with self.subTest(option=option), self.assertRaisesRegex(RuntimeError, 'Missing feature option'):
                self.binding(PROFILES[1], unit=(option,))


if __name__ == '__main__':
    unittest.main(verbosity=2)
