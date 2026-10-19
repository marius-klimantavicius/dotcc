"""Same-input native/emitted record locks, with an independent native owner.

Consumes existing A-owned compiler tools. Never builds the compiler, translates
CPython, edits generated code, or publishes a product. Use a fresh work leaf.
"""
import argparse
import json
import os
from pathlib import Path
import platform
import queue
import subprocess
import sys
import threading
import time

REPO = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(REPO / 'Scripts'))
from campaigns.identity import digest, write_json
from campaigns.process import execute

LOCK = '/tmp/dotcc-cpython-shared-build.lock'
NATIVE_LOCK = '/tmp/dotcc-cpython-native-build.lock'
FIXTURES = Path(__file__).resolve().parent


class Child:
    def __init__(self, command, cwd, log):
        self.command = list(map(str, command))
        self.process = subprocess.Popen(self.command, cwd=cwd, stdin=subprocess.PIPE,
                                        stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                                        text=True, bufsize=1, start_new_session=True)
        self.lines = queue.Queue()
        self.log = log
        self.transcript = []
        self.started = time.monotonic()
        self.reader = threading.Thread(target=self.read, daemon=True)
        self.reader.start()

    def read(self):
        for line in self.process.stdout:
            self.transcript.append(line)
            self.log.write_text(''.join(self.transcript))
            self.lines.put(line.rstrip('\n'))
        self.lines.put(None)

    def line(self, timeout=8):
        value = self.lines.get(timeout=timeout)
        if value is None:
            raise AssertionError(f'Child ended before expected assertion: {self.command}; {self.transcript}')
        return value

    def send(self, text):
        self.process.stdin.write(text + '\n')
        self.process.stdin.flush()

    def finish(self):
        self.process.stdin.close()
        result = self.process.wait(timeout=8)
        self.reader.join(timeout=2)
        if result:
            raise AssertionError(f'Child exit {result}: {self.command}; {self.transcript}')

    def stop(self):
        if self.process.poll() is None:
            os.killpg(self.process.pid, 9)
            self.process.wait(timeout=5)
        self.reader.join(timeout=2)


