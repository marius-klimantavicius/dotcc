#!/usr/bin/env python3
"""Stage reviewed INC auxiliary-carry and CMPXCHG8B register-width corrections."""
import argparse
import difflib
import hashlib
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REVISION = 'f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
UPSTREAM = ROOT / 'ref' / ('blink-' + REVISION) / 'blink'
sha = lambda value: hashlib.sha256(value).hexdigest()
SOURCE_PINS = {
    'alu.c': '673ddd60cb6fb1de6ea9f02a4e8e958f3c299a522dd754b4e82950e7a21fc4f4',
    'machine.c': '73a32c95fbc191394c116bd2f41ebba464c963718b3785adaca22c5a93e8dc89',
}
BLOCK_PINS = {
    'alu.c': {
        'i64 Inc32(': '4ef5bca0e414c90dbdd7d9950307d6be646f2e1222d644ba10e07761255c3615',
        'i64 Inc64(': '6fa095c3f530cb3d83f58d40497208a1e8dbfcfe27717d7b860b7e091e0fb31b',
        'i64 Inc8(': 'b02161171469198c16b474a358e15062f432341ef68dac007e092383f7e2485b',
        'i64 Inc16(': '545b606a9e35e75333458ad6c6bcea92a6f77376ce9ed4c5ba7cad157071e4ce',
    },
    'machine.c': {
        'static void OpCmpxchg8b(': 'd7ae10f0e906c9b3a6037c00c04b82384f850715541bab82c42e5293d928865c',
    },
}
GUARD = '#ifndef DISABLE_JIT\n#error Reviewed integer staging requires DISABLE_JIT\n#endif\n'

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', type=Path, required=True)
parser.add_argument('--receipt', type=Path, required=True)
args = parser.parse_args()
for target in (args.output.resolve(), args.receipt.resolve()):
    if target.is_relative_to((ROOT/'ref').resolve()) or target.is_relative_to(HERE):
        raise SystemExit('Staging output/receipt must not overwrite the immutable reference or authored sources')

script_hash = sha(Path(__file__).read_bytes())
reviewed_patch = (HERE/'integer.patch').read_bytes()
receipt = {'kind': 'reviewed-upstream-integer-correction', 'upstream': REVISION,
           'stage_sha256': script_hash,
           'scope': 'INC8/16/32/64 AF and CMPXCHG8B nonmatch zeroextension; DISABLE_JIT required',
           'sources': {}}
staged_files, patch = {}, ''
for name, expected in SOURCE_PINS.items():
    original = (UPSTREAM/name).read_bytes()
    if sha(original) != expected:
        raise SystemExit('Immutable upstream source mismatch: ' + name)
    source = original.decode()
    blocks = []
    for marker, block_hash in BLOCK_PINS[name].items():
        if source.count(marker) != 1:
            raise SystemExit('Upstream function boundary differs: ' + marker)
        start = source.index(marker)
        end = source.index('\n}', start) + 3
        before = source[start:end]
        if sha(before.encode()) != block_hash:
            raise SystemExit('Upstream function hash differs: ' + marker)
        if name == 'alu.c':
            old, new = 'af = (z & 15) < (y & 15);', 'af = (z & 15) < (x & 15);'
        else:
            old = '    Write32(m->ax, a);\n    Write32(m->dx, d);'
            new = '    Write64(m->ax, a);\n    Write64(m->dx, d);'
        if before.count(old) != 1:
            raise SystemExit('Reviewed expression differs: ' + marker)
        after = before.replace(old, new)
        source = source[:start] + after + source[end:]
        blocks.append({'function': marker, 'original_sha256': block_hash,
                       'replacement_sha256': sha(after.encode())})
    # Each marker lies after the ordinary includes that establish config.h.
    first = next(iter(BLOCK_PINS[name]))
    source = source.replace(first, GUARD + first, 1)
    staged_files[name] = source.encode()
    patch += ''.join(difflib.unified_diff(original.decode().splitlines(True), source.splitlines(True),
                                        fromfile='a/blink/'+name, tofile='b/blink/'+name))
    receipt['sources'][name] = {'source_sha256': expected, 'staged_sha256': sha(source.encode()),
                                'blocks': blocks}
if patch.encode() != reviewed_patch:
    raise SystemExit('Generated integer diff differs from the checked-in review patch')
if sha(Path(__file__).read_bytes()) != script_hash or (HERE/'integer.patch').read_bytes() != reviewed_patch:
    raise SystemExit('Staging implementation changed during derivation')
args.output.mkdir(parents=True, exist_ok=True)
for name, value in staged_files.items():
    (args.output/name).write_bytes(value)
(args.output/'integer.patch').write_bytes(reviewed_patch)
receipt['patch_sha256'] = sha(reviewed_patch)
args.receipt.parent.mkdir(parents=True, exist_ok=True)
pending = args.receipt.with_suffix(args.receipt.suffix + '.tmp')
pending.write_text(json.dumps(receipt, indent=2) + '\n')
pending.replace(args.receipt)
