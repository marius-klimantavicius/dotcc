"""Read actual shared-framework delivery evidence for specialist consumers.

This maps receipt fields in memory; it never manufactures a historical receipt
or replaces producer validation with a prior successful result.
"""
import hashlib
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from production_support import validate_tool_closure

# Only a root-accepted, immutable A32 capture may populate this table. A caller
# cannot authorize a compiler change by setting a manifest flag or label.
APPROVED_RELINK_HANDOFFS = json.loads((ROOT / 'config/approved-relink-handoffs.json').read_text())


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def manifest_sha(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, separators=(',', ':')).encode()).hexdigest()


def pinned(reference):
    if set(reference) != {'path', 'sha256'}:
        raise RuntimeError('Relink reference must bind an exact path and SHA256')
    path = Path(reference['path']).resolve()
    if Path(reference['path']).is_symlink() or not path.is_file() or sha(path) != reference['sha256']:
        raise RuntimeError('Relink evidence changed or missing: ' + str(path))
    return path


def relink_object_producer(receipt_path, receipt, profile, inputs, current_tools, tools):
    reference = receipt['object_relink']
    manifest_path = pinned(reference)
    if manifest_path.parent != Path(receipt_path).resolve().parent or receipt.get('hash_policy') != 'strict':
        raise RuntimeError('Relink manifest requires its strict owning delivery receipt')
    manifest = json.loads(manifest_path.read_text())
    if set(manifest) != {'format', 'handoff', 'original_assembly', 'profile_inputs', 'object_tool_directory',
                         'assembly', 'link_outputs'} or manifest['format'] != 'blink-validation-only-relink-v1':
        raise RuntimeError('Unsupported bounded relink manifest')
    handoff = pinned(manifest['handoff'])
    approval = APPROVED_RELINK_HANDOFFS.get(manifest['handoff']['sha256'])
    if not approval or str(handoff) != approval['path']:
        raise RuntimeError('Relink lacks a root-accepted sealed A32 handoff')
    producer_tools = Path(manifest['object_tool_directory']).resolve()
    producer = validate_tool_closure(producer_tools / 'dotcc.dll')
    for name, observed in [('object_producer', producer), ('linker', current_tools['DotCC']),
                           ('postprocessor', current_tools['DotCC.PostProcess'])]:
        if manifest_sha(observed) != approval[name]:
            raise RuntimeError('Relink approved recursive tool identity differs: ' + name)
    if (manifest['original_assembly'] != approval['original_assembly'] or
            manifest['profile_inputs'] != approval['profile_inputs'] or
            set(approval['evidence']) != {'validation_only_source', 'object_format_identity', 'runtime_identity'}):
        raise RuntimeError('Relink original source/format/runtime approval differs')
    for evidence in approval['evidence'].values():
        pinned(evidence)
    original_path = pinned(manifest['original_assembly'])
    original = json.loads(original_path.read_text())
    assembly_path = pinned(manifest['assembly'])
    assembly = json.loads(assembly_path.read_text())
    if (assembly_path != Path(receipt['assembly']).resolve() or
            pinned(manifest['profile_inputs']) != (profile / 'inputs.json').resolve() or
            original['identity']['profile_inputs_sha256'] != sha(profile / 'inputs.json') or
            original['identity']['compiler_sha256'] != producer or inputs['compiler'] != producer or
            original['profile'] != str(profile) or original['failures'] or len(original['objects']) != 108 or
            len(original['selected']) != 108 or set(original['selected']) != set(original['objects']) or original['linked'] or
            original['link']['exit_code'] == 0):
        raise RuntimeError('Relink original object/profile producer differs')
    for name, expected in inputs['staged_headers'].items():
        path = profile / name
        if not path.resolve().is_relative_to(profile.resolve()) or not path.is_file() or sha(path) != expected:
            raise RuntimeError('Relink frozen profile source changed: ' + name)
    from campaigns.recipes import helper
    core = helper(ROOT / 'scripts/core_inputs.py')
    entries = core.profile_sources(profile, ROOT, inputs)
    if core.upstream_identity(ROOT) != inputs['upstream_inputs'] or [e['path'] for e in entries] != original['selected']:
        raise RuntimeError('Relink upstream/source selection differs')
    for entry in entries:
        row = original['objects'][entry['path']]
        source = ROOT / entry['staged_path']
        if (sha(source) != entry['sha256'] or row['source_sha256'] != entry['sha256'] or
                sha(row['object_path']) != row['object_sha256'] or
                row['emission_identity']['compiler_sha256'] != producer):
            raise RuntimeError('Relink source/object input changed: ' + entry['path'])
    expected = dict(original)
    expected.update(linked=True, link=assembly.get('link'), relink_handoff=manifest['handoff'])
    if assembly != expected:
        raise RuntimeError('Relink changed the original object receipt beyond its actual link')
    link = assembly['link']
    command = list(original['link']['command'])
    command[1] = str(tools / 'DotCC/dotcc.dll')
    command[-1] = link['output']
    output = Path(link['output']).resolve()
    if (not output.is_relative_to(manifest_path.parent) or link.get('command') != command or
            link.get('exit_code') != 0):
        raise RuntimeError('Relink command/output differs from its approved original input set')
    records = [r for r in receipt['commands'] if r.get('label') == 'assemble-relink']
    if (len(records) != 1 or records[0].get('status') != 'passed' or records[0].get('exit_code') != 0 or
            records[0].get('requested_command', records[0]['command']) != command):
        raise RuntimeError('Relink lacks its actual successful command')
    actual = {p.relative_to(output).as_posix(): sha(p) for p in output.rglob('*') if p.is_file()}
    if not actual or actual != manifest['link_outputs'] or any(p.is_symlink() for p in output.rglob('*')):
        raise RuntimeError('Relink actual command output changed')
    from campaigns.recipes import link_flags
    delivery_command = ['dotnet', str(tools / 'DotCC/dotcc.dll'),
        *[original['objects'][name]['object_path'] for name in original['selected']], '--emit=managedlib',
        *link_flags('BlinkCore', 'Managed.Emulation'), '--deduplicate-inline', '--instance-methods',
        '-o', str(ROOT / receipt['staging'] / 'linked')]
    delivery = next(r for r in receipt['commands'] if r['label'] == 'delivery-link')
    if delivery.get('requested_command', delivery['command']) != delivery_command:
        raise RuntimeError('Normal delivery did not link the same objects with the approved linker')
    return producer, producer_tools, dict(handoff=manifest['handoff'], manifest=reference,
        object_producer=dict(compiler=producer, directory=str(producer_tools)),
        linker=dict(compiler=current_tools['DotCC'], directory=str(tools / 'DotCC')))


