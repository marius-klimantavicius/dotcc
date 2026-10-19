#!/usr/bin/env python3
"""Validate the explicit managed host binding and ABI against the same native headers."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sys as _production_sys
from pathlib import Path as _ProductionPath
_production_sys.path.insert(0, str(next(parent for parent in _ProductionPath(__file__).resolve().parents
    if (parent / "Scripts/production_support.py").is_file()) / "Scripts"))
from production_support import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--no-fetch', action='store_true', help='Use the selected local source tree without fetching')
args = parser.parse_args()
from production_support import selected_tool, snapshot_tool
selected_compiler = selected_tool()
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
    run([sys.executable, str(ROOT / 'scripts/stage-product.py'),
         *(['--no-fetch'] if args.no_fetch else [])], 'stage')
    stage = ROOT / 'build/product-source'
    manifest = json.loads((stage / 'manifest.json').read_text())
    receipt['stage_manifest_sha256'] = hashlib.sha256((stage / 'manifest.json').read_bytes()).hexdigest()
    compiler = build / 'compiler'
    compiler_tool, receipt['compiler_tool'] = snapshot_tool(selected_compiler, compiler)
    receipt['compiler_hashes'] = receipt['compiler_tool']['files']
    effective_defines = [*manifest['defines'], 'QUIC_API_ENABLE_PREVIEW_FEATURES=1']
    flags = ['-D' + value for value in effective_defines]
    receipt['host_feature_policy'] = dict(defines=effective_defines,
        header_sha256={name: hashlib.sha256((stage / name).read_bytes()).hexdigest() for name in
                       ('src/platform/platform_internal.h', 'src/inc/msquic.h', 'src/inc/quic_tls.h',
                        'host/msquic_host.h', 'host/msquic_host_internal.h')},
        include_order=dict(tls='quic_platform.h -> quic_tls.h -> msquic.h',
                           platform='platform_internal.h: preview=1 before quic_platform.h/msquic.h',
                           host='msquic_host_internal.h -> msquic_host.h -> platform_internal.h'))
    macro_exports = ['QUIC_STATUS_*']
    export_flags = [part for pattern in macro_exports for part in ['--emit-define', pattern]]
    receipt['macro_exports'] = macro_exports
    profile = ROOT / 'config/dotcc-overrides.json'
    receipt['translation_profile_sha256'] = hashlib.sha256(profile.read_bytes()).hexdigest()
    (logs / 'dotcc-overrides.json').write_bytes(profile.read_bytes())
    probe_options = json.loads(profile.read_text())
    if set(probe_options.get('stableFunctionPointers', [])) != {'MsQuicOpenVersion', 'MsQuicClose'} or len(probe_options['stableFunctionPointers']) != 2:
        raise RuntimeError('Unexpected product stable function pointer policy')
    del probe_options['stableFunctionPointers']
    probe_profile = build / 'header-probe-overrides.json'
    probe_profile.write_text(json.dumps(probe_options, indent=2) + '\n')
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
        selected_profile = profile if case == 'core' else probe_profile
        profile_flags = ['--overrides-file', str(selected_profile)]
        receipt.setdefault('translation_profiles', {})[case] = dict(
            role='host-' + case, source_profile=str(profile),
            source_sha256=receipt['translation_profile_sha256'], effective_profile=str(selected_profile),
            effective_sha256=hashlib.sha256(selected_profile.read_bytes()).hexdigest(),
            translation_units={path: hashlib.sha256(Path(path).read_bytes()).hexdigest() for path in inputs})
        if case == 'core':
            # Independent TUs bound monolithically are expensive and retain all
            # parser state. Use the product object-link path with provenance.
            objects = build / 'core-objects'
            objects.mkdir(exist_ok=True)
            paths = []
            for index, input_path in enumerate(inputs):
                output = objects / f'{index:02d}-{Path(input_path).stem}.cs'
                run(['dotnet', str(compiler_tool), '--emit=obj', *export_flags, *profile_flags, *flags, *includes,
                     input_path, '-o', str(output)], f'core-object-{index:02d}')
                object_receipt = dict(source=input_path,
                    source_sha256=hashlib.sha256(Path(input_path).read_bytes()).hexdigest(),
                    object=str(output), object_sha256=hashlib.sha256(output.read_bytes()).hexdigest(),
                    compiler_hashes=receipt['compiler_hashes'], flags=flags, includes=includes, macro_exports=macro_exports,
                    stage_manifest_sha256=receipt['stage_manifest_sha256'],
                    translation_profile_sha256=receipt['translation_profile_sha256'])
                (objects / (output.stem + '.json')).write_text(json.dumps(object_receipt, indent=2) + '\n')
                paths.append(str(output))
                print(f'core object {index + 1}/{len(inputs)}: emitted', flush=True)
            run(['dotnet', str(compiler_tool), '--emit=csproj', '--split=size',
                 *paths, '-o', str(project)], case + '-link')
        else:
            run(['dotnet', str(compiler_tool), '--emit=csproj', *export_flags, *profile_flags, *flags, *includes,
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
    if hashlib.sha256(profile.read_bytes()).hexdigest() != receipt['translation_profile_sha256']:
        raise RuntimeError('Product translation profile changed during host probes')
    receipt['passed'] = len(receipt['cases']) == 3
finally:
    (logs / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({'passed': receipt['passed'], 'cases': receipt['cases']}))
