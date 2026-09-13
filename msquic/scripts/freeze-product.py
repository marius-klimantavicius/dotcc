#!/usr/bin/env python3
"""Archive or freeze the translated closure using completed, matching evidence."""
import argparse
import hashlib
import json
import os
import re
from pathlib import Path
import shutil

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
CLOSURE = ROOT / 'config/product-closure.json'
parser = argparse.ArgumentParser(description=__doc__)
mode = parser.add_mutually_exclusive_group(required=True)
mode.add_argument('--archive-current', action='store_true')
mode.add_argument('--sqlite-receipt', type=Path)
mode.add_argument('--without-sqlite', action='store_true',
                  help='Freeze the MsQuic ABI/build gates without claiming a fresh shared SQLite campaign')
args = parser.parse_args()


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def check(condition, message):
    if not condition:
        raise RuntimeError(message)


def hashes(directory, names):
    return {name: sha(directory / name) for name in names}


def read(path):
    return json.loads(path.read_text())


def verified(mapping, base, label):
    for name, expected in mapping.items():
        path = base / name
        check(path.is_file() and sha(path) == expected, label + ': changed/missing ' + str(path))


def verified_archive(directory, closure_hash):
    check((directory / 'archive.json').is_file(), 'Archive the previous closure before regenerating')
    saved = read(directory / 'archive.json')
    check(saved['closure_sha256'] == closure_hash, 'Existing archive has another closure')
    verified(saved['files'], directory, 'Archived checkpoint')
    check(sha(directory / 'msquic/config/product-closure.json') == closure_hash,
          'Archived closure does not match its identity')
    return saved


previous = read(CLOSURE)
old_hash = sha(CLOSURE)
archive = ROOT / 'artifacts' / ('closure-' + old_hash[:16])
if args.archive_current:
    if (archive / 'archive.json').is_file():
        # Existing historical archives are immutable, including archives made
        # with the earlier manifest schema. Do not relabel or replace their files.
        verified_archive(archive, old_hash)
        print(archive)
        raise SystemExit(0)
    stage_path = ROOT / 'build/product-source/manifest.json'
    check(sha(stage_path) == previous['stage_manifest_sha256'], 'Previous staging manifest changed before archive')
    verified(previous['evidence_sha256'], ROOT, 'Previous closure-bound evidence')
    files = [CLOSURE, stage_path]
    closure_bound = {CLOSURE.resolve(): old_hash, stage_path.resolve(): previous['stage_manifest_sha256']}
    compiler = ROOT / 'build/host-contract/compiler'
    verified(previous['compiler_hashes'], compiler, 'Previous frozen compiler')
    files.extend(compiler / name for name in previous['compiler_hashes'])
    closure_bound.update({(compiler / name).resolve(): digest for name, digest in previous['compiler_hashes'].items()})
    for variant in ['raw', 'optimized']:
        directory = ROOT / previous.get('generated_directories', {}).get(variant, 'generated/' + variant + '/TranslatedMsQuic')
        verified(previous['generated'][variant], directory, 'Previous generated closure')
        files.extend(directory / name for name in previous['generated'][variant])
        closure_bound.update({(directory / name).resolve(): digest for name, digest in previous['generated'][variant].items()})
    for name in previous['evidence_sha256']:
        files.append(ROOT / name)
    closure_bound.update({(ROOT / name).resolve(): digest for name, digest in previous['evidence_sha256'].items()})
    # Keep the last executed service/transport/facade receipts as historical
    # observations too, even if a newer authored input has superseded them.
    files.extend((ROOT / 'artifacts').glob('*/results.json'))
    files.extend((ROOT / 'build/host-contract/core-objects').glob('*.json'))
    recorded, historical = {}, {}
    # Check the entire plan before copying, and never replace even an incomplete
    # archive's files with different contents on a retry.
    planned = []
    for source in sorted(set(files)):
        if source == archive / 'archive.json': continue
        relative = source.resolve().relative_to(REPO)
        destination = archive / relative
        expected = closure_bound.get(source.resolve(), sha(source))
        check(sha(source) == expected, 'Closure-bound input changed before archive: ' + str(source))
        check(not destination.exists() or (destination.is_file() and sha(destination) == expected),
              'Refusing to overwrite archived file: ' + str(destination))
        planned.append((source, relative, destination, expected))
    for source, relative, destination, expected in planned:
        destination.parent.mkdir(parents=True, exist_ok=True)
        if not destination.exists():
            with source.open('rb') as incoming, destination.open('xb') as outgoing:
                shutil.copyfileobj(incoming, outgoing)
        check(sha(destination) == expected and sha(source) == expected, 'Input changed while archiving: ' + str(source))
        recorded[str(relative)] = expected
        if source.resolve() not in closure_bound:
            historical[str(relative)] = dict(sha256=expected,
                scope='Historical observation only; not closure-bound qualification evidence')
    with (archive / 'archive.json').open('x') as output:
        output.write(json.dumps(dict(closure_sha256=old_hash, files=recorded,
            closure_bound_files={str(path.relative_to(REPO)): digest for path, digest in sorted(closure_bound.items())},
            historical_observations=historical), indent=2) + '\n')
    print(archive)
    raise SystemExit(0)

