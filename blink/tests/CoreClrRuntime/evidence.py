"""Attempt-local bounded commands and immutable input identities for CoreCLR."""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import signal
import subprocess
import sys
import tempfile
import time

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
BLINK = HERE.parents[1]
REPO = BLINK.parent
ARTIFACTS = BLINK / 'artifacts/coreclr-runtime'
BASELINE = '80e4726c8970b968e00d8ac2c39c608ed04e98e9'
SDK = '10.0.112'
VERSION = '10.0.12'
HOST_COMMIT = '95017c711e'
DOTNET = Path('/usr/lib/dotnet/dotnet')
ENVIRONMENT = {'LANG': 'C', 'TZ': 'UTC', 'DOTNET_ROOT': '/dotnet',
    'LD_LIBRARY_PATH': '/lib/x86_64-linux-gnu:/usr/lib/x86_64-linux-gnu',
    'DOTNET_EnableDiagnostics': '0', 'DOTNET_SYSTEM_GLOBALIZATION_INVARIANT': '1',
    'DOTNET_PROCESSOR_COUNT': '2', 'DOTNET_GCHeapHardLimit': '2000000',
    'DOTNET_GCRegionRange': '4000000', 'DOTNET_GCRegionSize': '100000',
    'DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE': 'false', 'TMPDIR': '/tmp'}