def interaction(candidate, peer_command, work, artifacts, label, shared):
    work.mkdir()
    path = work / 'locked file.bin'
    path.write_bytes(bytes(range(256)))
    peer = Child([*peer_command, path], work, artifacts / (label + '-peer.log'))
    app = None
    evidence = {'status': 'failed', 'candidate': list(map(str, candidate)),
                'peer': list(map(str, peer_command)), 'peer_checks': 0, 'phases': []}

    def request(command, kind, start, length):
        peer.send(f'{command} {kind} 0 {start} {length}')
        row = peer.line().split()
        assert row[0] == 'result' and len(row) == 8, row
        return list(map(int, row[1:]))

    def check(kind, start, length, expected_kind, expected_start=None, expected_length=None):
        row = request('get', kind, start, length)
        assert row[0] == 0 and row[2] == expected_kind, row
        if expected_kind != 2:
            assert row[3:] == [0, expected_start, expected_length, app.process.pid], row
        evidence['peer_checks'] += 1

    def set_peer(kind, start, length, conflict=False):
        row = request('set', kind, start, length)
        assert (row[0] == -1 and row[1] in (11, 13)) if conflict else row[0] == 0, row
        evidence['peer_checks'] += 1

    try:
        ready = peer.line().split()
        assert ready == ['ready', str(peer.process.pid)], ready
        # The flock supervisor's real executable child owns the record locks.
        command = [*candidate, path, str(peer.process.pid)]
        if shared:
            command = ['flock', '--close', LOCK, *command]
        app = Child(command, work, artifacts / (label + '-candidate.log'))
        evidence['command'] = app.command
        layout = app.line()
        assert layout == 'layout flock=32 align=8 offsets=0,2,8,16,24 widths=2,2,8,8,4 signed=1,1', layout
        # The flock supervisor has one real executable child; use its PID.
        if shared:
            children = Path(f'/proc/{app.process.pid}/task/{app.process.pid}/children').read_text().split()
            assert len(children) == 1, children
            owner_pid = int(children[0])
        else:
            owner_pid = app.process.pid
        evidence['owner_pid'] = owner_pid

        def conflict(kind, start, length, expected_kind, expected_start, expected_length):
            row = request('get', kind, start, length)
            assert row == [0, 0, expected_kind, 0, expected_start, expected_length, owner_pid], row
            evidence['peer_checks'] += 1

        phases = ['owned', 'split', 'shared', 'peer-lock', 'blocking', 'acquired',
                  'current', 'end', 'negative-length', 'large-to-eof', 'close-other',
                  'closed', 'reused', 'record-done', 'flock-owned', 'flock-close-other',
                  'flock-shared', 'flock-peer', 'flock-blocking', 'flock-acquired', 'flock-closed']
        for phase in phases:
            line = app.line()
            assert line == 'phase ' + phase, line
            evidence['phases'].append(phase)
            if phase == 'owned':
                conflict(1, 32, 1, 1, 32, 16)
                set_peer(1, 32, 1, True)
                set_peer(1, 48, 1)
                set_peer(2, 0, 0)
            elif phase == 'split':
                check(1, 36, 4, 2)
                conflict(1, 32, 1, 1, 32, 4)
                conflict(1, 40, 1, 1, 40, 8)
            elif phase == 'shared':
                check(0, 32, 16, 2)
                set_peer(0, 32, 16)
                set_peer(1, 32, 1, True)
                set_peer(2, 0, 0)
            elif phase == 'peer-lock':
                set_peer(1, 100, 20)
            elif phase in ('blocking', 'flock-blocking'):
                app.send('')
                started = time.monotonic()
                try:
                    premature = app.lines.get(timeout=0.3)
                    raise AssertionError('Blocking lock returned before independent owner released: ' + str(premature))
                except queue.Empty:
                    pass
                if phase == 'blocking':
                    set_peer(2, 0, 0)
                else:
                    assert request('flock', 8, 0, 0)[0] == 0
                    evidence['peer_checks'] += 1
                evidence[phase + '_minimum_seconds'] = time.monotonic() - started
                continue
            elif phase == 'acquired':
                conflict(1, 100, 1, 1, 100, 20)
            elif phase == 'current':
                conflict(1, 50, 1, 1, 50, 5)
                check(1, 40, 10, 2)
            elif phase == 'end':
                conflict(1, 236, 1, 1, 236, 8)
                check(1, 244, 1, 2)
            elif phase == 'negative-length':
                conflict(1, 70, 1, 1, 70, 10)
                check(1, 80, 1, 2)
            elif phase == 'large-to-eof':
                conflict(1, 5368709223, 1, 1, 5368709123, 0)
                check(1, 5368709122, 1, 2)
            elif phase in ('close-other', 'closed', 'record-done'):
                set_peer(1, 0, 0)
                set_peer(2, 0, 0)
            elif phase == 'reused':
                conflict(1, 20, 1, 1, 20, 4)
            elif phase in ('flock-owned', 'flock-close-other', 'flock-acquired'):
                row = request('flock', 2 | 4, 0, 0)
                assert row[0] == -1 and row[1] == 11, row
                evidence['peer_checks'] += 1
                if phase == 'flock-owned':
                    # On this local Linux filesystem record locks and flock
                    # remain independent even for an overlapping file region.
                    set_peer(1, 0, 0)
                    set_peer(2, 0, 0)
            elif phase == 'flock-shared':
                assert request('flock', 1 | 4, 0, 0)[0] == 0
                row = request('flock', 2 | 4, 0, 0)
                assert row[0] == -1 and row[1] == 11, row
                assert request('flock', 8, 0, 0)[0] == 0
                evidence['peer_checks'] += 3
            elif phase == 'flock-peer':
                assert request('flock', 2 | 4, 0, 0)[0] == 0
                evidence['peer_checks'] += 1
            elif phase == 'flock-closed':
                assert request('flock', 2 | 4, 0, 0)[0] == 0
                assert request('flock', 8, 0, 0)[0] == 0
                evidence['peer_checks'] += 2
            app.send('')
        completed = app.line()
        assert completed.startswith('record-locks passed checks='), completed
        evidence['candidate_checks'] = int(completed.rsplit('=', 1)[1])
        app.finish()
        peer.finish()
        evidence['status'] = 'passed'
    finally:
        if app:
            app.stop()
        peer.stop()
        evidence['seconds'] = time.monotonic() - peer.started
        write_json(artifacts / (label + '-interaction.json'), evidence)
    return evidence


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work', required=True, type=Path)
    parser.add_argument('--artifacts', required=True, type=Path)
    parser.add_argument('--tools', required=True, type=Path)
    parser.add_argument('--before', action='store_true')
    args = parser.parse_args()
    if sys.platform != 'linux' or platform.machine() != 'x86_64':
        raise RuntimeError('Linux x64 LP64 required for this control')
    work, artifacts = args.work.resolve(), args.artifacts.resolve()
    work.mkdir(parents=True, exist_ok=False)
    artifacts.mkdir(parents=True, exist_ok=False)
    commands = []

    def run(label, argv, timeout=300):
        record = {}
        commands.append(record)
        try:
            return execute(argv, label=label, cwd=REPO, output=artifacts / (label + '.log'),
                           timeout=timeout, record=record)
        finally:
            write_json(artifacts / 'commands.json', commands)

    fixture = FIXTURES / 'fcntl_record_locks_r9.c'
    peer = FIXTURES / 'fcntl_record_lock_peer_r9.c'
    write_json(artifacts / 'inputs.json', {
        'scope': 'shared fcntl ABI/service only; no translated interpreter qualification',
        'source_pin': json.loads((REPO / 'cpython/config/source.json').read_text()),
        'fixtures': {str(p): digest(p) for p in (fixture, peer)},
        'tools': {p.name: digest(p) for p in args.tools.glob('*') if p.is_file()},
        'runtime': {str(p.relative_to(REPO)): digest(p) for p in (REPO / 'DotCC.Libc').glob('*.cs')},
        'header': digest(REPO / 'DotCC.Lib/include/fcntl.h')})
    for source, name in ((fixture, 'native'), (peer, 'peer')):
        run(name + '-build', ['flock', '--close', NATIVE_LOCK, 'cc', '-std=c11',
            '-Wall', '-Wextra', '-Werror', source, '-o', work / name])
    native = interaction([work / 'native'], [work / 'peer'], work / 'native-run', artifacts, 'native', False)
    try:
        run('emit', ['flock', '--close', LOCK, 'dotnet', args.tools.resolve() / 'dotcc.dll',
            '--emit=csproj', '--runtime=c', '-std=c11', fixture, '-o', work / 'emitted'])
    except Exception:
        if not args.before:
            raise
        text = (artifacts / 'emit.log').read_text()
        if 'incomplete aggregate' not in text or 'flock' not in text:
            raise
        write_json(artifacts / 'comparison.json', {'native': native, 'emitted': 'expected incomplete flock failure',
                   'status': 'before-failure-reproduced', 'command_exit': commands[-1].get('exit_code')})
        return
    if args.before:
        raise AssertionError('Before control unexpectedly emitted')
    project = work / 'emitted/emitted.csproj'
    run('jit-build', ['flock', '--close', LOCK, 'dotnet', 'build', project, '-c', 'Release',
        '--artifacts-path', work / 'jit', '--disable-build-servers', '-m:1'])
    jit = interaction(['dotnet', work / 'jit/bin/emitted/release/emitted.dll'], [work / 'peer'],
                      work / 'jit-run', artifacts, 'jit', True)
    run('aot-build', ['flock', '--close', LOCK, 'dotnet', 'publish', project, '-c', 'Release',
        '-r', 'linux-x64', '-p:PublishAot=true', '-p:StripSymbols=true', '--artifacts-path',
        work / 'aot', '--disable-build-servers', '-m:1', '-o', work / 'aot-app'])
    aot = interaction([work / 'aot-app/emitted'], [work / 'peer'], work / 'aot-run', artifacts, 'aot', True)
    assert native['candidate_checks'] == jit['candidate_checks'] == aot['candidate_checks']
    assert native['peer_checks'] == jit['peer_checks'] == aot['peer_checks']
    write_json(artifacts / 'comparison.json', {'status': 'passed', 'native': native, 'jit': jit, 'aot': aot})
    print(json.dumps({'status': 'passed', 'candidate_checks_per_cell': jit['candidate_checks'],
                      'independent_peer_checks_per_cell': jit['peer_checks']}))


if __name__ == '__main__':
    main()
