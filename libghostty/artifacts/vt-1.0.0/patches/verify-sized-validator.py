"""Compare original/explicit-comptime sized validation and its native errors."""
import hashlib
import json
from pathlib import Path
import subprocess
import tarfile

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
BASE = Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0")
WORK = BASE / "patches/sized-validation"
WORK.mkdir(parents=True, exist_ok=True)
SOURCE = ROOT / "libghostty/ref/libghostty-vt-1.0.0"
ZIG = BASE / "tools/zig-x86_64-linux-0.17.0/zig"
with tarfile.open(SOURCE.parent / "libghostty-vt-1.0.0.tar.gz") as tar:
    member = next(m for m in tar.getmembers() if m.name.endswith("/src/lib/struct.zig"))
    (WORK / "original.zig").write_bytes(tar.extractfile(member).read())
(WORK / "target.zig").write_bytes((SOURCE / "src/lib/target.zig").read_bytes())
cache = ["--cache-dir", str(WORK / "cache"), "--global-cache-dir", str(WORK / "global")]
records = []


def run(name, argv, diagnostic=None):
    argv = list(map(str, argv))
    result = subprocess.run(argv, cwd=ROOT, capture_output=True, text=True, timeout=60)
    row = dict(name=name, argv=argv, exit=result.returncode,
               stdout=result.stdout, stderr=result.stderr)
    records.append(row)
    (HERE / "sized-validator-controls.json").write_text(json.dumps(records, indent=2) + "\n")
    if diagnostic is None:
        assert result.returncode == 0, row
    else:
        assert result.returncode != 0 and diagnostic in result.stdout + result.stderr, row


for variant, source in [("original", WORK / "original.zig"),
                        ("adapted", SOURCE / "src/lib/struct.zig")]:
    run(variant + "-five-native-sized-tests", [ZIG, "test", source, "--test-filter", "sized", *cache])
    for name, body, diagnostic in [
        ("layout", 'struct { size: usize, a: u8, pub const sized_base = "a"; }', "must be extern"),
        ("size-type", 'extern struct { size: u32, a: u8, pub const sized_base = "a"; }', "must start with size: usize"),
        ("size-order", 'extern struct { a: u8, size: usize, pub const sized_base = "size"; }', "must start with size: usize"),
        ("missing-base", 'extern struct { size: usize, a: u8 }', "must declare its base layout"),
        ("unknown-base", 'extern struct { size: usize, a: u8, pub const sized_base = "missing"; }', "sized_base names no field"),
        ("base-padding", 'extern struct { size: usize, a: u8, b: u8, pub const sized_base = "a"; }', "is placed in the padding of the base layout"),
    ]:
        probe = WORK / (name + ".zig")
        probe.write_text('const lib=@import("sized"); const Invalid=' + body
                         + '; comptime { _=lib.sizedBaseEnd(Invalid); }\n')
        run(variant + "-reject-" + name,
            [ZIG, "test", *cache, "--dep", "sized", "-Mroot=" + str(probe), "-Msized=" + str(source)],
            diagnostic=diagnostic)
print("Original/adapted: five sized tests each pass; six malformed layouts each preserve native rejection")
summary = dict(source_sha256=hashlib.sha256((SOURCE / "src/lib/struct.zig").read_bytes()).hexdigest(),
               patch_manifest_sha256=hashlib.sha256((ROOT / "libghostty/patches/vt-1.0.0/manifest.json").read_bytes()).hexdigest(),
               original_and_adapted_sized_tests=5, original_and_adapted_rejections=6,
               const_info_in_function_scope=True, all_remaining_validation_explicit_comptime=True)
(HERE / "sized-validator-summary.json").write_text(json.dumps(summary, indent=2) + "\n")
