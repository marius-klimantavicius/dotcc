"""Publication-binding rejection controls; synthetic bytes qualify no engine."""
import copy
import json
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[3]/'Scripts'))
from campaigns.recipes import helper

profile = helper(Path(__file__).with_name('profile_evidence.py'))
audit = helper(Path(__file__).with_name('audit_product.py'))


class ProfilePublicationBinding(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.project = self.root/'generated/profiles/upstream-tests-static/TranslatedCPython/TranslatedCPython.csproj'
        self.project.parent.mkdir(parents=True)
        self.project.write_text('<Project />')
        self.source = self.project.with_name('CPython.cs')
        self.source.write_text('// synthetic identity control')
        self.project.with_name('Dotcc.SourceFiles.txt').write_text('CPython.cs\n')
        self.project.with_name('.campaign-files.json').write_text(json.dumps(['CPython.cs',self.project.name]))
        hashes = {name:audit.digest(self.project.parent/name) for name in
                  ['CPython.cs',self.project.name,'Dotcc.SourceFiles.txt','.campaign-files.json']}
        self.output = {'project':str(self.project), 'hashes':hashes}
        base = self.root/'artifacts/campaign'
        base.mkdir(parents=True)
        self.receipt = base/'receipt.json'
        self.document = {'project':'cpython','profile':profile.PROFILE,'status':'passed',
                         'outputs':{'processed':self.output}}
        self.pointer = base/'current-upstream-tests-static.json'
        self.publication = {'receipt':str(self.receipt),'outputs':{'processed':copy.deepcopy(self.output)}}
        self.write()

    def write(self):
        self.receipt.write_text(json.dumps(self.document))
        self.pointer.write_text(json.dumps(self.publication))

    def observe(self):
        # The real reader location is separate from the synthetic campaign.
        original = profile.reader
        profile.reader = lambda _:audit
        try:
            return profile.identity(self.root,'processed',self.project,self.receipt)
        finally:
            profile.reader = original

    def test_exact_current_publication_binds_its_own_receipt(self):
        observed = self.observe()
        self.assertEqual(observed['receipt_sha256'],audit.digest(self.receipt))
        self.assertEqual(observed['files'],self.output['hashes'])

    def test_embedded_receipt_cannot_qualify_test_profile(self):
        self.document['profile'] = 'embedded-gil'
        self.write()
        with self.assertRaisesRegex(RuntimeError,'translation incomplete'):
            self.observe()

    def test_failed_publication_rejected(self):
        self.document['status'] = 'failed'
        self.write()
        with self.assertRaisesRegex(RuntimeError,'translation incomplete'):
            self.observe()

    def test_changed_source_rejected_even_if_pointer_is_unchanged(self):
        self.source.write_text('// different current bytes')
        with self.assertRaisesRegex(RuntimeError,'publication bytes changed'):
            self.observe()

    def test_pointer_hashes_cannot_override_translation_receipt(self):
        self.source.write_text('// different current bytes')
        self.publication['outputs']['processed']['hashes']['CPython.cs'] = audit.digest(self.source)
        self.write()
        with self.assertRaisesRegex(RuntimeError,'publication bytes changed'):
            self.observe()

    def test_another_profile_project_rejected(self):
        self.publication['outputs']['processed']['project'] = str(self.root/'embedded.csproj')
        self.write()
        with self.assertRaisesRegex(RuntimeError,'project changed'):
            self.observe()

    def test_duplicate_ownership_rejected(self):
        self.project.with_name('.campaign-files.json').write_text('["CPython.cs","CPython.cs"]')
        with self.assertRaisesRegex(RuntimeError,'ownership closure'):
            self.observe()


if __name__ == '__main__':
    unittest.main()
