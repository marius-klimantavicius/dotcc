"""Pure A10 feature-policy controls; no compiler, native process, lock or publish."""
import copy
import hashlib
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.model import Unit
from campaigns.recipes import helper

POLICY = helper(ROOT / 'scripts/pyconfig.py')

SOCKET_DEPENDENCIES = ('HAVE_BIND', 'HAVE_LISTEN', 'HAVE_CONNECT', 'HAVE_ACCEPT',
    'HAVE_GETSOCKNAME', 'HAVE_GETPEERNAME', 'HAVE_GETADDRINFO', 'HAVE_NETDB_H',
    'HAVE_ADDRINFO', 'HAVE_SOCKADDR_STORAGE', 'HAVE_GETNAMEINFO', 'HAVE_GETHOSTBYNAME')
FEATURES = ('HAVE_GETPID', 'HAVE_STRFTIME', 'HAVE_WAITPID', 'HAVE_SYS_WAIT_H')


def candidate(document):
    """The accepted selection, exercised privately until B's SOURCE-READY proof."""
    document = copy.deepcopy(document)
    document['values']['HAVE_STRFTIME'] = '1'
    document['values']['HAVE_GETPID'] = '1'
    document['profiles']['upstream-tests-static']['values'].update(
        HAVE_WAITPID='1', HAVE_SYS_WAIT_H='1')
    return document


class ImportFeaturePolicy(unittest.TestCase):
    def setUp(self):
        self.document = json.loads((ROOT / 'config/pyconfig-policy.json').read_text())

    def binding(self, *, profile='upstream-tests-static', common=(), unit=(), header_edit=None):
        policy = POLICY.resolve(self.document, profile)
        with tempfile.TemporaryDirectory() as temporary:
            header = Path(temporary) / 'pyconfig.h'
            text = POLICY.render(policy)
            header.write_text(text if header_edit is None else header_edit(text))
            ctx = SimpleNamespace(root=ROOT, profile=profile, receipt={})
            POLICY.bind(ctx, policy, header, [Unit(Path('actual-helper.c'), unit)], common)
            return ctx.receipt['profile_feature_policy']

    def test_import_features_cannot_differ_in_common_or_unit_options(self):
        for profile in ('embedded-gil', 'upstream-tests-static'):
            for feature in FEATURES:
                for options in [('-U' + feature,), ('-U', feature),
                                ('-D' + feature + '=1',), ('-D', feature + '=0')]:
                    for location in ('common', 'unit'):
                        with self.subTest(profile=profile, feature=feature, options=options, location=location):
                            with self.assertRaisesRegex(RuntimeError, 'override of profile feature'):
                                self.binding(profile=profile, **{location: options})

    def test_header_cannot_disagree_with_resolved_policy(self):
        for transform in [lambda s: s.replace('#define SIZEOF_LONG 8', '#define SIZEOF_LONG 4'),
                          lambda s: s.replace('#define HAVE_FORK 1\n', ''),
                          lambda s: s + '\n#define HAVE_STRFTIME 1\n']:
            with self.subTest(transform=transform), self.assertRaisesRegex(RuntimeError, 'header differs'):
                self.binding(header_edit=transform)

    def test_waitpid_and_status_header_are_one_import_contract(self):
        for feature in ('HAVE_WAITPID', 'HAVE_SYS_WAIT_H'):
            document = candidate(self.document)
            del document['profiles']['upstream-tests-static']['values'][feature]
            with self.subTest(missing=feature), self.assertRaisesRegex(RuntimeError, 'waitpid/status-header'):
                POLICY.resolve(document, 'upstream-tests-static')

    def test_zero_is_not_undefined_for_ifdef_selected_service(self):
        for feature in FEATURES:
            document = candidate(self.document)
            section = document['values'] if feature in ('HAVE_STRFTIME', 'HAVE_GETPID') else document['profiles']['upstream-tests-static']['values']
            section[feature] = '0'
            with self.subTest(feature=feature), self.assertRaisesRegex(RuntimeError, 'must be 1'):
                POLICY.resolve(document, 'upstream-tests-static')

    def test_wide_formatter_is_not_selected_without_shared_contract(self):
        document = candidate(self.document)
        document['undefined'].pop('HAVE_WCSFTIME', None)
        document['values']['HAVE_WCSFTIME'] = '1'
        with self.assertRaisesRegex(RuntimeError, 'wcsftime'):
            POLICY.resolve(document, 'upstream-tests-static')

    def test_missing_define_argument_fails_before_emission(self):
        for option in ('-D', '-U'):
            with self.subTest(option=option), self.assertRaisesRegex(RuntimeError, 'Missing feature option'):
                self.binding(unit=(option,))

    def test_other_upstream_roles_and_repeated_dispatch_option_remain_allowed(self):
        row = self.binding(common=('-DUSE_COMPUTED_GOTOS=0',), unit=('-DPy_BUILD_CORE_BUILTIN',))
        self.assertEqual(row['units'][0]['ordered_options'], ['-DUSE_COMPUTED_GOTOS=0', '-DPy_BUILD_CORE_BUILTIN'])

    def test_private_candidate_has_common_time_and_test_only_wait(self):
        document = candidate(self.document)
        before = copy.deepcopy(document)
        embedded = POLICY.resolve(document, 'embedded-gil')
        upstream = POLICY.resolve(document, 'upstream-tests-static')
        self.assertEqual(embedded['values']['HAVE_STRFTIME'], '1')
        self.assertNotIn('HAVE_WAITPID', embedded['values'])
        self.assertNotIn('HAVE_SYS_WAIT_H', embedded['values'])
        self.assertEqual(upstream['values'], {**embedded['values'], 'HAVE_FORK': '1',
                                             'HAVE_WAITPID': '1', 'HAVE_SYS_WAIT_H': '1', 'HAVE_GETHOSTNAME': '1', **dict.fromkeys(SOCKET_DEPENDENCIES, '1')})
        self.assertNotIn('HAVE_WCSFTIME', upstream['values'])
        for feature in ('SIZEOF_WCHAR_T', 'SIZEOF_LONG', 'SIZEOF_SIZE_T', 'WITH_PYMALLOC'):
            self.assertEqual(embedded['values'][feature], upstream['values'][feature])
        self.assertEqual(document, before)


if __name__ == '__main__':
    unittest.main(verbosity=2)
