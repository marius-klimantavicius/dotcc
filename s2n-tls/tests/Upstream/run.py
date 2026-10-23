"""Preserve every frozen upstream case, assertion count and exact exclusion.

The executable manifest is produced by build.py. Missing harnesses/native
comparisons and any capability skip leave required cases unrun/failed.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
from feature_adaptations import apply as apply_features

ROOT = Path(__file__).resolve().parents[2]


def python_reports(paths, endpoint_kind, form=None, mode=None):
    """Combine disjoint family receipts without rerunning their controllers."""
    families = {}
    for path in paths:
        report = json.loads(path.read_text())
        if report.get('evidence_kind') == 'original-python-matrix':
            if report['endpoint_kind'] != endpoint_kind:
                raise RuntimeError('Original Python matrix endpoint kind differs: ' + str(path))
            for entry in report['cases']:
                if form is not None and (entry['form'], entry['mode']) != (form, mode):
                    continue
                if entry['status'] != 'passed':
                    continue
                leaf = Path(entry['report'])
                if hashlib.sha256(leaf.read_bytes()).hexdigest() != entry['report_sha256']:
                    raise RuntimeError('Original Python matrix leaf identity changed: ' + str(leaf))
                data = json.loads(leaf.read_text())
                if data.get('evidence_kind') != 'original-python-integration' or data['endpoint_kind'] != endpoint_kind:
                    raise RuntimeError('Matrix leaf is not an original complete-family execution')
                if (data['form'], data['mode']) != (entry['form'], entry['mode']):
                    raise RuntimeError('Matrix leaf execution variant differs')
                selected = [row for row in data['cases'] if row['case'] == entry['case']]
                if len(selected) != 1 or entry['case'] in families:
                    raise RuntimeError('Missing or duplicate exact original Python matrix family')
                families[entry['case']] = (selected[0], data, leaf)
            continue
        if report.get('evidence_kind') != 'original-python-integration':
            raise RuntimeError('A focused diagnostic cannot qualify an original Python family: ' + str(path))
        if report.get('endpoint_kind') != endpoint_kind:
            raise RuntimeError('Original Python endpoint kind differs: ' + str(path))
        if form is not None and (report.get('form'), report.get('mode')) != (form, mode):
            raise RuntimeError('Original Python execution variant differs: ' + str(path))
        if not report.get('python_source_sha256'):
            raise RuntimeError('Original Python source inventory is missing: ' + str(path))
        for row in report['cases']:
            if row['case'] in families:
                raise RuntimeError('Duplicate original Python family receipt: ' + row['case'])
            families[row['case']] = (row, report, path)
    return families


def successful_parameters(row, report=None):
    if report is not None:
        from python_receipt import Validator
        return Validator().family(report, row)
    process = row.get('process', {})
    if (row.get('status') != 'passed' or row.get('exit_code') != 0 or process.get('exit_code') != 0
        or process.get('timed_out') is not False or process.get('drained') is not True
        or process.get('remaining_endpoints') is not False):
        raise RuntimeError('Original Python family did not pass and drain its endpoints')
    parameters = json.loads(Path(row['parameters']).read_text())
    collected = [r['nodeid'] for r in parameters['collected']]
    passed = [r['nodeid'] for r in parameters['outcomes'] if r['phase'] == 'call' and r['outcome'] == 'passed']
    if (parameters.get('status') != 'passed' or parameters.get('exit_code') != 0
        or parameters.get('failed') != 0 or parameters.get('skipped') != 0 or parameters['collection_errors']
        or not collected or len(set(collected)) != len(collected) or sorted(passed) != sorted(collected)
        or parameters.get('passed') != len(collected) or row.get('passed') != len(collected)
        or row.get('parameter_cases') != len(collected)
        or any(r['outcome'] != 'passed' for r in parameters['outcomes'])
        or any(r['predicate'] is None or r['product_capability_vetoes'] for r in parameters['deselected'])):
        raise RuntimeError('Original Python parameter/assertion execution is incomplete, failed or skipped')
    return parameters


def inventory(form, mode):
    config = json.loads((ROOT / 'config/upstream-tests.json').read_text())
    rows = []
    for case in config['cases']:
        status = case['status'] if case['status'] in ('excluded', 'support') else 'unrun'
        rows.append({'suite': 'upstream', 'form': form, 'mode': mode, 'case': case['path'],
                     'category': case['category'], 'status': status, 'reason': case['reason'],
                     'assertion_sites': case['assertion_sites'], 'test_functions': case['test_functions'],
                     'conditional_or_skip_sites': case['conditional_or_skip_sites'],
                     'assertions': 0, 'skipped': 0})
    if len(rows) != len({r['case'] for r in rows}):
        raise RuntimeError('Duplicate frozen upstream case')
    return rows


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--form', choices=('raw', 'processed'), default='raw')
    parser.add_argument('--mode', choices=('jit', 'aot'), default='jit')
    parser.add_argument('--executables', type=Path, default=ROOT / 'build/managed-upstream/executables.json')
    parser.add_argument('--native-report', type=Path)
    parser.add_argument('--native-equivalence', type=Path, help='Matching-profile native equivalence proof for a retained C aggregate')
    parser.add_argument('--c-report', type=Path, help='Consume the completed provider-owned 257-case aggregate; never launch another C controller')
    parser.add_argument('--campaign-run', type=Path, help='Require the retained C aggregate to belong to this immutable campaign')
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--inventory-only', action='store_true', help='Write a static inventory; required cases remain unrun and exit status is nonzero.')
    parser.add_argument('--python-executables', type=Path, default=ROOT / 'build/managed-upstream/python-endpoints.json')
    parser.add_argument('--peer-commands', type=Path)
    parser.add_argument('--python', default=sys.executable)
    parser.add_argument('--python-workers', type=int, default=1)
    parser.add_argument('--python-report', type=Path, action='append', help='Consume existing disjoint original Python family reports; do not launch another controller')
    parser.add_argument('--native-python-report', type=Path, action='append')
    parser.add_argument('--c-only', action='store_true', help='Qualify only the complete required C inventory; Python acceptance remains a separate gate')
    args = parser.parse_args()
    if args.c_only and args.python_report: parser.error('--c-only and --python-report are separate selections')
    if args.c_report and (args.inventory_only or not args.native_report):
        parser.error('--c-report requires --native-report, without --inventory-only; cross-campaign controls additionally require --native-equivalence')
    args.output = args.output.resolve()
    report = {'evidence_kind': 'translated-upstream', 'status': 'failed', 'cases': inventory(args.form, args.mode)}
    if args.c_only:
        report.update(evidence_kind='translated-upstream-c', scope='Original C inventory only; the 18 required Python families are outside this report.')
        report['cases'] = [row for row in report['cases'] if row['case'].endswith('.c')]
    manifest = json.loads(args.executables.read_text()) if not args.c_report and args.executables.is_file() else {'executables': []}
    native_rows = {}
    retained_c, c_error = {}, None
    if args.c_report:
        try:
            sys.path.insert(0, str(ROOT / 'tests/Provider'))
            from required_c_receipt import validate_completed_report
            completed = validate_completed_report(args.c_report, args.native_report, args.native_equivalence,
                                                  campaign_run=args.campaign_run)
            selected = [row for row in completed['cases'] if (row['form'], row['mode']) == (args.form, args.mode)]
            required = {row['case'] for row in report['cases'] if row['status'] == 'unrun' and row['case'].endswith('.c')}
            if len(selected) != len(required) or {row['case'] for row in selected} != required:
                raise RuntimeError('Retained C aggregate lacks the exact required case/variant inventory')
            retained_c = {row['case']: row for row in selected}
            report['retained_c_validation'] = {key: value for key, value in completed.items() if key != 'cases'}
        except Exception as error:
            c_error = str(error)
    elif not args.inventory_only:
        try:
            sys.path.insert(0, str(ROOT / 'tests/Provider'))
            from required_c_receipt import require_passed_native
            native = json.loads(args.native_report.read_text()) if args.native_report else {}
            require_passed_native(native)
            native_rows = {r.get('case', 'tests/unit/' + str(r.get('name')) + '.c'): r
                           for r in native.get('upstream', []) if r['status'] == 'passed'}
        except Exception as error:
            c_error = str(error)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    for index, row in enumerate(report['cases']):
        if row['status'] != 'unrun': continue
        if row['case'].endswith('.py'): continue
        if args.c_report:
            if c_error:
                row.update(status='failed', error=c_error)
            else:
                row.update(retained_c[row['case']])
            continue
        entries = [e for e in manifest['executables'] if e['case'] == row['case'] and e['form'] == args.form and e['mode'] == args.mode]
        if args.inventory_only:
            row['reason'] = 'Static inventory only; required translated harness remains unrun.'; continue
        if c_error:
            row.update(status='failed', error=c_error)
            continue
        if len(entries) != 1 or entries[0].get('status') != 'built':
            row['reason'] = 'Required exact translated harness is missing or failed to build.'; continue
        entry = entries[0]
        row['status'] = 'failed'
        try:
            original = ROOT / 'ref/s2n-tls' / row['case']
            staged = Path(entry['compiled_source'])
            if hashlib.sha256(original.read_bytes()).hexdigest() != entry['original_sha256']:
                raise RuntimeError('Upstream input changed since harness compilation')
            adapted, adaptations = apply_features(row['case'], original.read_bytes())
            if entry.get('test_adaptations', []) != adaptations:
                raise RuntimeError('Exact test adaptation provenance differs from the accepted conditional-only guards')
            if staged.read_bytes() != b'#include "utils/s2n_prelude.h"\n#include "dotcc_s2n_host.h"\n' + adapted:
                raise RuntimeError('Harness changed upstream assertions/source beyond the recorded prelude/accepted conditional guards')
            row['test_adaptations'] = adaptations
            if hashlib.sha256(staged.read_bytes()).hexdigest() != entry['compiled_sha256']:
                raise RuntimeError('Compiled harness source identity changed')
            for path, expected in entry['execution_sha256'].items():
                if hashlib.sha256(Path(path).read_bytes()).hexdigest() != expected:
                    raise RuntimeError('Executable identity changed: ' + path)
            log = args.output.parent / (args.output.stem + f'-{index:04d}.log')
            with log.open('w') as stream:
                proc = subprocess.run(entry['command'], cwd=ROOT / 'ref/s2n-tls/tests/unit',
                                      stdout=stream, stderr=subprocess.STDOUT, timeout=120)
            row.update(log=str(log), exit_code=proc.returncode)
            transcript = log.read_text()
            matches = re.findall(r'PASSED\s+(\d+) tests', transcript)
            if proc.returncode != 0 or len(matches) != 1 or int(matches[0]) <= 0:
                raise RuntimeError('Missing successful preserved upstream assertion count')
            if re.search(r'\bSKIP(?:PED)?\b', transcript, re.I):
                raise RuntimeError('Required capability/whole-test skip is not accepted')
            row['assertions'] = int(matches[0])
            comparison = native_rows.get(row['case'])
            if comparison is None:
                raise RuntimeError('Matching native assertion execution is missing; translated-only execution does not close this case')
            if comparison.get('test_adaptations', []) != adaptations:
                raise RuntimeError('Matching native outside-PLAN conditional profile receipt is missing or differs')
            expected = comparison['assertions']
            row['native_assertions'] = expected
            if row['assertions'] != expected:
                raise RuntimeError('Native/translated runtime assertion counts differ')
            row['status'] = 'passed'
        except Exception as error:
            row['error'] = str(error)
    if not args.inventory_only and not args.c_only:
        python_path = args.output.parent / (args.output.stem + '-python.json')
        command = [sys.executable, str(ROOT / 'tests/Upstream/python-integration.py'), '--form', args.form, '--mode', args.mode,
                   '--executables', str(args.python_executables), '--python', args.python, '--workers', str(args.python_workers), '--output', str(python_path)]
        if args.peer_commands: command += ['--peer-commands', str(args.peer_commands)]
        if not args.python_report and not args.c_report:
            subprocess.run(command, timeout=259200)
        python_paths = args.python_report or ([python_path] if python_path.is_file() else [])
        try:
            actual_families = python_reports(python_paths, 'translated', args.form, args.mode)
            native_families = python_reports(args.native_python_report or [], 'native')
            report['python_reports_sha256'] = {str(p): hashlib.sha256(p.read_bytes()).hexdigest()
                for p in [*python_paths, *(args.native_python_report or [])]}
            python_error = None
        except Exception as error:
            actual_families, native_families = {}, {}
            python_error = str(error)
        for row in report['cases']:
            if not row['case'].endswith('.py') or row['status'] in ('excluded', 'support'): continue
            actual_family = actual_families.get(row['case'])
            if actual_family is None:
                row.update(status='failed', error=python_error or 'Required original Python adapter produced no family receipt'); continue
            actual, python_result, actual_path = actual_family
            row.update(actual)
            if row['status'] == 'passed':
                native_family = native_families.get(row['case'])
                if native_family is None:
                    row.update(status='failed', error='Matching original native Python parameter/assertion receipt is missing'); continue
                expected, native_python, native_path = native_family
                try:
                    actual_parameters = successful_parameters(actual, python_result)
                    native_parameters = successful_parameters(expected, native_python)
                    if actual_parameters['collected'] != native_parameters['collected']:
                        raise RuntimeError('Original native/translated Python parameter cases differ')
                    if actual_parameters['deselected'] != native_parameters['deselected']:
                        raise RuntimeError('Original native/translated Python parameter exclusions differ')
                    if python_result['python_source_sha256'] != native_python['python_source_sha256']:
                        raise RuntimeError('Original native/translated Python source inventories differ')
                    def applicable_exclusions(parameters):
                        return [{key: value for key, value in row.items() if key != 'supplemental_proof'}
                                for row in parameters.get('original_inapplicable', [])]
                    if applicable_exclusions(actual_parameters) != applicable_exclusions(native_parameters):
                        raise RuntimeError('Original native/translated inapplicable combinations differ')
                    if args.c_report:
                        for path in python_result['endpoint_manifest_sha256']:
                            endpoint_manifest = json.loads(Path(path).read_text())
                            if Path(endpoint_manifest['campaign_run']).resolve() != Path(report['retained_c_validation']['campaign_run']).resolve():
                                raise RuntimeError('Original Python CLI and original C aggregate belong to different campaigns')
                    row.update(translated_python_report=str(actual_path), native_python_report=str(native_path),
                        parameter_receipts_sha256={str(p): hashlib.sha256(p.read_bytes()).hexdigest()
                            for p in (Path(actual['parameters']), Path(expected['parameters']))})
                except Exception as error:
                    row.update(status='failed', error=str(error))
    report['counts'] = {s: sum(r['status'] == s for r in report['cases']) for s in ('passed', 'failed', 'unrun', 'excluded', 'support')}
    if not report['counts']['failed'] and not report['counts']['unrun'] and report['counts']['passed']:
        report['status'] = 'passed'
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps({'status': report['status'], 'counts': report['counts'], 'report': str(args.output)}))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
