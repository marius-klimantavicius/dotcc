#!/usr/bin/env python3
"""Measure the pinned service's actual native Blink loader seam and source closure."""
import hashlib
import json
from pathlib import Path
import re
import shutil
import struct
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[2]
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
base = ROOT / 'artifacts/loader-seam'
base.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
native = ROOT / 'build/native/source'
archive = native / 'o/blink/blink.a'
guest = ROOT / 'build/guest/service'
pins = json.loads((ROOT / 'tests/ServiceFixture/qualified-build.json').read_text())
core = json.loads((ROOT / 'artifacts/core/closure.json').read_text())
if sha(guest) != pins['executable_sha256']:
    raise SystemExit('service executable differs from pinned fixture')
if sha(archive) != core['native_archive_sha256'] or sha(native / 'config.h') != core['native_config_sha256']:
    raise SystemExit('native archive/config differ from interpreter oracle')
source = attempt / 'probe.c'
shutil.copyfile(ROOT / 'tests/LoaderSeam/probe.c', source)
compiler = Path(shutil.which('cc')).resolve()
receipt = dict(kind='native loader oracle only; no managed loader execution', passed=False,
               runner_sha256=sha(Path(__file__)), source_sha256=sha(source),
               executable_sha256=sha(guest), native_archive_sha256=sha(archive),
               native_config_sha256=sha(native / 'config.h'), compiler_sha256=sha(compiler),
               compiler_version=subprocess.check_output([str(compiler), '--version'], text=True).splitlines()[0],
               results={})

def save():
    (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

def run(command, label, timeout):
    with (attempt / (label + '.log')).open('wb') as log:
        try:
            code = subprocess.run(list(map(str, command)), stdout=log, stderr=subprocess.STDOUT,
                                  timeout=timeout).returncode
        except subprocess.TimeoutExpired:
            code = 124
    receipt['results'][label] = dict(command=list(map(str, command)), exit_code=code)
    save()
    if code:
        raise RuntimeError(label + ' failed; see ' + str(attempt))
    return (attempt / (label + '.log')).read_text()

run([compiler, '-D_GNU_SOURCE', '-D_DEFAULT_SOURCE', '-DNOLINEAR', '-Werror',
     '-I', native, source, archive, '-lz', '-lrt', '-lm', '-pthread',
     '-Wl,-Map=' + str(attempt / 'native-link.map'), '-o', attempt / 'probe'], 'build', 60)
output = run([attempt / 'probe', guest], 'native', 30).splitlines()
pattern = r'loader repeat=(\d) entry=([0-9a-f]+) phdr=([0-9a-f]+) phnum=(\d+) argc=3 envc=2 auxc=13 stack_aligned=1 random_matches=1 entry_bytes=([0-9a-f]{32})'
if len(output) != 2:
    raise RuntimeError('expected exactly two loader cases')
elf = guest.read_bytes()
if elf[:6] != b'\x7fELF\x02\x01' or struct.unpack_from('<HH', elf, 16) != (2, 62):
    raise RuntimeError('fixture must be little-endian ELF64 x86-64 ET_EXEC')
entry, phoff = struct.unpack_from('<QQ', elf, 24)
phentsize, phnum = struct.unpack_from('<HH', elf, 54)
segments = [struct.unpack_from('<IIQQQQQQ', elf, phoff + i * phentsize) for i in range(phnum)]
containing = [p for p in segments if p[0] == 1 and p[3] <= entry < p[3] + p[5]]
if len(containing) != 1 or any(p[0] == 3 for p in segments):
    raise RuntimeError('fixture entry segment/interpreter differs')
segment = containing[0]
entry_offset = segment[2] + entry - segment[3]
expected_bytes = elf[entry_offset:entry_offset + 16].hex()
header_segments = [p for p in segments if p[0] == 1 and p[2] <= phoff and phoff + phentsize * phnum <= p[2] + p[5]]
if len(header_segments) != 1:
    raise RuntimeError('fixture program headers must be in one load segment')
expected_phdr = header_segments[0][3] + phoff - header_segments[0][2]
for repeat, line in enumerate(output):
    match = re.fullmatch(pattern, line)
    if not match or int(match[1]) != repeat or int(match[2], 16) != entry or int(match[3], 16) != expected_phdr or int(match[4]) != phnum or match[5] != expected_bytes:
        raise RuntimeError('loaded entry differs from pinned ELF')
objects = sorted(set(re.findall(r'blink\.a\(([^()]+)\.o\)', (attempt / 'native-link.map').read_text())))
inventory = {row['path']: row['sha256'] for row in json.loads((ROOT / 'config/source-inventory.json').read_text())['files']}
sources = []
for member in objects:
    name = 'blink/' + Path(member).name + '.c'
    if name not in inventory:
        raise RuntimeError('unmapped native archive object: ' + member)
    sources.append(dict(path=name, sha256=inventory[name]))
previous = {row['path'] for row in core['sources']}
receipt.update(passed=True, native_binary_sha256=sha(attempt / 'probe'),
               source_closure=sources, additions_to_interpreter_closure=[row for row in sources if row['path'] not in previous],
               cases=output, fixture_entry=entry, fixture_program_headers=phnum,
               limitations=['valid pinned static ET_EXEC only', 'no malformed ELF qualification',
                            'no managed execution', 'no guest instructions executed'])
save()
print(json.dumps(dict(receipt=str(attempt / 'receipt.json'), native_sources=len(sources),
                      added_sources=[row['path'] for row in receipt['additions_to_interpreter_closure']])))
