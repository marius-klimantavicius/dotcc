"""Explicit five-exception CPython acceptance, separate from raw unittest data.

This is not a DotCC CI waiver and never changes the frozen inventory, executed
tests, decorators, raw statuses, reports, counts, or process exit codes.
"""
from collections import Counter, defaultdict
import hashlib
import json
from pathlib import Path

EXACT_IDS = frozenset((
    'test.test_descr.ClassPropertiesAndMethods.test_bad_new',
    'test.test_descr.ClassPropertiesAndMethods.test_restored_object_new',
    'test.test_importlib.resources.test_resource.DeletingZipsTest.test_as_file_does_not_keep_open',
    'test.test_importlib.test_namespace_pkgs.ZipWithMissingDirectory.test_missing_directory',
    'test.test_threading.ConditionAsRLockTests.test_recursion_count',
))
TWO_MODE_IDS = frozenset((
    'test.test_codecs.LocaleCodecTest.test_decode_surrogatepass',
    'test.test_codecs.LocaleCodecTest.test_encode_surrogatepass',
))


def require(condition, message):
    if not condition:
        raise ValueError(message)


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def validate_policy(policy, inventory, markers, native):
    rows = policy['exceptions']
    require(policy['scope'] == 'CPython frozen upstream cases only' and policy['explicitUserAuthorization'], 'Wrong policy scope/authority')
    require(len(rows) == 5 and {r['case'] for r in rows} == EXACT_IDS, 'Wrong authorized exception IDs')
    require(Counter(r['status'] for r in rows) == {'required-skip': 2, 'required-expected-failure': 3}, 'Wrong exception statuses')
    require(inventory['status'] == 'frozen' and len(inventory['required_cases']) == 4867, 'Wrong frozen inventory')
    original = {r['case']: r for r in markers['cases']}
    witnesses = {r['case']: r for r in native['witnesses']}
    require(len(original) == len(witnesses) == 104, 'Incomplete original/native marker closure')
    for row in rows:
        case = row['case']
        require(case in inventory['required_cases'] and case in original and case in witnesses, 'Exception outside frozen/native closure')
        source = original[case]['source']
        require(row['sourceSha256'] == source['file']['sha256'] and row['methodSource'] == source['originalMethodSource'], 'Wrong exception source')
        require(row['expectedFailure'] == source['expectedFailure'] and row['method'] == source['method']
                and row['declaringClass'] == source['declaringClass'], 'Wrong method/decorator binding')
        require(row['status'] == original[case]['status'], 'Wrong original exception status')
        witness = witnesses[case]['nativeModeWitnesses']['0']
        require(witness['availability'] == 'executed exact unchanged case'
                and witness['originalSource']['sha256'] == row['sourceSha256'], 'Wrong native source witness')
        require(len(witness['outcomes']) == 1, 'Wrong native occurrence count')
        outcome = witness['outcomes'][0]
        require(outcome['case'] == case and outcome['status'] == row['status']
                and outcome.get('reason') == row.get('reason'), 'Wrong native status/reason witness')
        if row['status'] == 'required-expected-failure':
            require(row['expectedFailure'] and bool(outcome.get('traceback')), 'Missing real native expected failure')
        else:
            require(original[case]['category'] == 'unconditional' and bool(row.get('reason')), 'Skip is not the authorized unconditional marker')


