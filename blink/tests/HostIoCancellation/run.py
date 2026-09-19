#!/usr/bin/env python3
"""Qualify private callback cancellation through translated C, not guest execution."""
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import signal
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
sys.path.insert(0, str(ROOT/'scripts'))
from core_inputs import compiler_identity

base = ROOT/'generated/host-io-cancellation'
base.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
output = ROOT/'artifacts/host-io-cancellation'/attempt.name
output.mkdir(parents=True)
temporary = attempt/'tmp'
temporary.mkdir()
command_env = dict(os.environ, LC_ALL='C', TMPDIR=str(temporary))
cli = REPO/'DotCC/bin/Release/net10.0/dotcc.dll'
post = REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
BRIDGES = ['HostIo', 'HostNetwork', 'HostMessages', 'HostReadiness']
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
r = {'kind': 'translated-private-io-cancellation', 'passed': False, 'results': {},
     'temporary_directory': str(temporary),
     'finite_cases': {
         'translated_scenarios': 22,
         'direct_bcl_scenarios': 1,
         'translated': ['pipe-read-cancel', 'pipe-readv-cancel', 'pipe-write-cancel',
             'pipe-writev-cancel', 'pipe-read-deadline', 'pipe-readv-dispose',
             'pipe-write-partial', 'pipe-writev-partial', 'socket-recv-cancel',
             'socket-recvmsg-cancel', 'socket-accept-cancel', 'socket-recv-deadline',
             'socket-recvmsg-dispose', 'poll-cancel', 'poll-deadline',
             'send-pre-canceled', 'sendmsg-pre-canceled', 'connect-pre-canceled',
             'binding-reset', 'default-connect-send', 'default-connect-sendmsg',
             'two-owner-isolation'],
         'direct_bcl': ['pipe-read-cancel'],
     },
     'scope': 'authored callback contract; no guest dispatcher/loop, signal delivery, service or worker',
     'limitations': [
         'Native C validates ordinary pipe/vector/poll behavior and ABI, not CancellationToken semantics.',
         'Direct BCL counterpart qualifies pending pipe-read cancellation only; other rows assert explicit private callback contracts. ECANCELED125 is not guest EINTR.',
         'Pipe/socket pending counters observe actual operations. Poll has no public pending counter; its incomplete worker is observed on a known nonready source.',
         'Send/sendmsg/connect cancellation is checked before entry with valid resources; no blocked connect/send timing claim.',
         'Guest poll/nanosleep retry and CPU execution stop/deadline integration remain unqualified.',
     ]}


def save():
    pending = output/'receipt.json.tmp'
    pending.write_text(json.dumps(r, indent=2)+'\n')
    pending.replace(output/'receipt.json')


def stop_group(process):
    if process.poll() is not None:
        return
    try:
        os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError:
        process.wait()
        return
    try:
        process.wait(timeout=10)
    except subprocess.TimeoutExpired:
        try:
            os.killpg(process.pid, signal.SIGKILL)
        except ProcessLookupError:
            pass
        process.wait()


def interrupted(signum, frame):
    raise InterruptedError('Runner interrupted by signal '+str(signum))

signal.signal(signal.SIGTERM, interrupted)


def run(command, label, timeout=180, binaries=(), execution=False):
    command = list(map(str, command))
    before = {str(p): sha(p) for p in binaries}
    started = time.monotonic()
    with (output/(label+'.stdout')).open('wb') as stdout, (output/(label+'.stderr')).open('wb') as stderr:
        process = subprocess.Popen(command, cwd=attempt, stdout=stdout, stderr=stderr,
                                   start_new_session=True, env=command_env)
        failure = None
        try:
            code = process.wait(timeout=timeout)
        except BaseException as error:
            failure = error
            stop_group(process)
            code = process.returncode
    after = {str(p): sha(p) for p in binaries}
    r['results'][label] = {'command': command, 'exit_code': code,
                           'seconds': time.monotonic()-started,
                           'stdout_sha256': sha(output/(label+'.stdout')),
                           'stderr_sha256': sha(output/(label+'.stderr')),
                           'binaries_before': before, 'binaries_after': after}
    if failure is not None:
        r['results'][label]['interruption'] = {'type': type(failure).__name__, 'message': str(failure)}
    save()
    if failure is not None:
        raise failure
    if code:
        raise RuntimeError(label+' failed; '+str(output))
    if before != after:
        raise RuntimeError(label+' execution binary identity changed')
    if execution and (output/(label+'.stderr')).stat().st_size:
        raise RuntimeError(label+' produced unexpected execution stderr')
    return (output/(label+'.stdout')).read_bytes()


