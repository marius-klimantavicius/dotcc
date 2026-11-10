"""Reproduce bounded pending-Zig native controls without writing project inputs."""
from pathlib import Path
import json
import os
import subprocess

ROOT = Path(__file__).resolve().parents[3]
WORK = Path('/mnt/storage/dotcc-libghostty-disposable/zig-pending-20261110/native-final')
ZIG = Path('/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/tools/zig-x86_64-linux-0.17.0/zig')
WORK.mkdir(parents=True, exist_ok=True)
ENV = dict(os.environ, TMPDIR=str(WORK))
CACHE = ['--cache-dir', str(WORK/'cache'), '--global-cache-dir', str(WORK/'global')]
rows = []
def run(label, command, expected=0, diagnostic=None):
    command = list(map(str, command))
    result = subprocess.run(command, capture_output=True, text=True, env=ENV)
    rows.append(dict(label=label, command=command, exit=result.returncode, stdout=result.stdout, stderr=result.stderr))
    (Path(__file__).with_name('native-final.json')).write_text(json.dumps(rows, indent=2)+'\n')
    if diagnostic:
        assert result.returncode != 0 and diagnostic in result.stderr, rows[-1]
    else:
        assert result.returncode == expected, rows[-1]

for name in ['forward-u8.zig', 'hash-bytes.zig', 'cache.zig']:
    source = WORK/name
    source.write_text((ROOT/'DotCC.FunctionalTests/Fixtures/ZigGenericElementWidths'/name).read_text()
                      + '\npub fn main() u8 { return answer(); }\n')
    for mode in ['Debug', 'ReleaseFast']:
        run(name+'-'+mode, [ZIG, 'build-exe', source, '-O'+mode, *CACHE, '-femit-bin='+str(WORK/'control')])
        run(name+'-'+mode+'-execute', [WORK/'control'], expected=42)
fixture = ROOT/'DotCC.FunctionalTests/Fixtures/ZigWideVectors/optional-storage.zig'
source = WORK/'optional-vector.zig'
source.write_text(fixture.read_text()+'\ntest "answer" { try @import("std").testing.expectEqual(@as(u8,42),answer()); }\n')
for cpu in ['baseline', 'x86_64_v3', 'native']:
    run('vector-'+cpu, [ZIG, 'test', source, '-target', 'x86_64-linux-gnu', '-mcpu='+cpu, *CACHE])
source = WORK/'forward-null.zig'
source.write_text((ROOT/'DotCC.FunctionalTests/Fixtures/ZigRenderOptional/forward-null.zig').read_text()
                  +'\ntest "answer" { try @import("std").testing.expectEqual(@as(u8,42),answer()); }\n')
run('forward-null', [ZIG, 'test', source, *CACHE])
source = WORK/'zero-optionals.zig'
source.write_text('const std=@import("std"); test "layout" {\n'
                  +'try std.testing.expectEqual(@as(usize,1),@sizeOf(?struct{}));\n'
                  +'try std.testing.expectEqual(@as(usize,1),@alignOf(?struct{}));\n'
                  +'try std.testing.expectEqual(@as(usize,1),@sizeOf(?[0]u8));\n'
                  +'try std.testing.expectEqual(@as(usize,1),@alignOf(?[0]u8)); }\n')
run('zero-optionals', [ZIG, 'test', source, *CACHE])
(WORK/'host.c').write_text('extern unsigned char answer(void); int main(void) { return answer(); }\n')
for name in ['for-else-return.zig','comptime-slice-arguments.zig','runtime-comptime-slice.zig',
             'runtime-call-comptime-slice.zig','mutable-read.zig']:
    source=ROOT/'DotCC.FunctionalTests/Fixtures/ZigStaticPointers'/name
    command=[ZIG,'build-obj',source,'-OReleaseFast',*CACHE,'-femit-bin='+str(WORK/'control.o')]
    if name in ['for-else-return.zig','comptime-slice-arguments.zig']:
        run(name,command);run(name+'-link',['cc',WORK/'host.c',WORK/'control.o','-o',WORK/'control'])
        run(name+'-execute',[WORK/'control'],expected=42)
    else:
        run(name,command,diagnostic='comptime')
print('Native generic, loop/slice, null/zero payload and three CPU vector profiles PASS')