def assess(report, inventory, markers, native, policy, source_hashes):
    validate_policy(policy, inventory, markers, native)
    require(report['implementation'] == 'translated' and report['scope'] == 'selected-upstream-cases', 'Not full translated execution')
    require(report['selected_case_ids'] == inventory['required_cases'], 'Changed/subset selection')
    require(len(report['discovery']) == 23 and all(g['status'] == 'discovered' for g in report['discovery']), 'Incomplete discovery')
    discovered = [case for group in report['discovery'] for case in group['cases']]
    require(len(discovered) == len(set(discovered)) and set(discovered) == set(inventory['discovered_cases']), 'Changed discovery IDs')
    require(set(report.get('helper_origins', {})) == set(inventory['required_helper_modules'])
            and set(report['helper_origins'].values()) == {'built-in'}, 'Wrong static helper closure/origin')
    details = {r['case']: r for group in report['discovery'] for r in group['case_details']}
    exceptions = {r['case']: r for r in policy['exceptions']}
    for case, exception in exceptions.items():
        require(case in details, 'Missing exception source detail')
        detail = details[case]
        require(source_hashes.get(case) == exception['sourceSha256'], 'Actual exception source bytes differ')
        require(detail['source'] == exception['methodSource'] and detail['method'] == exception['method']
                and detail['declaring_class'] == exception['declaringClass']
                and detail['expected_failure'] == exception['expectedFailure'], 'Actual method/decorator/source differs')
    outcomes = defaultdict(list)
    for row in report['cases']:
        outcomes[row['case']].append(row)
    required = set(inventory['required_cases'])
    blockers = []
    missing = sorted(required - outcomes.keys())
    for case in sorted(outcomes.keys() - required):
        blockers.append({'case': case, 'status': 'unexpected-id'})
    for case in sorted(required & outcomes.keys()):
        for row in outcomes[case]:
            if case in exceptions:
                exception = exceptions[case]
                if row['status'] != exception['status'] or row.get('reason') != exception.get('reason'):
                    blockers.append({'case': case, 'status': row['status'], 'reason': 'Authorized native status/reason changed'})
            elif row['status'] != 'passed':
                blockers.append(row)
    blockers.extend({'case': case, 'status': 'unrun-or-fatal'} for case in missing)
    require(not report.get('failure'), 'Interrupted/fatal runner report')
    accepted = [outcomes[case][0] for case in sorted(EXACT_IDS) if case in outcomes
                and all(row['status'] == exceptions[case]['status'] and row.get('reason') == exceptions[case].get('reason')
                        for row in outcomes[case])]
    return {'acceptance': 'accepted-with-five-explicit-native-parity-exceptions' if not blockers and len(accepted) == 5 else 'blocked',
        'rawOutcomeEvents': dict(Counter(row['status'] for row in report['cases'])),
        'acceptedExceptionCount': len(accepted), 'acceptedExceptions': accepted,
        'blockingOutcomes': blockers, 'rawRunnerStatus': report['status'],
        'rawQualifiesTranslatedInterpreter': report['qualifies_translated_interpreter'],
        'scope': 'This CPython upstream acceptance decision does not close other campaign gates or waive DotCC CI.'}


