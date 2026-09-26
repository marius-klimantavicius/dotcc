#!/usr/bin/env python3
"""Normal pinned-header endian differential; no guest or performance claim."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_SOURCE = _campaign_json.loads((_CAMPAIGN_ROOT / "config/source-manifest.json").read_text())["upstream"]
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['tests/EndianIntrinsics/run.py']

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

UPSTREAM = ROOT / ("ref/" + _CAMPAIGN_SOURCE["directory"])
PINNED = _CAMPAIGN_INPUTS['PINNED']
base = ROOT / 'generated/endian-intrinsics'
base.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
output = ROOT / 'artifacts/endian-intrinsics' / attempt.name
output.mkdir(parents=True)
temporary = attempt / 'tmp'
temporary.mkdir()
command_env = dict(os.environ, LC_ALL='C', TMPDIR=str(temporary))
cli = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
post = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
r = {'kind': 'pinned-upstream-unsigned-endian-intrinsics', 'passed': False,
     'results': {}, 'attempt': str(attempt), 'temporary_directory': str(temporary),
     'finite_rows_per_execution': 336, 'managed_modes': 4,
     'scope': 'normal valid buffers; direct and function-pointer Get/Put16/32/64; no guest or performance qualification'}


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


def expected_transcript():
    values = [0, 1, 0x7fff, 0x8000, 0xffff, 0x10000, 0x7fffffff,
              0x80000000, 0xffffffff, 0x100000000, 0x7fffffffffffffff,
              0x8000000000000000, 0xffffffffffffffff, 0x0123456789abcdef]
    lines = []
    for width in [2, 4, 8]:
        for indirect in [0, 1]:
            for offset in [0, 1, 3, 7]:
                for value in values:
                    changed = (value & ((1 << (width * 8)) - 1)) ^ 0x5a
                    memory = bytearray([0xa5] * 32)
                    memory[8 + offset:8 + offset + width] = changed.to_bytes(width, 'little')
                    lines.append(f'{width} {indirect} {offset} {value:016x} {changed:016x} {memory.hex()}\n')
    assert len(lines) == 336
    return (''.join(lines) + 'endian intrinsics: 336 normal rows: PASS\n').encode()


def project(path, generated):
    xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(xml, 'PropertyGroup')
    for key, value in [('TargetFramework', 'net10.0'), ('OutputType', 'Exe'),
                       ('AllowUnsafeBlocks', 'true'), ('Nullable', 'enable'),
                       ('AssemblyName', 'EndianIntrinsics'), ('WarningsAsErrors', 'CS8500')]:
        ET.SubElement(props, key).text = value
    items = ET.SubElement(xml, 'ItemGroup')
    ET.SubElement(items, 'Compile', Include=str(generated / '*.cs'))
    ET.SubElement(items, 'TrimmerRootAssembly', Include='EndianIntrinsics')
    ET.ElementTree(xml).write(path, encoding='unicode')


try:
    if platform.system() != 'Linux' or platform.machine() != 'x86_64':
        raise RuntimeError('This fixture requires its native Linux x86-64 witness')
    r['head'] = subprocess.check_output(['git', '-C', REPO, 'rev-parse', 'HEAD'], text=True).strip()
    authored = [ROOT / 'tests/EndianIntrinsics' / name for name in ['run.py', 'probe.c', 'Program.cs']]
    authored.append(ROOT / 'config/managed-host/config.h')
    r['authored_inputs'] = {str(p.relative_to(ROOT)): sha(p) for p in authored}
    for name in ['probe.c', 'Program.cs']:
        shutil.copyfile(ROOT / 'tests/EndianIntrinsics' / name, attempt / name)
    headers = attempt / 'include/blink'
    headers.mkdir(parents=True)
    r['upstream_inputs'] = {}
    for name, expected in PINNED.items():
        source = UPSTREAM / 'blink' / name
        if sha(source) != expected:
            raise RuntimeError('Pinned upstream header changed: ' + str(source))
        shutil.copyfile(source, headers / name)
        r['upstream_inputs'][str(source)] = expected
    shutil.copyfile(ROOT / 'config/managed-host/config.h', attempt / 'include/config.h')
    rules = []
    for bits in [16, 32, 64]:
        for store in [False, True]:
            rules.append({'name': ('Put' if store else 'Get') + str(bits),
                          'signature': {'returnType': 'void' if store else 'u' + str(bits),
                                        'parameterTypes': ['u8 *', 'u' + str(bits)] if store else ['const u8 *'],
                                        'variadic': False},
                          'target': {'kind': 'intrinsic', 'name': ('store' if store else 'load') + '.u' + str(bits) + '.le'},
                          'declarationFile': str(headers / 'endian.h'), 'translationUnit': str(attempt / 'probe.c'),
                          'linkage': 'internal', 'requireMatch': True})
    profile = attempt / 'overrides.json'
    profile.write_text(json.dumps({'version': 1, 'functionOverrides': rules}, indent=2) + '\n')
    r['inputs'] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob('*') if p.is_file()}
    r['compiler'] = compiler_identity(cli.parent)
    r['postprocessor'] = {p.name: sha(p) for p in post.parent.glob('*.dll')}
    cc, dotnet = [Path(shutil.which(name)).resolve() for name in ['cc', 'dotnet']]
    r['native_tool'] = {'path': str(cc), 'sha256': sha(cc)}
    r['dotnet_tool'] = {'path': str(dotnet), 'sha256': sha(dotnet)}
    run([cc, '--version'], 'native-version')
    run([dotnet, '--info'], 'dotnet-info')
    run([cc, '-std=c17', '-Wall', '-Wextra', '-Werror', '-I', attempt / 'include',
         attempt / 'probe.c', '-o', attempt / 'native'], 'native-build')
    expected = run([attempt / 'native'], 'native', 30, [attempt / 'native'], execution=True)
    if expected != expected_transcript():
        raise RuntimeError('Native bytes differ from independent little-endian transcript')
    r['native_passed'] = True
    r['expected_stdout_sha256'] = hashlib.sha256(expected).hexdigest()
    raw, optimized = attempt / 'raw-generated', attempt / 'optimized-generated'
    report = attempt / 'overrides.jsonl'
    run([dotnet, cli, '-std=c17', '-DBLINK_MANAGED_ENDIAN', '-I', attempt / 'include',
         attempt / 'probe.c', '--overrides-file', profile, '--override-report', report,
         '--runtime=c', '--emit=managedlib', '--nest-types', '--class-name', 'Blink',
         '--namespace', 'Managed.Emulation', '-o', raw], 'translate')
    events = [json.loads(line) for line in report.read_text().splitlines() if line.strip()]
    matches = [event for event in events if event.get('event') == 'function-override']
    wanted = {rule['name']: 'intrinsic:' + rule['target']['name'] for rule in rules}
    if len(matches) != 6 or {event['name']: event['target'] for event in matches} != wanted:
        raise RuntimeError('Expected exactly six actual typed override matches')
    for event in matches:
        if (event.get('matches') != '1' or not event.get('signature') or
                event.get('declarationFile') != str(headers / 'endian.h') or
                event.get('translationUnit') != str(attempt / 'probe.c')):
            raise RuntimeError('Override report does not identify the exact typed upstream declaration')
    r['overrides'] = {'path': str(report), 'sha256': sha(report), 'matches': matches,
                      'physical_header': str(headers / 'endian.h'), 'header_sha256': PINNED['endian.h']}
    emission = '\n'.join(p.read_text() for p in raw.glob('*.cs'))
    methods = ['BinaryPrimitives.' + operation + 'UInt' + str(bits) + 'LittleEndian'
               for operation in ['Read', 'Write'] for bits in [16, 32, 64]]
    r['emission_evidence'] = {method: emission.count(method) for method in methods}
    if not all(r['emission_evidence'].values()) or 'delegate*<' not in emission:
        raise RuntimeError('Missing unsigned endian intrinsics or emitted function pointers')
    r['raw_generated'] = {p.name: sha(p) for p in raw.glob('*.cs')}
    shutil.copytree(raw, optimized)
    for label, generated in [('raw', raw), ('optimized', optimized)]:
        consumer = attempt / (label + '-consumer')
        consumer.mkdir()
        shutil.copyfile(attempt / 'Program.cs', consumer / 'Program.cs')
        csproj = consumer / 'EndianIntrinsics.csproj'
        project(csproj, generated)
        if label == 'optimized':
            run([dotnet, 'restore', csproj], 'optimized-restore')
            run([dotnet, post, csproj, '--in-place'], 'postprocess', 600)
        run([dotnet, 'build', csproj, '-c', 'Release'], label + '-build', 600)
        # Keep the JIT execution closure independent of the later AOT publish.
        runtime = attempt / (label + '-jit')
        shutil.copytree(consumer / 'bin/Release/net10.0', runtime)
        jit_inputs = sorted(list(runtime.glob('*.dll')) + list(runtime.glob('*.json')))
        if run([dotnet, runtime / 'EndianIntrinsics.dll'], label + '-jit', 30, jit_inputs, True) != expected:
            raise RuntimeError(label + ' JIT differs from native')
        publish = attempt / (label + '-aot')
        run([dotnet, 'publish', csproj, '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-o', publish], label + '-aot-build', 900)
        if run([publish / 'EndianIntrinsics'], label + '-aot', 30, [publish / 'EndianIntrinsics'], True) != expected:
            raise RuntimeError(label + ' NativeAOT differs from native')
    for category, basepath in [('authored_inputs', ROOT), ('inputs', attempt)]:
        for name, digest in r[category].items():
            if sha(basepath / name) != digest:
                raise RuntimeError('Input changed: ' + name)
    for name, digest in r['upstream_inputs'].items():
        if sha(Path(name)) != digest:
            raise RuntimeError('Upstream input changed: ' + name)
    if r['raw_generated'] != {p.name: sha(p) for p in raw.glob('*.cs')} or sha(report) != r['overrides']['sha256']:
        raise RuntimeError('Raw generated source or typed report changed')
    if compiler_identity(cli.parent) != r['compiler'] or r['postprocessor'] != {p.name: sha(p) for p in post.parent.glob('*.dll')}:
        raise RuntimeError('Compiler/postprocessor changed')
    for name in ['native_tool', 'dotnet_tool']:
        if sha(Path(r[name]['path'])) != r[name]['sha256']:
            raise RuntimeError('Tool identity changed: ' + name)
    r['optimized_generated'] = {p.name: sha(p) for p in optimized.glob('*.cs')}
    r['passed'] = True
    print('Pinned endian intrinsics passed native + four managed modes: ' + str(output / 'receipt.json'))
except BaseException as error:
    r['failure'] = {'type': type(error).__name__, 'message': str(error)}
    raise
finally:
    save()
