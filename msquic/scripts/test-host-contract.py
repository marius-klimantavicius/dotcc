#!/usr/bin/env python3
"""Validate the explicit managed host binding and ABI against the same native headers."""
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
build = ROOT / 'build/host-contract'
logs = ROOT / 'artifacts/host-contract'
build.mkdir(parents=True, exist_ok=True)
logs.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, runtime_services_implemented=False, commands=[], cases=[])
(logs / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')


def run(command, name):
    receipt['commands'].append(dict(name=name, arguments=command))
    result = subprocess.run(command, capture_output=True, text=True, timeout=600)
    (logs / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(f'{name}: exit {result.returncode}; see {logs / (name + ".log")}')
    return result.stdout


try:
    run([sys.executable, str(ROOT / 'scripts/generate-host-contract.py')], 'generate')
    run([sys.executable, str(ROOT / 'scripts/stage-product.py')], 'stage')
    stage = ROOT / 'build/product-source'
    manifest = json.loads((stage / 'manifest.json').read_text())
    receipt['stage_manifest_sha256'] = hashlib.sha256((stage / 'manifest.json').read_bytes()).hexdigest()
    compiler = build / 'compiler'
    # This directory is an owned tool snapshot. Replacing it prevents stale
    # dependencies from surviving a local/package compiler dependency switch.
    if compiler.exists():
        shutil.rmtree(compiler)
    shutil.copytree(REPO / 'DotCC/bin/Release/net10.0', compiler)
    receipt['compiler_hashes'] = {str(p.relative_to(compiler)): hashlib.sha256(p.read_bytes()).hexdigest()
                                 for p in sorted(compiler.rglob('*')) if p.is_file()}
    flags = ['-D' + value for value in manifest['defines']]
    macro_exports = ['QUIC_STATUS_*']
    export_flags = [part for pattern in macro_exports for part in ['--emit-define', pattern]]
    receipt['macro_exports'] = macro_exports
    includes = ['-I' + str(stage / path) for path in ['system', 'src/inc', 'src/core', 'src/platform', 'host']]
    includes.extend(['-I' + str(ROOT / 'tests/Abi'), '-I' + str(ROOT / 'tests/HostContract')])
    for case, source in [('binding', ROOT / 'tests/HostContract/binding.c'),
                         ('tls', ROOT / 'tests/Abi/tls.c'), ('core', ROOT / 'tests/Abi/core.c')]:
        native = build / (case + '-native')
        run(['gcc', '-std=gnu17', '-fms-extensions', '-O2', '-Wno-multichar',
             '-ffunction-sections', '-fdata-sections', '-Wl,--gc-sections', *flags, *includes,
             str(source), '-o', str(native)], case + '-native-build')
        expected = run([str(native)], case + '-native')
        project = build / case
        inputs = [str(source), str(stage / 'src/platform/hashtable.c'), str(stage / 'src/platform/toeplitz.c')]
        if case == 'core':
            inputs = [str(source)] + [str(stage / unit) for unit in manifest['units']]
        elif case != 'binding':
            inputs.extend(str(stage / 'host' / name) for name in ['host.c', 'forwarders.c', 'portable.c'])
        if case == 'core':
            # Independent TUs bound monolithically are expensive and retain all
            # parser state. Use the product object-link path with provenance.
            objects = build / 'core-objects'
            objects.mkdir(exist_ok=True)
            paths = []
            for index, input_path in enumerate(inputs):
                output = objects / f'{index:02d}-{Path(input_path).stem}.cs'
                run(['dotnet', str(compiler / 'dotcc.dll'), '--emit=obj', *export_flags, *flags, *includes,
                     input_path, '-o', str(output)], f'core-object-{index:02d}')
                object_receipt = dict(source=input_path,
                    source_sha256=hashlib.sha256(Path(input_path).read_bytes()).hexdigest(),
                    object=str(output), object_sha256=hashlib.sha256(output.read_bytes()).hexdigest(),
                    compiler_hashes=receipt['compiler_hashes'], flags=flags, includes=includes, macro_exports=macro_exports,
                    stage_manifest_sha256=receipt['stage_manifest_sha256'])
                (objects / (output.stem + '.json')).write_text(json.dumps(object_receipt, indent=2) + '\n')
                paths.append(str(output))
                print(f'core object {index + 1}/{len(inputs)}: emitted', flush=True)
            run(['dotnet', str(compiler / 'dotcc.dll'), '--emit=csproj', '--split=size',
                 *paths, '-o', str(project)], case + '-link')
        else:
            run(['dotnet', str(compiler / 'dotcc.dll'), '--emit=csproj', *export_flags, *flags, *includes,
                 *inputs, '-o', str(project)], case + '-emit')
        csproj = project / (case + '.csproj')
        run(['dotnet', 'build', str(csproj), '-c', 'Release'], case + '-jit-build')
        jit = run(['dotnet', str(project / 'bin/Release/net10.0' / (case + '.dll'))], case + '-jit')
        if jit != expected:
            raise RuntimeError(case + ': JIT/native mismatch')
        run(['dotnet', 'publish', str(csproj), '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-o', str(project / 'aot')], case + '-aot-build')
        aot = run([str(project / 'aot' / case)], case + '-aot')
        if aot != expected:
            raise RuntimeError(case + ': AOT/native mismatch')
        receipt['cases'].append(dict(name=case, passed=True, native_records=len(expected.splitlines()),
            source_sha256=hashlib.sha256(source.read_bytes()).hexdigest(),
            output_sha256=hashlib.sha256(expected.encode()).hexdigest()))
        print(case + ': native/JIT/NativeAOT PASS', flush=True)
    receipt['passed'] = len(receipt['cases']) == 3
finally:
    (logs / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({'passed': receipt['passed'], 'cases': receipt['cases']}))
