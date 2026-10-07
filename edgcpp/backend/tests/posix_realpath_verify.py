#!/usr/bin/env python3
"""Native/translated filesystem traversal and malloc-owned realpath results."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-dir', required=True, type=Path)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=False)
tree = out / 'tree'
(tree / 'child').mkdir(parents=True)
(tree / 'child/file').write_text('contents')
(tree / 'link').symlink_to('child', target_is_directory=True)
(tree / 'dangling').symlink_to('missing')
(tree / 'loop').symlink_to('loop')
fixture = Path(__file__).with_suffix('.c').with_name('posix_realpath.c')
report = dict(status='running', commands=[], checks=[])
def save():
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
def run(label, command):
    command = list(map(str, command))
    result = subprocess.run(command, cwd=out, capture_output=True, text=True)
    (out / (label + '.stdout')).write_text(result.stdout)
    (out / (label + '.stderr')).write_text(result.stderr)
    report['commands'].append(dict(label=label, command=command, exitCode=result.returncode))
    save()
    if result.returncode:
        raise RuntimeError(label + ' failed; see ' + str(out))
    return result.stdout
try:
    run('native-build', ['cc', '-std=c11', '-D_XOPEN_SOURCE=700', fixture, '-o', out / 'native'])
    expected = run('native-run', [out / 'native', tree])
    for instance in (False, True):
        label = 'instance' if instance else 'static'
        generated = out / label
        run(label + '-translate', [sys.executable, ROOT / 'dotnet.py', fixture, '--language', 'c',
            '--libc', 'musl', '--posix', '--output-dir', generated,
            *(['--instance-methods'] if instance else [])])
        run(label + '-build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release', '--nologo'])
        actual = run(label + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll', tree])
        if actual != expected:
            raise RuntimeError(label + ' JIT differs from native')
        report['checks'].append(label + ' JIT matches native (14 assertions)')
        if args.nativeaot:
            run(label + '-publish', ['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release',
                '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', generated / 'aot'])
            if run(label + '-aot', [generated / 'aot/Generated', tree]) != expected:
                raise RuntimeError(label + ' NativeAOT differs from native')
            report['checks'].append(label + ' NativeAOT matches native (14 assertions)')
    report['status'] = 'passed'
finally:
    if report['status'] == 'running':
        report['status'] = 'failed'
    save()
