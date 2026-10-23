"""Read-only validation of retained original Python execution and input files."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def evidence_path(path):
    path = Path(path)
    return path.resolve() if path.is_absolute() else (ROOT.parent / path).resolve()


class Validator:
    def __init__(self):
        self.checked = {}

    def file(self, path, expected=None):
        path = str(evidence_path(path))
        observed = hashlib.sha256(Path(path).read_bytes()).hexdigest()
        if expected is not None and observed != expected:
            raise RuntimeError('Retained Python execution input changed: ' + path)
        if path in self.checked and self.checked[path] != observed:
            raise RuntimeError('Python evidence changed during validation: ' + path)
        self.checked[path] = observed

    def files(self, entries):
        if not entries:
            raise RuntimeError('Required Python identity inventory is empty')
        for path, expected in entries.items():
            self.file(path, expected)

    def family(self, report, row):
        process = row.get('process', {})
        if (row['status'] != 'passed' or row.get('exit_code') != 0 or process.get('exit_code') != 0
            or process.get('timed_out') is not False or process.get('drained') is not True
            or process.get('remaining_endpoints') is not False):
            raise RuntimeError('Original Python family did not pass and drain')
        self.file(row['parameters'])
        result = json.loads(Path(row['parameters']).read_text())
        collected = [r['nodeid'] for r in result['collected']]
        passed = [r['nodeid'] for r in result['outcomes'] if r['phase'] == 'call' and r['outcome'] == 'passed']
        if (result['status'] != 'passed' or result['exit_code'] != 0 or result['failed'] or result['skipped']
            or result['collection_errors'] or not collected or len(set(collected)) != len(collected)
            or sorted(passed) != sorted(collected) or result['passed'] != len(collected)
            or row['passed'] != len(collected) or row['parameter_cases'] != len(collected)
            or any(r['outcome'] != 'passed' for r in result['outcomes'])
            or any(r['predicate'] is None or r['product_capability_vetoes'] for r in result['deselected'])):
            raise RuntimeError('Original Python assertion inventory contains missing, skipped, failed or retried cases')
        for key in ('endpoint_manifest_sha256', 'endpoint_closure_sha256', 'native_peer_sha256',
                    'peer_configuration_sha256', 'fixture_sha256', 'frozen_adapter_sha256'):
            self.files(report[key])
        if report.get('frozen_runner_sha256'):
            self.files(report['frozen_runner_sha256'])
        if report.get('measured_psk_peer'):
            self.files(report['measured_psk_peer']['inputs_sha256'])
        if row['case'] == 'tests/integrationv2/test_hello_retry_requests.py':
            from hrr_applicability import validate_proof
            proof = report.get('hrr_applicability')
            excluded = result.get('original_inapplicable', [])
            if not proof or result['passed'] != 118 or len(excluded) != 9:
                raise RuntimeError('Original HRR family lacks its exact applicable/inapplicable classification')
            validate_proof(proof['proof'], report['endpoint_kind'], report['form'], report['mode'])
            if sorted(row['nodeid'] for row in excluded) != proof['original_nodeids']:
                raise RuntimeError('Original HRR inapplicable nodes differ from their separate controls')
            self.files(proof['inputs_sha256'])
        command_sources = [Path(value.split('::', 1)[0]) for value in row['command'] if '.py' in value]
        stage = Path(report['original_source_root']) if report.get('original_source_root') else next(
            parent for path in command_sources for parent in path.parents
            if parent.name == 'source' and (parent / 'tests/integrationv2').is_dir())
        for relative, expected in report['python_source_sha256'].items():
            path = (stage / relative).resolve()
            if not path.is_relative_to(stage.resolve()):
                raise RuntimeError('Original Python source path escapes its frozen tree')
            self.file(path, expected)
        if report.get('authored_control_sha256'):
            self.files(report['authored_control_sha256'])
        for path in report['endpoint_manifest_sha256']:
            manifest = json.loads(evidence_path(path).read_text())
            entries = [r for r in manifest['executables'] if r['status'] == 'built'
                       and (r['form'], r['mode']) == (report['form'], report['mode'])
                       and Path(r['case']).stem in ('s2nc', 's2nd')]
            if len(entries) != 2 or {Path(r['case']).stem for r in entries} != {'s2nc', 's2nd'}:
                raise RuntimeError('Original Python report lacks its exact two real endpoint builds')
            for entry in entries:
                self.files(entry['execution_sha256'])
                if report['endpoint_kind'] != 'translated':
                    continue
                self.file(entry['application_execution_receipt'], entry['application_execution_receipt_sha256'])
                identity = json.loads(Path(entry['application_execution_receipt']).read_text())
                if (identity['command'] != entry['command'] or identity['actual_runtime']['kind'] != 'actual-runtime-identity'
                    or identity['actual_runtime']['dynamic_code'] != (report['mode'] == 'jit')):
                    raise RuntimeError('Translated CLI actual application/runtime identity differs')
                for key in ('output_sha256', 'runtime_closure_sha256', 'authored_source_sha256'):
                    self.files(identity[key])
                self.files(entry['dependency_inputs_sha256'])
                for unit in identity['product_snapshot']['core_tu_coverage']:
                    self.file(unit['object'], unit['object_sha256'])
                    self.file(unit['staged_source'], unit['staged_source_sha256'])
        self.file(row['log'])
        for service in report.get('services', []):
            if not service['drained'] or service['status'] != 'stopped':
                raise RuntimeError('Declared Python peer service did not stop and drain')
            self.files(service['log_sha256'])
        return result
