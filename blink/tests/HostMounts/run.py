#!/usr/bin/env python3
"""Normal BCL mount contract qualification and native file-description witness."""
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

base = ROOT / 'generated/host-mounts'
base.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
output = ROOT / 'artifacts/host-mounts' / attempt.name
output.mkdir(parents=True)
temporary = attempt / 'tmp'
temporary.mkdir()
command_env = dict(os.environ, LC_ALL='C', TMPDIR=str(temporary))
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
r = {'kind': 'bcl-host-mounts', 'passed': False,
     'results': {}, 'attempt': str(attempt), 'temporary_directory': str(temporary),
     'managed_modes': 2,
     'scope': 'normal native file-description witness; BCL filesystem JIT/AOT mount checks, no full translated guest claim'}


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


def verify():
    for category, base in [('authored_inputs', Path('/')), ('copied_inputs', attempt)]:
        for name, digest in r[category].items():
            if sha(base / name) != digest:
                raise RuntimeError('Frozen input changed: ' + name)


try:
    if platform.system() != 'Linux' or platform.machine() != 'x86_64':
        raise RuntimeError('The native witness is currently qualified only on Linux x64')
    r['head'] = subprocess.check_output(['git', '-C', REPO, 'rev-parse', 'HEAD'], text=True).strip()
    host = ROOT / 'src/Managed.Emulation.Host'
    files = [p for p in host.rglob('*') if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(host).parts)]
    files += [ROOT / 'tests/HostMounts' / name for name in ['run.py', 'Program.cs', 'probe.c', 'README.md']]
    files += [REPO / name for name in ['Directory.Build.props', 'Directory.Packages.props', 'nuget.config']]
    cc, dotnet = [Path(shutil.which(name)).resolve() for name in ['cc', 'dotnet']]
    files += [cc, dotnet]
    r['authored_inputs'] = {str(p): sha(p) for p in files}
    shutil.copytree(host, attempt / 'host', ignore=shutil.ignore_patterns('bin', 'obj'))
    consumer = attempt / 'consumer'; consumer.mkdir()
    shutil.copyfile(ROOT / 'tests/HostMounts/Program.cs', consumer / 'Program.cs')
    shutil.copyfile(ROOT / 'tests/HostMounts/probe.c', attempt / 'probe.c')
    project = consumer / 'HostMounts.csproj'
    xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk'); props = ET.SubElement(xml, 'PropertyGroup')
    for key, value in [('TargetFramework', 'net10.0'), ('OutputType', 'Exe'), ('ImplicitUsings', 'enable'), ('Nullable', 'enable')]:
        ET.SubElement(props, key).text = value
    items = ET.SubElement(xml, 'ItemGroup'); ET.SubElement(items, 'ProjectReference', Include=str(attempt / 'host/Managed.Emulation.Host.csproj'))
    ET.ElementTree(xml).write(project, encoding='unicode')
    r['copied_inputs'] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob('*') if p.is_file()}
    run([cc, '--version'], 'native-version'); run([dotnet, '--info'], 'dotnet-info')
    run([cc, '-std=c17', '-Wall', '-Wextra', '-Werror', attempt / 'probe.c', '-o', attempt / 'native'], 'native-build')
    expected = run([attempt / 'native'], 'native', 30, [attempt / 'native'], True)
    if expected != b'normal file descriptions: PASS\n':
        raise RuntimeError('Native witness differs')
    run([dotnet, 'build', project, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'], 'managed-build', 600)
    execution = attempt / 'jit-execution'; shutil.copytree(consumer / 'bin/Release/net10.0', execution)
    binaries = sorted(p for p in execution.iterdir() if p.is_file())
    verify()
    if run([dotnet, execution / 'HostMounts.dll', attempt / 'jit-files'], 'jit', 60, binaries, True) != expected:
        raise RuntimeError('Managed JIT differs')
    publish = attempt / 'aot-execution'
    run([dotnet, 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
         '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', publish], 'aot-build', 900)
    verify()
    if run([publish / 'HostMounts', attempt / 'aot-files'], 'aot', 60, [publish / 'HostMounts'], True) != expected:
        raise RuntimeError('Managed AOT differs')
    verify(); r['passed'] = True
    print('BCL host mount contracts passed native file witness + JIT/AOT: ' + str(output / 'receipt.json'))
except BaseException as error:
    r['failure'] = {'type': type(error).__name__, 'message': str(error)}
    raise
finally:
    save()

