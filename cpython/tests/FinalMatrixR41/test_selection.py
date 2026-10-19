"""Source-only current-publication controls; historical proof never transfers."""
import copy
from pathlib import Path
import unittest
from run import ART,read
from selection import publications,request_selected,cell_selected,input_selected


class PublicationSelectionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selected = publications(ART/'locale-policy-r42-authority.json')
        cls.current = read(ART/'actual-pe-requests/upstream-tests-static-raw-05766383551c4c3e.json')
        cls.directory = ART/'cells/upstream-tests-static-raw-jit-upstream-locale-policy-r42'

    def test_exact_actual_request_selected(self):
        self.assertTrue(request_selected(self.current,self.selected))

    def test_initial_two_requests_remain_historical(self):
        for filename in ('embedded-gil-raw-c48f217dcfb7c36b.json','upstream-tests-static-raw-59a81008f8e00709.json'):
            self.assertFalse(request_selected(read(ART/'actual-pe-requests'/filename),self.selected))

    def test_same_pe_wrong_receipt_or_publication_rejects(self):
        for field in ('receipt','publication'):
            value = copy.deepcopy(self.current);value[field]['sha256']='f'*64
            self.assertFalse(request_selected(value,self.selected))

    def test_new_receipt_cannot_use_legacy_missing_publication(self):
        value = copy.deepcopy(self.current);del value['publication']
        self.assertFalse(request_selected(value,self.selected))

    def test_raw_only_cannot_supply_processed(self):
        value = dict(self.current,form='processed')
        self.assertFalse(request_selected(value,self.selected))

    def test_actual_cell_and_input_match_but_old_cell_does_not(self):
        path = self.directory/'result.json'
        self.assertTrue(cell_selected(path,read(path),self.selected))
        self.assertTrue(input_selected(read(self.directory/'Upstream-actual-inputs.json'),self.selected))
        old = ART/'cells/upstream-tests-static-raw-jit-upstream-initial/blocker.json'
        self.assertFalse(cell_selected(old,read(old),self.selected))


if __name__ == '__main__':
    unittest.main()
