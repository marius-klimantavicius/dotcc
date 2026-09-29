"""Exact local patch guards preserve editable-source hash policy."""
import importlib.util,json,pathlib,tempfile,unittest
ROOT=pathlib.Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('stage',ROOT/'scripts/stage.py');stage=importlib.util.module_from_spec(spec);spec.loader.exec_module(stage)
class ForwardingStageTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory(dir=ROOT/'build');self.directory=pathlib.Path(self.temp.name)
        self.source=self.directory/'source';self.file=self.source/'src/core/ipv4/ip4.c';self.file.parent.mkdir(parents=True)
        pin=json.loads((ROOT/'config/sources.json').read_text());self.original=(ROOT/'ref'/pin['directory']/'src/core/ipv4/ip4.c').read_bytes();self.file.write_bytes(self.original)
    def tearDown(self):self.temp.cleanup()
    def test_repairs_four_local_conditions_without_reference_mutation(self):
        target,evidence=stage.ipv4_forwarding(ROOT,self.source,self.directory/'staged')
        self.assertEqual(self.file.read_bytes(),self.original);self.assertEqual(len(evidence[0]['changes']),4)
        self.assertEqual(target.read_text().count('LWIP_CHECKSUM_CTRL_PER_NETIF && !NETIF_CHECKSUM_ENABLED(netif, NETIF_CHECKSUM_GEN_'),4)
    def test_unrelated_local_edits_are_supported(self):
        self.file.write_bytes(self.original+b'\n/* local editable-source witness */\n')
        target,_=stage.ipv4_forwarding(ROOT,self.source,self.directory/'staged')
        self.assertIn('local editable-source witness',target.read_text())
    def test_changed_target_condition_requires_review(self):
        self.file.write_text(self.original.decode().replace('if (CHECKSUM_GEN_IP || NETIF_CHECKSUM_ENABLED(inp, NETIF_CHECKSUM_GEN_IP))','if (0)',1))
        with self.assertRaisesRegex(RuntimeError,'unique local condition'):stage.ipv4_forwarding(ROOT,self.source,self.directory/'staged')
if __name__=='__main__':unittest.main()
