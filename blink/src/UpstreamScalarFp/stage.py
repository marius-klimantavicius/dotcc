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
           'scope': 'scalar COMIS/UCOMIS, SS/SD-to-GPR, and SIMD Linux signal code; DISABLE_JIT required',
           'sources': {}}
patch = ''
for name, row in json.loads((HERE / 'patch-inputs.json').read_text()).items():
    original = (UPSTREAM / name).read_bytes()
    if sha(original) != row['source_sha256']:
        raise SystemExit('immutable upstream source mismatch: ' + name)
    source = original.decode()
    if source.count(row['start']) != 1 or source.count(row['end']) != 1:
        raise SystemExit('upstream block boundary mismatch: ' + name)
    start, end = source.index(row['start']), source.index(row['end'])
    before = source[start:end]
    if sha(before.encode()) != row['block_sha256']:
        raise SystemExit('upstream block mismatch: ' + name)
    replacement = (HERE / row['replacement']).read_bytes()
    after = ('#ifndef DISABLE_JIT\n#error Reviewed scalar FP staging requires DISABLE_JIT\n#endif\n' +
             replacement.decode() + '\n')
    staged = source[:start] + after + source[end:]
    (a.output / name).write_text(staged)
    patch += ''.join(difflib.unified_diff(source.splitlines(True), staged.splitlines(True),
                                        fromfile='a/blink/' + name, tofile='b/blink/' + name))
    receipt['sources'][name] = dict(row, replacement_sha256=sha(replacement),
                                    staged_sha256=sha(staged.encode()))
(a.output / 'scalar-fp.patch').write_text(patch)
receipt['patch_sha256'] = sha(patch.encode())
a.receipt.parent.mkdir(parents=True, exist_ok=True)
a.receipt.write_text(json.dumps(receipt, indent=2) + '\n')
