#!/usr/bin/env python3
"""Link diagnostic objects and try Roslyn; this does not create a usable PAL."""
import argparse
import collections
import json
from pathlib import Path
import re
import subprocess

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--label', default='syntax', help='Input probe result directory')
args = parser.parse_args()
if Path(args.label).name != args.label or args.label in ('.', '..'):
    parser.error('--label must be a single directory name')
scoreboard = json.loads((root / 'artifacts' / args.label / 'results.json').read_text())
if len(scoreboard['results']) != 44 or any(r['exit_code'] for r in scoreboard['results']):
    raise SystemExit('Run the complete selected probe first; all 44 object emissions must pass')
objects = [root / 'generated' / args.label / (Path(r['unit']).parent.name + '-' + Path(r['unit']).stem + '.cs')
           for r in scoreboard['results']]
prefix = 'scope-library' if args.label == 'syntax' else args.label + '-library'
destination = root / 'generated' / prefix
command = ['dotnet', scoreboard.get('compiler', str(root.parent / 'DotCC/bin/Release/net10.0/dotcc.dll'))]
command += [str(obj) for obj in objects]
command += ['--emit=managedlib', '--class-name', 'MsQuicScopeProbe', '--split=size', '-o', str(destination)]
records = []


def run(name, command):
    (root / ('artifacts/' + name + '.command.json')).write_text(json.dumps(command, indent=2) + '\n')
    with (root / ('artifacts/' + name + '.log')).open('w') as log:
        result = subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, timeout=240)
    records.append({'stage': name, 'exit_code': result.returncode})
    print(name, result.returncode, flush=True)
    return result.returncode


if run(prefix + '-link', command) == 0:
    project = next(destination.glob('*.csproj'))
    run(prefix + '-build', ['dotnet', 'build', str(project), '-c', 'Release', '--nologo'])
    lines = set(line.strip() for line in (root / 'artifacts' / (prefix + '-build.log')).read_text().splitlines()
                if re.search(r': error [A-Z]+[0-9]+:', line))
    counts = collections.Counter(re.search(r': error ([A-Z]+[0-9]+):', line)[1] for line in lines)
    names = collections.Counter(match[1] for line in lines
        if (match := re.search(r"The name '([^']+)' does not exist", line)))
    errors = {'unique_diagnostics': len(lines), 'by_code': dict(counts),
              'undefined_names': dict(sorted(names.items())),
              'examples': sorted(lines)[:15]}
    (root / 'artifacts' / (prefix + '-errors.json')).write_text(json.dumps(errors, indent=2) + '\n')
(root / 'artifacts' / (prefix + '-results.json')).write_text(json.dumps(records, indent=2) + '\n')
