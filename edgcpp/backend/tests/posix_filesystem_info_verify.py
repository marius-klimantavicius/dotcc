#!/usr/bin/env python3
"""Compare portable statvfs and terminal-window ioctl with a native PTY oracle."""
import argparse
import fcntl
import json
import os
from pathlib import Path
import struct
import subprocess
import sys
import termios

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-dir', type=Path, required=True)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=False)
fixture = Path(__file__).with_name('posix_filesystem_info.c')
master, slave = os.openpty()
fcntl.ioctl(slave, termios.TIOCSWINSZ, struct.pack('HHHH', 37, 101, 7, 11))
terminal = os.ttyname(slave)
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
        raise RuntimeError(label + ' failed: ' + str(out))
    return result.stdout
try:
    run('native-build', ['cc', '-std=c11', fixture, '-o', out / 'native'])
    expected = run('native', [out / 'native', terminal])
    for instance in (False, True):
        label = 'instance' if instance else 'static'
        generated = out / label
        run(label + '-translate', [sys.executable, ROOT / 'dotnet.py', fixture,
            '--language', 'c', '--libc', 'musl', '--posix', '--output-dir', generated,
            *(['--instance-methods'] if instance else [])])
        run(label + '-build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release', '--nologo'])
        if run(label + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll', terminal]) != expected:
            raise RuntimeError('JIT differs from native')
        report['checks'].append(label + ' JIT: 15 native filesystem/PTY assertions')
        if args.nativeaot:
            run(label + '-publish', ['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release',
                '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', generated / 'aot'])
            if run(label + '-aot', [generated / 'aot/Generated', terminal]) != expected:
                raise RuntimeError('NativeAOT differs from native')
            report['checks'].append(label + ' NativeAOT: same 15 assertions')
    report['status'] = 'passed'
finally:
    os.close(slave)
    os.close(master)
    if report['status'] == 'running':
        report['status'] = 'failed'
    save()