def evaluate(report_path, journal_path, actual_proof_path, execution_proof_path, commands_path, policy_path):
    def read(path):
        return json.loads(Path(path).read_text())
    def reference(path):
        return {'path': str(Path(path).resolve()), 'sha256': digest(path)}
    policy = read(policy_path)
    inputs = {}
    for key, ref in policy['inputs'].items():
        require(digest(ref['path']) == ref['sha256'], 'Policy input bytes changed: ' + key)
        inputs[key] = read(ref['path'])
    report = read(report_path)
    rows = [json.loads(line) for line in Path(journal_path).read_text().splitlines()]
    outcomes = [r for r in rows if r['status'] != 'started']
    require(outcomes == report['cases'], 'Raw report differs from durable case journal')
    started = Counter(r['case'] for r in rows if r['status'] == 'started')
    completed = Counter(r['case'] for r in outcomes)
    require(started == completed and set(started) == set(inputs['inventory']['required_cases']), 'Unrun/fatal/extra journal IDs or occurrences')
    expected_occurrences = Counter(inputs['inventory']['required_cases'])
    for group in report['discovery']:
        for case, count in group['duplicate_loader_occurrences'].items():
            if case in expected_occurrences:
                expected_occurrences[case] += count - 1
    require(started == expected_occurrences, 'Changed upstream loader occurrence counts')
    for status, count in Counter(r['status'] for r in outcomes).items():
        require(report['counts'][status] == count, 'Raw report counts differ')
    proof = read(actual_proof_path)
    execution = read(execution_proof_path)
    entry = execution.get('entry', proof['entry'])
    entry = entry.get('identity', entry)
    require(digest(entry['path']) == entry['sha256'], 'Actual entry bytes changed')
    library = proof['library']['identity']
    facade = proof['facade']['identity']
    require(all(digest(ref['path']) == ref['sha256'] for ref in (library, facade)), 'Actual product/facade bytes changed')
    refs = execution.get('references', proof['references'])
    for selected in (library, facade):
        require(any(Path(ref['path']).name == Path(selected['path']).name and ref['sha256'] == selected['sha256']
                    and digest(ref['path']) == ref['sha256'] for ref in refs), 'Wrong execution library/facade')
    matching = [r for r in read(commands_path) if entry['path'] in r['command'] and str(Path(report_path).resolve()) in r['command']]
    require(len(matching) == 1 and matching[0]['status'] in ('passed', 'failed') and matching[0]['exit_code'] in (0, 1), 'No matching completed actual command')
    details = {r['case']: r for group in report['discovery'] for r in group['case_details']}
    hashes = {case: digest(details[case]['upstream_path']) for case in EXACT_IDS}
    result = assess(report, inputs['inventory'], inputs['markers'], inputs['native'], policy, hashes)
    result['bindings'] = {name: reference(path) for name, path in (
        ('rawReport', report_path), ('rawJournal', journal_path), ('actualProductProof', actual_proof_path),
        ('executionProof', execution_proof_path), ('commands', commands_path), ('exceptionPolicy', policy_path))}
    result['actualLibrary'] = library
    result['actualEntry'] = entry
    return result


def validate_coverage(coverage, expected_sources, actual_library):
    require(coverage['scope'] == 'exact two supported startup configurations', 'Wrong configuration coverage scope')
    require(set(coverage['caseIds']) == TWO_MODE_IDS and len(coverage['caseIds']) == 2, 'Wrong configuration coverage IDs')
    require(coverage['actualLibrary']['sha256'] == actual_library['sha256']
            and coverage['actualLibrary']['mvid'] == actual_library['mvid'], 'Wrong actual PE for complementary coverage')
    for role in ('native', 'actual'):
        report = coverage[role]
        require(report['executed'] == 2 and {r['case'] for r in report['cases']} == TWO_MODE_IDS
                and len(report['cases']) == 2 and all(r['status'] == 'passed' for r in report['cases']), 'Missing/nonPASS complementary ' + role + ' witness')
        for stage in ('beforeTests', 'afterTests'):
            flags = report[stage]
            require(flags['utf8_mode'] == 1 and flags['isolated'] == flags['ignore_environment'] == 1
                    and flags['locale'] == 'C.UTF-8' and flags['filesystemencoding'] == 'utf-8', 'Wrong complementary mode/locale: ' + role)
        witnesses = report['sourceWitnesses']
        require(len(witnesses) == 2 and {r['case'] for r in witnesses} == TWO_MODE_IDS, 'Missing complementary source witnesses')
        for row in witnesses:
            require(row['sourceSha256'] == expected_sources[row['case']]
                    and row['methodAndDecoratorUnchanged'], 'Wrong complementary source/native method binding')


