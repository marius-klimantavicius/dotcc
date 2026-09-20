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
sys.path.insert(0, str(ROOT / "scripts"))
from core_inputs import compiler_identity
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
if 'guest-threads-boundary.json' in inputs['staged_headers']:
    raise SystemExit('CoreExecution requires the single-threaded profile: its exit probe has no guest thread owner. Use a threaded owning consumer for this product.')
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
        if Path(relative).name == 'HostGuestSignalsBridge.cs':
            # The derived C probe supplies its own TerminateSignal callback.
            receipt['omitted_product_callback'] = dict(path=relative,sha256=sha(profile/relative))
            continue
        shutil.copyfile(profile / relative, a / 'bridges' / Path(relative).name)
    for name in ('Program.cs', 'abi.c'):
        shutil.copyfile(ROOT / 'tests/CoreExecution' / name, a / name)
    shutil.copytree(ROOT / 'tools/BoundaryAudit', a / 'audit-tool',
                    ignore=shutil.ignore_patterns('bin', 'obj'))
    receipt['consumer_inputs'] = {str(p.relative_to(a)):sha(p) for p in a.rglob('*') if p.is_file()}
    receipt['runner_sha256'] = sha(Path(__file__))
    # Product delivery has no C execution frontend. Reintroduce the normal probe
    # from authored C in this derived test link, never by editing generated C#.
    for name, digest in assembly['generated'].items():
        if sha(generated / name) != digest:
            raise RuntimeError('linked product source changed: ' + name)
    if 'authored/managed-driver.c' not in assembly['objects']:
        cli = ROOT.parent / 'DotCC/bin/Release/net10.0/dotcc.dll'
        if compiler_identity(cli.parent) != inputs['compiler']:
            raise RuntimeError('Compiler differs from the frozen product')
        objects = {}
        for name, row in assembly['objects'].items():
            if sha(Path(row['object_path'])) != row['object_sha256'] or sha(Path(row['canonical_source'])) != row['source_sha256']:
                raise RuntimeError('Canonical producer changed: ' + name)
            objects[name] = Path(row['object_path'])
        receipt['retained_objects'] = {name:dict(path=str(path), sha256=sha(path),
            producer=assembly['objects'][name]['receipt'],
            producer_sha256=sha(Path(assembly['objects'][name]['receipt']))) for name,path in objects.items()}
        frontend = a / 'test-frontend'
        frontend.mkdir()
        for name in ['probe.c', 'managed-driver.c']:
            shutil.copyfile(ROOT/'src/core-probe'/name, frontend/name)
        receipt['test_frontend_inputs'] = {name:sha(frontend/name) for name in ['probe.c','managed-driver.c']}
        row = assembly['objects']['blink/syscall.c']
        command = list(row['command'])
        headers = Path(command[command.index('-I') + 1])
        for name,digest in row['emission_identity']['dependencies'].items():
            if sha(headers/name) != digest:
                raise RuntimeError('Canonical frontend dependency changed: ' + name)
        command[command.index(row['canonical_source'])] = str(frontend/'managed-driver.c')
        test_object = a/'CoreProbe.cs'
        command[command.index('-o')+1] = str(test_object)
        if '--override-report' in command:
            command[command.index('--override-report')+1] = str(out/'core-probe.overrides.jsonl')
        command[3:3] = ['-I',str(frontend)]
        run(command, 'test-frontend-emit', 300)
        receipt['test_frontend_object_sha256'] = sha(test_object)
        receipt['test_frontend_template'] = 'blink/syscall.c'
        receipt['test_frontend_added'] = True
        generated = a/'test-generated'
        run(['dotnet',cli,*objects.values(),test_object,'--emit=managedlib','--literal-pool','--deduplicate-inline','--nest-types',
             '--class-name','BlinkCore','--namespace','Managed.Emulation','--runtime=c','-o',generated],
             'test-frontend-link',300)
        if compiler_identity(cli.parent) != inputs['compiler']:
            raise RuntimeError('Compiler changed during test frontend derivation')
        receipt['derived_test_link'] = True
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

    run(['dotnet', 'build', a / 'audit-tool/BoundaryAudit.csproj', '-c', 'Release'],
        'boundary-audit-build')
    auditor = a / 'audit-tool/bin/Release/net10.0/BoundaryAudit.dll'
    receipt['boundary_auditor_sha256'] = sha(auditor)

    def audit(directory, label):
        report = out / (label + '-boundary-audit.json')
        run(['dotnet', auditor, directory / 'consumer/bin/Release/net10.0/ManagedCore.dll',
             report], label + '-boundary-audit')
        inventory = json.loads(report.read_text())
        if not inventory['complete'] or inventory['traversedNativeImports']:
            raise RuntimeError(label + ' incomplete inventory or direct native import')
        forbidden = ('System.Environment::Void Exit(', 'System.Diagnostics.Process::',
                     'System.Runtime.InteropServices.NativeLibrary::')
        if any(any(term in target for term in forbidden)
               for target in inventory['externalManagedCalls']):
            raise RuntimeError(label + ' direct process/native-loader escape')
        receipt.setdefault('boundary_audits', {})[label] = dict(
            report=str(report), sha256=sha(report),
            scope='direct IL and initializer inventory; indirect/framework dispatch remains unaudited')
        save()

    def check(command, label):
        binary = Path(command[-1] if label.endswith('-jit') else command[0])
        digest = sha(binary)
        managed_inputs = {str(p):sha(p) for p in binary.parent.glob('*.dll')} if label.endswith('-jit') else {}
        actual = run(command, label, 30).splitlines()
        if not actual or actual[0] != expected_abi:
            raise RuntimeError(label + ' profile ABI mismatch')
        if sha(binary) != digest or any(sha(Path(p)) != value for p,value in managed_inputs.items()):
            raise RuntimeError(label + ' execution binary changed')
        receipt['results'][label]['binary_sha256'] = digest
        receipt['results'][label]['managed_dependencies'] = managed_inputs
        footer = re.fullmatch(r'core retained-mappings=(\d+) charged-bytes=(\d+)', actual[-1])
        if not footer or int(footer[2]) > 64 * 1024 * 1024:
            raise RuntimeError(label + ' missing/invalid owned-memory accounting')
        receipt['results'][label]['retained_mappings'] = int(footer[1])
        receipt['results'][label]['charged_bytes'] = int(footer[2])
        if actual[1:-1] != expected_rows:
            (out / (label + '.diff')).write_text('\n'.join(difflib.unified_diff(expected_rows, actual[1:-1], fromfile='native', tofile=label)) + '\n')
            raise RuntimeError(label + ' normal instruction/budget/exit rows differ from native')
        save()

    for label in ('raw', 'optimized'):
        directory = a / label
        consumer = directory / 'consumer/CoreExecution.csproj'
        if label == 'optimized':
            run(['dotnet','restore',directory / 'library/ManagedCore.csproj'], 'optimized-restore')
            run(['dotnet',post,directory / 'library/ManagedCore.csproj','--in-place'], 'postprocess', 600)
        run(['dotnet','build',consumer,'-c','Release'], label + '-build', 600)
        audit(directory, label)
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
    for name,row in receipt.get('retained_objects',{}).items():
        if sha(Path(row['path'])) != row['sha256'] or sha(Path(row['producer'])) != row['producer_sha256']:
            raise RuntimeError('Retained product producer changed: ' + name)
    for name,digest in receipt.get('test_frontend_inputs',{}).items():
        if sha(a/'test-frontend'/name) != digest or sha(ROOT/'src/core-probe'/name) != digest:
            raise RuntimeError('Authored test frontend changed: ' + name)
    if receipt.get('derived_test_link') and sha(a/'CoreProbe.cs') != receipt['test_frontend_object_sha256']:
        raise RuntimeError('Test frontend object changed')
    receipt['runtime_matrix_passed'] = not args.build_only
    receipt['passed'] = not args.build_only and not receipt['diagnostic_replay']
    receipt['build_only'] = args.build_only
    save()
    print('core consumer result: ' + str(out / 'receipt.json'))
finally:
    save()
