#!/usr/bin/env python3
"""Real POSIX, native virtual-mask, and emitted signal-jump comparisons."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
base = ROOT / 'generated/host-signals'
base.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT / 'artifacts/host-signals' / attempt.name
out.mkdir(parents=True, exist_ok=True)
cli = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
postprocess = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
source_dir = attempt / 'source'
source_dir.mkdir()
for original in [ROOT / 'tests/HostSignals/probe.c', ROOT / 'src/HostSignals/HostSignals.c',
                 ROOT / 'src/HostSignals/HostSignals.h', ROOT / 'config/managed-host/abi.h']:
    shutil.copy2(original, source_dir / original.name)
profile = attempt / 'profile'
shutil.copytree(ROOT / 'config/managed-host', profile)
source = source_dir / 'probe.c'
adapter = source_dir / 'HostSignals.c'
hooks = ROOT / 'tests/HostSignals/Hooks.cs'
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
receipt = dict(scope='explicit-virtual-host-delivery-mask-unwind-not-guest-signal-delivery',
               inputs={str(p.relative_to(attempt)):sha(p) for directory in [source_dir, profile]
                       for p in directory.rglob('*') if p.is_file()},
               hookSha256=sha(hooks), compilerSha256=sha(cli.with_name('DotCC.Lib.dll')),
               results={}, passed=False)
receipt['postprocessorSha256'] = sha(postprocess)


def save():
    (out / 'receipt.json').write_text(json.dumps(receipt, indent=2)+'\n')


def run(command, name, timeout=180):
    with (out / (name + '.log')).open('wb') as output:
        result = subprocess.run(command, env=dict(os.environ, LC_ALL='C'),
                                stdout=output, stderr=subprocess.STDOUT, timeout=timeout)
    receipt['results'][name] = dict(command=list(map(str, command)), exitCode=result.returncode)
    save()
    if result.returncode:
        raise RuntimeError(f'{name} failed; see {out / (name + ".log")}')
    return (out / (name + '.log')).read_bytes()


def create_consumer(generated, variant):
    consumer = attempt / (variant + '-consumer')
    consumer.mkdir()
    # Optimization is allowed to change only the attempt's copied test hook.
    local_hooks = consumer / 'Hooks.cs'
    shutil.copy2(hooks, local_hooks)
    project = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(project, 'PropertyGroup')
    for name, value in [('OutputType','Exe'), ('TargetFramework','net10.0'),
                        ('AllowUnsafeBlocks','true'), ('EnableDefaultCompileItems','false'),
                        ('WarningsAsErrors','CS8500'), ('Nullable','disable'),
                        ('AssemblyName','HostSignals')]:
        ET.SubElement(props, name).text = value
    items = ET.SubElement(project, 'ItemGroup')
    ET.SubElement(items, 'Compile', Include=str(generated / '*.cs'))
    ET.SubElement(items, 'Compile', Include=str(local_hooks))
    project_path = consumer / 'HostSignals.csproj'
    ET.ElementTree(project).write(project_path, encoding='unicode')
    return consumer, project_path


try:
    native = attempt / 'native-posix'
    run(['cc', '-std=c17', '-Wall', '-Wextra', '-Werror', '-DBLINK_REAL_POSIX_ORACLE',
         str(source), '-o', str(native)], 'native-posix-build')
    expected = run([str(native)], 'native-posix', 30)
    contract = attempt / 'native-contract'
    run(['cc', '-std=c17', '-Wall', '-Wextra', '-Werror', '-DBLINK_HOST_NATIVE_ORACLE',
         '-iquote', str(source_dir), str(source), str(adapter), '-o', str(contract)], 'native-contract-build')
    if run([str(contract)], 'native-contract', 30) != expected:
        raise RuntimeError('native virtual contract differs from real POSIX semantics')
    generated = attempt / 'emitted'
    run(['dotnet', str(cli), '-std=c17', '-DBLINK_TEST_MANAGED',
         '-I', str(source_dir), '-I', str(profile), str(source), str(adapter),
         '--runtime=c', '-o', str(generated)], 'translate')
    if receipt['compilerSha256'] != sha(cli.with_name('DotCC.Lib.dll')):
        raise RuntimeError('compiler changed during emission')
    receipt['rawGenerated'] = {str(p.relative_to(generated)):sha(p) for p in generated.glob('*.cs')}
    optimized = attempt / 'optimized'
    shutil.copytree(generated, optimized)
    # An authored test consumer includes the unchanged emitted sources and its
    # explicit GC hook. Neither generated C# nor generated project is edited.
    for variant, input_dir in [('raw', generated), ('optimized', optimized)]:
        consumer, project_path = create_consumer(input_dir, variant)
        if variant == 'optimized':
            run(['dotnet', 'restore', str(project_path)], 'optimized-restore')
            run(['dotnet', str(postprocess), str(project_path), '--in-place'], 'postprocess')
            receipt['optimizedGenerated'] = {str(p.relative_to(optimized)):sha(p) for p in optimized.glob('*.cs')}
            receipt['optimizedHookSha256'] = sha(consumer / 'Hooks.cs')
        run(['dotnet', 'build', str(project_path), '-c', 'Release'], variant + '-jit-build')
        actual = run(['dotnet', str(consumer / 'bin/Release/net10.0/HostSignals.dll')], variant + '-jit', 30)
        if actual != expected:
            raise RuntimeError(f'forced-GC {variant} JIT output differs from native')
        publish = attempt / (variant + '-aot')
        run(['dotnet', 'publish', str(project_path), '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-o', str(publish)], variant + '-aot-build', 300)
        actual = run([str(publish / 'HostSignals')], variant + '-aot', 30)
        if actual != expected:
            raise RuntimeError(f'forced-GC {variant} NativeAOT output differs from native')
    if receipt['rawGenerated'] != {str(p.relative_to(generated)):sha(p) for p in generated.glob('*.cs')}:
        raise RuntimeError('raw generated snapshot changed during qualification')
    receipt['passed'] = True
    receipt['stdoutSha256'] = hashlib.sha256(expected).hexdigest()
    save()
    print(f'Virtual signal jumps match real POSIX and native contract under raw/optimized JIT/NativeAOT; host mask unchanged and CS8500 forbidden. Receipt: {out / "receipt.json"}')
except Exception as error:
    receipt['error'] = str(error)
    save()
    raise
