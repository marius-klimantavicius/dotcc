"""An adjacent/canonical PE cannot supply an actual consumer's typed proof."""
import json
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0,str(Path(__file__).resolve().parents[3]/'Scripts'))
from campaigns.recipes import helper
consumer=helper(Path(__file__).with_name('consumer_evidence.py'))
audit=helper(Path(__file__).with_name('audit_product.py'))


class TypedConsumerSelection(unittest.TestCase):
    def setUp(self):
        temporary=tempfile.TemporaryDirectory(prefix='cpython-typed-consumer-')
        self.addCleanup(temporary.cleanup)
        self.root=Path(temporary.name)
        self.compiled={'assembly_sha256':'actual-consumer-sha','mvid':'actual-consumer-mvid'}

    def proof(self, name, sha, mvid):
        path=self.root/name
        path.write_text(json.dumps({'product':{'sha256':sha,'mvid':mvid}}))
        return {'path':str(path),'sha256':audit.digest(path)}

    def test_canonical_proof_cannot_qualify_source_equivalent_consumer(self):
        evidence={'typed_data':self.proof('canonical.json','canonical-sha','canonical-mvid'),
                  'source_files':{'same-source.cs':'same-bytes'}}
        with self.assertRaisesRegex(RuntimeError,'fresh actual-input probes required'):
            consumer.data_review(self.compiled,evidence)

    def test_exact_pe_selects_its_own_proof(self):
        own=self.proof('actual.json',self.compiled['assembly_sha256'],self.compiled['mvid'])
        evidence={'typed_data':self.proof('canonical.json','canonical-sha','canonical-mvid'),
                  'typed_data_by_pe':{self.compiled['assembly_sha256']:own}}
        self.assertEqual(consumer.data_review(self.compiled,evidence)['typed_data'],own)

    def test_wrong_mvid_with_same_claimed_sha_rejected(self):
        evidence={'typed_data':self.proof('wrong.json',self.compiled['assembly_sha256'],'wrong-mvid')}
        with self.assertRaisesRegex(RuntimeError,'PE/MVID mismatch'):
            consumer.data_review(self.compiled,evidence)

    def test_changed_proof_rejected(self):
        own=self.proof('actual.json',self.compiled['assembly_sha256'],self.compiled['mvid'])
        Path(own['path']).write_text('{}')
        with self.assertRaisesRegex(RuntimeError,'missing/changed'):
            consumer.data_review(self.compiled,{'typed_data':own})


if __name__ == '__main__':unittest.main()
