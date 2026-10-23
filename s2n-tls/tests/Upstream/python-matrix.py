"""Durable, serial qualification of the exact original eighteen Python families.

Resume never retries a failure implicitly. Each attempt and its original leaf
report survives; passed leaves can be adopted after complete read-only checks.
"""
import argparse
from contextlib import nullcontext
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import select
import subprocess
import sys

from python_receipt import Validator, evidence_path

ROOT = Path(__file__).resolve().parents[2]


def read(path):
    return json.loads(Path(path).read_text())


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


class Lease:
    def __init__(self, path, resume=False):
        self.path, self.resume = Path(path), resume

    def __enter__(self):
        self.path.parent.mkdir(parents=True, exist_ok=True)
        for attempt in range(2):
            try:
                descriptor = os.open(self.path, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o600)
            except FileExistsError:
                previous = read(self.path)
                try:
                    os.kill(previous['pid'], 0)
                except ProcessLookupError:
                    if self.resume and attempt == 0:
                        self.path.unlink()
                        continue
                raise RuntimeError('A Python fixture/controller lease already exists: ' + str(self.path))
            with os.fdopen(descriptor, 'w') as stream:
                json.dump({'pid': os.getpid()}, stream)
            return self
        raise RuntimeError('Could not acquire the Python fixture/controller lease')

    def __exit__(self, *_):
        self.path.unlink()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--executables', type=Path, required=True)
    parser.add_argument('--peer-commands', type=Path, required=True)
    parser.add_argument('--psk-peer-commands', type=Path, required=True)
    parser.add_argument('--python', required=True)
    parser.add_argument('--form', choices=('raw', 'processed', 'all'), default='all')
    parser.add_argument('--mode', choices=('jit', 'aot', 'all'), default='all')
    parser.add_argument('--endpoint-kind', choices=('native', 'translated'), default='translated')
    parser.add_argument('--provider-version')
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--resume', action='store_true')
    parser.add_argument('--retry-failed', action='store_true', help='Explicit new attempts; retains every failed leaf')
    parser.add_argument('--adopt-report', type=Path, action='append', default=[])
    parser.add_argument('--await-pid', type=int, action='append', default=[],
                        help='Wait for an existing controller to exit before adoption, using a process-exit event rather than polling')
    parser.add_argument('--case', action='append', help='Select whole original families; never individual parameter filtering')
    parser.add_argument('--workers', type=int, default=1)
    parser.add_argument('--port-low', type=int)
    parser.add_argument('--port-high', type=int)
    args = parser.parse_args()
    if args.workers < 1 or args.retry_failed and not args.resume:
        parser.error('Positive workers and --resume for --retry-failed are required')
    if (args.port_low is None) != (args.port_high is None):
        parser.error('--port-low and --port-high must be supplied together')
    if args.endpoint_kind == 'native':
        if args.form not in ('all', 'raw') or args.mode not in ('all', 'jit'):
            parser.error('Native originals use the single raw/JIT manifest identity')
        variants = [('raw', 'jit')]
    else:
        variants = [(form, mode) for form in (('raw', 'processed') if args.form == 'all' else (args.form,))
                    for mode in (('jit', 'aot') if args.mode == 'all' else (args.mode,))]
    inventory = read(ROOT / 'config/upstream-tests.json')
    families = [r['path'] for r in inventory['cases'] if r['status'].startswith('required') and r['path'].endswith('.py')]
    if len(families) != 18:
        raise RuntimeError('The frozen original eighteen-family inventory changed')
    if args.case:
        if set(args.case) - set(families):
            parser.error('--case must select exact required original Python family paths')
        families = [case for case in families if case in args.case]
    output = args.output.resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    leaves = output.parent / (output.stem + '-leaves')
    leaves.mkdir(exist_ok=True)
    identity = {'endpoint_kind': args.endpoint_kind, 'executables': str(args.executables.resolve()),
        'executables_sha256': sha(args.executables), 'peer_commands_sha256': sha(args.peer_commands),
        'psk_peer_commands_sha256': sha(args.psk_peer_commands), 'variants': [list(v) for v in variants], 'families': families}
    with Lease(output.with_suffix('.lock'), args.resume):
        if output.exists():
            if not args.resume:
                raise RuntimeError('Existing matrix evidence requires --resume')
            state = read(output)
            if state['selection'] != identity:
                raise RuntimeError('Resume cannot change the immutable manifest, peers or family/variant selection')
        else:
            state = {'evidence_kind': 'original-python-matrix', 'status': 'running', 'selection': identity,
                'endpoint_kind': args.endpoint_kind, 'pid': os.getpid(), 'cases': [
                    {'case': case, 'form': form, 'mode': mode, 'status': 'unrun', 'attempts': []}
                    for form, mode in variants for case in families], 'leaf_reports': [], 'supplemental_controls': []}
        state['pid'] = os.getpid()
        controller_bytes = Path(__file__).read_bytes()
        controller_hash = hashlib.sha256(controller_bytes).hexdigest()
        controller_copy = leaves / ('controller-' + controller_hash + '.py')
        if not controller_copy.exists():
            controller_copy.write_bytes(controller_bytes)
        controller = {'path': str(controller_copy), 'sha256': controller_hash}
        if controller not in state.setdefault('controller_versions', []):
            state['controller_versions'].append(controller)
        audit = Validator()

        def save():
            state['updated_utc'] = datetime.now(timezone.utc).isoformat()
            state['counts'] = {status: sum(r['status'] == status for r in state['cases'])
                               for status in ('passed', 'failed', 'unrun', 'running')}
            temporary = output.with_suffix('.tmp')
            temporary.write_text(json.dumps(state, indent=2) + '\n')
            temporary.replace(output)

        def accept(row, path, leaf, actual):
            if leaf['endpoint_kind'] != args.endpoint_kind or (leaf['form'], leaf['mode']) != (row['form'], row['mode']):
                raise RuntimeError('Adopted Python family has a different execution variant')
            if leaf['endpoint_manifest_sha256'] != {str(args.executables): identity['executables_sha256']}:
                # Original reports may have recorded an absolute spelling.
                if {str(evidence_path(p)): value for p, value in leaf['endpoint_manifest_sha256'].items()} != {
                        str(args.executables.resolve()): identity['executables_sha256']}:
                    raise RuntimeError('Adopted Python family uses a different endpoint manifest')
            peer = args.psk_peer_commands if Path(row['case']).name == 'test_external_psk.py' else args.peer_commands
            if {str(evidence_path(p)): value for p, value in leaf['peer_manifest_sha256'].items()} != {str(peer.resolve()): sha(peer)}:
                raise RuntimeError('Adopted Python family uses a different declared peer manifest')
            result = audit.family(leaf, actual)
            if Path(row['case']).name == 'test_hello_retry_requests.py':
                from hrr_applicability import validate_proof
                proof = leaf.get('hrr_applicability')
                if not proof or len(result.get('original_inapplicable', [])) != 9 or result['passed'] != 118:
                    raise RuntimeError('HRR family needs its exact 118 applicable cases and nine explicit inapplicable combinations')
                validate_proof(proof['proof'], args.endpoint_kind, row['form'], row['mode'])
            row.update(status='passed', report=str(path), report_sha256=sha(path), passed=actual['passed'],
                       skipped=actual['skipped'], inapplicable=actual.get('inapplicable', 0))
            item = {'path': str(path), 'sha256': sha(path), 'form': row['form'], 'mode': row['mode'], 'case': row['case']}
            if item not in state['leaf_reports']:
                state['leaf_reports'].append(item)

        for pid in args.await_pid:
            try:
                descriptor = os.pidfd_open(pid)
            except ProcessLookupError:
                continue
            state['awaiting_existing_controller'] = pid
            save()
            try:
                select.select([descriptor], [], [])
            finally:
                os.close(descriptor)
            state.pop('awaiting_existing_controller', None)
            save()
        for path in args.adopt_report:
            leaf = read(path)
            if leaf.get('evidence_kind') == 'original-python-matrix':
                if leaf['endpoint_kind'] != args.endpoint_kind:
                    raise RuntimeError('Adopted matrix endpoint kind differs')
                for retained in leaf['cases']:
                    if retained['status'] != 'passed':
                        continue
                    row = next((r for r in state['cases'] if (r['case'], r['form'], r['mode']) ==
                                (retained['case'], retained['form'], retained['mode'])), None)
                    if row and row['status'] != 'passed':
                        retained_path = Path(retained['report'])
                        if sha(retained_path) != retained['report_sha256']:
                            raise RuntimeError('Adopted matrix leaf identity changed')
                        retained_leaf = read(retained_path)
                        actual = next(r for r in retained_leaf['cases'] if r['case'] == row['case'])
                        accept(row, retained_path, retained_leaf, actual)
                save()
                continue
            if leaf.get('evidence_kind') != 'original-python-integration':
                raise RuntimeError('Only complete original family executions may be adopted')
            for actual in leaf['cases']:
                if actual['status'] != 'passed':
                    continue
                row = next((r for r in state['cases'] if (r['case'], r['form'], r['mode']) ==
                            (actual['case'], leaf['form'], leaf['mode'])), None)
                if row and row['status'] != 'passed':
                    accept(row, path.resolve(), leaf, actual)
            save()
        for row in state['cases']:
            if row['status'] == 'passed':
                leaf = read(row['report'])
                accept(row, Path(row['report']), leaf, next(r for r in leaf['cases'] if r['case'] == row['case']))
                continue
            if row['status'] == 'running' and row['attempts']:
                previous = row['attempts'][-1]
                pid = previous.get('pid')
                if pid:
                    try:
                        os.kill(pid, 0)
                    except ProcessLookupError:
                        pass
                    else:
                        raise RuntimeError('The retained original family is still running; refusing duplicate execution: ' + str(pid))
                retained = Path(previous['report'])
                if retained.is_file():
                    leaf = read(retained)
                    if leaf['status'] == 'passed':
                        accept(row, retained, leaf, leaf['cases'][0])
                        previous['status'] = 'passed'
                        save()
                        continue
                row['status'] = 'failed'
                previous['status'] = 'failed'
                save()
            if row['status'] in ('failed', 'running') and not args.retry_failed:
                continue
            attempt = len(row['attempts']) + 1
            prefix = leaves / (row['form'] + '-' + row['mode'] + '-' + Path(row['case']).stem + '-' + str(attempt))
            path = prefix.with_suffix('.json')
            if path.exists():
                raise RuntimeError('Refusing to overwrite an original Python attempt: ' + str(path))
            command = [sys.executable, '-B', str(ROOT / 'tests/Upstream/python-integration.py'), '--executables',
                str(args.executables.resolve()), '--peer-commands', str((args.psk_peer_commands if
                Path(row['case']).name == 'test_external_psk.py' else args.peer_commands).resolve()),
                '--python', os.path.abspath(args.python), '--form', row['form'], '--mode', row['mode'],
                '--endpoint-kind', args.endpoint_kind, '--workers', str(args.workers), '--no-reruns',
                '--available-port-probe', '--output', str(path)]
            if args.provider_version:
                command += ['--provider-version', args.provider_version]
            if args.port_low is not None:
                command += ['--port-low', str(args.port_low), '--port-high', str(args.port_high)]
            elif args.endpoint_kind == 'native':
                command += ['--port-low', '8000', '--port-high', '18000']
            record = {'command': command, 'report': str(path), 'status': 'running'}
            row['attempts'].append(record)
            row['status'] = 'running'
            save()
            try:
                if Path(row['case']).name == 'test_hello_retry_requests.py':
                    proof = prefix.with_name(prefix.name + '-no-retry-controls').with_suffix('.json')
                    # --output is not necessarily last after explicit bounds.
                    proof_command = list(command)
                    proof_command[proof_command.index('--output') + 1] = str(proof)
                    proof_command += ['--hrr-no-retry']
                    record['supplemental_command'] = proof_command
                    save()
                    result = subprocess.run(proof_command, cwd=ROOT.parent)
                    if result.returncode:
                        raise RuntimeError('Separate no-HRR controls failed; original applicability remains open')
                    from hrr_applicability import validate_proof
                    validate_proof(proof, args.endpoint_kind, row['form'], row['mode'])
                    state['supplemental_controls'].append({'report': str(proof), 'sha256': sha(proof),
                        'form': row['form'], 'mode': row['mode'], 'passed': 9})
                    command += ['--hrr-proof', str(proof)]
                command += ['--case', row['case']]
                record['command'] = command
                with (Lease(ROOT / 'build/managed-python-upstream/apache7777.lock', args.resume)
                      if Path(row['case']).name == 'test_renegotiate_apache.py' else nullcontext()):
                    process = subprocess.Popen(command, cwd=ROOT.parent)
                    record['pid'] = process.pid
                    save()
                    record['exit_code'] = process.wait()
                if record['exit_code']:
                    raise RuntimeError('Original family returned a strict failure; inspect the retained attempt')
                leaf = read(path)
                accept(row, path, leaf, leaf['cases'][0])
                record['status'] = 'passed'
            except Exception as error:
                row.update(status='failed', error=str(error))
                record.update(status='failed', error=str(error))
            save()
        state['status'] = 'passed' if all(row['status'] == 'passed' for row in state['cases']) else 'failed'
        state['scope'] = 'Complete eighteen-family matrix' if len(families) == 18 else 'Selected complete families; remaining original families are unrun'
        state['files_checked'] = len(audit.checked)
        save()
    print('REPORT ' + str(output))
    return 0 if state['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
