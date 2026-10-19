"""Focused ordinary-hook controls; no native or managed command is started."""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import types
import unittest
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location('b41_upstream_hook',Path(__file__).with_name('campaign_hooks.py'))
HOOK = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(HOOK)


class AcceptanceReportingTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.ctx = types.SimpleNamespace(root=self.root,artifacts=self.root)
        self.raw = dict(status='blocked-or-failed',qualifies_translated_interpreter=False,
            counts={'selected':4867,'passed':4868,'required-skip':4,'required-expected-failure':3},
            required_unexecuted=[],cases=[dict(case='exact-native-skip',status='required-skip')])
        actual = dict(path=str(self.root/'actual.dll'),sha256='a'*64,mvid='11111111-1111-1111-1111-111111111111')
        self.verdict = dict(acceptance=HOOK.ACCEPTED,blockingOutcomes=[],actualLibrary=actual,
            rawRunnerStatus=self.raw['status'],rawQualifiesTranslatedInterpreter=False)
        values = dict(rawReport=self.raw,rawJournal={'durable':'journal'},
            actualInputProof={'library':{'identity':actual}},executionProof={'actual':'entry'},
            commands=[{'exit_code':1,'status':'failed'}],configurationCoverage={'exactTwo':'same actual PE'})
        self.artifacts = {}
        for name,value in values.items():
            path = self.root/(name+'.json');path.write_text(json.dumps(value))
            self.artifacts[name] = dict(path=str(path),sha256=hashlib.sha256(path.read_bytes()).hexdigest())
        self.review = dict(status='passed',compiled=dict(assembly_sha256=actual['sha256'],mvid=actual['mvid']))

    def call(self,verdict=None,review=None):
        gate = types.SimpleNamespace(evaluate_integrated=lambda *args:copy.deepcopy(verdict or self.verdict))
        cell = {}
        with patch.object(HOOK,'acceptance_gate',return_value=gate):
            result = HOOK.reviewed_acceptance(self.ctx,cell,self.artifacts,
                self.review if review is None else review,'actual-cell')
        return cell,result

    def test_raw_failed_exit_and_qualification_stay_unchanged_with_separate_acceptance(self):
        original = Path(self.artifacts['rawReport']['path']).read_bytes()
        command = Path(self.artifacts['commands']['path']).read_bytes()
        cell,result = self.call()
        self.assertEqual(cell['acceptance_verdict'],HOOK.ACCEPTED)
        self.assertEqual(cell['raw_runner_status'],'blocked-or-failed')
        self.assertIs(cell['raw_qualifies_translated_interpreter'],False)
        self.assertEqual(Path(self.artifacts['rawReport']['path']).read_bytes(),original)
        self.assertEqual(Path(self.artifacts['commands']['path']).read_bytes(),command)

    def test_raw_outcome_report_retains_nonpasses_and_counts(self):
        cell = {}
        result = HOOK.retain_report(cell,Path(self.artifacts['rawReport']['path']))
        self.assertIs(result['qualifies_translated_interpreter'],False)
        self.assertEqual(cell['counts'],self.raw['counts'])
        self.assertEqual(cell['first_required_nonpass'],self.raw['cases'][0])

    def test_blocked_gate_never_promotes_error_skip_unrun_or_unexpected_success(self):
        for status in ('failed','error','required-skip','unexpected-success','unrun-or-fatal'):
            with self.subTest(status=status):
                verdict = dict(self.verdict,acceptance='blocked',blockingOutcomes=[dict(case='extra-case',status=status)])
                with self.assertRaisesRegex(RuntimeError,'acceptance blocked'):
                    self.call(verdict=verdict)

    def test_missing_strict_compilation_review_rejects_before_acceptance(self):
        for review in ({},{'status':'unrun'}):
            with self.assertRaisesRegex(RuntimeError,'Strict completed actual-PE'):
                self.call(review=review)

    def test_other_actual_library_sha_or_mvid_cannot_reuse_review(self):
        for key,value in (('assembly_sha256','b'*64),('mvid','22222222-2222-2222-2222-222222222222')):
            review = copy.deepcopy(self.review);review['compiled'][key]=value
            with self.assertRaisesRegex(RuntimeError,'different actual library'):
                self.call(review=review)

    def test_changed_report_command_or_coverage_bytes_reject(self):
        for name in ('rawReport','commands','configurationCoverage'):
            with self.subTest(name=name):
                path = Path(self.artifacts[name]['path']);original=path.read_bytes();path.write_bytes(original+b' ')
                try:
                    with self.assertRaisesRegex(RuntimeError,'artifact bytes changed'):
                        self.call()
                finally:
                    path.write_bytes(original)

    def test_gate_cannot_return_other_library(self):
        verdict = copy.deepcopy(self.verdict);verdict['actualLibrary']['sha256']='c'*64
        with self.assertRaisesRegex(RuntimeError,'acceptance selects different actual library'):
            self.call(verdict=verdict)


