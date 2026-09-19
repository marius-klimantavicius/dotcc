#!/usr/bin/env python3
"""Run only the existing normal WAT oracle with frozen compiler/test binaries."""
from collections import Counter
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import signal
import subprocess
import tempfile
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'DotCC.FunctionalTests/WatOracleTests.cs'
PROJECT = ROOT / 'DotCC.FunctionalTests/DotCC.FunctionalTests.csproj'
TESTS = PROJECT.parent / 'bin/Release/net10.0'
CLI = ROOT / 'DotCC/bin/Release/net10.0'
FILTER = 'FullyQualifiedName~DotCC.FunctionalTests.WatOracleTests'
PREFIX = 'DotCC.FunctionalTests.WatOracleTests.'
BASE = ROOT / 'blink/artifacts/wat-regression'
BASE.mkdir(parents=True, exist_ok=True)
OUT = Path(tempfile.mkdtemp(prefix='attempt-', dir=BASE))
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
receipt = dict(kind='normal-wat-oracle', passed=False, commands=[], runner_sha256=sha(Path(__file__)),
               scope='Existing authored normal C-to-WAT cases, wat2wasm validation and Node execution; no native differential')
shutil.copyfile(Path(__file__), OUT / 'runner.py')
(OUT / 'tmp').mkdir()
env = dict(os.environ, DOTCC_RUN_WAT='1', TMPDIR=str(OUT / 'tmp'),
           LC_ALL='C', DOTNET_CLI_UI_LANGUAGE='en-US', VSLANG='1033')
receipt['environment_overrides'] = {k: env[k] for k in ('DOTCC_RUN_WAT', 'TMPDIR', 'LC_ALL', 'DOTNET_CLI_UI_LANGUAGE', 'VSLANG')}