def assess_integrated(report, inventory, markers, native, policy, source_hashes, coverage, actual_library):
    result = assess(report, inventory, markers, native, policy, source_hashes)
    if coverage is None:
        result['complementaryAssertionCoverage'] = []
        return result
    validate_coverage(coverage, source_hashes, actual_library)
    recognized_reasons = {
        'test.test_codecs.LocaleCodecTest.test_decode_surrogatepass': "'utf-8' decoder doesn't support surrogatepass error handler",
        'test.test_codecs.LocaleCodecTest.test_encode_surrogatepass': "'utf-8' encoder doesn't support surrogatepass error handler",
    }
    covered = []
    blockers = []
    for row in result['blockingOutcomes']:
        if row['case'] in TWO_MODE_IDS and row['status'] == 'required-skip' and row.get('reason') == recognized_reasons[row['case']]:
            covered.append(row)
        else:
            blockers.append(row)
    config = report['interpreter_configuration']
    startup = (config['utf8_mode'] == 0 and config['isolated'] == config['ignore_environment'] == 1
               and config['filesystem_encoding'] == 'utf-8')
    if not startup:
        blockers.append({'case': '<primary-startup>', 'status': 'wrong-startup',
            'reason': 'Primary full execution requires native-faithful preinit C.UTF-8 with Utf8Mode=false'})
    result['blockingOutcomes'] = blockers
    result['complementaryAssertionCoverage'] = covered
    result['primaryStartupQualified'] = startup
    result['acceptance'] = ('accepted-with-five-exceptions-and-two-configuration-witnesses'
        if not blockers and len(result['acceptedExceptions']) == 5 else 'blocked')
    result['configurationCoverageDoesNotRelabelRawSkips'] = True
    return result


def load_coverage(manifest_path, actual_library):
    manifest = json.loads(Path(manifest_path).read_text())
    def checked(ref):
        require(digest(ref['path']) == ref['sha256'], 'Complementary evidence bytes changed')
        return json.loads(Path(ref['path']).read_text())
    result = checked(manifest['result'])
    entry = checked(result['actualInput'])
    require(digest(entry['entry']['path']) == entry['entry']['sha256'], 'Complementary entry bytes changed')
    for path, sha in entry['sourceBinding']['actualCompileInputs'].items():
        require(digest(path) == sha, 'Complementary actual Compile/PDB bytes changed')
    require(any(Path(r['path']).name == 'TranslatedCPython.dll' and r['sha256'] == actual_library['sha256']
                and digest(r['path']) == r['sha256'] for r in entry['references']), 'Complementary actual library mismatch')
    for ref in result['nativeInputs']:
        require(digest(ref['path']) == ref['sha256'], 'Native complementary executable/source/archive changed')
    commands = checked(result['commands'])
    native = checked(result['native'])
    actual = checked(result['actual'])
    for report_ref, executable in ((result['actual'], entry['entry']['path']), (result['native'], result['nativeInputs'][0]['path'])):
        require(any(executable in r['command'] and report_ref['path'] in r['command']
                    and r['status'] == 'passed' and r['exit_code'] == 0 for r in commands), 'No matching complementary execution command')
    return {'scope': manifest['scope'], 'caseIds': manifest['caseIds'], 'actualLibrary': actual_library,
            'native': native, 'actual': actual, 'manifest': {'path': str(manifest_path), 'sha256': digest(manifest_path)}}


def evaluate_integrated(report_path, journal_path, actual_proof_path, execution_proof_path, commands_path, policy_path, coverage_manifest):
    strict = evaluate(report_path, journal_path, actual_proof_path, execution_proof_path, commands_path, policy_path)
    policy = json.loads(Path(policy_path).read_text())
    inputs = {key: json.loads(Path(ref['path']).read_text()) for key, ref in policy['inputs'].items()}
    report = json.loads(Path(report_path).read_text())
    details = {r['case']: r for group in report['discovery'] for r in group['case_details']}
    hashes = {case: digest(details[case]['upstream_path']) for case in EXACT_IDS | TWO_MODE_IDS}
    coverage = load_coverage(coverage_manifest, strict['actualLibrary'])
    result = assess_integrated(report, inputs['inventory'], inputs['markers'], inputs['native'], policy,
        hashes, coverage, strict['actualLibrary'])
    result['bindings'] = strict['bindings']
    result['bindings']['configurationCoverage'] = coverage['manifest']
    result['actualLibrary'] = strict['actualLibrary']
    result['actualEntry'] = strict['actualEntry']
    return result
