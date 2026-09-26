#!/usr/bin/env python3
"""Source changes must invalidate caches, not require compatibility hash updates."""
import importlib.util
import json
from pathlib import Path
import shutil
import tempfile
import unittest

import core_inputs

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location('thread_stage', ROOT / 'src/UpstreamGuestThreads/stage.py')
THREADS = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(THREADS)


class SourceInputsTests(unittest.TestCase):
    def test_thread_headers_preserve_new_declarations(self):
        pthread = (ROOT.parent / 'DotCC.Lib/include/pthread.h').read_bytes()
        signal = (ROOT / 'config/managed-host/signal.h').read_bytes()
        addition = b'\nint newly_supported_runtime_function(void);\n'
        overlays = THREADS.overlay_headers(pthread + addition, signal)
        self.assertTrue(overlays['pthread.h'].endswith(addition))
        self.assertIn(b'#define pthread_create blink_unqualified_pthread_create', overlays['pthread.h'])
        self.assertIn(b'pthread_attr_getstacksize', overlays['pthread.h'])

    def test_header_text_replacements_still_require_unique_anchors(self):
        pthread = (ROOT.parent / 'DotCC.Lib/include/pthread.h').read_bytes()
        signal = (ROOT / 'config/managed-host/signal.h').read_bytes()
        with self.assertRaisesRegex(ValueError, 'Reviewed boundary differs'):
            THREADS.overlay_headers(pthread.replace(b'int pthread_create(', b'int renamed_create('), signal)

    def test_override_inputs_are_recorded_afresh_and_invalidate_emission_cache(self):
        with tempfile.TemporaryDirectory() as temporary:
            campaign = Path(temporary)
            config = campaign / 'config'
            config.mkdir()
            for name in ('source-manifest.json', 'semantic-intrinsics.json', 'managed-boundaries.json'):
                shutil.copyfile(ROOT / 'config' / name, config / name)
            upstream = campaign / 'ref' / json.loads((config / 'source-manifest.json').read_text())['upstream']['directory']
            (upstream / 'blink').mkdir(parents=True)
            for name in ('endian.h', 'machine.h', 'syscall.h', 'signal.h', 'syscall.c', 'memorymalloc.c'):
                (upstream / 'blink' / name).write_text('/* current source */\n')
            authored = campaign / 'src/Host/include'
            authored.mkdir(parents=True)
            (authored / 'host-guest-threads.h').write_text('/* current declarations */\n')
            (config / 'source-inventory.json').write_text('{}')
            (campaign / 'scripts').mkdir()
            for name in ('isolate-core.py', 'core_inputs.py'):
                (campaign / 'scripts' / name).write_text('')

            def stage(name):
                profile = campaign / name
                profile.mkdir()
                (profile / 'overrides.json').write_text('{}')
                core_inputs.stage_semantic_intrinsics(campaign, profile)
                core_inputs.stage_managed_boundaries(campaign, profile, instance_methods=True)
                inputs = dict(compiler={}, staged_headers={'overrides.json': 'test'},
                              upstream_inputs=core_inputs.upstream_identity(campaign))
                identity = core_inputs.emission_identity(profile, campaign, inputs,
                    dict(path='blink/syscall.c', sha256='test'))
                return (json.loads((profile / 'overrides.json').read_text()),
                        json.loads((profile / 'managed-boundaries.json').read_text()),
                        json.loads((profile / 'semantic-intrinsics.json').read_text()), identity)

            first = stage('first')
            for name in ('endian.h', 'machine.h', 'memorymalloc.c'):
                path = upstream / 'blink' / name
                path.write_text(path.read_text() + '/* unrelated source evolution */\n')
            second = stage('second')
            self.assertEqual(first[0], second[0])  # Same typed rules and selectors.
            self.assertNotEqual(first[1]['headers'], second[1]['headers'])
            self.assertNotEqual(first[1]['implementations'], second[1]['implementations'])
            self.assertNotEqual(first[2]['header_sha256'], second[2]['header_sha256'])
            self.assertNotEqual(first[3], second[3])


if __name__ == '__main__':
    unittest.main()