def save():
    (OUT / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')


def binaries(directory):
    return {str(p): sha(p) for p in sorted(directory.iterdir())
            if p.is_file() and p.suffix in ('.dll', '.json')}


def tool(name):
    path = shutil.which(name, path=env['PATH'])
    if path is None:
        raise RuntimeError('Required tool unavailable: ' + name)
    resolved = Path(path).resolve()
    return dict(path=path, resolved=str(resolved), sha256=sha(resolved))


def identity():
    return dict(cli=binaries(CLI), tests=binaries(TESTS),
                tools={name: tool(name) for name in ('dotnet', 'wat2wasm', 'node')},
                authored={str(p): sha(p) for p in authored}, runner_sha256=sha(Path(__file__)))


def check_identity():
    if identity() != receipt['inputs']:
        raise RuntimeError('Frozen compiler, test, tool or source identity changed')


def stop(process):
    try:
        os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError:
        process.wait()
        return
    try:
        process.wait(timeout=10)
    except subprocess.TimeoutExpired:
        pass
    try:
        os.killpg(process.pid, signal.SIGKILL)
    except ProcessLookupError:
        pass
    process.wait()


def interrupted(signum, frame):
    raise KeyboardInterrupt('Signal ' + str(signum))


signal.signal(signal.SIGTERM, interrupted)


def run(label, command, timeout):
    path = OUT / (label + '.log')
    row = dict(label=label, command=list(map(str, command)), cwd=str(ROOT), log=str(path),
               timeout_seconds=timeout, exit_code=None)
    receipt['commands'].append(row)
    save()
    print('RUN ' + label, flush=True)
    start = time.monotonic()
    try:
        with path.open('w') as log:
            process = subprocess.Popen(row['command'], cwd=ROOT, env=env, stdout=log,
                                       stderr=subprocess.STDOUT, start_new_session=True)
            try:
                row['exit_code'] = process.wait(timeout=timeout)
            except BaseException as error:
                stop(process)
                row.update(exit_code=process.returncode, error=repr(error),
                           timed_out=isinstance(error, subprocess.TimeoutExpired))
                raise
    finally:
        row['seconds'] = time.monotonic() - start
        if path.is_file(): row['log_sha256'] = sha(path)
        save()
    check_identity()
    if row['exit_code'] != 0:
        raise RuntimeError(label + ' failed; see retained log')
    return path.read_text()


def source_selection():
    pending, rows, methods = [], [], {}
    for line_number, line in enumerate(SOURCE.read_text().splitlines(), 1):
        if line.strip().startswith('[InlineData('):
            if ')]' not in line:
                raise RuntimeError('Review changed multiline WAT test metadata')
            pending.append(dict(line=line_number, inline_data=line.strip()))
        match = re.match(r'    public void (Wat_\w+)\(', line)
        if match:
            method = match.group(1)
            if not pending or method in methods:
                raise RuntimeError('Unexpected WAT theory metadata')
            methods[method] = len(pending)
            rows.extend(dict(method=method, **row) for row in pending)
            pending = []
    if pending or set(methods) != {'Wat_program_returns_expected_value', 'Wat_program_writes_expected_stdout'}:
        raise RuntimeError('Review changed WAT method selection')
    return dict(source=str(SOURCE), source_sha256=sha(SOURCE), methods=methods, rows=rows, count=len(rows))


save()
print(OUT / 'receipt.json', flush=True)
try:
    authored = [SOURCE, PROJECT, ROOT / '.github/workflows/dotnet.yml']
    for name in ('global.json', 'Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'NuGet.Config'):
        for directory in (ROOT, PROJECT.parent):
            path = directory / name
            if path.is_file(): authored.append(path)
    receipt['inputs'] = identity()
    if sha(TESTS / 'DotCC.Lib.dll') != sha(CLI / 'DotCC.Lib.dll'):
        raise RuntimeError('Already-built functional tests use another compiler')
    receipt['shared_compiler_inputs'] = {}
    for path in CLI.glob('*.dll'):
        other = TESTS / path.name
        if other.exists():
            if sha(path) != sha(other):
                raise RuntimeError('Test compiler dependency differs: ' + path.name)
            receipt['shared_compiler_inputs'][path.name] = sha(path)
    receipt['selection'] = source_selection()
    shutil.copyfile(SOURCE, OUT / 'WatOracleTests.cs')
    receipt['tool_versions'] = {name: run(name + '-version', [receipt['inputs']['tools'][name]['path'], '--version'], 30).strip()
                                for name in ('dotnet', 'wat2wasm', 'node')}
    common = [receipt['inputs']['tools']['dotnet']['path'], 'test', PROJECT, '-c', 'Release', '--no-build',
              '--nologo', '--filter', FILTER]
    listing = run('discover', [*common, '--list-tests'], 180)
    discovered = [line.strip() for line in listing.splitlines() if line.strip().startswith(PREFIX)]
    expected_methods = receipt['selection']['methods']
    observed_methods = Counter(name[len(PREFIX):].split('(', 1)[0] for name in discovered)
    if not discovered or len(discovered) != receipt['selection']['count'] or observed_methods != expected_methods:
        raise RuntimeError('Discovered test metadata differs from reviewed source selection')
    receipt['discovered_cases'] = discovered
    run('oracle', [*common, '--logger', 'trx;LogFileName=results.trx', '--results-directory', OUT,
                   '--blame-hang-timeout', '300s'], 3600)
    trx_path = OUT / 'results.trx'
    trx = ET.parse(trx_path).getroot()
    counts = trx.find('.//{*}Counters').attrib
    rows = [dict(x.attrib) for x in trx.findall('.//{*}UnitTestResult')]
    receipt.update(trx_sha256=sha(trx_path), counts=counts, cases=rows)
    count = receipt['selection']['count']
    if (Counter(row['testName'] for row in rows) != Counter(discovered)
            or len({row['testId'] for row in rows}) != count
            or any(row.get('outcome') != 'Passed' for row in rows)
            or any(int(counts.get(key, -1)) != count for key in ('total', 'executed', 'passed'))
            or any(int(counts.get(key, 0)) for key in ('failed', 'error', 'timeout', 'aborted', 'notExecuted', 'notRunnable', 'inconclusive'))):
        raise RuntimeError('Incomplete, skipped, duplicated or unexpected WAT case results')
    receipt['passed'] = True
except BaseException as error:
    receipt['passed'] = False
    receipt['error'] = repr(error)
    raise
finally:
    receipt['final_identity_stable'] = False
    try:
        receipt['inputs_after'] = identity()
        check_identity()
        for row in receipt['commands']:
            if sha(Path(row['log'])) != row['log_sha256']:
                raise RuntimeError('Command log changed after closure')
        receipt['final_identity_stable'] = True
    except Exception as error:
        receipt['passed'] = False
        receipt['identity_error'] = repr(error)
    save()
raise SystemExit(0 if receipt['passed'] else 1)
