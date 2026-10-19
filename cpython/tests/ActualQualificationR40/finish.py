"""Seal terminal B40 observations and one genuine no-queue/free-lock release."""
from collections import Counter
import datetime
import fcntl
import json
from pathlib import Path
import sys

from run_actual import ART, WORK, HERE, CPY, ROOT, read, verify, write, item, preserve_payload


def main():
    command_files = sorted(ART.glob('*-commands.json'))
    commands = [row for path in command_files for row in read(path)]
    commands += [read(ART / name)['command'] for name in ('acceptance-controls.json', 'integrated-acceptance-controls.json')]
    assert all(r['status'] in ('passed', 'failed', 'timed-out', 'cancelled') for r in commands)
    pids = sorted({r['pid'] for r in commands})
    assert all(not Path('/proc', str(pid)).exists() for pid in pids)
    # Check that no descendant/driver using these private outputs remains, even
    # though ordinary command receipts identify the flock process, not its child.
    active = []
    for entry in Path('/proc').iterdir():
        if not entry.name.isdigit():
            continue
        try:
            argv = (entry / 'cmdline').read_bytes().split(b'\0')
        except (FileNotFoundError, PermissionError, ProcessLookupError):
            continue
        if any(part.startswith(str(WORK).encode()) or part == str(HERE / 'run_actual.py').encode()
               or part == b'cpython/tests/ActualQualificationR40/run_actual.py' for part in argv):
            active.append({'pid': int(entry.name), 'argv': [p.decode(errors='replace') for p in argv if p]})
    assert not active, active
    authority = read(ART / 'direct-a31-authority.json')
    verify(authority['release'])
    release = read(authority['release']['path'])
    startup_changes = {row['path']: row for row in read(ART / 'tested-startup-source-pins.json')['sources']}
    for path, digest in release['sourcePins'].items():
        if path in startup_changes:
            row = startup_changes[path]
            assert row['originalSha256'] == digest
            assert item(row['retainedCopy'])['sha256'] == digest
            assert item(path)['sha256'] == row['currentSha256']
        else:
            assert item(path)['sha256'] == digest, path
    for reference in read(CPY / 'artifacts/owner-b-r37-20261018/native-host-startup-inputs.json'):
        verify(reference)
    proof = read(ART / 'actual-input-proof.json')
    for reference in proof['references']:
        verify(reference)
    for key in ('entry', 'facade', 'library'):
        for path, digest in proof[key]['sourceBinding']['actualCompileInputs'].items():
            assert item(path)['sha256'] == digest, path
    for row in read(ART / 'actual-entry.json')['ownSources']:
        verify(row)
    for name in ('locale-entry-input-proof.json', 'complementary-input-proof.json',
                 'OrdinaryStartupBindingR40-input-proof.json', 'QualificationStartupBindingR40-input-proof.json'):
        binding = read(ART / name)
        for path, digest in binding['sourceBinding']['actualCompileInputs'].items():
            assert item(path)['sha256'] == digest
        assert item(binding['entry']['path'])['sha256'] == binding['entry']['sha256']
        for ref in binding['references']:
            verify(ref)
    preservation = {path.stem: preserve_payload(item(path)) for path in ART.glob('*-payload.json')}
    counts = read(ART / 'full-journal-counts.json')
    full = read(ART / 'full-required-raw-jit.json')
    inventory = read(CPY / 'tests/Upstream/inventory.frozen.json')
    assert full['selected_case_ids'] == inventory['required_cases']
    for status, count in counts['outcomeEvents'].items():
        assert full['counts'][status] == count, (status, count)
    assert counts['uniqueStarted'] == counts['uniqueCompleted'] == counts['required'] == 4867
    assert not counts['unexpectedIds'] and not counts['outcomesWithoutStart']
    focused = read(ART / 'focused-104-raw-jit.json')
    assert focused['all104ExactNativeStatusesMatch']
    details = {r['case']: r for group in full['discovery'] for r in group['case_details']}
    focused_sources = {r['case']: r for group in focused['discovery'] for r in group['case_details']}
    comparison = []
    for case in focused['selectedIds']:
        a = [(r['status'], r.get('reason')) for r in focused['cases'] if r['case'] == case]
        b = [(r['status'], r.get('reason')) for r in full['cases'] if r['case'] == case]
        assert a == b, (case, a, b)
        for key in ('source', 'expected_failure', 'method', 'declaring_class'):
            assert details[case][key] == focused_sources[case][key], (case, key)
        comparison.append({'case': case, 'focused': a, 'full': b, 'equal': True})
    write('full-marker-comparison.json', {'scope': 'Every original104 has same full/focused/native mode0 status and unchanged source/method/decorator',
        'cases': comparison, 'all104Match': True, 'rawStatusesRelabeled': False,
        'explicitPolicy': item(HERE / 'five-exceptions.json')})
    marker_categories = {r['case']: r['category'] for r in
        read(CPY / 'artifacts/owner-b-r36-20261018/required-marker-source-inventory.json')['cases']}
    unique_cases = {r['case']: r for r in full['cases']}
    category_counts = {category: dict(Counter(unique_cases[case]['status'] for case, cat in marker_categories.items()
        if cat == category)) for category in sorted(set(marker_categories.values()))}
    nonpasses = [row for row in full['cases'] if row['status'] != 'passed']
    from acceptance import evaluate_integrated
    integrated = evaluate_integrated(ART / 'full-required-raw-jit.json', ART / 'full-required-raw-jit.journal.jsonl',
        ART / 'actual-input-proof.json', ART / 'actual-input-proof.json', ART / 'qualify-commands.json',
        HERE / 'five-exceptions.json', ART / 'two-mode-coverage.json')
    assert integrated == read(ART / 'first-full-integrated-acceptance.json')
    assert integrated['acceptance'] == 'blocked'
    assert read(ART / 'integrated-acceptance-controls.json')['tests'] == 18
    result = {'status': 'First actual RAW JIT completed/failed and preserved; integrated source/controls ready for final generation/matrix',
        'inputProof': item(ART / 'actual-input-proof.json'), 'counts': counts,
        'fullRunnerStatus': full['status'], 'fullRunnerQualifies': full['qualifies_translated_interpreter'],
        'host': item(ART / 'host-result.json'), 'managedStartup': item(ART / 'startup-result.json'),
        'focused104': item(ART / 'focused-104-raw-jit.json'),
        'fullMarkerComparison': item(ART / 'full-marker-comparison.json'),
        'original104CategoryOutcomes': category_counts, 'remainingRequiredNonpasses': nonpasses,
        'commands': [item(path) for path in command_files],
        'commandCount': len(commands), 'commandSeconds': sum(r['seconds'] for r in commands),
        'preservation': preservation,
        'scope': 'Current V22 actual own library/facade/entry, raw upstream-tests-static Linux x64 JIT only; no current nativeAOT or processed/193 claim',
        'fiveExplicitExceptions': item(HERE / 'five-exceptions.json'),
        'twoSupportedModeCoverage': item(ART / 'two-mode-coverage.json'),
        'complementaryActualAndNativeTwoPASS': item(ART / 'complementary-result.json'),
        'matchedNativeExplicitLegacyLocale': item(ART / 'locale-control-result.json'),
        'integratedGate': item(HERE / 'acceptance.py'),
        'integratedControls': item(ART / 'integrated-acceptance-controls.json'),
        'originalPrimaryIntegratedDecision': item(ART / 'first-full-integrated-acceptance.json'),
        'startupSourcePins': item(ART / 'tested-startup-source-pins.json'),
        'remaining': ['Final correctly-started full primary mode0/C.UTF-8 must execute Unicode-directory PASS; original ERROR remains preserved',
            'Exact2 conditional mode0 skips require fresh same-actual-PE supported mode1 native/actual PASS; only exact5 raw SKIP/XFAIL acceptance exceptions',
            'Actual final193 and current286 raw/processed JIT/NativeAOT full qualification',
            'Complete same-PE D10/address/cache/pin/lifetime/semantic/ABI/thread/GC/lifecycle/rooted/trimmed/delivery/audit',
            'Final-tool/current-runtime downstream/regeneration/dependency/licenses/offline clean output/reproduction/P6'],
        'authoredSources': [item(p) for p in sorted(HERE.iterdir()) if p.is_file()]}
    assert not (ART / 'actual-result.json').exists()
    write('actual-result.json', result)
    locks = []
    for lock in ('/tmp/dotcc-cpython-shared-build.lock', '/tmp/dotcc-cpython-native-build.lock',
                 str(CPY / 'build/campaign/run.lock')):
        with Path(lock).open('r+') as stream:
            fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            fcntl.flock(stream, fcntl.LOCK_UN)
        locks.append({'path': lock, 'independentAcquireAndRelease': True, 'freeAtTransfer': True})
    assert not (ART / 'direct-safe-to-a32.json').exists()
    safe = write('direct-safe-to-a32.json', {'status': 'DIRECT GENUINE SAFE to replacement A32/supervisor; B40 source/reporting only',
        'utc': datetime.datetime.now(datetime.timezone.utc).isoformat(), 'terminalPidsAbsent': pids,
        'noPrivateOutputProcessOrDriverRemains': True, 'activeOwned': [], 'queuedOwned': [], 'locks': locks,
        'actualResult': item(ART / 'actual-result.json'), 'priorDirectAuthority': item(ART / 'direct-a31-authority.json'),
        'commandCount': len(commands), 'commandSeconds': result['commandSeconds'],
        'noExtraHeavyWorkQueued': True,
        'PLANreleased': True, 'startupSources': result['startupSourcePins'],
        'fiveExceptionManifest': result['fiveExplicitExceptions'], 'twoModeCoverage': result['twoSupportedModeCoverage'],
        'integratedGateControls': result['integratedControls'],
        'next': 'A32 routine authorized final193/BOTH286 generation and actual D10, then current-PE final matrix/P6; no further permission round'})
    write('final-handoff.json', {'status': 'B40 source integration ready; genuine SAFE to A32; final primary/matrix remains OPEN',
        'actualResult': item(ART / 'actual-result.json'), 'genuineSafe': safe,
        'startupSourcePins': result['startupSourcePins'], 'fiveExceptions': result['fiveExplicitExceptions'],
        'twoModeCoverage': result['twoSupportedModeCoverage'], 'integratedGate': result['integratedGate'],
        'controls': result['integratedControls'], 'PLANreleased': True, 'activeOwned': [], 'queuedOwned': [],
        'scope': 'No production Host/runtime/header/compiler change; no new full or AOT run queued. Firstfull/raw per-mode counts and failed harness controls preserved.'})
    print(json.dumps({'release': safe, 'uniqueOutcomes': counts['uniqueOutcomes'],
        'events': counts['outcomeEvents'], 'remainingNonpasses': len(nonpasses), 'commandSeconds': result['commandSeconds']}))


if __name__ == '__main__':
    main()
