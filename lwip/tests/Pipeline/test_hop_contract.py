"""The scoped override must fail if upstream adds a different setter role."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('hop',ROOT/'scripts/ip6-hop.py');hop=importlib.util.module_from_spec(spec);spec.loader.exec_module(hop)

class HopContractTests(unittest.TestCase):
    def test_changed_body_and_new_call_are_rejected(self):
        reference=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
        with tempfile.TemporaryDirectory() as directory:
            source=Path(directory)
            for relative in ('src/include/lwip/prot/ip6.h','src/core/ipv6/ip6.c'):
                target=source/relative;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes((reference/relative).read_bytes())
            hop.check_contract(ROOT,source)
            unit=source/'src/core/ipv6/ip6.c';original=unit.read_text()
            unit.write_text(original+'\n/* local note: IP6H_HOPLIM_SET(other, 123); */\n')
            hop.check_contract(ROOT,source)
            unit.write_text(original+'\nIP6H_HOPLIM_SET(other, 123);\n')
            with self.assertRaisesRegex(RuntimeError,'call roles changed'):hop.check_contract(ROOT,source)
            unit.write_text(original)
            header=source/'src/include/lwip/prot/ip6.h';header.write_text(header.read_text().replace('(hdr)->_hoplim = (u8_t)(hl)','(hdr)->_hoplim = (u8_t)((hl)+1)'))
            with self.assertRaisesRegex(RuntimeError,'macro contract changed'):hop.check_contract(ROOT,source)

if __name__=='__main__':unittest.main()
