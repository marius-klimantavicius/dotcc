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

    def test_smtp_body_flag_only_and_unique_guard(self):
        source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
        path=self.source/'src/apps/smtp/smtp.c';path.parent.mkdir(parents=True)
        original=(source/'src/apps/smtp/smtp.c').read_text()
        local=original+'\n/* unrelated editable-source comment remains */\n';path.write_text(local)
        staged,evidence=stage.smtp_body_validation(self.root,self.source,self.root/'smtp-stage')
        repair=next(item for item in stage.metadata(self.root)['textPatches'] if item['id']=='smtp-body-linebreak-validation')
        change=repair['changes'][0]
        self.assertEqual(staged.read_text(),local.replace(change['before'],change['after'],1))
        self.assertEqual(path.read_text(),local)
        for name in ('to','from','subject'):
            self.assertIn(f'smtp_verify(s->{name}, s->{name}_len, 0)',staged.read_text())
        self.assertEqual(evidence[0]['changes'][0]['matches'],1)
        path.write_text(local+change['before'])
        with self.assertRaisesRegex(RuntimeError,'unique local condition'):
            stage.smtp_body_validation(self.root,self.source,self.root/'ambiguous-smtp')
        path.write_text(local.replace(change['before'],change['after'],1))
        with self.assertRaisesRegex(RuntimeError,'unique local condition'):
            stage.smtp_body_validation(self.root,self.source,self.root/'already-fixed-smtp')

    def test_unknown_repair_is_not_silently_ignored(self):
        path=self.root/'config/adaptations.json';data=json.loads(path.read_text());data['textPatches'].append({'id':'unassigned'})
        path.write_text(json.dumps(data))
        with self.assertRaisesRegex(RuntimeError,'profile-assigned'):
            stage.ipv4_forwarding(self.root,self.source,self.root/'stage')

    def test_lowpan_repairs_only_source_short_address_branch(self):
        source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
        path=self.source/'src/netif/lowpan6.c';path.parent.mkdir(parents=True)
        original=(source/'src/netif/lowpan6.c').read_text()
        local=original+'\n/* unrelated editable source change */\n';path.write_text(local)
        staged,evidence=stage.lowpan_short_source(self.root,self.source,self.root/'lowpan-stage')
        repair=next(item for item in stage.metadata(self.root)['textPatches'] if item['id']=='lowpan6-short-source-address')
        change=repair['changes'][0]
        self.assertEqual(staged.read_text(),local.replace(change['before'],change['after'],1))
        self.assertEqual(path.read_text(),local)
        self.assertEqual(evidence[0]['changes'][0]['matches'],1)
        path.write_text(local+change['before'])
        with self.assertRaisesRegex(RuntimeError,'unique local condition'):
            stage.lowpan_short_source(self.root,self.source,self.root/'bad-lowpan')

    def test_lowpan_composed_repairs_keep_previous_stage_and_guard_drift(self):
        source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
        path=self.source/'src/netif/lowpan6.c';path.parent.mkdir(parents=True)
        original=(source/'src/netif/lowpan6.c').read_text()
        path.write_text(original+'\n/* unrelated local source remains */\n')
        destination=self.root/'lowpan-composed'
        first,_=stage.lowpan_short_source(self.root,self.source,destination)
        prior=first.read_text()
        second,evidence=stage.lowpan_reordered_fragment(self.root,self.source,destination,first)
        repair=next(item for item in stage.metadata(self.root)['textPatches'] if item['id']=='lowpan6-reordered-fragment-link')
        change=repair['changes'][0]
        self.assertEqual(second.read_text(),prior.replace(change['before'],change['after'],1))
        self.assertIn('unrelated local source remains',second.read_text())
        self.assertEqual(evidence[0]['changes'][0]['matches'],1)
        self.assertNotIn('        p->next = q;\n        lrh->frags = p;',path.read_text())
        second.write_text(prior+change['before'])
        with self.assertRaisesRegex(RuntimeError,'unique local condition'):
            stage.lowpan_reordered_fragment(self.root,self.source,destination,second)


if __name__=='__main__':unittest.main()
