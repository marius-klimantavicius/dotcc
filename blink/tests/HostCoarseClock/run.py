#!/usr/bin/env python3
"""Normal coarse monotonic clock native/managed qualification."""
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
sys.path.insert(0, str(ROOT / 'scripts'))
from core_inputs import compiler_identity

base = ROOT / 'generated/host-coarse-clock'
base.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
output = ROOT / 'artifacts/host-coarse-clock' / attempt.name
output.mkdir(parents=True)
temporary = attempt / 'tmp'
temporary.mkdir()
command_env = dict(os.environ, LC_ALL='C', TMPDIR=str(temporary))
cli = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
post = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
r = {'kind': 'host-coarse-monotonic-clock', 'passed': False,
     'results': {}, 'attempt': str(attempt), 'temporary_directory': str(temporary),
     'normal_live_reads': 64, 'controlled_provider_rows': 13, 'managed_modes': 4,
     'scope': 'normal valid clock6 calls through authored host callbacks; no full guest dispatch qualification'}


def save():
    pending = output / 'receipt.json.tmp'
    pending.write_text(json.dumps(r, indent=2) + '\n')
    pending.replace(output / 'receipt.json')


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
    raise InterruptedError('Runner interrupted by signal ' + str(signum))


signal.signal(signal.SIGTERM, interrupted)


def run(command, label, timeout=180, binaries=(), execution=False):
    command = list(map(str, command))
    before = {str(p): sha(p) for p in binaries}
    started = time.monotonic()
    with (output / (label + '.stdout')).open('wb') as stdout, (output / (label + '.stderr')).open('wb') as stderr:
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
                          'seconds': time.monotonic() - started,
                          'stdout_sha256': sha(output / (label + '.stdout')),
                          'stderr_sha256': sha(output / (label + '.stderr')),
                          'binaries_before': before, 'binaries_after': after}
    if failure is not None:
        r['results'][label]['interruption'] = {'type': type(failure).__name__, 'message': str(failure)}
    save()
    if failure is not None:
        raise failure
    if code:
        raise RuntimeError(label + ' failed; ' + str(output))
    if before != after:
        raise RuntimeError(label + ' execution binary changed')
    if execution and (output / (label + '.stderr')).stat().st_size:
        raise RuntimeError(label + ' produced execution stderr')
    return (output / (label + '.stdout')).read_bytes()


def project(path, generated):
    xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(xml, 'PropertyGroup')
    for key, value in [('TargetFramework', 'net10.0'), ('OutputType', 'Exe'),
                       ('AllowUnsafeBlocks', 'true'), ('Nullable', 'enable'),
                       ('AssemblyName', 'HostCoarseClock'), ('WarningsAsErrors', 'CS8500')]:
        ET.SubElement(props, key).text = value
    items = ET.SubElement(xml, 'ItemGroup')
    ET.SubElement(items, 'Compile', Include=str(generated / '*.cs'))
    ET.SubElement(items, 'ProjectReference', Include=str(attempt / 'host/Managed.Emulation.Host.csproj'))
    ET.SubElement(items, 'TrimmerRootAssembly', Include='HostCoarseClock')
    ET.ElementTree(xml).write(path, encoding='unicode')


def verify_inputs():
    for name, digest in r.get('authored_inputs', {}).items():
        if sha(Path(name)) != digest:
            raise RuntimeError('Authored input changed: ' + name)
    for name, digest in r.get('inputs', {}).items():
        if sha(attempt / name) != digest:
            raise RuntimeError('Copied input changed: ' + name)
    if compiler_identity(cli.parent) != r['compiler'] or r['postprocessor'] != {p.name: sha(p) for p in post.parent.glob('*.dll')}:
        raise RuntimeError('Compiler/postprocessor changed')
    for name in ['native_tool', 'dotnet_tool']:
        if sha(Path(r[name]['path'])) != r[name]['sha256']:
            raise RuntimeError('Tool identity changed: ' + name)
    if 'raw_generated' in r and r['raw_generated'] != {p.name: sha(p) for p in (attempt / 'raw-generated').glob('*.cs')}:
        raise RuntimeError('Raw generated source changed')


