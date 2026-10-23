"""Own declared integration peers from startup through process-group drain."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import signal
import socket
import subprocess
import time


def run_and_drain(command, *, cwd, env, stream, timeout):
    """Reap pytest and its endpoint children even after a timeout/interruption."""
    process = subprocess.Popen(command, cwd=cwd, env=env, stdout=stream,
                               stderr=subprocess.STDOUT, start_new_session=True)
    timed_out = False
    try:
        try:
            code = process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            timed_out = True
            code = 'timeout'
    finally:
        remaining = PeerServices.group_exists(process.pid)
        if remaining:
            os.killpg(process.pid, signal.SIGTERM)
        try:
            process.wait(timeout=10)
        except subprocess.TimeoutExpired:
            os.killpg(process.pid, signal.SIGKILL)
            process.wait(timeout=5)
        deadline = time.monotonic() + 5
        while PeerServices.group_exists(process.pid) and time.monotonic() < deadline:
            time.sleep(0.05)
        drained = not PeerServices.group_exists(process.pid)
        if not drained:
            os.killpg(process.pid, signal.SIGKILL)
    return {'exit_code': code, 'timed_out': timed_out, 'drained': drained,
            'remaining_endpoints': remaining and not timed_out}


class PeerServices:
    def __init__(self, specifications, directory, environment, observations):
        self.specifications = specifications
        self.directory = Path(directory).resolve()
        self.environment = environment
        self.observations = observations
        self.running = []

    def __enter__(self):
        names = [s['name'] for s in self.specifications]
        if len(names) != len(set(names)):
            raise RuntimeError('Integration service names must be unique')
        try:
            for specification in self.specifications:
                self.start(specification)
        except BaseException:
            self.close()
            raise
        return self

    def start(self, specification):
        name = specification['name']
        if not name or Path(name).name != name or name in ('.', '..'):
            raise RuntimeError('Invalid integration service name: ' + name)
        directory = self.directory / name
        directory.mkdir(parents=True)
        log = directory / 'process.log'
        host, port = specification['host'], int(specification['port'])
        row = {'name': name, 'command': specification['command'], 'host': host,
               'port': port, 'log': str(log), 'status': 'starting', 'drained': False}
        self.observations.append(row)
        # A successful probe must belong to the process we start. Never reuse
        # another qualification's listener on the upstream fixed Apache port.
        try:
            with socket.create_connection((host, port), timeout=0.2):
                raise RuntimeError(f'Integration service port is already occupied: {host}:{port}')
        except (ConnectionRefusedError, TimeoutError):
            pass
        environment = dict(self.environment, **specification.get('environment', {}))
        if name == 'apache-renegotiation':
            runtime = directory / 'run'; runtime.mkdir()
            logs = directory / 'logs'; logs.mkdir()
            environment.update(APACHE_RUN_DIR=str(runtime), APACHE_PID_FILE=str(runtime / 'apache2.pid'),
                               APACHE_LOG_DIR=str(logs))
            row['private_environment'] = {key: environment[key] for key in
                                          ('APACHE_RUN_DIR', 'APACHE_PID_FILE', 'APACHE_LOG_DIR')}
        started = time.monotonic()
        with log.open('wb') as stream:
            process = subprocess.Popen(specification['command'], cwd=directory, env=environment,
                                       stdout=stream, stderr=subprocess.STDOUT, start_new_session=True)
        self.running.append((process, row, directory))
        row['pid'] = process.pid
        deadline = started + float(specification.get('ready_timeout_seconds', 20))
        while time.monotonic() < deadline:
            if process.poll() is not None:
                raise RuntimeError(f'Integration service exited before readiness ({process.returncode}): {log}')
            try:
                with socket.create_connection((host, port), timeout=0.2):
                    if process.poll() is not None:
                        raise RuntimeError('Integration service exited during readiness: ' + str(log))
                    row.update(status='ready', ready_seconds=round(time.monotonic() - started, 6))
                    return
            except (ConnectionRefusedError, TimeoutError):
                time.sleep(0.05)
        raise RuntimeError('Integration service readiness timed out: ' + str(log))

    @staticmethod
    def group_exists(pid):
        try:
            os.killpg(pid, 0)
            return True
        except ProcessLookupError:
            return False

    def close(self):
        errors = []
        for process, row, directory in reversed(self.running):
            was_ready = row['status'] == 'ready'
            unexpected_exit = was_ready and process.poll() is not None
            row['forced_kill'] = False
            if self.group_exists(process.pid):
                os.killpg(process.pid, signal.SIGTERM)
            try:
                process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                row['forced_kill'] = True
                os.killpg(process.pid, signal.SIGKILL)
                process.wait(timeout=5)
            deadline = time.monotonic() + 5
            while self.group_exists(process.pid) and time.monotonic() < deadline:
                time.sleep(0.05)
            if self.group_exists(process.pid):
                row['forced_kill'] = True
                os.killpg(process.pid, signal.SIGKILL)
                errors.append('Service process group did not drain: ' + row['name'])
            else:
                row['drained'] = True
            row.update(exit_code=process.returncode, status='stopped' if was_ready else 'failed')
            row['log_sha256'] = {str(p): hashlib.sha256(p.read_bytes()).hexdigest()
                                 for p in directory.rglob('*.log')}
            if unexpected_exit:
                row['status'] = 'failed'
                errors.append('Service exited before qualification finished: ' + row['name'])
        self.running.clear()
        if errors:
            raise RuntimeError('; '.join(errors))

    def __exit__(self, kind, value, traceback):
        self.close()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--peer-commands', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    output = args.output.resolve(); output.parent.mkdir(parents=True, exist_ok=True)
    peers = json.loads(args.peer_commands.read_text())
    report = {'evidence_kind': 'integration-service-start-readiness-drain', 'status': 'failed', 'services': []}
    try:
        if not peers.get('services'):
            raise RuntimeError('No declared integration services to probe')
        with PeerServices(peers['services'], output.parent / (output.stem + '-services'),
                          dict(os.environ, **peers.get('environment', {})), report['services']):
            pass
        if not all(r['drained'] and r['status'] == 'stopped' for r in report['services']):
            raise RuntimeError('Not every declared service reached readiness and drained')
        report['status'] = 'passed'
    except Exception as error:
        report['error'] = str(error)
    output.write_text(json.dumps(report, indent=2) + '\n')
    print('REPORT ' + str(output))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
