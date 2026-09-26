#!/usr/bin/env python3
"""Exercise source switching without compiling or changing the active product."""
import hashlib
import importlib.util
import io
import json
import os
from pathlib import Path
import shutil
import runpy
import subprocess
import sys
import tarfile
import tempfile
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[1]


def load(name):
    spec = importlib.util.spec_from_file_location(name, ROOT / 'scripts' / (name + '.py'))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class SourceSelectionTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        for name in ('scripts', 'config/managed-host', 'src/Host', 'ref'):
            (self.root / name).mkdir(parents=True)
        for name in ('stage-product.py', 'source_inputs.py', 'inventory.py', 'inventory-upstream-tests.py'):
            shutil.copy2(ROOT / 'scripts' / name, self.root / 'scripts' / name)
        self.overlay = json.loads((ROOT / 'config/managed-host/overlay.json').read_text())
        (self.root / 'config/managed-host/overlay.json').write_text(json.dumps(self.overlay))
        for entry in self.overlay['overlays']:
            path = self.root / 'config/managed-host' / entry['overlay']
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text('/* reusable managed header */\n')
        (self.root / 'src/Host/host.c').write_text('void Host(void) {}\n')
        # A stale survey must not control staging or block a version switch.
        (self.root / 'config/source-inventory.json').write_text('{"units": [{"path": "removed.c", "sha256": "obsolete"}]}')
        self.fetch = load('fetch')
        self.fetch.ROOT = self.root
        self.native = load('native-oracle')
        self.native.ROOT = self.root

    def snapshot(self, revision, extra=False):
        spec = dict(commit=revision, directory='msquic-' + revision,
                    archive='msquic-' + revision + '.tar.gz', sha256='unused')
        reference = self.root / 'ref' / spec['directory']
        files = {
            'src/core/CMakeLists.txt': 'set(SOURCES\n first.c\n' + (' second.c\n' if extra else '') + ')\n',
            'src/core/first.c': f'int first(void) {{ return {int(extra)}; }}\n',
            'src/inc/msquic.h': 'typedef struct QUIC_API_TABLE {\n    QUIC_OPEN_FN Open;\n} QUIC_API_TABLE;\n',
            '.gitmodules': '[submodule "submodules/quictls"]\npath = submodules/quictls\nurl = https://github.com/quictls/openssl.git\n',
        }
        if extra:
            files['src/core/second.c'] = 'int second(void) { return 2; }\n'
        for name in ('crypt', 'hashtable', 'pcp', 'platform_worker', 'toeplitz'):
            files['src/platform/' + name + '.c'] = '/* ' + revision + ' */\n'
        for entry in self.overlay['overlays']:
            files[entry['source']] = '/* upstream header ' + revision + ' */\n'
        for key in ('portable_fragment', 'route_fragment'):
            fragment = self.overlay[key]
            files[fragment['source']] = (fragment['start'] + '\n/* body ' + revision + ' */\n' + fragment['end'] + '\n')
        for name, contents in files.items():
            path = reference / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(contents)
        archive = self.root / 'ref' / spec['archive']
        with tarfile.open(archive, 'w:gz') as package:
            package.add(reference, arcname=spec['directory'])
        spec['sha256'] = hashlib.sha256(archive.read_bytes()).hexdigest()
        spec['url'] = archive.as_uri()
        return spec

    def stage(self, spec):
        (self.root / 'config/source.json').write_text(json.dumps(spec))
        subprocess.run([sys.executable, self.root / 'scripts/stage-product.py', '--no-fetch'],
                       check=True, capture_output=True, text=True)
        return json.loads((self.root / 'build/product-source/manifest.json').read_text())

    def test_switch_reuses_headers_and_refreshes_units_and_fragments(self):
        first, second = self.snapshot('first'), self.snapshot('second', extra=True)
        first_manifest = self.stage(first)
        self.assertNotIn('src/core/second.c', first_manifest['units'])
        second_manifest = self.stage(second)
        self.assertIn('src/core/second.c', second_manifest['units'])
        self.assertNotIn('src/platform/pcp.c', second_manifest['units'])
        stage = self.root / 'build/product-source'
        self.assertIn('return 1', (stage / 'src/core/first.c').read_text())
        for entry in self.overlay['overlays']:
            self.assertEqual((stage / entry['source']).read_text(), '/* reusable managed header */\n')
        for filename in ('portable.c', 'route.c'):
            self.assertIn('body second', (stage / 'host' / filename).read_text())
            self.assertNotIn('body first', (stage / 'host' / filename).read_text())
        self.stage(first)
        self.assertFalse((stage / 'src/core/second.c').exists())
        self.assertIn('body first', (stage / 'host/portable.c').read_text())

    def test_missing_fragment_boundary_is_reported(self):
        spec = self.snapshot('first')
        source = self.root / 'ref' / spec['directory'] / self.overlay['route_fragment']['source']
        source.write_text('/* function removed upstream */\n')
        with self.assertRaises(subprocess.CalledProcessError) as failure:
            self.stage(spec)
        self.assertIn('Cannot locate portable function boundaries', failure.exception.stderr)

    def test_select_main_stable_tag_and_commit_refreshes_manifests(self):
        first, second = self.snapshot('first'), self.snapshot('second', extra=True)
        for selector, spec in [('main', first), ('stable', second), ('v2.6.1', second), ('first', first)]:
            revision = spec['commit']
            dependency = self.root / 'ref' / f'quictls-tls-{revision}.tar.gz'
            dependency.write_bytes(('tls-' + revision).encode())

            def api(path):
                if path == '/releases/latest':
                    return {'tag_name': 'v2.6.1'}
                if path.startswith('/commits/'):
                    return {'sha': revision, 'commit': {'committer': {'date': '2026-09-25T00:00:00Z'}, 'message': 'fixture\nbody'}}
                if path == f'/git/trees/{revision}?recursive=1':
                    return {'tree': [{'path': 'submodules/quictls', 'type': 'commit', 'sha': 'tls-' + revision}]}
                self.fail('Unexpected API request: ' + path)

            with patch.object(self.fetch, 'github', side_effect=api) as github, \
                    patch.object(sys, 'argv', ['fetch.py', '--ref', selector]), \
                    patch('sys.stdout', new_callable=io.StringIO):
                self.fetch.main()
            selected = json.loads((self.root / 'config/source.json').read_text())
            self.assertEqual(selected['commit'], revision)
            self.assertEqual(selected['ref'], 'v2.6.1' if selector == 'stable' else selector)
            requested = [call.args[0] for call in github.call_args_list]
            self.assertIn('/commits/' + selected['ref'], requested)
            native = json.loads((self.root / 'config/native-inputs.json').read_text())
            self.assertEqual(native['quictls']['revision'], 'tls-' + revision)
            self.assertEqual(native['quictls']['sha256'], hashlib.sha256(dependency.read_bytes()).hexdigest())
            inventory = json.loads((self.root / 'config/source-inventory.json').read_text())
            self.assertEqual(inventory['revision'], revision)
            self.assertEqual('src/core/second.c' in [unit['path'] for unit in inventory['units']], revision == 'second')
            self.assertEqual(json.loads((self.root / 'config/upstream-test-inventory.json').read_text())['commit'], revision)

    def test_failed_dependency_resolution_keeps_active_selection(self):
        first, second = self.snapshot('first'), self.snapshot('second')
        pin = self.root / 'config/source.json'
        pin.write_text(json.dumps(first))
        with patch.object(self.fetch, 'resolve_source', return_value=second), \
                patch.object(self.fetch, 'resolve_native', side_effect=RuntimeError('download failed')), \
                patch.object(sys, 'argv', ['fetch.py', '--ref', 'stable']):
            with self.assertRaisesRegex(RuntimeError, 'download failed'):
                self.fetch.main()
        self.assertEqual(json.loads(pin.read_text()), first)

    def test_native_worktree_rebuilt_for_source_or_tls_change(self):
        first, second = self.snapshot('first'), self.snapshot('second', extra=True)
        native = {'quictls': {'revision': 'tls-first'}}
        work, build = self.native.prepare_worktree(first, native)
        build.mkdir()
        (build / 'old-binary').touch()
        (work / 'generated-output').touch()
        self.native.prepare_worktree(first, native)
        self.assertTrue((build / 'old-binary').exists())
        self.assertTrue((work / 'generated-output').exists())
        self.native.prepare_worktree(second, native)
        self.assertFalse(build.exists())
        self.assertFalse((work / 'generated-output').exists())
        self.assertTrue((work / 'src/core/second.c').exists())
        build.mkdir()
        (work / 'tls-old').touch()
        self.native.prepare_worktree(second, {'quictls': {'revision': 'tls-second'}})
        self.assertFalse(build.exists())
        self.assertFalse((work / 'tls-old').exists())

    def test_translate_no_fetch_routes_full_and_fast_modes(self):
        repository = self.root / 'wrapper-repository'
        campaign = repository / 'msquic'
        (campaign / 'scripts').mkdir(parents=True)
        (repository / 'Scripts').mkdir()
        for name in ('translate.sh', 'common.sh'):
            shutil.copy2(ROOT / 'scripts' / name, campaign / 'scripts' / name)
        shutil.copy2(ROOT.parent / 'Scripts/campaign-common.sh', repository / 'Scripts/campaign-common.sh')
        commands = self.root / 'commands.jsonl'
        binary = self.root / 'bin'
        binary.mkdir()
        # Intercept orchestration only; no compiler or network runs in this test.
        launcher = binary / 'python3'
        launcher.write_text('#!' + sys.executable + '\nimport json, sys\n'
                            'if sys.argv[1] != "-c":\n'
                            '    with open(' + repr(str(commands)) + ', "a") as log:\n'
                            '        log.write(json.dumps(sys.argv[1:]) + "\\n")\n')
        launcher.chmod(0o755)
        for flags in ([], ['--no-fetch'], ['--fast', '--no-fetch']):
            commands.write_text('')
            subprocess.run(['bash', campaign / 'scripts/translate.sh', '--no-build-tools', *flags],
                           check=True, env={**os.environ, 'PATH': str(binary) + os.pathsep + os.environ['PATH']})
            calls = [json.loads(line) for line in commands.read_text().splitlines()]
            self.assertEqual(len(calls), 1)
            self.assertEqual(Path(calls[0][0]).name, 'campaign.py')
            self.assertEqual(calls[0][1:], ['translate', 'msquic', '--no-build-tools', *flags])

    def test_host_no_fetch_reaches_real_staging_without_archive(self):
        spec = self.snapshot('local')
        (self.root / 'ref' / spec['archive']).unlink()
        (self.root / 'config/source.json').write_text(json.dumps(spec))
        shutil.copy2(ROOT / 'scripts/test-host-contract.py', self.root / 'scripts/test-host-contract.py')
        (self.root / 'scripts/fetch.py').write_text('raise RuntimeError("Unexpected fetch")\n')
        real_run = subprocess.run

        class Staged(BaseException):
            pass

        def run(command, **kwargs):
            if Path(command[1]).name == 'generate-host-contract.py':
                return subprocess.CompletedProcess(command, 0, '', '')
            self.assertEqual(Path(command[1]).name, 'stage-product.py')
            self.assertIn('--no-fetch', command)
            result = real_run(command, **kwargs)
            self.assertEqual(result.returncode, 0, result.stderr)
            raise Staged()

        with patch.object(subprocess, 'run', side_effect=run), \
                patch.object(sys, 'argv', ['test-host-contract.py', '--no-fetch']), \
                self.assertRaises(Staged):
            runpy.run_path(str(self.root / 'scripts/test-host-contract.py'), run_name='__main__')
        self.assertTrue((self.root / 'build/product-source/host/portable.c').is_file())

    def test_abi_local_source_mode_needs_no_archive_and_records_inputs(self):
        pin = self.snapshot('local')
        (self.root / 'ref' / pin['archive']).unlink()
        (self.root / 'config/source.json').write_text(json.dumps(pin))
        (self.root / 'config/dotcc-overrides.json').write_text('{}')
        shutil.copy2(ROOT / 'scripts/test-abi.py', self.root / 'scripts/test-abi.py')
        spec = importlib.util.spec_from_file_location('offline_abi', self.root / 'scripts/test-abi.py')
        abi = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(abi)
        with patch.object(abi, 'verify_reference', side_effect=AssertionError('Unexpected archive verification')), \
                patch.object(abi.subprocess, 'run', side_effect=lambda command, **kwargs:
                             subprocess.CompletedProcess(command, 0, 'layout fixture 4\n', '')), \
                patch.object(sys, 'argv', ['test-abi.py', '--no-fetch', '--native-only', '--groups', 'public']), \
                patch.object(abi.os, 'sched_setaffinity'), patch('sys.stdout', new_callable=io.StringIO):
            self.assertEqual(abi.main(), 0)
        report = json.loads((self.root / 'artifacts/abi/results.json').read_text())
        self.assertEqual(report['reference_verification'], 'local-source')
        self.assertEqual(report['verified_reference_files'], 0)
        self.assertTrue(report['reference_stable'])
        self.assertTrue(report['local_reference_sha256'])
        header = self.root / 'ref' / pin['directory'] / 'src/inc/msquic.h'
        header.write_text(header.read_text() + '/* changed */\n')
        self.assertNotEqual(report['local_reference_sha256'], abi.local_reference_hashes())

    def test_download_hash_policy(self):
        archive = self.root / 'ref/archive.tar.gz'
        archive.write_bytes(b'cached archive')
        with patch.dict(os.environ, DOTCC_CAMPAIGN_HASHES='warn'), patch('sys.stderr', new_callable=io.StringIO) as warnings:
            self.fetch.download('unused', archive, expected='incorrect')
            self.assertIn('WARNING', warnings.getvalue())
        with patch.dict(os.environ, DOTCC_CAMPAIGN_HASHES='strict'):
            with self.assertRaisesRegex(RuntimeError, 'Archive checksum mismatch'):
                self.fetch.download('unused', archive, expected='incorrect')


if __name__ == '__main__':
    unittest.main()
