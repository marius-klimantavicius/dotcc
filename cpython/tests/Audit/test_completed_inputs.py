"""Missing prebuilt inputs reject; synthetic files never qualify a product."""
import json
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0,str(Path(__file__).resolve().parents[3]/'Scripts'))
from campaigns.recipes import helper

completed = helper(Path(__file__).with_name('completed_inputs.py'))


class CompletedInputRejections(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix='completed-input-rejection-')
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.campaign = Path(__file__).resolve().parents[2]
        self.receipt = self.root/'receipt.json'
        self.receipt.write_text(json.dumps({'profile':'upstream-tests-static'}))
        self.compiled = {'identity':{'receipt':str(self.receipt),'form':'raw'}}

    def reject(self, proof, message):
        path = self.root/'actual.json'
        path.write_text(json.dumps(proof))
        with self.assertRaisesRegex(RuntimeError,message):
            completed.validate(self.campaign,path,self.compiled)

    def test_wrong_mode_rejected(self):
        self.reject({'mode':'unknown','form':'raw'}, 'mode/form')

    def test_another_profile_cannot_reuse_completed_proof(self):
        self.reject({'mode':'jit','form':'raw','profile':'embedded-gil'}, 'profile differs')

    def test_entry_only_cannot_stand_in_for_facade_and_library(self):
        self.reject({'mode':'jit','form':'raw','profile':'upstream-tests-static','entry':{}},
                    'entry/library/facade closure missing')

    def test_empty_role_identity_is_not_a_completed_binding(self):
        self.reject({'mode':'jit','form':'raw','profile':'upstream-tests-static',
                     'entry':{},'library':{},'facade':{}}, 'missing entry PE')

    def test_native_abi_route_rejects_arbitrary_entry_without_facade(self):
        path = self.root/'actual.json'
        path.write_text(json.dumps({'mode':'jit','form':'raw','profile':'upstream-tests-static',
                                    'entry':{},'library':{}}))
        with self.assertRaisesRegex(RuntimeError,'native ABI component/project boundary'):
            completed.validate_native_abi(self.campaign,path,self.compiled)


if __name__ == '__main__':
    unittest.main()