class CompletedCellReportingTests(unittest.TestCase):
    def setUp(self):
        AcceptanceReportingTests.setUp(self)
        self.ctx.receipt = {}
        self.ctx.save = lambda: None
        self.proof_path = Path(self.artifacts['actualInputProof']['path'])
        self.proof = json.loads(self.proof_path.read_text())
        self.proof.update(profile=HOOK.PROFILE,form='raw',mode='jit',
            entry=dict(sourceBinding=dict(actualProject=dict(path=str(self.root/'tests/Upstream/Upstream.csproj')))),
            executable=dict(path=str(self.root/'Upstream.dll')))
        self.store('actualInputProof',self.proof)
        self.store('commands',[dict(command=[self.proof['executable']['path'],self.artifacts['rawReport']['path']],
            exit_code=1,status='failed')])
        self.receipt = self.root/'receipt.json';self.receipt.write_text('{}')
        self.index = self.root/'completed-cells.json'
        self.index_value = dict(profile=HOOK.PROFILE,receipt=self.reference(self.receipt),
            cells={'raw-jit':dict(artifacts=self.artifacts,a32Evidence=dict(path='A32 exact evidence',sha256='d'*64))})
        self.index.write_text(json.dumps(self.index_value))
        self.calls = []
        self.profile = types.SimpleNamespace(identity=lambda *args:dict(receipt=str(self.receipt),form='raw'),
            completed_compilation_review=self.completed_review)

    def reference(self,path):
        return dict(path=str(path),sha256=hashlib.sha256(path.read_bytes()).hexdigest())

    def store(self,name,value):
        path = Path(self.artifacts[name]['path']);path.write_text(json.dumps(value))
        self.artifacts[name] = self.reference(path)

    def completed_review(self,*args):
        self.calls.append(args)
        return self.review

    def run_completed(self):
        self.index.write_text(json.dumps(self.index_value))
        gate = types.SimpleNamespace(evaluate_integrated=lambda *args:copy.deepcopy(self.verdict))
        with patch.object(HOOK,'acceptance_gate',return_value=gate):
            HOOK.completed_cells(self.ctx,{'raw':self.root/'TranslatedCPython.csproj'},['jit'],self.index,self.profile)
        return self.ctx.receipt['upstream_cells'][-1]

    def test_normal_completed_route_consumes_strict_api_and_preserves_raw_failed_command(self):
        cell = self.run_completed()
        self.assertEqual(cell['status'],'accepted')
        self.assertEqual(cell['raw_runner_exit'],1)
        self.assertEqual(cell['raw_runner_command_status'],'failed')
        self.assertIs(cell['raw_qualifies_translated_interpreter'],False)
        self.assertEqual(len(self.calls),1)
        self.assertEqual(self.calls[0][2],self.artifacts['actualInputProof'])
        self.assertEqual(self.calls[0][3],self.index_value['cells']['raw-jit']['a32Evidence'])

    def test_private_qualification_entry_cannot_supply_ordinary_upstream_cell(self):
        self.proof['entry']['sourceBinding']['actualProject']['path'] = str(self.root/'tests/Qualification/Qualification.csproj')
        self.store('actualInputProof',self.proof)
        with self.assertRaisesRegex(RuntimeError,'failed/blocked'):
            self.run_completed()
        self.assertIn('ordinary Upstream entry',self.ctx.receipt['upstream_cells'][-1]['error'])
        self.assertEqual(self.calls,[])

    def test_completed_cell_mode_and_selection_are_bound(self):
        self.proof['mode'] = 'aot';self.store('actualInputProof',self.proof)
        with self.assertRaisesRegex(RuntimeError,'failed/blocked'):
            self.run_completed()
        self.assertIn('profile/form/mode differs',self.ctx.receipt['upstream_cells'][-1]['error'])

    def test_missing_selected_cell_blocks_without_partial_success(self):
        self.index_value['cells'] = {}
        with self.assertRaisesRegex(RuntimeError,'failed/blocked'):
            self.run_completed()
        self.assertEqual(self.ctx.receipt['upstream_cells'][-1]['status'],'blocked-or-failed')

    def test_incomplete_actual_proof_blocks_after_retaining_raw_execution(self):
        def reject(*args):
            raise RuntimeError('A32 strict typed/address/lifetime proof incomplete')
        self.profile.completed_compilation_review = reject
        with self.assertRaisesRegex(RuntimeError,'failed/blocked'):
            self.run_completed()
        cell = self.ctx.receipt['upstream_cells'][-1]
        self.assertEqual(cell['raw_runner_exit'],1)
        self.assertEqual(cell['counts'],self.raw['counts'])
        self.assertNotIn('acceptance_verdict',cell)

    def test_completed_index_receipt_bytes_cannot_change(self):
        self.receipt.write_text('{"changed":true}')
        with self.assertRaisesRegex(RuntimeError,'artifact bytes changed'):
            self.run_completed()
        self.assertEqual(self.calls,[])

    def test_completed_gate_still_blocks_real_extra_failure(self):
        self.verdict.update(acceptance='blocked',blockingOutcomes=[dict(case='real-extra-case',status='failed-subtest')])
        with self.assertRaisesRegex(RuntimeError,'failed/blocked'):
            self.run_completed()
        cell = self.ctx.receipt['upstream_cells'][-1]
        self.assertEqual(cell['acceptance_verdict'],'blocked')
        self.assertEqual(cell['raw_runner_exit'],1)


if __name__ == '__main__':
    unittest.main()
