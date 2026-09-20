#!/usr/bin/env python3
"""Stage only reviewed pinned scalar FP paths, requiring interpreter mode."""
import argparse, difflib, hashlib, json
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
UPSTREAM = ROOT / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink'
sha = lambda data: hashlib.sha256(data).hexdigest()
p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--output', type=Path, required=True)
p.add_argument('--receipt', type=Path, required=True)
a = p.parse_args()
a.output.mkdir(parents=True, exist_ok=True)
receipt = {'kind': 'reviewed-upstream-scalar-fp-correction',
           'upstream': 'f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580',
           'stage_sha256': sha(Path(__file__).read_bytes()),
           'manifest_sha256': sha((HERE / 'patch-inputs.json').read_bytes()),
           'scope': 'scalar COMIS/UCOMIS, SS/SD-to-GPR, SS/SD/PS/PD comparison masks, and SIMD Linux signal code; DISABLE_JIT required',
           'sources': {}}
patch = ''
for name, row in json.loads((HERE / 'patch-inputs.json').read_text()).items():
    original = (UPSTREAM / name).read_bytes()
    if sha(original) != row['source_sha256']:
        raise SystemExit('immutable upstream source mismatch: ' + name)
    source = original.decode()
    edits, blocks = [], []
    for selected in [row, *row.get('additional_blocks', [])]:
        if source.count(selected['start']) != 1 or source.count(selected['end']) != 1:
            raise SystemExit('upstream block boundary mismatch: ' + name)
        start, end = source.index(selected['start']), source.index(selected['end'])
        before = source[start:end]
        if sha(before.encode()) != selected['block_sha256']:
            raise SystemExit('upstream block mismatch: ' + name)
        replacement = (HERE / selected['replacement']).read_bytes()
        after = ('#ifndef DISABLE_JIT\n#error Reviewed scalar FP staging requires DISABLE_JIT\n#endif\n' +
                 replacement.decode() + '\n')
        edits.append((start, end, after))
        blocks.append(dict(selected, replacement_sha256=sha(replacement)))
    ordered = sorted(edits)
    if any(left[1] > right[0] for left, right in zip(ordered, ordered[1:])):
        raise SystemExit('overlapping reviewed blocks: ' + name)
    staged = source
    for start, end, after in reversed(ordered):
        staged = staged[:start] + after + staged[end:]
    (a.output / name).write_text(staged)
    patch += ''.join(difflib.unified_diff(source.splitlines(True), staged.splitlines(True),
                                        fromfile='a/blink/' + name, tofile='b/blink/' + name))
    receipt['sources'][name] = dict(row, replacement_sha256=blocks[0]['replacement_sha256'],
                                    staged_sha256=sha(staged.encode()))
    if len(blocks) > 1:
        receipt['sources'][name]['additional_blocks'] = blocks[1:]
(a.output / 'scalar-fp.patch').write_text(patch)
receipt['patch_sha256'] = sha(patch.encode())
a.receipt.parent.mkdir(parents=True, exist_ok=True)
a.receipt.write_text(json.dumps(receipt, indent=2) + '\n')
