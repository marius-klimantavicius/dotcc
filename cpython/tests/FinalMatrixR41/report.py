"""Count observed final matrix gates without promoting open gates to PASS."""
import argparse
from pathlib import Path
from run import ART, A32, read, ref, verify, write, utc
from selection import publications,cell_selected


def report(authority):
    selected = publications(authority)
    expected = [('native','embedded-gil','raw','jit')]
    expected += [(phase,'embedded-gil',form,mode) for phase in ('contracts','delivery')
                 for form in ('raw','processed') for mode in ('jit','aot')]
    expected += [('boundary','embedded-gil',form,'aot') for form in ('raw','processed')]
    expected += [('upstream','upstream-tests-static',form,mode) for form in ('raw','processed') for mode in ('jit','aot')]
    expected += [('solution','embedded-gil','processed','jit')]
    actual = {}
    historical = []
    for path in sorted((ART / 'cells').glob('*/result.json')):
        d = read(path)
        if not cell_selected(path,d,selected):
            historical.append(ref(path))
            continue
        key = (d['phase'],d['profile'],d['form'],d['mode'])
        actual.setdefault(key,[]).append((path,d))
    blockers = [p for p in sorted((ART / 'cells').glob('*/blocker.json'))]
    for path in blockers:
        if (path.parent / 'result.json').exists():
            continue
        d = read(path)
        if not cell_selected(path,d,selected):
            continue
        key = (d['phase'],d['profile'],d['form'],d['mode'])
        actual.setdefault(key,[]).append((path,d))
    native = ART / 'native/result.json'
    if native.exists(): actual[expected[0]] = [(native,read(native))]
    audit_path = ART / 'audits/result.json'
    audit = read(audit_path) if audit_path.exists() else None
    completed_path = A32 / 'bounded-actual-twope-result.json'
    completed = ref(completed_path) if completed_path.exists() else None
    audited_cells = {r['cell']['path'] for r in audit['cells']} if audit else set()
    rows = []
    for key in expected:
        attempts = actual.get(key,[])
        available = key[1] in selected and key[2] in selected[key[1]]['publication']['products']
        row = dict(phase=key[0],profile=key[1],form=key[2],mode=key[3],status='unrun',
            currentPublicationAvailable=available)
        if attempts:
            path,data = max(attempts,key=lambda item:item[1].get('utc',''))
            row.update(execution=ref(path),status='executed-awaiting-actual-PE-audit')
            if path.name == 'blocker.json':
                row.update(status='blocked-or-failed',error=data['error'])
                raw = path.parent / 'full-required.json'
                counts = path.parent / 'full-journal-counts.json'
                if raw.exists():
                    observed = read(raw)
                    row.update(rawReport=ref(raw),rawCounts=observed['counts'],
                        rawQualifiesTranslatedInterpreter=observed['qualifies_translated_interpreter'],
                        rawFailure=observed.get('failure'))
                if counts.exists():
                    counted = read(counts)
                    row['durableJournalAccounting'] = ref(counts)
                    row['durableUniqueAndEventCounts'] = {name:counted[name] for name in
                        ('required','uniqueStarted','uniqueCompleted','uniqueOutcomes','outcomeEvents',
                         'startEvents','withoutCompletedOutcome','neverStarted','mixedStatusIds')}
            elif key[0] == 'native' or str(path.resolve()) in audited_cells:
                row['status'] = 'passed'
            if key[0] == 'upstream' and 'upstreamAcceptance' in data:
                gate = data['upstreamAcceptance']; verify(gate)
                accepted = read(gate['path'])
                row.update(rawRunnerExit=data['rawRunnerExit'],counts=data['counts'],acceptance=accepted['acceptance'])
                if accepted['acceptance'] != 'accepted-with-five-exceptions-and-two-configuration-witnesses':
                    row['status'] = 'blocked-or-failed'
        rows.append(row)
    supplementary = [dict(evidence=ref(path),status=data['status'],scope=data.get('scope'))
        for key,attempts in actual.items() if key not in expected for path,data in attempts]
    normal_path = ART / 'final-normal-campaign-result.json'
    normal = read(normal_path) if normal_path.exists() else None
    if normal:
        assert normal['status'] == 'passed' and normal['publication'] == selected['upstream-tests-static']['reference']
        verify(normal['receipt']);verify(normal['completedIndex'])
        assert read(normal['receipt']['path'])['status'] == 'passed'
    bindings_path = ART / 'final-completed-envelope-coverage.json'
    bindings = read(bindings_path) if bindings_path.exists() else None
    if bindings:
        assert bindings['status'] == 'passed' and len(bindings['bindings']) == 51
        assert len(bindings['actualLibraries']) == 8 and len(bindings['retainedFailedDiagnosticInputs']) == 1
        assert len(bindings['retainedSupplementaryBuildBindings']) == 1 and bindings['sealedA32InputCount'] == 52
        verify(bindings['authority'])
        assert publications(bindings['authority']['path']) == selected
    remaining = []
    if any(r['status'] != 'passed' for r in rows):
        remaining.append('Any unrun/failed row or actual-PE proof remains blocking')
    if not normal: remaining.append('Normal framework integration/receipt for the final explicit acceptance policy')
    if not bindings: remaining.append('Strict completed actual input-envelope coverage')
    remaining += ['A32 four canonical warmed product proofs',
        'A32 complete eight expanded scalar/native/whole-payload/GC/TLS/owner controls',
        'Director/A P6 current-runtime downstream, dependency/licenses and clean offline reproduction']
    result = dict(utc=utc(),status='open',campaignComplete=False,authority=ref(authority),rows=rows,
        currentActualMatrixStatus='passed' if all(r['status']=='passed' for r in rows) and normal and bindings else 'open',
        counts={status:sum(r['status']==status for r in rows) for status in
                ('passed','executed-awaiting-actual-PE-audit','blocked-or-failed','unrun')},
        retainedFailedAttempts=[ref(p) for p in sorted((ART / 'cells').glob('*/blocker*.json'))],supplementaryEvidence=supplementary,
        retainedHistoricalExecutions=historical,
        actualAudits=ref(audit_path) if audit else None,retainedHistoricalA32Batch=completed,
        normalFramework=ref(normal_path) if normal else None,completedActualBindings=ref(bindings_path) if bindings else None,
        remaining=remaining,
        scope='Observed final actual matrix only; old products and duplicate unittest events cannot supply missing passes.')
    write(ART / 'matrix-report.json',result)
    print(result['counts'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--authority',required=True,type=Path)
    report(parser.parse_args().authority)
