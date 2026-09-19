#!/usr/bin/env python3
"""Test checkpoint preservation in fast translation without invoking dotnet."""
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch

SCRIPTS = Path(__file__).resolve().parent


class FastTranslationTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix='dotcc-fast-archive-test-')
        self.addCleanup(temporary.cleanup)
        self.root = root = Path(temporary.name) / 'msquic'
        for directory in ['scripts', 'config', 'build/product-source',
                          'generated/raw/TranslatedMsQuic', 'generated/TranslatedMsQuic']:
            (root / directory).mkdir(parents=True)
        shutil.copyfile(SCRIPTS / 'freeze-product.py', root / 'scripts/freeze-product.py')
        stage = root / 'build/product-source/manifest.json'
        stage.write_text(json.dumps(dict(defines=[], units=[])))
        (root / 'config/inline-exports.txt').write_text('test_inline\n')
        digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
        self.closure = dict(stage_manifest_sha256=digest(stage), evidence_sha256={}, compiler_hashes={},
                            generated_directories=dict(raw='generated/raw/TranslatedMsQuic',
                                                       optimized='generated/TranslatedMsQuic'), generated={})
        for variant, directory in self.closure['generated_directories'].items():
            folder = root / directory
            (folder / 'MsQuic.cs').write_text('// original output\n')
            (folder / 'Dotcc.SourceFiles.txt').write_text('MsQuic.cs\n')
            (folder / 'TranslatedMsQuic.csproj').write_text('<Project />')
            self.closure['generated'][variant] = {p.name: digest(p) for p in folder.iterdir()}
        (root / 'config/product-closure.json').write_text(json.dumps(self.closure))
        for path in ['DotCC/bin/Release/net10.0/dotcc.dll',
                     'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll']:
            tool = root.parent / path
            tool.parent.mkdir(parents=True)
            tool.touch()
        spec = importlib.util.spec_from_file_location('fast_translation', SCRIPTS / 'translate-fast.py')
        self.module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.module)
        self.module.ROOT, self.module.REPO = root, root.parent
        self.module.STAGE, self.module.BUILD = root / 'build/product-source', root / 'build/fast-translate'
        self.commands = []

    def qualify(self):
        receipt = self.root / 'artifacts/product-build/results.json'
        receipt.parent.mkdir(parents=True)
        receipt.write_text('{}')
        self.closure['evidence_sha256']['artifacts/product-build/results.json'] = hashlib.sha256(receipt.read_bytes()).hexdigest()
        (self.root / 'config/product-closure.json').write_text(json.dumps(self.closure))

    def archive(self):
        subprocess.run([sys.executable, self.root / 'scripts/freeze-product.py', '--archive-current'],
                       check=True, capture_output=True)

    def run_fast(self):
        def run(command):
            command = [str(p) for p in command]
            self.commands.append(command)
            if '--archive-current' in command:
                self.archive()
            elif '--emit=managedlib' in command:
                (self.root / 'generated/raw/TranslatedMsQuic/MsQuic.cs').write_text('// fast output\n')
        with patch.object(self.module, 'run', run), patch.object(sys, 'argv', ['translate-fast.py', '--jobs', '1']):
            self.module.main()

    def test_qualified_checkpoint_survives_fast_and_normal_regeneration(self):
        self.qualify()
        self.run_fast()
        self.assertIn('--archive-current', self.commands[0])
        self.archive()  # The first gate of the normal translation must still succeed.
        self.run_fast()  # Repeated fast runs reuse the immutable checkpoint.
        archive = next((self.root / 'artifacts').glob('closure-*'))
        self.assertEqual((archive / 'msquic/generated/TranslatedMsQuic/MsQuic.cs').read_text(), '// original output\n')
        self.assertEqual((self.root / 'generated/TranslatedMsQuic/MsQuic.cs').read_text(), '// fast output\n')

    def test_fresh_checkout_does_not_require_qualification_evidence(self):
        self.run_fast()
        self.assertFalse(any('--archive-current' in command for command in self.commands))

    def test_archive_failure_precedes_any_staging_or_output_changes(self):
        self.qualify()
        raw = self.root / 'generated/raw/TranslatedMsQuic/MsQuic.cs'
        raw.write_text('// changed before archive\n')
        with self.assertRaises(subprocess.CalledProcessError):
            self.run_fast()
        self.assertEqual(len(self.commands), 1)
        self.assertIn('--archive-current', self.commands[0])
        self.assertEqual(raw.read_text(), '// changed before archive\n')


if __name__ == '__main__':
    unittest.main()
