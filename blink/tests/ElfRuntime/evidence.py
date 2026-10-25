"""Shared bounded process/identity handling for the finite ELF suite."""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import signal
import sys
import tempfile

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REPO = ROOT.parent
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import subprocess


def module(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


workers = module(ROOT / 'tests/WorkerInstances/run.py', 'elf_worker_evidence')
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()


class Evidence:
    def __init__(self, kind):
        base = ROOT / 'artifacts/elf-runtime' / kind
        base.mkdir(parents=True, exist_ok=True)
        self.directory = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
        self.value = dict(kind='elf-runtime-' + kind, passed=False, completed=False,
                          inputs={}, commands=[], scope='Finite valid Linux x86-64 ELF suite; no general runtime compatibility claim')
        self.environment = dict(os.environ, LC_ALL='C', MSBUILDDISABLENODEREUSE='1',
            DOTNET_CLI_USE_MSBUILD_SERVER='0', DOTCC_CAMPAIGN_COMMAND_LOCKS='production')
        for key in ('LD_PRELOAD', 'LD_LIBRARY_PATH', 'LD_AUDIT', 'MAKEFLAGS', 'CFLAGS', 'CPPFLAGS', 'LDFLAGS'):
            self.environment.pop(key, None)
        print(self.directory, flush=True)

    def save(self):
        path = self.directory / 'receipt.json'
        pending = path.with_suffix('.tmp')
        pending.write_text(json.dumps(self.value, indent=2) + '\n')
        pending.replace(path)
        return path

    def pin(self, path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
        if expected is not None and digest != expected:
            raise RuntimeError('Identity differs: ' + str(path))
        if self.value['inputs'].setdefault(str(path), digest) != digest:
            raise RuntimeError('Input changed: ' + str(path))
        return path

    def check(self):
        for path, digest in self.value['inputs'].items():
            if sha(path) != digest: raise RuntimeError('Frozen input changed: ' + path)

    def run(self, label, command, timeout=60, cwd=None, expected=0):
        self.check()
        row = dict(label=label, command=list(map(str, command)), cwd=str(cwd or self.directory),
                   timeout_seconds=timeout, exit_code=None)
        self.value['commands'].append(row)
        output, errors = [self.directory / (label + suffix) for suffix in ('.stdout', '.stderr')]
        process = None
        self.save()
        try:
            with output.open('wb') as out, errors.open('wb') as err:
                process = subprocess.Popen(row['command'], cwd=row['cwd'], env=self.environment,
                    stdin=subprocess.DEVNULL, stdout=out, stderr=err, start_new_session=True)
                row['pid'] = process.pid
                row['exit_code'] = process.wait(timeout=timeout)
        finally:
            if process is not None: row['cleanup'] = workers.cleanup(process)
            row['logs'] = {str(p): sha(p) for p in (output, errors) if p.is_file()}
            self.save()
        if row['exit_code'] != expected or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError(label + ' did not complete normally; see ' + str(errors))
        self.check()
        print(label + ': passed', flush=True)
        return output.read_bytes(), errors.read_bytes()

    def finish(self):
        self.value['completed'] = True
        path = self.save()
        index = path.parent.parent.parent / 'index.json'
        previous = json.loads(index.read_text()) if index.exists() else {}
        previous[self.value['kind']] = dict(receipt=str(path), sha256=sha(path), passed=self.value['passed'])
        index.write_text(json.dumps(previous, indent=2) + '\n')
        print('passed=' + str(self.value['passed']) + ' receipt=' + str(path), flush=True)
        return 0 if self.value['passed'] else 1


def interrupted(number, frame):
    raise InterruptedError('Received signal ' + str(number))


signal.signal(signal.SIGTERM, interrupted)
