"""Local repair guards preserve editable-source policy and reject ambiguous drift."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('stage',ROOT/'scripts/stage.py');stage=importlib.util.module_from_spec(spec);spec.loader.exec_module(stage)


class StagingTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.root=Path(self.temp.name)
        (self.root/'config').mkdir();(self.root/'config/adaptations.json').write_bytes((ROOT/'config/adaptations.json').read_bytes())
        self.source=self.root/'ref';self.file=self.source/'src/core/ipv4/ip4.c';self.file.parent.mkdir(parents=True)
        source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
        self.original=(source/'src/core/ipv4/ip4.c').read_text();self.file.write_text(self.original)
    def tearDown(self):self.temp.cleanup()

    def test_ambiguous_local_condition_fails(self):
        anchor='if (CHECKSUM_GEN_IP || NETIF_CHECKSUM_ENABLED(inp, NETIF_CHECKSUM_GEN_IP))'
        self.file.write_text(self.original+'\n'+anchor)
        with self.assertRaisesRegex(RuntimeError,'unique local condition'):
            stage.ipv4_forwarding(self.root,self.source,self.root/'stage')

    def test_eap_local_edit_preserved_and_ambiguous_anchor_rejected(self):
        source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
        path=self.source/'src/netif/ppp/eap.c';path.parent.mkdir(parents=True)
        original=(source/'src/netif/ppp/eap.c').read_text()
        local=original+'\n/* unrelated local edit retained under warn/off hash policy */\n'
        path.write_text(local)
        staged,evidence=stage.eap_request(self.root,self.source,self.root/'eap-stage')
        self.assertEqual(path.read_text(),local)
        self.assertIn('unrelated local edit retained',staged.read_text())
        self.assertEqual(evidence[0]['changes'][0]['matches'],1)
        self.assertNotEqual(evidence[0]['source_sha256'],evidence[0]['staged_sha256'])
        repair=next(item for item in stage.metadata(self.root)['textPatches'] if item['id']=='eap-request-invalid-state-release')
        path.write_text(local+repair['changes'][0]['before'])
        with self.assertRaisesRegex(RuntimeError,'unique local condition'):
            stage.eap_request(self.root,self.source,self.root/'bad-stage')

    def test_unknown_repair_is_not_silently_ignored(self):
        path=self.root/'config/adaptations.json';data=json.loads(path.read_text());data['textPatches'].append({'id':'unassigned'})
        path.write_text(json.dumps(data))
        with self.assertRaisesRegex(RuntimeError,'profile-assigned'):
            stage.ipv4_forwarding(self.root,self.source,self.root/'stage')


if __name__=='__main__':unittest.main()
