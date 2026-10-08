#!/usr/bin/env python3
"""Registered musl IPv4 parsers and POSIX socket imports: native/JIT/NativeAOT."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import signal
import subprocess
import sys
import time

sys.dont_write_bytecode = True
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', required=True, type=Path)
parser.add_argument('--output-dir', required=True, type=Path)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
tests = Path(__file__).parent
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=False)
report = {'schema': 1, 'status': 'running', 'commands': []}

def record(path):
    return {'path': str(path), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}

def save():
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

def memory(pid):
    todo, seen, rss = [pid], set(), 0
    while todo:
        current = todo.pop()
        if current in seen:
            continue
        seen.add(current)
        try:
            for task in Path(f'/proc/{current}/task').iterdir():
                todo.extend(map(int, (task / 'children').read_text().split()))
            for line in Path(f'/proc/{current}/status').read_text().splitlines():
                if line.startswith('VmRSS:'):
                    rss += int(line.split()[1]) * 1024
        except FileNotFoundError:
            pass
    return rss

def run(label, command, reject=False):
    command = list(map(str, command))
    start, peak, guard = time.monotonic(), 0, None
    with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
        child = subprocess.Popen(command, cwd=out, stdout=stdout, stderr=stderr,
                                 start_new_session=True, env=dict(os.environ, DOTNET_PROCESSOR_COUNT='2', PYTHONDONTWRITEBYTECODE='1'))
        while child.poll() is None:
            if platform.system() == 'Linux':
                peak = max(peak, memory(child.pid))
            if peak > 4 * 1024**3:
                guard = '4GiB-process-tree'
            elif time.monotonic() - start > 600:
                guard = 'bounded-fixture-600s'
            if guard:
                os.killpg(child.pid, signal.SIGTERM)
                try:
                    child.wait(timeout=5)
                except subprocess.TimeoutExpired:
                    os.killpg(child.pid, signal.SIGKILL)
                    child.wait()
                break
            time.sleep(.1)
    report['commands'].append({'label': label, 'argv': command, 'exitCode': child.returncode,
                              'guard': guard, 'peakRssBytes': peak, 'elapsedSeconds': time.monotonic() - start,
                              'stdout': record(out / (label + '.stdout')), 'stderr': record(out / (label + '.stderr'))})
    save()
    if guard or (not reject and child.returncode):
        raise RuntimeError(label + ' failed; retained diagnostics in ' + str(out))
    if reject and (child.returncode == 0 or b'POSIX import ABI' not in (out / (label + '.stderr')).read_bytes()):
        raise RuntimeError(label + ' did not reject the incompatible ABI')
    return (out / (label + '.stdout')).read_bytes()

compiler = args.compiler.resolve()
report['compiler'] = record(compiler)
network = root / 'backend/runtime/musl/network'
strto = root / 'backend/runtime/musl/strto'
report['fixtures'] = [record(tests / name) for name in ['posix_socket_translated.c', 'musl_inet_addr.c', 'posix_socket_names.cpp']]
try:
    expected = {}
    for fixture in ['posix_socket_translated', 'musl_inet_addr']:
        native = out / (fixture + '-native')
        extra = [network / 'inet_aton.c', network / 'inet_addr.c', strto / 'strtol.c', strto / 'intscan.c'] if fixture == 'musl_inet_addr' else []
        run(fixture + '-native-build', ['cc', '-std=c11', '-D_DEFAULT_SOURCE', tests / (fixture + '.c'), *extra, '-o', native])
        expected[fixture] = run(fixture + '-native', [native])
    negatives = {
        'socket-result': 'long socket(int,int,int);int main(void){return (int)socket(2,1,0);}',
        'bind-void': 'int bind(int,const void*,unsigned);int main(void){return bind(3,0,0);}',
        'accept-length': 'struct sockaddr{unsigned short sa_family;char sa_data[14];};int accept(int,struct sockaddr*,unsigned long*);int main(void){return accept(3,0,0);}',
    }
    for name, members in [('size', 'unsigned short sa_family;char sa_data[15];'), ('family', 'unsigned sa_family;char sa_data[12];'), ('field-name', 'unsigned short family;char sa_data[14];'), ('data-type', 'unsigned short sa_family;unsigned char sa_data[14];')]:
        negatives['sockaddr-' + name] = 'struct sockaddr{' + members + '};int bind(int,const struct sockaddr*,unsigned);int main(void){struct sockaddr a;return bind(3,&a,sizeof a);}'
    for name, source in negatives.items():
        path = out / (name + '.c')
        path.write_text(source + '\n')
        run('reject-' + name, [sys.executable, '-B', root / 'dotnet.py', path, '--compiler', compiler, '--language', 'c', '--libc', 'dotcc', '--posix', '--output-dir', out / ('reject-' + name)], reject=True)
    names = out / 'names'
    run('names-translate', [sys.executable, '-B', root / 'dotnet.py', tests / 'posix_socket_names.cpp', '--compiler', compiler, '--std', 'c++17', '--libc', 'dotcc', '--posix', '--no-exceptions', '--no-rtti', '--output-dir', names])
    run('names-build', ['dotnet', 'build', names / 'Generated.csproj', '-c', 'Release', '--nologo'])
    run('names-jit', ['dotnet', names / 'bin/Release/net10.0/Generated.dll'])
    # New provider TUs must parse without the optional POSIX header profile too.
    empty = out / 'empty.c'
    empty.write_text('int main(void){return 0;}\n')
    run('provider-without-posix', [sys.executable, '-B', root / 'dotnet.py', empty, '--compiler', compiler, '--language', 'c', '--libc', 'musl', '--output-dir', out / 'no-posix'])
    for fixture in expected:
        for language in ['c', 'c++']:
            for mode in ['static', 'instance']:
                label = fixture + '-' + language.replace('+', 'x') + '-' + mode
                directory = out / label
                run(label + '-translate', [sys.executable, '-B', root / 'dotnet.py', tests / (fixture + '.c'), '--compiler', compiler, '--language', language, '--libc', 'musl', '--posix', '--unbound-externals', 'reject', '--output-dir', directory,
                    *(['--instance-methods'] if mode == 'instance' else []), *(['--std', 'c++17', '--no-exceptions', '--no-rtti'] if language == 'c++' else [])])
                manifest = json.loads((directory / 'dot-gen.json').read_text())
                entries = {(entry['component'], entry['file']) for entry in manifest['libcProvider']['translationUnits']}
                assert {('network', 'inet_aton.c'), ('network', 'inet_addr.c')} <= entries
                assert not manifest['unboundExternals']['symbols']
                assert not {'inet_aton', 'inet_addr'} & set(manifest['imports'])
                if fixture == 'posix_socket_translated':
                    assert 'PosixSockets' in manifest['runtimeHelpers']
                project = directory / 'Generated.csproj'
                run(label + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo'])
                assert run(label + '-jit', ['dotnet', directory / 'bin/Release/net10.0/Generated.dll']) == expected[fixture]
                if args.nativeaot:
                    system = {'Linux': 'linux', 'Darwin': 'osx'}[platform.system()]
                    arch = 'arm64' if platform.machine().lower() in ['arm64', 'aarch64'] else 'x64'
                    run(label + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', system + '-' + arch, '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot', '--nologo'])
                    assert run(label + '-aot', [directory / 'aot/Generated']) == expected[fixture]
    assert record(compiler) == report['compiler']
    report.update(status='passed',compilerUnchanged=True)
except Exception as error:
    report.update(status='failed',failure=str(error))
    raise
finally:
    save()
