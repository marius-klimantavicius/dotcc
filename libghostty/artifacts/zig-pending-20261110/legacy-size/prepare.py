"""Prepare bounded legacy Patch16 inputs; never mutate either upstream checkout."""
import hashlib
import json
import subprocess
from pathlib import Path

R = Path(__file__).resolve().parents[4]
E = Path(__file__).resolve().parent
W = Path('/mnt/storage/dotcc-libghostty-disposable/zig-pending-20261110/legacy-size')
S = R / 'libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75/src'
Z = Path('/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/tools/zig-x86_64-linux-0.17.0')
(W / 'source/terminal').mkdir(parents=True, exist_ok=True)
(W / 'source/lib').mkdir(exist_ok=True)
(W / 'tmp').mkdir(exist_ok=True)
manifest = json.loads((R / 'libghostty/patches/direct/manifest.json').read_text())
entry = next(e for e in manifest['patches'] if e['patch'] == '0016-concrete-size-report.patch')
sha = lambda data: hashlib.sha256(data).hexdigest()
assert sha((R / 'libghostty/patches/direct' / entry['patch']).read_bytes()) == entry['patch_sha256']
original = (S / 'terminal/size_report.zig').read_bytes()
assert sha(original) == entry['original_sha256'], 'Preserving unknown legacy source state'
(W / 'source/terminal/original.zig').write_bytes(original)
(W / 'source/terminal/patched.zig').write_bytes(original)
subprocess.run(['patch', '--batch', str(W / 'source/terminal/patched.zig'), '-i',
                str(R / 'libghostty/patches/direct' / entry['patch'])], check=True, capture_output=True)
patched = (W / 'source/terminal/patched.zig').read_bytes()
assert sha(patched) == entry['patched_sha256']
a, b = original.decode(), patched.decode()
assert a[a.index('\nfn widthPixels'):] == b[b.index('\nfn widthPixels'):]
# Exact original factories and terminal lib aliases; the unreferenced lib/main
# exports are omitted so this focused graph needs no font/platform build graph.
for path in ['lib/enum.zig', 'lib/struct.zig', 'lib/target.zig', 'terminal/lib.zig']:
    (W / 'source' / path).write_bytes((S / path).read_bytes())
main_lines = (S / 'lib/main.zig').read_text().splitlines(keepends=True)
retained = ('const enumpkg =', 'const structpkg =', 'pub const Enum =', 'pub const Struct =', 'pub const Target =')
(W / 'source/lib/main.zig').write_text(''.join(line for line in main_lines if line.startswith(retained)))
# Only CellCountInt is reached. Reroute the unused size helper's assert import
# to avoid importing the unrelated font graph. No reached declaration changes.
size = (S / 'terminal/size.zig').read_text()
assert 'const assert = @import("../quirks.zig").inlineAssert;' in size
(W / 'source/terminal/size.zig').write_text(size.replace(
    'const assert = @import("../quirks.zig").inlineAssert;',
    'const assert = @import("std").debug.assert;'))
proof = (E / 'proof.zig').read_text()
(W / 'source/proof.zig').write_text(proof)
managed_proof = proof.replace('pub fn main() u8 { return answer(); }\n', '')
# Runtime controls use direct field/offset/size checks; native additionally checks
# reflection records without materializing their comptime-only enum fields.
managed_proof = '\n'.join(line for line in managed_proof.split('\n') if '@typeInfo(' not in line)
(W / 'source/managed-proof.zig').write_text(managed_proof)
# Additional non-C behavior control omits only the independently failed native
# offset assertions; it retains original/patched layout parity and all reports.
zig_behavior = '\n'.join(line for line in managed_proof.split('\n') if '!=expected_offsets[' not in line)
(W / 'source/managed-zig-behavior.zig').write_text(zig_behavior)
for profile in ['c', 'zig']:
    (W / (profile + '-options.zig')).write_text('pub const c_abi: bool = ' + ('true' if profile == 'c' else 'false') + ';\n')
    graph = dict(schema_version=1, root_module='root', output_mode='Lib', managed_roots=['answer'], optimize_mode='fast',
                 stdlib_types='source', function_pointer_abi='managed', zig_lib_dir=str(Z / 'lib'), modules=[
        dict(name='root', root=str(W / 'source/managed-proof.zig'), dependencies=[
            dict(name='terminal_options', module='terminal_options'), dict(name='proof_options', module='proof_options')]),
        dict(name='terminal_options', root=str(W / (profile + '-options.zig')), dependencies=[]),
        dict(name='proof_options', root=str(W / 'managed-options.zig'), dependencies=[])])
    (W / (profile + '-graph.json')).write_text(json.dumps(graph, indent=2) + '\n')
    if profile == 'zig':
        graph['modules'][0]['root'] = str(W / 'source/managed-zig-behavior.zig')
        (W / 'zig-behavior-graph.json').write_text(json.dumps(graph, indent=2) + '\n')
(W / 'native-options.zig').write_text('pub const native_receipt: bool = true;\n')
(W / 'managed-options.zig').write_text('pub const native_receipt: bool = false;\n')
(E / 'inputs.json').write_text(json.dumps(dict(upstream_commit=manifest['upstream_commit'],
    entry=entry, actual_size_report_bodies_unchanged=True,
    files={str(path.relative_to(W / 'source')): sha(path.read_bytes())
           for path in sorted((W / 'source').rglob('*.zig'))}), indent=2) + '\n')
print('Prepared exact original/patched legacy source and focused actual-factory/Writer graph.')
