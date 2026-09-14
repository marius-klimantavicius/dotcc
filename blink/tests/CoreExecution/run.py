#!/usr/bin/env python3
"""Build the frozen real-core object link with its owning managed consumer."""
import argparse
import difflib
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--assembly-receipt', type=Path, required=True)
parser.add_argument('--prepare-only', action='store_true')
parser.add_argument('--build-only', action='store_true')
args = parser.parse_args()
assembly_path = args.assembly_receipt.resolve()
assembly = json.loads(assembly_path.read_text())
profile = Path(assembly['profile'])
inputs = json.loads((profile / 'inputs.json').read_text())
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
for name, digest in inputs['staged_headers'].items():
    if sha(profile / name) != digest:
        raise SystemExit('frozen profile changed: ' + name)
if sha(profile / 'inputs.json') != assembly['identity']['profile_inputs_sha256']:
    raise SystemExit('assembly/profile identity mismatch')
base = ROOT / 'generated/core-execution'
base.mkdir(parents=True, exist_ok=True)
a = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT / 'artifacts/core-execution' / a.name
out.mkdir(parents=True)
bindings = json.loads((profile / 'binding-sources.json').read_text())
generated = ROOT / 'generated/core-objects' / assembly['key'] / 'ManagedCore'
receipt = dict(kind='actual-upstream-core-consumer', passed=False, assembly_receipt=str(assembly_path),
               assembly_receipt_sha256=sha(assembly_path), profile=str(profile),
               profile_inputs_sha256=sha(profile / 'inputs.json'),
               diagnostic_replay=assembly.get('diagnostic_replay', False), results={})

