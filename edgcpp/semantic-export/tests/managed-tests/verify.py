#!/usr/bin/env python3
"""Exercise semantic consumer proofs and rejection paths without changing source packages."""
import argparse
import copy
import json
from pathlib import Path
import shlex
import shutil
import subprocess

parser = argparse.ArgumentParser()
parser.add_argument('--reader', required=True, help='Command prefix, e.g. "dotnet /path/Edg.Semantic.dll"')
parser.add_argument('--probe', required=True, type=Path)
parser.add_argument('--plain', required=True, type=Path)
parser.add_argument('--output', required=True, type=Path)
args = parser.parse_args()
args.output.mkdir(parents=True, exist_ok=False)
command = shlex.split(args.reader)
checks = []


def run(name, path, *, mode='inspect', extra=(), expected=0, contains=None):
    result = subprocess.run([*command, mode, str(path), *extra], text=True, capture_output=True)
    (args.output / (name + '.stdout')).write_text(result.stdout)
    (args.output / (name + '.stderr')).write_text(result.stderr)
    if result.returncode != expected:
        raise AssertionError(f'{name}: exit {result.returncode}, expected {expected}: {result.stderr}')
    if contains and contains not in result.stderr:
        raise AssertionError(f'{name}: expected diagnostic {contains!r}: {result.stderr}')
    summary = json.loads(result.stdout) if expected == 0 else None
    checks.append({'name': name, 'exit': result.returncode, 'assertions': summary.get('assertions', []) if summary else []})


for name, path in [('cpp', args.probe), ('c', args.plain)]:
    run(name + '-probe', path, mode='assert-probe')
    run(name + '-strict', path, extra=['--strict-emission'], expected=1, contains='not emission-ready')

original = json.loads((args.probe / 'graph.json').read_text())


def mutate(name, action, diagnostic):
    graph = copy.deepcopy(original)
    action(graph)
    path = args.output / (name + '.json')
    path.write_text(json.dumps(graph))
    run(name, path, expected=1, contains=diagnostic)


mutate('unknown-schema', lambda g: g.update(schema='edg-semantic/999'), 'Unsupported semantic graph schema')
mutate('zero-root', lambda g: g.update(rootScope=0), 'Missing root scope')
mutate('lowered', lambda g: g.update(lowered=True), 'Expected unlowered')
mutate('duplicate-id', lambda g: g['nodes'].append(copy.deepcopy(g['nodes'][0])), 'duplicate node identity')
mutate('dangling-root', lambda g: g.update(rootScope=99999999), 'Unknown node ID')
mutate('wrong-root-kind', lambda g: g.update(rootScope=g['primarySource']), 'expects scope')
mutate('dangling-field', lambda g: next(n for n in g['nodes'] if 'type' in n).update(type=99999999), 'Unknown node ID')
mutate('nonnumeric-reference', lambda g: next(n for n in g['nodes'] if 'type' in n).update(type='bad'), 'Invalid node reference')
mutate('unknown-macro-parent', lambda g: g['macroInvocations'][0].update(parentIndex=99999999), 'Unknown macro parent')
mutate('unknown-macro-position', lambda g: next(n for n in g['nodes'] if 'location' in n)['location'].update(macroContext=99999999), 'Unknown position macro context')
mutate('wrong-native-edge-kind', lambda g: next(n for n in g['nodes'] if n.get('nativeEdges'))['nativeEdges'][0].update(kind='not-a-kind'), 'expects not-a-kind')

mutate('invalid-macro-placeholder', lambda g: g['macroInvocations'][0].update(parentIndex=-1), 'Invalid macro placeholder')
mutate('self-parent-macro', lambda g: g['macroInvocations'][0].update(parentIndex=g['macroInvocations'][0]['index']), 'invalid parent order')
placeholder_graph = copy.deepcopy(original)
placeholder = copy.deepcopy(placeholder_graph['macroInvocations'][0])
placeholder.update(index=max(m['index'] for m in placeholder_graph['macroInvocations']) + 1, parentIndex=-1, definition=0)
placeholder_graph['macroInvocations'].append(placeholder)
placeholder_path = args.output / 'macro-placeholder.json'
placeholder_path.write_text(json.dumps(placeholder_graph))
run('accept-macro-placeholder', placeholder_path)

copy_path = args.output / 'modified-package'
shutil.copytree(args.probe, copy_path)
manifest_path = copy_path / 'package.json'
manifest = json.loads(manifest_path.read_text())
source = next(s for s in manifest['sources'] if s.get('available', True))
source_path = copy_path / source['path']
source_bytes = source_path.read_bytes()
source_path.write_bytes(source_bytes + b'\n// edited after export\n')
run('changed-source-hash', copy_path, expected=1, contains='SHA256 mismatch')
source_path.write_bytes(source_bytes)
manifest['graph']['path'] = '../escape.json'
manifest_path.write_text(json.dumps(manifest))
run('escaping-package-path', copy_path, expected=1, contains='escapes package')

(args.output / 'results.json').write_text(json.dumps({'checks': checks, 'passed': len(checks)}, indent=2) + '\n')
print(f'PASS {len(checks)} managed semantic consumer checks')
