#!/usr/bin/env python3
"""Build raw/optimized selected managed core from provenance-checked host ABI objects.

No host services are installed. The AOT consumer roots the complete generated
assembly, and only exercises an invalid-table rejection and unbound cleanup.
"""
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
STAGE = ROOT / 'build/product-source'
HOST = ROOT / 'build/host-contract'
LOGS = ROOT / 'artifacts/product-build'
LOGS.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, runtime_services_implemented=False, transport_validated=False,
               commands=[], variants=[])


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def run(command, name, timeout=600):
    receipt['commands'].append(dict(name=name, arguments=[str(p) for p in command]))
    result = subprocess.run(command, capture_output=True, text=True, timeout=timeout)
    (LOGS / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(f'{name} failed ({result.returncode}); see {LOGS / (name + ".log")}')
    return result.stdout


def generated_files(directory):
    names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    if not names or len(set(names)) != len(names):
        raise RuntimeError('Invalid source manifest: ' + str(directory))
    if any(not re.fullmatch(r'[A-Za-z_][A-Za-z_0-9.]*\.cs', n) or '..' in n for n in names):
        raise RuntimeError('Unsafe generated source manifest: ' + str(directory))
    return names


def hashes(directory):
    return {name: sha(directory / name) for name in generated_files(directory)
            + ['Dotcc.SourceFiles.txt', 'TranslatedMsQuic.csproj']}


try:
    host_result = json.loads((ROOT / 'artifacts/host-contract/results.json').read_text())
    if not host_result['passed']:
        raise RuntimeError('The complete host ABI gate must pass before reusing its product objects')
    manifest_path = STAGE / 'manifest.json'
    stage_hash = sha(manifest_path)
    if stage_hash != host_result['stage_manifest_sha256']:
        raise RuntimeError('Staged inputs changed after the host ABI run')
    manifest = json.loads(manifest_path.read_text())
    for item in manifest['files']:
        if sha(STAGE / item['path']) != item['sha256']:
            raise RuntimeError('Changed staged source: ' + item['path'])
    compiler = HOST / 'compiler/dotcc.dll'
    for name, digest in host_result['compiler_hashes'].items():
        if sha(compiler.parent / name) != digest:
            raise RuntimeError('Host ABI compiler changed: ' + name)
    receipt['stage_manifest_sha256'] = stage_hash
    receipt['compiler_hashes'] = host_result['compiler_hashes']
    if host_result.get('macro_exports') != ['QUIC_STATUS_*']:
        raise RuntimeError('Rebuild host ABI objects with QUIC_STATUS_* macro exports')
    receipt['macro_exports'] = host_result['macro_exports']
    profile_hash = sha(ROOT / 'config/dotcc-overrides.json')
    if host_result.get('translation_profile_sha256') != profile_hash:
        raise RuntimeError('Rebuild host ABI objects with the current translation profile')
    receipt['translation_profile_sha256'] = profile_hash
    receipt['objects'] = []
    objects = []
    commands = {record['name']: record['arguments'] for record in host_result['commands']}
    for index, unit in enumerate(manifest['units'], 1):
        obj = HOST / 'core-objects' / f'{index:02d}-{Path(unit).stem}.cs'
        command = commands.get(f'core-object-{index:02d}')
        if command is None or str(STAGE / unit) not in command or str(obj) not in command:
            raise RuntimeError('Missing matching ABI object command: ' + unit)
        cached = json.loads(obj.with_suffix('.json').read_text())
        if (cached['source'] != str(STAGE / unit) or cached['source_sha256'] != sha(STAGE / unit)
                or cached['object'] != str(obj) or cached['object_sha256'] != sha(obj)
                or cached['stage_manifest_sha256'] != stage_hash
                or cached['compiler_hashes'] != host_result['compiler_hashes']
                or cached.get('macro_exports') != host_result['macro_exports']
                or cached.get('translation_profile_sha256') != profile_hash):
            raise RuntimeError('Stale or changed object cache: ' + unit)
        objects.append(obj)
        receipt['objects'].append(dict(source=unit, source_sha256=sha(STAGE / unit),
            object=str(obj.relative_to(ROOT)), object_sha256=sha(obj), arguments=command,
            abi_object_receipt_sha256=sha(obj.with_suffix('.json'))))
    # Only tool binaries and their runtime dependencies are copied; shared builds
    # cannot change the postprocessor while a complete core pass is in progress.
    post_source = REPO / 'DotCC.PostProcess/bin/Release/net10.0'
    contents = {p.name: p.read_bytes() for p in post_source.iterdir() if p.suffix in ('.dll', '.json')}
    post_hashes = {name: hashlib.sha256(data).hexdigest() for name, data in contents.items()}
    post_id = hashlib.sha256(json.dumps(post_hashes, sort_keys=True).encode()).hexdigest()
    post = ROOT / 'artifacts/toolchains' / post_id
    post.mkdir(parents=True, exist_ok=True)
    for name, data in contents.items():
        (post / name).write_bytes(data)
    if any(sha(post_source / name) != digest for name, digest in post_hashes.items()):
        raise RuntimeError('Postprocessor changed during snapshot')
    receipt['postprocessor_hashes'] = post_hashes
    inline_exports = [line.strip() for line in (ROOT / 'config/inline-exports.txt').read_text().splitlines()
                      if line.strip() and not line.lstrip().startswith('#')]
    if not inline_exports or len(set(inline_exports)) != len(inline_exports):
        raise RuntimeError('Inline export selectors must be nonempty and unique')
    inline_flags = ['--deduplicate-inline', *[part for pattern in inline_exports for part in ['--export-inline', pattern]]]
    receipt['output_options'] = dict(nest_types=True, runtime='c', deduplicate_inline=True, export_inline=inline_exports)
    receipt['generated_directories'] = dict(raw='generated/raw/TranslatedMsQuic', optimized='generated/TranslatedMsQuic')
    raw = ROOT / 'generated/raw/TranslatedMsQuic'
    optimized = ROOT / 'generated/TranslatedMsQuic'
    run(['dotnet', compiler, '--emit=managedlib', '--nest-types', '--runtime=c', '--class-name', 'MsQuic',
         '--namespace', 'Managed.Transport', '--split=size', *inline_flags, *objects, '-o', raw], 'raw-link')
    optimized.mkdir(parents=True, exist_ok=True)
    # Cleanup is limited to previously generated manifest-owned files.
    if (optimized / 'Dotcc.SourceFiles.txt').exists():
        for name in set(generated_files(optimized)) - set(generated_files(raw)):
            (optimized / name).unlink()
    for name in generated_files(raw) + ['Dotcc.SourceFiles.txt', 'TranslatedMsQuic.csproj']:
        shutil.copyfile(raw / name, optimized / name)
    run(['dotnet', 'restore', optimized / 'TranslatedMsQuic.csproj', '--nologo'], 'optimized-restore')
    run(['dotnet', post / 'dotcc-postprocess.dll', optimized / 'TranslatedMsQuic.csproj', '--in-place'], 'optimize')
    for variant, directory in [('raw', raw), ('optimized', optimized)]:
        project = directory / 'TranslatedMsQuic.csproj'
        sources = hashes(directory)
        runtime_findings = []
        for name in generated_files(directory):
            application, marker, runtime = (directory / name).read_text().partition(
                '// ---- Embedded DotCC.Libc runtime — single source of truth.')
            if re.search(r'\b(?:DllImport|LibraryImport|NativeLibrary|dlopen|dlsym)\b', application):
                raise RuntimeError('Native import surface in translated application: ' + name)
            if marker:
                runtime_findings.extend(dict(file=name, symbol=symbol)
                    for symbol in sorted(set(re.findall(r'\b(?:DllImport|LibraryImport|NativeLibrary|dlopen|dlsym)\b', runtime))))
        receipt.setdefault('embedded_runtime_inventory', {})[variant] = runtime_findings
        receipt['native_audit_limit'] = ('Pre-existing embedded Libc loader facilities are inventoried; '
            'no translated application native binding is permitted. This is not an IL reachability proof.')
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo'], variant + '-build')
        consumer = ROOT / 'build/product-consumer' / variant
        consumer.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(ROOT / 'tests/ProductBuild/Program.cs', consumer / 'Program.cs')
        from xml.sax.saxutils import escape
        (consumer / 'ProductBuild.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup>
  <ItemGroup><ProjectReference Include="''' + escape(str(project)) + '''"/><TrimmerRootAssembly Include="TranslatedMsQuic"/></ItemGroup>
</Project>\n''')
        run(['dotnet', 'build', consumer / 'ProductBuild.csproj', '-c', 'Release', '--nologo'], variant + '-consumer-build')
        jit = run(['dotnet', consumer / 'bin/Release/net10.0/ProductBuild.dll'], variant + '-jit')
        if jit.strip() != 'jit: boundary rejection passed':
            raise RuntimeError('Incorrect JIT boundary receipt')
        run(['dotnet', 'publish', consumer / 'ProductBuild.csproj', '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-o', consumer / 'aot', '--nologo'], variant + '-aot-build')
        aot = run([consumer / 'aot/ProductBuild'], variant + '-aot')
        if aot.strip() != 'nativeaot: boundary rejection passed':
            raise RuntimeError('Incorrect NativeAOT boundary receipt')
        receipt['variants'].append(dict(name=variant, passed=True, generated_sha256=sources,
            complete_assembly_rooted_for_aot=True, jit=jit.strip(), aot=aot.strip()))
        print(variant + ': generated library and rooted JIT/AOT consumer PASS', flush=True)
    if sha(manifest_path) != stage_hash or any(sha(STAGE / item['path']) != item['sha256'] for item in manifest['files']):
        raise RuntimeError('Staged inputs changed during product build')
    receipt['passed'] = True
finally:
    (LOGS / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