def load_delivery(path):
    receipt = json.loads(Path(path).read_text())
    if 'schema_version' not in receipt:
        return receipt
    if receipt.get('project') != 'blink' or receipt.get('profile') != 'threaded':
        raise RuntimeError('Specialist consumers require the actual threaded Blink producer')
    required = ('delivery-link', 'raw-build', 'postprocess', 'processed-build',
                'final-raw-build', 'final-processed-build')
    results = {}
    for label in required:
        matches = [row for row in receipt['commands'] if row['label'] == label]
        if len(matches) != 1 or matches[0].get('status') != 'passed' or matches[0].get('exit_code') != 0:
            raise RuntimeError('Required current delivery command did not pass: ' + label)
        row = matches[0]
        results[label] = dict(exit_code=row['exit_code'], log=row['stdout'], log_sha256=sha(row['stdout']))
    derivation = json.loads(Path(receipt['threaded_derivation']).read_text())
    profile = Path(derivation['profile'])
    inputs = json.loads((profile / 'inputs.json').read_text())
    bindings = json.loads((profile / 'host-bindings.json').read_text())
    authored = {}
    # Compare original authored sources against the actual frozen producer copies,
    # whose hashes the assembler already validated before and after emission.
    for name in bindings['managedSources']:
        frozen = 'managed/' + Path(name).name
        expected = inputs['staged_headers'][frozen]
        if sha(profile / frozen) != expected or sha(ROOT / name) != expected:
            raise RuntimeError('Authored bridge differs from its producer copy: ' + name)
        authored[name] = expected
    for name, expected in inputs['staged_headers'].items():
        if name.startswith('host-project/'):
            original = 'src/Managed.Emulation.Host/' + name.removeprefix('host-project/')
            if sha(profile / name) != expected or sha(ROOT / original) != expected:
                raise RuntimeError('Authored host differs from its producer copy: ' + original)
            authored[original] = expected
    outputs = receipt['outputs']
    output_manifests = {}
    for form in ('raw', 'processed'):
        directory = Path(outputs[form]['project']).parent
        actual = {str(p.relative_to(directory)): sha(p) for p in directory.rglob('*')
                  if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
        owned = json.loads((directory / '.campaign-files.json').read_text())
        if set(actual) != set(owned) | {'.campaign-files.json'}:
            raise RuntimeError('Current delivered ownership differs from producer: ' + form)
        if receipt.get('hash_policy') != 'off' and actual != outputs[form]['hashes']:
            raise RuntimeError('Current delivered files differ from producer receipt: ' + form)
        output_manifests[form] = actual
    tools = ROOT / receipt['staging'] / 'tools'
    current_tools = {name: validate_tool_closure(tools / name / executable)
                     for name, executable in [('DotCC', 'dotcc.dll'), ('DotCC.PostProcess', 'dotcc-postprocess.dll')]}
    if receipt.get('hash_policy') != 'off' and current_tools != receipt['tools']:
        raise RuntimeError('Current tools differ from producer receipt')
    object_compiler, object_tools, roles = current_tools['DotCC'], tools / 'DotCC', None
    if 'object_relink' in receipt:
        object_compiler, object_tools, roles = relink_object_producer(path, receipt, profile, inputs, current_tools, tools)
    elif current_tools['DotCC'] != inputs['compiler']:
        raise RuntimeError('Shared and object producer compiler identities differ')
    return dict(passed=True, authored_sources_unchanged=True, selected_profile='threaded',
                product_surface={'instance_abi': 'instance-v1'}, profile=str(profile),
                profile_inputs_sha256=sha(profile / 'inputs.json'),
                stable_output=str(Path(outputs['processed']['project']).parent),
                raw_snapshot=str(Path(outputs['raw']['project']).parent),
                final_files=output_manifests['processed'], raw_files=output_manifests['raw'],
                assembly={'path': receipt['assembly'], 'sha256': sha(receipt['assembly'])},
                # Compatibility alias used by consumers to check object inputs.
                # The actual new delivery compiler remains receipt.tools/V23.
                compiler=object_compiler, postprocessor=current_tools['DotCC.PostProcess'],
                object_producer=dict(compiler=object_compiler, directory=str(object_tools)),
                linker=dict(compiler=current_tools['DotCC'], directory=str(tools / 'DotCC')),
                object_relink=roles,
                tool_directories={'compiler': str(object_tools), 'linker': str(tools / 'DotCC'),
                                  'postprocessor': str(tools / 'DotCC.PostProcess')},
                authored_sources=authored, results=results,
                semantic_intrinsics=receipt['semantic_intrinsics'], managed_boundaries=receipt['managed_boundaries'],
                framework_receipt={'path': str(Path(path).resolve()), 'sha256': sha(path)})


def tool_directory(delivery, category, fallback):
    return Path(delivery.get('tool_directories', {}).get(category, fallback))
