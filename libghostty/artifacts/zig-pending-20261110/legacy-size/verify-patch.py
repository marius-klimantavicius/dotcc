"""Verify declaration-only change and bounded legacy patch application behavior."""
import hashlib
import json
import os
import shutil
import subprocess
from pathlib import Path

E = Path(__file__).resolve().parent
R = E.parents[3]
W = Path('/mnt/storage/dotcc-libghostty-disposable/zig-pending-20261110/legacy-size')
manifest = json.loads((R / 'libghostty/patches/direct/manifest.json').read_text())
entry = next(e for e in manifest['patches'] if e['patch'] == '0016-concrete-size-report.patch')
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
assert sha(R / 'libghostty/patches/direct' / entry['patch']) == entry['patch_sha256']
a = (W / 'source/terminal/original.zig').read_text()
b = (W / 'source/terminal/patched.zig').read_text()
assert sha(W / 'source/terminal/original.zig') == entry['original_sha256']
assert sha(W / 'source/terminal/patched.zig') == entry['patched_sha256']
assert a[a.index('\nfn widthPixels'):] == b[b.index('\nfn widthPixels'):]
assert a[:a.index('pub const Size')].replace('const lib = @import("lib.zig");',
       'const lib = @import("lib.zig");\nconst terminal_options = @import("terminal_options");') == b[:b.index('pub const Size')]
original_fields = a[a.index('pub const Size'):a.index('\nfn widthPixels')].split('struct {', 1)[1].removesuffix('\n});\n')
patched_fields = b[b.index('pub const Size'):b.index('\nfn widthPixels')].split('extern struct {', 1)[1].removesuffix('\n};\n').split('\n} else struct {')
assert patched_fields == [original_fields, original_fields]
tool, target = W / 'application-tool', W / 'application'
tool.mkdir(exist_ok=True)
(target / 'src/terminal').mkdir(parents=True, exist_ok=True)
for name in ['apply.py', entry['patch']]:
    shutil.copyfile(R / 'libghostty/patches/direct' / name, tool / name)
(tool / 'manifest.json').write_text(json.dumps(dict(upstream_commit=manifest['upstream_commit'], patches=[entry]), indent=2) + '\n')
source = target / entry['source']
source.write_text(a)
records = []
for stage in ['apply', 'idempotent', 'unknown-preserved']:
    if stage == 'unknown-preserved':
        source.write_bytes(b'unknown source\n')
    before = sha(source)
    command = ['python3', str(tool / 'apply.py'), str(target)]
    result = subprocess.run(command, env=dict(os.environ, TMPDIR=str(W / 'tmp')), text=True, capture_output=True)
    records.append(dict(stage=stage, command=command, exit=result.returncode, stdout=result.stdout,
                        stderr=result.stderr, source_sha256=sha(source)))
    if stage == 'unknown-preserved':
        assert result.returncode != 0 and sha(source) == before and 'preserving it' in result.stderr
    else:
        assert result.returncode == 0 and sha(source) == entry['patched_sha256']
        if stage == 'idempotent':
            assert 'already applied:' in result.stdout
(E / 'patch-checks.json').write_text(json.dumps(records, indent=2) + '\n')
print('Declaration-only change, patch hashes, apply/idempotence/unknown-source preservation PASS.')
