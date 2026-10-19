"""Notice delivery must retain exact upstream bytes and fail closed."""
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
import zipfile

SPEC = importlib.util.spec_from_file_location('sqlite_notices', Path(__file__).resolve().parents[2] / 'scripts/notices.py')
NOTICES = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(NOTICES)


class NoticeTests(unittest.TestCase):
    def fixture(self, directory):
        repo = Path(directory)
        (repo / 'LICENSE').write_bytes(b'MIT exact runtime attribution\n')
        root = repo / 'sqlite'
        (root / 'config').mkdir(parents=True)
        (root / 'ref').mkdir()
        archive = root / 'ref/source.zip'
        with zipfile.ZipFile(archive, 'w') as source:
            source.writestr('sqlite-src/LICENSE.md', b'Exact SQLite public-domain source notice\n')
        pin = dict(url='https://example.invalid/pinned-source.zip', archive='source.zip', directory='sqlite-src',
                   sha256=hashlib.sha256(archive.read_bytes()).hexdigest())
        (root / 'config/sources.json').write_text(json.dumps(dict(product=pin, tests=pin)))
        project = root / 'generated/Product.csproj'
        project.parent.mkdir()
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"/>')
        return root, project

    def test_exact_notices_origins_and_publish_ownership(self):
        import xml.etree.ElementTree as ET
        with tempfile.TemporaryDirectory() as directory:
            root, project = self.fixture(directory)
            report = NOTICES.package(root, project)
            self.assertEqual((project.parent / 'DotCC.LICENSE').read_bytes(), (root.parent / 'LICENSE').read_bytes())
            self.assertEqual((project.parent / 'SQLite.LICENSE.md').read_bytes(), b'Exact SQLite public-domain source notice\n')
            self.assertEqual(report['origins']['sqlite']['tests']['url'], 'https://example.invalid/pinned-source.zip')
            items = {n.get('Update'): n for n in ET.parse(project).iter('None')}
            for name in (*report['files'], 'SQLite.NOTICES.json'):
                self.assertEqual(items[name].get('CopyToPublishDirectory'), 'PreserveNewest')
            NOTICES.validate(project.parent, items, root=root)

    def test_missing_ownership_rejects_even_when_notice_file_exists(self):
        with tempfile.TemporaryDirectory() as directory:
            root, project = self.fixture(directory)
            report = NOTICES.package(root, project)
            with self.assertRaisesRegex(RuntimeError, 'ownership manifest'):
                NOTICES.validate(project.parent, list(report['files']), root=root)

    def test_tampered_or_missing_notice_rejects(self):
        with tempfile.TemporaryDirectory() as directory:
            root, project = self.fixture(directory)
            NOTICES.package(root, project)
            notice = project.parent / 'DotCC.LICENSE'
            notice.write_text('truncated')
            with self.assertRaisesRegex(RuntimeError, 'changed delivered notice'):
                NOTICES.validate(project.parent, root=root)
            notice.unlink()
            with self.assertRaises(FileNotFoundError):
                NOTICES.validate(project.parent, root=root)

    def test_rewritten_manifest_cannot_authorize_truncated_notice(self):
        with tempfile.TemporaryDirectory() as directory:
            root, project = self.fixture(directory)
            report = NOTICES.package(root, project)
            (project.parent / 'DotCC.LICENSE').write_text('truncated')
            report['files']['DotCC.LICENSE'] = NOTICES.digest(project.parent / 'DotCC.LICENSE')
            (project.parent / 'SQLite.NOTICES.json').write_text(json.dumps(report))
            with self.assertRaisesRegex(RuntimeError, 'exact source origins'):
                NOTICES.validate(project.parent, root=root)

    def test_wrong_source_archive_rejects_before_packaging(self):
        with tempfile.TemporaryDirectory() as directory:
            root, project = self.fixture(directory)
            (root / 'ref/source.zip').write_bytes(b'wrong archive')
            with self.assertRaisesRegex(RuntimeError, 'source archive differs'):
                NOTICES.package(root, project)
            self.assertFalse((project.parent / 'DotCC.LICENSE').exists())

    def test_ilc_without_actual_package_inputs_rejects(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'consumer.ilc.rsp'
            path.write_text('-r:/missing/unbound.dll\n')
            with self.assertRaisesRegex(RuntimeError, 'No actual NativeAOT package inputs'):
                NOTICES.nativeaot(Path(directory), path)

    def test_signed_cache_raw_hash_and_metadata_hash_remain_distinct(self):
        import base64
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            package = root / 'runtime.fixture/1.0'
            package.mkdir(parents=True)
            library = package / 'runtime.dll'
            library.write_bytes(b'fake runtime input')
            archive = package / 'runtime.fixture.1.0.nupkg'
            with zipfile.ZipFile(archive, 'w') as cached:
                for name in ('LICENSE.TXT', 'THIRD-PARTY-NOTICES.TXT'):
                    cached.writestr(name, name.encode())
                    (package / name).write_bytes(name.encode())
                cached.writestr('.signature.p7s', b'fake signed-package fixture')
            raw = base64.b64encode(hashlib.sha512(archive.read_bytes()).digest()).decode()
            sidecar = archive.with_suffix('.nupkg.sha512')
            sidecar.write_text(raw)
            (package / '.nupkg.metadata').write_text(json.dumps(dict(contentHash='distinct-metadata-value', source='offline-fixture')))
            response = root / 'actual.ilc.rsp'
            response.write_text('-r:' + str(library) + '\n')
            result = NOTICES.nativeaot(root, response)
            self.assertEqual(len(result['files']), 2)
            self.assertEqual(result['files'][0]['archiveSha512'], raw)
            self.assertEqual(result['files'][0]['nugetMetadataContentHash'], 'distinct-metadata-value')
            sidecar.write_text('wrong raw archive hash')
            with self.assertRaisesRegex(RuntimeError, 'archive SHA512 differs'):
                NOTICES.nativeaot(root, response)


if __name__ == '__main__':
    unittest.main()