def project(path, generated):
    xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(xml, 'PropertyGroup')
    for key, value in [('TargetFramework', 'net10.0'), ('OutputType', 'Exe'),
                       ('AllowUnsafeBlocks', 'true'), ('Nullable', 'enable'),
                       ('AssemblyName', 'HostIoCancellation'), ('WarningsAsErrors', 'CS8500')]:
        ET.SubElement(props, key).text = value
    items = ET.SubElement(xml, 'ItemGroup')
    ET.SubElement(items, 'Compile', Include=str(generated/'*.cs'))
    ET.SubElement(items, 'ProjectReference', Include=str(attempt/'host/Managed.Emulation.Host.csproj'))
    ET.SubElement(items, 'TrimmerRootAssembly', Include='HostIoCancellation')
    ET.ElementTree(xml).write(path, encoding='unicode')


try:
    if platform.system() != 'Linux' or platform.machine() != 'x86_64':
        raise RuntimeError('This normal fixture requires Linux x86-64; no substituted native witness')
    r['head'] = subprocess.check_output(['git', '-C', REPO, 'rev-parse', 'HEAD'], text=True).strip()
    r['runner_sha256'] = sha(Path(__file__))
    owned = [ROOT/'tests/HostIoCancellation'/name for name in ['run.py', 'probe.c', 'Program.cs']]
    bridges = [ROOT/'src'/name/(name+'Bridge.cs') for name in BRIDGES]
    r['authored_inputs'] = {str(p.relative_to(ROOT)): sha(p) for p in owned+bridges}
    for name in ['probe.c', 'Program.cs']:
        shutil.copyfile(ROOT/'tests/HostIoCancellation'/name, attempt/name)
    for source in bridges:
        shutil.copyfile(source, attempt/source.name)
    shutil.copytree(ROOT/'config/managed-host', attempt/'profile')
    for directory, name in [('HostIo', 'host-io.h'), ('HostMessages', 'host-messages.h')]:
        shutil.copyfile(ROOT/'src'/directory/name, attempt/'profile'/name)
    shutil.copytree(ROOT/'src/Managed.Emulation.Host', attempt/'host', ignore=shutil.ignore_patterns('bin', 'obj'))
    r['inputs'] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob('*') if p.is_file()}
    r['host_sources'] = {str(p.relative_to(attempt/'host')): sha(p) for p in (attempt/'host').rglob('*') if p.is_file()}
    r['compiler'] = compiler_identity(cli.parent)
    r['postprocessor'] = {p.name: sha(p) for p in post.parent.glob('*.dll')}
    cc = Path(shutil.which('cc')).resolve()
    dotnet = Path(shutil.which('dotnet')).resolve()
    r['native_tool'] = {'path': str(cc), 'sha256': sha(cc)}
    r['dotnet_tool'] = {'path': str(dotnet), 'sha256': sha(dotnet)}
    r['platform'] = platform.platform()
    run([cc, '--version'], 'native-version')
    run(['dotnet', '--info'], 'dotnet-info')
    run([cc, '-std=c17', '-Wall', '-Wextra', '-Werror', attempt/'probe.c', '-o', attempt/'native'], 'native-build')
    expected_abi = run([attempt/'native'], 'native-smoke', binaries=[attempt/'native'], execution=True)
    required_abi = b'C ABI iovec=16 msghdr=56 pollfd=8 canceled=125\n'
    if expected_abi != required_abi:
        raise RuntimeError('Native ABI/ordinary pipe smoke differs from fixed Linux LP64 contract')
    r['native_smoke_passed'] = True
    r['expected_private_stdout'] = (expected_abi + b'callback cancellation: pipes, vectors, sockets, messages, poll, deadlines, partial writes, reset, isolation, drain: PASS\n').decode()
    raw = attempt/'raw-generated'
    optimized = attempt/'optimized-generated'
    run(['dotnet', cli, '-std=c17', '-DBLINK_MANAGED_CANCELLATION', '-I', attempt/'profile',
         attempt/'probe.c', '--runtime=c', '--emit=managedlib', '--nest-types', '--class-name', 'Blink',
         '--namespace', 'Managed.Emulation', '-o', raw], 'translate')
    if compiler_identity(cli.parent) != r['compiler']:
        raise RuntimeError('Compiler changed during emission')
    r['raw_generated'] = {p.name: sha(p) for p in raw.glob('*.cs')}
    shutil.copytree(raw, optimized)
    for label, generated in [('raw', raw), ('optimized', optimized)]:
        consumer = attempt/(label+'-consumer')
        consumer.mkdir()
        for source in ['Program.cs']+[name+'Bridge.cs' for name in BRIDGES]:
            shutil.copyfile(attempt/source, consumer/source)
        csproj = consumer/'HostIoCancellation.csproj'
        project(csproj, generated)
        if label == 'optimized':
            run(['dotnet', 'restore', csproj], 'optimized-restore')
            run(['dotnet', post, csproj, '--in-place'], 'postprocess', 600)
        run(['dotnet', 'build', csproj, '-c', 'Release'], label+'-build', 600)
        runtime = consumer/'bin/Release/net10.0'
        jit_inputs = sorted(list(runtime.glob('*.dll'))+list(runtime.glob('*.json')))
        actual = run(['dotnet', runtime/'HostIoCancellation.dll'], label+'-jit', 90, jit_inputs, execution=True)
        if actual.decode() != r['expected_private_stdout']:
            raise RuntimeError(label+' JIT private contract transcript differs')
        publish = attempt/(label+'-aot')
        run(['dotnet', 'publish', csproj, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', publish], label+'-aot-build', 900)
        actual = run([publish/'HostIoCancellation'], label+'-aot', 90, [publish/'HostIoCancellation'], execution=True)
        if actual.decode() != r['expected_private_stdout']:
            raise RuntimeError(label+' NativeAOT private contract transcript differs')
    for name, digest in r['authored_inputs'].items():
        if sha(ROOT/name) != digest:
            raise RuntimeError('Authored input changed: '+name)
    for name, digest in r['inputs'].items():
        if sha(attempt/name) != digest:
            raise RuntimeError('Frozen input changed: '+name)
    if r['raw_generated'] != {p.name: sha(p) for p in raw.glob('*.cs')}:
        raise RuntimeError('Raw generated sources changed')
    if compiler_identity(cli.parent) != r['compiler'] or r['postprocessor'] != {p.name: sha(p) for p in post.parent.glob('*.dll')}:
        raise RuntimeError('Compiler/postprocessor changed during qualification')
    r['tools_after'] = {name: {'path': str(Path(shutil.which(name)).resolve()),
                              'sha256': sha(Path(shutil.which(name)).resolve())}
                        for name in ['cc', 'dotnet']}
    if r['tools_after'] != {'cc': r['native_tool'], 'dotnet': r['dotnet_tool']}:
        raise RuntimeError('Native compiler or dotnet executable identity changed')
    r['optimized_generated'] = {p.name: sha(p) for p in optimized.glob('*.cs')}
    r['passed'] = True
    save()
    print('Private callback cancellation all four modes passed: '+str(output/'receipt.json'))
except BaseException as error:
    r['failure'] = {'type': type(error).__name__, 'message': str(error)}
    raise
finally:
    save()
