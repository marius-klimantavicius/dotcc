"""Pure miniature-source controls for the project-local notice delivery guard."""
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import tarfile
import tempfile
import unittest
import xml.etree.ElementTree as ET

spec = importlib.util.spec_from_file_location('picotls_notices', Path(__file__).parents[1] / 'scripts/notices.py')
notices = importlib.util.module_from_spec(spec)
spec.loader.exec_module(notices)


class NoticeTests(unittest.TestCase):
    def test_exact_sources_copy_metadata_and_reject_tampering(self):
        with tempfile.TemporaryDirectory() as temporary:
            base = Path(temporary)
            root = base / 'picotls'
            (root / 'config').mkdir(parents=True)
            (base / 'LICENSE').write_bytes(b'exact root MIT license\n')
            names = ['lib/hpke.c', 'lib/picotls.c', 'lib/pembase64.c',
                     'include/picotls.h', 'include/picotls/pembase64.h']
            reference = root / 'ref/picotls-mini'
            reference.mkdir(parents=True)
            archive = root / 'ref/picotls-mini.tar.gz'
            with tarfile.open(archive, 'w:gz') as source:
                for name in names:
                    data = ('/* Copyright ' + name + '\nPermission retained exactly. */\ncode\n').encode()
                    path = reference / name
                    path.parent.mkdir(parents=True, exist_ok=True)
                    path.write_bytes(data)
                    member = tarfile.TarInfo('picotls-mini/' + name)
                    member.size = len(data)
                    source.addfile(member, io.BytesIO(data))
            (root / 'config/inputs.json').write_text(json.dumps({'picotls': dict(
                directory='picotls-mini', revision='mini', url='offline:mini',
                sha256=hashlib.sha256(archive.read_bytes()).hexdigest())}))
            (root / 'config/core-sources.txt').write_text('\n'.join(names[:3]))
            output = root / 'output'
            output.mkdir()
            project = output / 'TranslatedPicotls.csproj'
            project.write_text('<Project/>')
            result = notices.package(root, project)
            notices.package(root, project)
            owned = [*result['files'], 'Picotls.NOTICES.json']
            self.assertEqual(notices.validate(root, output, owned), result)
            nodes = list(ET.parse(project).iter('None'))
            self.assertEqual(len(nodes), 3)
            self.assertTrue(all(node.get('CopyToOutputDirectory') == 'PreserveNewest' and
                                node.get('CopyToPublishDirectory') == 'PreserveNewest' for node in nodes))
            with self.assertRaisesRegex(RuntimeError, 'ownership manifest'):
                notices.validate(root, output, [])
            (output / 'DotCC.LICENSE').write_bytes(b'changed')
            with self.assertRaisesRegex(RuntimeError, 'delivered notice'):
                notices.validate(root, output)
            notices.package(root, project)
            manifest = output / 'Picotls.NOTICES.json'
            manifest.write_text('{}')
            with self.assertRaisesRegex(RuntimeError, 'source origins'):
                notices.validate(root, output)
            (reference / names[0]).write_bytes(b'changed source')
            with self.assertRaisesRegex(RuntimeError, 'Changed picotls notice source'):
                notices.payload(root)
            archive.write_bytes(b'wrong archive')
            with self.assertRaisesRegex(RuntimeError, 'pinned SHA256'):
                notices.payload(root)


if __name__ == '__main__':
    unittest.main()
