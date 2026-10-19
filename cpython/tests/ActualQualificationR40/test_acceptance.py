"""Focused controls for the exact user-authorized five, never product passes."""
import copy
import json
from pathlib import Path
import unittest

import acceptance as gate

HERE = Path(__file__).resolve().parent
ART = HERE.parents[1] / 'artifacts/owner-b-r40-20261018'


class FiveExceptionControls(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.policy = json.loads((HERE / 'five-exceptions.json').read_text())
        cls.inputs = {key: json.loads(Path(ref['path']).read_text()) for key, ref in cls.policy['inputs'].items()}
        cls.original = json.loads((ART / 'full-required-raw-jit.json').read_text())
        cls.positive = copy.deepcopy(cls.original)
        # Hypothetical pure positive control only; retained actual report stays
        # failed with three blockers and is tested separately below.
        for row in cls.positive['cases']:
            if row['case'] not in gate.EXACT_IDS and row['status'] != 'passed':
                case = row['case']
                row.clear()
                row.update(case=case, status='passed')
        details = {r['case']: r for g in cls.original['discovery'] for r in g['case_details']}
        cls.hashes = {case: gate.digest(details[case]['upstream_path']) for case in gate.EXACT_IDS}

    def assess(self, report=None, *, policy=None, native=None, hashes=None):
        return gate.assess(report if report is not None else self.positive, self.inputs['inventory'],
            self.inputs['markers'], native if native is not None else self.inputs['native'],
            policy if policy is not None else self.policy, hashes if hashes is not None else self.hashes)

    def test_positive_control_retains_five_nonpass_statuses(self):
        result = self.assess()
        self.assertEqual(result['acceptance'], 'accepted-with-five-explicit-native-parity-exceptions')
        self.assertEqual(result['acceptedExceptionCount'], 5)
        self.assertEqual(result['rawOutcomeEvents']['required-skip'], 2)
        self.assertEqual(result['rawOutcomeEvents']['required-expected-failure'], 3)
        self.assertEqual(result['rawRunnerStatus'], 'blocked-or-failed')
        self.assertFalse(result['rawQualifiesTranslatedInterpreter'])

    def test_real_full_retains_three_blockers(self):
        result = self.assess(self.original)
        self.assertEqual(result['acceptance'], 'blocked')
        self.assertEqual(len(result['blockingOutcomes']), 3)

    def test_wrong_authorized_id_rejected(self):
        policy = copy.deepcopy(self.policy)
        policy['exceptions'][0]['case'] = 'test.test_codecs.LocaleCodecTest.test_decode_surrogatepass'
        with self.assertRaisesRegex(ValueError, 'IDs'):
            self.assess(policy=policy)

    def test_wrong_authorized_status_rejected(self):
        policy = copy.deepcopy(self.policy)
        policy['exceptions'][0]['status'] = 'passed'
        with self.assertRaises(ValueError):
            self.assess(policy=policy)

    def test_wrong_actual_source_bytes_rejected(self):
        hashes = dict(self.hashes)
        hashes[next(iter(gate.EXACT_IDS))] = 'wrong-source-bytes'
        with self.assertRaisesRegex(ValueError, 'source bytes'):
            self.assess(hashes=hashes)

    def test_wrong_method_or_decorator_rejected(self):
        report = copy.deepcopy(self.positive)
        for group in report['discovery']:
            for detail in group['case_details']:
                if detail['case'] in gate.EXACT_IDS:
                    detail['expected_failure'] = not detail['expected_failure']
                    with self.assertRaisesRegex(ValueError, 'method/decorator/source'):
                        self.assess(report)
                    return
        self.fail('Missing original exception')

    def test_wrong_native_source_witness_rejected(self):
        native = copy.deepcopy(self.inputs['native'])
        row = next(r for r in native['witnesses'] if r['case'] in gate.EXACT_IDS)
        row['nativeModeWitnesses']['0']['originalSource']['sha256'] = 'wrong-native-bytes'
        with self.assertRaisesRegex(ValueError, 'native source'):
            self.assess(native=native)

    def test_wrong_native_status_or_reason_rejected(self):
        native = copy.deepcopy(self.inputs['native'])
        row = next(r for r in native['witnesses'] if r['case'] in gate.EXACT_IDS)
        row['nativeModeWitnesses']['0']['outcomes'][0]['reason'] = 'different native reason'
        with self.assertRaisesRegex(ValueError, 'native status/reason'):
            self.assess(native=native)

    def test_both_extra_locale_skips_rejected(self):
        for case in ('test.test_codecs.LocaleCodecTest.test_decode_surrogatepass',
                     'test.test_codecs.LocaleCodecTest.test_encode_surrogatepass'):
            with self.subTest(case=case):
                report = copy.deepcopy(self.positive)
                row = next(r for r in report['cases'] if r['case'] == case)
                row.update(status='required-skip', reason='native parity cannot expand five exceptions')
                self.assertEqual(self.assess(report)['acceptance'], 'blocked')

    def test_unexpected_success_never_accepted(self):
        report = copy.deepcopy(self.positive)
        next(r for r in report['cases'] if r['case'] in gate.EXACT_IDS)['status'] = 'unexpected-success'
        self.assertEqual(self.assess(report)['acceptance'], 'blocked')

    def test_missing_case_and_fatal_rejected(self):
        report = copy.deepcopy(self.positive)
        case = report['cases'][0]['case']
        report['cases'] = [r for r in report['cases'] if r['case'] != case]
        self.assertEqual(self.assess(report)['acceptance'], 'blocked')
        report['failure'] = 'fatal or interrupted runner'
        with self.assertRaisesRegex(ValueError, 'Interrupted/fatal'):
            self.assess(report)

    def test_subset_native_or_dotcc_scope_rejected(self):
        report = copy.deepcopy(self.positive)
        report['selected_case_ids'] = report['selected_case_ids'][:-1]
        with self.assertRaisesRegex(ValueError, 'subset'):
            self.assess(report)
        policy = copy.deepcopy(self.policy)
        policy['scope'] = 'DotCC CI waiver'
        with self.assertRaisesRegex(ValueError, 'scope'):
            self.assess(policy=policy)


class TwoConfigurationControls(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        FiveExceptionControls.setUpClass.__func__(cls)
        cls.primary = copy.deepcopy(cls.original)
        for row in cls.primary['cases']:
            if row['case'] in gate.TWO_MODE_IDS:
                word = 'decoder' if 'decode' in row['case'] else 'encoder'
                row['reason'] = "'utf-8' " + word + " doesn't support surrogatepass error handler"
            elif row['status'] == 'error':
                case = row['case']
                row.clear()
                row.update(case=case, status='passed')
        cls.primary['interpreter_configuration']['filesystem_encoding'] = 'utf-8'
        cls.library = json.loads((ART / 'actual-input-proof.json').read_text())['library']['identity']
        cls.coverage = {'scope': 'exact two supported startup configurations', 'caseIds': sorted(gate.TWO_MODE_IDS),
            'actualLibrary': cls.library,
            'native': json.loads((ART / 'native-complementary-two.json').read_text()),
            'actual': json.loads((ART / 'actual-complementary-two.json').read_text())}
        details = {r['case']: r for g in cls.original['discovery'] for r in g['case_details']}
        cls.hashes.update({case: gate.digest(details[case]['upstream_path']) for case in gate.TWO_MODE_IDS})

    def integrated(self, report=None, coverage=None):
        return gate.assess_integrated(report if report is not None else self.primary, self.inputs['inventory'],
            self.inputs['markers'], self.inputs['native'], self.policy, self.hashes,
            coverage if coverage is not None else self.coverage, self.library)

    def test_correct_two_mode_control_preserves_raw_skips(self):
        result = self.integrated()
        self.assertEqual(result['acceptance'], 'accepted-with-five-exceptions-and-two-configuration-witnesses')
        self.assertEqual(result['rawOutcomeEvents']['required-skip'], 4)
        self.assertEqual(len(result['complementaryAssertionCoverage']), 2)
        self.assertEqual(result['acceptedExceptionCount'], 5)

    def test_missing_mode_evidence_rejects(self):
        result = gate.assess_integrated(self.primary, self.inputs['inventory'], self.inputs['markers'],
            self.inputs['native'], self.policy, self.hashes, None, self.library)
        self.assertEqual(result['acceptance'], 'blocked')

    def test_wrong_mode_id_source_and_product_rejected(self):
        for kind in ('mode', 'id', 'source', 'product'):
            with self.subTest(kind=kind):
                coverage = copy.deepcopy(self.coverage)
                if kind == 'mode':
                    coverage['actual']['beforeTests']['utf8_mode'] = 0
                elif kind == 'id':
                    coverage['caseIds'][0] = 'some.other.required.case'
                elif kind == 'source':
                    coverage['native']['sourceWitnesses'][0]['sourceSha256'] = 'wrong-native-source'
                else:
                    coverage['actualLibrary']['sha256'] = 'wrong-actual-library'
                with self.assertRaises(ValueError):
                    self.integrated(coverage=coverage)

    def test_native_and_actual_nonpass_witnesses_rejected(self):
        for role in ('native', 'actual'):
            with self.subTest(role=role):
                coverage = copy.deepcopy(self.coverage)
                coverage[role]['cases'][0]['status'] = 'required-skip'
                with self.assertRaisesRegex(ValueError, 'nonPASS'):
                    self.integrated(coverage=coverage)

    def test_no_error_unexpected_success_or_extra_skip_hidden(self):
        for status in ('error', 'unexpected-success', 'failed', 'interrupted'):
            with self.subTest(status=status):
                report = copy.deepcopy(self.primary)
                next(r for r in report['cases'] if r['case'] in gate.TWO_MODE_IDS)['status'] = status
                self.assertEqual(self.integrated(report)['acceptance'], 'blocked')
        report = copy.deepcopy(self.primary)
        next(r for r in report['cases'] if r['status'] == 'passed').update(status='required-skip', reason='extra skip')
        self.assertEqual(self.integrated(report)['acceptance'], 'blocked')

    def test_first_full_setup_and_metadata_error_remain_blocked(self):
        result = self.integrated(self.original)
        self.assertEqual(result['acceptance'], 'blocked')
        self.assertTrue(any(r['status'] == 'error' for r in result['blockingOutcomes']))
        self.assertFalse(result['primaryStartupQualified'])


if __name__ == '__main__':
    unittest.main(verbosity=2)
