"""Compare real stdio opens with an independent native flock/fcntl observer.

Only consumes immutable A-owned tools. Every shared operation locks itself;
never wrap this harness in the shared lock.
"""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys

from validate_fcntl_record_locks_r9 import Child, LOCK, NATIVE_LOCK, REPO

sys.path.insert(0, str(REPO / 'Scripts'))
from campaigns.identity import digest, write_json
from campaigns.process import execute

HERE = Path(__file__).resolve().parent


def observe(command, peer_binary, work, artifacts, label, contract=False):
    work.mkdir()
    path = work / 'ordinary file.bin'
    path.write_bytes(b'original')
    template = work / 'privateXXXXXX'
    # Set an inherited native process umask; no product umask/subprocess service
    # is supplied by this independent test orchestrator.
    app = Child(['bash', '-c', 'umask 027; exec "$@"', 'r10-stdio-control',
                 *command, path, template], work, artifacts / (label + '-app.log'))
    rows, peer = [], None
    result = {'status': 'failed', 'command': app.command, 'observations': rows}

    def request(text):
        peer.send(text)
        line = peer.line().split()
        assert line[0] == 'result' and len(line) == 8, line
        return list(map(int, line[1:]))

    try:
        while True:
            line = app.line(timeout=30)
            if line == 'probe complete' or line.startswith('stdio-opens passed checks='):
                if contract:
                    result['c_assertions'] = int(line.rsplit('=', 1)[1])
                break
            tag, phase, name = line.split(' ', 2)
            assert tag == 'phase', line
            if peer is None or phase in ('mkstemp', 'private-temp', 'created'):
                if peer:
                    peer.finish()
                peer = Child([peer_binary, name], work, artifacts / (label + '-' + phase + '-peer.log'))
                assert peer.line().split() == ['ready', str(peer.process.pid)]
            row = {'phase': phase, 'path': name}
            if phase == 'peer-owned':
                assert request('flock 6 0 0 0')[0] == 0
            elif phase == 'opened-under-lock':
                # A second native descriptor still sees the peer's explicit lock.
                with path.open('rb') as unused:
                    import fcntl
                    try:
                        fcntl.flock(unused, fcntl.LOCK_EX | fcntl.LOCK_NB)
                    except BlockingIOError:
                        row['explicit_lock_retained'] = True
                    else:
                        raise AssertionError('Caller-owned peer flock was removed')
            elif phase == 'peer-release':
                assert request('flock 8 0 0 0')[0] == 0
            elif phase in ('explicit', 'explicit-after-other-close'):
                lock = request('flock 6 0 0 0')
                assert lock[0:2] == [-1, 11], lock
                row['exclusive_conflict'] = lock
            elif phase in ('record', 'record-other-close'):
                lock = request('get 1 0 2 3')
                assert lock[0] == 0 and lock[2] == (1 if phase == 'record' else 2), lock
                if phase == 'record':
                    assert lock[4:6] == [2, 3] and lock[6] > 0, lock
                row['record_observation'] = lock
            else:
                lock = request('flock 6 0 0 0')
                row['native_exclusive'] = lock[0:2]
                if lock[0] == 0:
                    assert request('flock 8 0 0 0')[0] == 0
                if contract:
                    assert lock[0] == 0, row
                if phase in ('mkstemp', 'private-temp'):
                    row['mode'] = os.stat(name).st_mode & 0o777
                    assert row['mode'] == 0o600, row
                elif phase == 'created':
                    row['mode'] = os.stat(name).st_mode & 0o777
                    assert row['mode'] == 0o640, row
            rows.append(row)
            app.send('')
        app.finish()
        if peer:
            peer.finish()
        result['status'] = 'passed'
    finally:
        app.stop()
        if peer:
            peer.stop()
        write_json(artifacts / (label + '-observations.json'), result)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work', type=Path, required=True)
    parser.add_argument('--artifacts', type=Path, required=True)
    parser.add_argument('--tools', type=Path, required=True)
    parser.add_argument('--before', action='store_true')
    args = parser.parse_args()
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

    write_json(artifacts / 'inputs.json', {
        'scope': 'stdio and temporary-file services only; no CPython profile qualification',
        'tools': {p.name: digest(p) for p in args.tools.glob('*') if p.is_file()},
        'runtime': {str(p.relative_to(REPO)): digest(p) for p in (REPO / 'DotCC.Libc').glob('*.cs')},
        'fixtures': {str(p.relative_to(REPO)): digest(p) for p in
                     [HERE / 'stdio_lock_opens_r10.c', Path(__file__), HERE / 'fcntl_record_lock_peer_r9.c']}})
    for source, name in [(HERE / 'stdio_lock_opens_r10.c', 'native'),
                         (HERE / 'fcntl_record_lock_peer_r9.c', 'peer')]:
        run(name + '-build', ['flock', '--close', NATIVE_LOCK, 'cc', '-std=c11',
                              '-Wall', '-Wextra', '-Werror', source, '-o', work / name])
    native = observe([work / 'native', 'probe'], work / 'peer', work / 'native-run', artifacts, 'native')
    assert [x['native_exclusive'] for x in native['observations']] == [[0, 0]] * 4
    direct_project = HERE / 'StdioOpensDirect/StdioOpensDirect.csproj'
    run('direct-build', ['flock', '--close', LOCK, 'dotnet', 'build', direct_project, '-c', 'Release',
                        '--artifacts-path', work / 'direct', '--disable-build-servers', '-m:1'])
    direct = observe(['flock', '--close', LOCK, 'dotnet', work / 'direct/bin/StdioOpensDirect/release/StdioOpensDirect.dll'],
                     work / 'peer', work / 'direct-run', artifacts, 'direct')
    run('emit', ['flock', '--close', LOCK, 'dotnet', args.tools.resolve() / 'dotcc.dll',
                 '--emit=csproj', '--runtime=c', '-std=c11', HERE / 'stdio_lock_opens_r10.c', '-o', work / 'emitted'])
    project = work / 'emitted/emitted.csproj'
    run('jit-build', ['flock', '--close', LOCK, 'dotnet', 'build', project, '-c', 'Release',
                     '--artifacts-path', work / 'jit', '--disable-build-servers', '-m:1'])
    jit_command = ['flock', '--close', LOCK, 'dotnet', work / 'jit/bin/emitted/release/emitted.dll']
    jit = observe([*jit_command, 'probe'], work / 'peer', work / 'jit-run', artifacts, 'jit')
    expected = [[-1, 11]] * 3 + [[0, 0]] if args.before else [[0, 0]] * 4
    assert [x['native_exclusive'] for x in direct['observations']] == expected, direct
    assert [x['native_exclusive'] for x in jit['observations']] == expected, jit
    summary = {'status': 'before-mismatch-reproduced' if args.before else 'passed',
               'native': native, 'direct': direct, 'jit': jit}
    if not args.before:
        summary['native_contract'] = observe([work / 'native', 'contract'], work / 'peer',
                                            work / 'native-contract-run', artifacts, 'native-contract', True)
        summary['jit_contract'] = observe([*jit_command, 'contract'], work / 'peer',
                                         work / 'jit-contract-run', artifacts, 'jit-contract', True)
        run('aot-build', ['flock', '--close', LOCK, 'dotnet', 'publish', project, '-c', 'Release',
                         '-r', 'linux-x64', '-p:PublishAot=true', '-p:StripSymbols=true', '--artifacts-path',
                         work / 'aot', '--disable-build-servers', '-m:1', '-o', work / 'aot-app'])
        summary['aot'] = observe(['flock', '--close', LOCK, work / 'aot-app/emitted', 'probe'],
                                 work / 'peer', work / 'aot-run', artifacts, 'aot')
        summary['aot_contract'] = observe(['flock', '--close', LOCK, work / 'aot-app/emitted', 'contract'],
                                          work / 'peer', work / 'aot-contract-run', artifacts, 'aot-contract', True)
        assert summary['native_contract']['c_assertions'] == summary['jit_contract']['c_assertions'] == summary['aot_contract']['c_assertions']
    write_json(artifacts / 'comparison.json', summary)
    print(json.dumps({'status': summary['status'], 'expected_exclusive': expected}))


if __name__ == '__main__':
    main()
