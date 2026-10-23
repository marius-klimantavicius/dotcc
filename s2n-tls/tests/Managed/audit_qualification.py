"""Independently verify the retained files identified by managed qualification.

This checks evidence integrity, not TLS behavior. The original execution reports
and transcripts remain the authority for their assertions.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


class Audit:
    def __init__(self):
        self.files = {}
        self.applications = {}
        self.copied_sources = []

    def check(self, path, expected=None):
        path = Path(path).resolve()
        observed = hashlib.sha256(path.read_bytes()).hexdigest()
        if expected is not None and observed != expected:
            raise RuntimeError('Retained evidence changed: ' + str(path))
        previous = self.files.get(str(path))
        if previous is not None and observed != previous:
            raise RuntimeError('Evidence changed during audit: ' + str(path))
        self.files[str(path)] = observed
        return observed

    def mapping(self, entries):
        if not entries:
            raise RuntimeError('Required file identity inventory is empty')
        for path, expected in entries.items():
            self.check(path, expected)

    def product(self, snapshot):
        project = Path(snapshot['private_project'])
        for relative, expected in snapshot['canonical_sha256'].items():
            selected = (project.parent / relative).resolve()
            if not selected.is_relative_to(project.parent.resolve()):
                raise RuntimeError('Product snapshot path escapes its private tree')
            # Relocation changes only the generated project references.
            self.check(selected, snapshot['relocated_project_sha256'] if selected == project else expected)

    def application(self, receipt, form, mode, expected_hash=None):
        receipt = str(Path(receipt).resolve())
        self.check(receipt, expected_hash)
        if receipt in self.applications:
            if self.applications[receipt]['variant'] != [form, mode]:
                raise RuntimeError('Application receipt is shared by different execution variants')
            return
        record = json.loads(Path(receipt).read_text())
        for key in ('output_sha256', 'managed_build_closure_sha256',
                    'runtime_closure_sha256', 'authored_source_sha256'):
            self.mapping(record[key])
        executable = Path(record['executable']).resolve()
        self.check(executable, record['executable_sha256'])
        if record['output_sha256'].get(str(executable)) != record['executable_sha256']:
            raise RuntimeError('Executable is absent from its actual output inventory')
        command = record['command']
        if Path(command[1] if command[0] == 'dotnet' else command[0]).resolve() != executable:
            raise RuntimeError('Execution command differs from the identified application')
        runtime = record['actual_runtime']
        if runtime['kind'] != 'actual-runtime-identity' or runtime['dynamic_code'] != (mode == 'jit'):
            raise RuntimeError('Actual runtime identity differs from the requested execution mode')
        mapped = runtime['mapped_files']
        if not mapped or any(path not in record['runtime_closure_sha256'] for path in mapped):
            raise RuntimeError('Actual mapped runtime files are missing from the identity inventory')
        self.product(record['product_snapshot'])
        copied_inventory = record.get('copied_consumer_source_sha256', {})
        copied_projects = [Path(path) for path in copied_inventory if Path(path).suffix == '.csproj']
        if copied_inventory and len(copied_projects) != 1:
            raise RuntimeError('Copied consumer must identify exactly one original project')
        for copied, expected in copied_inventory.items():
            # The temporary external project is deliberately removed after its
            # run. Its authored C# must still match the retained original sample.
            if Path(copied).suffix != '.cs':
                continue
            relative = Path(copied).relative_to(copied_projects[0].parent)
            original = ROOT / 'samples/ManagedConsumer' / relative
            self.check(original, expected)
            self.copied_sources.append({'copied': copied, 'original': str(original), 'sha256': expected})
        self.applications[receipt] = {'variant': [form, mode], 'executable': str(executable),
                                    'actual_runtime': runtime}

    def report(self, path, expected_rows=None):
        self.check(path)
        report = json.loads(Path(path).read_text())
        rows = report['rows']
        counts = dict(Counter(row['status'] for row in rows))
        if report['status'] != 'passed' or report.get('error') or report.get('errors') or not rows or counts != {'passed': len(rows)}:
            raise RuntimeError('Qualification is incomplete or contains failed/unrun rows: ' + str(path))
        if expected_rows is not None and len(rows) != expected_rows:
            raise RuntimeError('Qualification row count differs from the requested gate: ' + str(path))
        declared = {key: value for key, value in report.get('counts', counts).items() if value}
        if declared != counts:
            raise RuntimeError('Qualification summary differs from the actual case statuses')
        for snapshot in report.get('product_snapshots', []):
            self.product(snapshot)
        if report.get('product_snapshot'):
            self.product(report['product_snapshot'])
        if report.get('application_execution'):
            build = report['application_execution']
            self.application(build['receipt'], report['form'], report['mode'], build['sha256'])
        for build in report.get('application_builds', []):
            self.application(build['receipt'], build['form'], build['mode'], build['sha256'])
        for row in rows:
            if row.get('application_execution_receipt'):
                self.application(row['application_execution_receipt'], row['form'], row['mode'])
            if row.get('fixture_sha256'):
                self.mapping(row['fixture_sha256'])
            if row.get('native_endpoint_sha256'):
                self.mapping(row['native_endpoint_sha256'])
            logs = row.get('logs', []) + ([row['log']] if row.get('log') else [])
            structural_audit = row['suite'] == 'delivery' and row['case'] == 'dependencies-and-imports'
            if not logs and not structural_audit:
                raise RuntimeError('Executed row has no retained transcript: ' + str(row))
            for log in logs:
                self.check(log)
        for command in report['commands']:
            if command['exit_code'] != 0:
                raise RuntimeError('Qualification contains a failed build/execution command')
            self.check(command['log'])
            argv = command['command']
            if any(str(value).endswith('/tests/Delivery/run.py') for value in argv):
                nested = Path(argv[argv.index('--output') + 1])
                nested_report = json.loads(nested.read_text())
                if any(row not in rows for row in nested_report['rows']):
                    raise RuntimeError('Delivery child rows differ from their parent qualification')
                self.report(nested)
        return {'report': str(path), 'sha256': self.files[str(Path(path).resolve())],
                'counts': report.get('counts', counts), 'variants': dict(Counter(row['form'] + '/' + row['mode'] for row in rows))}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--report', type=Path, action='append', required=True)
    parser.add_argument('--expected-rows', type=int, action='append',
                        help='Expected count for each report, in the same order')
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    if args.expected_rows is not None and len(args.expected_rows) != len(args.report):
        parser.error('--expected-rows must be supplied once per --report')
    audit = Audit()
    result = {'evidence_kind': 'qualification-receipt-integrity-audit', 'status': 'failed', 'reports': [], 'errors': []}
    try:
        for index, path in enumerate(args.report):
            expected = args.expected_rows[index] if args.expected_rows is not None else None
            result['reports'].append(audit.report(path.resolve(), expected))
        # Recheck every distinct file to detect overlapping mutations during the
        # audit, including runtime and the private per-application build closure.
        audit.mapping(dict(audit.files))
        result['status'] = 'passed'
    except Exception as error:
        result['errors'].append(str(error))
    result.update(verified_distinct_files=len(audit.files), actual_application_identities=audit.applications,
                  copied_consumer_source_proof=audit.copied_sources, files_sha256=audit.files)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2) + '\n')
    print('REPORT ' + str(args.output))
    return 0 if result['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