try:
    if platform.system() != 'Linux' or platform.machine() != 'x86_64':
        raise RuntimeError('This fixture requires its native Linux x86-64 witness')
    r['head'] = subprocess.check_output(['git', '-C', REPO, 'rev-parse', 'HEAD'], text=True).strip()
    authored = [ROOT / 'tests/HostCoarseClock' / name for name in ['run.py', 'probe.c', 'Program.cs', 'README.md']]
    authored.append(ROOT / 'scripts/core_inputs.py')
    bridges = ['HostEnvironmentBridge.cs', 'HostClockBridge.cs']
    authored.extend(ROOT / 'src/Host' / name for name in bridges)
    authored.extend(p for p in (ROOT / 'config/managed-host').rglob('*') if p.is_file())
    authored.extend(p for p in (ROOT / 'src/Managed.Emulation.Host').rglob('*')
                    if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(ROOT / 'src/Managed.Emulation.Host').parts))
    r['authored_inputs'] = {str(p): sha(p) for p in authored}
    for name in ['probe.c', 'Program.cs']:
        shutil.copyfile(ROOT / 'tests/HostCoarseClock' / name, attempt / name)
    for name in bridges:
        shutil.copyfile(ROOT / 'src/Host' / name, attempt / name)
    shutil.copytree(ROOT / 'config/managed-host', attempt / 'profile')
    shutil.copytree(ROOT / 'src/Managed.Emulation.Host', attempt / 'host', ignore=shutil.ignore_patterns('bin', 'obj'))
    r['inputs'] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob('*') if p.is_file()}
    r['compiler'] = compiler_identity(cli.parent)
    r['postprocessor'] = {p.name: sha(p) for p in post.parent.glob('*.dll')}
    cc, dotnet = [Path(shutil.which(name)).resolve() for name in ['cc', 'dotnet']]
    r['native_tool'] = {'path': str(cc), 'sha256': sha(cc)}
    r['dotnet_tool'] = {'path': str(dotnet), 'sha256': sha(dotnet)}
    run([cc, '--version'], 'native-version')
    run([dotnet, '--info'], 'dotnet-info')
    run([cc, '-std=c17', '-Wall', '-Wextra', '-Werror', attempt / 'probe.c', '-o', attempt / 'native'], 'native-build')
    expected = run([attempt / 'native'], 'native', 30, [attempt / 'native'], execution=True)
    if expected != b'coarse monotonic clock: valid output, guards, argument evaluation, nondecreasing time and resolution: PASS\n':
        raise RuntimeError('Native normal-call witness did not pass')
    r['native_passed'] = True
    r['expected_stdout_sha256'] = hashlib.sha256(expected).hexdigest()
    raw, optimized = attempt / 'raw-generated', attempt / 'optimized-generated'
    run([dotnet, cli, '-std=c17', '-DBLINK_MANAGED_COARSE_CLOCK', '-I', attempt / 'profile',
         attempt / 'probe.c', '--runtime=c', '--emit=managedlib', '--nest-types', '--class-name', 'Blink',
         '--namespace', 'Managed.Emulation', '-o', raw], 'translate')
    r['raw_generated'] = {p.name: sha(p) for p in raw.glob('*.cs')}
    verify_inputs()
    shutil.copytree(raw, optimized)
    for label, generated in [('raw', raw), ('optimized', optimized)]:
        consumer = attempt / (label + '-consumer')
        consumer.mkdir()
        for name in ['Program.cs'] + bridges:
            shutil.copyfile(attempt / name, consumer / name)
        csproj = consumer / 'HostCoarseClock.csproj'
        project(csproj, generated)
        if label == 'optimized':
            run([dotnet, 'restore', csproj], 'optimized-restore')
            run([dotnet, post, csproj, '--in-place'], 'postprocess', 600)
        run([dotnet, 'build', csproj, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'], label + '-build', 600)
        runtime = attempt / (label + '-jit')
        shutil.copytree(consumer / 'bin/Release/net10.0', runtime)
        jit_inputs = sorted(p for p in runtime.iterdir() if p.is_file())
        verify_inputs()
        if run([dotnet, runtime / 'HostCoarseClock.dll'], label + '-jit', 30, jit_inputs, True) != expected:
            raise RuntimeError(label + ' JIT differs from native')
        publish = attempt / (label + '-aot')
        run([dotnet, 'publish', csproj, '-c', 'Release', '-r', 'linux-x64',
             '--disable-build-servers', '-p:UseSharedCompilation=false', '-p:PublishAot=true', '-o', publish], label + '-aot-build', 900)
        verify_inputs()
        if run([publish / 'HostCoarseClock'], label + '-aot', 30, [publish / 'HostCoarseClock'], True) != expected:
            raise RuntimeError(label + ' NativeAOT differs from native')
    verify_inputs()
    r['optimized_generated'] = {p.name: sha(p) for p in optimized.glob('*.cs')}
    r['passed'] = True
    print('Coarse monotonic clock passed native + four managed modes: ' + str(output / 'receipt.json'))
except BaseException as error:
    r['failure'] = {'type': type(error).__name__, 'message': str(error)}
    raise
finally:
    save()