if args.sqlite_receipt is not None:
    sqlite_path = args.sqlite_receipt.resolve()
    sqlite = read(sqlite_path)
    check(sqlite.get('passed') is True, 'Final SQLite/compiler campaign did not pass')
    prefix = sqlite.get('reused_sqlite_prefix')
    expected_stages = ['repaired-port-regressions' if prefix else 'sqlite-campaign-with-ports',
                       'fresh-raw-engine', 'postprocess-tool-build', 'raw-engine-restore',
                       'raw-optimized-snapshot', 'raw-optimized-jit-aot-corpora']
    check([row['label'] for row in sqlite['commands']] == expected_stages and
          all(row.get('completed') and row.get('exit_code') == 0 for row in sqlite['commands']), 'Incomplete SQLite/compiler stages')
    verified(sqlite['source_hashes'], REPO, 'Final SQLite sources')
    verified(sqlite['compiler_after'], REPO, 'Final SQLite compiler')
    identity = sqlite['compiler_after']
    check(identity and sqlite.get('compiler_post_build') == identity, 'Missing matching post-build SQLite compiler identity')
    for index, row in enumerate(sqlite['commands']):
        check(row.get('compiler_after') == identity and
              (row.get('compiler_before') == identity or (index == 0 and not prefix)),
              'Compiler changed across a SQLite translation/test stage: ' + row['label'])
    if prefix:
        original_path = Path(prefix['receipt'])
        check(sha(original_path) == prefix['receipt_sha256'], 'Original failed SQLite receipt changed')
        original = read(original_path)
        check(original.get('passed') is False and original['compiler_after'] == identity and
              len(original['commands']) == 1 and original['commands'][0]['label'] == 'sqlite-campaign-with-ports' and
              original['commands'][0].get('completed') and original['commands'][0].get('exit_code') == 1 and
              prefix['original_command_exit_code'] == 1, 'Composite prefix is not the preserved port-only campaign failure')
        repair = prefix['repaired_script']
        check(repair['path'] == 'sqlite/scripts/test-ports.sh' and
              original['source_hashes'][repair['path']] == repair['before_sha256'] and
              sqlite['source_hashes'][repair['path']] == repair['after_sha256'] and
              repair['before_sha256'] != repair['after_sha256'], 'Composite script repair differs from recorded inputs')
        check({k: v for k, v in original['source_hashes'].items() if k != repair['path']} ==
              {k: v for k, v in sqlite['source_hashes'].items() if k != repair['path']},
              'Other SQLite inputs changed after the reused prefix')
        verified(prefix['log_sha256'], REPO, 'Completed SQLite prefix logs')
        required_markers = ['PASS native ' + name for name in
                            ['core', 'api', 'vfs', 'vtable', 'allocation', 'upstream', 'fts5', 'layout']]
        required_markers += ['PASS translated ' + name for name in
                             ['core', 'api', 'vfs', 'vtable', 'allocation', 'upstream', 'fts5']]
        original_log = Path(original['commands'][0]['log'])
        check(str(original_log.relative_to(REPO)) in prefix['log_sha256'] and
              prefix['completed_markers'] == required_markers and
              all(marker in original_log.read_text() for marker in required_markers) and
              'sqlite/artifacts/campaign-image-exchange.log' in prefix['log_sha256'],
              'Reused SQLite prefix lacks completed native/translated/image-exchange evidence')
        failure = prefix['original_port_failure']
        check(sha(Path(failure['path'])) == failure['sha256'], 'Original Lua failure log changed')
    snapshot = Path(sqlite['snapshot'])
    check(snapshot.is_dir() and sha(snapshot / 'manifest.json') == sqlite['snapshot_manifest_sha256'],
          'Final SQLite snapshot manifest changed')
    snapshot_files = sqlite['snapshot_file_sha256']
    check(snapshot_files, 'Missing final SQLite snapshot file identities')
    verified(snapshot_files, REPO, 'Final SQLite snapshot files')
    actual_snapshot = {str(path.relative_to(REPO)): sha(path) for path in snapshot.rglob('*')
                       if path.is_file() and not {'bin', 'obj'}.intersection(path.relative_to(snapshot).parts)
                       and (path.suffix in {'.cs', '.csproj'} or path.name == 'manifest.json')}
    check(snapshot_files == actual_snapshot, 'Final SQLite snapshot file set differs from its evidence')
    normal_path = sqlite_path.parent / 'normal-optimized-product/results.json'
    normal = read(normal_path)
    check(normal.get('passed') and normal.get('exit_code') == 0 and
          normal.get('matches_qualified_optimized_snapshot') is True and
          normal['snapshot_receipt_sha256'] == sha(sqlite_path) and normal['compiler_sha256'] == identity,
          'Normal optimized SQLite product is not qualified against this snapshot/compiler')
    verified(normal['product_source_sha256'], REPO, 'Normal optimized SQLite emitted sources')
    verified(normal['linked_host_source_sha256'], REPO, 'Normal SQLite linked host sources')
    normal_directory = REPO / 'sqlite/generated/TranslatedSqlite'
    selected = (normal_directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    check(selected and len(selected) == len(set(selected)) and
          {str((normal_directory / name).relative_to(REPO)) for name in selected} == set(normal['product_source_sha256']),
          'Normal SQLite generated source manifest differs from qualified product')
    check(sha(normal_directory / 'TranslatedSqlite.csproj') == normal['product_project_sha256'] and
          sha(normal_path.parent / 'consumer.log') == normal['log_sha256'], 'Normal SQLite project/consumer evidence changed')
    verified(normal['restoration_optimizer_sha256'], normal_path.parent / 'restoration-optimizer', 'Archived restoration optimizer')
    verified(normal['restored_frozen_optimizer_sha256'], REPO / 'DotCC.PostProcess/bin/Release/net10.0', 'Frozen postprocessor restoration')
compiler = ROOT / 'build/host-contract/compiler'
host = read(ROOT / 'artifacts/host-contract/results.json')
product = read(ROOT / 'artifacts/product-build/results.json')
abi = read(ROOT / 'artifacts/abi/results.json')
for name, receipt in [('host ABI', host), ('product', product), ('public ABI', abi)]:
    check(receipt.get('passed') is True, name + ' gate did not pass')
check(product.get('output_options') == dict(nest_types=True, runtime='c'),
      'Product was not generated with the selected nested C layout')
check(product.get('generated_directories') == dict(raw='generated/raw/TranslatedMsQuic',
                                                 optimized='generated/TranslatedMsQuic'),
      'Product output locations differ from the selected layout')
verified(host['compiler_hashes'], compiler, 'Frozen emitter')
check(product['compiler_hashes'] == host['compiler_hashes'], 'Product and host compiler differ')
if args.sqlite_receipt is not None:
    for name, expected in sqlite['compiler_after'].items():
        relative = Path(name).relative_to('DotCC/bin/Release/net10.0')
        check(host['compiler_hashes'].get(str(relative)) == expected, 'Frozen emitter differs from SQLite compiler: ' + name)
check(abi.get('compiler_stable') and abi['compiler_sha256'] == abi['compiler_sha256_after'], 'Public ABI compiler changed')
for name, expected in abi['compiler_sha256'].items():
    check(host['compiler_hashes'].get(Path(name).name) == expected, 'Public ABI used another compiler')
verified(abi['source_sha256'], ROOT, 'Public ABI inputs')
public = next((row for row in abi['groups'] if row['name'] == 'public'), None)
check(public and public.get('passed') and set(public['runtimes']) == {'jit', 'aot'} and
      all(row.get('passed') for row in public['runtimes'].values()), 'Public ABI lacks both runtimes')
check({row['name'] for row in host['cases']} == {'binding', 'tls', 'core'} and
      len(host['cases']) == 3 and all(row['passed'] for row in host['cases']), 'Incomplete host ABI cases')
host_sources = {'binding': ROOT / 'tests/HostContract/binding.c',
                'tls': ROOT / 'tests/Abi/tls.c', 'core': ROOT / 'tests/Abi/core.c'}
for case in host['cases']:
    source = host_sources[case['name']]
    check(source.is_file() and sha(source) == case['source_sha256'], 'Host ABI probe changed: ' + str(source))
stage_path = ROOT / 'build/product-source/manifest.json'
stage, pin = read(stage_path), read(ROOT / 'config/source.json')
check(sha(stage_path) == host['stage_manifest_sha256'] == product['stage_manifest_sha256'], 'Staged inputs differ between gates')
check(stage['revision'] == pin['commit'] and 'VER_GIT_HASH=' + pin['commit'] in stage['defines'], 'Missing pinned revision metadata')
check(stage['source_archive_sha256'] == pin['sha256'] and sha(ROOT / 'ref' / pin['archive']) == pin['sha256'],
      'Pinned source archive differs from staging')
verified({item['path']: item['sha256'] for item in stage['files']}, stage_path.parent, 'Staged source')
for item in stage['files']:
    origin = (ROOT / 'ref' / pin['directory'] / item['origin'] if item['role'] == 'unchanged upstream'
              else ROOT / item['origin'])
    check(origin.is_file() and sha(origin) == item['sha256'], 'Authored/upstream source changed: ' + str(origin))
check([row['source'] for row in product['objects']] == stage['units'], 'Product object list differs from staged unit closure')
host_commands = {row['name']: row['arguments'] for row in host['commands']}
for index, record in enumerate(product['objects'], 1):
    source = stage_path.parent / record['source']
    obj = ROOT / 'build/host-contract/core-objects' / f'{index:02d}-{source.stem}.cs'
    sidecar = obj.with_suffix('.json')
    check(record['object'] == str(obj.relative_to(ROOT)) and obj.is_file() and sha(obj) == record['object_sha256'],
          'Product object changed/missing: ' + str(obj))
    check(sidecar.is_file() and sha(sidecar) == record['abi_object_receipt_sha256'],
          'ABI object receipt changed/missing: ' + str(sidecar))
    cached = read(sidecar)
    check(record['source_sha256'] == sha(source) == cached['source_sha256'] and cached['source'] == str(source)
          and cached['object'] == str(obj) and cached['object_sha256'] == record['object_sha256']
          and cached['stage_manifest_sha256'] == sha(stage_path) and cached['compiler_hashes'] == host['compiler_hashes'],
          'Object provenance differs from validated source/compiler: ' + record['source'])
    check(cached['flags'] == ['-D' + value for value in stage['defines']], 'Object defines differ from staging')
    expected_includes = ['-I' + str(stage_path.parent / path) for path in ['system', 'src/inc', 'src/core', 'src/platform', 'host']]
    expected_includes.extend(['-I' + str(ROOT / 'tests/Abi'), '-I' + str(ROOT / 'tests/HostContract')])
    check(cached['includes'] == expected_includes, 'Object include roots differ from the host ABI campaign')
    check(cached.get('macro_exports') == host.get('macro_exports') == product.get('macro_exports') == ['QUIC_STATUS_*'],
          'Object macro export selection differs from the product')
    expected_command = ['dotnet', str(compiler / 'dotcc.dll'), '--emit=obj', '--emit-define', 'QUIC_STATUS_*', *cached['flags'], *cached['includes'],
                        str(source), '-o', str(obj)]
    check(record['arguments'] == host_commands.get(f'core-object-{index:02d}') == expected_command,
          'Product/ABI object emission command differs: ' + record['source'])
check({v['name'] for v in product['variants']} == {'raw', 'optimized'}, 'Product variants incomplete')
generated = {}
for variant in product['variants']:
    name = variant['name']
    check(variant.get('passed') and variant.get('complete_assembly_rooted_for_aot') and
          variant.get('jit') == 'jit: boundary rejection passed' and
          variant.get('aot') == 'nativeaot: boundary rejection passed', 'Incomplete product consumer gate: ' + name)
    directory = ROOT / 'generated' / ('raw/TranslatedMsQuic' if name == 'raw' else 'TranslatedMsQuic')
    verified(variant['generated_sha256'], directory, 'Generated product')
    generated[name] = variant['generated_sha256']
# Record the SDK build revision from the generated attribute and require the
# same informational-version bytes in each already-hashed compiler assembly.
# Binary hashes, not this human-facing revision label, establish compiler identity.
versions = {}
for project in ['DotCC', 'DotCC.Lib']:
    metadata = REPO / project / 'obj/Release/net10.0' / (project + '.AssemblyInfo.cs')
    match = re.search(r'AssemblyInformationalVersionAttribute\("([^"\n]+)"\)', metadata.read_text())
    check(match is not None, 'Missing SDK compiler build metadata: ' + project)
    assembly = 'dotcc.dll' if project == 'DotCC' else 'DotCC.Lib.dll'
    check(match[1].encode() in (compiler / assembly).read_bytes(), 'SDK metadata differs from frozen compiler assembly')
    versions[assembly] = match[1]
check(len(set(versions.values())) == 1, 'CLI and compiler library build revisions differ')
revision = next(iter(versions.values())).split('+')[-1]
check(re.fullmatch(r'[0-9a-f]{40}', revision) is not None, 'Compiler build revision is not an exact commit')
operations = read(ROOT / 'config/managed-host/operations.json')
evidence = [ROOT / 'artifacts' / name / 'results.json' for name in ['host-contract', 'abi', 'product-build']]
if args.sqlite_receipt is not None:
    evidence += [sqlite_path, normal_path]
value = dict(schema_version=1, status='Nested MsQuic ABI/build closure passed; dependent runtime and shared regression qualification is separate',
    revision=pin['commit'], compiler_commit=revision, compiler_informational_versions=versions,
    stage_manifest_sha256=sha(stage_path), units=stage['units'],
    host_table_version=operations['version'], host_callback_slots=len(operations['operations']),
    data_model=stage['data_model'], source_files=stage['files'], api_profile_sha256=sha(ROOT / 'config/api-profile.json'),
    evidence_sha256={os.path.relpath(p, ROOT): sha(p) for p in evidence}, compiler_hashes=host['compiler_hashes'], generated=generated,
    generated_directories=product['generated_directories'], output_options=product['output_options'],
    macro_exports=product['macro_exports'],
    gates=dict(host_abi_native_records=sum(row['native_records'] for row in host['cases']),
        public_abi_native_records=public['native_records'], raw_optimized_jit_nativeaot=True,
        entire_generated_assembly_rooted_for_aot=True, shared_compiler_sqlite_campaign_passed=args.sqlite_receipt is not None,
        normal_optimized_sqlite_product_passed=args.sqlite_receipt is not None))
# Validate all current evidence above before considering a no-op. Reusing an
# unchanged valid closure keeps its original checkpoint and byte identity, so
# dependent receipts are not invalidated by running this command twice.
comparable_previous = {key: item for key, item in previous.items() if key != 'previous_checkpoint'}
if value == comparable_previous:
    checkpoint = previous.get('previous_checkpoint')
    if checkpoint:
        verified_archive((ROOT / checkpoint['closure']).parent.parent.parent, checkpoint['sha256'])
    print(json.dumps(dict(closure_sha256=old_hash, units=len(value['units']), gates=value['gates'], unchanged=True)))
    raise SystemExit(0)
verified_archive(archive, old_hash)
value['previous_checkpoint'] = dict(closure=str((archive / 'msquic/config/product-closure.json').relative_to(ROOT)), sha256=old_hash)
check(sha(CLOSURE) == old_hash, 'Current closure changed during validation')
CLOSURE.write_text(json.dumps(value, indent=2) + '\n')
print(json.dumps(dict(closure_sha256=sha(CLOSURE), units=len(value['units']), gates=value['gates'])))
