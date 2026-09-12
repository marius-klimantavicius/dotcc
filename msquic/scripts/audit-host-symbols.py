#!/usr/bin/env python3
"""Compile the selected native C closure and audit unresolved host/libc symbols."""
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
stage = ROOT / 'build/product-source'
manifest = json.loads((stage / 'manifest.json').read_text())
build = ROOT / 'build/host-symbol-audit'
logs = ROOT / 'artifacts/host-symbol-audit'
build.mkdir(parents=True, exist_ok=True)
logs.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, runtime_validation=False, commands=[], units=[], unresolved=[])
(logs / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
flags = ['gcc', '-std=gnu17', '-fms-extensions', '-O1', '-fno-inline', '-Wno-multichar']
flags.extend('-D' + define for define in manifest['defines'])
flags.extend('-I' + str(stage / path) for path in ['system', 'src/inc', 'src/core', 'src/platform', 'host'])
definitions, uses = set(), set()
# These are services already supplied by dotcc's managed libc, or native compiler
# helpers introduced by this GCC control build. This is not a native import list
# for the generated product, whose dependency audit is a separate gate.
allowed = {'abort', 'inet_ntop', 'memcmp', 'memcpy', 'memmove', 'memset', 'strlen', 'strnlen',
           'snprintf', 'in6addr_any', 'in6addr_loopback', '_GLOBAL_OFFSET_TABLE_',
           '__memcpy_chk', '__memmove_chk', '__memset_chk', '__stack_chk_fail'}
try:
    for index, unit in enumerate(manifest['units']):
        source = stage / unit
        output = build / f'{index:02d}-{source.stem}.o'
        command = flags + ['-c', str(source), '-o', str(output)]
        receipt['commands'].append(command)
        result = subprocess.run(command, capture_output=True, text=True, timeout=60)
        (logs / (source.stem + '.log')).write_text(result.stdout + result.stderr)
        if result.returncode:
            raise RuntimeError('Native C compilation failed: ' + unit)
        receipt['units'].append(dict(path=unit, source_sha256=hashlib.sha256(source.read_bytes()).hexdigest()))
        command = ['nm', '-g', str(output)]
        receipt['commands'].append(command)
        symbols = subprocess.check_output(command, text=True, timeout=30)
        for line in symbols.splitlines():
            words = line.split()
            if len(words) == 2 and words[0] == 'U':
                uses.add(words[1])
            elif len(words) == 3:
                definitions.add(words[2])
    receipt['unresolved'] = sorted(uses - definitions)
    receipt['unexpected'] = sorted(uses - definitions - allowed)
    if receipt['unexpected']:
        raise RuntimeError('Unbound symbols: ' + ', '.join(receipt['unexpected']))
    receipt['passed'] = True
finally:
    (logs / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({'passed': receipt['passed'], 'units': len(receipt['units']), 'unresolved': receipt['unresolved']}))
