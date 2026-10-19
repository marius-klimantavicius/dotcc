"""Pure preparation controls: no compiler/native tool, process lock or campaign.

Use the retained native generator outputs only as staged files. These tests
deliberately make no claim that a new feature profile compiled or executed.
"""
import copy
import hashlib
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent/'Scripts'))
from campaigns.model import Unit
from campaigns.recipes import helper

POLICY = helper(ROOT/'scripts/pyconfig.py')

SOCKET_DEPENDENCIES = ('HAVE_BIND', 'HAVE_LISTEN', 'HAVE_CONNECT', 'HAVE_ACCEPT',
    'HAVE_GETSOCKNAME', 'HAVE_GETPEERNAME', 'HAVE_GETADDRINFO', 'HAVE_NETDB_H',
    'HAVE_ADDRINFO', 'HAVE_SOCKADDR_STORAGE', 'HAVE_GETNAMEINFO', 'HAVE_GETHOSTBYNAME')


class ProfilePolicy(unittest.TestCase):
    def setUp(self):
        self.document = json.loads((ROOT/'config/pyconfig-policy.json').read_text())

    def test_embedded_header_has_only_reviewed_common_service_deltas(self):
        previous = ROOT/'build/owner-a-r8-20261018/before/cpython/config/pyconfig-policy.json'
        old = json.loads(previous.read_text())
        old['values']['HAVE_STRFTIME'] = '1'
        old['values']['HAVE_GETPID'] = '1'
        old['values']['HAVE_SOCKET'] = '1'
        self.assertEqual(POLICY.render(old), POLICY.render(POLICY.resolve(self.document, 'embedded-gil')))

    def test_only_test_feature_changes_values(self):
        before = POLICY.resolve(self.document, 'embedded-gil')
        after = POLICY.resolve(self.document, 'upstream-tests-static')
        self.assertNotIn('HAVE_FORK', before['values'])
        self.assertEqual(after['values'], {**before['values'], 'HAVE_FORK': '1',
                                           'HAVE_WAITPID': '1', 'HAVE_SYS_WAIT_H': '1', 'HAVE_GETHOSTNAME': '1', **dict.fromkeys(SOCKET_DEPENDENCIES, '1')})
        self.assertEqual(before['undefined'], after['undefined'])

    def test_no_document_or_other_profile_mutation(self):
        old = copy.deepcopy(self.document)
        selected = POLICY.resolve(self.document, 'upstream-tests-static')
        selected['values']['SIZEOF_WCHAR_T'] = '99'
        self.assertEqual(self.document, old)
        self.assertEqual(POLICY.resolve(self.document, 'embedded-gil')['values']['SIZEOF_WCHAR_T'], '2')

    def test_ambiguous_policy_rejects(self):
        self.document['profiles']['upstream-tests-static']['undefined'] = {'HAVE_FORK': 'invalid'}
        with self.assertRaisesRegex(RuntimeError, 'Ambiguous'):
            POLICY.resolve(self.document, 'upstream-tests-static')

    def test_unknown_profile_rejects(self):
        with self.assertRaisesRegex(RuntimeError, 'Unknown'):
            POLICY.resolve(self.document, 'typo')

    def test_unit_feature_overrides_reject_both_spellings(self):
        with tempfile.TemporaryDirectory() as tmp:
            header = Path(tmp)/'pyconfig.h'
            policy = POLICY.resolve(self.document, 'upstream-tests-static')
            header.write_text(POLICY.render(policy))
            for options in [('-UHAVE_FORK',), ('-D', 'HAVE_FORK=0'), ('-DHAVE_FORK=1',), ('-U', 'HAVE_FORK')]:
                ctx = SimpleNamespace(root=ROOT, profile='upstream-tests-static', receipt={})
                with self.subTest(options=options), self.assertRaisesRegex(RuntimeError, 'Per-unit override'):
                    POLICY.bind(ctx, policy, header, [Unit(Path('helper.c'), options)], [])

    def test_all_286_prepared_sources_roles_and_sysconfig(self):
        previous = json.loads((ROOT/'artifacts/campaign/20261018-010608-2ab6ce0b/receipt.json').read_text())
        build = Path(previous['native_generator']['path']).parent
        source = build.parent/'source'
        expected = json.loads((ROOT/'config/sources.json').read_text())
        prepare = helper(ROOT/'scripts/prepare.py')
        real_helper = prepare.helper

        def staged_helper(path):
            if path.name == 'native.py':
                return SimpleNamespace(ensure=lambda ctx: (build, source))
            if path.name == 'inventory.py':
                return SimpleNamespace(collect=lambda *args: expected)
            return real_helper(path)

        with tempfile.TemporaryDirectory() as tmp:
            work = Path(tmp)/'work'; work.mkdir()
            artifacts = Path(tmp)/'artifacts'; artifacts.mkdir()
            ctx = SimpleNamespace(root=ROOT, work=work, artifacts=artifacts, receipt={},
                                  profile='upstream-tests-static', sources={'product': Path(previous['sources']['product'])})
            prepare.helper = staged_helper
            try:
                translation = prepare.prepare(ctx)
            finally:
                prepare.helper = real_helper
            self.assertEqual(len(translation.units), 286)
            self.assertEqual(len({u.path for u in translation.units}), 286)
            expected_paths = [Path(p).relative_to(Path(previous['sources']['product']).parent.parent/'build/campaign'/previous['run_id']/'source').as_posix()
                              if '/source/' in p else p for p in previous['units']]
            actual_paths = [u.path.relative_to(work/'source').as_posix() if u.path.is_relative_to(work/'source') else str(u.path)
                            for u in translation.units]
            self.assertEqual(actual_paths, expected_paths)
            bound = ctx.receipt['profile_feature_policy']
            self.assertEqual(len(bound['units']), 286)
            self.assertEqual(bound['selected_features'], {'HAVE_FORK': '1',
                              'HAVE_WAITPID': '1', 'HAVE_SYS_WAIT_H': '1', 'HAVE_GETHOSTNAME': '1', **dict.fromkeys(SOCKET_DEPENDENCIES, '1')})
            self.assertEqual(bound['header_sha256'], hashlib.sha256((work/'config/pyconfig.h').read_bytes()).hexdigest())
            self.assertEqual(ctx.receipt['test_profile']['static_setup_modules'],
                             previous['test_profile']['static_setup_modules'])
            self.assertEqual((work/'source/Modules/config.c').read_bytes(),
                             (ROOT/'build/campaign'/previous['run_id']/'source/Modules/config.c').read_bytes())
            self.assertEqual(len(ctx.receipt['test_profile']['modules']), 30)
            for old, unit in zip(previous['translation_arguments']['common'][:1], translation.flags[:1]):
                self.assertEqual(old, unit)
            data = next((work/'runtime-assets/stdlib').glob('_sysconfigdata_*.py'))
            namespace = {'__file__': str(data)}
            exec(compile(data.read_text(), str(data), 'exec'), namespace)
            self.assertEqual(namespace['build_time_vars']['HAVE_FORK'], 1)
            for feature in ('HAVE_GETPID', 'HAVE_STRFTIME', 'HAVE_WAITPID', 'HAVE_SYS_WAIT_H', 'HAVE_GETHOSTNAME', 'HAVE_SOCKET'):
                self.assertEqual(namespace['build_time_vars'][feature], 1)
            self.assertNotIn('HAVE_WCSFTIME', namespace['build_time_vars'])
            self.assertEqual(namespace['build_time_vars']['SIZEOF_WCHAR_T'], 2)
            manifest = json.loads((work/'runtime-assets/assets.json').read_text())
            self.assertEqual(len(manifest['assets']), 2329)


if __name__ == '__main__':
    unittest.main(verbosity=2)
