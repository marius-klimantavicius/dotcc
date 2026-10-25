#!/usr/bin/env python3
"""Ordinary native libc fixture and private software process-maps regression.

No translation, shared builds, generated-source edits, or public managed harness.
Each compiler command uses the repository production locks; jobs are serial.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import sys
import tempfile
import time
sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REPO = ROOT.parent
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import locked_command, subprocess
EXPECTED = b'guest-process-maps: main/worker stacks, rwx/none, private/shared, unmap: PASS\n'
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--native-closure', type=Path, default=ROOT / 'artifacts/coreclr-runtime/native/attempt-h40bq9ta')
    args = parser.parse_args()
    base = ROOT / 'artifacts/guest-process-maps'; base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    print(attempt, flush=True)
    receipt = dict(kind='ordinary-guest-process-maps', passed=False, completed=False, fixture_ready=False,
        scope='Linux pinned-libc main/worker fixture; native actual HostMemory sharing metadata; private authored HostIo/InstanceIo and map-walker with ABI fixture, not generated integration',
        attempt=str(attempt), invocation=[sys.executable, *sys.argv], inputs={}, commands=[], executions=[],
        expected_stdout_hex=EXPECTED.hex(), max_build_jobs=1, managed_passed=False,
        prior_coordinator_stop='Prior coordinator stopped by automated content filter citing possible cyber risk; safeguards neither disabled nor circumvented')
    env = dict(os.environ, LC_ALL='C', MSBUILDDISABLENODEREUSE='1')
    def save(): (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
    def pin(path):
        path = Path(path).resolve(); digest = sha(path)
        if receipt['inputs'].setdefault(str(path), digest) != digest: raise RuntimeError('Input changed: ' + str(path))
        return path
    def copy(source, destination):
        source = pin(source); destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination); pin(destination)
    def run(label, command, timeout=60):
        row = dict(label=label, command=list(map(str, command)), cwd=str(attempt), timeout_seconds=timeout)
        row['effective_command'] = locked_command(row['command'], env=env)
        receipt['commands'].append(row); save()
        stdout, stderr = (attempt / (label + suffix) for suffix in ('.stdout', '.stderr'))
        process = None; start = time.monotonic()
        try:
            with stdout.open('wb') as out, stderr.open('wb') as err:
                process = subprocess.Popen(row['effective_command'], cwd=attempt, env=env,
                    stdin=subprocess.DEVNULL, stdout=out, stderr=err, start_new_session=True)
                row['pid'] = process.pid
                row['exit_code'] = process.wait(timeout=timeout)
        finally:
            signals = []
            if process is not None:
                try:
                    os.killpg(process.pid, 0)
                    os.killpg(process.pid, signal.SIGTERM); signals.append('SIGTERM')
                    try: process.wait(timeout=2)
                    except subprocess.TimeoutExpired: pass
                    try:
                        os.killpg(process.pid, 0)
                        os.killpg(process.pid, signal.SIGKILL); signals.append('SIGKILL')
                    except ProcessLookupError: pass
                    process.wait(timeout=2)
                except ProcessLookupError: pass
                try: os.killpg(process.pid, 0); gone = False
                except ProcessLookupError: gone = True
                row['cleanup'] = dict(signals=signals, group_gone=gone)
            row['seconds'] = time.monotonic() - start
            row['logs'] = {str(p): sha(p) for p in (stdout, stderr) if p.exists()}; save()
        if row.get('exit_code') != 0 or signals or not row['cleanup']['group_gone']:
            raise RuntimeError(label + ': retained output ' + str(attempt))
        print(label + ': passed', flush=True)
        return stdout.read_bytes(), stderr.read_bytes()
    try:
        pin(__file__); pin(REPO / 'Scripts/production_support.py')
        # Verify the real producer API, rather than trusting our focused stub.
        generated_api = pin(ROOT / 'generated/TranslatedBlink/Sources/BlinkCore.cs')
        api = generated_api.read_text()
        for declaration in ('public const int kStackSize', 'public const int PAGE_V',
                            'public const int PAGE_PS', 'public const int PAGE_HOST',
                            'public static unsafe partial class Libc',
                            'public static int pthread_mutex_lock', 'public static int pthread_mutex_unlock'):
            if declaration not in api: raise RuntimeError('Actual generated API differs: ' + declaration)
        receipt['generated_api'] = dict(path=str(generated_api), sha256=sha(generated_api),
            constants='BlinkCore direct members', mutex='BlinkCore.Libc', read_only_validation=True)
        if (HERE / 'README.md').exists(): pin(HERE / 'README.md')
        for name in ('fixture.c', 'memory-native.c', 'Program.cs'): copy(HERE / name, attempt / 'inputs' / name)
        native = attempt / 'native'; native.mkdir()
        for name in ('HostMemory.c', 'include/HostMemory.h'):
            copy(ROOT / 'src/Host' / name, native / Path(name).name)
        cc = pin(shutil.which('cc')); dotnet = pin(shutil.which('dotnet'))
        image = attempt / 'guest-process-maps.elf'
        run('fixture-build', [cc, '-std=c17', '-O1', '-Wall', '-Wextra', '-Werror', '-pthread', attempt / 'inputs/fixture.c', '-o', image])
        receipt['elf'] = dict(path=str(pin(image)), sha256=sha(image), dynamic=True)
        images = args.native_closure.resolve() / 'images'
        loader = pin(images / 'lib64/ld-linux-x86-64.so.2')
        libc = pin(images / 'lib/x86_64-linux-gnu/libc.so.6')
        receipt['pinned_libc'] = dict(path=str(libc), sha256=sha(libc), loader=str(loader), loader_sha256=sha(loader))
        trace = attempt / 'linux.trace'
        out, err = run('linux', [pin(shutil.which('strace')), '-f', '-s', '160', '-o', trace,
            loader, '--library-path', libc.parent, image], timeout=30)
        if out != EXPECTED or err: raise RuntimeError('Native libc output differs')
        receipt['trace'] = dict(path=str(pin(trace)), sha256=sha(trace))
        receipt['executions'].append(dict(mode='linux-pinned-libc', passed=True, stdout_hex=out.hex(), stderr_hex=err.hex()))
        receipt['fixture_ready'] = True; save()
        print('FIXTURE_READY receipt=' + str(attempt / 'receipt.json'), flush=True)
        run('native-memory-build', [cc, '-std=c17', '-Wall', '-Wextra', '-Werror', '-DBLINK_MANAGED_GUEST_THREADS',
            '-I', native, attempt / 'inputs/memory-native.c', native / 'HostMemory.c', '-pthread', '-o', native / 'memory-probe'])
        pin(native / 'memory-probe')
        out, err = run('native-memory', [native / 'memory-probe'])
        if err: raise RuntimeError('Native memory stderr')
        receipt['executions'].append(dict(mode='native-memory-owner', passed=True, stdout_hex=out.hex(), stderr_hex=err.hex()))
        host = attempt / 'host'
        shutil.copytree(ROOT / 'src/Managed.Emulation.Host', host, ignore=shutil.ignore_patterns('bin', 'obj'))
        for p in host.rglob('*'):
            if p.is_file(): pin(p)
        for name in ('HostIoBridge.cs', 'HostResourcesBridge.cs'):
            copy(ROOT / 'src/Host' / name, attempt / 'focused' / name)
        copy(ROOT / 'src/Managed.Emulation.ThreadedExecution/ThreadedGuestExecution.ProcessMaps.cs', attempt / 'focused/ThreadedGuestExecution.ProcessMaps.cs')
        copy(attempt / 'inputs/Program.cs', attempt / 'focused/Program.cs')
        project = attempt / 'focused/probe.csproj'
        project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>
<DefineConstants>BLINK_FULL_CORE</DefineConstants><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable></PropertyGroup><ItemGroup>
<ProjectReference Include="../host/Managed.Emulation.Host.csproj"/></ItemGroup></Project>''')
        pin(project)
        run('focused-build', [dotnet, 'build', project, '-c', 'Release'], timeout=120)
        out, err = run('focused', [dotnet, attempt / 'focused/bin/Release/net10.0/probe.dll'])
        if err: raise RuntimeError('Focused stderr')
        receipt['executions'].append(dict(mode='private-authored-focused', passed=True, stdout_hex=out.hex(), stderr_hex=err.hex()))
        receipt['focused_passed'] = True
        for p, digest in receipt['inputs'].items():
            if sha(p) != digest: raise RuntimeError('Frozen input changed: ' + p)
        receipt.update(passed=True, final_identities_stable=True)
    except BaseException as error: receipt['error'] = type(error).__name__ + ': ' + str(error)
    finally: receipt['completed'] = True; save()
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1
if __name__ == '__main__': raise SystemExit(main())