def save():
    (out / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

def run(command, label, timeout=180):
    start = time.monotonic()
    with (out / (label + '.log')).open('wb') as log:
        try:
            code = subprocess.run(list(map(str, command)), stdout=log, stderr=subprocess.STDOUT,
                                  timeout=timeout, env=dict(os.environ, LC_ALL='C')).returncode
        except subprocess.TimeoutExpired:
            code = 124
    receipt['results'][label] = dict(command=list(map(str, command)), exit_code=code,
                                     seconds=time.monotonic() - start)
    save()
    if code:
        diagnostic_inventory(out / (label + '.log'))
        raise RuntimeError(label + ' failed: ' + str(out / (label + '.log')))
    return (out / (label + '.log')).read_text()

def diagnostic_inventory(log):
    errors = sorted(set(line.strip() for line in log.read_text().splitlines() if ': error CS' in line))
    rows = []
    upstream = ROOT / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink'
    source_lines = {path: path.read_text().splitlines() for path in upstream.glob('*.[ch]')}
    for line in errors:
        symbols = re.findall(r"'([^']+)'", line)
        hosts = [name for name in symbols if name.startswith(('blink_host_', 'blink_io_'))]
        candidates = [name for name in symbols if re.fullmatch(r'[A-Za-z_]\w*', name) and name not in hosts]
        matches = []
        # Preserve source evidence without treating a textual match as proof of
        # ownership, declaration correctness, or reachable runtime behavior.
        for name in candidates:
            pattern = re.compile(r'\b' + re.escape(name) + r'\b')
            for path, lines in source_lines.items():
                for number, text in enumerate(lines, 1):
                    if pattern.search(text):
                        matches.append(dict(symbol=name, file=str(path.relative_to(ROOT)), line=number, text=text.strip()))
                        if len(matches) >= 40:
                            break
                if len(matches) >= 40:
                    break
        rows.append(dict(diagnostic=line, isolated_host_symbols=hosts,
                         other_symbols=candidates, upstream_text_matches=matches))
    (out / 'diagnostics.json').write_text(json.dumps(rows, indent=2) + '\n')
    receipt['diagnostic_count'] = len(rows)
    save()

def project(path, name, output, compile_paths=(), references=(), root_assembly=None):
    xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(xml, 'PropertyGroup')
    for key, value in [('TargetFramework','net10.0'), ('OutputType',output),
                       ('AssemblyName',name), ('AllowUnsafeBlocks','true'),
                       ('Nullable','disable'), ('EnableDefaultCompileItems','false'),
                       ('DefineConstants','BLINK_FULL_CORE'),
                       ('WarningsAsErrors','CS8500')]:
        ET.SubElement(props,key).text=value
    items = ET.SubElement(xml, 'ItemGroup')
    for source in compile_paths:
        ET.SubElement(items, 'Compile', Include=str(source))
    for reference in references:
        ET.SubElement(items, 'ProjectReference', Include=str(reference))
    if root_assembly:
        ET.SubElement(items, 'TrimmerRootAssembly', Include=root_assembly)
    ET.ElementTree(xml).write(path, encoding='unicode')

try:
    shutil.copytree(profile / 'host-project', a / 'host')
    (a / 'bridges').mkdir()
    for relative in bindings['authored_managed']:
        shutil.copyfile(profile / relative, a / 'bridges' / Path(relative).name)
    for name in ('Program.cs', 'abi.c'):
        shutil.copyfile(ROOT / 'tests/CoreExecution' / name, a / name)
    receipt['consumer_inputs'] = {str(p.relative_to(a)):sha(p) for p in a.rglob('*') if p.is_file()}
    receipt['runner_sha256'] = sha(Path(__file__))
    for label in ('raw', 'optimized'):
        directory = a / label
        directory.mkdir()
        shutil.copytree(a / 'bridges', directory / 'bridges')
        source_dir = generated if label == 'raw' else a / 'optimized-generated'
        (directory / 'library').mkdir()
        (directory / 'consumer').mkdir()
        library = directory / 'library/ManagedCore.csproj'
        project(library, 'ManagedCore', 'Library', [source_dir / '*.cs', directory / 'bridges/*.cs'],
                [a / 'host/Managed.Emulation.Host.csproj'])
        project(directory / 'consumer/CoreExecution.csproj', 'CoreExecution', 'Exe', [a / 'Program.cs'],
                [library], 'ManagedCore')
    receipt['prepared_inputs'] = {str(p.relative_to(a)):sha(p) for p in a.rglob('*') if p.is_file()}
    receipt['prepared'] = True
    save()
    if args.prepare_only:
        print('prepared frozen consumer: ' + str(out / 'receipt.json'))
        sys.exit(0)
    if not assembly['linked']:
        raise RuntimeError('object assembly is not linked; prepared projects retained')
    for name, digest in assembly['generated'].items():
        if sha(generated / name) != digest:
            raise RuntimeError('linked generated source changed: ' + name)
    raw_hashes = {p.name:sha(p) for p in generated.glob('*.cs')}
    receipt['raw_generated'] = raw_hashes
    (a / 'optimized-generated').mkdir()
    for path in generated.glob('*.cs'):
        shutil.copyfile(path, a / 'optimized-generated' / path.name)
    closure = json.loads((profile / 'closure.json').read_text())
    native = ROOT / 'build/core-native'
    if sha(native) != closure['native_binary_sha256']:
        raise RuntimeError('native oracle differs from frozen closure')
    shutil.copyfile(native, a / 'native')
    (a / 'native').chmod(0o755)
    native_rows = run([a / 'native'], 'native', 30).splitlines()
    if not native_rows or not native_rows[0].startswith('abi '):
        raise RuntimeError('unexpected native oracle output')
    expected_rows = native_rows[1:]
    upstream = ROOT / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
    run(['cc', '-std=c17', '-D_GNU_SOURCE', '-DNDEBUG', '-DNOLINEAR', '-I', profile,
         '-I', upstream, '-iquote', profile / 'host', a / 'abi.c', '-o', a / 'native-abi'], 'native-abi-build')
    expected_abi = run([a / 'native-abi'], 'native-abi', 30).strip()
    receipt['native_abi'] = native_rows[0]
    receipt['profile_abi'] = expected_abi
    post = ROOT.parent / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
    receipt['postprocessor'] = {p.name:sha(p) for p in post.parent.glob('*.dll')}

    def check(command, label):
        actual = run(command, label, 30).splitlines()
        if not actual or actual[0] != expected_abi:
            raise RuntimeError(label + ' profile ABI mismatch')
        footer = re.fullmatch(r'core retained-mappings=(\d+) charged-bytes=(\d+)', actual[-1])
        if not footer or int(footer[2]) > 64 * 1024 * 1024:
            raise RuntimeError(label + ' missing/invalid owned-memory accounting')
        receipt['results'][label]['retained_mappings'] = int(footer[1])
        receipt['results'][label]['charged_bytes'] = int(footer[2])
        if actual[1:-1] != expected_rows:
            (out / (label + '.diff')).write_text('\n'.join(difflib.unified_diff(expected_rows, actual[1:-1], fromfile='native', tofile=label)) + '\n')
            raise RuntimeError(label + ' instruction/fault/exit rows differ from native')
        save()

    for label in ('raw', 'optimized'):
        directory = a / label
        consumer = directory / 'consumer/CoreExecution.csproj'
        if label == 'optimized':
            run(['dotnet','restore',directory / 'library/ManagedCore.csproj'], 'optimized-restore')
            run(['dotnet',post,directory / 'library/ManagedCore.csproj','--in-place'], 'postprocess', 600)
        run(['dotnet','build',consumer,'-c','Release'], label + '-build', 600)
        if args.build_only:
            break
        check(['dotnet',directory / 'consumer/bin/Release/net10.0/CoreExecution.dll'], label + '-jit')
        publish = a / (label + '-aot')
        run(['dotnet','publish',consumer,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish], label + '-aot-build', 900)
        check([publish / 'CoreExecution'], label + '-aot')
    if raw_hashes != {p.name:sha(p) for p in generated.glob('*.cs')}:
        raise RuntimeError('raw generated source changed')
    for path in (a / 'raw/bridges').glob('*.cs'):
        if sha(path) != receipt['prepared_inputs'][str(path.relative_to(a))]:
            raise RuntimeError('raw bridge source changed: ' + path.name)
    receipt['optimized_generated'] = {p.name:sha(p) for p in (a / 'optimized-generated').glob('*.cs')}
    receipt['runtime_matrix_passed'] = not args.build_only
    receipt['passed'] = not args.build_only and not receipt['diagnostic_replay']
    receipt['build_only'] = args.build_only
    save()
    print('core consumer result: ' + str(out / 'receipt.json'))
finally:
    save()
