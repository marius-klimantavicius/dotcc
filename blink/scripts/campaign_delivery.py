"""Read actual shared-framework delivery evidence for specialist consumers.

This maps receipt fields in memory; it never manufactures a historical receipt
or replaces producer validation with a prior successful result.
"""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


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
    current_tools = {name: {p.name: sha(p) for p in (tools / name).iterdir()
                           if p.is_file() and p.suffix in ('.dll', '.json')}
                     for name in ('DotCC', 'DotCC.PostProcess')}
    if current_tools['DotCC'] != inputs['compiler']:
        raise RuntimeError('Shared and object producer compiler identities differ')
    if receipt.get('hash_policy') != 'off' and current_tools != receipt['tools']:
        raise RuntimeError('Current tools differ from producer receipt')
    return dict(passed=True, authored_sources_unchanged=True, selected_profile='threaded',
                product_surface={'instance_abi': 'instance-v1'}, profile=str(profile),
                profile_inputs_sha256=sha(profile / 'inputs.json'),
                stable_output=str(Path(outputs['processed']['project']).parent),
                raw_snapshot=str(Path(outputs['raw']['project']).parent),
                final_files=output_manifests['processed'], raw_files=output_manifests['raw'],
                assembly={'path': receipt['assembly'], 'sha256': sha(receipt['assembly'])},
                compiler=inputs['compiler'], postprocessor=current_tools['DotCC.PostProcess'],
                tool_directories={'compiler': str(tools / 'DotCC'), 'postprocessor': str(tools / 'DotCC.PostProcess')},
                authored_sources=authored, results=results,
                semantic_intrinsics=receipt['semantic_intrinsics'], managed_boundaries=receipt['managed_boundaries'],
                framework_receipt={'path': str(Path(path).resolve()), 'sha256': sha(path)})


def tool_directory(delivery, category, fallback):
    return Path(delivery.get('tool_directories', {}).get(category, fallback))
