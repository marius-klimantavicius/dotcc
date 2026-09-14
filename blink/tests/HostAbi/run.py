#!/usr/bin/env python3
"""Native LP64 ABI and unresolved-host declaration checks; no managed runtime."""
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'artifacts/host-abi'
BUILD = ROOT / 'build/host-abi'
PROFILE = ROOT / 'config/managed-host'
OUT.mkdir(parents=True, exist_ok=True)
BUILD.mkdir(parents=True, exist_ok=True)
(OUT / 'receipt.json').unlink(missing_ok=True)
ENV = dict(os.environ, LC_ALL='C')


def run(command):
    return subprocess.check_output(command, text=True, env=ENV, stderr=subprocess.STDOUT)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


compiler = ['cc', '-std=c11', '-Wall', '-Wextra', '-Werror']
run(compiler + ['-iquote', str(PROFILE), str(ROOT / 'tests/HostAbi/probe.c'), '-o', str(BUILD / 'probe')])
observed = run([str(BUILD / 'probe')])
(OUT / 'layout.txt').write_text(observed)
layouts = []
for line in observed.splitlines():
    name, native, profile = line.rsplit(' ', 2)
    layouts.append(dict(name=name, native=int(native), profile=int(profile)))

constants = json.loads((PROFILE / 'constants.json').read_text())
constant_results = []
for family, values in constants.items():
    header_values = {name:int(value) % (1 << 64) for name, value in
                     re.findall(r'^#define (\w+) \((-?\d+)\)$',
                                (PROFILE / (family + '-constants.h')).read_text(), re.M)}
    if header_values != values:
        raise SystemExit(f'{family} constant header differs from its pinned manifest')
    header = 'sys/socket.h' if family == 'socket' else family + '.h'
    source = '#define _GNU_SOURCE 1\n#include <stdio.h>\n#include <' + header + '>\nint main(void){\n'
    for name in values:
        source += 'printf("' + name + ' %llu\\n", (unsigned long long)(' + name + '));\n'
    path = OUT / (family + '-verify.c')
    path.write_text(source + '}\n')
    binary = BUILD / (family + '-verify')
    run(compiler + [str(path), '-o', str(binary)])
    output = run([str(binary)])
    for line in output.splitlines():
        name, value = line.split()
        constant_results.append(dict(name=name, native=int(value), profile=values[name]))

obj = BUILD / 'declarations.o'
run(compiler + ['-nostdinc', '-I', str(PROFILE), '-I', str(ROOT.parent / 'DotCC.Lib/include'),
                '-c', str(ROOT / 'tests/HostAbi/declarations.c'), '-o', str(obj)])
symbols = run(['nm', '-u', '-P', str(obj)])
(OUT / 'declaration-symbols.txt').write_text(symbols)
unresolved = sorted(line.split()[0] for line in symbols.splitlines())
expected = sorted(['blink_host_sigsetjmp', 'blink_host_siglongjmp', 'blink_host_sigprocmask',
                   'blink_host_poll', 'blink_host_readv', 'blink_host_tcgetattr', 'blink_host_socket'])
if unresolved != expected:
    raise SystemExit('host declarations did not stay isolated: ' + repr(unresolved))
passed = all(row['native'] == row['profile'] for row in layouts + constant_results)
receipt = dict(kind='native-host-abi-and-declaration-check-not-managed-execution',
               machine=platform.machine(), host=platform.platform(),
               compiler=run(['cc', '--version']).splitlines()[0],
               constantsSha256=sha(PROFILE / 'constants.json'),
               headers={str(p.relative_to(PROFILE)):sha(p) for p in sorted(PROFILE.rglob('*.h'))},
               layouts=layouts, constants=constant_results,
               unresolvedDeclarationSymbols=unresolved, passed=passed)
(OUT / 'receipt.json').write_text(json.dumps(receipt, indent=2)+'\n')
print(f'{len(layouts)} native layout checks, {len(constant_results)} constant checks, and {len(unresolved)} isolated unresolved host declarations: {"PASS" if passed else "FAIL"}')
if not passed:
    raise SystemExit(1)