EXPECTED = {'smoke': b'CORECLR IL OK\n',
    'workload': b'CORECLR WORKLOAD OK jit=2757056 threads=13 file=20\n',
    'service': b'SERVICE READY\nSERVICE STOPPED\n'}


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def module(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


def manifest(folder):
    return {p.relative_to(folder).as_posix(): sha(p) for p in sorted(Path(folder).rglob('*')) if p.is_file()}


def cleanup(process):
    def exists():
        process.poll()
        try: os.killpg(process.pid, 0); return True
        except ProcessLookupError: return False
    sent = []
    for sig, seconds in ((signal.SIGTERM, 2), (signal.SIGKILL, 3)):
        if not exists(): break
        try: os.killpg(process.pid, sig); sent.append(sig.name)
        except ProcessLookupError: break
        end = time.monotonic() + seconds
        while exists() and time.monotonic() < end: time.sleep(.025)
    return dict(signals=sent, group_gone=not exists(), exit_code=process.poll())


class Evidence:
    def __init__(self, kind):
        base = ARTIFACTS / kind
        base.mkdir(parents=True, exist_ok=True)
        self.directory = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
        self.value = dict(kind='coreclr-' + kind, baseline=BASELINE, passed=False, completed=False,
            invocation=[sys.executable, *sys.argv], invocation_cwd=str(Path.cwd()), inputs={}, commands=[],
            scope='Ordinary Linux x86-64 CoreCLR under translated Blink; bounded fixture evidence only')
        self.environment = dict(os.environ, LC_ALL='C', MSBUILDDISABLENODEREUSE='1',
            DOTNET_CLI_USE_MSBUILD_SERVER='0', DOTNET_CLI_TELEMETRY_OPTOUT='1')
        for name in list(self.environment):
            if name.startswith(('COMPlus_', 'CORECLR_', 'DOTNET_GC', 'DOTNET_Enable', 'DOTNET_Jit', 'LD_')):
                self.environment.pop(name)
        for name in ('BlinkProject', 'MSBuildSDKsPath', 'MSBUILD_EXE_PATH'):
            self.environment.pop(name, None)
        print('attempt=' + str(self.directory), flush=True)
        self.save()

    def save(self):
        path = self.directory / 'receipt.json'
        pending = path.with_suffix('.tmp')
        pending.write_text(json.dumps(self.value, indent=2) + '\n')
        pending.replace(path)
        return path

    def pin(self, path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
        if expected is not None and digest != expected: raise RuntimeError('Identity differs: ' + str(path))
        if self.value['inputs'].setdefault(str(path), digest) != digest: raise RuntimeError('Input changed: ' + str(path))
        return path

    def authored(self, freeze=False):
        for path in HERE.rglob('*'):
            if path.is_file() and not {'bin', 'obj', '__pycache__'}.intersection(path.relative_to(HERE).parts):
                if freeze: self.capture(path, self.directory / 'harness-inputs' / path.relative_to(HERE))
                else: self.pin(path)
        for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config', 'global.json'):
            if (REPO / name).is_file(): self.pin(REPO / name)

    def capture(self, source, target, expected=None):
        """Freeze mutable shared inputs without requiring the parent to stop work."""
        source, target = Path(source).resolve(), Path(target)
        digest = sha(source)
        if expected is not None and digest != expected: raise RuntimeError('Capture identity differs: ' + str(source))
        previous = self.value.get('captured_inputs', {}).get(str(source))
        if previous and previous['sha256'] != digest: raise RuntimeError('Input changed between captures: ' + str(source))
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(source.read_bytes())
        if sha(source) != digest or sha(target) != digest: raise RuntimeError('Source changed during capture: ' + str(source))
        self.value.setdefault('captured_inputs', {})[str(source)] = dict(sha256=digest, retained=str(target))
        self.pin(target, digest)

    def tree(self, folder):
        """Freeze a deployed closure, including membership, before execution."""
        folder = Path(folder).resolve()
        files = manifest(folder)
        if not files: raise RuntimeError('Empty execution closure: ' + str(folder))
        previous = self.value.setdefault('frozen_trees', {}).setdefault(str(folder), files)
        if previous != files: raise RuntimeError('Execution closure changed: ' + str(folder))
        return files

    def check(self):
        for name, digest in self.value['inputs'].items():
            if sha(name) != digest: raise RuntimeError('Pinned input changed: ' + name)
        for name, files in self.value.get('frozen_trees', {}).items():
            if manifest(name) != files: raise RuntimeError('Execution closure changed: ' + name)

    def start(self, label, command, timeout=60, cwd=None, env=None):
        self.check()
        row = dict(label=label, command=list(map(str, command)), cwd=str(cwd or self.directory),
            timeout_seconds=timeout, environment_overrides=env or {}, exit_code=None)
        self.value['commands'].append(row)
        stdout, stderr = [self.directory / (label + suffix) for suffix in ('.stdout', '.stderr')]
        row['stdout'], row['stderr'] = str(stdout), str(stderr)
        self.save()
        out, err = stdout.open('wb'), stderr.open('wb')
        try:
            process = subprocess.Popen(row['command'], cwd=row['cwd'], env=dict(self.environment, **(env or {})),
                stdin=subprocess.DEVNULL, stdout=out, stderr=err, start_new_session=True)
        finally: out.close(); err.close()
        row['pid'] = process.pid
        self.save()
        return process, row, time.monotonic()

    def finish_command(self, handle, require_success=True):
        process, row, began = handle
        try:
            row['exit_code'] = process.wait(timeout=max(.1, row['timeout_seconds'] - (time.monotonic() - began)))
        except BaseException as error:
            row['error'] = repr(error)
        finally:
            row['cleanup'] = cleanup(process)
            row['seconds'] = time.monotonic() - began
            row['logs'] = {row[name]: sha(row[name]) for name in ('stdout', 'stderr')}
            self.save()
        okay = row['exit_code'] == 0 and not row['cleanup']['signals'] and row['cleanup']['group_gone']
        if not okay and require_success: raise RuntimeError(row['label'] + ' failed; see ' + row['stderr'])
        self.check()
        print(row['label'] + ': ' + ('passed' if okay else 'failed'), flush=True)
        return Path(row['stdout']).read_bytes(), Path(row['stderr']).read_bytes()

    def run(self, label, command, timeout=60, cwd=None, env=None, require_success=True):
        return self.finish_command(self.start(label, command, timeout, cwd, env), require_success)

    def finish(self):
        self.value['completed'] = True
        path = self.save()
        print('passed=' + str(self.value['passed']) + ' receipt=' + str(path), flush=True)
        return 0 if self.value['passed'] else 1
