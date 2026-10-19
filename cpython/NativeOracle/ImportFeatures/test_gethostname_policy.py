"""Prepared selected/unselected guards; no native/compiler/product execution."""
from pathlib import Path
from types import SimpleNamespace
import copy
import json
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.model import Unit
from campaigns.recipes import helper

POLICY = helper(ROOT / 'scripts/pyconfig.py')

SOCKET_DEPENDENCIES = ('HAVE_BIND', 'HAVE_LISTEN', 'HAVE_CONNECT', 'HAVE_ACCEPT',
    'HAVE_GETSOCKNAME', 'HAVE_GETPEERNAME', 'HAVE_GETADDRINFO', 'HAVE_NETDB_H',
    'HAVE_ADDRINFO', 'HAVE_SOCKADDR_STORAGE', 'HAVE_GETNAMEINFO', 'HAVE_GETHOSTBYNAME')
FEATURE = 'HAVE_GETHOSTNAME'
PROFILES = ('embedded-gil', 'upstream-tests-static')


class HostnamePolicy(unittest.TestCase):
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
            # bind records the resolver's bytes as well as the policy's bytes.
            (root / 'scripts').mkdir()
            (root / 'scripts/pyconfig.py').write_bytes((ROOT / 'scripts/pyconfig.py').read_bytes())
            POLICY.bind(ctx, effective, header, [Unit(Path('selected-source.c'), unit)], common)
            return ctx.receipt['profile_feature_policy']

    def test_only_actual_selected_socket_profile_enables_hostname(self):
        before = copy.deepcopy(self.document)
        embedded = POLICY.resolve(self.document, PROFILES[0])
        tests = POLICY.resolve(self.document, PROFILES[1])
        self.assertNotIn(FEATURE, embedded['values'])
        self.assertEqual(tests['values'], {**embedded['values'], 'HAVE_FORK': '1',
                         'HAVE_WAITPID': '1', 'HAVE_SYS_WAIT_H': '1', FEATURE: '1', **dict.fromkeys(SOCKET_DEPENDENCIES, '1')})
        self.assertEqual(self.document, before)
        for policy in (embedded, tests):
            self.assertEqual(policy['values']['HAVE_GETPID'], '1')
            self.assertEqual(policy['values']['HAVE_STRFTIME'], '1')
            for name in ('HAVE_UNAME', 'HAVE_GETPPID', 'HAVE_GETGROUPS', 'HAVE_KILL',
                         'HAVE_SETHOSTNAME', 'HAVE_WCSFTIME'):
                self.assertNotIn(name, policy['values'])

    def test_selected_and_unselected_common_and_unit_overrides_reject(self):
        spellings = [('-D' + FEATURE,), ('-D', FEATURE), ('-D' + FEATURE + '=1',),
                     ('-D', FEATURE + '=1'), ('-D' + FEATURE + '=0',),
                     ('-D', FEATURE + '=0'), ('-U' + FEATURE,), ('-U', FEATURE)]
        for profile in PROFILES:
            for options in spellings:
                for place in ('common', 'unit'):
                    with self.subTest(profile=profile, options=options, place=place):
                        with self.assertRaisesRegex(RuntimeError, 'override of profile feature'):
                            self.binding(profile, **{place: options})

    def test_defined_zero_selects_ifdef_and_must_be_rejected(self):
        for section, profile in [('values', 'embedded-gil'), ('overlay', 'upstream-tests-static')]:
            document = copy.deepcopy(self.document)
            values = document['values'] if section == 'values' else document['profiles'][profile]['values']
            values[FEATURE] = '0'
            with self.subTest(profile=profile), self.assertRaisesRegex(RuntimeError, 'must be 1'):
                POLICY.resolve(document, profile)

    def test_absent_everywhere_still_cannot_be_added_in_one_unit(self):
        document = copy.deepcopy(self.document)
        document['profiles']['upstream-tests-static']['values'].pop(FEATURE)
        for profile in PROFILES:
            with self.subTest(profile=profile), self.assertRaisesRegex(RuntimeError, 'override of profile feature'):
                self.binding(profile, document=document, unit=('-D', FEATURE + '=1'))

    def test_selected_header_cannot_drop_zero_or_duplicate_the_feature(self):
        for edit in [lambda text: text.replace('#define ' + FEATURE + ' 1\n', ''),
                     lambda text: text.replace('#define ' + FEATURE + ' 1', '#define ' + FEATURE + ' 0'),
                     lambda text: text + '\n#define ' + FEATURE + ' 1\n']:
            with self.subTest(edit=edit), self.assertRaisesRegex(RuntimeError, 'header differs'):
                self.binding(PROFILES[1], edit=edit)

    def test_unselected_header_cannot_silently_add_a_service(self):
        for value in ('0', '1'):
            with self.subTest(value=value), self.assertRaisesRegex(RuntimeError, 'header differs'):
                self.binding(PROFILES[0], edit=lambda text: text + '\n#define ' + FEATURE + ' ' + value + '\n')

    def test_binding_records_both_profiles_and_every_ordered_option(self):
        for profile, value in zip(PROFILES, (None, '1')):
            row = self.binding(profile, common=('-DUSE_COMPUTED_GOTOS=0',), unit=('-UPy_BUILD_CORE', '-DPy_BUILD_CORE_BUILTIN'))
            self.assertEqual(row['uniform_features'][FEATURE], value)
            if value is None:
                self.assertNotIn(FEATURE, row['selected_features'])
            else:
                self.assertEqual(row['selected_features'][FEATURE], value)
            self.assertEqual(row['units'][0]['ordered_options'],
                             ['-DUSE_COMPUTED_GOTOS=0', '-UPy_BUILD_CORE', '-DPy_BUILD_CORE_BUILTIN'])

    def test_missing_split_argument_rejects(self):
        for option in ('-D', '-U'):
            with self.subTest(option=option), self.assertRaisesRegex(RuntimeError, 'Missing feature option'):
                self.binding(PROFILES[1], unit=(option,))


if __name__ == '__main__':
    unittest.main(verbosity=2)
