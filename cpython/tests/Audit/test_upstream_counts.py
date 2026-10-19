"""Unique IDs and occurrence events have separate qualification counters."""
from pathlib import Path
import importlib.util
import unittest

spec=importlib.util.spec_from_file_location('count_controls_audit',Path(__file__).with_name('audit_product.py'))
audit=importlib.util.module_from_spec(spec);spec.loader.exec_module(audit)


class UpstreamCountControls(unittest.TestCase):
    def setUp(self):
        self.required=['case'+str(i) for i in range(4867)]
        statuses=['passed']*4751+['failed']*12+['required-skip']*101+['required-expected-failure']*3
        self.events=[]
        for case,status in zip(self.required,statuses):
            self.events.extend([{'case':case,'status':'started'},{'case':case,'status':status}])
        self.events.extend({'case':case,'status':'passed'} for case in self.required[:8])

    def test_b34_correct_unique_and_event_totals_are_explicit(self):
        counts=audit.upstream_outcomes(self.required,self.events)
        self.assertEqual(counts['uniqueCompleted'],4867)
        self.assertEqual(counts['uniqueOutcomes']['passed'],4751)
        self.assertEqual(counts['outcomeEvents']['passed'],4759)
        self.assertEqual(sum(counts['outcomeEvents'].values()),4875)
        self.assertEqual(len(counts['duplicateOutcomeOccurrences']),8)
        self.assertFalse(counts['required_qualification_passed'])
        self.assertEqual(audit.upstream_outcomes(self.required,self.events,counts),counts)

    def test_4759_pass_events_cannot_be_claimed_as_unique_4867_completion(self):
        counts=audit.upstream_outcomes(self.required,self.events)
        counts['uniqueOutcomes']['passed']=4759
        with self.assertRaisesRegex(audit.AuditError,'impossible unique outcome total'):
            audit.upstream_outcomes(self.required,self.events,counts)

    def test_omitting_duplicate_events_from_claims_is_rejected(self):
        counts=audit.upstream_outcomes(self.required,self.events)
        counts['outcomeEvents']['passed']=4751
        with self.assertRaisesRegex(audit.AuditError,'actual journal: outcomeEvents'):
            audit.upstream_outcomes(self.required,self.events,counts)

    def test_duplicate_conflicting_status_cannot_be_silently_selected(self):
        self.events.append({'case':self.required[0],'status':'failed'})
        with self.assertRaisesRegex(audit.AuditError,'conflicting duplicate'):
            audit.upstream_outcomes(self.required,self.events)

    def test_same_number_wrong_case_is_rejected(self):
        self.events[-1]['case']='unselected-case'
        with self.assertRaisesRegex(audit.AuditError,'unknown case/status'):
            audit.upstream_outcomes(self.required,self.events)

    def test_missing_case_is_not_completed(self):
        self.events=[e for e in self.events if e['case']!=self.required[-1]]
        counts=audit.upstream_outcomes(self.required,self.events)
        self.assertEqual(counts['withoutCompletedOutcome'],1)
        self.assertEqual(counts['neverStarted'],1)
        self.assertFalse(counts['required_qualification_passed'])

    def test_started_fatal_without_outcome_stays_visible(self):
        self.events=[e for e in self.events if not(e['case']==self.required[-1] and e['status']!='started')]
        counts=audit.upstream_outcomes(self.required,self.events)
        self.assertEqual(counts['fatalStartedWithoutOutcome'],[self.required[-1]])
        self.assertFalse(counts['required_qualification_passed'])


if __name__=='__main__':unittest.main()
